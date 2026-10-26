---
title: "Start with xUnit 3 or 4"
description: "Create an executable test project and run Avalonia facts and theories."
sidebar_position: 2
doc-type: how-to
---

Create a directory for your tests and save this as `Tests.csproj`. This example selects xUnit 4; the substitutions for xUnit 3 are listed below.

```xml title="Tests.csproj"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <OutputType>Exe</OutputType>
    <IsTestProject>true</IsTestProject>
    <IsPackable>false</IsPackable>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="ProTest.Avalonia.Headless.XUnit.v4" Version="12.1.4" />
    <PackageReference Include="xunit.v3.mtp-off" Version="4.0.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="4.0.1" PrivateAssets="all" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.11.0" />
  </ItemGroup>
</Project>
```

For **xUnit 3**, use `ProTest.Avalonia.Headless.XUnit.v3` version `12.1.4` and `xunit.v3.mtp-off` version `3.2.2`. Set the target framework to `net8.0` if your tests run on .NET 8. Both versions support the same attributes and application setup.

## Configure the application

```csharp title="TestAppBuilder.cs"
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

Place the assembly attribute once in the test project. The adapter creates the application and dispatches UI work. Avoid initializing another Avalonia application yourself.

## Write facts and theories

```csharp title="EditorTests.cs"
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Xunit;

namespace Tests;

public class EditorTests
{
    [AvaloniaFact]
    public async Task Typing_updates_text()
    {
        var editor = new TextBox();
        var window = new Window { Width = 240, Height = 80, Content = editor };
        window.Show();
        try
        {
            editor.Focus();
            window.KeyTextInput("Ready");
            await Task.Yield();
            Assert.True(Dispatcher.UIThread.CheckAccess());
            Assert.Equal("Ready", editor.Text);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTheory]
    [InlineData("alpha")]
    [InlineData("beta")]
    public void Theory_rows_have_dispatcher_access(string value)
    {
        Assert.True(Dispatcher.UIThread.CheckAccess());
        Assert.Equal(value, new TextBox { Text = value }.Text);
    }
}
```

```sh
dotnet test
```

Use `[AvaloniaFact]` and `[AvaloniaTheory]` for controls, layout, binding, input, or dispatcher operations. Ordinary `[Fact]` tests remain useful for independent model and service logic.

## Add application themes

Template-dependent controls need the theme used by your application. Add `Avalonia.Themes.Fluent` at `12.1.4` and configure an application class:

```csharp
using Avalonia;
using Avalonia.Themes.Fluent;

public sealed class TestApplication : Application
{
    public override void Initialize() => Styles.Add(new FluentTheme());
}
```

Change the builder to `AppBuilder.Configure<TestApplication>()`. You can also reuse an application's XAML initialization and styles, provided its startup does not create a separate desktop lifetime.

## Next steps

- [Isolation and lifecycle](../guides/isolation.md) explains application reuse and assembly-wide integration.
- [Test runners](../guides/runners.md) covers VSTest, native xUnit execution, and Microsoft Testing Platform.
- [Headless platform](../guides/headless-platform.md) covers input, layout, and manual sessions.

Topic reference: [Avalonia's xUnit guide](https://docs.avaloniaui.net/docs/testing/headless-xunit). The project configuration above is specific to ProTest's xUnit 3 and 4 packages.
