# Attribution and source provenance

ProTest is maintained by [Wiesław Šoltés](https://github.com/wieslawsoltes).
It contains software from the AvaloniaUI project, distributed under the MIT
license in [licence.md](licence.md).

The headless platform, original xUnit adapter, shared headless tests, signing
key, and public-key declaration were imported from
[`wieslawsoltes/Avalonia` at `6b6c573a03599510b568033757f59c908f7328a7`](https://github.com/wieslawsoltes/Avalonia/tree/6b6c573a03599510b568033757f59c908f7328a7).
The renderer implementation in
`src/Headless/Avalonia.Headless/HeadlessPlatformRenderInterface.cs` comes from
the matching Avalonia 12.1.4 release,
[`2d79ba55fae1b1e71c69f93a668080c9029e6a31`](https://github.com/AvaloniaUI/Avalonia/tree/2d79ba55fae1b1e71c69f93a668080c9029e6a31),
because the fork's development renderer depends on unreleased geometry APIs.

ProTest changes include standalone NuGet dependencies, package metadata,
the xUnit 4.x adapter, cancellable session shutdown and cleanup, regression
and package-consumer tests, and build and release automation. Original C#
namespaces and the `Avalonia.Headless` assembly identity are retained; adapter
projects, assemblies, and packages use `.v3` and `.v4` suffixes. Compatible
adapter source files are linked into both projects. ProTest is an independent
distribution, not an official Avalonia or xUnit package.

`build/avalonia.snk` is the signing key deliberately distributed in Avalonia's
public source repository. It preserves the assembly identity used by
Avalonia's `InternalsVisibleTo` declarations; it is not a NuGet publishing
credential. NuGet publishing uses a separate GitHub OIDC trusted-publishing
policy.

Third-party NuGet dependencies retain their own licenses and ownership.

## PanAndZoom testing helpers

The 31 C# files under `src/Headless/Avalonia.Headless/Testing` and the imported
helper tests originate from [PanAndZoom at
`0c938aa771970ad8e2387f669f910c8b2a2d4d30`](https://github.com/wieslawsoltes/PanAndZoom/tree/0c938aa771970ad8e2387f669f910c8b2a2d4d30/src/HeadlessTestingFramework).
Copyright (c) Wiesław Šoltés. They are distributed under the MIT license
in `licence.md`; the upstream [LICENSE.TXT](https://github.com/wieslawsoltes/PanAndZoom/blob/0c938aa771970ad8e2387f669f910c8b2a2d4d30/LICENSE.TXT) grants those terms.
ProTest organizes these files by feature, retains their
`Avalonia.HeadlessTestingFramework` namespaces, and includes them in
`ProTest.Avalonia.Headless`. Recording fixes and compatibility tests are
maintained here. Documentation adaptations and upstream guide coverage are
described in [the documentation source inventory](docs/content/attribution/sources.md).
