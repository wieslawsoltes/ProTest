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

namespace Avalonia.Headless.UnitTests.Testing.Gestures;

/// <summary>
/// Comprehensive tests for GestureRecognizerTestHelper API.
/// </summary>
public class GestureRecognizerTestHelperTests
{
    #region Constructor Tests

    [AvaloniaFact]
    public void Constructor_Default_ShouldCreateTouchPointer()
    {
        // Arrange & Act
        var helper = new GestureRecognizerTestHelper();

        // Assert
        Assert.NotNull(helper.Pointer);
        Assert.Equal(PointerType.Touch, helper.Pointer.Type);
    }

    [AvaloniaFact]
    public void Constructor_WithPointerType_ShouldCreateCorrectType()
    {
        // Arrange & Act
        var touchHelper = new GestureRecognizerTestHelper(PointerType.Touch);
        var penHelper = new GestureRecognizerTestHelper(PointerType.Pen);
        var mouseHelper = new GestureRecognizerTestHelper(PointerType.Mouse);

        // Assert
        Assert.Equal(PointerType.Touch, touchHelper.Pointer.Type);
        Assert.Equal(PointerType.Pen, penHelper.Pointer.Type);
        Assert.Equal(PointerType.Mouse, mouseHelper.Pointer.Type);
    }

    [AvaloniaFact]
    public void Constructor_ShouldCreateUniquePointerId()
    {
        // Arrange & Act
        var helper1 = new GestureRecognizerTestHelper();
        var helper2 = new GestureRecognizerTestHelper();
        var helper3 = new GestureRecognizerTestHelper();

        // Assert - Each pointer should have a unique ID
        Assert.NotEqual(helper1.Pointer.Id, helper2.Pointer.Id);
        Assert.NotEqual(helper2.Pointer.Id, helper3.Pointer.Id);
        Assert.NotEqual(helper1.Pointer.Id, helper3.Pointer.Id);
    }

    #endregion

    #region Properties Tests

    [AvaloniaFact]
    public void Captured_Initially_ShouldBeNull()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper();

