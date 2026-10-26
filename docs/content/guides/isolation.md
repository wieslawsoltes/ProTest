---
title: "Isolation and lifecycle"
description: "Control application reuse, dispatcher affinity, and cleanup."
sidebar_position: 3
doc-type: how-to
---

By default, each Avalonia test runs with its own application/service scope. To reuse the application within a test assembly:

```csharp
using Avalonia.Headless;

[assembly: AvaloniaTestIsolation(AvaloniaTestIsolationLevel.PerAssembly)]
```

| Isolation | Suitable for | Cleanup responsibility |
| --- | --- | --- |
| `PerTest` | Tests that replace services, styles, or application resources | Close windows and dispose test-owned resources |
| `PerAssembly` | Large suites with a stable shared application | Also restore modified shared resources and global state |

UI test work is serialized through the headless session even when xUnit schedules tests concurrently. Keep windows and controls local to a test whenever possible.

## Apply Avalonia to all tests

```csharp
using Avalonia.Headless.XUnit;

[assembly: AvaloniaTestFramework]
```

The assembly-wide framework runs ordinary `[Fact]` and `[Theory]` tests through the Avalonia session. Without this attribute, only Avalonia-attributed tests opt in.

## Async code and cleanup

Test-class construction, test methods, async continuations, and test-class cleanup have dispatcher affinity. Place control creation in those stages. Class and collection fixtures retain xUnit's own lifetime and do not automatically run on the dispatcher.

Prefer `await` to blocking on tasks. Dispose bitmaps, recorders, and other owned resources, and close each window in `finally`. Posted dispatcher exceptions are collected before the adapter reports the test result.

## Session cancellation

A dispatch cancelled before it starts does not run its delegate. Cancelling active incomplete work shuts down that session, drains queued work, and completes cleanup before cancellation is returned. A later session request can create a replacement. Synchronous code that blocks the UI thread cannot be forcibly interrupted; configure runner or process timeouts for that case.

Topic reference: [Avalonia's xUnit isolation guidance](https://docs.avaloniaui.net/docs/testing/headless-xunit#test-isolation-level).
