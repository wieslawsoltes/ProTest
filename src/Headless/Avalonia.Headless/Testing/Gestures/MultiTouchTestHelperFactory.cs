// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See licence.md and NOTICE.md in the repository root for details.
using System;
using Avalonia.Interactivity;

namespace Avalonia.HeadlessTestingFramework;

/// <summary>
/// Factory class for creating paired gesture recognizer test helpers for multi-touch scenarios.
/// </summary>
public static class MultiTouchTestHelperFactory
{
    /// <summary>
    /// Creates a pair of touch helpers for two-finger gestures like pinch.
    /// </summary>
    /// <returns>A tuple containing two gesture recognizer test helpers.</returns>
    public static (GestureRecognizerTestHelper First, GestureRecognizerTestHelper Second) CreatePair()
    {
        return (new GestureRecognizerTestHelper(), new GestureRecognizerTestHelper());
    }

    /// <summary>
    /// Creates multiple touch helpers for multi-finger gestures.
    /// </summary>
    /// <param name="count">The number of helpers to create.</param>
    /// <returns>An array of gesture recognizer test helpers.</returns>
    public static GestureRecognizerTestHelper[] Create(int count)
    {
        var helpers = new GestureRecognizerTestHelper[count];
        for (int i = 0; i < count; i++)
        {
            helpers[i] = new GestureRecognizerTestHelper();
        }
        return helpers;
    }

    /// <summary>
    /// Simulates a pinch gesture using two touch helpers.
    /// </summary>
    /// <param name="target">The target control.</param>
    /// <param name="firstStart">Starting position of first finger.</param>
    /// <param name="secondStart">Starting position of second finger.</param>
    /// <param name="firstEnd">Ending position of first finger.</param>
    /// <param name="secondEnd">Ending position of second finger.</param>
    /// <param name="steps">Number of intermediate steps (must be at least 1).</param>
    /// <exception cref="ArgumentNullException">Thrown when target is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when steps is less than 1.</exception>
    public static void SimulatePinch(
        Interactive target,
        Point firstStart,
        Point secondStart,
        Point firstEnd,
        Point secondEnd,
        int steps = 10)
    {
        if (target == null)
            throw new ArgumentNullException(nameof(target));
        if (steps < 1)
            throw new ArgumentOutOfRangeException(nameof(steps), "Steps must be at least 1.");

        var (first, second) = CreatePair();

        // Start with both fingers down
        first.Down(target, firstStart);
        second.Down(target, secondStart);

        // Move both fingers
        var deltaX1 = (firstEnd.X - firstStart.X) / steps;
        var deltaY1 = (firstEnd.Y - firstStart.Y) / steps;
        var deltaX2 = (secondEnd.X - secondStart.X) / steps;
        var deltaY2 = (secondEnd.Y - secondStart.Y) / steps;

        for (int i = 1; i <= steps; i++)
        {
            var point1 = new Point(
                firstStart.X + (deltaX1 * i),
                firstStart.Y + (deltaY1 * i)
            );
            var point2 = new Point(
                secondStart.X + (deltaX2 * i),
                secondStart.Y + (deltaY2 * i)
            );

            first.Move(target, point1);
            second.Move(target, point2);
        }

        // Release both fingers
        first.Up(target, firstEnd);
        second.Up(target, secondEnd);
    }

    /// <summary>
    /// Simulates a pinch zoom in gesture (fingers moving apart).
    /// </summary>
    /// <param name="target">The target control.</param>
    /// <param name="center">Center point of the pinch.</param>
    /// <param name="startDistance">Starting distance between fingers.</param>
    /// <param name="endDistance">Ending distance between fingers.</param>
    /// <param name="steps">Number of intermediate steps (must be at least 1).</param>
    /// <exception cref="ArgumentNullException">Thrown when target is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when steps is less than 1.</exception>
    public static void SimulatePinchZoomIn(
        Interactive target,
        Point center,
        double startDistance,
        double endDistance,
        int steps = 10)
    {
        if (target == null)
            throw new ArgumentNullException(nameof(target));
        if (steps < 1)
            throw new ArgumentOutOfRangeException(nameof(steps), "Steps must be at least 1.");

        var halfStart = startDistance / 2;
        var halfEnd = endDistance / 2;

        SimulatePinch(
            target,
            new Point(center.X - halfStart, center.Y),
            new Point(center.X + halfStart, center.Y),
            new Point(center.X - halfEnd, center.Y),
            new Point(center.X + halfEnd, center.Y),
            steps);
    }

