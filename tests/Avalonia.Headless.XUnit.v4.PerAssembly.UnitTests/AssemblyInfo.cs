global using Xunit;
global using Avalonia.Headless.XUnit;
using Avalonia.Headless;
using Avalonia.Headless.UnitTests;

[assembly: AvaloniaTestApplication(typeof(TestApplication))]
[assembly: AvaloniaTestIsolation(AvaloniaTestIsolationLevel.PerAssembly)]
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]
[assembly: Xunit.v3.Parallelization(Mode = Xunit.Sdk.ParallelMode.None)]
