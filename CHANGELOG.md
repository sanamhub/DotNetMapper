# Changelog

Format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
Versioning follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [2.0.0] - 2026-10-07

### Added

- A Roslyn incremental source generator, shipped inside the package, that replaces
  `Mapper.Map<A, B>(x)` calls with generated `new B { P = x.P, ... }` code at compile time when
  `A` and `B` are concrete and nameable. The call site pays no reflection, no delegate, and no
  boxing. Generated methods are marked for aggressive inlining, so the call site compiles to the
  same code as a hand-written initializer. Calls inside expression trees, including query clauses
  over `IQueryable`, are left alone so query providers still see `Mapper.Map`.
- NativeAOT and trim annotations on the public API, and `IsAotCompatible` on the package, so a
  NativeAOT consumer compiles with warnings-as-errors and no IL2xxx/IL3xxx diagnostics.
- Symbols package (`.snupkg`) and SourceLink/deterministic build settings.
- Tests, benchmarks, CI, release automation, ADRs, and a release runbook.

### Changed

- The runtime fallback now caches one typed `Func<TInput, TOutput>` per closed generic type pair
  in a generic static field. The CLR runs the static initializer once and thread-safely, so there
  is no dictionary lookup and no boxing.
- Targets to `net10.0` only.
- Targets with a private, internal or protected setter are no longer written. Source properties
  without a public getter are skipped. When a property is hidden or overridden, the most-derived
  declaration decides, including its accessors. Both paths agree on this.
- The repository moved to a `src/` layout with a `.slnx` solution, central package management,
  warnings-as-errors, and PublicAPI tracking.

### Fixed

- The mapping cache now actually caches. 1.0.2 used the `GetOrAdd(key, valueFactory)` value
  overload, which ran the reflection and expression compile on every call and threw the result
  away when the key already existed.
- Indexers are ignored instead of throwing.
- Write-only source properties and non-public getters are skipped instead of throwing or leaking
  non-public state.

### Removed

- The `net7.0` target. .NET 10 is the only supported target.

## [1.0.2]

Last release before the changelog was kept.
