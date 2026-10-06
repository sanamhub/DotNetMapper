# Contributing to DotNetMapper

Here's how you can contribute:

1. Fork the repository.
2. Create a new branch for your changes.
3. Make your changes and ensure they are properly tested.
4. Create a pull request with a clear description of your changes and why they are necessary.
5. Wait for your pull request to be reviewed.

Please ensure that your code follows the style and conventions used in the rest of the project,
and that your changes do not break any existing functionality.

By contributing to DotNetMapper, you agree to release your changes under the terms of the MIT
License.

## Building and testing

```bash
dotnet build
dotnet test
```

The build treats warnings as errors and runs the full analyzer set, so a warning is a failed
build. Tests run on Microsoft.Testing.Platform.

## Benchmarks

```bash
dotnet run -c Release --project benchmarks/DotNetMapper.Benchmarks -- --filter "*"
```

Results go under `docs/benchmarks/` when they change.

## Commits

Use conventional commits: `feat`, `fix`, `docs`, `chore`, `refactor`, `test`, `build`, `ci`,
`perf`. The summary is imperative, lower case, and under 72 characters. The body explains why.

## Keep it minimal

DotNetMapper is one public method and nothing else. A change that adds a public type, an
attribute, a profile, or a configuration knob is probably the wrong change. If the feature needs
configuration or conversions, Mapperly already covers it.
