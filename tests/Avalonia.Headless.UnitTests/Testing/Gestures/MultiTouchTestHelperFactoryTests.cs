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
/// Comprehensive tests for MultiTouchTestHelperFactory API.
/// </summary>
public class MultiTouchTestHelperFactoryTests
{
    #region CreatePair Tests

    [AvaloniaFact]
    public void CreatePair_ShouldReturnTwoDistinctHelpers()
    {
        // Act
        var (first, second) = MultiTouchTestHelperFactory.CreatePair();

        // Assert
        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.NotSame(first, second);
    }

    [AvaloniaFact]
    public void CreatePair_ShouldReturnHelpersWithDistinctPointers()
    {
        // Act
        var (first, second) = MultiTouchTestHelperFactory.CreatePair();

        // Assert
        Assert.NotEqual(first.Pointer.Id, second.Pointer.Id);
    }

    [AvaloniaFact]
    public void CreatePair_BothShouldBeTouchPointers()
    {
        // Act
        var (first, second) = MultiTouchTestHelperFactory.CreatePair();

        // Assert
        Assert.Equal(PointerType.Touch, first.Pointer.Type);
        Assert.Equal(PointerType.Touch, second.Pointer.Type);
    }

    #endregion

    #region Create Tests

    [AvaloniaFact]
    public void Create_WithZero_ShouldReturnEmptyArray()
    {
        // Act
        var helpers = MultiTouchTestHelperFactory.Create(0);

        // Assert
        Assert.Empty(helpers);
    }

    [AvaloniaFact]
    public void Create_WithOne_ShouldReturnSingleHelper()
    {
        // Act
        var helpers = MultiTouchTestHelperFactory.Create(1);

        // Assert
        Assert.Single(helpers);
        Assert.NotNull(helpers[0]);
    }

    [AvaloniaFact]
    public void Create_WithMultiple_ShouldReturnCorrectCount()
    {
        // Act
        var helpers = MultiTouchTestHelperFactory.Create(5);

        // Assert
        Assert.Equal(5, helpers.Length);
        foreach (var helper in helpers)
        {
            Assert.NotNull(helper);
        }
    }

    [AvaloniaFact]
    public void Create_AllHelpersShouldHaveDistinctPointers()
    {
        // Act
        var helpers = MultiTouchTestHelperFactory.Create(4);

        // Assert
        var pointerIds = new System.Collections.Generic.HashSet<int>();
        foreach (var helper in helpers)
        {
            Assert.True(pointerIds.Add(helper.Pointer.Id), "Duplicate pointer ID found");
        }
    }

    [AvaloniaFact]
    public void Create_AllShouldBeTouchPointers()
    {
        // Act
        var helpers = MultiTouchTestHelperFactory.Create(3);

        // Assert
        foreach (var helper in helpers)
        {
            Assert.Equal(PointerType.Touch, helper.Pointer.Type);
        }
    }

    #endregion

    #region SimulatePinch Tests

    [AvaloniaFact]
    public void SimulatePinch_ShouldRaiseTwoPointerPressed()
    {
        // Arrange
        var border = CreateBorderWithWindow();
        var pressCount = 0;

        border.PointerPressed += (s, e) => pressCount++;

        // Act
        MultiTouchTestHelperFactory.SimulatePinch(
            border,
            new Point(100, 150),
            new Point(200, 150),
            new Point(50, 150),
            new Point(250, 150),
            steps: 5);

        // Assert
        Assert.Equal(2, pressCount);
    }

    [AvaloniaFact]
    public void SimulatePinch_ShouldRaiseTwoPointerReleased()
    {
        // Arrange
        var border = CreateBorderWithWindow();
        var releaseCount = 0;

        border.PointerReleased += (s, e) => releaseCount++;

        // Act
        MultiTouchTestHelperFactory.SimulatePinch(
            border,
            new Point(100, 150),
            new Point(200, 150),
            new Point(50, 150),
            new Point(250, 150),
            steps: 5);

        // Assert
        Assert.Equal(2, releaseCount);
    }

