using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Themes.Simple;
using Xunit;

[assembly: AvaloniaTestApplication(typeof(PackageConsumers.TestApplication))]
[assembly: AvaloniaTestIsolation(AvaloniaTestIsolationLevel.PerTest)]

#if AVALONIA_TEST_FRAMEWORK
[assembly: AvaloniaTestFramework]
#endif

#if XUNIT_V4
[assembly: Xunit.v3.Parallelization(Mode = Xunit.Sdk.ParallelMode.All, MaxThreads = 2)]
#endif

namespace PackageConsumers;

public sealed class TestApplication : Application
{
    public override void Initialize() => Styles.Add(new SimpleTheme());

    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<TestApplication>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
        .UseSkia()
        .UseHarfBuzz();
}
