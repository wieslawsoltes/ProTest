#nullable enable

// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See licence.md and NOTICE.md in the repository root for details.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.HeadlessTestingFramework;
using Xunit;

namespace Avalonia.Headless.UnitTests.Testing.Input;

/// <summary>
/// Comprehensive tests for TouchInputSimulator API.
/// </summary>
public class TouchInputSimulatorTests
{
    #region Constructor and Initialization Tests

    [AvaloniaFact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var simulator = new TouchInputSimulator();

        // Assert
        Assert.Equal(0UL, simulator.Timestamp);
        Assert.Empty(simulator.ActiveTouchPoints);
    }

    [AvaloniaFact]
    public void Reset_ShouldClearAllState()
    {
        // Arrange
        var simulator = new TouchInputSimulator();
        var border = CreateBorderWithWindow();
        simulator.TouchDown(border, new Point(100, 100));
        simulator.AdvanceTime(100);

        // Act
        simulator.Reset();

        // Assert
        Assert.Equal(0UL, simulator.Timestamp);
        Assert.Empty(simulator.ActiveTouchPoints);
    }

    [AvaloniaFact]
    public void AdvanceTime_ShouldIncrementTimestamp()
    {
        // Arrange
        var simulator = new TouchInputSimulator();

        // Act
        simulator.AdvanceTime(100);
        simulator.AdvanceTime(50);

        // Assert
        Assert.Equal(150UL, simulator.Timestamp);
    }

    #endregion

    #region TouchPoint Tests

    [AvaloniaFact]
    public void TouchPoint_Constructor_ShouldSetProperties()
    {
        // Arrange & Act
        var touchPoint = new TouchInputSimulator.TouchPoint(5, new Point(100, 200), 0.75f);

        // Assert
        Assert.Equal(5, touchPoint.Id);
        Assert.Equal(new Point(100, 200), touchPoint.Position);
        Assert.Equal(0.75f, touchPoint.Pressure);
        Assert.True(touchPoint.IsActive);
    }

    [AvaloniaFact]
    public void TouchPoint_DefaultPressure_ShouldBeOne()
    {
        // Arrange & Act
        var touchPoint = new TouchInputSimulator.TouchPoint(0, new Point(0, 0));

        // Assert
        Assert.Equal(1.0f, touchPoint.Pressure);
    }

    #endregion

    #region Touch Events Tests

    [AvaloniaFact]
    public void TouchDown_ShouldCreateActiveTouchPoint()
    {
        // Arrange
        var simulator = new TouchInputSimulator();
        var border = CreateBorderWithWindow();

        // Act
        var touchId = simulator.TouchDown(border, new Point(100, 100));

        // Assert
        Assert.Single(simulator.ActiveTouchPoints);
        Assert.True(simulator.ActiveTouchPoints.ContainsKey(touchId));
        Assert.Equal(new Point(100, 100), simulator.ActiveTouchPoints[touchId].Position);
    }

    [AvaloniaFact]
    public void TouchDown_ShouldReturnIncrementingIds()
    {
        // Arrange
        var simulator = new TouchInputSimulator();
        var border = CreateBorderWithWindow();

        // Act
        var id1 = simulator.TouchDown(border, new Point(100, 100));
        var id2 = simulator.TouchDown(border, new Point(200, 200));
        var id3 = simulator.TouchDown(border, new Point(300, 300));

        // Assert
        Assert.Equal(0, id1);
        Assert.Equal(1, id2);
        Assert.Equal(2, id3);
    }

    [AvaloniaFact]
    public void TouchDown_WithPressure_ShouldStorePressure()
    {
        // Arrange
        var simulator = new TouchInputSimulator();
        var border = CreateBorderWithWindow();

        // Act
        var touchId = simulator.TouchDown(border, new Point(100, 100), 0.5f);

        // Assert
        Assert.Equal(0.5f, simulator.ActiveTouchPoints[touchId].Pressure);
    }

    [AvaloniaFact]
    public void TouchDown_ShouldRaisePointerPressedEvent()
    {
        // Arrange
        var simulator = new TouchInputSimulator();
        var border = CreateBorderWithWindow();
        var eventRaised = false;
        Point? eventPosition = null;

        border.PointerPressed += (s, e) =>
        {
            eventRaised = true;
            eventPosition = e.GetPosition(border);
        };

        // Act
        simulator.TouchDown(border, new Point(100, 100));

        // Assert
        Assert.True(eventRaised);
        Assert.Equal(new Point(100, 100), eventPosition);
    }

