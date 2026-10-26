---
title: "Sources and attribution"
description: "Upstream source commits, documentation coverage, and licensing."
sidebar_position: 1
doc-type: reference
---

## Avalonia testing documentation

This site's navigation and coverage are based on [AvaloniaUI/avalonia-docs](https://github.com/AvaloniaUI/avalonia-docs) at commit [`2c4100e691cf67fc3cb72825b638ae7e3edfb855`](https://github.com/AvaloniaUI/avalonia-docs/tree/2c4100e691cf67fc3cb72825b638ae7e3edfb855). ProTest supplies original, package-specific explanations and examples for these topics:

| Upstream page | ProTest coverage |
| --- | --- |
| [Testing overview](https://github.com/AvaloniaUI/avalonia-docs/blob/2c4100e691cf67fc3cb72825b638ae7e3edfb855/docs/testing/index.md) | [Testing strategies](../guides/testing-strategies.md) |
| [Headless platform](https://github.com/AvaloniaUI/avalonia-docs/blob/2c4100e691cf67fc3cb72825b638ae7e3edfb855/docs/testing/setting-up-the-headless-platform.md) | [Platform input and manual sessions](../guides/headless-platform.md), [rendering](../guides/rendering.md) |
| [xUnit](https://github.com/AvaloniaUI/avalonia-docs/blob/2c4100e691cf67fc3cb72825b638ae7e3edfb855/docs/testing/headless-xunit.md) | [xUnit 3/4 setup](../getting-started/xunit.md), [isolation](../guides/isolation.md), [runners](../guides/runners.md) |
| [NUnit](https://github.com/AvaloniaUI/avalonia-docs/blob/2c4100e691cf67fc3cb72825b638ae7e3edfb855/docs/testing/headless-nunit.md) | [NUnit and other frameworks](../guides/nunit.md) |
| [Appium](https://github.com/AvaloniaUI/avalonia-docs/blob/2c4100e691cf67fc3cb72825b638ae7e3edfb855/docs/testing/ui-testing-with-appium.md) | [Native Appium testing](../guides/appium.md) |
| [Docker headless section](https://github.com/AvaloniaUI/avalonia-docs/blob/2c4100e691cf67fc3cb72825b638ae7e3edfb855/docs/deployment/docker.md) | [CI and containers](../guides/ci.md) |

The upstream documentation repository did not contain an explicit license declaration at that commit. Its prose is therefore linked as a reference rather than redistributed under ProTest's MIT license. Avalonia documentation contributors retain credit for the original guides. This is an independent ProTest documentation site, not an official Avalonia site.

## Avalonia source

The headless platform and xUnit adapter origins, version-specific compatibility changes, and signing-key provenance are recorded in the repository's [NOTICE.md](https://github.com/wieslawsoltes/ProTest/blob/main/NOTICE.md). Avalonia source retains its MIT attribution.
