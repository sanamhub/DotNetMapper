# Release runbook

## Before tagging

1. `CHANGELOG.md` has an entry for the version, moved out of Unreleased. The GitHub release notes
   are built from that section.
2. `src/DotNetMapper/DotNetMapper.csproj` has the matching `<Version>`.
3. For a stable release, `src/DotNetMapper/PublicAPI.Unshipped.txt` is moved to
   `PublicAPI.Shipped.txt`, leaving only `#nullable enable`.
4. CI is green on `main`.

The first three are checked by the `preflight` job before anything is built.

## Releasing

```bash
git tag v2.0.0
git push origin v2.0.0
```

That triggers `release.yml`:

| Job | What it does |
| --- | --- |
| `preflight` | Checks the tag, the project version and the changelog agree. |
| `verify` | Packs and consumes the package under NativeAOT from a clean project. |
| `publish` | Waits on the `production` environment approval, then pushes to nuget.org with trusted publishing and creates the GitHub release. |

Approve the deployment at the run's page, under `publish`, `Review deployments`.

The GitHub release is created by the workflow, with notes from the changelog section, an install
snippet, and the `.nupkg` and `.snupkg` attached. A tag containing a `-` is marked as a
prerelease. Do not create one by hand at `/releases/new`; that only produces a duplicate.

Use the `workflow_dispatch` trigger for a dry run of everything except the push and the release.

## Rolling back

A published NuGet package cannot be deleted. Unlisting is the only rollback.

```bash
dotnet nuget delete DotNetMapper <version> --source https://api.nuget.org/v3/index.json --non-interactive
```

Despite the command name that unlists rather than deletes. The package stays resolvable for
anyone pinned to that exact version. Follow it with a patch release, because unlisting alone
leaves existing consumers on the bad version.

## After releasing

1. Read the release page. Confirm the notes match the changelog and both files are attached.
2. Install the package from nuget.org into a fresh console app and run one mapping call.
3. Open the next `Unreleased` section in `CHANGELOG.md` and bump `<Version>` for the next cycle.

## One time setup on nuget.org

Publishing uses trusted publishing, so there is no long lived API key. Register the policy once,
at nuget.org under your username, Trusted Publishing:

| Field | Value |
| --- | --- |
| Repository Owner | `sanamhub` |
| Repository | `DotNetMapper` |
| Workflow File | `release.yml` (file name only, no path) |
| Environment | `production` |

Then add one repository secret, `NUGET_USER`, holding the nuget.org profile name, not the email.

Also in the repository: create the `production` environment with a required reviewer and a tag
rule of `v*`, and enable private vulnerability reporting. The dependency-review job needs the
repository Dependency graph enabled.
