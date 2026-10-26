using System.Reflection;
using Xunit.v3;

namespace Avalonia.Headless.XUnit;

internal sealed class AvaloniaTestFramework : XunitTestFramework
{
    protected override ITestFrameworkDiscoverer CreateDiscoverer(Assembly assembly)
        => new AvaloniaTestFrameworkDiscoverer(CreateTestAssembly(assembly));

    protected override ITestFrameworkExecutor CreateExecutor(Assembly assembly)
        => new AvaloniaTestFrameworkExecutor(CreateTestAssembly(assembly));

    private static XunitTestAssembly CreateTestAssembly(Assembly assembly)
#if XUNIT_V4
        => new(assembly, configFilePath: null, version: assembly.GetName().Version);
#else
        => new(assembly, null, assembly.GetName().Version);
#endif
}
