using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Reactive;
using Avalonia.Threading;

namespace Avalonia.Headless;

/// <summary>
/// Headless unit test session that needs to be used by the actual testing framework.
/// All UI tests are supposed to be executed from one of the <see cref="Dispatch"/> methods to keep execution flow on the UI thread.
/// Disposing unit test session stops internal dispatcher loop.
/// </summary>
public sealed class HeadlessUnitTestSession : IDisposable, IAsyncDisposable
{
    private static readonly Dictionary<Assembly, HeadlessUnitTestSession> s_session = new();

    private readonly AppBuilder _appBuilder;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly BlockingCollection<DispatchWorkItem> _queue;
    private readonly object _gate = new();
    private readonly int _dispatchThreadId;
    private bool _shutdownRequested;
    private readonly Task _dispatchTask;
    private readonly bool _isolated;
    // Only set and used with PerAssembly isolation
    private SynchronizationContext? _sharedContext;
    private IDisposable? _sharedApplication;

    internal const DynamicallyAccessedMemberTypes DynamicallyAccessed =
        DynamicallyAccessedMemberTypes.PublicMethods |
        DynamicallyAccessedMemberTypes.NonPublicMethods |
        DynamicallyAccessedMemberTypes.PublicParameterlessConstructor;

    private HeadlessUnitTestSession(
        AppBuilder appBuilder, CancellationTokenSource cancellationTokenSource,
        BlockingCollection<DispatchWorkItem> queue, Task dispatchTask,
        bool isolated)
    {
        _appBuilder = appBuilder;
        _cancellationTokenSource = cancellationTokenSource;
        _queue = queue;
        _dispatchTask = dispatchTask;
        _isolated = isolated;
        _dispatchThreadId = Environment.CurrentManagedThreadId;
    }

    /// <inheritdoc cref="DispatchCore{TResult}"/>
    public Task Dispatch(Action action, CancellationToken cancellationToken)
    {
        return DispatchCore(() =>
        {
            action();
            return Task.FromResult(0);
        }, true ,cancellationToken);
    }

    /// <inheritdoc cref="DispatchCore{TResult}"/>
    public Task<TResult> Dispatch<TResult>(Func<TResult> action, CancellationToken cancellationToken)
    {
        return DispatchCore(() => Task.FromResult(action()), true, cancellationToken);
    }

    /// <inheritdoc cref="DispatchCore{TResult}"/>
    public Task<TResult> Dispatch<TResult>(Func<Task<TResult>> action, CancellationToken cancellationToken)
    {
        return DispatchCore(action, true, cancellationToken);
    }

    /// <summary>
    /// Dispatch method queues an async operation on the dispatcher thread, creates a new application instance,
    /// setting app avalonia services, and runs <paramref name="action"/> parameter.
    /// </summary>
    /// <param name="action">Action to execute on the dispatcher thread with avalonia services.</param>
    /// <param name="captureExecutionContext">Whether dispatch should capture ExecutionContext.</param>
    /// <param name="cancellationToken">Cancellation token to cancel execution.</param>
    /// <remarks>
    /// Cancellation before execution cancels only the queued dispatch. If cancellation
    /// interrupts an incomplete asynchronous delegate, the session is stopped and its
    /// dispatcher is shut down so stale UI continuations cannot enter another test.
    /// Subsequent dispatches throw <see cref="ObjectDisposedException"/>. A delegate
    /// which has already completed retains its result even if its token was canceled.
    /// Cancellation cannot interrupt synchronous user code that blocks the dispatcher.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">
    /// If global session was already cancelled and thread killed, it's not possible to dispatch any actions again
    /// </exception>
    internal Task<TResult> DispatchCore<TResult>(Func<Task<TResult>> action, bool captureExecutionContext, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(action);

        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_shutdownRequested, this);

