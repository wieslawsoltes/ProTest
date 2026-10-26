#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace Avalonia.Headless.UnitTests;

/// <summary>Exercises runner behavior that must survive the xUnit 3.x to 4.x API changes.</summary>
public class AdapterRegressionTests : IAsyncLifetime, IClassFixture<AdapterFixture>
{
    private readonly int _threadId;
    private readonly AdapterFixture _fixture;
    private bool _initialized;

    public AdapterRegressionTests(AdapterFixture fixture)
    {
        Dispatcher.UIThread.VerifyAccess();
        _threadId = Environment.CurrentManagedThreadId;
        _fixture = fixture;
        Assert.True(fixture.Initialized);
    }

    public async ValueTask InitializeAsync()
    {
        VerifyContext();
        await Task.Yield();
        VerifyContext();
        _initialized = true;
    }

    [AvaloniaFact]
    public async Task Fact_Await_Resumes_On_Original_Dispatcher()
    {
        Assert.True(_initialized);
        VerifyContext();
        await Task.Delay(1);
        VerifyContext();
        await Dispatcher.UIThread.InvokeAsync(() => Assert.True(_fixture.Initialized));
        VerifyContext();
    }

    [AvaloniaTheory]
    [InlineData(1)]
    [InlineData(2)]
    public async ValueTask Enumerated_Theory_Rows_Run_On_Dispatcher(int value)
    {
        Assert.InRange(value, 1, 2);
        Assert.True(_initialized);
        await Task.Yield();
        VerifyContext();
    }

    [AvaloniaTheory(DisableDiscoveryEnumeration = true)]
    [MemberData(nameof(DeferredRows))]
    public async Task Deferred_Theory_Rows_Run_On_Dispatcher(NonSerializableRow value)
    {
        Assert.InRange(value.Number, 3, 4);
        Assert.True(_initialized);
        VerifyContext();
        await Task.Delay(1);
        VerifyContext();
    }

    public static IEnumerable<object[]> DeferredRows()
    {
        yield return [new NonSerializableRow(3)];
        yield return [new NonSerializableRow(4)];
    }

    public sealed class NonSerializableRow(int number)
    {
        public int Number { get; } = number;
    }

    public async ValueTask DisposeAsync()
    {
        VerifyContext();
        await Task.Yield();
        VerifyContext();
        Assert.True(_initialized);
    }

    private void VerifyContext()
    {
        Dispatcher.UIThread.VerifyAccess();
        Assert.Equal(_threadId, Environment.CurrentManagedThreadId);
        Assert.IsType<AvaloniaSynchronizationContext>(SynchronizationContext.Current);
    }
}

public sealed class AdapterFixture : IAsyncLifetime
{
    public bool Initialized { get; private set; }

    public async ValueTask InitializeAsync()
    {
        await Task.Yield();
        Initialized = true;
    }

    public ValueTask DisposeAsync()
    {
        Assert.True(Initialized);
        return ValueTask.CompletedTask;
    }
}

public class AdapterSkipTests
{
    [AvaloniaFact(Skip = "Verifies fact skip reporting without executing its body.")]
    public void Skipped_Fact_Does_Not_Execute() => Assert.Fail("Skipped fact executed.");

    [AvaloniaTheory(Skip = "Verifies theory skip reporting without executing its body.")]
    [InlineData(1)]
    public void Skipped_Theory_Does_Not_Execute(int value) => Assert.Fail($"Skipped theory executed: {value}.");

    public static bool IsUnavailable => true;

    [AvaloniaFact(Skip = "Conditional skip is intentional.", SkipWhen = nameof(IsUnavailable))]
    public void Conditional_Skip_Does_Not_Execute() => Assert.Fail("Conditionally skipped fact executed.");

    [Fact]
    public void Ordinary_Xunit_Facts_Are_Still_Discovered() => Assert.True(true);
}