    [AvaloniaFact]
    public void TouchMove_ShouldUpdateTouchPointPosition()
    {
        // Arrange
        var simulator = new TouchInputSimulator();
        var border = CreateBorderWithWindow();
        var touchId = simulator.TouchDown(border, new Point(100, 100));

        // Act
        simulator.TouchMove(border, touchId, new Point(150, 150));

        // Assert
        Assert.Equal(new Point(150, 150), simulator.ActiveTouchPoints[touchId].Position);
    }

    [AvaloniaFact]
    public void TouchMove_ShouldRaisePointerMovedEvent()
    {
        // Arrange
        var simulator = new TouchInputSimulator();
        var border = CreateBorderWithWindow();
        var touchId = simulator.TouchDown(border, new Point(100, 100));
        var eventRaised = false;
        Point? eventPosition = null;

        border.PointerMoved += (s, e) =>
        {
            eventRaised = true;
            eventPosition = e.GetPosition(border);
        };

        // Act
        simulator.TouchMove(border, touchId, new Point(150, 150));

        // Assert
        Assert.True(eventRaised);
        Assert.Equal(new Point(150, 150), eventPosition);
    }

    [AvaloniaFact]
    public void TouchMove_WithInvalidTouchId_ShouldThrow()
    {
        // Arrange
        var simulator = new TouchInputSimulator();
        var border = CreateBorderWithWindow();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            simulator.TouchMove(border, 999, new Point(100, 100)));
        Assert.Contains("999", exception.Message);
        Assert.Contains("not active", exception.Message);
    }

    [AvaloniaFact]
    public void TouchUp_ShouldRemoveTouchPoint()
    {
        // Arrange
        var simulator = new TouchInputSimulator();
        var border = CreateBorderWithWindow();
        var touchId = simulator.TouchDown(border, new Point(100, 100));

        // Act
        simulator.TouchUp(border, touchId);

        // Assert
        Assert.Empty(simulator.ActiveTouchPoints);
    }

    [AvaloniaFact]
    public void TouchUp_ShouldRaisePointerReleasedEvent()
    {
        // Arrange
        var simulator = new TouchInputSimulator();
        var border = CreateBorderWithWindow();
        var touchId = simulator.TouchDown(border, new Point(100, 100));
        var eventRaised = false;

        border.PointerReleased += (s, e) => eventRaised = true;

        // Act
        simulator.TouchUp(border, touchId);

        // Assert
        Assert.True(eventRaised);
    }

    [AvaloniaFact]
    public void TouchUp_WithInvalidTouchId_ShouldThrow()
    {
        // Arrange
        var simulator = new TouchInputSimulator();
        var border = CreateBorderWithWindow();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            simulator.TouchUp(border, 999));
        Assert.Contains("999", exception.Message);
    }

    [AvaloniaFact]
    public void Tap_ShouldRaisePressAndRelease()
    {
        // Arrange
        var simulator = new TouchInputSimulator();
        var border = CreateBorderWithWindow();
        var pressedRaised = false;
        var releasedRaised = false;

        border.PointerPressed += (s, e) => pressedRaised = true;
        border.PointerReleased += (s, e) => releasedRaised = true;

        // Act
        simulator.Tap(border, new Point(100, 100));

        // Assert
        Assert.True(pressedRaised);
        Assert.True(releasedRaised);
        Assert.Empty(simulator.ActiveTouchPoints);
    }

    [AvaloniaFact]
    public void Tap_ShouldAdvanceTimeByHoldTime()
    {
        // Arrange
        var simulator = new TouchInputSimulator();
        var border = CreateBorderWithWindow();

        // Act
        simulator.Tap(border, new Point(100, 100), holdTime: 100);

        // Assert
        Assert.Equal(100UL, simulator.Timestamp);
    }

    [AvaloniaFact]
    public void DoubleTap_ShouldRaiseTwoTaps()
    {
        // Arrange
        var simulator = new TouchInputSimulator();
        var border = CreateBorderWithWindow();
        var pressCount = 0;
        var releaseCount = 0;

        border.PointerPressed += (s, e) => pressCount++;
        border.PointerReleased += (s, e) => releaseCount++;

        // Act
        simulator.DoubleTap(border, new Point(100, 100));

        // Assert
        Assert.Equal(2, pressCount);
        Assert.Equal(2, releaseCount);
    }

    [AvaloniaFact]
    public void DoubleTap_ShouldAdvanceTimeByTapInterval()
    {
        // Arrange
        var simulator = new TouchInputSimulator();
        var border = CreateBorderWithWindow();

        // Act
        simulator.DoubleTap(border, new Point(100, 100), tapInterval: 150);

        // Assert - two taps with default 50ms hold + 150ms interval
        Assert.Equal(250UL, simulator.Timestamp);
    }

    #endregion

    #region Gesture Events Tests

    [AvaloniaFact]
    public void PinchGesture_ShouldRaisePinchEvent()
    {
        // Arrange
        var simulator = new TouchInputSimulator();
        var border = CreateBorderWithWindow();
        var eventRaised = false;
        double? receivedScale = null;
        Point? receivedOrigin = null;

        border.AddHandler(InputElement.PinchEvent, (object? s, PinchEventArgs e) =>
        {
            eventRaised = true;
            receivedScale = e.Scale;
            receivedOrigin = e.ScaleOrigin;
        });

        // Act
        simulator.PinchGesture(border, scale: 1.5, scaleOrigin: new Point(200, 150));

        // Assert
        Assert.True(eventRaised);
        Assert.Equal(1.5, receivedScale);
        Assert.Equal(new Point(200, 150), receivedOrigin);
    }

    [AvaloniaFact]
    public void PinchGesture_WithAngleDelta_ShouldRaiseEvent()
    {
        // Arrange
        var simulator = new TouchInputSimulator();
        var border = CreateBorderWithWindow();
        var eventRaised = false;
        double receivedScale = 0;
        Point? receivedOrigin = null;

        border.AddHandler(InputElement.PinchEvent, (object? s, PinchEventArgs e) =>
        {
            eventRaised = true;
            receivedScale = e.Scale;
            receivedOrigin = e.ScaleOrigin;
        });

        // Act
        simulator.PinchGesture(border, scale: 1.5, scaleOrigin: new Point(100, 100), angleDelta: 0.5);

        // Assert - verify event is raised with correct scale and origin
        Assert.True(eventRaised);
        Assert.Equal(1.5, receivedScale);
        Assert.Equal(new Point(100, 100), receivedOrigin);
    }

    [AvaloniaFact]
    public void PinchGestureEnded_ShouldRaisePinchEndedEvent()
    {
        // Arrange
        var simulator = new TouchInputSimulator();
        var border = CreateBorderWithWindow();
        var eventRaised = false;

        border.AddHandler(InputElement.PinchEndedEvent, (object? s, PinchEndedEventArgs e) =>
        {
            eventRaised = true;
        });

        // Act
        simulator.PinchGestureEnded(border);

        // Assert
        Assert.True(eventRaised);
    }

    [AvaloniaFact]
    public void ScrollGesture_ShouldRaiseScrollGestureEvent()
    {
        // Arrange
        var simulator = new TouchInputSimulator();
        var border = CreateBorderWithWindow();
        var eventRaised = false;
        Vector? receivedDelta = null;

        border.AddHandler(InputElement.ScrollGestureEvent, (object? s, ScrollGestureEventArgs e) =>
        {
            eventRaised = true;
            receivedDelta = e.Delta;
        });

        // Act
        simulator.ScrollGesture(border, new Vector(50, 30));

        // Assert
        Assert.True(eventRaised);
        Assert.Equal(new Vector(50, 30), receivedDelta);
    }

    [AvaloniaFact]
    public void ScrollGestureEnded_ShouldRaiseScrollGestureEndedEvent()
    {
        // Arrange
        var simulator = new TouchInputSimulator();
        var border = CreateBorderWithWindow();
        var eventRaised = false;

        border.AddHandler(InputElement.ScrollGestureEndedEvent, (object? s, ScrollGestureEndedEventArgs e) =>
        {
            eventRaised = true;
        });

        // Act
        simulator.ScrollGestureEnded(border);

        // Assert
        Assert.True(eventRaised);
    }

    [AvaloniaFact]
    public void TouchpadMagnify_ShouldRaiseMagnifyEvent()
    {
        // Arrange
        var simulator = new TouchInputSimulator();
        var border = CreateBorderWithWindow();
        var eventRaised = false;
        Vector? receivedDelta = null;

        border.AddHandler(InputElement.PointerTouchPadGestureMagnifyEvent, (object? s, PointerDeltaEventArgs e) =>
        {
            eventRaised = true;
            receivedDelta = e.Delta;
        });

        // Act
        simulator.TouchpadMagnify(border, new Vector(0.5, 0), new Point(200, 150));

        // Assert
        Assert.True(eventRaised);
        Assert.Equal(new Vector(0.5, 0), receivedDelta);
    }

    [AvaloniaFact]
    public void TouchpadSwipe_ShouldRaiseSwipeEvent()
    {
        // Arrange
        var simulator = new TouchInputSimulator();
        var border = CreateBorderWithWindow();
        var eventRaised = false;
        Vector? receivedDelta = null;

        border.AddHandler(InputElement.PointerTouchPadGestureSwipeEvent, (object? s, PointerDeltaEventArgs e) =>
        {
            eventRaised = true;
            receivedDelta = e.Delta;
        });

        // Act
        simulator.TouchpadSwipe(border, new Vector(100, 0), new Point(200, 150));

        // Assert
        Assert.True(eventRaised);
        Assert.Equal(new Vector(100, 0), receivedDelta);
    }

    [AvaloniaFact]
    public void TouchpadRotate_ShouldRaiseRotateEvent()
    {
        // Arrange
        var simulator = new TouchInputSimulator();
        var border = CreateBorderWithWindow();
        var eventRaised = false;

        border.AddHandler(InputElement.PointerTouchPadGestureRotateEvent, (object? s, PointerDeltaEventArgs e) =>
        {
            eventRaised = true;
        });

        // Act
        simulator.TouchpadRotate(border, 45, new Point(200, 150));

        // Assert
        Assert.True(eventRaised);
    }

    #endregion

    #region Multi-Touch Scenario Tests

    [AvaloniaFact]
    public void SimulatePinchZoom_ShouldCreateTwoTouchPoints()
    {
        // Arrange
        var simulator = new TouchInputSimulator();
        var border = CreateBorderWithWindow();
        var maxTouchPoints = 0;

        border.PointerPressed += (s, e) =>
        {
            if (simulator.ActiveTouchPoints.Count > maxTouchPoints)
                maxTouchPoints = simulator.ActiveTouchPoints.Count;
        };

        // Act
        simulator.SimulatePinchZoom(border, new Point(200, 150), startDistance: 50, endDistance: 150);

        // Assert
        Assert.Equal(2, maxTouchPoints);
        Assert.Empty(simulator.ActiveTouchPoints); // All released after
    }

    [AvaloniaFact]
    public void SimulatePinchZoom_ShouldRaisePinchEvents()
    {
        // Arrange
        var simulator = new TouchInputSimulator();
        var border = CreateBorderWithWindow();
        var pinchCount = 0;
        var pinchEndedRaised = false;

        border.AddHandler(InputElement.PinchEvent, (object? s, PinchEventArgs e) => pinchCount++);
        border.AddHandler(InputElement.PinchEndedEvent, (object? s, PinchEndedEventArgs e) => pinchEndedRaised = true);

        // Act
        simulator.SimulatePinchZoom(border, new Point(200, 150), startDistance: 50, endDistance: 150, steps: 5);

        // Assert
        Assert.Equal(5, pinchCount);
        Assert.True(pinchEndedRaised);
    }

    [AvaloniaFact]
    public void SimulateTwoFingerPan_ShouldRaiseScrollEvents()
    {
        // Arrange
        var simulator = new TouchInputSimulator();
        var border = CreateBorderWithWindow();
        var scrollCount = 0;
        var scrollEndedRaised = false;

        border.AddHandler(InputElement.ScrollGestureEvent, (object? s, ScrollGestureEventArgs e) => scrollCount++);
        border.AddHandler(InputElement.ScrollGestureEndedEvent, (object? s, ScrollGestureEndedEventArgs e) => scrollEndedRaised = true);

        // Act
        simulator.SimulateTwoFingerPan(border, new Point(100, 100), new Point(200, 150), steps: 5);

        // Assert
        Assert.Equal(5, scrollCount);
        Assert.True(scrollEndedRaised);
    }

    [AvaloniaFact]
    public void SimulateRotation_ShouldRaisePinchEventsWithAngleDelta()
    {
        // Arrange
        var simulator = new TouchInputSimulator();
        var border = CreateBorderWithWindow();
        var pinchCount = 0;
        var hasAngleDelta = false;

        border.AddHandler(InputElement.PinchEvent, (object? s, PinchEventArgs e) =>
        {
            pinchCount++;
            if (Math.Abs(e.AngleDelta) > 0.001)
                hasAngleDelta = true;
        });

        // Act
        simulator.SimulateRotation(border, new Point(200, 150), radius: 50, startAngle: 0, endAngle: 45, steps: 5);

        // Assert
        Assert.Equal(5, pinchCount);
        Assert.True(hasAngleDelta);
    }

    [AvaloniaFact]
    public void SimulateDrag_ShouldRaiseMoveEvents()
    {
        // Arrange
        var simulator = new TouchInputSimulator();
        var border = CreateBorderWithWindow();
        var moveCount = 0;
        var pressedRaised = false;
        var releasedRaised = false;

        border.PointerPressed += (s, e) => pressedRaised = true;
        border.PointerMoved += (s, e) => moveCount++;
        border.PointerReleased += (s, e) => releasedRaised = true;

        // Act
        simulator.SimulateDrag(border, new Point(100, 100), new Point(200, 200), steps: 5);

        // Assert
        Assert.True(pressedRaised);
        Assert.Equal(5, moveCount);
        Assert.True(releasedRaised);
    }

    [AvaloniaFact]
    public void Swipe_Left_ShouldMoveInCorrectDirection()
    {
        // Arrange
        var simulator = new TouchInputSimulator();
        var border = CreateBorderWithWindow();
        Point? lastPosition = null;

        border.PointerMoved += (s, e) => lastPosition = e.GetPosition(border);

        // Act
        simulator.Swipe(border, new Point(200, 100), Avalonia.HeadlessTestingFramework.SwipeDirection.Left, distance: 100);

        // Assert
        Assert.NotNull(lastPosition);
        Assert.True(lastPosition.Value.X < 200); // Moved left
    }

    [AvaloniaFact]
    public void Swipe_Right_ShouldMoveInCorrectDirection()
    {
        // Arrange
        var simulator = new TouchInputSimulator();
        var border = CreateBorderWithWindow();
        Point? lastPosition = null;

        border.PointerMoved += (s, e) => lastPosition = e.GetPosition(border);

        // Act
        simulator.Swipe(border, new Point(100, 100), Avalonia.HeadlessTestingFramework.SwipeDirection.Right, distance: 100);

        // Assert
        Assert.NotNull(lastPosition);
        Assert.True(lastPosition.Value.X > 100); // Moved right
    }

    [AvaloniaFact]
    public void Swipe_Up_ShouldMoveInCorrectDirection()
    {
        // Arrange
        var simulator = new TouchInputSimulator();
        var border = CreateBorderWithWindow();
        Point? lastPosition = null;

        border.PointerMoved += (s, e) => lastPosition = e.GetPosition(border);

        // Act
        simulator.Swipe(border, new Point(100, 200), Avalonia.HeadlessTestingFramework.SwipeDirection.Up, distance: 100);

        // Assert
        Assert.NotNull(lastPosition);
        Assert.True(lastPosition.Value.Y < 200); // Moved up
    }

    [AvaloniaFact]
    public void Swipe_Down_ShouldMoveInCorrectDirection()
    {
        // Arrange
        var simulator = new TouchInputSimulator();
        var border = CreateBorderWithWindow();
        Point? lastPosition = null;

        border.PointerMoved += (s, e) => lastPosition = e.GetPosition(border);

        // Act
        simulator.Swipe(border, new Point(100, 100), Avalonia.HeadlessTestingFramework.SwipeDirection.Down, distance: 100);

        // Assert
        Assert.NotNull(lastPosition);
        Assert.True(lastPosition.Value.Y > 100); // Moved down
    }

    #endregion

    #region Multiple Touch Points Tests

    [AvaloniaFact]
    public void MultipleTouchPoints_ShouldBeTrackedIndependently()
    {
        // Arrange
        var simulator = new TouchInputSimulator();
        var border = CreateBorderWithWindow();

        // Act
        var id1 = simulator.TouchDown(border, new Point(100, 100));
        var id2 = simulator.TouchDown(border, new Point(200, 200));
        var id3 = simulator.TouchDown(border, new Point(300, 300));

        simulator.TouchMove(border, id2, new Point(250, 250));

        // Assert
        Assert.Equal(3, simulator.ActiveTouchPoints.Count);
        Assert.Equal(new Point(100, 100), simulator.ActiveTouchPoints[id1].Position);
        Assert.Equal(new Point(250, 250), simulator.ActiveTouchPoints[id2].Position);
        Assert.Equal(new Point(300, 300), simulator.ActiveTouchPoints[id3].Position);

        // Release middle touch
        simulator.TouchUp(border, id2);
        Assert.Equal(2, simulator.ActiveTouchPoints.Count);
        Assert.False(simulator.ActiveTouchPoints.ContainsKey(id2));
    }

    #endregion

    #region Helper Methods

    private static Border CreateBorderWithWindow()
    {
        var border = new Border
        {
            Width = 400,
            Height = 300,
            Background = Avalonia.Media.Brushes.White
        };
        var window = new Window { Content = border };
        window.Show();
        return border;
    }

    #endregion
}