    [AvaloniaFact]
    public void SimulatePinch_ShouldRaiseMoveEventsForBothFingers()
    {
        // Arrange
        var border = CreateBorderWithWindow();
        var moveCount = 0;

        border.PointerMoved += (s, e) => moveCount++;

        // Act
        MultiTouchTestHelperFactory.SimulatePinch(
            border,
            new Point(100, 150),
            new Point(200, 150),
            new Point(50, 150),
            new Point(250, 150),
            steps: 5);

        // Assert - 2 fingers * 5 steps = 10 move events
        Assert.Equal(10, moveCount);
    }

    [AvaloniaFact]
    public void SimulatePinch_WithSingleStep_ShouldRaiseTwoMoveEvents()
    {
        // Arrange
        var border = CreateBorderWithWindow();
        var moveCount = 0;

        border.PointerMoved += (s, e) => moveCount++;

        // Act
        MultiTouchTestHelperFactory.SimulatePinch(
            border,
            new Point(100, 150),
            new Point(200, 150),
            new Point(50, 150),
            new Point(250, 150),
            steps: 1);

        // Assert - 2 fingers * 1 step = 2 move events
        Assert.Equal(2, moveCount);
    }

    #endregion

    #region SimulatePinchZoomIn Tests

    [AvaloniaFact]
    public void SimulatePinchZoomIn_ShouldMoveFingersFartherApart()
    {
        // Arrange
        var border = CreateBorderWithWindow();
        var firstPositions = new System.Collections.Generic.List<Point>();
        var secondPositions = new System.Collections.Generic.List<Point>();
        var pointersSeen = new System.Collections.Generic.HashSet<int>();

        border.PointerMoved += (s, e) =>
        {
            if (!pointersSeen.Contains(e.Pointer.Id))
            {
                pointersSeen.Add(e.Pointer.Id);
                firstPositions.Add(e.GetPosition(border));
            }
            else
            {
                secondPositions.Add(e.GetPosition(border));
            }
        };

        // Act
        MultiTouchTestHelperFactory.SimulatePinchZoomIn(
            border,
            center: new Point(200, 150),
            startDistance: 100,
            endDistance: 200,
            steps: 3);

        // Assert - fingers should move apart (increasing distance from center)
        Assert.NotEmpty(firstPositions);
    }

    [AvaloniaFact]
    public void SimulatePinchZoomIn_ShouldCompleteFullGesture()
    {
        // Arrange
        var border = CreateBorderWithWindow();
        var pressCount = 0;
        var releaseCount = 0;

        border.PointerPressed += (s, e) => pressCount++;
        border.PointerReleased += (s, e) => releaseCount++;

        // Act
        MultiTouchTestHelperFactory.SimulatePinchZoomIn(
            border,
            center: new Point(200, 150),
            startDistance: 50,
            endDistance: 150,
            steps: 5);

        // Assert
        Assert.Equal(2, pressCount);
        Assert.Equal(2, releaseCount);
    }

    #endregion

    #region SimulatePinchZoomOut Tests

    [AvaloniaFact]
    public void SimulatePinchZoomOut_ShouldCompleteFullGesture()
    {
        // Arrange
        var border = CreateBorderWithWindow();
        var pressCount = 0;
        var releaseCount = 0;

        border.PointerPressed += (s, e) => pressCount++;
        border.PointerReleased += (s, e) => releaseCount++;

        // Act
        MultiTouchTestHelperFactory.SimulatePinchZoomOut(
            border,
            center: new Point(200, 150),
            startDistance: 150,
            endDistance: 50,
            steps: 5);

        // Assert
        Assert.Equal(2, pressCount);
        Assert.Equal(2, releaseCount);
    }

    [AvaloniaFact]
    public void SimulatePinchZoomOut_ShouldMoveFingersCloser()
    {
        // Arrange
        var border = CreateBorderWithWindow();
        var moveCount = 0;

        border.PointerMoved += (s, e) => moveCount++;

        // Act
        MultiTouchTestHelperFactory.SimulatePinchZoomOut(
            border,
            center: new Point(200, 150),
            startDistance: 200,
            endDistance: 50,
            steps: 4);

        // Assert - should have move events
        Assert.Equal(8, moveCount); // 2 fingers * 4 steps
    }

    #endregion

