using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Xunit;

namespace PackageConsumers;

public sealed class PackageConsumerTests : IAsyncLifetime, IClassFixture<ConsumerFixture>
{
    private readonly int _threadId = Environment.CurrentManagedThreadId;
    private readonly ConsumerFixture _fixture;
    private bool _initialized;

    public PackageConsumerTests(ConsumerFixture fixture)
    {
        Dispatcher.UIThread.VerifyAccess();
        _fixture = fixture;
        Assert.True(fixture.Initialized);
    }

    public async ValueTask InitializeAsync()
    {
        VerifyDispatcher();
        await Task.Yield();
        VerifyDispatcher();
        _initialized = true;
    }

    [AvaloniaFact]
    public async Task Fact_And_Async_Lifecycle_Use_The_Dispatcher()
    {
        Assert.True(_initialized);
        Assert.True(_fixture.Initialized);
        VerifyDispatcher();
        await Task.Delay(1);
        VerifyDispatcher();
        await Dispatcher.UIThread.InvokeAsync(() => Assert.IsType<TestApplication>(Application.Current));
        VerifyDispatcher();
    }

    [AvaloniaTheory]
    [InlineData("first")]
    [InlineData("second")]
    public async Task Enumerated_Theory_Rows_Use_The_Dispatcher(string value)
    {
        Assert.NotEmpty(value);
        await Task.Yield();
        VerifyDispatcher();
    }

    [AvaloniaTheory(DisableDiscoveryEnumeration = true)]
    [MemberData(nameof(DeferredRows))]
    public async Task Deferred_Theory_Rows_Use_The_Dispatcher(ConsumerRow row)
    {
        Assert.InRange(row.Value, 1, 2);
        await Task.Yield();
        VerifyDispatcher();
    }

    public static IEnumerable<object[]> DeferredRows()
    {
        yield return [new ConsumerRow(1)];
        yield return [new ConsumerRow(2)];
    }

    public sealed class ConsumerRow(int value)
    {
        public int Value { get; } = value;
    }

    [AvaloniaFact]
    public void Official_Avalonia_Controls_Receive_Headless_Keyboard_Input()
    {
        var textBox = new TextBox();
        var window = new Window { Content = textBox, Width = 160, Height = 80 };
        try
        {
            window.Show();
            textBox.Focus();
            window.KeyTextInput("NuGet consumer");
            Assert.Equal("NuGet consumer", textBox.Text);
            window.KeyPress(Key.Back, RawInputModifiers.None, PhysicalKey.Backspace, null);
            window.KeyRelease(Key.Back, RawInputModifiers.None, PhysicalKey.Backspace, null);
            Assert.Equal("NuGet consume", textBox.Text);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Official_Skia_Renders_A_Headless_Window()
    {
        var window = new Window
        {
            Width = 100,
            Height = 60,
            Content = new Border { Background = Brushes.CornflowerBlue }
        };
        try
        {
            window.Show();
            using var frame = window.CaptureRenderedFrame();
            Assert.NotNull(frame);
            Assert.Equal(new PixelSize(100, 60), frame.PixelSize);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Original_Assembly_Identity_And_Public_Key_Are_Preserved()
    {
        var headless = typeof(HeadlessUnitTestSession).Assembly.GetName();
        Assert.Equal("Avalonia.Headless", headless.Name);
        // Avalonia's published friend assembly grants require the original strong-name identity.
        Assert.Equal("c8d484a7012f9a8b", Convert.ToHexString(headless.GetPublicKeyToken()!).ToLowerInvariant());
        Assert.Equal("Avalonia.Controls", typeof(Window).Assembly.GetName().Name);

        var adapter = typeof(AvaloniaFactAttribute).Assembly.GetName();
#if XUNIT_V4
        Assert.Equal("Avalonia.Headless.XUnit.v4", adapter.Name);
#else
        Assert.Equal("Avalonia.Headless.XUnit.v3", adapter.Name);
#endif
        Assert.Equal("Avalonia.Headless.XUnit", typeof(AvaloniaFactAttribute).Namespace);
    }

    public async ValueTask DisposeAsync()
    {
        VerifyDispatcher();
        await Task.Yield();
        VerifyDispatcher();
        Assert.True(_initialized);
    }

    private void VerifyDispatcher()
    {
        Dispatcher.UIThread.VerifyAccess();
        Assert.Equal(_threadId, Environment.CurrentManagedThreadId);
        Assert.IsType<AvaloniaSynchronizationContext>(SynchronizationContext.Current);
    }
}

public sealed class ConsumerFixture : IAsyncLifetime
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

public sealed class SkipAndDiscoveryTests
{
    [AvaloniaFact(Skip = "Intentional fact skip used to verify package runner reporting.")]
    public void Skipped_Fact() => Assert.Fail("Skipped fact executed.");

    [AvaloniaTheory(Skip = "Intentional theory skip used to verify package runner reporting.")]
    [InlineData(1)]
    public void Skipped_Theory(int row) => Assert.Fail($"Skipped row executed: {row}");

    [Fact]
    public async Task Normal_Xunit_Test_Is_Discovered()
    {
#if AVALONIA_TEST_FRAMEWORK
        Dispatcher.UIThread.VerifyAccess();
        var threadId = Environment.CurrentManagedThreadId;
        await Task.Yield();
        Dispatcher.UIThread.VerifyAccess();
        Assert.Equal(threadId, Environment.CurrentManagedThreadId);
#else
        await Task.CompletedTask;
#endif
    }

#if AVALONIA_TEST_FRAMEWORK
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Ordinary_Theory_Uses_The_Dispatcher(int row)
    {
        Dispatcher.UIThread.VerifyAccess();
        var threadId = Environment.CurrentManagedThreadId;
        await Task.Yield();
        Dispatcher.UIThread.VerifyAccess();
        Assert.Equal(threadId, Environment.CurrentManagedThreadId);
        Assert.InRange(row, 1, 2);
    }

    [Theory(DisableDiscoveryEnumeration = true)]
    [MemberData(nameof(PackageConsumerTests.DeferredRows), MemberType = typeof(PackageConsumerTests))]
    public async Task Ordinary_Deferred_Theory_Uses_The_Dispatcher(PackageConsumerTests.ConsumerRow row)
    {
        Dispatcher.UIThread.VerifyAccess();
        await Task.Yield();
        Dispatcher.UIThread.VerifyAccess();
        Assert.InRange(row.Value, 1, 2);
    }
#endif
}


public sealed class FailureReportingTests
{
    // CI enables this switch in a separate run and requires two failed tests and a nonzero exit code.
    private static bool ProbeEnabled => Environment.GetEnvironmentVariable("PROTEST_FAILURE_PROBE") == "1";

    [AvaloniaFact]
    public async Task Async_Exceptions_Are_Reported_As_Test_Failures()
    {
        await Task.Yield();
        if (ProbeEnabled)
            throw new InvalidOperationException("PROTEST_EXPECTED_ASYNC_FAILURE");
    }

    [AvaloniaFact]
    public void Dispatcher_Exceptions_Are_Reported_As_Test_Failures()
    {
        if (ProbeEnabled)
            Dispatcher.UIThread.Post(() => throw new InvalidOperationException("PROTEST_EXPECTED_DISPATCHER_FAILURE"));
    }
}
