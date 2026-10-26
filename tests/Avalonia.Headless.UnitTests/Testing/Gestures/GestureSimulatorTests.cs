#nullable enable

// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See licence.md and NOTICE.md in the repository root for details.
using System.Collections.Generic;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.HeadlessTestingFramework;
using Xunit;

namespace Avalonia.Headless.UnitTests.Testing.Gestures;

/// <summary>
/// Comprehensive tests for the GestureSimulator class.
/// Tests all gesture types including tap, double-tap, right-tap, holding,
/// pinch, scroll, pull-to-refresh, and touchpad gestures.
/// </summary>
public class GestureSimulatorTests
{
    #region Tap Gesture Tests

    [AvaloniaFact]
    public void Tapped_RaisesEvent_OnTarget()
    {
        // Arrange
        var target = new Border { Width = 100, Height = 100, Background = Brushes.Red };
        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();
        var eventRaised = false;
        var eventPosition = default(Point);

        target.AddHandler(InputElement.TappedEvent, (sender, args) =>
        {
            eventRaised = true;
            if (args is TappedEventArgs tapped)
            {
                eventPosition = tapped.GetPosition(target);
            }
        });

        // Act
        simulator.Tap(target, new Point(50, 50));

        // Assert
        Assert.True(eventRaised, "Tapped event should be raised");
    }

    [AvaloniaFact]
    public void DoubleTapped_RaisesEvent_OnTarget()
    {
        // Arrange
        var target = new Border { Width = 100, Height = 100, Background = Brushes.Red };
        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();
        var eventRaised = false;

        target.AddHandler(InputElement.DoubleTappedEvent, (sender, args) =>
        {
            eventRaised = true;
        });

        // Act
        simulator.DoubleTap(target, new Point(50, 50));

        // Assert
        Assert.True(eventRaised, "DoubleTapped event should be raised");
    }

    [AvaloniaFact]
    public void RightTapped_RaisesEvent_OnTarget()
    {
        // Arrange
        var target = new Border { Width = 100, Height = 100, Background = Brushes.Red };
        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();
        var eventRaised = false;

        target.AddHandler(InputElement.RightTappedEvent, (sender, args) =>
        {
            eventRaised = true;
        });

        // Act
        simulator.RightTap(target, new Point(50, 50));

        // Assert
        Assert.True(eventRaised, "RightTapped event should be raised");
    }

    [AvaloniaFact]
    public void Tapped_WithDifferentPointerTypes_WorksCorrectly()
    {
        // Arrange
        var target = new Border { Width = 100, Height = 100, Background = Brushes.Red };
        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();
        var pointerTypes = new List<PointerType>();

        target.AddHandler(InputElement.TappedEvent, (sender, args) =>
        {
            if (args is TappedEventArgs tapped)
            {
                pointerTypes.Add(tapped.Pointer?.Type ?? PointerType.Mouse);
            }
        });

        // Act
        simulator.Tap(target, new Point(50, 50), PointerType.Touch);
        simulator.Tap(target, new Point(50, 50), PointerType.Mouse);
        simulator.Tap(target, new Point(50, 50), PointerType.Pen);

        // Assert
        Assert.Equal(3, pointerTypes.Count);
    }

    #endregion

    #region Holding Gesture Tests

    [AvaloniaFact]
    public void HoldingStarted_RaisesEvent_WithCorrectState()
    {
        // Arrange
        var target = new Border { Width = 100, Height = 100, Background = Brushes.Red };
        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();
        HoldingState? capturedState = null;
        Point? capturedPosition = null;

        target.AddHandler(InputElement.HoldingEvent, (sender, args) =>
        {
            if (args is HoldingRoutedEventArgs holding)
            {
                capturedState = holding.HoldingState;
                capturedPosition = holding.Position;
            }
        });

        // Act
        simulator.HoldingStarted(target, new Point(25, 25));

        // Assert
        Assert.NotNull(capturedState);
        Assert.Equal(HoldingState.Started, capturedState);
        Assert.Equal(new Point(25, 25), capturedPosition);
    }

