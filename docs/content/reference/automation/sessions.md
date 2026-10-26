---
title: "Driver sessions"
description: "Examples and API guidance for driver sessions."
sidebar_position: 4
doc-type: reference
---

The `DriverSession` class manages test session state and cleanup.

```csharp
using Avalonia.HeadlessTestingFramework.Appium;

// Create session with capabilities
var capabilities = new SessionCapabilities
{
    ImplicitWait = TimeSpan.FromSeconds(5),
    PageLoadTimeout = TimeSpan.FromSeconds(30),
    ScreenshotsOnFailure = true,
    ScreenshotDirectory = "test-screenshots",
    RecordLogs = true
};

using var session = new DriverSession(driver, capabilities);

// Store session data
session.SetData("testUser", "admin");
session.SetData("environment", "staging");

// Retrieve data
var user = session.GetData<string>("testUser");
var timeout = session.GetData("timeout", TimeSpan.FromSeconds(10)); // with default

// Check data
bool hasUser = session.HasData("testUser");
var keys = session.DataKeys;

// Remove data
session.RemoveData("testUser");

// Register cleanup actions
session.RegisterCleanup(() => driver.FindElement(By.Id("logout")).Click());
session.RegisterCleanup(() => Console.WriteLine("Test completed"));

// Get session info
var info = session.GetSessionInfo();
Console.WriteLine($"Session ID: {info["sessionId"]}");
Console.WriteLine($"Duration: {info["duration"]}");

// Reset session (runs cleanup and clears data)
session.Reset();

// Session logging
var logger = new SessionLogger();
logger.Info("Starting test");
logger.Debug("Clicking button");
logger.Warning("Element took longer than expected");
logger.Error("Failed to find element", new NoSuchElementException("Element not found"));

// Get logs
var errors = logger.GetEntries(LogLevel.Error);
foreach (var entry in errors)
{
    Console.WriteLine(entry.ToString());
}
```
