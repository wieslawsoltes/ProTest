using System.Collections.Generic;
using System.Threading;
using Xunit.Sdk;
using Xunit.v3;

namespace Avalonia.Headless.XUnit;

internal sealed class AvaloniaTestCaseRunnerContext(
    IXunitTestCase testCase,
    IReadOnlyCollection<IXunitTest> tests,
    IMessageBus messageBus,
    ExceptionAggregator aggregator,
    CancellationTokenSource cancellationTokenSource,
    ParallelMode parallelMode,
    ExecutionScheduler scheduler,
    string displayName,
    string? skipReason,
    ExplicitOption explicitOption,
    object?[] constructorArguments,
    FixtureMappingManager methodFixtureMappings,
    HeadlessUnitTestSession session)
    : XunitTestCaseRunnerContext(
        testCase,
        tests,
        explicitOption,
        messageBus,
        aggregator,
        displayName,
        skipReason,
        cancellationTokenSource,
        parallelMode,
        scheduler,
        constructorArguments,
        methodFixtureMappings)
{
    public HeadlessUnitTestSession Session { get; } = session;
}
