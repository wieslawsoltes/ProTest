# Publishing ProTest packages

The release workflow publishes the exact packages validated by the Linux CI job after the Windows, Linux, and macOS jobs all pass. Every job exercises both .NET 8 and .NET 10, including projects restored from the newly built NuGet packages in a temporary directory outside this checkout. The package tests cover attribute-based integration and the optional assembly-wide test framework through the native xUnit runner, VSTest, and Microsoft.Testing.Platform. Separate intentional-failure probes verify that async and dispatcher exceptions fail their tests and the runner process.

The release contains `ProTest.Avalonia.Headless`, `ProTest.Avalonia.Headless.XUnit.v3`, and `ProTest.Avalonia.Headless.XUnit.v4`, plus their symbol packages.

## Repository configuration

The following GitHub settings were configured and verified for `wieslawsoltes/ProTest` on October 26, 2026:

| Setting | Value |
| --- | --- |
| Actions environment | `nuget` |
| Allowed deployment refs | Tags matching `v*` |
| Required reviewers | None |
| Wait timer | None |
| Environment variable | `NUGET_USER=wieslawsoltes` |

The username matches the [NuGet.org profile](https://www.nuget.org/profiles/wieslawsoltes). Releases proceed after the workflow's version check and CI matrix pass. Publishing authenticates with GitHub OIDC. Review the [environment settings](https://github.com/wieslawsoltes/ProTest/settings/environments) when changing release policy.

## One-time account setup

These steps describe how to reproduce or update the configuration, including for a fork.

1. Sign in to the NuGet.org account that should own the packages. The repository metadata credits **Wiesław Šoltés**; NuGet.org ownership is established by the publishing account and cannot be assigned through a `.csproj` or `.nuspec` field.
2. Add a GitHub Actions environment named `nuget` to the repository. Select deployment branches and tags, then add a **tag** rule for `v*` to match the release workflow. The ProTest configuration has no required reviewer or wait timer; add those only if your release process needs them.
3. Add the GitHub Actions variable `NUGET_USER` at repository level or in the `nuget` environment. Use your NuGet.org **profile username**, not an email address. ProTest uses `wieslawsoltes` in the `nuget` environment. No long-lived API key secret is needed.
4. In NuGet.org, open your account's **Trusted Publishing** settings and add a GitHub policy with these values:

   | Setting | Value |
   | --- | --- |
   | Repository owner | `wieslawsoltes` |
   | Repository | `ProTest` |
   | Workflow file | `release.yml` |
   | Environment | `nuget` |
   | Package pattern | `ProTest.*` |

   Select the intended package owner and grant the scopes for creating new packages and publishing new versions. Enter only `release.yml` in the workflow field, without `.github/workflows/`.

The workflow grants `id-token: write` only to the publishing job. `NuGet/login` exchanges the GitHub OIDC token for a short-lived NuGet key immediately before publishing. The key is passed through an environment variable and is not stored in the repository. See Microsoft's [trusted publishing documentation](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing) and the official [NuGet/login action](https://github.com/NuGet/login).

## Create a release

1. Update `build/Version.props`. Keep `AvaloniaVersion` pinned to the official Avalonia release against which the private API integration has been verified; the ProTest package version begins with the same Avalonia version. Use a prerelease suffix or a fourth version component when publishing subsequent ProTest builds against the same Avalonia release. After changing dependency versions, run `dotnet restore ProTest.slnx --force-evaluate` and commit the updated lock files; CI restores the source solution in locked mode.
2. Run `python scripts/build.py`. This requires the .NET 10 SDK and .NET 8 and 10 runtimes. The script writes packages to `artifacts/packages` and test results to `artifacts/test-results`.
3. Commit and push the reviewed changes. Tag that commit with `v` followed by the exact `Version` value from `build/Version.props`, then push the tag. For example, a `Version` of `12.1.4-preview.1` requires `v12.1.4-preview.1`.
4. The **Release** workflow checks the version, reruns the full CI matrix, and publishes the validated `.nupkg` and accompanying `.snupkg` files. Approve the `nuget` environment deployment if you configured required reviewers.
5. After NuGet publishing succeeds, a separate job creates a GitHub release with generated notes and attaches those same packages and symbols. Version tags containing a hyphen produce GitHub prereleases. Only this job receives `contents: write`; it receives no NuGet credential or OIDC permission.

The workflow does not build again after validation. `dotnet nuget push` also uploads each package's adjacent symbol package. A partially completed release can be rerun; `--skip-duplicate` permits already published versions to be skipped, and an existing GitHub release receives updated attachments. Package versions already published to NuGet.org are immutable, so changed package contents require a new version.

## Local commands

```sh
# Full CI-equivalent validation on both supported frameworks.
python scripts/build.py

# Run tests only on .NET 10, while still building and packing both target frameworks.
python scripts/build.py --framework net10.0

# Recheck existing packages and rerun isolated package consumer tests.
python scripts/build.py --package-tests-only --framework net10.0

# Validate a proposed release tag without building or publishing.
python scripts/build.py --check-tag v12.1.4-preview.1
```

CI and release configuration are committed to this repository. Creating the GitHub environment, setting `NUGET_USER`, and registering the NuGet.org policy are account-level setup steps; building the repository does not perform them.
