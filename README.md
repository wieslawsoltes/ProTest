# ProTest

[![CI](https://github.com/wieslawsoltes/ProTest/actions/workflows/ci.yml/badge.svg)](https://github.com/wieslawsoltes/ProTest/actions/workflows/ci.yml)
[![Documentation](https://github.com/wieslawsoltes/ProTest/actions/workflows/docs.yml/badge.svg)](https://wieslawsoltes.github.io/ProTest/)

Headless Avalonia UI testing for xUnit 3.x and 4.x, maintained by
[Wiesław Šoltés](https://github.com/wieslawsoltes).

ProTest packages Avalonia's headless platform and xUnit integration as a
standalone solution, adds an adapter for xUnit 4.x, and builds against
**official Avalonia NuGet packages**. No Avalonia checkout, private feed,
or developer signing secret is required.

**[Read the documentation](https://wieslawsoltes.github.io/ProTest/)** for setup,
headless testing, xUnit integration, input simulation, tree inspection,
Appium-style automation, and recording.

## Testing helpers

`ProTest.Avalonia.Headless` includes a comprehensive set of testing helpers:

- Mouse, keyboard, touch, and multi-touch simulation.
- Gesture recognizer helpers and scripted gestures.
- Visual and logical tree queries, XPath-like selectors, validation, and comparison.
- An in-process Appium-style driver with locators, actions, and waits.
- Frame capture, image sequences, recording sessions, and optional FFmpeg video conversion.

The source is organized under `src/Headless/Avalonia.Headless/Testing`.
Existing `Avalonia.HeadlessTestingFramework`, `.Appium`, and `.Recording`
namespaces are preserved. These APIs ship in the headless package and require
no separate `HeadlessTestingFramework` project reference. The Appium-style API
runs in-process; real Appium server testing is documented separately.

## Packages and compatibility

| Package | NuGet | Purpose | Tested xUnit version | Avalonia version |
| --- | --- | --- | --- | --- |
| `ProTest.Avalonia.Headless` | [![NuGet](https://img.shields.io/nuget/v/ProTest.Avalonia.Headless.svg)](https://www.nuget.org/packages/ProTest.Avalonia.Headless) | Headless platform, test sessions, and testing helpers | Framework independent | **12.1.4**, exact |
| `ProTest.Avalonia.Headless.XUnit.v3` | [![NuGet](https://img.shields.io/nuget/v/ProTest.Avalonia.Headless.XUnit.v3.svg)](https://www.nuget.org/packages/ProTest.Avalonia.Headless.XUnit.v3) | xUnit 3.x adapter | **3.2.2** | **12.1.4**, exact |
| `ProTest.Avalonia.Headless.XUnit.v4` | [![NuGet](https://img.shields.io/nuget/v/ProTest.Avalonia.Headless.XUnit.v4.svg)](https://www.nuget.org/packages/ProTest.Avalonia.Headless.XUnit.v4) | xUnit 4.x adapter | **4.0.2** | **12.1.4**, exact |

All three packages target **.NET 8 and .NET 10**. Their initial version is
**12.1.4**, aligned with Avalonia. New Avalonia versions require a tested
ProTest release because the headless platform uses version-specific internals.

**xUnit naming:** the xUnit framework's 4.x releases still ship as
`xunit.v3.*` packages and use `Xunit` / `Xunit.v3` namespaces. ProTest's `.v4`
suffix identifies the adapter for those 4.x releases. The adapters constrain
their xUnit dependency to the corresponding major version.

Install **one adapter per test project**. Both expose the original
`Avalonia.Headless.XUnit` namespace, including `[AvaloniaFact]` and
`[AvaloniaTheory]`. Adapter projects, assemblies, and packages use matching
`.v3` and `.v4` suffixes. The headless assembly remains `Avalonia.Headless`.
Remove official `Avalonia.Headless` / `Avalonia.Headless.XUnit` package
references when migrating: ProTest supplies those types and assembly identities.
Keep other Avalonia packages, such as themes and Skia, at version **12.1.4**.

## Quick start

Create a test project with the following project file. This example uses
xUnit 4.x and supports `dotnet test` with the VSTest runner:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <OutputType>Exe</OutputType>
    <IsTestProject>true</IsTestProject>
    <IsPackable>false</IsPackable>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="ProTest.Avalonia.Headless.XUnit.v4" Version="12.1.4" />
    <PackageReference Include="xunit.v3.mtp-off" Version="4.0.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="4.0.1" PrivateAssets="all" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.11.0" />
  </ItemGroup>
</Project>
```

The `mtp-off` package supports VSTest on .NET 10 while retaining xUnit's
native executable runner. For xUnit 3.x, replace the adapter with `ProTest.Avalonia.Headless.XUnit.v3`
and set `xunit.v3.mtp-off` to **3.2.2**. Use `net8.0` to run on .NET 8.
Use reflection-based xUnit packages; xUnit Native AOT packages are not
supported by these reflection-based adapters.

Configure the test application in `TestAppBuilder.cs`:

```csharp
using Avalonia;
using Avalonia.Headless;

[assembly: AvaloniaTestApplication(typeof(Tests.TestAppBuilder))]

namespace Tests;

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<Application>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
```

Use the Avalonia attributes for tests that need the UI thread. Constructors,
test bodies, async continuations, and test-class cleanup run through the
headless session:

```csharp
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Xunit;

namespace Tests;

public class EditorTests
{
    [AvaloniaFact]
    public async Task Text_input_updates_the_focused_control()
    {
        var editor = new TextBox();
        var window = new Window { Content = editor };
        window.Show();
        try
        {
            editor.Focus();
            window.KeyTextInput("Hello, ProTest");
            await Task.Yield();

            Assert.True(Dispatcher.UIThread.CheckAccess());
            Assert.Equal("Hello, ProTest", editor.Text);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTheory]
    [InlineData("first")]
    [InlineData("second")]
    public void Theory_rows_run_on_the_UI_thread(string value)
    {
        Assert.True(Dispatcher.UIThread.CheckAccess());
        Assert.Equal(value, new TextBox { Text = value }.Text);
    }
}
```

Run the tests:

```sh
dotnet test
```

The default headless drawing backend supports control behavior and input
without a display server. Add the same themes your application uses when
testing template-dependent behavior.

## Microsoft Testing Platform

To use Microsoft Testing Platform with the .NET 10 SDK, replace
`xunit.v3.mtp-off` with `xunit.v3.mtp-v2` at the same xUnit version.
Keep the ProTest adapter and test application configuration unchanged.
Place this `global.json` in the test project directory or a parent directory
(or merge the `test` section into your existing file):

```json
{
  "test": {
    "runner": "Microsoft.Testing.Platform"
  }
}
```

From that directory, run:

```sh
dotnet test --project YourTests.csproj
```

This setup works with both the xUnit 3.x and 4.x adapters. You can remove
`xunit.runner.visualstudio` and `Microsoft.NET.Test.Sdk` if all your tools
use MTP. For MTP command-line options, see the
[official xUnit guide](https://xunit.net/docs/getting-started/v3/microsoft-testing-platform).
The repository's package integration tests exercise VSTest, the native
xUnit runner, and the .NET 10 SDK's MTP runner separately.

## Assembly-wide UI tests and isolation

To run ordinary `[Fact]` and `[Theory]` methods through Avalonia, add:

```csharp
using Avalonia.Headless.XUnit;

[assembly: AvaloniaTestFramework]
```

By default, a fresh application/service scope is created for every test.
To share an application for the assembly, add:

```csharp
using Avalonia.Headless;

[assembly: AvaloniaTestIsolation(AvaloniaTestIsolationLevel.PerAssembly)]
```

Use per-test isolation when tests mutate global services or application
resources. UI work is serialized through the headless session even when
xUnit schedules tests in parallel. Normal xUnit tests can coexist with
Avalonia-attributed tests when assembly-wide integration is not enabled.

Keep UI initialization in the test-class constructor or `InitializeAsync`.
Class and collection fixtures follow xUnit's own lifetime and do not
automatically receive dispatcher affinity.

## Rendering tests

For real pixel rendering, reference `Avalonia.Skia` and your theme package
at **12.1.4**, then configure:

```csharp
public static AppBuilder BuildAvaloniaApp() =>
    AppBuilder.Configure<Application>()
        .UseSkia()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions
        {
            UseHeadlessDrawing = false
        });
```

After showing a window, call `window.CaptureRenderedFrame()` in an Avalonia
test to obtain a bitmap. The repository's shared headless tests include
rendering, keyboard/mouse/touch input, popup behavior, and session isolation.

## Direct headless sessions

The framework-independent `ProTest.Avalonia.Headless` package also supports
explicit sessions:

```csharp
await using var session = HeadlessUnitTestSession.StartNew(typeof(TestAppBuilder));
bool onUiThread = await session.Dispatch(async () =>
{
    await Task.Yield();
    return Dispatcher.UIThread.CheckAccess();
}, CancellationToken.None);
```

Dispose sessions from outside their dispatcher thread. Cancellation of queued
work prevents its delegate from running. Canceling an incomplete active
delegate stops that session, cancels its queue, and shuts down its dispatcher
so old UI continuations cannot enter another test. Start a new session after
that cancellation. A completed delegate retains its result; cancellation
cannot interrupt synchronous code that blocks the UI thread.

## Private APIs and signing

The standalone headless project uses Avalonia's supported build opt-in for
unstable private APIs and pins its official Avalonia dependencies to
`[12.1.4]`. This selects the implementation assemblies rather than the
restricted public reference assemblies.

Internal friend access additionally requires the original `Avalonia.Headless`
assembly name and matching strong-name key. The repository includes Avalonia's
publicly distributed `build/avalonia.snk` and public-key declaration. This key
is for assembly identity; it is unrelated to NuGet publishing credentials.
Consumer test projects need neither that key nor the private-API build flags.

See [Avalonia's private API packaging guidance](https://github.com/AvaloniaUI/Avalonia/wiki/Using-private-apis-in-nuget-packages)
and [source provenance](NOTICE.md).

## Build, test, and publish

Compatible adapter sources live in `src/Headless/Avalonia.Headless.XUnit.v3`
and are linked into the `.v4` project with MSBuild `Compile` items. Shared
attributes, discovery, test-case definitions, and result handling are maintained
once; version-specific execution and scheduling code stays in each project.
Both adapters keep the same C# namespace and compile into separate assemblies.

Build with the .NET 10 SDK; install the .NET 8 and .NET 10 runtimes to run
the full test matrix. Python 3 drives the portable verification script.

```sh
python3 scripts/build.py
```

CI runs on Linux, Windows, and macOS. It builds both target frameworks,
executes the headless tests, creates `.nupkg` and `.snupkg` files, checks
package metadata, and runs separate consumer projects against the packed
packages. Consumers are copied outside this checkout and restored using
only the local package feed and NuGet.org, with an isolated package cache.
Both attribute-based and assembly-wide integration run through the native
xUnit runner and VSTest. Separate failure probes verify nonzero exits and
exception reporting; ordinary test runs reject failures and runner errors.

Packages are written to `artifacts/packages`. Publishing uses GitHub OIDC
and NuGet trusted publishing. Configure the NuGet policy, GitHub environment,
and `NUGET_USER` variable as described in [publishing.md](docs/publishing.md).
NuGet gallery ownership is assigned by NuGet.org, not by project metadata.

## License

[MIT](licence.md). Original Avalonia attribution is preserved in
[NOTICE.md](NOTICE.md). ProTest is an independent distribution maintained by
Wiesław Šoltés.
