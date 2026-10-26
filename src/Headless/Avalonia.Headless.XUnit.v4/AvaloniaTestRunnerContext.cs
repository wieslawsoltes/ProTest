using System.Collections.Generic;
using System.Threading;
using Xunit.Sdk;
using Xunit.v3;

namespace Avalonia.Headless.XUnit;

internal sealed class AvaloniaTestRunnerContext(
    IXunitTest test,
    IMessageBus messageBus,
    ExplicitOption explicitOption,
    ExceptionAggregator aggregator,
    CancellationTokenSource cancellationTokenSource,
    ExecutionScheduler scheduler,
    IReadOnlyCollection<IBeforeAfterTestAttribute> beforeAfterTestAttributes,
    object?[] constructorArguments,
    FixtureMappingManager caseFixtureMappings,
    HeadlessUnitTestSession session)
    : XunitTestRunnerContext(
        test,
        explicitOption,
        messageBus,
        aggregator,
        cancellationTokenSource,
        // HeadlessUnitTestSession owns serialization and thread affinity. Letting the
        // xUnit scheduler run this inner body in parallel would leave the UI thread.
        ParallelMode.None,
        scheduler,
        beforeAfterTestAttributes,
        constructorArguments,
        caseFixtureMappings)
{
    public HeadlessUnitTestSession Session { get; } = session;
}
