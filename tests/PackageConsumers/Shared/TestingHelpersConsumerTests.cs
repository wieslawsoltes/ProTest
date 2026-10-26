using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.HeadlessTestingFramework;
using Avalonia.HeadlessTestingFramework.Appium;
using Avalonia.HeadlessTestingFramework.Recording;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Xunit;

namespace PackageConsumers;

public sealed class TestingHelpersConsumerTests
{
    [AvaloniaFact]
    public void Packaged_Helpers_Find_Controls_And_Deliver_Text_Input()
    {
        var editor = new TextBox { Name = "editor" };
        var window = new Window { Content = editor, Width = 240, Height = 80 };
        try
        {
            window.Show();
            using var driver = new AvaloniaDriver(window);
            driver.FindElement(By.Id("editor")).SendKeys("ProTest helpers");
            Assert.Equal("ProTest helpers", editor.Text);
            Assert.Same(editor, window.Find<TextBox>().FindFirst<TextBox>());
            Assert.Same(editor, new TreeXPath(window).Select<TextBox>("//TextBox[@Name='editor']").Single());
            Assert.Contains("editor", driver.PageSource);
            Assert.Equal("Avalonia.Headless", typeof(AvaloniaDriver).Assembly.GetName().Name);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Packaged_Touch_Helper_Delivers_A_Complete_Drag()
    {
        var target = new Border { Background = Brushes.White };
        var window = new Window { Content = target, Width = 200, Height = 100 };
        var pressed = 0;
        var moved = new List<Point>();
        var released = 0;
        target.PointerPressed += (_, _) => pressed++;
        target.PointerMoved += (_, e) => moved.Add(e.GetPosition(target));
        target.PointerReleased += (_, _) => released++;
        try
        {
            window.Show();
            var touch = new GestureRecognizerTestHelper();
            touch.Drag(target, new Point(10, 10), new Point(90, 50), steps: 4);
            Assert.Equal(1, pressed);
            Assert.Equal(4, moved.Count);
            Assert.Equal(new Point(90, 50), moved[^1]);
            Assert.Equal(1, released);
            Assert.Null(touch.Captured);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Packaged_Recorder_Captures_And_Encodes_Real_Frames()
    {
        var output = Path.Combine(Path.GetTempPath(), "ProTest-consumer-" + Guid.NewGuid().ToString("N"));
        var window = new Window
        {
            Width = 120,
            Height = 80,
            Content = new Border { Background = Brushes.CornflowerBlue }
        };
        try
        {
            window.Show();
            using var recorder = new HeadlessScreenRecorder();
            recorder.StartRecording(window, new RecordingOptions { Format = RecordingFormat.PngSequence }, output);
            Assert.True(recorder.CaptureFrame(), recorder.LastCaptureFailureReason);
            recorder.StopRecording();
            var frames = Directory.GetFiles(output, "*.png");
            Assert.NotEmpty(frames);
            using var decoded = new Bitmap(frames[0]);
            Assert.Equal(new PixelSize(120, 80), decoded.PixelSize);
        }
        finally
        {
            window.Close();
            if (Directory.Exists(output))
                Directory.Delete(output, recursive: true);
        }
    }
}
