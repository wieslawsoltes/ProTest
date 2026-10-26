// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See licence.md and NOTICE.md in the repository root for details.
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace Avalonia.HeadlessTestingFramework.Recording;

/// <summary>
/// Integrates touch input simulation with screen recording for comprehensive visual testing.
/// Records frames during touch interactions, gestures, scrolling, and animations.
/// Also supports keyboard and mouse input simulation with recording.
/// </summary>
public class RecordedTouchSimulator : IDisposable
{
    private readonly TouchInputSimulator _touchSimulator;
    private readonly KeyboardInputSimulator _keyboardSimulator;
    private readonly MouseInputSimulator _mouseSimulator;
    private readonly HeadlessScreenRecorder _recorder;
    private TopLevel? _target;
    private bool _disposed;

    /// <summary>
    /// Gets the underlying touch input simulator.
    /// </summary>
    public TouchInputSimulator TouchSimulator => _touchSimulator;

    /// <summary>
    /// Gets the underlying keyboard input simulator.
    /// </summary>
    public KeyboardInputSimulator KeyboardSimulator => _keyboardSimulator;

    /// <summary>
    /// Gets the underlying mouse input simulator.
    /// </summary>
    public MouseInputSimulator MouseSimulator => _mouseSimulator;

    /// <summary>
    /// Gets the underlying screen recorder.
    /// </summary>
    public HeadlessScreenRecorder Recorder => _recorder;

    /// <summary>
    /// Gets whether recording is currently active.
    /// </summary>
    public bool IsRecording => _recorder.IsRecording;

    /// <summary>
    /// Gets or sets the number of frames to capture per gesture step.
    /// </summary>
    public int FramesPerStep { get; set; } = 2;

