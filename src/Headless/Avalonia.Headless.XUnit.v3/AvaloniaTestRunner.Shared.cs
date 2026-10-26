using System;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace Avalonia.Headless.XUnit;

internal sealed partial class AvaloniaTestRunner
{
    public static AvaloniaTestRunner Instance { get; } = new();

    private AvaloniaTestRunner()
    {
    }

    protected override async ValueTask<TimeSpan> RunTest(AvaloniaTestRunnerContext ctxt)
    {
        var elapsed = await base.RunTest(ctxt);
        // Drain work posted by the test and its disposal while xUnit is still
        // collecting the test result. Flushing after Run emits a false pass followed
        // by a test-case cleanup error when a dispatcher callback throws.
        ctxt.Aggregator.Run(() => Dispatcher.UIThread.RunJobs());
        return elapsed;
    }
}
