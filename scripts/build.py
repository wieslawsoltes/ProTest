#!/usr/bin/env python3
"""Build, test, pack, and exercise the NuGet packages as an external consumer."""

from __future__ import annotations

import argparse
import json
import os
from pathlib import Path
import shutil
import signal
import subprocess
import sys
import tempfile
import xml.etree.ElementTree as ET
import zipfile


ROOT = Path(__file__).resolve().parents[1]
PACKAGES = ROOT / "artifacts" / "packages"
RESULTS = ROOT / "artifacts" / "test-results"
EXPECTED_PACKAGES = {
    "ProTest.Avalonia.Headless": "Avalonia.Headless.dll",
    "ProTest.Avalonia.Headless.XUnit.v3": "Avalonia.Headless.XUnit.v3.dll",
    "ProTest.Avalonia.Headless.XUnit.v4": "Avalonia.Headless.XUnit.v4.dll",
}


def run(*args: str | Path, cwd: Path = ROOT, env: dict | None = None,
        timeout: int = 300, expected_exit: int = 0) -> None:
    command = [str(arg) for arg in args]
    print("+ " + subprocess.list2cmdline(command), flush=True)
    process = subprocess.Popen(command, cwd=cwd, env=env, start_new_session=os.name != "nt")
    try:
        exit_code = process.wait(timeout=timeout)
    except subprocess.TimeoutExpired:
        # dotnet run/test may launch a child test host. Terminate that host too,
        # otherwise a hung dispatcher can keep CI and its output pipes alive.
        if os.name == "nt":
            subprocess.run(["taskkill", "/F", "/T", "/PID", str(process.pid)],
                           stdout=subprocess.DEVNULL, stderr=subprocess.STDOUT, timeout=30)
        else:
            try:
                os.killpg(process.pid, signal.SIGKILL)
            except ProcessLookupError:
                pass
        process.wait(timeout=30)
        raise
    if exit_code != expected_exit:
        raise RuntimeError(f"Expected exit code {expected_exit}, received {exit_code}: {command}")


def versions() -> dict[str, str]:
    document = ET.parse(ROOT / "build" / "Version.props")
    return {
        name: document.findtext(f".//{name}") or ""
        for name in ("Version", "AvaloniaVersion", "XunitV3Version", "XunitV4Version")
    }


def validate_packages(version: str, avalonia_version: str) -> None:
    found = set()
    for package in sorted(PACKAGES.glob("*.nupkg")):
        with zipfile.ZipFile(package) as archive:
            nuspecs = [name for name in archive.namelist() if name.endswith(".nuspec")]
            if len(nuspecs) != 1:
                raise RuntimeError(f"Expected one nuspec in {package.name}")
            metadata = ET.fromstring(archive.read(nuspecs[0]))
            # Ignore the schema version; NuGet has used several namespace URIs.
            for node in metadata.iter():
                node.tag = node.tag.split("}")[-1]
            package_id = metadata.findtext("metadata/id")
            if package_id not in EXPECTED_PACKAGES:
                raise RuntimeError(f"Unexpected package: {package_id}")
            if package_id in found:
                raise RuntimeError(f"Multiple packages with the same ID: {package_id}")
            if metadata.findtext("metadata/version") != version:
                raise RuntimeError(f"Incorrect version in {package.name}")
            if "Wiesław" not in (metadata.findtext("metadata/authors") or ""):
                raise RuntimeError(f"Owner author metadata is missing from {package.name}")
            if metadata.findtext("metadata/license") != "MIT":
                raise RuntimeError(f"MIT license metadata is missing from {package.name}")
            readme = metadata.findtext("metadata/readme")
            if not readme or readme not in archive.namelist():
                raise RuntimeError(f"Embedded README is missing from {package.name}")
            for framework in ("net8.0", "net10.0"):
                assembly = f"lib/{framework}/{EXPECTED_PACKAGES[package_id]}"
                if assembly not in archive.namelist():
                    raise RuntimeError(f"{package.name} is missing {assembly}")
            for filename in archive.namelist():
                if filename.endswith(("/Avalonia.Base.dll", "/Avalonia.Controls.dll")):
                    raise RuntimeError(f"Official Avalonia binary bundled in {package.name}")
            dependencies = metadata.findall("metadata/dependencies/group/dependency")
            if package_id == "ProTest.Avalonia.Headless" and not any(
                dependency.attrib["id"] == "Avalonia" for dependency in dependencies
            ):
                raise RuntimeError("The headless package must depend on the official Avalonia package")
            for dependency in dependencies:
                dependency_id = dependency.attrib["id"]
                if dependency_id == "Avalonia.Headless":
                    raise RuntimeError("Must depend on ProTest.Avalonia.Headless, not the official headless package")
                if dependency_id == "Avalonia" and dependency.attrib.get("version") != f"[{avalonia_version}]":
                    raise RuntimeError("Avalonia's private API dependency must be pinned to an exact version")
            if package_id != "ProTest.Avalonia.Headless" and not any(
                dependency.attrib["id"] == "ProTest.Avalonia.Headless" for dependency in dependencies
            ):
                raise RuntimeError(f"Headless package dependency is missing from {package.name}")
            if not package.with_suffix(".snupkg").is_file():
                raise RuntimeError(f"Symbol package is missing for {package.name}")
            found.add(package_id)
    if found != set(EXPECTED_PACKAGES):
        raise RuntimeError(f"Expected {sorted(EXPECTED_PACKAGES)}, found {sorted(found)}")
    print(f"Validated all {len(found)} packages and symbol packages.", flush=True)