    /// <summary>
    /// Simulates a pinch zoom out gesture (fingers moving together).
    /// </summary>
    /// <param name="target">The target control.</param>
    /// <param name="center">Center point of the pinch.</param>
    /// <param name="startDistance">Starting distance between fingers.</param>
    /// <param name="endDistance">Ending distance between fingers.</param>
    /// <param name="steps">Number of intermediate steps (must be at least 1).</param>
    /// <exception cref="ArgumentNullException">Thrown when target is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when steps is less than 1.</exception>
    public static void SimulatePinchZoomOut(
        Interactive target,
        Point center,
        double startDistance,
        double endDistance,
        int steps = 10)
    {
        if (target == null)
            throw new ArgumentNullException(nameof(target));
        if (steps < 1)
            throw new ArgumentOutOfRangeException(nameof(steps), "Steps must be at least 1.");

        // For zoom out, fingers move together (from startDistance to endDistance)
        // Just call SimulatePinchZoomIn with the same parameters -
        // it handles any direction (expanding or contracting)
        SimulatePinchZoomIn(target, center, startDistance, endDistance, steps);
    }

    /// <summary>
    /// Simulates a two-finger pan gesture.
    /// </summary>
    /// <param name="target">The target control.</param>
    /// <param name="start">Starting center position.</param>
    /// <param name="end">Ending center position.</param>
    /// <param name="fingerSpacing">Distance between the two fingers.</param>
    /// <param name="steps">Number of intermediate steps.</param>
    public static void SimulateTwoFingerPan(
        Interactive target,
        Point start,
        Point end,
        double fingerSpacing = 50,
        int steps = 10)
    {
        var halfSpacing = fingerSpacing / 2;

        SimulatePinch(
            target,
            new Point(start.X - halfSpacing, start.Y),
            new Point(start.X + halfSpacing, start.Y),
            new Point(end.X - halfSpacing, end.Y),
            new Point(end.X + halfSpacing, end.Y),
            steps);
    }

    /// <summary>
    /// Simulates a rotation gesture.
    /// </summary>
    /// <param name="target">The target control.</param>
    /// <param name="center">Center point of the rotation.</param>
    /// <param name="radius">Radius from center to fingers.</param>
    /// <param name="startAngleDegrees">Starting angle in degrees.</param>
    /// <param name="endAngleDegrees">Ending angle in degrees.</param>
    /// <param name="steps">Number of intermediate steps.</param>
    public static void SimulateRotation(
        Interactive target,
        Point center,
        double radius,
        double startAngleDegrees,
        double endAngleDegrees,
        int steps = 10)
    {
        var (first, second) = CreatePair();

        var startAngleRad = startAngleDegrees * Math.PI / 180;

        // Calculate starting positions (opposite sides of circle)
        var firstStart = new Point(
            center.X + radius * Math.Cos(startAngleRad),
            center.Y + radius * Math.Sin(startAngleRad)
        );
        var secondStart = new Point(
            center.X + radius * Math.Cos(startAngleRad + Math.PI),
            center.Y + radius * Math.Sin(startAngleRad + Math.PI)
        );

        // Start with both fingers down
        first.Down(target, firstStart);
        second.Down(target, secondStart);

        var angleStep = (endAngleDegrees - startAngleDegrees) / steps;

        for (int i = 1; i <= steps; i++)
        {
            var currentAngle = startAngleDegrees + (angleStep * i);
            var currentAngleRad = currentAngle * Math.PI / 180;

            var point1 = new Point(
                center.X + radius * Math.Cos(currentAngleRad),
                center.Y + radius * Math.Sin(currentAngleRad)
            );
            var point2 = new Point(
                center.X + radius * Math.Cos(currentAngleRad + Math.PI),
                center.Y + radius * Math.Sin(currentAngleRad + Math.PI)
            );

            first.Move(target, point1);
            second.Move(target, point2);
        }

        // Calculate ending positions
        var endAngleRad = endAngleDegrees * Math.PI / 180;
        var firstEnd = new Point(
            center.X + radius * Math.Cos(endAngleRad),
            center.Y + radius * Math.Sin(endAngleRad)
        );
        var secondEnd = new Point(
            center.X + radius * Math.Cos(endAngleRad + Math.PI),
            center.Y + radius * Math.Sin(endAngleRad + Math.PI)
        );

        // Release both fingers
        first.Up(target, firstEnd);
        second.Up(target, secondEnd);
    }
}
