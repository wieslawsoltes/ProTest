---
title: "NUnit and other frameworks"
description: "Understand the boundary between ProTest and official Avalonia NUnit support."
sidebar_position: 8
doc-type: how-to
---

ProTest publishes xUnit 3 and xUnit 4 adapters. It does not publish a `ProTest.Avalonia.Headless.NUnit` package.

For Avalonia's maintained NUnit integration, follow the [official NUnit guide](https://docs.avaloniaui.net/docs/testing/headless-nunit) and install its official `Avalonia.Headless.NUnit` package in a separate NUnit test project. That stack supplies `[AvaloniaTest]`, application configuration through `[AvaloniaTestApplication]`, and isolation options supported by its matching Avalonia release.

The lifecycle is familiar: configure an application builder, load the application's theme, declare the test application attribute once, and mark UI-dependent test methods with the framework's Avalonia attribute. Show the test window, simulate input, assert observable state, and close the window during cleanup.

Avoid combining official and ProTest headless packages in one test project: both supply the `Avalonia.Headless` assembly identity. A project using another harness can instead reference only `ProTest.Avalonia.Headless` and explicitly own a [manual headless session](headless-platform.md#manual-sessions).

This page retains the NUnit topic from Avalonia's testing documentation while keeping ProTest's supported package boundary explicit.
