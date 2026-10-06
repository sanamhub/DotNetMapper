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
- expression trees: lambdas converted to `Expression<T>` and query clauses over `IQueryable`, so
  a provider such as EF Core still sees `Mapper.Map`
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
- When a name is hidden with `new` or overridden, the most-derived declaration wins, accessors
  included. A hiding property with a private setter is not written, and neither is an override
  that declares only `get`.
- Unmatched properties are ignored. Values are copied by assignment, so reference-type values are
  shared, not cloned.
- Interface types consider only properties declared on that interface itself, not base
  interfaces.

## NativeAOT and trimming

The package is annotated and `IsAotCompatible`, so a NativeAOT or trimmed consumer compiles with
warnings-as-errors and no IL2xxx/IL3xxx diagnostics. Intercepted calls are plain generated code.
The runtime fallback interprets its expression tree under NativeAOT, which is slower but correct.

`Map` declares which members it reads with `[DynamicallyAccessedMembers]`. If you call it from your
own generic method with trim analysis on, the analyzer reports IL2091 until you put the same
attributes on your type parameters.

## Requirements

A project targeting `net10.0` or later, built with the .NET 10 SDK. Every .NET 10 SDK supports
interceptors, so no setup is needed: the package adds the `InterceptorsNamespaces` entry itself.

## See the generated code

Set `<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>` and look under `obj/` for
`DotNetMapper.Interceptors.g.cs`.

## Turning the generator off

```xml
<PackageReference Include="DotNetMapper" ExcludeAssets="analyzers" />
```

Every call then uses the runtime path.

## Performance

Mapping a 10-property class, .NET 10 on a Windows 11 x64 laptop, each library with its defaults.
Lower is better.

| Library | Mean | vs hand-written | Allocated |
| --- | ---: | ---: | ---: |
| Hand-written | 13.5 ns | 1.00 | 136 B |
| **DotNetMapper 2.0** | **12.7 ns** | **0.94** | **136 B** |
| Mapperly 4.3 | 13.8 ns | 1.03 | 136 B |
| Mapster 10.0 | 33.9 ns | 2.52 | 208 B |
| AutoMapper 16.2 | 61.5 ns | 4.57 | 224 B |
| TinyMapper 3.0 | 62.0 ns | 4.61 | 272 B |
| AgileMapper 1.8 | 242.4 ns | 18.01 | 456 B |
| DotNetMapper 1.0.2 | 180,053 ns | 13,378 | 11,771 B |

DotNetMapper is as fast as writing the mapping by hand, because after compilation it is the
mapping written by hand. Mapperly emits the same code and is within noise of it. Mapster,
AutoMapper, TinyMapper and AgileMapper also copy the `List<string>` property by default, which
accounts for part of their gap; on a 2-property model with no collection they are still 2.7 to
53 times slower than hand-written code.

[docs/benchmarks](https://github.com/sanamhub/DotNetMapper/blob/main/docs/benchmarks/README.md)
has the full tables, the 2-property model, cold start, and how to run them.

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
