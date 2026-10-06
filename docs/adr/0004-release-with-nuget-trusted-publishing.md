# ADR-0004: Release with NuGet trusted publishing

- Status: accepted
- Date: 2026-10-06

## Context

A long-lived NuGet API key is a secret that can leak and must be rotated. The repository owner's
other projects publish through NuGet trusted publishing, which exchanges a short-lived GitHub
OIDC token for a temporary API key.

## Decision

Publishing goes through a `release.yml` workflow with three jobs. `preflight` checks that the
tag, the project `<Version>` and the changelog section agree. `verify` packs and consumes the
package under NativeAOT. `publish` waits on the `production` environment approval, exchanges the
OIDC token with the `NuGet/login` action, pushes with the temporary key, and creates the GitHub
release from the changelog section.

The only repository secret is `NUGET_USER`, the nuget.org profile name. No API key exists.

## Consequences

- A release cannot publish a version whose tag, project file and changelog disagree.
- The package is verified from a clean consumer before it is pushed.
- One-time setup on nuget.org is required: a trusted publishing policy for this repository and
  workflow, plus the `production` environment and the `NUGET_USER` secret. The runbook lists it.
