import argparse
import json
import re
import sys
import tempfile
from pathlib import Path

from unitypackage_extractor.extractor import extractPackage


def read_text(path):
    return path.read_text(encoding="utf-8", errors="replace")


def require_file(root, relative_path):
    path = root / relative_path
    if not path.is_file():
        raise AssertionError(f"Missing expected file: {relative_path}")
    return path


def assert_contains(path, expected):
    content = read_text(path)
    if expected not in content:
        raise AssertionError(f"{path} does not contain expected text: {expected}")


def assert_json_version(path, version):
    content = json.loads(read_text(path))
    actual = content.get("version")
    if actual != version:
        raise AssertionError(f"{path} version is {actual!r}, expected {version!r}")


def assert_no_rc_metadata(root, base_version):
    rc_pattern = re.compile(rf"{re.escape(base_version)}-rc[0-9]+")
    for path in root.rglob("*"):
        if not path.is_file():
            continue
        content = path.read_bytes().decode("utf-8", errors="ignore")
        match = rc_pattern.search(content)
        if match:
            rel = path.relative_to(root)
            raise AssertionError(f"RC metadata {match.group(0)!r} found in {rel}")


def validate_common_surfaces(root, version):
    appsflyer_root = root / "Assets" / "AppsFlyer"
    assert_json_version(appsflyer_root / "package.json", version)
    assert_contains(
        require_file(appsflyer_root, "AppsFlyer.cs"),
        f'kAppsFlyerPluginVersion = "{version}"',
    )
    assert_contains(
        require_file(appsflyer_root, "Plugins/iOS/AppsFlyeriOSWrapper.mm"),
        f'pluginVersion:@"{version}"',
    )
    assert_no_rc_metadata(appsflyer_root, version)


def validate_strict_surfaces(root):
    appsflyer_root = root / "Assets" / "AppsFlyer"
    deps = require_file(appsflyer_root, "Editor/AppsFlyerDependencies.xml")
    ios_wrapper = require_file(appsflyer_root, "Plugins/iOS/AppsFlyeriOSWrapper.mm")
    assert_contains(deps, 'name="AppsFlyerFramework/Strict"')
    assert_contains(deps, 'name="PurchaseConnector/Strict"')
    assert_contains(ios_wrapper, "//[AppsFlyerLib shared].disableAdvertisingIdentifier")
    assert_contains(
        ios_wrapper,
        "//[[AppsFlyerLib shared] waitForATTUserAuthorizationWithTimeoutInterval:timeoutInterval];",
    )


def main():
    parser = argparse.ArgumentParser(
        description="Validate production Unity packages contain base-version metadata."
    )
    parser.add_argument("--version", required=True)
    parser.add_argument("--regular", required=True)
    parser.add_argument("--strict", required=True)
    args = parser.parse_args()

    with tempfile.TemporaryDirectory() as temp_dir:
        temp = Path(temp_dir)
        regular_root = temp / "regular"
        strict_root = temp / "strict"
        extractPackage(args.regular, outputPath=str(regular_root))
        extractPackage(args.strict, outputPath=str(strict_root))

        validate_common_surfaces(regular_root, args.version)
        validate_common_surfaces(strict_root, args.version)
        validate_strict_surfaces(strict_root)

    print(f"Production package validation passed for {args.version}.")


if __name__ == "__main__":
    try:
        main()
    except Exception as exc:
        print(f"Production package validation failed: {exc}", file=sys.stderr)
        sys.exit(1)
