# Contributing to DotNetMapper

Bug reports, fixes and documentation improvements are welcome. For a new feature, open an issue
first. DotNetMapper is deliberately one method with no configuration, so most features belong in
[Mapperly](https://github.com/riok/mapperly) instead.

This project follows the [Code of Conduct](CODE_OF_CONDUCT.md). Report security problems as
described in [SECURITY.md](SECURITY.md), not in a public issue.

## Setup

You need the .NET SDK version in [`global.json`](global.json). Nothing else.

```bash
dotnet build -c Release
dotnet test -c Release
```

Benchmarks are in [`docs/benchmarks`](docs/benchmarks/README.md).

## Pull requests

- Keep a pull request to one change. Small is easier to review.
- The generated code and the runtime fallback must give the same result
  ([ADR-0003](docs/adr/0003-mapping-rules.md)). A behaviour test goes through both paths; a
  generator change also gets a test in `tests/DotNetMapper.Generator.Tests`.
- Public API changes go in `src/DotNetMapper/PublicAPI.Unshipped.txt`. The build fails if you
  forget.
- Add a line to the `Unreleased` section of [`CHANGELOG.md`](CHANGELOG.md) for anything a user of
  the package would notice.
- A significant design decision gets an ADR in [`docs/adr/`](docs/adr). ADRs are not edited after
  they are accepted; a later ADR supersedes an earlier one.
- CI must pass. It builds and tests on Windows and Linux, and publishes a NativeAOT consumer.

## Commit messages

[Conventional Commits](https://www.conventionalcommits.org/): `type(scope): summary`, in the
imperative, lower case, no trailing period. Types: `feat`, `fix`, `docs`, `chore`, `refactor`,
`test`, `build`, `ci`, `perf`. The body says why, not what.

```
fix(generator): skip calls inside query expressions

A query clause over IQueryable becomes an expression tree, and a provider
cannot translate a call to a file-local generated method.
```

## Code style

The rules in [`.editorconfig`](.editorconfig) are enforced by the build. Public members need XML
documentation that says what the member does, what it returns and what breaks it.
