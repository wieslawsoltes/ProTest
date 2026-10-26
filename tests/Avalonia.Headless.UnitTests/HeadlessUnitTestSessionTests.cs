// These lifecycle regressions use their own bounded waits to verify session cancellation independently.
#pragma warning disable xUnit1051
using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace Avalonia.Headless.UnitTests;

#if XUNIT
[Collection("Session lifecycle")]
#endif
public class HeadlessUnitTestSessionTests
{
#if NUNIT
    [TestCase(false)]
    [TestCase(true)]
#elif XUNIT
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
#endif
    public async Task Session_Should_Support_Immediate_Disposal(bool asynchronously)
    {
        for (var i = 0; i < 100; ++i)
        {
            var session = HeadlessUnitTestSession.StartNew(typeof(TestApplication));
            if (asynchronously)
                await session.DisposeAsync();
            else
                session.Dispose();

            // Repeated disposal must be harmless, including mixed sync/async callers.
            session.Dispose();
            await session.DisposeAsync();
        }
    }

#if NUNIT
    [Test]
#elif XUNIT
    [Fact]
#endif
    public async Task Dispatch_Should_Report_Cleanup_Exceptions_And_Continue()
    {
        var session = HeadlessUnitTestSession.GetOrStartForAssembly(GetType().Assembly);

        const string message = "Thrown by a dispatcher job during cleanup.";
        var poisonedDispatch = session.Dispatch(
            () => Dispatcher.UIThread.Post(() => throw new InvalidOperationException(message)),
            CancellationToken.None);

#if NUNIT
        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
#elif XUNIT
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
#endif
            await poisonedDispatch.WaitAsync(TimeSpan.FromSeconds(10)));

        AssertHelper.Equal(message, exception!.Message);

        var result = await session.Dispatch(() => 42, CancellationToken.None)
            .WaitAsync(TimeSpan.FromSeconds(10));

        AssertHelper.Equal(42, result);
    }
}
