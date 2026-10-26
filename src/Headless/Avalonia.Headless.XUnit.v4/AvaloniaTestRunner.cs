using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Sdk;
using Xunit.v3;

namespace Avalonia.Headless.XUnit;

internal sealed partial class AvaloniaTestRunner : XunitTestRunnerBase<AvaloniaTestRunnerContext, IXunitTest>
{
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
}