def validate_test_results(result: Path) -> None:
    if not result.is_file():
        raise RuntimeError(f"Test runner did not produce {result}")
    assemblies = ET.parse(result).findall(".//assembly")
    total = sum(int(assembly.attrib.get("total", 0)) for assembly in assemblies)
    passed = sum(int(assembly.attrib.get("passed", 0)) for assembly in assemblies)
    failed = sum(int(assembly.attrib.get("failed", 0)) for assembly in assemblies)
    errors = sum(int(assembly.attrib.get("errors", 0)) for assembly in assemblies)
    if total == 0 or passed == 0 or failed or errors:
        raise RuntimeError(f"Invalid test run: {result.name}: total={total}, passed={passed}, failed={failed}, errors={errors}")
    print(f"Verified {result.name}: {total} tests, no failures or runner errors.", flush=True)


def validate_trx_results(result: Path, runner: str) -> None:
    if not result.is_file():
        raise RuntimeError(f"{runner} did not produce {result}")
    document = ET.parse(result)
    for node in document.iter():
        node.tag = node.tag.split("}")[-1]
    counters = document.find(".//Counters")
    if counters is None or int(counters.attrib.get("executed", 0)) == 0:
        raise RuntimeError(f"{runner} did not execute any tests: {result.name}")
    if int(counters.attrib.get("failed", 0)) or int(counters.attrib.get("error", 0)):
        raise RuntimeError(f"{runner} reported failures: {result.name}")
    print(f"Verified {result.name}: {counters.attrib['executed']} tests executed by {runner}.", flush=True)


def validate_failure_probe(result: Path) -> None:
    if not result.is_file():
        raise RuntimeError(f"Failure probe did not produce {result}")
    assemblies = ET.parse(result).findall(".//assembly")
    total = sum(int(assembly.attrib.get("total", 0)) for assembly in assemblies)
    failed = sum(int(assembly.attrib.get("failed", 0)) for assembly in assemblies)
    errors = sum(int(assembly.attrib.get("errors", 0)) for assembly in assemblies)
    output = result.read_text(encoding="utf-8")
    markers = ("PROTEST_EXPECTED_ASYNC_FAILURE", "PROTEST_EXPECTED_DISPATCHER_FAILURE")
    if total != 2 or failed != 2 or errors or any(marker not in output for marker in markers):
        raise RuntimeError(f"Failure reporting probe did not report exactly the two expected failures: {result}")
    print(f"Verified {result.name}: both intentional failures reached the runner and produced a failure exit code.", flush=True)


def source_tests(configuration: str, frameworks: list[str]) -> None:
    projects = sorted((ROOT / "tests").glob("Avalonia.Headless.XUnit.v[34].*.UnitTests/*.csproj"))
    if len(projects) != 4:
        raise RuntimeError(f"Expected four source test projects, found {len(projects)}")
    for project in projects:
        xml_option = "-result-xml" if ".v4" in project.stem.lower() else "-xml"
        for framework in frameworks:
            result = RESULTS / f"{project.stem}-{framework}.xml"
            result.unlink(missing_ok=True)
            run("dotnet", "run", "--project", project, "--configuration", configuration,
                "--framework", framework, "--no-build", "--", xml_option, result, timeout=180)
            validate_test_results(result)


