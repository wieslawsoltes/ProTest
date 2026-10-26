---
title: "CI and containers"
description: "Run headless tests, collect diagnostics, and build in containers."
sidebar_position: 10
doc-type: how-to
---

Headless tests do not need an interactive desktop. Install the SDK and runtimes required by the project's target frameworks, restore dependencies, and run the test runner selected in the project.

```sh
dotnet restore Tests.csproj
dotnet test Tests.csproj --configuration Release --no-restore \
  --logger "trx;LogFileName=tests.trx" --results-directory TestResults
```

These are VSTest commands; see [runner configuration](runners.md) for MTP syntax.

## Build the ProTest repository

```sh
python3 scripts/build.py
```

The build validates source projects, produces NuGet and symbol packages, and creates isolated consumer projects outside the checkout. Consumer tests restore the produced ProTest packages together with official NuGet dependencies and exercise native xUnit, VSTest, and MTP execution. Negative probes confirm that assertion failures and posted dispatcher errors fail the process as expected.

The [CI workflow](https://github.com/wieslawsoltes/ProTest/actions/workflows/ci.yml) runs on Linux, Windows, and macOS. Keep TRX files, failure logs, captured frames, and comparison output as artifacts so a failing UI test can be inspected without rerunning it locally.

## Containers

For a single `net10.0` consumer test project, a minimal starting point is:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0
WORKDIR /tests
COPY . .
RUN dotnet restore Tests.csproj
CMD ["dotnet", "test", "Tests.csproj", "--configuration", "Release", "--no-restore"]
```

Use a pinned container image digest in a reproducible production pipeline. Install .NET 8 as well when running the repository's multi-target suite. Real Skia rendering may need additional native libraries and fonts for the selected distribution; build those into the image and keep them consistent with the baseline environment.

A headless test builder calls `.UseHeadless(...)` and does not require X11 or Xvfb. A native Avalonia application launched in a container has different display requirements. See [Avalonia's Docker guide](https://docs.avaloniaui.net/docs/deployment/docker#using-the-headless-platform-instead) for that distinction.

## Recording artifacts

Choose a test-specific temporary output directory. Keep PNG sequences when a video conversion fails, and report the converter's result/error. Install FFmpeg only in jobs that exercise conversion; PNG frame capture and built-in GIF recording do not require it.
