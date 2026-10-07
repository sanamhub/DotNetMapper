# DotNetMapper

[![NuGet](https://img.shields.io/nuget/v/DotNetMapper.svg)](https://www.nuget.org/packages/DotNetMapper)
[![CI](https://github.com/sanamhub/DotNetMapper/actions/workflows/ci.yml/badge.svg)](https://github.com/sanamhub/DotNetMapper/actions/workflows/ci.yml)

A zero-config object mapper for .NET with one method. It copies public properties that have the
same name and the same type into a new object. A bundled source generator turns each call into
the code you would write by hand, so it runs at hand-written speed and is NativeAOT safe.

```bash
dotnet add package DotNetMapper
```

Requires .NET 10.

## Usage

```csharp
using DotNetMapper;

UserDto dto = Mapper.Map<UserEntity, UserDto>(entity);
```

No attributes, profiles or configuration. Properties without a match are ignored, values are
assigned rather than cloned, and types must be identical: `int` is not copied to `long`.

## Performance

Mapping a 10-property class on .NET 10. Lower is better.

| Library | Mean | vs hand-written | Allocated |
| --- | ---: | ---: | ---: |
| Hand-written | 13.5 ns | 1.00 | 136 B |
| **DotNetMapper 2.0** | **12.7 ns** | **0.94** | **136 B** |
| Mapperly 4.3 | 13.8 ns | 1.03 | 136 B |
| Mapster 10.0 | 33.9 ns | 2.52 | 208 B |
| AutoMapper 16.2 | 61.5 ns | 4.57 | 224 B |
| TinyMapper 3.0 | 62.0 ns | 4.61 | 272 B |
| AgileMapper 1.8 | 242.4 ns | 18.01 | 456 B |

DotNetMapper and Mapperly both emit the hand-written code, so they are equal within noise. The
[benchmarks](https://github.com/sanamhub/DotNetMapper/blob/main/docs/benchmarks/README.md) have
the full tables, setup and how to run them.

## Docs

The [wiki](https://github.com/sanamhub/DotNetMapper/wiki) has every rule with examples:

- [Mapping rules](https://github.com/sanamhub/DotNetMapper/wiki/Mapping-rules): which properties
  are copied, setters, records, inheritance, interfaces.
- [Generated code](https://github.com/sanamhub/DotNetMapper/wiki/Generated-code): how to see it,
  when a call falls back to the runtime path, `IQueryable`, NativeAOT and IL2091.
- [Migrating from 1.x](https://github.com/sanamhub/DotNetMapper/wiki/Migrating-from-1x).

## When to use something else

If you need renaming, type conversion, flattening or configuration, use
[Mapperly](https://github.com/riok/mapperly). DotNetMapper is deliberately the one-method case.

## License

[MIT](https://github.com/sanamhub/DotNetMapper/blob/main/LICENSE).
