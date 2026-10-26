// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See licence.md and NOTICE.md in the repository root for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;

namespace Avalonia.HeadlessTestingFramework.Appium;

/// <summary>
/// Manages test session state and provides session-level operations.
/// Inspired by Appium's session management.
/// </summary>
/// <example>
/// <code>
/// // Create a session with capabilities
/// var session = new DriverSession(driver, new SessionCapabilities
/// {
///     ImplicitWait = TimeSpan.FromSeconds(5),
///     PageLoadTimeout = TimeSpan.FromSeconds(30)
/// });
///
/// // Store session data
/// session.SetData("user", "testuser");
/// var user = session.GetData&lt;string&gt;("user");
///
/// // Reset session
/// session.Reset();
/// </code>
/// </example>
public class DriverSession : IDisposable
{
    private readonly AvaloniaDriver _driver;
    private readonly Dictionary<string, object> _sessionData = new();
    private readonly List<Action> _cleanupActions = new();
    private bool _disposed;

    /// <summary>
    /// Gets the session ID.
    /// </summary>
    public string SessionId { get; }

    /// <summary>
    /// Gets the session capabilities.
    /// </summary>
    public SessionCapabilities Capabilities { get; }

    /// <summary>
    /// Gets the session start time.
    /// </summary>
    public DateTime StartTime { get; }

    /// <summary>
    /// Gets the session duration.
    /// </summary>
    public TimeSpan Duration => DateTime.UtcNow - StartTime;

    /// <summary>
    /// Gets the associated driver.
    /// </summary>
    public AvaloniaDriver Driver => _driver;

    /// <summary>
    /// Creates a new driver session.
    /// </summary>
    /// <param name="driver">The driver instance.</param>
    /// <param name="capabilities">Optional capabilities.</param>
    public DriverSession(AvaloniaDriver driver, SessionCapabilities? capabilities = null)
    {
        _driver = driver ?? throw new ArgumentNullException(nameof(driver));
        SessionId = Guid.NewGuid().ToString("N");
        Capabilities = capabilities ?? new SessionCapabilities();
        StartTime = DateTime.UtcNow;

        ApplyCapabilities();
    }

    #region Session Data

    /// <summary>
    /// Stores data in the session.
    /// </summary>
    /// <param name="key">The data key.</param>
    /// <param name="value">The data value.</param>
    public void SetData(string key, object value)
    {
        _sessionData[key] = value;
    }

    /// <summary>
    /// Gets data from the session.
    /// </summary>
    /// <typeparam name="T">The data type.</typeparam>
    /// <param name="key">The data key.</param>
    /// <returns>The data value, or default if not found.</returns>
    public T? GetData<T>(string key)
    {
        if (_sessionData.TryGetValue(key, out var value) && value is T typed)
            return typed;
        return default;
    }

    /// <summary>
    /// Gets data from the session with a default value.
    /// </summary>
    /// <typeparam name="T">The data type.</typeparam>
    /// <param name="key">The data key.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The data value, or the default if not found.</returns>
    public T GetData<T>(string key, T defaultValue)
    {
        if (_sessionData.TryGetValue(key, out var value) && value is T typed)
            return typed;
        return defaultValue;
    }

    /// <summary>
    /// Removes data from the session.
    /// </summary>
    /// <param name="key">The data key.</param>
    /// <returns>True if data was removed.</returns>
    public bool RemoveData(string key)
    {
        return _sessionData.Remove(key);
    }

    /// <summary>
    /// Checks if the session has specific data.
    /// </summary>
    /// <param name="key">The data key.</param>
    /// <returns>True if data exists.</returns>
    public bool HasData(string key)
    {
        return _sessionData.ContainsKey(key);
    }

    /// <summary>
    /// Gets all session data keys.
    /// </summary>
    public IEnumerable<string> DataKeys => _sessionData.Keys;

    /// <summary>
    /// Clears all session data.
    /// </summary>
    public void ClearData()
    {
        _sessionData.Clear();
    }

    #endregion

    #region Cleanup Management

    /// <summary>
    /// Registers a cleanup action to run on session end.
    /// </summary>
    /// <param name="cleanup">The cleanup action.</param>
    public void RegisterCleanup(Action cleanup)
    {
        _cleanupActions.Add(cleanup);
    }

