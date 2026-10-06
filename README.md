# DotNetMapper

[![NuGet](https://img.shields.io/nuget/v/DotNetMapper.svg)](https://www.nuget.org/packages/DotNetMapper)
[![CI](https://github.com/sanamhub/DotNetMapper/actions/workflows/ci.yml/badge.svg)](https://github.com/sanamhub/DotNetMapper/actions/workflows/ci.yml)

A tiny, zero-config object mapper for .NET. One method: `Mapper.Map<TInput, TOutput>(input)`.
It copies public properties that have the same name and the same type into a new `TOutput`.

No attributes, no profiles, no configuration, no fluent API. A bundled source generator turns
each call into plain generated code at compile time, so the steady-state cost is the same as
hand-written code.

```bash
dotnet add package DotNetMapper
```

## Usage

```csharp
using DotNetMapper;

var input = new InputClass { Id = 1, Name = "John" };
var output = Mapper.Map<InputClass, OutputClass>(input);
```

`output` is a new `OutputClass` with `Id` and `Name` copied from `input`.

## How it works

Calls with concrete types are replaced at compile time by a C# interceptor. The generator emits
`new B { P = x.P, ... }`, so there is no reflection, no delegate, and no boxing at runtime.

Calls the generator cannot see fall back to a runtime path: one `Func<A, B>` compiled once per
type pair and cached in a generic static field. That path covers:

- open generic call sites, where the types are not known at compile time
- private or protected nested types and `file`-local types
- anonymous types
- expression-tree lambdas (`IQueryable.Select(x => Mapper.Map<A, B>(x))`)
- method groups (`Func<A, B> f = Mapper.Map<A, B>;`)
- consumers on a compiler without interceptor support, or who excluded the analyzer

Both paths produce identical results.

## Mapping rules

- `null` input throws `ArgumentNullException` with parameter name `inputObject`.
- The result is `new TOutput()` followed by property assignments.
- Source properties: public, instance, not an indexer, with a public getter. Inherited included.
- Target properties: public, instance, not an indexer, with a public `set` or `init`. Inherited
  included.
- A pair matches when names are equal (ordinal, case-sensitive) and types are identical. Nullable
  reference annotations are ignored, so `string?` matches `string`.
- Unmatched properties are ignored. Values are copied by assignment, so reference-type values are
  shared, not cloned.
- Interface types consider only properties declared on that interface itself, not base
  interfaces.

## NativeAOT and trimming

The package is annotated and `IsAotCompatible`, so a NativeAOT or trimmed consumer compiles with
warnings-as-errors and no IL2xxx/IL3xxx diagnostics. Intercepted calls are plain generated code.
The runtime fallback interprets its expression tree under NativeAOT, which is slower but correct.

## Requirements

.NET 10 SDK. Interception needs a compiler with interceptor support, which every .NET 10 SDK has.
On an older compiler everything still works through the runtime path.

## See the generated code

Set `<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>` and look under `obj/` for
`DotNetMapper.Interceptors.g.cs`.

## Turning the generator off

```xml
<PackageReference Include="DotNetMapper" ExcludeAssets="analyzers" />
```

Every call then uses the runtime path.

## Performance

See [docs/benchmarks](https://github.com/sanamhub/DotNetMapper/blob/main/docs/benchmarks/README.md)
for the full run. On a Windows 11 x64 machine with .NET 10, mapping a 10-property class:

| Path | Mean | Allocated |
| --- | ---: | ---: |
| Hand-written | 13.8 ns | 136 B |
| DotNetMapper (intercepted) | 14.6 ns | 136 B |
| DotNetMapper runtime fallback | 16.2 ns | 136 B |
| DotNetMapper 1.0.2 | 183,711 ns | 11,771 B |

## Migrating from 1.x

2.0 drops `net8.0` and fixes mapping rules that the two paths now agree on:

- Targets with a private, internal or protected setter are no longer written.
- Source properties without a public getter are skipped.
- Indexers are ignored instead of throwing.

## When to use something else

If you need configuration, type conversion, flattening, or attribute-driven mapping, use
[Mapperly](https://github.com/riok/mapperly). DotNetMapper is deliberately the one-method case.

## License

[MIT](https://github.com/sanamhub/DotNetMapper/blob/main/LICENSE).
