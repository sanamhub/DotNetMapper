# Benchmarks

Run on 2026-10-06 with BenchmarkDotNet 0.15.8, Windows 11 25H2, 12th Gen Intel Core i5-12500H,
.NET SDK 10.0.401, runtime 10.0.12, x64 RyuJIT. Ratios are against the hand-written `Manual` row
of the same table. Absolute nanoseconds are not comparable across machines.

Library versions: DotNetMapper 2.0.0, Riok.Mapperly 4.3.1, Mapster 10.0.13, AutoMapper 16.2.0,
TinyMapper 3.0.3, AgileObjects.AgileMapper 1.8.1. Every library runs with its default settings.
The code is in [benchmarks/DotNetMapper.Benchmarks](https://github.com/sanamhub/DotNetMapper/tree/main/benchmarks/DotNetMapper.Benchmarks).

## Small model, like for like

`Small` to `SmallDto`: an `int` and a `string`. No collection, so every library does the same work.

| Method | Mean | Error | Ratio | Allocated |
| --- | ---: | ---: | ---: | ---: |
| Manual | 4.201 ns | 0.134 ns | 1.00 | 32 B |
| DotNetMapper | 4.552 ns | 0.120 ns | 1.08 | 32 B |
| Mapperly | 4.603 ns | 0.125 ns | 1.10 | 32 B |
| DotNetMapper, runtime path | 7.760 ns | 0.107 ns | 1.85 | 32 B |
| TinyMapper | 11.242 ns | 0.269 ns | 2.68 | 32 B |
| Mapster | 13.686 ns | 0.184 ns | 3.26 | 32 B |
| AutoMapper | 35.402 ns | 0.750 ns | 8.43 | 32 B |
| AgileMapper | 220.659 ns | 2.478 ns | 52.53 | 296 B |
| DotNetMapper 1.0.2 | 90,846.644 ns | 806.996 ns | 21,625 | 7,215 B |

DotNetMapper and Mapperly are within each other's error: both emit the same object initializer.

## Ten properties

`Source` to `Destination`: `int`, `long`, two `string`, `DateTime`, `decimal`, `Guid`, `bool`, an
enum and a `List<string>`. DotNetMapper and Mapperly copy the list reference, as a hand-written
mapping would. Mapster, AutoMapper, TinyMapper and AgileMapper copy the list by default, which is
extra work and shows in the Allocated column.

| Method | Mean | Error | Ratio | Allocated |
| --- | ---: | ---: | ---: | ---: |
| Manual | 13.459 ns | 0.305 ns | 1.00 | 136 B |
| DotNetMapper | 12.678 ns | 0.296 ns | 0.94 | 136 B |
| Mapperly | 13.821 ns | 0.306 ns | 1.03 | 136 B |
| DotNetMapper, runtime path | 16.365 ns | 0.363 ns | 1.22 | 136 B |
| Mapster | 33.850 ns | 0.663 ns | 2.52 | 208 B |
| AutoMapper | 61.459 ns | 1.279 ns | 4.57 | 224 B |
| TinyMapper | 62.007 ns | 0.949 ns | 4.61 | 272 B |
| AgileMapper | 242.362 ns | 4.728 ns | 18.01 | 456 B |
| DotNetMapper 1.0.2 | 180,053.064 ns | 3,475.611 ns | 13,378 | 11,771 B |

DotNetMapper below `Manual` is noise: the generated method is inlined into the call site and
compiles to the same code. 1.0.2 recompiled its expression tree on every call.

## Cold start

One call per process, ten processes. Includes whatever setup a library needs before its first
map: AutoMapper builds a `MapperConfiguration`, TinyMapper binds the pair, the others need nothing
explicit.

| Method | Mean | Allocated |
| --- | ---: | ---: |
| Mapperly | 0.61 ms | 136 B |
| DotNetMapper | 0.72 ms | 136 B |
| DotNetMapper, runtime path | 11.77 ms | 136 B |
| DotNetMapper 1.0.2 | 13.19 ms | 12,168 B |
| TinyMapper | 28.41 ms | 59,672 B |
| Mapster | 63.52 ms | 208 B |
| AutoMapper | 64.16 ms | 263,440 B |
| AgileMapper | 123.84 ms | 456 B |

The DotNetMapper and Mapperly rows are process startup: neither does work on the first call that
it does not do on every call. The 0.1 ms between them is inside the run-to-run spread of about
0.07 ms.

## Running them

```bash
dotnet run -c Release --project benchmarks/DotNetMapper.Benchmarks -- --filter "*" --memory
```

The `bench` workflow runs the same command on demand. It is not a gate: shared runners are too
noisy for nanosecond thresholds.
