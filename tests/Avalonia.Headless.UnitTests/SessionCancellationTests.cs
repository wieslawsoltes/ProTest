#nullable enable
// Cancellation is the subject under test; cleanup must also run after the test token is canceled.
#pragma warning disable xUnit1051

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Avalonia.Headless.UnitTests;

// Each test owns its session; do not reset global Avalonia state alongside another collection.
[CollectionDefinition("Session lifecycle", DisableParallelization = true)]
public sealed class SessionLifecycleCollection;

// Independent sessions reset process-wide dispatcher state, so exercise them only with per-test isolation.
#if PERTEST
[Collection("Session lifecycle")]
public sealed class SessionCancellationTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    [Theory]
    [InlineData(AvaloniaTestIsolationLevel.PerTest)]
    [InlineData(AvaloniaTestIsolationLevel.PerAssembly)]
    public async Task Active_Cancellation_Stops_The_Session_Without_Leaking_Into_A_New_One(AvaloniaTestIsolationLevel isolation)
    {
        var session = HeadlessUnitTestSession.StartNew(typeof(TestApplication), isolation);
        var pending = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var cancellation = new CancellationTokenSource();
        SynchronizationContext? oldContext = null;
        var staleCallbackExecuted = false;
        try
        {
            var dispatch = session.Dispatch(() =>
            {
                oldContext = SynchronizationContext.Current;
                started.SetResult();
                return pending.Task;
            }, cancellation.Token);
            await started.Task.WaitAsync(Timeout);
            cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => dispatch.WaitAsync(Timeout));
            Assert.Throws<ObjectDisposedException>(() => { _ = session.Dispatch(() => 42, CancellationToken.None); });
            Assert.NotNull(oldContext);
            try
            {
                oldContext.Post(_ => staleCallbackExecuted = true, null);
            }
            catch (InvalidOperationException)
            {
                // Rejecting a post to the stopped dispatcher also prevents stale work.
            }
            await using var replacement = HeadlessUnitTestSession.StartNew(typeof(TestApplication));
            Assert.Equal(42, await replacement.Dispatch(() => 42, CancellationToken.None).WaitAsync(Timeout));
            Assert.False(staleCallbackExecuted);
        }
        finally
        {
            // A regressed implementation may still be blocked on this task: release it for cleanup.
            pending.TrySetResult(0);
            await Task.Run(async () => await session.DisposeAsync()).WaitAsync(Timeout);
        }
    }

    [Fact]
    public async Task Canceled_Queued_Dispatch_Does_Not_Invoke_Its_Delegate()
    {
        var session = HeadlessUnitTestSession.StartNew(typeof(TestApplication));
        var pending = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var cancellation = new CancellationTokenSource();
        var invoked = false;
        try
        {
            var first = session.Dispatch(() =>
            {
                started.SetResult();
                return pending.Task;
            }, CancellationToken.None);
            await started.Task.WaitAsync(Timeout);
            var queued = session.Dispatch(() => invoked = true, cancellation.Token);
            cancellation.Cancel();
            pending.TrySetResult(1);
            Assert.Equal(1, await first.WaitAsync(Timeout));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => queued.WaitAsync(Timeout));
            Assert.False(invoked);
            Assert.Equal(42, await session.Dispatch(() => 42, CancellationToken.None).WaitAsync(Timeout));
        }
        finally
        {
            pending.TrySetResult(0);
            await Task.Run(async () => await session.DisposeAsync()).WaitAsync(Timeout);
        }
    }

    [Theory]
    [InlineData(AvaloniaTestIsolationLevel.PerTest)]
    [InlineData(AvaloniaTestIsolationLevel.PerAssembly)]
    public async Task Disposal_Cancels_Active_And_Queued_Dispatches(AvaloniaTestIsolationLevel isolation)
    {
        var session = HeadlessUnitTestSession.StartNew(typeof(TestApplication), isolation);
        var pending = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Task? disposal = null;
        var invoked = false;
        try
        {
            var active = session.Dispatch(() =>
            {
                started.SetResult();
                return pending.Task;
            }, CancellationToken.None);
            await started.Task.WaitAsync(Timeout);
            var queued = session.Dispatch(() => invoked = true, CancellationToken.None);
            disposal = Task.Run(async () => await session.DisposeAsync());
            await disposal.WaitAsync(Timeout);
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => active.WaitAsync(Timeout));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => queued.WaitAsync(Timeout));
            Assert.False(invoked);
            Assert.Throws<ObjectDisposedException>(() => { _ = session.Dispatch(() => 42, CancellationToken.None); });
        }
        finally
        {
            pending.TrySetResult(0);
            if (disposal is not null)
                await disposal.WaitAsync(Timeout);
            else
                await Task.Run(async () => await session.DisposeAsync()).WaitAsync(Timeout);
        }
    }
    [Fact]
    public async Task Already_Canceled_Dispatch_Does_Not_Invoke_Its_Delegate()
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(TestApplication));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var invoked = false;
        var dispatch = session.Dispatch(() => invoked = true, cancellation.Token);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => dispatch.WaitAsync(Timeout));
        Assert.False(invoked);
        Assert.Equal(42, await session.Dispatch(() => 42, CancellationToken.None).WaitAsync(Timeout));
    }

    [Fact]
    public async Task Completed_Delegate_Wins_A_Racing_Cancellation_And_Leaves_Session_Usable()
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(TestApplication));
        using var cancellation = new CancellationTokenSource();
        var dispatch = session.Dispatch(() =>
        {
            cancellation.Cancel();
            return 42;
        }, cancellation.Token);
        Assert.Equal(42, await dispatch.WaitAsync(Timeout));
        Assert.Equal(43, await session.Dispatch(() => 43, CancellationToken.None).WaitAsync(Timeout));
    }
}

#endif
