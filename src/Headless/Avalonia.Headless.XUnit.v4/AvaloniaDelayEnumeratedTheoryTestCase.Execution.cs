using System.Threading;
using System.Threading.Tasks;
using Xunit.Sdk;
using Xunit.v3;

namespace Avalonia.Headless.XUnit;

internal sealed partial class AvaloniaDelayEnumeratedTheoryTestCase
{
    public async ValueTask<RunSummary> Run(
        ExplicitOption explicitOption,
        IMessageBus messageBus,
        object?[] constructorArguments,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource,
        ParallelMode parallelMode,
        ExecutionScheduler scheduler,
        FixtureMappingManager methodFixtureMappings)
    {
        var tests = await aggregator.RunAsync(CreateTests, []);

        return await AvaloniaTestCaseRunner.Instance.Run(
            this,
            tests,
            messageBus,
            aggregator,
            cancellationTokenSource,
            parallelMode,
            scheduler,
            TestCaseDisplayName,
            SkipReason,
            explicitOption,
            constructorArguments,
            methodFixtureMappings);
    }
}
