---
title: "Packages and compatibility"
description: "Choose ProTest packages and understand assembly identities and supported versions."
sidebar_position: 1
doc-type: how-to
---

| Package | Includes | xUnit dependency | Avalonia |
| --- | --- | --- | --- |
| `ProTest.Avalonia.Headless` | Platform, sessions, input, trees, automation, recording | None | Exactly 12.1.4 |
| `ProTest.Avalonia.Headless.XUnit.v3` | Headless package and xUnit 3 adapter | 3.2.2, constrained to 3.x | Exactly 12.1.4 |
| `ProTest.Avalonia.Headless.XUnit.v4` | Headless package and xUnit 4 adapter | 4.0.2, constrained to 4.x | Exactly 12.1.4 |

All packages have version **12.1.4** and contain `net8.0` and `net10.0` assets. Install an adapter to receive the headless package transitively; install only the headless package for manual sessions or another test harness.

## Names and namespaces

| Code | Namespace | Assembly |
| --- | --- | --- |
| Headless platform and sessions | `Avalonia.Headless` | `Avalonia.Headless` |
| xUnit 3 attributes | `Avalonia.Headless.XUnit` | `Avalonia.Headless.XUnit.v3` |
| xUnit 4 attributes | `Avalonia.Headless.XUnit` | `Avalonia.Headless.XUnit.v4` |
| Input, gestures, tree helpers | `Avalonia.HeadlessTestingFramework` | `Avalonia.Headless` |
| In-process automation | `Avalonia.HeadlessTestingFramework.Appium` | `Avalonia.Headless` |
| Recording and conversion | `Avalonia.HeadlessTestingFramework.Recording` | `Avalonia.Headless` |

The upstream helper namespace remains available after moving from the separate `HeadlessTestingFramework` project into ProTest. No additional helper package is needed.

## Upgrade an existing project

1. Remove direct references to official `Avalonia.Headless` and `Avalonia.Headless.XUnit`, and the old standalone `HeadlessTestingFramework` project/package.
2. Add exactly one ProTest xUnit adapter and the matching xUnit runner package from the [quick start](xunit.md).
3. Keep existing `using Avalonia.Headless`, `using Avalonia.Headless.XUnit`, and helper namespace imports.
4. Align other Avalonia references, including themes, Skia, and application projects, to **12.1.4**.
5. Rebuild and run tests with the selected runner.

The xUnit framework's 4.x releases still use NuGet names beginning `xunit.v3` and namespaces `Xunit` / `Xunit.v3`. The ProTest `.v4` suffix selects the adapter for those releases.

## Official packages and private APIs

ProTest restores Avalonia from NuGet.org. Its headless assembly retains the upstream name and public signing key to satisfy Avalonia's friend-assembly declarations. The project also explicitly opts into unstable Avalonia APIs and pins the exact Avalonia version in package dependencies.

No Avalonia checkout or private feed is required to consume or build ProTest. The checked-in key is the upstream public signing identity, not a NuGet publishing credential. Changing Avalonia versions requires rebuilding and validating ProTest against that version.

## Consume a local build

If the matching package version is not yet published to NuGet.org, build this repository with `python3 scripts/build.py`. The resulting packages are written to `artifacts/packages`. Restore a consumer project using that directory alongside the official feed:

```sh
dotnet restore Tests.csproj \
  --source /absolute/path/to/ProTest/artifacts/packages \
  --source https://api.nuget.org/v3/index.json
```

Use the published package version once the release appears on NuGet.org. The repository's integration tests use an isolated package cache and source mapping to verify this local-package consumption path.