def mtp_package_tests(consumers: Path, nuget_config: Path, environment: dict,
                      configuration: str, frameworks: list[str], properties: list[str]) -> None:
    mtp_consumers = consumers.with_name("consumers-mtp")
    shutil.copytree(consumers, mtp_consumers, ignore=shutil.ignore_patterns("bin", "obj", "TestResults"))
    sdk = json.loads((ROOT / "global.json").read_text())
    sdk["test"] = {"runner": "Microsoft.Testing.Platform"}
    (mtp_consumers / "global.json").write_text(json.dumps(sdk, indent=2), encoding="utf-8")
    mtp_properties = [*properties, "-p:XunitRunnerPackage=xunit.v3.mtp-v2"]
    for project in sorted(mtp_consumers.glob("*/*.csproj")):
        for framework in frameworks:
            framework_property = f"-p:TestTargetFramework={framework}"
            run("dotnet", "restore", project, "--configfile", nuget_config,
                framework_property, *mtp_properties, cwd=mtp_consumers, env=environment)
            for mode in ("attributes", "framework"):
                framework_mode = f"-p:UseAvaloniaTestFramework={str(mode == 'framework').lower()}"
                result = RESULTS / f"{project.stem}-{framework}-{mode}-mtp.trx"
                result.unlink(missing_ok=True)
                run("dotnet", "test", "--project", project, "--configuration", configuration,
                    "--no-restore", framework_property, framework_mode, *mtp_properties,
                    "--results-directory", RESULTS, "--", "--report-xunit-trx",
                    "--report-xunit-trx-filename", result.name,
                    cwd=mtp_consumers, env=environment, timeout=180)
                validate_trx_results(result, "Microsoft.Testing.Platform")


