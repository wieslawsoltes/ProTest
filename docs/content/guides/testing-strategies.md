---
title: "Choose a testing strategy"
description: "Combine model, headless, rendering, and native UI tests."
sidebar_position: 1
doc-type: explanation
---

Keep assertions at the smallest level that exercises the behavior you need.

| Level | Typical assertion | Setup |
| --- | --- | --- |
| Model or service | A command changes application state | Plain xUnit |
| Headless control | Input changes text, selection, layout, or bindings | ProTest xUnit adapter |
| Rendered output | A border or custom control produces expected pixels | ProTest plus Skia |
| Native application | An operating-system window exposes accessibility and native menus | Real Appium tooling |

Headless tests create Avalonia controls and the dispatcher in memory. A `Window` participates in layout and input processing without appearing on the desktop. This makes the same suite usable on local machines and CI hosts.

Use the [Appium-style helpers](../reference/automation/driver.md) to express in-process tests with familiar locators. These helpers still belong to the headless layer; [real Appium tests](appium.md) exercise a launched application through native automation.

A practical progression is to test model state first, add headless tests for the controls that connect to it, and add image baselines or native tests for behavior that the earlier layers cannot observe.

Topic reference: [Avalonia testing overview](https://docs.avaloniaui.net/docs/testing).