    /// <summary>
    /// Gets or sets whether to capture a frame before each gesture starts.
    /// </summary>
    public bool CaptureBeforeGesture { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to capture a frame after each gesture ends.
    /// </summary>
    public bool CaptureAfterGesture { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to use render hook for automatic frame capture.
    /// When true, frames are captured automatically when the compositor renders.
    /// </summary>
    public bool UseRenderHook
    {
        get => _recorder.UseRenderHook;
        set => _recorder.UseRenderHook = value;
    }

    /// <summary>
    /// Creates a new recorded touch simulator.
    /// </summary>
    public RecordedTouchSimulator()
    {
        _touchSimulator = new TouchInputSimulator();
        _keyboardSimulator = new KeyboardInputSimulator();
        _mouseSimulator = new MouseInputSimulator();
        _recorder = new HeadlessScreenRecorder();
        // Don't use render hook - use manual capture for reliable frame capture in tests
        _recorder.UseRenderHook = false;
    }

    /// <summary>
    /// Starts recording for the specified target.
    /// </summary>
    /// <param name="target">The top-level control to record.</param>
    /// <param name="options">Recording options.</param>
    /// <param name="outputPath">Output path for the recording.</param>
    /// <returns>The recording session.</returns>
    public RecordingSession StartRecording(TopLevel target, RecordingOptions? options = null, string? outputPath = null)
    {
        _target = target;

        // Use manual capture mode for precise control during gestures
        options ??= new RecordingOptions();
        options.AutoCapture = false;

        return _recorder.StartRecording(target, options, outputPath);
    }

    /// <summary>
    /// Stops recording and returns statistics.
    /// </summary>
    public RecordingStatistics StopRecording()
    {
        var stats = _recorder.StopRecording();
        _target = null;
        return stats;
    }

    /// <summary>
    /// Captures a frame at the current state.
    /// </summary>
    public void CaptureFrame()
    {
        if (_recorder.IsRecording)
        {
            Dispatcher.UIThread.RunJobs();
            _recorder.CaptureFrame();
        }
    }

    /// <summary>
    /// Simulates a recorded tap gesture.
    /// </summary>
    public void RecordedTap(Interactive target, Point position, int holdTime = 50)
    {
        if (CaptureBeforeGesture) CaptureFrame();

        _recorder.RecordInputEvent("Tap", new { Position = position });

        var touchId = _touchSimulator.TouchDown(target, position);
        CaptureFrame();

        _touchSimulator.AdvanceTime(holdTime);

        _touchSimulator.TouchUp(target, touchId);

        if (CaptureAfterGesture) CaptureFrame();
    }

    /// <summary>
    /// Simulates a recorded double tap gesture.
    /// </summary>
    public void RecordedDoubleTap(Interactive target, Point position, int tapInterval = 100)
    {
        if (CaptureBeforeGesture) CaptureFrame();

        _recorder.RecordInputEvent("DoubleTap", new { Position = position });

        RecordedTap(target, position);
        _touchSimulator.AdvanceTime(tapInterval);
        RecordedTap(target, position);

        if (CaptureAfterGesture) CaptureFrame();
    }

    /// <summary>
    /// Simulates a recorded drag gesture with frame capture at each step.
    /// </summary>
    public void RecordedDrag(Interactive target, Point startPoint, Point endPoint, int steps = 10)
    {
        if (CaptureBeforeGesture) CaptureFrame();

        _recorder.RecordInputEvent("Drag", new { Start = startPoint, End = endPoint, Steps = steps });

        var touchId = _touchSimulator.TouchDown(target, startPoint);
        CaptureFrame();

        var deltaX = (endPoint.X - startPoint.X) / steps;
        var deltaY = (endPoint.Y - startPoint.Y) / steps;

        for (int i = 1; i <= steps; i++)
        {
            var currentPoint = new Point(
                startPoint.X + (deltaX * i),
                startPoint.Y + (deltaY * i)
            );

            _touchSimulator.TouchMove(target, touchId, currentPoint);
            _touchSimulator.AdvanceTime(16);

            for (int f = 0; f < FramesPerStep; f++)
            {
                CaptureFrame();
            }
        }

        _touchSimulator.TouchUp(target, touchId);

        if (CaptureAfterGesture) CaptureFrame();
    }

    /// <summary>
    /// Simulates a recorded pinch zoom gesture with frame capture.
    /// </summary>
    public void RecordedPinchZoom(Interactive target, Point centerPoint, double startDistance, double endDistance, int steps = 10)
    {
        if (CaptureBeforeGesture) CaptureFrame();

        _recorder.RecordGestureEvent("PinchZoom", new
        {
            Center = centerPoint,
            StartDistance = startDistance,
            EndDistance = endDistance,
            Scale = endDistance / startDistance
        });

        var halfStartDist = startDistance / 2;
        var point1Start = new Point(centerPoint.X - halfStartDist, centerPoint.Y);
        var point2Start = new Point(centerPoint.X + halfStartDist, centerPoint.Y);

        var touch1Id = _touchSimulator.TouchDown(target, point1Start);
        var touch2Id = _touchSimulator.TouchDown(target, point2Start);
        CaptureFrame();

        var distanceStep = (endDistance - startDistance) / steps;
        var previousDistance = startDistance;

        for (int i = 1; i <= steps; i++)
        {
            var currentDistance = startDistance + (distanceStep * i);
            var halfDist = currentDistance / 2;

            var point1 = new Point(centerPoint.X - halfDist, centerPoint.Y);
            var point2 = new Point(centerPoint.X + halfDist, centerPoint.Y);

            _touchSimulator.TouchMove(target, touch1Id, point1);
            _touchSimulator.TouchMove(target, touch2Id, point2);

            // Calculate incremental scale (delta from previous step)
            // The scale represents how much the distance changed relative to the previous step
            // e.g., going from distance 50 to 60 is scale = 60/50 = 1.2 (20% increase)
            var incrementalScale = currentDistance / previousDistance;
            _touchSimulator.PinchGesture(target, incrementalScale, centerPoint, 0.0, currentDistance);
            previousDistance = currentDistance;

            _touchSimulator.AdvanceTime(16);

            // Force render update before capturing frame
            Dispatcher.UIThread.RunJobs();

            for (int f = 0; f < FramesPerStep; f++)
            {
                CaptureFrame();
            }
        }

        _touchSimulator.PinchGestureEnded(target);
        _touchSimulator.TouchUp(target, touch1Id);
        _touchSimulator.TouchUp(target, touch2Id);

        if (CaptureAfterGesture) CaptureFrame();
    }

    /// <summary>
    /// Simulates a recorded scroll gesture with frame capture.
    /// </summary>
    public void RecordedScroll(Interactive target, Vector delta, int gestureId = 1)
    {
        if (CaptureBeforeGesture) CaptureFrame();

        _recorder.RecordScrollEvent("Scroll", delta);

        _touchSimulator.ScrollGesture(target, delta, gestureId);
        CaptureFrame();

        _touchSimulator.ScrollGestureEnded(target, gestureId);

        if (CaptureAfterGesture) CaptureFrame();
    }

    /// <summary>
    /// Simulates a recorded two-finger pan gesture with frame capture.
    /// </summary>
    public void RecordedTwoFingerPan(Interactive target, Point startPoint, Point endPoint, double fingerSpacing = 50, int steps = 10)
    {
        if (CaptureBeforeGesture) CaptureFrame();

        _recorder.RecordGestureEvent("TwoFingerPan", new { Start = startPoint, End = endPoint });

        var halfSpacing = fingerSpacing / 2;
        var point1Start = new Point(startPoint.X - halfSpacing, startPoint.Y);
        var point2Start = new Point(startPoint.X + halfSpacing, startPoint.Y);

        var touch1Id = _touchSimulator.TouchDown(target, point1Start);
        var touch2Id = _touchSimulator.TouchDown(target, point2Start);
        CaptureFrame();

        var deltaX = (endPoint.X - startPoint.X) / steps;
        var deltaY = (endPoint.Y - startPoint.Y) / steps;

        for (int i = 1; i <= steps; i++)
        {
            var currentCenter = new Point(
                startPoint.X + (deltaX * i),
                startPoint.Y + (deltaY * i)
            );

            var point1 = new Point(currentCenter.X - halfSpacing, currentCenter.Y);
            var point2 = new Point(currentCenter.X + halfSpacing, currentCenter.Y);

            _touchSimulator.TouchMove(target, touch1Id, point1);
            _touchSimulator.TouchMove(target, touch2Id, point2);
            _touchSimulator.ScrollGesture(target, new Vector(deltaX, deltaY));

            _touchSimulator.AdvanceTime(16);

            for (int f = 0; f < FramesPerStep; f++)
            {
                CaptureFrame();
            }
        }

        _touchSimulator.ScrollGestureEnded(target);
        _touchSimulator.TouchUp(target, touch1Id);
        _touchSimulator.TouchUp(target, touch2Id);

        if (CaptureAfterGesture) CaptureFrame();
    }

    /// <summary>
    /// Simulates a recorded rotation gesture with frame capture.
    /// </summary>
    public void RecordedRotation(Interactive target, Point centerPoint, double radius, double startAngle, double endAngle, int steps = 10)
    {
        if (CaptureBeforeGesture) CaptureFrame();

        _recorder.RecordGestureEvent("Rotation", new
        {
            Center = centerPoint,
            StartAngle = startAngle,
            EndAngle = endAngle,
            AngleDelta = endAngle - startAngle
        });

        var startAngleRad = startAngle * Math.PI / 180;
        var point1Start = new Point(
            centerPoint.X + radius * Math.Cos(startAngleRad),
            centerPoint.Y + radius * Math.Sin(startAngleRad)
        );
        var point2Start = new Point(
            centerPoint.X + radius * Math.Cos(startAngleRad + Math.PI),
            centerPoint.Y + radius * Math.Sin(startAngleRad + Math.PI)
        );

        var touch1Id = _touchSimulator.TouchDown(target, point1Start);
        var touch2Id = _touchSimulator.TouchDown(target, point2Start);
        CaptureFrame();

        var angleStep = (endAngle - startAngle) / steps;

        for (int i = 1; i <= steps; i++)
        {
            var currentAngle = startAngle + (angleStep * i);
            var currentAngleRad = currentAngle * Math.PI / 180;

            var point1 = new Point(
                centerPoint.X + radius * Math.Cos(currentAngleRad),
                centerPoint.Y + radius * Math.Sin(currentAngleRad)
            );
            var point2 = new Point(
                centerPoint.X + radius * Math.Cos(currentAngleRad + Math.PI),
                centerPoint.Y + radius * Math.Sin(currentAngleRad + Math.PI)
            );

            _touchSimulator.TouchMove(target, touch1Id, point1);
            _touchSimulator.TouchMove(target, touch2Id, point2);

            var angleDeltaRad = angleStep * Math.PI / 180;
            _touchSimulator.PinchGesture(target, 1.0, centerPoint, angleDeltaRad, radius * 2);

            _touchSimulator.AdvanceTime(16);

            for (int f = 0; f < FramesPerStep; f++)
            {
                CaptureFrame();
            }
        }

        _touchSimulator.PinchGestureEnded(target);
        _touchSimulator.TouchUp(target, touch1Id);
        _touchSimulator.TouchUp(target, touch2Id);

        if (CaptureAfterGesture) CaptureFrame();
    }

    /// <summary>
    /// Simulates a recorded swipe gesture with frame capture.
    /// </summary>
    public void RecordedSwipe(Interactive target, Point startPoint, SwipeDirection direction, double distance = 100, int duration = 200)
    {
        var endPoint = direction switch
        {
            SwipeDirection.Left => new Point(startPoint.X - distance, startPoint.Y),
            SwipeDirection.Right => new Point(startPoint.X + distance, startPoint.Y),
            SwipeDirection.Up => new Point(startPoint.X, startPoint.Y - distance),
            SwipeDirection.Down => new Point(startPoint.X, startPoint.Y + distance),
            _ => startPoint
        };

        if (CaptureBeforeGesture) CaptureFrame();

        _recorder.RecordGestureEvent("Swipe", new
        {
            Direction = direction.ToString(),
            Distance = distance,
            Start = startPoint,
            End = endPoint
        });

        var steps = (int)(duration / 16.0);
        RecordedDrag(target, startPoint, endPoint, Math.Max(1, steps));
    }

    /// <summary>
    /// Simulates a recorded touchpad magnify gesture with frame capture.
    /// </summary>
    public void RecordedTouchpadMagnify(Interactive target, Vector delta, Point position, KeyModifiers modifiers = KeyModifiers.None)
    {
        if (CaptureBeforeGesture) CaptureFrame();

        _recorder.RecordGestureEvent("TouchpadMagnify", new { Delta = delta, Position = position });

        _touchSimulator.TouchpadMagnify(target, delta, position, modifiers);

        if (CaptureAfterGesture) CaptureFrame();
    }

    /// <summary>
    /// Records frames during an async action.
    /// </summary>
    /// <param name="action">The action to record.</param>
    /// <param name="frameInterval">Interval between frames in milliseconds.</param>
    public async Task RecordActionAsync(Func<Task> action, int frameInterval = 16)
    {
        _recorder.RecordEvent(RecordingEventType.Generic, "RecordAction", new { FrameInterval = frameInterval });
        await _recorder.RecordActionAsync(action, frameInterval);
    }

    /// <summary>
    /// Records frames for a specified duration while allowing animations to run.
    /// </summary>
    /// <param name="duration">Duration to record.</param>
    /// <param name="frameInterval">Interval between frames in milliseconds.</param>
    public async Task RecordAnimationAsync(TimeSpan duration, int frameInterval = 16)
    {
        _recorder.RecordAnimationEvent("RecordAnimation", new { Duration = duration.TotalMilliseconds });
        await _recorder.RecordDurationAsync(duration, frameInterval);
    }

    /// <summary>
    /// Adds a marker to the recording.
    /// </summary>
    /// <param name="name">Marker name.</param>
    /// <param name="data">Optional data.</param>
    public void AddMarker(string name, object? data = null)
    {
        _recorder.AddMarker(name, data);
    }

    /// <summary>
    /// Converts the current recording session's PNG output to a video file.
    /// </summary>
    /// <param name="outputPath">Output video file path. If null, uses the recording directory.</param>
    /// <param name="options">Video conversion options.</param>
    /// <returns>The conversion result.</returns>
    public VideoConversionResult ConvertToVideo(string? outputPath = null, VideoConversionOptions? options = null)
    {
        var session = _recorder.CurrentSession;
        if (session == null)
        {
            return new VideoConversionResult
            {
                Success = false,
                ErrorMessage = "No recording session available."
            };
        }

        return session.ToVideo(outputPath, options);
    }

    /// <summary>
    /// Converts the current recording session's PNG output to a video file asynchronously.
    /// </summary>
    /// <param name="outputPath">Output video file path. If null, uses the recording directory.</param>
    /// <param name="options">Video conversion options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The conversion result.</returns>
    public async Task<VideoConversionResult> ConvertToVideoAsync(
        string? outputPath = null,
        VideoConversionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var session = _recorder.CurrentSession;
        if (session == null)
        {
            return new VideoConversionResult
            {
                Success = false,
                ErrorMessage = "No recording session available."
            };
        }

        return await session.ToVideoAsync(outputPath, options, cancellationToken);
    }

    /// <summary>
    /// Stops recording and converts to video.
    /// </summary>
    /// <param name="videoOptions">Video conversion options.</param>
    /// <returns>A tuple containing the recording statistics and video conversion result.</returns>
    public (RecordingStatistics Stats, VideoConversionResult VideoResult) StopRecordingAndConvertToVideo(
        VideoConversionOptions? videoOptions = null)
    {
        var session = _recorder.CurrentSession;
        var stats = StopRecording();

        if (session == null)
        {
            return (stats, new VideoConversionResult
            {
                Success = false,
                ErrorMessage = "No recording session available."
            });
        }

        var videoResult = session.ToVideo(null, videoOptions);
        return (stats, videoResult);
    }

    #region Recorded Keyboard Input

    /// <summary>
    /// Simulates a recorded key press with frame capture.
    /// </summary>
    public void RecordedKeyPress(Interactive target, Key key, int holdTime = 50, KeyModifiers? modifiers = null)
    {
        if (CaptureBeforeGesture) CaptureFrame();

        _recorder.RecordInputEvent("KeyPress", new { Key = key.ToString(), Modifiers = modifiers?.ToString() });

        _keyboardSimulator.KeyPress(target, key, holdTime, modifiers);
        CaptureFrame();

        if (CaptureAfterGesture) CaptureFrame();
    }

    /// <summary>
    /// Simulates recorded text typing with frame capture.
    /// </summary>
    public void RecordedTypeText(Interactive target, string text, int keyInterval = 50)
    {
        if (CaptureBeforeGesture) CaptureFrame();

        _recorder.RecordInputEvent("TypeText", new { Text = text, Length = text.Length });

        foreach (char c in text)
        {
            var key = CharToKeySimple(c);
            _keyboardSimulator.KeyPress(target, key, keyInterval / 2);

            // Capture frame periodically (every few characters)
            if (text.IndexOf(c) % 3 == 0)
            {
                CaptureFrame();
            }

            _keyboardSimulator.AdvanceTime(keyInterval);
        }

        if (CaptureAfterGesture) CaptureFrame();
    }

    /// <summary>
    /// Simulates a recorded keyboard shortcut with frame capture.
    /// </summary>
    public void RecordedKeyboardShortcut(Interactive target, Key key, KeyModifiers modifiers)
    {
        if (CaptureBeforeGesture) CaptureFrame();

        _recorder.RecordInputEvent("KeyboardShortcut", new { Key = key.ToString(), Modifiers = modifiers.ToString() });

        // Press modifier keys
        if (modifiers.HasFlag(KeyModifiers.Control))
            _keyboardSimulator.KeyDown(target, Key.LeftCtrl);
        if (modifiers.HasFlag(KeyModifiers.Shift))
            _keyboardSimulator.KeyDown(target, Key.LeftShift);
        if (modifiers.HasFlag(KeyModifiers.Alt))
            _keyboardSimulator.KeyDown(target, Key.LeftAlt);
        if (modifiers.HasFlag(KeyModifiers.Meta))
            _keyboardSimulator.KeyDown(target, Key.LWin);

        CaptureFrame();

        // Press the key
        _keyboardSimulator.KeyPress(target, key, modifiers: modifiers);
        CaptureFrame();

        // Release modifier keys
        if (modifiers.HasFlag(KeyModifiers.Meta))
            _keyboardSimulator.KeyUp(target, Key.LWin);
        if (modifiers.HasFlag(KeyModifiers.Alt))
            _keyboardSimulator.KeyUp(target, Key.LeftAlt);
        if (modifiers.HasFlag(KeyModifiers.Shift))
            _keyboardSimulator.KeyUp(target, Key.LeftShift);
        if (modifiers.HasFlag(KeyModifiers.Control))
            _keyboardSimulator.KeyUp(target, Key.LeftCtrl);

        if (CaptureAfterGesture) CaptureFrame();
    }

    /// <summary>
    /// Simulates recorded arrow key navigation with frame capture.
    /// </summary>
    public void RecordedArrowNavigation(Interactive target, Key[] keys, int keyInterval = 100)
    {
        if (CaptureBeforeGesture) CaptureFrame();

        _recorder.RecordInputEvent("ArrowNavigation", new { Keys = string.Join(",", keys.Select(k => k.ToString())), Count = keys.Length });

        foreach (var key in keys)
        {
            _keyboardSimulator.KeyPress(target, key);
            _keyboardSimulator.AdvanceTime(keyInterval);
            CaptureFrame();
        }

        if (CaptureAfterGesture) CaptureFrame();
    }

    /// <summary>
    /// Simulates recorded Tab navigation with frame capture.
    /// </summary>
    public void RecordedTabNavigation(Interactive target, int tabCount, bool reverse = false, int tabInterval = 100)
    {
        if (CaptureBeforeGesture) CaptureFrame();

        _recorder.RecordInputEvent("TabNavigation", new { TabCount = tabCount, Reverse = reverse });

        for (int i = 0; i < tabCount; i++)
        {
            if (reverse)
            {
                _keyboardSimulator.ShiftDown(target);
                _keyboardSimulator.KeyPress(target, Key.Tab, modifiers: KeyModifiers.Shift);
                _keyboardSimulator.ShiftUp(target);
            }
            else
            {
                _keyboardSimulator.KeyPress(target, Key.Tab);
            }

            _keyboardSimulator.AdvanceTime(tabInterval);
            CaptureFrame();
        }

        if (CaptureAfterGesture) CaptureFrame();
    }

    private static Key CharToKeySimple(char c)
    {
        if (char.IsLetter(c))
            return (Key)((int)Key.A + (char.ToUpperInvariant(c) - 'A'));
        if (char.IsDigit(c))
            return (Key)((int)Key.D0 + (c - '0'));
        return c switch
        {
            ' ' => Key.Space,
            '\n' or '\r' => Key.Enter,
            '\t' => Key.Tab,
            _ => Key.None
        };
    }

    #endregion

    #region Recorded Mouse Input

    /// <summary>
    /// Simulates a recorded mouse click with frame capture.
    /// </summary>
    public void RecordedMouseClick(Interactive target, Point position, MouseButton button = MouseButton.Left, int holdTime = 50)
    {
        if (CaptureBeforeGesture) CaptureFrame();

        _recorder.RecordInputEvent("MouseClick", new { Position = position, Button = button.ToString() });

        _mouseSimulator.Click(target, position, button, holdTime);
        CaptureFrame();

        if (CaptureAfterGesture) CaptureFrame();
    }

    /// <summary>
    /// Simulates a recorded mouse double-click with frame capture.
    /// </summary>
    public void RecordedMouseDoubleClick(Interactive target, Point position, MouseButton button = MouseButton.Left, int clickInterval = 100)
    {
        if (CaptureBeforeGesture) CaptureFrame();

        _recorder.RecordInputEvent("MouseDoubleClick", new { Position = position, Button = button.ToString() });

        _mouseSimulator.DoubleClick(target, position, button, clickInterval);
        CaptureFrame();

        if (CaptureAfterGesture) CaptureFrame();
    }

    /// <summary>
    /// Simulates a recorded mouse right-click with frame capture.
    /// </summary>
    public void RecordedMouseRightClick(Interactive target, Point position, int holdTime = 50)
    {
        if (CaptureBeforeGesture) CaptureFrame();

        _recorder.RecordInputEvent("MouseRightClick", new { Position = position });

        _mouseSimulator.RightClick(target, position, holdTime);
        CaptureFrame();

        if (CaptureAfterGesture) CaptureFrame();
    }

    /// <summary>
    /// Simulates a recorded mouse drag with frame capture.
    /// </summary>
    public void RecordedMouseDrag(Interactive target, Point startPoint, Point endPoint, MouseButton button = MouseButton.Left, int steps = 10)
    {
        if (CaptureBeforeGesture) CaptureFrame();

        _recorder.RecordInputEvent("MouseDrag", new { Start = startPoint, End = endPoint, Button = button.ToString() });

        _mouseSimulator.ButtonDown(target, button, startPoint);
        CaptureFrame();

        var deltaX = (endPoint.X - startPoint.X) / steps;
        var deltaY = (endPoint.Y - startPoint.Y) / steps;

        for (int i = 1; i <= steps; i++)
        {
            var currentPoint = new Point(
                startPoint.X + (deltaX * i),
                startPoint.Y + (deltaY * i)
            );
            _mouseSimulator.MoveTo(target, currentPoint);
            _mouseSimulator.AdvanceTime(16);

            for (int f = 0; f < FramesPerStep; f++)
            {
                CaptureFrame();
            }
        }

        _mouseSimulator.ButtonUp(target, button, endPoint);

        if (CaptureAfterGesture) CaptureFrame();
    }

    /// <summary>
    /// Simulates a recorded mouse wheel scroll with frame capture.
    /// </summary>
    public void RecordedMouseWheel(Interactive target, Vector delta, Point? position = null, int steps = 5)
    {
        if (CaptureBeforeGesture) CaptureFrame();

        _recorder.RecordInputEvent("MouseWheel", new { Delta = delta, Position = position });

        if (position.HasValue)
        {
            _mouseSimulator.MoveTo(target, position.Value);
        }

        var stepDelta = new Vector(delta.X / steps, delta.Y / steps);

        for (int i = 0; i < steps; i++)
        {
            _mouseSimulator.Wheel(target, stepDelta, position);
            _mouseSimulator.AdvanceTime(16);
            CaptureFrame();
        }

        if (CaptureAfterGesture) CaptureFrame();
    }

    /// <summary>
    /// Simulates a recorded mouse hover with frame capture.
    /// </summary>
    public void RecordedMouseHover(Interactive target, Point position, int duration = 500)
    {
        if (CaptureBeforeGesture) CaptureFrame();

        _recorder.RecordInputEvent("MouseHover", new { Position = position, Duration = duration });

        _mouseSimulator.Enter(target, position);
        CaptureFrame();

        // Capture frames during hover
        var frameCount = duration / 100;
        for (int i = 0; i < frameCount; i++)
        {
            _mouseSimulator.AdvanceTime(100);
            CaptureFrame();
        }

        if (CaptureAfterGesture) CaptureFrame();
    }

    /// <summary>
    /// Simulates a recorded mouse move path with frame capture.
    /// </summary>
    public void RecordedMouseMovePath(Interactive target, Point[] path, int stepInterval = 50)
    {
        if (path.Length < 2)
            throw new ArgumentException("Path must contain at least 2 points.", nameof(path));

        if (CaptureBeforeGesture) CaptureFrame();

        _recorder.RecordInputEvent("MouseMovePath", new { PathLength = path.Length });

        _mouseSimulator.Enter(target, path[0]);
        CaptureFrame();

        for (int i = 1; i < path.Length; i++)
        {
            _mouseSimulator.MoveTo(target, path[i]);
            _mouseSimulator.AdvanceTime(stepInterval);
            CaptureFrame();
        }

        if (CaptureAfterGesture) CaptureFrame();
    }

    #endregion

    /// <summary>
    /// Disposes the simulator and recorder.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _recorder.Dispose();
        GC.SuppressFinalize(this);
    }
}