def package_tests(configuration: str, frameworks: list[str], version: dict[str, str]) -> None:
    # A temporary directory outside the checkout prevents accidental imports of
    # repository props, linked source files, signing keys, and project references.
    with tempfile.TemporaryDirectory(prefix="protest-package-consumers-") as temporary:
        temporary_root = Path(temporary)
        consumers = temporary_root / "consumers"
        shutil.copytree(ROOT / "tests" / "PackageConsumers", consumers,
                        ignore=shutil.ignore_patterns("bin", "obj", "TestResults"))
        # Keep SDK selection deterministic even outside the repository.
        shutil.copyfile(ROOT / "global.json", temporary_root / "global.json")
        configuration_xml = ET.Element("configuration")
        sources = ET.SubElement(configuration_xml, "packageSources")
        ET.SubElement(sources, "clear")
        ET.SubElement(sources, "add", key="local", value=str(PACKAGES))
        ET.SubElement(sources, "add", key="nuget.org", value="https://api.nuget.org/v3/index.json")
        mapping = ET.SubElement(configuration_xml, "packageSourceMapping")
        local = ET.SubElement(mapping, "packageSource", key="local")
        ET.SubElement(local, "package", pattern="ProTest.*")
        official = ET.SubElement(mapping, "packageSource", key="nuget.org")
        ET.SubElement(official, "package", pattern="*")
        nuget_config = temporary_root / "NuGet.Config"
        ET.ElementTree(configuration_xml).write(nuget_config, encoding="utf-8", xml_declaration=True)
        environment = os.environ.copy()
        environment.pop("PROTEST_FAILURE_PROBE", None)
        # Disposable consumers must not leave build servers holding their working
        # directories open on Windows. These settings also reach MTP's in-process
        # MSBuild host, whose CLI does not accept --disable-build-servers.
        environment.update({
            "NUGET_PACKAGES": str(temporary_root / "packages"),
            "MSBUILDDISABLENODEREUSE": "1",
            "DOTNET_CLI_USE_MSBUILD_SERVER": "0",
        })
        properties = ["-p:UseSharedCompilation=false", f"-p:ProTestVersion={version['Version']}"] + [
            f"-p:{name}={value}" for name, value in version.items() if name != "Version"
        ]
        projects = sorted(consumers.glob("*/*.csproj"))
        if len(projects) != 2:
            raise RuntimeError("Expected separate v3 and v4 package consumers")
        mtp_package_tests(consumers, nuget_config, environment, configuration, frameworks, properties)
        for project in projects:
            xml_option = "-result-xml" if ".v4" in project.stem.lower() else "-xml"
            if ET.parse(project).findall(".//ProjectReference"):
                raise RuntimeError(f"Package consumer has a ProjectReference: {project}")
            for framework in frameworks:
                framework_property = f"-p:TestTargetFramework={framework}"
                run("dotnet", "restore", project, "--configfile", nuget_config,
                    framework_property, *properties, cwd=consumers, env=environment)
                assets = json.loads((project.parent / "obj" / "project.assets.json").read_text())
                if any(library.get("type") == "project" for library in assets["libraries"].values()):
                    raise RuntimeError("Package consumer restored a project reference")
                for mode in ("attributes", "framework"):
                    framework_mode = f"-p:UseAvaloniaTestFramework={str(mode == 'framework').lower()}"
                    result_name = f"{project.stem}-{framework}-{mode}"
                    result = RESULTS / f"{result_name}.xml"
                    result.unlink(missing_ok=True)
                    run("dotnet", "run", "--project", project, "--configuration", configuration,
                        "--no-restore", framework_property, framework_mode, *properties, "--", xml_option, result,
                        cwd=consumers, env=environment, timeout=180)
                    validate_test_results(result)
                    # Also cover the adapter used by IDEs and traditional dotnet test.
                    trx = RESULTS / f"{result_name}.trx"
                    trx.unlink(missing_ok=True)
                    run("dotnet", "test", project, "--configuration", configuration,
                        "--no-build", "--no-restore", framework_property, framework_mode, *properties,
                        "--logger", f"trx;LogFileName={trx.name}", "--results-directory", RESULTS,
                        cwd=consumers, env=environment, timeout=180)
                    validate_trx_results(trx, "VSTest")
                    probe = RESULTS / f"{result_name}-expected-failures.xml"
                    probe.unlink(missing_ok=True)
                    probe_environment = {**environment, "PROTEST_FAILURE_PROBE": "1"}
                    print("Checking intentional failures: expect exactly two failed tests and exit code 1.", flush=True)
                    run("dotnet", "run", "--project", project, "--configuration", configuration,
                        "--no-build", "--no-restore", framework_property, framework_mode, *properties,
                        "--", "-class", "PackageConsumers.FailureReportingTests", xml_option, probe,
                        cwd=consumers, env=probe_environment, timeout=180, expected_exit=1)
                    validate_failure_probe(probe)


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--configuration", default="Release", choices=["Debug", "Release"])
    parser.add_argument("--framework", action="append", choices=["net8.0", "net10.0"],
                        help="Runtime framework to execute; repeat to select both (default: both).")
    parser.add_argument("--check-tag", help="Validate a release tag against the package version, then exit.")
    parser.add_argument("--package-tests-only", action="store_true",
                        help="Validate and test existing packages without rebuilding source projects.")
    args = parser.parse_args()
    version = versions()
    if args.check_tag:
        expected = f"v{version['Version']}"
        if args.check_tag != expected:
            raise RuntimeError(f"Release tag must equal {expected}; received {args.check_tag}")
        print(f"Release tag {expected} matches build/Version.props.")
        return
    frameworks = args.framework or ["net8.0", "net10.0"]
    RESULTS.mkdir(parents=True, exist_ok=True)
    if not args.package_tests_only:
        run("dotnet", "restore", "ProTest.slnx", "--locked-mode")
        run("dotnet", "build", "ProTest.slnx", "--configuration", args.configuration,
            "--no-restore", "-p:ContinuousIntegrationBuild=true")
        source_tests(args.configuration, frameworks)
        # Avoid releasing stale packages from an earlier version or removed project.
        if PACKAGES.exists():
            shutil.rmtree(PACKAGES)
        PACKAGES.mkdir(parents=True)
        run("dotnet", "pack", "ProTest.slnx", "--configuration", args.configuration,
            "--no-build", "--output", PACKAGES, "-p:ContinuousIntegrationBuild=true")
    validate_packages(version["Version"], version["AvaloniaVersion"])
    package_tests(args.configuration, frameworks, version)
    print("Build, tests, package validation, and isolated NuGet consumer tests passed.", flush=True)


if __name__ == "__main__":
    try:
        main()
    except (RuntimeError, subprocess.CalledProcessError, subprocess.TimeoutExpired) as error:
        print(f"ERROR: {error}", file=sys.stderr)
        sys.exit(1)
