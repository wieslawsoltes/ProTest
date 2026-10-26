---
title: "Native UI tests with Appium"
description: "Choose between ProTest in-process automation and external native application testing."
sidebar_position: 9
doc-type: how-to
---

ProTest's `AvaloniaDriver` is an in-process testing API over Avalonia controls. A real Appium driver launches or attaches to a native application and interacts through platform automation. Keep those test suites separate so their dependencies and environment requirements are clear.

| Requirement | ProTest helpers | Native Appium tooling |
| --- | --- | --- |
| Locate and interact with Avalonia controls | Yes, directly in memory | Through native accessibility |
| Run without a display server | Yes | Depends on the desktop driver and environment |
| Exercise native window chrome and menus | No | Yes, when exposed by the platform |
| Use the WebDriver wire protocol | No | Yes |
| Inspect application-specific CLR properties | Yes | Usually through exposed accessibility attributes only |

## Native suite setup

Use a dedicated test project with the Appium .NET client and a driver matching the target platform. The external fixture should start/connect to the application, expose the session to tests, and quit it during disposal. Configure platform capabilities, application path or bundle ID, server endpoint, and timeouts in that fixture.

Assign stable automation IDs in application XAML:

```xml
<Button AutomationProperties.AutomationId="SaveButton" Content="Save" />
<TextBox AutomationProperties.AutomationId="TitleInput" />
```

Native tests can then locate those IDs, perform input, and inspect the accessibility properties exposed by the platform. IDs are also usable with ProTest's `By.AutomationId` locator.

## Platform and CI considerations

Windows and macOS use different desktop automation drivers and report some state through different attributes. Gate platform-specific tests, grant the accessibility permissions required by the chosen driver, and keep the server/driver versions explicit in CI.

A native desktop suite requires a suitable interactive host. Use ProTest's [headless CI setup](ci.md) for the cross-platform control suite and reserve native tests for behavior that needs the desktop stack.

The [Avalonia Appium guide](https://docs.avaloniaui.net/docs/testing/ui-testing-with-appium) provides its platform setup and example fixtures. Follow [Appium's current installation documentation](https://appium.io/docs/en/latest/quickstart/) for supported driver versions and capabilities. The ProTest-specific [driver reference](../reference/automation/driver.md) documents the in-process alternative.
