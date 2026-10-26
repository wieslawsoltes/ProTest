using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Xunit.Sdk;
using Xunit.v3;

namespace Avalonia.Headless.XUnit;

internal sealed class AvaloniaTestRunner : XunitTestRunnerBase<AvaloniaTestRunnerContext, IXunitTest>
{
    public static AvaloniaTestRunner Instance { get; } = new();

    private AvaloniaTestRunner()
    {
    }

    public async ValueTask<RunSummary> Run(
        IXunitTest test,
        IMessageBus messageBus,
        object?[] constructorArguments,
        ExplicitOption explicitOption,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource,
        ParallelMode parallelMode,
        ExecutionScheduler scheduler,
        IReadOnlyCollection<IBeforeAfterTestAttribute> beforeAfterAttributes,
        FixtureMappingManager caseFixtureMappings,
        HeadlessUnitTestSession session)
    {
        await using var ctxt = new AvaloniaTestRunnerContext(
            test,
            messageBus,
            explicitOption,
            aggregator,
            cancellationTokenSource,
            scheduler,
            beforeAfterAttributes,
            constructorArguments,
            caseFixtureMappings,
            session
        );
        await ctxt.InitializeAsync();

        async ValueTask<RunSummary> RunOnDispatcher() => await session.Dispatch(
            async () =>
            {
                return await Run(ctxt);
            },
            ctxt.CancellationTokenSource.Token);

        // Enter xUnit's concurrency gate before dispatching. Scheduling inside the
        // dispatcher callback could replace Avalonia's synchronization context.
        return parallelMode == ParallelMode.All && !test.DisableParallelization
            ? await scheduler.RunParallelTask(RunOnDispatcher, cancellationTokenSource.Token)
            : await RunOnDispatcher();
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