        // Assert
        Assert.Null(helper.Captured);
    }

    [AvaloniaFact]
    public void CapturedGestureRecognizer_Initially_ShouldBeNull()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper();

        // Assert
        Assert.Null(helper.CapturedGestureRecognizer);
    }

    [AvaloniaFact]
    public void Down_ShouldCaptureTarget()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper();
        var border = CreateBorderWithWindow();

        // Act
        helper.Down(border, new Point(100, 100));

        // Assert
        Assert.Equal(border, helper.Captured);
    }

    #endregion

    #region Down Tests

    [AvaloniaFact]
    public void Down_ShouldRaisePointerPressedEvent()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper();
        var border = CreateBorderWithWindow();
        var eventRaised = false;
        Point? eventPosition = null;

        border.PointerPressed += (s, e) =>
        {
            eventRaised = true;
            eventPosition = e.GetPosition(border);
        };

        // Act
        helper.Down(border, new Point(100, 100));

        // Assert
        Assert.True(eventRaised);
        Assert.Equal(new Point(100, 100), eventPosition);
    }

    [AvaloniaFact]
    public void Down_WithModifiers_ShouldIncludeModifiers()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper();
        var border = CreateBorderWithWindow();
        KeyModifiers? receivedModifiers = null;

        border.PointerPressed += (s, e) =>
        {
            receivedModifiers = e.KeyModifiers;
        };

        // Act
        helper.Down(border, new Point(100, 100), KeyModifiers.Control);

        // Assert
        Assert.Equal(KeyModifiers.Control, receivedModifiers);
    }

    [AvaloniaFact]
    public void Down_WithSeparateSource_ShouldUseSource()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper();
        var target = CreateBorderWithWindow();
        var source = new Border { Width = 100, Height = 100 };
        target.Child = source;

        object? eventSource = null;

        source.PointerPressed += (s, e) =>
        {
            eventSource = e.Source;
        };

        // Act
        helper.Down(target, source, new Point(50, 50));

        // Assert
        Assert.Equal(source, eventSource);
    }

    #endregion

    #region Move Tests

    [AvaloniaFact]
    public void Move_WithoutCapture_ShouldRaisePointerMovedEvent()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper();
        var border = CreateBorderWithWindow();
        helper.Down(border, new Point(100, 100));

        var eventRaised = false;
        Point? eventPosition = null;

        border.PointerMoved += (s, e) =>
        {
            eventRaised = true;
            eventPosition = e.GetPosition(border);
        };

        // Act
        helper.Move(border, new Point(150, 150));

        // Assert
        Assert.True(eventRaised);
        Assert.Equal(new Point(150, 150), eventPosition);
    }

    [AvaloniaFact]
    public void Move_WithModifiers_ShouldIncludeModifiers()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper();
        var border = CreateBorderWithWindow();
        helper.Down(border, new Point(100, 100));

        KeyModifiers? receivedModifiers = null;

        border.PointerMoved += (s, e) =>
        {
            receivedModifiers = e.KeyModifiers;
        };

        // Act
        helper.Move(border, new Point(150, 150), KeyModifiers.Shift);

        // Assert
        Assert.Equal(KeyModifiers.Shift, receivedModifiers);
    }

    #endregion

    #region Up Tests

    [AvaloniaFact]
    public void Up_ShouldRaisePointerReleasedEvent()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper();
        var border = CreateBorderWithWindow();
        helper.Down(border, new Point(100, 100));

        var eventRaised = false;

        border.PointerReleased += (s, e) =>
        {
            eventRaised = true;
        };

        // Act
        helper.Up(border);

        // Assert
        Assert.True(eventRaised);
    }

    [AvaloniaFact]
    public void Up_ShouldReleaseCapture()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper();
        var border = CreateBorderWithWindow();
        helper.Down(border, new Point(100, 100));

        // Act
        helper.Up(border);

        // Assert
        Assert.Null(helper.Captured);
    }

    [AvaloniaFact]
    public void Up_WithPosition_ShouldUsePosition()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper();
        var border = CreateBorderWithWindow();
        helper.Down(border, new Point(100, 100));

        Point? eventPosition = null;

        border.PointerReleased += (s, e) =>
        {
            eventPosition = e.GetPosition(border);
        };

        // Act
        helper.Up(border, new Point(150, 150));

        // Assert
        Assert.Equal(new Point(150, 150), eventPosition);
    }

    #endregion

    #region Tap Tests

    [AvaloniaFact]
    public void Tap_ShouldRaiseDownAndUp()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper();
        var border = CreateBorderWithWindow();
        var pressedRaised = false;
        var releasedRaised = false;

        border.PointerPressed += (s, e) => pressedRaised = true;
        border.PointerReleased += (s, e) => releasedRaised = true;

        // Act
        helper.Tap(border, new Point(100, 100));

        // Assert
        Assert.True(pressedRaised);
        Assert.True(releasedRaised);
    }

    [AvaloniaFact]
    public void Tap_ShouldReleaseCaptureAfter()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper();
        var border = CreateBorderWithWindow();

        // Act
        helper.Tap(border, new Point(100, 100));

        // Assert
        Assert.Null(helper.Captured);
    }

    [AvaloniaFact]
    public void Tap_WithModifiers_ShouldIncludeModifiers()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper();
        var border = CreateBorderWithWindow();
        KeyModifiers? pressedModifiers = null;
        KeyModifiers? releasedModifiers = null;

        border.PointerPressed += (s, e) => pressedModifiers = e.KeyModifiers;
        border.PointerReleased += (s, e) => releasedModifiers = e.KeyModifiers;

        // Act
        helper.Tap(border, new Point(100, 100), KeyModifiers.Alt);

        // Assert
        Assert.Equal(KeyModifiers.Alt, pressedModifiers);
        Assert.Equal(KeyModifiers.Alt, releasedModifiers);
    }

    #endregion

    #region Cancel Tests

    [AvaloniaFact]
    public void Cancel_ShouldReleaseCapture()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper();
        var border = CreateBorderWithWindow();
        helper.Down(border, new Point(100, 100));

        // Act
        helper.Cancel();

        // Assert
        Assert.Null(helper.Captured);
    }

    [AvaloniaFact]
    public void Cancel_WithoutCapture_ShouldNotThrow()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper();

        // Act & Assert - should not throw
        helper.Cancel();
        Assert.Null(helper.Captured);
    }

    #endregion

    #region Drag Tests

    [AvaloniaFact]
    public void Drag_ShouldPerformCompleteGesture()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper();
        var border = CreateBorderWithWindow();
        var pressedRaised = false;
        var moveCount = 0;
        var releasedRaised = false;

        border.PointerPressed += (s, e) => pressedRaised = true;
        border.PointerMoved += (s, e) => moveCount++;
        border.PointerReleased += (s, e) => releasedRaised = true;

        // Act
        helper.Drag(border, new Point(100, 100), new Point(200, 200), steps: 5);

        // Assert
        Assert.True(pressedRaised);
        Assert.Equal(5, moveCount);
        Assert.True(releasedRaised);
    }

    [AvaloniaFact]
    public void Drag_ShouldInterpolatePositions()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper();
        var border = CreateBorderWithWindow();
        var positions = new System.Collections.Generic.List<Point>();

        border.PointerMoved += (s, e) => positions.Add(e.GetPosition(border));

        // Act
        helper.Drag(border, new Point(0, 0), new Point(100, 100), steps: 4);

        // Assert
        Assert.Equal(4, positions.Count);
        Assert.Equal(new Point(25, 25), positions[0]);
        Assert.Equal(new Point(50, 50), positions[1]);
        Assert.Equal(new Point(75, 75), positions[2]);
        Assert.Equal(new Point(100, 100), positions[3]);
    }

    [AvaloniaFact]
    public void Drag_ShouldReleaseCaptureAfter()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper();
        var border = CreateBorderWithWindow();

        // Act
        helper.Drag(border, new Point(100, 100), new Point(200, 200));

        // Assert
        Assert.Null(helper.Captured);
    }

    #endregion

    #region Pointer Type Behavior Tests

    [AvaloniaFact]
    public void TouchPointer_ShouldHaveIsPrimaryTrue()
    {
        // Arrange & Act
        var helper = new GestureRecognizerTestHelper(PointerType.Touch);

        // Assert
        Assert.True(helper.Pointer.IsPrimary);
    }

    [AvaloniaFact]
    public void MultipleHelpers_ShouldHaveDistinctPointers()
    {
        // Arrange
        var helper1 = new GestureRecognizerTestHelper();
        var helper2 = new GestureRecognizerTestHelper();
        var border = CreateBorderWithWindow();

        IPointer? pointer1 = null;
        IPointer? pointer2 = null;

        border.PointerPressed += (s, e) =>
        {
            if (pointer1 == null)
                pointer1 = e.Pointer;
            else
                pointer2 = e.Pointer;
        };

        // Act
        helper1.Down(border, new Point(100, 100));
        helper2.Down(border, new Point(200, 200));

        // Assert
        Assert.NotNull(pointer1);
        Assert.NotNull(pointer2);
        Assert.NotSame(pointer1, pointer2);
    }

    #endregion

    #region Negative Tests - Invalid Inputs

    [AvaloniaFact]
    public void Down_WithNullTarget_ShouldThrowNullReferenceException()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper();

        // Act & Assert - library throws NullReferenceException for null target
        Assert.Throws<NullReferenceException>(() => helper.Down(null!, new Point(100, 100)));
    }

    [AvaloniaFact]
    public void Move_WithNullTarget_ShouldThrowNullReferenceException()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper();

        // Act & Assert - library throws NullReferenceException for null target
        Assert.Throws<NullReferenceException>(() => helper.Move(null!, new Point(100, 100)));
    }

    [AvaloniaFact]
    public void Up_WithNullTarget_ShouldThrowNullReferenceException()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper();

        // Act & Assert - library throws NullReferenceException for null target
        Assert.Throws<NullReferenceException>(() => helper.Up(null!));
    }

    [AvaloniaFact]
    public void Tap_WithNullTarget_ShouldThrowNullReferenceException()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper();

        // Act & Assert - library throws NullReferenceException for null target
        Assert.Throws<NullReferenceException>(() => helper.Tap(null!, new Point(100, 100)));
    }

    [AvaloniaFact]
    public void Drag_WithNullTarget_ShouldThrowNullReferenceException()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper();

        // Act & Assert - library throws NullReferenceException for null target
        Assert.Throws<NullReferenceException>(() => helper.Drag(null!, new Point(0, 0), new Point(100, 100)));
    }

    [AvaloniaFact]
    public void Drag_WithZeroSteps_ShouldNotRaiseMoveEvents()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper();
        var border = CreateBorderWithWindow();
        var moveCount = 0;

        border.PointerMoved += (s, e) => moveCount++;

        // Act
        helper.Drag(border, new Point(0, 0), new Point(100, 100), steps: 0);

        // Assert
        Assert.Equal(0, moveCount);
    }

    [AvaloniaFact]
    public void Drag_WithNegativeSteps_ShouldNotRaiseMoveEvents()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper();
        var border = CreateBorderWithWindow();
        var moveCount = 0;

        border.PointerMoved += (s, e) => moveCount++;

        // Act
        helper.Drag(border, new Point(0, 0), new Point(100, 100), steps: -5);

        // Assert
        Assert.Equal(0, moveCount);
    }

    [AvaloniaFact]
    public void Down_WithNegativeCoordinates_ShouldWork()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper();
        var border = CreateBorderWithWindow();
        Point? eventPosition = null;

        border.PointerPressed += (s, e) => eventPosition = e.GetPosition(border);

        // Act
        helper.Down(border, new Point(-50, -50));

        // Assert
        Assert.Equal(new Point(-50, -50), eventPosition);
    }

    [AvaloniaFact]
    public void Down_WithVeryLargeCoordinates_ShouldWork()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper();
        var border = CreateBorderWithWindow();
        Point? eventPosition = null;

        border.PointerPressed += (s, e) => eventPosition = e.GetPosition(border);

        // Act
        helper.Down(border, new Point(double.MaxValue / 2, double.MaxValue / 2));

        // Assert
        Assert.NotNull(eventPosition);
    }

    #endregion

    #region Concurrent Usage Tests

    [AvaloniaFact]
    public void MultipleHelpers_SimultaneousDown_ShouldAllCapture()
    {
        // Arrange
        var helper1 = new GestureRecognizerTestHelper();
        var helper2 = new GestureRecognizerTestHelper();
        var helper3 = new GestureRecognizerTestHelper();
        var border = CreateBorderWithWindow();

        // Act
        helper1.Down(border, new Point(100, 100));
        helper2.Down(border, new Point(150, 150));
        helper3.Down(border, new Point(200, 200));

        // Assert - all should have captured
        Assert.Equal(border, helper1.Captured);
        Assert.Equal(border, helper2.Captured);
        Assert.Equal(border, helper3.Captured);
    }

    [AvaloniaFact]
    public void MultipleHelpers_IndependentRelease_ShouldNotAffectOthers()
    {
        // Arrange
        var helper1 = new GestureRecognizerTestHelper();
        var helper2 = new GestureRecognizerTestHelper();
        var border = CreateBorderWithWindow();

        helper1.Down(border, new Point(100, 100));
        helper2.Down(border, new Point(200, 200));

        // Act - release helper1
        helper1.Up(border);

        // Assert - helper2 should still have capture
        Assert.Null(helper1.Captured);
        Assert.Equal(border, helper2.Captured);
    }

    [AvaloniaFact]
    public void MultipleHelpers_InterleavedOperations_ShouldWorkCorrectly()
    {
        // Arrange
        var helper1 = new GestureRecognizerTestHelper();
        var helper2 = new GestureRecognizerTestHelper();
        var border = CreateBorderWithWindow();
        var positions1 = new System.Collections.Generic.List<Point>();
        var positions2 = new System.Collections.Generic.List<Point>();

        border.PointerMoved += (s, e) =>
        {
            if (e.Pointer.Id == helper1.Pointer.Id)
                positions1.Add(e.GetPosition(border));
            else if (e.Pointer.Id == helper2.Pointer.Id)
                positions2.Add(e.GetPosition(border));
        };

        // Act - interleaved moves
        helper1.Down(border, new Point(0, 0));
        helper2.Down(border, new Point(100, 100));
        helper1.Move(border, new Point(10, 10));
        helper2.Move(border, new Point(110, 110));
        helper1.Move(border, new Point(20, 20));
        helper2.Move(border, new Point(120, 120));

        // Assert
        Assert.Equal(2, positions1.Count);
        Assert.Equal(2, positions2.Count);
    }

    [AvaloniaFact]
    public void MultipleHelpers_SamePositionDown_ShouldHaveDistinctPointers()
    {
        // Arrange
        var helper1 = new GestureRecognizerTestHelper();
        var helper2 = new GestureRecognizerTestHelper();
        var border = CreateBorderWithWindow();
        var pointerIds = new System.Collections.Generic.List<int>();

        border.PointerPressed += (s, e) => pointerIds.Add(e.Pointer.Id);

        // Act - both press at same position
        helper1.Down(border, new Point(100, 100));
        helper2.Down(border, new Point(100, 100));

        // Assert
        Assert.Equal(2, pointerIds.Count);
        Assert.NotEqual(pointerIds[0], pointerIds[1]);
    }

    [AvaloniaFact]
    public void MultipleHelpers_CancelOne_ShouldNotAffectOthers()
    {
        // Arrange
        var helper1 = new GestureRecognizerTestHelper();
        var helper2 = new GestureRecognizerTestHelper();
        var border = CreateBorderWithWindow();

        helper1.Down(border, new Point(100, 100));
        helper2.Down(border, new Point(200, 200));

        // Act
        helper1.Cancel();

        // Assert
        Assert.Null(helper1.Captured);
        Assert.Equal(border, helper2.Captured);
    }

    #endregion

    #region Mouse Pointer Type Tests

    [AvaloniaFact]
    public void MousePointer_Down_ShouldRaisePointerPressedEvent()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper(PointerType.Mouse);
        var border = CreateBorderWithWindow();
        PointerType? receivedType = null;

        border.PointerPressed += (s, e) => receivedType = e.Pointer.Type;

        // Act
        helper.Down(border, new Point(100, 100));

        // Assert
        Assert.Equal(PointerType.Mouse, receivedType);
    }

    [AvaloniaFact]
    public void MousePointer_Drag_ShouldWorkCorrectly()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper(PointerType.Mouse);
        var border = CreateBorderWithWindow();
        var moveCount = 0;
        PointerType? moveType = null;

        border.PointerMoved += (s, e) =>
        {
            moveCount++;
            moveType = e.Pointer.Type;
        };

        // Act
        helper.Drag(border, new Point(0, 0), new Point(100, 100), steps: 5);

        // Assert
        Assert.Equal(5, moveCount);
        Assert.Equal(PointerType.Mouse, moveType);
    }

    [AvaloniaFact]
    public void MousePointer_IsPrimary_ShouldBeTrue()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper(PointerType.Mouse);

        // Assert
        Assert.True(helper.Pointer.IsPrimary);
    }

    [AvaloniaFact]
    public void MousePointer_Tap_ShouldWork()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper(PointerType.Mouse);
        var border = CreateBorderWithWindow();
        var pressedRaised = false;
        var releasedRaised = false;
        PointerType? pressedType = null;

        border.PointerPressed += (s, e) =>
        {
            pressedRaised = true;
            pressedType = e.Pointer.Type;
        };
        border.PointerReleased += (s, e) => releasedRaised = true;

        // Act
        helper.Tap(border, new Point(100, 100));

        // Assert
        Assert.True(pressedRaised);
        Assert.True(releasedRaised);
        Assert.Equal(PointerType.Mouse, pressedType);
    }

    #endregion

    #region Pen Pointer Type Tests

    [AvaloniaFact]
    public void PenPointer_Down_ShouldRaisePointerPressedEvent()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper(PointerType.Pen);
        var border = CreateBorderWithWindow();
        PointerType? receivedType = null;

        border.PointerPressed += (s, e) => receivedType = e.Pointer.Type;

        // Act
        helper.Down(border, new Point(100, 100));

        // Assert
        Assert.Equal(PointerType.Pen, receivedType);
    }

    [AvaloniaFact]
    public void PenPointer_Drag_ShouldWorkCorrectly()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper(PointerType.Pen);
        var border = CreateBorderWithWindow();
        var moveCount = 0;
        PointerType? moveType = null;

        border.PointerMoved += (s, e) =>
        {
            moveCount++;
            moveType = e.Pointer.Type;
        };

        // Act
        helper.Drag(border, new Point(0, 0), new Point(100, 100), steps: 5);

        // Assert
        Assert.Equal(5, moveCount);
        Assert.Equal(PointerType.Pen, moveType);
    }

    [AvaloniaFact]
    public void PenPointer_IsPrimary_ShouldBeTrue()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper(PointerType.Pen);

        // Assert
        Assert.True(helper.Pointer.IsPrimary);
    }

    [AvaloniaFact]
    public void PenPointer_Tap_ShouldWork()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper(PointerType.Pen);
        var border = CreateBorderWithWindow();
        var pressedRaised = false;
        var releasedRaised = false;
        PointerType? pressedType = null;

        border.PointerPressed += (s, e) =>
        {
            pressedRaised = true;
            pressedType = e.Pointer.Type;
        };
        border.PointerReleased += (s, e) => releasedRaised = true;

        // Act
        helper.Tap(border, new Point(100, 100));

        // Assert
        Assert.True(pressedRaised);
        Assert.True(releasedRaised);
        Assert.Equal(PointerType.Pen, pressedType);
    }

    [AvaloniaFact]
    public void PenPointer_Move_ShouldReportCorrectType()
    {
        // Arrange
        var helper = new GestureRecognizerTestHelper(PointerType.Pen);
        var border = CreateBorderWithWindow();
        PointerType? moveType = null;

        border.PointerMoved += (s, e) => moveType = e.Pointer.Type;

        // Act
        helper.Down(border, new Point(100, 100));
        helper.Move(border, new Point(150, 150));

        // Assert
        Assert.Equal(PointerType.Pen, moveType);
    }

    #endregion

    #region Mixed Pointer Type Tests

    [AvaloniaFact]
    public void MixedPointerTypes_ShouldWorkTogether()
    {
        // Arrange
        var touchHelper = new GestureRecognizerTestHelper(PointerType.Touch);
        var mouseHelper = new GestureRecognizerTestHelper(PointerType.Mouse);
        var penHelper = new GestureRecognizerTestHelper(PointerType.Pen);
        var border = CreateBorderWithWindow();
        var pointerTypes = new System.Collections.Generic.List<PointerType>();

        border.PointerPressed += (s, e) => pointerTypes.Add(e.Pointer.Type);

        // Act
        touchHelper.Down(border, new Point(100, 100));
        mouseHelper.Down(border, new Point(150, 150));
        penHelper.Down(border, new Point(200, 200));

        // Assert
        Assert.Equal(3, pointerTypes.Count);
        Assert.Contains(PointerType.Touch, pointerTypes);
        Assert.Contains(PointerType.Mouse, pointerTypes);
        Assert.Contains(PointerType.Pen, pointerTypes);
    }

    [AvaloniaFact]
    public void MixedPointerTypes_IndependentCapture()
    {
        // Arrange
        var touchHelper = new GestureRecognizerTestHelper(PointerType.Touch);
        var mouseHelper = new GestureRecognizerTestHelper(PointerType.Mouse);
        var border = CreateBorderWithWindow();

        // Act
        touchHelper.Down(border, new Point(100, 100));
        mouseHelper.Down(border, new Point(200, 200));
        touchHelper.Up(border);

        // Assert
        Assert.Null(touchHelper.Captured);
        Assert.Equal(border, mouseHelper.Captured);
    }

    [AvaloniaFact]
    public void MixedPointerTypes_SimultaneousDrag()
    {
        // Arrange
        var touchHelper = new GestureRecognizerTestHelper(PointerType.Touch);
        var penHelper = new GestureRecognizerTestHelper(PointerType.Pen);
        var border = CreateBorderWithWindow();
        var touchMoves = 0;
        var penMoves = 0;

        border.PointerMoved += (s, e) =>
        {
            if (e.Pointer.Type == PointerType.Touch)
                touchMoves++;
            else if (e.Pointer.Type == PointerType.Pen)
                penMoves++;
        };

        // Act
        touchHelper.Down(border, new Point(0, 0));
        penHelper.Down(border, new Point(100, 100));

        for (int i = 1; i <= 5; i++)
        {
            touchHelper.Move(border, new Point(i * 10, i * 10));
            penHelper.Move(border, new Point(100 + i * 10, 100 + i * 10));
        }

        // Assert
        Assert.Equal(5, touchMoves);
        Assert.Equal(5, penMoves);
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
        // Note: In headless tests, window cleanup is handled by the test framework
        return border;
    }

    #endregion
}
