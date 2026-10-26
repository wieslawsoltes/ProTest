---
title: "Test runners"
description: "Run ProTest under VSTest, the xUnit executable, or Microsoft Testing Platform."
sidebar_position: 4
doc-type: how-to
---

The adapter supplies Avalonia execution; xUnit supplies discovery and runner integration. Use one of the following configurations consistently within a project.

## VSTest and IDE integration

The [quick start](../getting-started/xunit.md) uses `xunit.v3.mtp-off`, `xunit.runner.visualstudio`, and `Microsoft.NET.Test.Sdk`. It supports conventional `dotnet test` and VSTest-compatible IDE discovery.

```sh
dotnet test Tests.csproj --configuration Release
```

## Native executable runner

The test project has `OutputType=Exe`, so xUnit's native runner can execute it directly:

```sh
dotnet run --project Tests.csproj --configuration Release
```

## Microsoft Testing Platform

Replace `xunit.v3.mtp-off` with `xunit.v3.mtp-v2`, keeping its matching xUnit version (`3.2.2` or `4.0.2`). Add or merge this setting into `global.json` for a .NET 10 SDK invocation:

```json
{
  "test": {
    "runner": "Microsoft.Testing.Platform"
  }
}
```

```sh
dotnet test --project Tests.csproj --configuration Release
```

If all tools use MTP, the VSTest adapter and `Microsoft.NET.Test.Sdk` references can be removed. ProTest's package integration suite exercises these runner modes separately.

These adapters depend on reflection-based xUnit APIs. Use the reflection runner packages above; Native AOT runner packages are not supported.

See the [official xUnit MTP guide](https://xunit.net/docs/getting-started/v3/microsoft-testing-platform) for runner-specific options.