    #region SimulateTwoFingerPan Tests

    [AvaloniaFact]
    public void SimulateTwoFingerPan_ShouldCompleteFullGesture()
    {
        // Arrange
        var border = CreateBorderWithWindow();
        var pressCount = 0;
        var releaseCount = 0;

        border.PointerPressed += (s, e) => pressCount++;
        border.PointerReleased += (s, e) => releaseCount++;

        // Act
        MultiTouchTestHelperFactory.SimulateTwoFingerPan(
            border,
            start: new Point(100, 100),
            end: new Point(200, 150),
            fingerSpacing: 50,
            steps: 5);

        // Assert
        Assert.Equal(2, pressCount);
        Assert.Equal(2, releaseCount);
    }

    [AvaloniaFact]
    public void SimulateTwoFingerPan_ShouldMaintainFingerSpacing()
    {
        // Arrange
        var border = CreateBorderWithWindow();
        var pointerPositions = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<Point>>();

        border.PointerPressed += (s, e) =>
        {
            pointerPositions[e.Pointer.Id] = new System.Collections.Generic.List<Point> { e.GetPosition(border) };
        };

        border.PointerMoved += (s, e) =>
        {
            if (pointerPositions.TryGetValue(e.Pointer.Id, out var list))
                list.Add(e.GetPosition(border));
        };

        // Act
        MultiTouchTestHelperFactory.SimulateTwoFingerPan(
            border,
            start: new Point(100, 100),
            end: new Point(200, 100),
            fingerSpacing: 60,
            steps: 2);

        // Assert - two fingers tracked
        Assert.Equal(2, pointerPositions.Count);
    }

    [AvaloniaFact]
    public void SimulateTwoFingerPan_DefaultFingerSpacing_ShouldBe50()
    {
        // Arrange
        var border = CreateBorderWithWindow();
        var startPositions = new System.Collections.Generic.List<Point>();

        border.PointerPressed += (s, e) =>
        {
            startPositions.Add(e.GetPosition(border));
        };

        // Act
        MultiTouchTestHelperFactory.SimulateTwoFingerPan(
            border,
            start: new Point(100, 100),
            end: new Point(200, 100),
            steps: 1);

        // Assert - default spacing is 50, so fingers at 75 and 125 (100 ± 25)
        Assert.Equal(2, startPositions.Count);
        var distance = Math.Abs(startPositions[0].X - startPositions[1].X);
        Assert.Equal(50, distance, precision: 1);
    }

    #endregion

    #region SimulateRotation Tests

    [AvaloniaFact]
    public void SimulateRotation_ShouldCompleteFullGesture()
    {
        // Arrange
        var border = CreateBorderWithWindow();
        var pressCount = 0;
        var releaseCount = 0;

        border.PointerPressed += (s, e) => pressCount++;
        border.PointerReleased += (s, e) => releaseCount++;

        // Act
        MultiTouchTestHelperFactory.SimulateRotation(
            border,
            center: new Point(200, 150),
            radius: 50,
            startAngleDegrees: 0,
            endAngleDegrees: 90,
            steps: 5);

        // Assert
        Assert.Equal(2, pressCount);
        Assert.Equal(2, releaseCount);
    }

    [AvaloniaFact]
    public void SimulateRotation_ShouldRaiseMoveEventsForBothFingers()
    {
        // Arrange
        var border = CreateBorderWithWindow();
        var moveCount = 0;

        border.PointerMoved += (s, e) => moveCount++;

        // Act
        MultiTouchTestHelperFactory.SimulateRotation(
            border,
            center: new Point(200, 150),
            radius: 50,
            startAngleDegrees: 0,
            endAngleDegrees: 45,
            steps: 4);

        // Assert - 2 fingers * 4 steps = 8 move events
        Assert.Equal(8, moveCount);
    }