    /// <summary>
    /// Runs all registered cleanup actions.
    /// </summary>
    public void RunCleanup()
    {
        foreach (var cleanup in _cleanupActions)
        {
            try
            {
                cleanup();
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
        _cleanupActions.Clear();
    }

    #endregion

    #region Session Operations

    /// <summary>
    /// Resets the session to initial state.
    /// </summary>
    public void Reset()
    {
        RunCleanup();
        ClearData();
        Dispatcher.UIThread.RunJobs();
    }

    /// <summary>
    /// Gets session information as a dictionary.
    /// </summary>
    /// <returns>Session information.</returns>
    public Dictionary<string, object> GetSessionInfo()
    {
        return new Dictionary<string, object>
        {
            ["sessionId"] = SessionId,
            ["startTime"] = StartTime,
            ["duration"] = Duration,
            ["capabilities"] = Capabilities,
            ["dataCount"] = _sessionData.Count,
            ["cleanupCount"] = _cleanupActions.Count
        };
    }

    private void ApplyCapabilities()
    {
        if (Capabilities.ImplicitWait.HasValue)
        {
            _driver.ImplicitWait = Capabilities.ImplicitWait.Value;
        }
    }

    #endregion

    #region IDisposable

    /// <summary>
    /// Disposes the session.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            RunCleanup();
            ClearData();
            _disposed = true;
        }
    }

    #endregion
}

/// <summary>
/// Session capabilities for configuring driver behavior.
/// </summary>
public class SessionCapabilities
{
    /// <summary>
    /// Gets or sets the implicit wait timeout.
    /// </summary>
    public TimeSpan? ImplicitWait { get; set; }

    /// <summary>
    /// Gets or sets the page load timeout.
    /// </summary>
    public TimeSpan? PageLoadTimeout { get; set; }

    /// <summary>
    /// Gets or sets the script timeout.
    /// </summary>
    public TimeSpan? ScriptTimeout { get; set; }

    /// <summary>
    /// Gets or sets whether to take screenshots on failure.
    /// </summary>
    public bool ScreenshotsOnFailure { get; set; } = true;

    /// <summary>
    /// Gets or sets the screenshot directory.
    /// </summary>
    public string? ScreenshotDirectory { get; set; }

    /// <summary>
    /// Gets or sets whether to record logs.
    /// </summary>
    public bool RecordLogs { get; set; }

    /// <summary>
    /// Gets or sets custom capabilities.
    /// </summary>
    public Dictionary<string, object> CustomCapabilities { get; } = new();

    /// <summary>
    /// Gets or sets a custom capability.
    /// </summary>
    /// <param name="key">The capability key.</param>
    /// <returns>The capability value.</returns>
    public object? this[string key]
    {
        get => CustomCapabilities.TryGetValue(key, out var value) ? value : null;
        set => CustomCapabilities[key] = value!;
    }
}

/// <summary>
/// Provides logging for test sessions.
/// </summary>
public class SessionLogger
{
    private readonly List<LogEntry> _entries = new();
    private readonly object _lock = new();

    /// <summary>
    /// Gets all log entries.
    /// </summary>
    public IReadOnlyList<LogEntry> Entries
    {
        get
        {
            lock (_lock)
            {
                return _entries.ToList();
            }
        }
    }

    /// <summary>
    /// Logs a message.
    /// </summary>
    /// <param name="level">Log level.</param>
    /// <param name="message">Log message.</param>
    /// <param name="data">Optional additional data.</param>
    public void Log(LogLevel level, string message, object? data = null)
    {
        var entry = new LogEntry
        {
            Timestamp = DateTime.UtcNow,
            Level = level,
            Message = message,
            Data = data
        };

        lock (_lock)
        {
            _entries.Add(entry);
        }
    }

    /// <summary>
    /// Logs an info message.
    /// </summary>
    /// <param name="message">The message.</param>
    public void Info(string message) => Log(LogLevel.Info, message);

    /// <summary>
    /// Logs a debug message.
    /// </summary>
    /// <param name="message">The message.</param>
    public void Debug(string message) => Log(LogLevel.Debug, message);

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The message.</param>
    public void Warning(string message) => Log(LogLevel.Warning, message);

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="exception">Optional exception.</param>
    public void Error(string message, Exception? exception = null) =>
        Log(LogLevel.Error, message, exception);

    /// <summary>
    /// Gets entries filtered by level.
    /// </summary>
    /// <param name="minLevel">Minimum log level.</param>
    /// <returns>Filtered entries.</returns>
    public IEnumerable<LogEntry> GetEntries(LogLevel minLevel = LogLevel.Debug)
    {
        return Entries.Where(e => e.Level >= minLevel);
    }

    /// <summary>
    /// Clears all entries.
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _entries.Clear();
        }
    }
}

/// <summary>
/// A log entry.
/// </summary>
public class LogEntry
{
    /// <summary>
    /// Gets or sets the timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the log level.
    /// </summary>
    public LogLevel Level { get; set; }

    /// <summary>
    /// Gets or sets the message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional data.
    /// </summary>
    public object? Data { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"[{Timestamp:HH:mm:ss.fff}] [{Level}] {Message}";
    }
}

/// <summary>
/// Log levels.
/// </summary>
public enum LogLevel
{
    /// <summary>Debug level.</summary>
    Debug = 0,
    /// <summary>Info level.</summary>
    Info = 1,
    /// <summary>Warning level.</summary>
    Warning = 2,
    /// <summary>Error level.</summary>
    Error = 3
}
