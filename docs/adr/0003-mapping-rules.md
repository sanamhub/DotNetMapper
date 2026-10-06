# ADR-0003: Mapping rules

- Status: accepted
- Date: 2026-10-06

## Context

The two execution paths, interception and runtime fallback, must produce identical results. That
forces the mapping rules to be written down once and implemented twice.

1.0.2 had several holes: it wrote private setters, read non-public getters, threw on indexers,
and boxed both sides through `Func<object, object>`.

## Decision

The rules are:

- `null` input throws `ArgumentNullException` with `ParamName == "inputObject"`, except for
  non-nullable value types where no check is needed.
- The result is `new TOutput()` followed by assignments.
- Source properties: public, instance, not an indexer, with a public getter. Inherited included.
- Target properties: public, instance, not an indexer, with a public `set` or `init`. Inherited
  included.
- A pair matches on equal name (ordinal, case-sensitive) and identical type. Nullable reference
  annotations are ignored.
- Unmatched properties are ignored. Values are assigned, not cloned.
- When a name appears more than once (hidden with `new`, or overridden), the most-derived
  declaration wins, and only then are its accessors checked. A hiding property with a private
  setter therefore blocks the public base setter, and an override that declares only `get` is
  read-only. Checking accessors first would let the generator emit an assignment that C# binds to
  the derived property and refuses to compile.
- Interfaces consider only properties declared on that interface itself.

## Consequences

- Private, internal and protected setters are no longer written. This is a breaking change from
  1.0.2 and goes in the changelog under Changed.
- Write-only source properties and non-public getters are skipped instead of throwing.
- Both paths implement the same rules, and the test suite runs each behaviour case through both.