    [AvaloniaFact]
    public void SimulateRotation_FingersOnOppositeSidesOfCircle()
    {
        // Arrange
        var border = CreateBorderWithWindow();
        var startPositions = new System.Collections.Generic.List<Point>();
        var center = new Point(200, 150);
        var radius = 50.0;

        border.PointerPressed += (s, e) =>
        {
            startPositions.Add(e.GetPosition(border));
        };

        // Act
        MultiTouchTestHelperFactory.SimulateRotation(
            border,
            center: center,
            radius: radius,
            startAngleDegrees: 0,
            endAngleDegrees: 90,
            steps: 1);

        // Assert - fingers should be on opposite sides (180 degrees apart)
        Assert.Equal(2, startPositions.Count);

        // Calculate distance between fingers - should be approximately 2 * radius
        var distance = Math.Sqrt(
            Math.Pow(startPositions[0].X - startPositions[1].X, 2) +
            Math.Pow(startPositions[0].Y - startPositions[1].Y, 2));
        Assert.Equal(2 * radius, distance, precision: 1);
    }

    [AvaloniaFact]
    public void SimulateRotation_CounterClockwise_ShouldWork()
    {
        // Arrange
        var border = CreateBorderWithWindow();
        var moveCount = 0;

        border.PointerMoved += (s, e) => moveCount++;

        // Act - negative rotation (counterclockwise)
        MultiTouchTestHelperFactory.SimulateRotation(
            border,
            center: new Point(200, 150),
            radius: 50,
            startAngleDegrees: 90,
            endAngleDegrees: 0,
            steps: 3);

        // Assert
        Assert.Equal(6, moveCount); // 2 fingers * 3 steps
    }

    #endregion

    #region Integration Tests

    [AvaloniaFact]
    public void CombinedGestures_SequentialPinchAndPan()
    {
        // Arrange
        var border = CreateBorderWithWindow();
        var totalPresses = 0;
        var totalReleases = 0;

        border.PointerPressed += (s, e) => totalPresses++;
        border.PointerReleased += (s, e) => totalReleases++;

        // Act - pinch then pan
        MultiTouchTestHelperFactory.SimulatePinchZoomIn(
            border,
            center: new Point(200, 150),
            startDistance: 50,
            endDistance: 100,
            steps: 2);

        MultiTouchTestHelperFactory.SimulateTwoFingerPan(
            border,
            start: new Point(200, 150),
            end: new Point(300, 200),
            steps: 2);

        // Assert - 2 gestures * 2 fingers = 4 presses and 4 releases
        Assert.Equal(4, totalPresses);
        Assert.Equal(4, totalReleases);
    }

    [AvaloniaFact]
    public void ManualPinch_UsingCreatePair()
    {
        // Arrange
        var border = CreateBorderWithWindow();
        var pressedPointers = new System.Collections.Generic.HashSet<int>();
        var movedPointers = new System.Collections.Generic.HashSet<int>();
        var releasedPointers = new System.Collections.Generic.HashSet<int>();

        border.PointerPressed += (s, e) => pressedPointers.Add(e.Pointer.Id);
        border.PointerMoved += (s, e) => movedPointers.Add(e.Pointer.Id);
        border.PointerReleased += (s, e) => releasedPointers.Add(e.Pointer.Id);

        // Act - manual pinch using CreatePair
        var (first, second) = MultiTouchTestHelperFactory.CreatePair();

        first.Down(border, new Point(150, 150));
        second.Down(border, new Point(250, 150));

        first.Move(border, new Point(100, 150));
        second.Move(border, new Point(300, 150));

        first.Up(border, new Point(100, 150));
        second.Up(border, new Point(300, 150));

        // Assert
        Assert.Equal(2, pressedPointers.Count);
        Assert.Equal(2, movedPointers.Count);
        Assert.Equal(2, releasedPointers.Count);
    }

    [AvaloniaFact]
    public void ManualGesture_UsingCreateWithMultipleFingers()
    {
        // Arrange
        var border = CreateBorderWithWindow();
        var pressCount = 0;

        border.PointerPressed += (s, e) => pressCount++;

        // Act - 4-finger gesture
        var helpers = MultiTouchTestHelperFactory.Create(4);

        helpers[0].Down(border, new Point(100, 100));
        helpers[1].Down(border, new Point(200, 100));
        helpers[2].Down(border, new Point(100, 200));
        helpers[3].Down(border, new Point(200, 200));

        // Assert
        Assert.Equal(4, pressCount);
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
