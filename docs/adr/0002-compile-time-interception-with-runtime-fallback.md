# ADR-0002: Compile-time interception with runtime fallback

- Status: accepted
- Date: 2026-10-06

## Context

The public API is one method, `Mapper.Map<TInput, TOutput>(input)`. A runtime-only mapper pays
reflection or an expression compile somewhere, and 1.0.2 paid it on every call because its cache
used the value overload of `GetOrAdd`.

A source generator could emit code per call site, but the attribute-driven partial mapper model
that Mapperly uses would change the API. Interceptors keep the one-method API: the generator
emits a method decorated with `InterceptsLocationAttribute`, and the compiler redirects the
existing call.

## Decision

Ship a Roslyn incremental source generator inside the package. It finds
`Mapper.Map<A, B>(x)` where `A` and `B` are concrete and nameable, and emits
`new B { P = x.P, ... }` as an interceptor. Everything else uses the runtime fallback: one
`Func<A, B>` compiled once per type pair.

The generator compiles against Roslyn 5.0.0, the version that ships with the oldest .NET 10 SDK
band. A newer reference would stop the generator loading in an older band or IDE, so dependabot
is told to ignore those two packages.

The generator ships inside the `DotNetMapper` package at `analyzers/dotnet/cs/`, and a
`buildTransitive/DotNetMapper.props` file adds `DotNetMapper.Generated` to
`InterceptorsNamespaces`, which is what opts a consumer into interception.

## Consequences

- Intercepted calls are as fast as hand-written code and allocate only the result object.
- Call sites the generator cannot name fall back to the runtime path, so the API works everywhere
  the package installs.
- The generator and the runtime matcher must implement identical mapping rules, which the test
  suite checks by running every behaviour case through both paths.
