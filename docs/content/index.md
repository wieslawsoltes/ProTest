---
title: "ProTest documentation"
description: "Headless Avalonia tests with xUnit 3 and 4, input automation, tree inspection, and recording."
sidebar_position: 0
doc-type: overview
slug: /
---

ProTest runs Avalonia UI tests without a desktop window. It includes xUnit **3.x and 4.x** adapters, the Avalonia headless platform, and the testing helpers originally developed for PanAndZoom.

Start with the [xUnit quick start](getting-started/xunit.md), then choose the APIs needed by your tests.

| Task | Guide |
| --- | --- |
| Install the correct adapter | [Packages and compatibility](getting-started/packages.md) |
| Test controls, binding, focus, and commands | [Headless platform](guides/headless-platform.md) |
| Choose an input, tree, or automation API | [Testing helpers](getting-started/helpers.md) |
| Test touch, keyboard, mouse, and gestures | [Input and gestures](reference/input/touch.md) |
| Find controls and compare their structure | [Control finder](reference/trees/control-finder.md) |
| Write Selenium-style interaction sequences | [Driver and elements](reference/automation/driver.md) |
| Capture images, animations, or test videos | [Rendering](guides/rendering.md) and [screen recording](reference/recording/screen-recorder.md) |
| Run the suite in a build pipeline | [CI and containers](guides/ci.md) |
| Release ProTest packages | [Publishing](reference/publishing.md) |

The packages target **.NET 8 and .NET 10**, with official **Avalonia 12.1.4** dependencies. The original C# namespaces are retained. Select one xUnit adapter per test project.

This documentation follows Avalonia's task-oriented organization: getting started, practical guides, and API reference. The [source inventory](attribution/sources.md) records the upstream guides and commits used for this edition.
