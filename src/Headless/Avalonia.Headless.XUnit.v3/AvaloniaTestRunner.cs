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
        IReadOnlyCollection<IBeforeAfterTestAttribute> beforeAfterAttributes,
        HeadlessUnitTestSession session)
    {
        await using var ctxt = new AvaloniaTestRunnerContext(
            test,
            messageBus,
            explicitOption,
            aggregator,
            cancellationTokenSource,
            beforeAfterAttributes,
            constructorArguments,
            session
        );
        await ctxt.InitializeAsync();

        return await session.Dispatch(
            async () =>
            {
                return await Run(ctxt);
            },
            ctxt.CancellationTokenSource.Token);
    }
}