    [AvaloniaFact]
    public void HoldingStarted_UsesPressedPointerEventArgs()
    {
        var target = new Border { Width = 100, Height = 100, Background = Brushes.Red };
        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();
        HoldingRoutedEventArgs? capturedHoldingArgs = null;

        target.AddHandler(InputElement.HoldingEvent, (_, args) =>
        {
            if (args is HoldingRoutedEventArgs holding)
            {
                capturedHoldingArgs = holding;
            }
        });

        simulator.HoldingStarted(target, new Point(25, 25));

        Assert.NotNull(capturedHoldingArgs);

        var pointerEventArgsProperty = typeof(HoldingRoutedEventArgs).GetProperty(
            "PointerEventArgs",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        Assert.NotNull(pointerEventArgsProperty);

        var capturedPointerEventArgs = pointerEventArgsProperty!.GetValue(capturedHoldingArgs) as PointerEventArgs;
        Assert.NotNull(capturedPointerEventArgs);
        Assert.IsType<PointerPressedEventArgs>(capturedPointerEventArgs);
        Assert.Equal(InputElement.PointerPressedEvent, capturedPointerEventArgs!.RoutedEvent);
    }

    [AvaloniaFact]
    public void HoldingCompleted_RaisesEvent_WithCorrectState()
    {
        // Arrange
        var target = new Border { Width = 100, Height = 100, Background = Brushes.Red };
        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();
        HoldingState? capturedState = null;

        target.AddHandler(InputElement.HoldingEvent, (sender, args) =>
        {
            if (args is HoldingRoutedEventArgs holding)
            {
                capturedState = holding.HoldingState;
            }
        });

        // Act
        simulator.HoldingCompleted(target, new Point(50, 50));

        // Assert
        Assert.NotNull(capturedState);
        Assert.Equal(HoldingState.Completed, capturedState);
    }

    [AvaloniaFact]
    public void HoldingCompleted_UsesReleasedPointerEventArgs()
    {
        var target = new Border { Width = 100, Height = 100, Background = Brushes.Red };
        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();
        HoldingRoutedEventArgs? capturedHoldingArgs = null;

        target.AddHandler(InputElement.HoldingEvent, (_, args) =>
        {
            if (args is HoldingRoutedEventArgs holding)
            {
                capturedHoldingArgs = holding;
            }
        });

        simulator.HoldingCompleted(target, new Point(50, 50));

        Assert.NotNull(capturedHoldingArgs);

        var pointerEventArgsProperty = typeof(HoldingRoutedEventArgs).GetProperty(
            "PointerEventArgs",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        Assert.NotNull(pointerEventArgsProperty);

        var capturedPointerEventArgs = pointerEventArgsProperty!.GetValue(capturedHoldingArgs) as PointerEventArgs;
        Assert.NotNull(capturedPointerEventArgs);
        Assert.IsType<PointerReleasedEventArgs>(capturedPointerEventArgs);
        Assert.Equal(InputElement.PointerReleasedEvent, capturedPointerEventArgs!.RoutedEvent);
    }

    [AvaloniaFact]
    public void HoldingStarted_UsesPresentationRootCoordinates_ForAncestorLookups()
    {
        var target = new Border { Width = 100, Height = 100, Background = Brushes.Red };
        Canvas.SetLeft(target, 40);
        Canvas.SetTop(target, 30);

        var canvas = new Canvas { Width = 200, Height = 200 };
        canvas.Children.Add(target);

        var window = new Window
        {
            Width = 300,
            Height = 300,
            Content = canvas
        };
        window.Show();

        var simulator = new GestureSimulator();
        PointerEventArgs? capturedPointerEventArgs = null;

        target.AddHandler(InputElement.HoldingEvent, (_, args) =>
        {
            if (args is HoldingRoutedEventArgs holding)
            {
                capturedPointerEventArgs = GetHoldingPointerEventArgs(holding);
            }
        });

        simulator.HoldingStarted(target, new Point(25, 25));

        Assert.NotNull(capturedPointerEventArgs);
        var expectedCanvasPosition = target.TransformToVisual(canvas)!.Value.Transform(new Point(25, 25));
        var expectedWindowPosition = target.TransformToVisual(window)!.Value.Transform(new Point(25, 25));

        Assert.Equal(expectedCanvasPosition, capturedPointerEventArgs!.GetPosition(canvas));
        Assert.Equal(expectedWindowPosition, capturedPointerEventArgs.GetPosition(window));
    }

    [AvaloniaFact]
    public void HoldingCancelled_RaisesEvent_WithCorrectState()
    {
        // Arrange
        var target = new Border { Width = 100, Height = 100, Background = Brushes.Red };
        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();
        HoldingState? capturedState = null;

        target.AddHandler(InputElement.HoldingEvent, (sender, args) =>
        {
            if (args is HoldingRoutedEventArgs holding)
            {
                capturedState = holding.HoldingState;
            }
        });

        // Act
        simulator.HoldingCancelled(target, new Point(50, 50));

        // Assert
        Assert.NotNull(capturedState);
        Assert.Equal(HoldingState.Canceled, capturedState);
    }

    [AvaloniaFact]
    public void Hold_RaisesStartedAndCompletedEvents()
    {
        // Arrange
        var target = new Border { Width = 100, Height = 100, Background = Brushes.Red };
        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();
        var states = new List<HoldingState>();

        target.AddHandler(InputElement.HoldingEvent, (sender, args) =>
        {
            if (args is HoldingRoutedEventArgs holding)
            {
                states.Add(holding.HoldingState);
            }
        });

        // Act
        simulator.Hold(target, new Point(50, 50), holdDuration: 500);

        // Assert
        Assert.Equal(2, states.Count);
        Assert.Equal(HoldingState.Started, states[0]);
        Assert.Equal(HoldingState.Completed, states[1]);
    }

    [AvaloniaFact]
    public void Hold_ReusesTheSamePointerId_AcrossLifecycleEvents()
    {
        var target = new Border { Width = 100, Height = 100, Background = Brushes.Red };
        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();
        var pointerIds = new List<int>();

        target.AddHandler(InputElement.HoldingEvent, (_, args) =>
        {
            if (args is HoldingRoutedEventArgs holding)
            {
                var pointerEventArgs = GetHoldingPointerEventArgs(holding);
                pointerIds.Add(pointerEventArgs.Pointer.Id);
            }
        });

        simulator.Hold(target, new Point(50, 50), holdDuration: 500);

        Assert.Equal(2, pointerIds.Count);
        Assert.Equal(pointerIds[0], pointerIds[1]);
    }

    #endregion

    private static PointerEventArgs GetHoldingPointerEventArgs(HoldingRoutedEventArgs args)
    {
        var pointerEventArgsProperty = typeof(HoldingRoutedEventArgs).GetProperty(
            "PointerEventArgs",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        Assert.NotNull(pointerEventArgsProperty);

        var pointerEventArgs = pointerEventArgsProperty!.GetValue(args) as PointerEventArgs;
        Assert.NotNull(pointerEventArgs);

        return pointerEventArgs!;
    }

    #region Pinch Gesture Tests

    [AvaloniaFact]
    public void Pinch_RaisesEvent_WithCorrectScaleAndOrigin()
    {
        // Arrange
        var target = new Border { Width = 100, Height = 100, Background = Brushes.Red };
        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();
        double? capturedScale = null;
        Point? capturedOrigin = null;

        target.AddHandler(InputElement.PinchEvent, (sender, args) =>
        {
            if (args is PinchEventArgs pinch)
            {
                capturedScale = pinch.Scale;
                capturedOrigin = pinch.ScaleOrigin;
            }
        });

        // Act
        simulator.Pinch(target, scale: 2.0, scaleOrigin: new Point(50, 50));

        // Assert
        Assert.NotNull(capturedScale);
        Assert.NotNull(capturedOrigin);
        Assert.Equal(2.0, capturedScale);
        Assert.Equal(new Point(50, 50), capturedOrigin);
    }

    [AvaloniaFact]
    public void PinchEnded_RaisesEvent()
    {
        // Arrange
        var target = new Border { Width = 100, Height = 100, Background = Brushes.Red };
        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();
        var eventRaised = false;

        target.AddHandler(InputElement.PinchEndedEvent, (sender, args) =>
        {
            eventRaised = true;
        });

        // Act
        simulator.PinchEnded(target);

        // Assert
        Assert.True(eventRaised);
    }

    [AvaloniaFact]
    public void PinchZoom_RaisesMultiplePinchEvents_ThenEnded()
    {
        // Arrange
        var target = new Border { Width = 100, Height = 100, Background = Brushes.Red };
        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();
        var pinchCount = 0;
        var pinchEndedRaised = false;
        var lastScale = 0.0;

        target.AddHandler(InputElement.PinchEvent, (sender, args) =>
        {
            if (args is PinchEventArgs pinch)
            {
                pinchCount++;
                lastScale = pinch.Scale;
            }
        });

        target.AddHandler(InputElement.PinchEndedEvent, (sender, args) =>
        {
            pinchEndedRaised = true;
        });

        // Act
        simulator.PinchZoom(target, origin: new Point(50, 50), startScale: 1.0, endScale: 2.0, steps: 5);

        // Assert
        Assert.True(pinchCount >= 5, $"Expected at least 5 pinch events, got {pinchCount}");
        Assert.True(pinchEndedRaised, "PinchEnded event should be raised");
        Assert.Equal(2.0, lastScale, 2);
    }

    [AvaloniaFact]
    public void PinchRotate_RaisesMultiplePinchEvents()
    {
        // Arrange
        var target = new Border { Width = 100, Height = 100, Background = Brushes.Red };
        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();
        var pinchCount = 0;

        target.AddHandler(InputElement.PinchEvent, (sender, args) =>
        {
            pinchCount++;
        });

        // Act
        simulator.PinchRotate(target, origin: new Point(50, 50), startAngle: 0, endAngle: 90, steps: 10);

        // Assert
        Assert.True(pinchCount >= 10, $"Expected at least 10 pinch events, got {pinchCount}");
    }

    #endregion

    #region Scroll Gesture Tests

    [AvaloniaFact]
    public void Scroll_RaisesEvent_WithCorrectDelta()
    {
        // Arrange
        var target = new Border { Width = 100, Height = 100, Background = Brushes.Red };
        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();
        Vector? capturedDelta = null;
        int? capturedId = null;

        target.AddHandler(InputElement.ScrollGestureEvent, (sender, args) =>
        {
            if (args is ScrollGestureEventArgs scroll)
            {
                capturedDelta = scroll.Delta;
                capturedId = scroll.Id;
            }
        });

        // Act
        simulator.Scroll(target, new Vector(0, 100));

        // Assert
        Assert.NotNull(capturedDelta);
        Assert.NotNull(capturedId);
        Assert.Equal(new Vector(0, 100), capturedDelta);
    }

    [AvaloniaFact]
    public void ScrollEnded_RaisesEvent_WithCorrectGestureId()
    {
        // Arrange
        var target = new Border { Width = 100, Height = 100, Background = Brushes.Red };
        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();
        int? capturedId = null;

        target.AddHandler(InputElement.ScrollGestureEndedEvent, (sender, args) =>
        {
            if (args is ScrollGestureEndedEventArgs scrollEnded)
            {
                capturedId = scrollEnded.Id;
            }
        });

        // Act
        var gestureId = simulator.GetNextGestureId();
        simulator.ScrollEnded(target, gestureId);

        // Assert
        Assert.NotNull(capturedId);
        Assert.Equal(gestureId, capturedId);
    }

    [AvaloniaFact]
    public void ScrollSequence_RaisesMultipleScrollEvents_ThenEnded()
    {
        // Arrange
        var target = new Border { Width = 100, Height = 100, Background = Brushes.Red };
        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();
        var scrollCount = 0;
        var scrollEndedRaised = false;

        target.AddHandler(InputElement.ScrollGestureEvent, (sender, args) =>
        {
            scrollCount++;
        });

        target.AddHandler(InputElement.ScrollGestureEndedEvent, (sender, args) =>
        {
            scrollEndedRaised = true;
        });

        // Act
        simulator.ScrollSequence(target, totalDelta: new Vector(0, 200), steps: 10);

        // Assert
        Assert.Equal(10, scrollCount);
        Assert.True(scrollEndedRaised);
    }

    [AvaloniaFact]
    public void ScrollInertiaStarting_RaisesEvent_WhenReflectionSucceeds()
    {
        // Arrange
        var target = new Border { Width = 100, Height = 100, Background = Brushes.Red };
        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();
        var gestureId = simulator.GetNextGestureId();

        // Act
        var result = simulator.ScrollInertiaStarting(target, gestureId, new Vector(0, 500));

        // Assert - reflection should work since we're testing within Avalonia context
        Assert.True(result, "ScrollInertiaStarting should succeed with reflection");
    }

    #endregion

    #region Pull Gesture Tests

    [AvaloniaFact]
    public void Pull_RaisesEvent_WithCorrectDeltaAndDirection()
    {
        // Arrange
        var target = new Border { Width = 100, Height = 100, Background = Brushes.Red };
        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();
        Vector? capturedDelta = null;
        PullDirection? capturedDirection = null;

        target.AddHandler(InputElement.PullGestureEvent, (sender, args) =>
        {
            if (args is PullGestureEventArgs pull)
            {
                capturedDelta = pull.Delta;
                capturedDirection = pull.PullDirection;
            }
        });

        // Act
        simulator.Pull(target, new Vector(0, 50), PullDirection.TopToBottom);

        // Assert
        Assert.NotNull(capturedDelta);
        Assert.NotNull(capturedDirection);
        Assert.Equal(new Vector(0, 50), capturedDelta);
        Assert.Equal(PullDirection.TopToBottom, capturedDirection);
    }

    [AvaloniaFact]
    public void PullEnded_RaisesEvent_WithCorrectDirection()
    {
        // Arrange
        var target = new Border { Width = 100, Height = 100, Background = Brushes.Red };
        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();
        int? capturedId = null;
        PullDirection? capturedDirection = null;

        target.AddHandler(InputElement.PullGestureEndedEvent, (sender, args) =>
        {
            if (args is PullGestureEndedEventArgs pullEnded)
            {
                capturedId = pullEnded.Id;
                capturedDirection = pullEnded.PullDirection;
            }
        });

        // Act
        var gestureId = simulator.GetNextGestureId();
        simulator.PullEnded(target, gestureId, PullDirection.BottomToTop);

        // Assert
        Assert.NotNull(capturedId);
        Assert.NotNull(capturedDirection);
        Assert.Equal(gestureId, capturedId);
        Assert.Equal(PullDirection.BottomToTop, capturedDirection);
    }

    [AvaloniaFact]
    public void PullToRefresh_RaisesMultiplePullEvents_ThenEnded()
    {
        // Arrange
        var target = new Border { Width = 100, Height = 100, Background = Brushes.Red };
        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();
        var pullCount = 0;
        var pullEndedRaised = false;
        PullDirection? pullEndedDirection = null;

        target.AddHandler(InputElement.PullGestureEvent, (sender, args) =>
        {
            pullCount++;
        });

        target.AddHandler(InputElement.PullGestureEndedEvent, (sender, args) =>
        {
            pullEndedRaised = true;
            if (args is PullGestureEndedEventArgs pullEnded)
            {
                pullEndedDirection = pullEnded.PullDirection;
            }
        });

        // Act
        simulator.PullToRefresh(target, PullDirection.TopToBottom, distance: 100, steps: 10);

        // Assert
        Assert.Equal(10, pullCount);
        Assert.True(pullEndedRaised);
        Assert.Equal(PullDirection.TopToBottom, pullEndedDirection);
    }

    [AvaloniaTheory]
    [InlineData(PullDirection.TopToBottom)]
    [InlineData(PullDirection.BottomToTop)]
    [InlineData(PullDirection.LeftToRight)]
    [InlineData(PullDirection.RightToLeft)]
    public void PullToRefresh_WorksForAllDirections(PullDirection direction)
    {
        // Arrange
        var target = new Border { Width = 100, Height = 100, Background = Brushes.Red };
        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();
        var capturedDirections = new List<PullDirection>();

        target.AddHandler(InputElement.PullGestureEvent, (sender, args) =>
        {
            if (args is PullGestureEventArgs pull)
            {
                capturedDirections.Add(pull.PullDirection);
            }
        });

        // Act
        simulator.PullToRefresh(target, direction, distance: 50, steps: 3);

        // Assert
        Assert.True(capturedDirections.Count > 0);
        Assert.All(capturedDirections, d => Assert.Equal(direction, d));
    }

    #endregion

    #region Touchpad Gesture Tests

    [AvaloniaFact]
    public void TouchpadMagnify_RaisesEvent_WithCorrectDelta()
    {
        // Arrange
        var target = new Border { Width = 100, Height = 100, Background = Brushes.Red };
        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();
        Vector? capturedDelta = null;

        target.AddHandler(InputElement.PointerTouchPadGestureMagnifyEvent, (sender, args) =>
        {
            if (args is PointerDeltaEventArgs delta)
            {
                capturedDelta = delta.Delta;
            }
        });

        // Act
        simulator.TouchpadMagnify(target, new Vector(0.5, 0), new Point(50, 50));

        // Assert
        Assert.NotNull(capturedDelta);
        Assert.Equal(0.5, capturedDelta.Value.X, 2);
    }

    [AvaloniaFact]
    public void TouchpadRotate_RaisesEvent()
    {
        // Arrange
        var target = new Border { Width = 100, Height = 100, Background = Brushes.Red };
        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();
        var eventRaised = false;

        target.AddHandler(InputElement.PointerTouchPadGestureRotateEvent, (sender, args) =>
        {
            eventRaised = true;
        });

        // Act
        simulator.TouchpadRotate(target, angleDelta: 45, position: new Point(50, 50));

        // Assert
        Assert.True(eventRaised);
    }

    [AvaloniaFact]
    public void TouchpadSwipe_RaisesEvent_WithCorrectDelta()
    {
        // Arrange
        var target = new Border { Width = 100, Height = 100, Background = Brushes.Red };
        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();
        Vector? capturedDelta = null;

        target.AddHandler(InputElement.PointerTouchPadGestureSwipeEvent, (sender, args) =>
        {
            if (args is PointerDeltaEventArgs delta)
            {
                capturedDelta = delta.Delta;
            }
        });

        // Act
        simulator.TouchpadSwipe(target, new Vector(100, 0), new Point(50, 50));

        // Assert
        Assert.NotNull(capturedDelta);
        Assert.Equal(new Vector(100, 0), capturedDelta);
    }

    [AvaloniaFact]
    public void TouchpadMagnifySequence_RaisesMultipleEvents()
    {
        // Arrange
        var target = new Border { Width = 100, Height = 100, Background = Brushes.Red };
        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();
        var magnifyCount = 0;

        target.AddHandler(InputElement.PointerTouchPadGestureMagnifyEvent, (sender, args) =>
        {
            magnifyCount++;
        });

        // Act
        simulator.TouchpadMagnifySequence(target, new Point(50, 50), totalMagnification: 1.0, steps: 5);

        // Assert
        Assert.Equal(5, magnifyCount);
    }

    [AvaloniaFact]
    public void TouchpadSwipeSequence_RaisesMultipleEvents()
    {
        // Arrange
        var target = new Border { Width = 100, Height = 100, Background = Brushes.Red };
        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();
        var swipeCount = 0;

        target.AddHandler(InputElement.PointerTouchPadGestureSwipeEvent, (sender, args) =>
        {
            swipeCount++;
        });

        // Act
        simulator.TouchpadSwipeSequence(target, new Point(50, 50), Avalonia.HeadlessTestingFramework.SwipeDirection.Left, distance: 100, steps: 5);

        // Assert
        Assert.Equal(5, swipeCount);
    }

    #endregion

    #region Compound Gesture Tests

    [AvaloniaFact]
    public void TapAndHold_RaisesTapThenHoldEvents()
    {
        // Arrange
        var target = new Border { Width = 100, Height = 100, Background = Brushes.Red };
        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();
        var tapRaised = false;
        var holdingStarted = false;
        var holdingCompleted = false;

        target.AddHandler(InputElement.TappedEvent, (sender, args) =>
        {
            tapRaised = true;
        });

        target.AddHandler(InputElement.HoldingEvent, (sender, args) =>
        {
            if (args is HoldingRoutedEventArgs holding)
            {
                if (holding.HoldingState == HoldingState.Started)
                    holdingStarted = true;
                if (holding.HoldingState == HoldingState.Completed)
                    holdingCompleted = true;
            }
        });

        // Act
        simulator.TapAndHold(target, new Point(50, 50), holdDuration: 300);

        // Assert
        Assert.True(tapRaised);
        Assert.True(holdingStarted);
        Assert.True(holdingCompleted);
    }

    [AvaloniaFact]
    public void DoubleTapZoom_RaisesDoubleTapThenPinchEvents()
    {
        // Arrange
        var target = new Border { Width = 100, Height = 100, Background = Brushes.Red };
        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();
        var doubleTapRaised = false;
        var pinchCount = 0;

        target.AddHandler(InputElement.DoubleTappedEvent, (sender, args) =>
        {
            doubleTapRaised = true;
        });

        target.AddHandler(InputElement.PinchEvent, (sender, args) =>
        {
            pinchCount++;
        });

        // Act
        simulator.DoubleTapZoom(target, new Point(50, 50), zoomScale: 2.0, steps: 5);

        // Assert
        Assert.True(doubleTapRaised);
        Assert.True(pinchCount > 0);
    }

    [AvaloniaFact]
    public void Flick_RaisesScrollSequenceWithInertia()
    {
        // Arrange
        var target = new Border { Width = 100, Height = 100, Background = Brushes.Red };
        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();
        var scrollCount = 0;
        var scrollEndedRaised = false;

        target.AddHandler(InputElement.ScrollGestureEvent, (sender, args) =>
        {
            scrollCount++;
        });

        target.AddHandler(InputElement.ScrollGestureEndedEvent, (sender, args) =>
        {
            scrollEndedRaised = true;
        });

        // Act
        simulator.Flick(target, Avalonia.HeadlessTestingFramework.SwipeDirection.Up, distance: 50, velocity: 500);

        // Assert
        Assert.True(scrollCount > 0);
        Assert.True(scrollEndedRaised);
    }

    [AvaloniaFact]
    public void ThreeFingerSwipe_RaisesTouchpadSwipeEvents()
    {
        // Arrange
        var target = new Border { Width = 100, Height = 100, Background = Brushes.Red };
        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();
        var swipeCount = 0;

        target.AddHandler(InputElement.PointerTouchPadGestureSwipeEvent, (sender, args) =>
        {
            swipeCount++;
        });

        // Act
        simulator.ThreeFingerSwipe(target, new Point(50, 50), Avalonia.HeadlessTestingFramework.SwipeDirection.Right, distance: 100);

        // Assert
        Assert.True(swipeCount > 0);
    }

    #endregion

    #region Timestamp and State Tests

    [AvaloniaFact]
    public void AdvanceTime_IncreasesTimestamp()
    {
        // Arrange
        var simulator = new GestureSimulator();
        var initialTime = simulator.Timestamp;

        // Act
        simulator.AdvanceTime(100);

        // Assert
        Assert.Equal(initialTime + 100, simulator.Timestamp);
    }

    [AvaloniaFact]
    public void Reset_RestoresInitialState()
    {
        // Arrange
        var simulator = new GestureSimulator();
        simulator.AdvanceTime(1000);
        var id1 = simulator.GetNextGestureId();
        var id2 = simulator.GetNextGestureId();

        // Act
        simulator.Reset();

        // Assert
        Assert.Equal(0UL, simulator.Timestamp);
        Assert.Equal(1, simulator.GetNextGestureId()); // First ID after reset should be 1
    }

    [AvaloniaFact]
    public void GetNextGestureId_ReturnsIncreasingIds()
    {
        // Arrange
        var simulator = new GestureSimulator();

        // Act
        var id1 = simulator.GetNextGestureId();
        var id2 = simulator.GetNextGestureId();
        var id3 = simulator.GetNextGestureId();

        // Assert
        Assert.Equal(1, id1);
        Assert.Equal(2, id2);
        Assert.Equal(3, id3);
    }

    #endregion
}
