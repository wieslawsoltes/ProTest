using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Rendering.Composition;

namespace Avalonia.Headless.UnitTests;

public class RenderingTests
{
#if NUNIT
    [Test]
#elif XUNIT
    [Fact]
#endif
    public async Task Headless_Stream_Geometry_Should_Append_Without_Mutating_Clones()
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(Application));
#if XUNIT
        var cancellationToken = TestContext.Current.CancellationToken;
#else
        var cancellationToken = System.Threading.CancellationToken.None;
#endif
        await session.Dispatch(() =>
        {
            var geometry = StreamGeometry.Parse("M0,0 L10,0 0,10 Z");
            var clone = geometry.Clone();

            using (var context = geometry.Open())
            {
                context.BeginFigure(new Point(20, 0));
                context.LineTo(new Point(30, 0));
                context.LineTo(new Point(20, 10));
                context.EndFigure(true);
            }

            AssertHelper.Equal(new Rect(0, 0, 30, 10), geometry.Bounds);
            AssertHelper.Equal(new Rect(0, 0, 10, 10), clone.Bounds);
            AssertHelper.True(geometry.FillContains(new Point(2, 2)));
            AssertHelper.True(geometry.FillContains(new Point(22, 2)));
            AssertHelper.False(geometry.FillContains(new Point(15, 2)));
            AssertHelper.False(clone.FillContains(new Point(22, 2)));

            var probe = new RectangleGeometry(new Rect(21, 1, 1, 1));
            AssertHelper.Equal<IntersectionResult?>(IntersectionResult.FullyInside, geometry.GetFillIntersectionResult(probe));
            AssertHelper.Equal<IntersectionResult?>(IntersectionResult.Empty, clone.GetFillIntersectionResult(probe));
        }, cancellationToken);
    }

#if NUNIT
    [AvaloniaTest]
#elif XUNIT
    [AvaloniaFact]
#endif
    public void Should_Render_Last_Frame_To_Bitmap()
    {
        var window = new Window
        {
            Content = new ContentControl
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Padding = new Thickness(4),
                Content = new PathIcon
                {
                    Data = StreamGeometry.Parse("M0,9 L10,0 20,9 19,10 10,2 1,10 z")
                }
            },
            SizeToContent = SizeToContent.WidthAndHeight
        };

        window.Show();

        var frame = window.CaptureRenderedFrame();

        AssertHelper.NotNull(frame);
    }

#if NUNIT
    [AvaloniaTest]
#elif XUNIT
    [AvaloniaFact]
#endif
    public void Should_Not_Crash_On_GeometryGroup()
    {
        var window = new Window
        {
            Content = new ContentControl
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Padding = new Thickness(4),
                Content = new PathIcon
                {
                    Data = new GeometryGroup()
                    {
                        Children = new GeometryCollection(new []
                        {
                            new RectangleGeometry(new Rect(0, 0, 50, 50)),
                            new RectangleGeometry(new Rect(50, 50, 100, 100))
                        })
                    }
                }
            },
            SizeToContent = SizeToContent.WidthAndHeight
        };

        window.Show();

        var frame = window.CaptureRenderedFrame();

        AssertHelper.NotNull(frame);
    }
    
#if NUNIT
    [AvaloniaTest]
#elif XUNIT
    [AvaloniaFact]
#endif
    public void Should_Not_Crash_On_CombinedGeometry()
    {
        var window = new Window
        {
            Content = new ContentControl
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Padding = new Thickness(4),
                Content = new PathIcon
                {
                    Data = new CombinedGeometry(GeometryCombineMode.Union,
                        new RectangleGeometry(new Rect(0, 0, 50, 50)),
                        new RectangleGeometry(new Rect(50, 50, 100, 100)))
                }
            },
            SizeToContent = SizeToContent.WidthAndHeight
        };

        window.Show();

        var frame = window.CaptureRenderedFrame();

        AssertHelper.NotNull(frame);
    }

#if NUNIT
    [AvaloniaTest]
#elif XUNIT
    [AvaloniaFact]
#endif
    public void Should_Not_Hang_With_Non_Trivial_Layout()
    {
        var window = new Window
        {
            Content = new ContentControl
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Padding = new Thickness(1),
                Content = new ListBox
                {
                    ItemsSource = new ObservableCollection<string>()
                    {
                        "Test 1",
                        "Test 2"
                    }
                }
            },
            SizeToContent = SizeToContent.WidthAndHeight
        };

        window.Show();

        var frame = window.CaptureRenderedFrame();
        AssertHelper.NotNull(frame);
    }

#if NUNIT
    [AvaloniaTest]
#elif XUNIT
    [AvaloniaFact]
#endif
    public async Task Should_Render_To_A_Compositor_Snapshot_Capture()
    {
        var window = new Window
        {
            Content = new ContentControl
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Width = 100,
                Height = 100,
                Background = Brushes.Green
            },
            SizeToContent = SizeToContent.WidthAndHeight
        };

        window.Show();
        try
        {
            // A dispatcher drain alone can precede the first render timer tick.
            // Render a frame so the visual is attached to its composition target
            // before requesting a snapshot (required by Avalonia 12.1.4).
            using var initialFrame = window.CaptureRenderedFrame();
            AssertHelper.NotNull(initialFrame);

            var compositionVisual = ElementComposition.GetElementVisual(window)!;
            using var snapshot = await compositionVisual.Compositor.CreateCompositionVisualSnapshot(compositionVisual, 1);

            AssertHelper.NotNull(snapshot);
            AssertHelper.Equal(100, snapshot.Size.Width);
            AssertHelper.Equal(100, snapshot.Size.Height);
        }
        finally
        {
            window.Close();
        }
    }

#if NUNIT
    [AvaloniaTest]
#elif XUNIT
    [AvaloniaFact]
#endif
    public void Should_Change_Render_Scaling()
    {
        var window = new Window
        {
            Content = new Border
            {
                Background = Brushes.Red
            },
            Width = 100,
            Height = 100,
        };

        window.Show();

        var frameBefore = window.CaptureRenderedFrame();
        AssertHelper.NotNull(frameBefore);

        var sizeBefore = frameBefore!.PixelSize;

        window.SetRenderScaling(2.0);

        AssertHelper.Equal(2.0, window.RenderScaling);

        var frameAfter = window.CaptureRenderedFrame();
        AssertHelper.NotNull(frameAfter);

        var sizeAfter = frameAfter!.PixelSize;

        AssertHelper.Equal(sizeBefore.Width * 2, sizeAfter.Width);
        AssertHelper.Equal(sizeBefore.Height * 2, sizeAfter.Height);
    }

#if NUNIT
    [AvaloniaTest]
#elif XUNIT
    [AvaloniaFact]
#endif
    public void Should_Keep_Client_Size_After_Scaling_Change()
    {
        var window = new Window
        {
            Width = 200,
            Height = 150
        };

        window.Show();
        window.CaptureRenderedFrame();

        var clientSizeBefore = window.ClientSize;

        window.SetRenderScaling(2.0);
        window.CaptureRenderedFrame();

        AssertHelper.Equal(clientSizeBefore.Width, window.ClientSize.Width);
        AssertHelper.Equal(clientSizeBefore.Height, window.ClientSize.Height);
    }
}