            var work = new DispatchWorkItem<TResult>(this, action, cancellationToken,
                captureExecutionContext ? ExecutionContext.Capture() : null);
            _queue.Add(work);
            return work.Task;
        }
    }

    private void Execute<TResult>(Func<Task<TResult>> action, CancellationToken cancellationToken,
        TaskCompletionSource<TResult> completion)
    {
        IDisposable? application = null;
        Exception? caught = null;
        CancellationToken? canceled = null;
        TResult result = default!;
        Task<TResult>? task = null;
        Dispatcher? dispatcher = null;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            application = _isolated ? EnsureIsolatedApplication() : EnsureSharedApplication();
            cancellationToken.ThrowIfCancellationRequested();

            dispatcher = Dispatcher.UIThread;
            task = action() ?? throw new InvalidOperationException("The dispatched delegate returned a null task.");
            if (!task.IsCompleted)
            {
                var frame = new DispatcherFrame();
                // Neither completion nor cancellation may depend on the UI context: the
                // dispatcher loop must be able to stop when the user task never completes.
                using var registration = cancellationToken.Register(
                    static state => ((DispatcherFrame)state!).Continue = false, frame);
                _ = task.ContinueWith(static (completed, state) =>
                {
                    // Observe faults even when cancellation has already abandoned this task.
                    _ = completed.Exception;
                    ((DispatcherFrame)state!).Continue = false;
                }, frame, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

                if (!task.IsCompleted && !cancellationToken.IsCancellationRequested)
                    dispatcher.PushFrame(frame);

                if (!task.IsCompleted)
                {
                    // An incomplete task may retain this dispatcher and its application.
                    // Terminate the session before exposing cancellation to the caller.
                    RequestShutdown();
                    dispatcher.InvokeShutdown();
                    cancellationToken.ThrowIfCancellationRequested();
                    throw new InvalidOperationException("The dispatcher stopped before the dispatched task completed.");
                }
            }

            // Synchronously completed delegates win a concurrent cancellation. Never
            // synchronously wait on an incomplete task after leaving the dispatcher loop.
            result = task.GetAwaiter().GetResult();
        }
        catch (OperationCanceledException exception)
        {
            canceled = exception.CancellationToken;
        }
        catch (Exception exception)
        {
            caught = exception;
        }
        finally
        {
            if (task is { IsCompleted: false })
            {
                RequestShutdown();
                try
                {
                    dispatcher?.InvokeShutdown();
                }
                catch (Exception exception)
                {
                    caught = caught is null ? exception : new AggregateException(caught, exception);
                }
            }
            try
            {
                application?.Dispose();
            }
            catch (Exception exception)
            {
                caught = caught is null ? exception : new AggregateException(caught, exception);
            }

            if (Volatile.Read(ref _shutdownRequested))
            {
                try
                {
                    ReleaseSharedApplication();
                }
                catch (Exception exception)
                {
                    caught = caught is null ? exception : new AggregateException(caught, exception);
                }
                // No application state remains to clean up after publishing the result.
                // A caller may safely request a replacement cached session at that point.
                RemoveCachedSession();
            }
        }

        if (caught is not null)
            completion.TrySetException(caught);
        else if (canceled is { } token)
            completion.TrySetCanceled(token);
        else
            completion.TrySetResult(result);
    }

    private abstract class DispatchWorkItem(ExecutionContext? executionContext)
    {
        public ExecutionContext? ExecutionContext { get; } = executionContext;
        public abstract void Execute();
        public abstract void Cancel();
    }

    private sealed class DispatchWorkItem<TResult> : DispatchWorkItem
    {
        private readonly HeadlessUnitTestSession _session;
        private readonly Func<Task<TResult>> _action;
        private readonly CancellationTokenSource _cancellation;
        private readonly CancellationTokenRegistration _registration;
        private readonly TaskCompletionSource<TResult> _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        // 0: queued; 1: running; 2: canceled while queued or finished.
        private int _state;

        public DispatchWorkItem(HeadlessUnitTestSession session, Func<Task<TResult>> action,
            CancellationToken cancellationToken, ExecutionContext? executionContext) : base(executionContext)
        {
            _session = session;
            _action = action;
            _cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, session._cancellationTokenSource.Token);
            _registration = _cancellation.Token.Register(() =>
            {
                if (Interlocked.CompareExchange(ref _state, 2, 0) == 0)
                    _completion.TrySetCanceled(_cancellation.Token);
            });
        }

        public Task<TResult> Task => _completion.Task;

        public override void Execute()
        {
            try
            {
                if (Interlocked.CompareExchange(ref _state, 1, 0) == 0)
                    _session.Execute(_action, _cancellation.Token, _completion);
            }
            catch (Exception exception)
            {
                _completion.TrySetException(exception);
            }
            finally
            {
                Interlocked.Exchange(ref _state, 2);
                _registration.Dispose();
                _cancellation.Dispose();
            }
        }

        public override void Cancel()
        {
            _completion.TrySetCanceled();
            Interlocked.Exchange(ref _state, 2);
            _registration.Dispose();
            _cancellation.Dispose();
        }
    }

    private IDisposable EnsureSharedApplication()
    {
        var oldContext = SynchronizationContext.Current;
        if (_sharedApplication is null)
        {
            // Hold one application scope for the entire session, then release it on
            // the dispatcher thread when the session ends.
            _sharedApplication = EnsureIsolatedApplication();
            _sharedContext = SynchronizationContext.Current;
        }
        else
        {
            SynchronizationContext.SetSynchronizationContext(_sharedContext);
        }

        return Disposable.Create(() =>
        {
            try
            {
                Dispatcher.UIThread.RunJobs();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(oldContext);
            }
        });
    }

    private IDisposable EnsureIsolatedApplication()
    {
        var scope = AvaloniaLocator.EnterScope();
        var oldContext = SynchronizationContext.Current;
        try
        {
            Dispatcher.ResetBeforeUnitTests();
            _appBuilder.SetupUnsafe();
        }
        catch
        {
            scope.Dispose();
            throw;
        }

        return Disposable.Create(() =>
        {
            try
            {
                ((ToolTipService?)AvaloniaLocator.Current.GetService<IToolTipService>())?.Dispose();
                (AvaloniaLocator.Current.GetService<FontManager>() as IDisposable)?.Dispose();
                Dispatcher.ResetForUnitTests();
            }
            finally
            {
                // Cleanup jobs can throw, but the ambient state still belongs to this dispatch.
                try
                {
                    scope.Dispose();
                }
                finally
                {
                    Dispatcher.ResetBeforeUnitTests();
                    SynchronizationContext.SetSynchronizationContext(oldContext);
                }
            }
        });
    }

    /// <summary>
    /// Stops the session, cancels queued and active dispatches, and waits for dispatcher cleanup.
    /// Repeated calls are safe. Disposal must be initiated outside the session's dispatcher thread.
    /// </summary>
    public void Dispose()
    {
        VerifyDisposalThread();
        RequestShutdown();
        _dispatchTask.GetAwaiter().GetResult();
    }

    /// <inheritdoc cref="Dispose"/>
    public ValueTask DisposeAsync()
    {
        VerifyDisposalThread();
        RequestShutdown();
        return new ValueTask(_dispatchTask);
    }

    private void VerifyDisposalThread()
    {
        if (Environment.CurrentManagedThreadId == _dispatchThreadId && !_dispatchTask.IsCompleted)
            throw new InvalidOperationException("A headless session cannot be disposed from its own dispatcher thread.");
    }

    private void RequestShutdown()
    {
        lock (_gate)
        {
            if (_shutdownRequested)
                return;

            _shutdownRequested = true;
            _queue.CompleteAdding();
            _cancellationTokenSource.Cancel();
        }
    }

    private void RunQueue()
    {
        try
        {
            foreach (var work in _queue.GetConsumingEnumerable())
            {
                try
                {
                    if (work.ExecutionContext is { } context)
                        ExecutionContext.Run(context, state => ((DispatchWorkItem)state!).Execute(), work);
                    else
                        work.Execute();
                }
                catch
                {
                    work.Cancel();
                    throw;
                }
            }
        }
        finally
        {
            RequestShutdown();
            while (_queue.TryTake(out var pending))
                pending.Cancel();

            try
            {
                ReleaseSharedApplication();
            }
            finally
            {
                RemoveCachedSession();
                _queue.Dispose();
                _cancellationTokenSource.Dispose();
            }
        }
    }

    private void ReleaseSharedApplication()
    {
        var application = _sharedApplication;
        _sharedApplication = null;
        application?.Dispose();
    }

    private void RemoveCachedSession()
    {
        lock (s_session)
        {
            Assembly? key = null;
            foreach (var pair in s_session)
                if (ReferenceEquals(pair.Value, this))
                {
                    key = pair.Key;
                    break;
                }
            if (key is not null)
                s_session.Remove(key);
        }
    }

    /// <summary>
    /// Creates instance of <see cref="HeadlessUnitTestSession"/>.
    /// </summary>
    /// <param name="entryPointType">
    /// Parameter from which <see cref="AppBuilder"/> should be created.
    /// It either needs to have BuildAvaloniaApp -> AppBuilder method or inherit Application.
    /// </param>
    public static HeadlessUnitTestSession StartNew(
        [DynamicallyAccessedMembers(DynamicallyAccessed)]
        Type entryPointType)
    {
        // Cannot be optional parameter for ABI stability
        // ReSharper disable once IntroduceOptionalParameters.Global
        return StartNew(entryPointType, AvaloniaTestIsolationLevel.PerTest);
    }

    /// <summary>
    /// Creates instance of <see cref="HeadlessUnitTestSession"/>.
    /// </summary>
    /// <param name="entryPointType">
    /// Parameter from which <see cref="AppBuilder"/> should be created.
    /// It either needs to have BuildAvaloniaApp -> AppBuilder method or inherit Application.
    /// </param>
    /// <param name="isolationLevel">Defines the isolation level for headless unit tests</param>
    public static HeadlessUnitTestSession StartNew(
        [DynamicallyAccessedMembers(DynamicallyAccessed)]
        Type entryPointType,
        AvaloniaTestIsolationLevel isolationLevel)
    {
        var tcs = new TaskCompletionSource<HeadlessUnitTestSession>(TaskCreationOptions.RunContinuationsAsynchronously);
        var cancellationTokenSource = new CancellationTokenSource();
        var queue = new BlockingCollection<DispatchWorkItem>();

        Task? task = null;
        task = new Task(() =>
        {
            HeadlessUnitTestSession session;
            try
            {
                var appBuilder = AppBuilder.Configure(entryPointType);
                var runIsolated = isolationLevel == AvaloniaTestIsolationLevel.PerTest;

                // If windowing subsystem wasn't initialized by user, force headless with default parameters.
                if (appBuilder.WindowingSubsystemName != "Headless")
                {
                    appBuilder = appBuilder.UseHeadless(new AvaloniaHeadlessPlatformOptions());
                }

                if (appBuilder.TextShapingSubsystemInitializer is null)
                {
                    appBuilder = appBuilder.UseHarfBuzz();
                }

                // ReSharper disable once AccessToModifiedClosure
                session = new HeadlessUnitTestSession(appBuilder, cancellationTokenSource, queue, task!, runIsolated);
                tcs.SetResult(session);
            }
            catch (Exception e)
            {
                queue.Dispose();
                cancellationTokenSource.Dispose();
                tcs.SetException(e);
                return;
            }

            session.RunQueue();
        }, TaskCreationOptions.DenyChildAttach | TaskCreationOptions.LongRunning);

        task.Start(TaskScheduler.Default);
        return tcs.Task.GetAwaiter().GetResult();
    }

    /// <summary>
    /// Creates a session from AvaloniaTestApplicationAttribute attribute or reuses any existing.
    /// If AvaloniaTestApplicationAttribute doesn't exist, empty application is used.
    /// </summary>
    [UnconditionalSuppressMessage("Trimming", "IL2072",
        Justification = "AvaloniaTestApplicationAttribute attribute should preserve type information.")]
    public static HeadlessUnitTestSession GetOrStartForAssembly(Assembly? assembly)
    {
        assembly ??= typeof(HeadlessUnitTestSession).Assembly;

        while (true)
        {
            HeadlessUnitTestSession? stopping;
            lock (s_session)
            {
                if (!s_session.TryGetValue(assembly, out var session))
                {
                    var appBuilderEntryPointType = assembly.GetCustomAttribute<AvaloniaTestApplicationAttribute>()
                        ?.AppBuilderEntryPointType;

                    var isolationLevel = assembly.GetCustomAttribute<AvaloniaTestIsolationAttribute>()
                        ?.IsolationLevel ?? AvaloniaTestIsolationLevel.PerTest;

                    session = appBuilderEntryPointType is not null ?
                        StartNew(appBuilderEntryPointType, isolationLevel) :
                        StartNew(typeof(Application), isolationLevel);

                    s_session.Add(assembly, session);
                }

                if (!Volatile.Read(ref session._shutdownRequested))
                    return session;

                stopping = session;
            }

            // Shutdown removes the cache entry after application cleanup. Do not wait
            // while holding the cache lock, which the dispatcher also needs to remove it.
            stopping.VerifyDisposalThread();
            stopping._dispatchTask.GetAwaiter().GetResult();
        }
    }
}
