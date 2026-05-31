#!/usr/bin/env python3

import argparse
import json
import posixpath
import shutil
import tarfile
import tempfile
from pathlib import Path


def parse_args():
    parser = argparse.ArgumentParser(
        description="Extract a .unitypackage into a Unity project without launching Unity."
    )
    parser.add_argument("--package", required=True, help="Path to .unitypackage")
    parser.add_argument("--project", required=True, help="Unity project root")
    parser.add_argument(
        "--remove-manifest-dependency",
        action="append",
        default=[],
        help="Package dependency name to remove from Packages/manifest.json",
    )
    parser.add_argument(
        "--clean",
        action="append",
        default=[],
        help="Project-relative path to remove before import, such as Assets/AppsFlyer",
    )
    return parser.parse_args()


def safe_project_path(project_root, unity_path):
    normalized = posixpath.normpath(unity_path)
    if normalized.startswith("../") or normalized == ".." or posixpath.isabs(normalized):
      raise ValueError(f"Unsafe package path: {unity_path}")
    return project_root / Path(normalized)


def remove_manifest_dependencies(project_root, dependency_names):
    if not dependency_names:
        return

    manifest_path = project_root / "Packages" / "manifest.json"
    if not manifest_path.exists():
        return

    with manifest_path.open("r", encoding="utf-8") as manifest_file:
        manifest = json.load(manifest_file)

    dependencies = manifest.get("dependencies", {})
    changed = False
    for dependency_name in dependency_names:
        if dependency_name in dependencies:
            del dependencies[dependency_name]
            changed = True

    if changed:
        manifest["dependencies"] = dependencies
        with manifest_path.open("w", encoding="utf-8") as manifest_file:
            json.dump(manifest, manifest_file, indent=2)
            manifest_file.write("\n")

    lock_path = project_root / "Packages" / "packages-lock.json"
    if lock_path.exists():
        with lock_path.open("r", encoding="utf-8") as lock_file:
            lock = json.load(lock_file)
        lock_dependencies = lock.get("dependencies", {})
        lock_changed = False
        for dependency_name in dependency_names:
            if dependency_name in lock_dependencies:
                del lock_dependencies[dependency_name]
                lock_changed = True
        if lock_changed:
            lock["dependencies"] = lock_dependencies
            with lock_path.open("w", encoding="utf-8") as lock_file:
                json.dump(lock, lock_file, indent=2)
                lock_file.write("\n")


def extract_tar_safely(package_path, temp_dir):
    with tarfile.open(package_path, "r:*") as package:
        for member in package.getmembers():
            target = temp_dir / member.name
            if not target.resolve().is_relative_to(temp_dir.resolve()):
                raise ValueError(f"Unsafe archive member: {member.name}")
            package.extract(member, temp_dir)


def import_unity_package(package_path, project_root):
    imported = 0

    with tempfile.TemporaryDirectory() as temp_name:
        temp_dir = Path(temp_name)
        extract_tar_safely(package_path, temp_dir)

        for entry in temp_dir.iterdir():
            if not entry.is_dir():
                continue

            pathname_file = entry / "pathname"
            if not pathname_file.exists():
                continue

            unity_path = pathname_file.read_text(encoding="utf-8").strip()
            if not unity_path:
                continue

            destination = safe_project_path(project_root, unity_path)
            asset_file = entry / "asset"
            meta_file = entry / "asset.meta"

            if asset_file.exists():
                destination.parent.mkdir(parents=True, exist_ok=True)
                shutil.copyfile(asset_file, destination)
                imported += 1
            else:
                destination.mkdir(parents=True, exist_ok=True)

            if meta_file.exists():
                meta_destination = Path(str(destination) + ".meta")
                meta_destination.parent.mkdir(parents=True, exist_ok=True)
                shutil.copyfile(meta_file, meta_destination)

    return imported


def main():
    args = parse_args()
    package_path = Path(args.package).resolve()
    project_root = Path(args.project).resolve()

    if not package_path.exists():
        raise FileNotFoundError(f"Package not found: {package_path}")
    if not project_root.exists():
        raise FileNotFoundError(f"Project not found: {project_root}")

    for clean_path in args.clean:
        target = safe_project_path(project_root, clean_path)
        if target.exists():
            if target.is_dir():
                shutil.rmtree(target)
            else:
                target.unlink()
        meta_target = Path(str(target) + ".meta")
        if meta_target.exists():
            meta_target.unlink()

    remove_manifest_dependencies(project_root, args.remove_manifest_dependency)
    imported = import_unity_package(package_path, project_root)
    print(f"Imported {imported} assets from {package_path} into {project_root}")


if __name__ == "__main__":
    main()
