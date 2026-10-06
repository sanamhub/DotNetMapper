# ADR-0001: Single target net10.0

- Status: accepted
- Date: 2026-10-06

## Context

The package previously targeted `net8.0` only. `net8.0` reaches end of support on 2026-11-10.
The repository owner's other projects standardise on the current .NET LTS, which is .NET 10.

## Decision

Target `net10.0` only. The SDK version in `global.json` pins the 10.0.400 feature band.

A `netstandard2.0` target was considered so older consumers could keep using the package. It was
rejected: the bundled source generator already builds against `netstandard2.0`, but the runtime
library itself has no reason to carry a second target. A single target also means no conditional
compilation, which CI enforces.

## Consequences

- Consumers on .NET 8 must stay on 1.0.2 or move to .NET 10.
- One target means one set of build warnings and one public API to track.
- `#if` in `src` or `tests` is rejected by CI, so target-specific code cannot creep back in.
