global using Xunit;
global using Avalonia.Headless.XUnit;
using Avalonia.Headless;
using Avalonia.Headless.UnitTests;

[assembly: AvaloniaTestApplication(typeof(TestApplication))]
[assembly: AvaloniaTestIsolation(AvaloniaTestIsolationLevel.PerTest)]

[assembly: Xunit.v3.Parallelization(Mode = Xunit.Sdk.ParallelMode.All, MaxThreads = 2)]
