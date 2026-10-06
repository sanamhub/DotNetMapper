# Benchmarks

Run on 2026-10-06 on a Windows 11 (10.0.26200.9550) machine, 12th Gen Intel Core i5-12500H
3.10 GHz, .NET SDK 10.0.401, runtime 10.0.12, x64.

Ratios are against the hand-written `Manual` baseline in the same table. Absolute nanoseconds
are not comparable across machines.

## Throughput

`Source` and `Destination` have 10 properties (int, long, two strings, DateTime, decimal, Guid,
bool, enum, `List<string>`). `Small` and `SmallDto` have 2.

| Model | Method | Mean | Ratio | Allocated |
| --- | --- | ---: | ---: | ---: |
| Source → Destination | Manual | 13.755 ns | 1.00 | 136 B |
| Source → Destination | DotNetMapper (intercepted) | 14.574 ns | 1.06 | 136 B |
| Source → Destination | DotNetMapper_Runtime | 16.235 ns | 1.18 | 136 B |
| Source → Destination | DotNetMapper 1.0.2 | 183,710.967 ns | 13,358 | 11,771 B |
| Source → Destination | Mapperly | 15.146 ns | 1.10 | 136 B |
| Source → Destination | Mapster | 35.596 ns | 2.59 | 208 B |
| Small → SmallDto | Manual | 4.540 ns | 1.00 | 32 B |
| Small → SmallDto | DotNetMapper (intercepted) | 4.475 ns | 0.99 | 32 B |
| Small → SmallDto | DotNetMapper_Runtime | 7.551 ns | 1.66 | 32 B |
| Small → SmallDto | DotNetMapper 1.0.2 | 91,724.911 ns | 20,204 | 7,295 B |
| Small → SmallDto | Mapperly | 7.574 ns | 1.67 | 32 B |
| Small → SmallDto | Mapster | 13.527 ns | 2.98 | 32 B |

The intercepted call is within noise of hand-written code and allocates exactly the result
object. The runtime fallback allocates the same. 1.0.2 is four orders of magnitude slower on
every call because it recompiled the expression tree on each `Map` call and boxed both sides.

## Cold start

One call per launch, ten launches. Measures the first-call cost.

| Method | Mean | Allocated |
| --- | ---: | ---: |
| DotNetMapper (intercepted) | 617.9 us | 136 B |
| DotNetMapper_Runtime | 11,561.7 us | 136 B |
| DotNetMapper 1.0.2 | 12,496.2 us | 12,168 B |
| Mapperly | 594.3 us | 136 B |
| Mapster | 61,979.0 us | 208 B |

The intercepted call pays no per-call compile; the figure is process startup. The runtime path
pays one expression compile on the first call for a type pair. Mapster pays its config
compilation on the first `Adapt`.
