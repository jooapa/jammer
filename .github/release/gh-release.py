#!/usr/bin/env python3
"""GitHub release script for Jammer.

Usage:
    python3 .github/release/gh-release.py 3.53

Steps:
    1. Validates that the argument matches the VERSION file
    2. Creates release notes from template (if not already written)
    3. Commits, tags, and pushes
    4. Creates a GitHub release with the notes
    5. Builds and uploads all platform artifacts
"""

from __future__ import annotations

import argparse
import subprocess
import sys
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent.parent
VERSION_FILE = REPO_ROOT / "VERSION"
RELEASE_DIR = Path(__file__).resolve().parent
NOTES_DIR = RELEASE_DIR / "notes"
TEMPLATE_FILE = RELEASE_DIR / "RELEASE_NOTES_TEMPLATE.md"
BUILD_SCRIPT = REPO_ROOT / "scripts" / "build.py"


def require_command(name: str, guidance: str) -> str:
    from shutil import which

    path = which(name)
    if path is None:
        die(f"Required tool '{name}' was not found. {guidance}")
    return path


def run(cmd: list[str], cwd: Path | None = None) -> None:
    result = subprocess.run(cmd, cwd=cwd or REPO_ROOT, check=False)
    if result.returncode != 0:
        die(f"Command failed (exit {result.returncode}): {' '.join(cmd)}")


def capture(cmd: list[str], cwd: Path | None = None) -> str:
    result = subprocess.run(
        cmd,
        cwd=cwd or REPO_ROOT,
        check=False,
        capture_output=True,
        text=True,
    )
    if result.returncode != 0:
        die(f"Command failed (exit {result.returncode}): {' '.join(cmd)}")
    return result.stdout.strip()


def die(msg: str) -> None:
    print(f"ERROR: {msg}", file=sys.stderr)
    sys.exit(1)


def get_previous_version(current: str) -> str:
    tags = capture(["git", "tag", "--sort=-v:refname"])
    versions = [t for t in tags.splitlines() if t != current and not t.startswith("v")]
    return versions[0] if versions else "0.0"


def generate_release_notes(version: str, prev_version: str) -> Path:
    NOTES_DIR.mkdir(parents=True, exist_ok=True)
    notes_file = NOTES_DIR / f"release_notes_{version}.md"
    if notes_file.exists():
        print(f"Release notes already exist: {notes_file}")
        return notes_file

    template = TEMPLATE_FILE.read_text(encoding="utf-8")
    content = template.replace("{VERSION}", version).replace(
        "{PREV_VERSION}", prev_version
    )
    notes_file.write_text(content, encoding="utf-8")
    print(f"Generated release notes: {notes_file}")
    return notes_file



def update_homebrew_formula(version: str) -> None:
    import hashlib
    import re
    
    formula_path = REPO_ROOT / "Formula" / "jammer.rb"
    if not formula_path.exists():
        return
        
    print(f"\nUpdating Homebrew formula for version {version}...")
    text = formula_path.read_text(encoding="utf-8")
    text = re.sub(r'version\s+".*?"', f'version "{version}"', text)
    
    artifacts_dir = REPO_ROOT / "artifacts"
    
    def repl_sha256(match):
        url_line = match.group(1)
        url_val = match.group(2)
        existing_sha = match.group(3)
        
        filename = url_val.split("/")[-1].replace("#{version}", version)
        filepath = artifacts_dir / filename
        
        if filepath.exists():
            sha256 = hashlib.sha256(filepath.read_bytes()).hexdigest()
            return f'{url_line}\n      sha256 "{sha256}"'
        else:
            die(f"Missing artifact for Homebrew update: {filepath}")
            return match.group(0)

    new_text = re.sub(r'(url\s+"(.*?)")\n\s+sha256\s+"(.*?)"', repl_sha256, text)
    if new_text == text:
        print("Homebrew formula is already up-to-date.")
        return
        
    formula_path.write_text(new_text, encoding="utf-8")
    
    run(["git", "add", str(formula_path)])
    run(["git", "commit", "-m", f"chore: update Homebrew formula for v{version}"])
    run(["git", "push", "origin", "main"])

def main() -> int:
    parser = argparse.ArgumentParser(description="Create a GitHub release for Jammer.")
    parser.add_argument("version", help="Version to release (must match VERSION file).")
    parser.add_argument(
        "--skip-build",
        action="store_true",
        help="Skip building and uploading artifacts.",
    )
    parser.add_argument(
        "--skip-push", action="store_true", help="Skip git commit/tag/push."
    )
    args = parser.parse_args()

    # Validate version matches VERSION file
    file_version = VERSION_FILE.read_text(encoding="utf-8").strip()
    if args.version != file_version:
        die(
            f"Version mismatch: argument '{args.version}' != VERSION file '{file_version}'.\n"
            f'Update the VERSION file first: echo "{args.version}" > VERSION'
        )

    version = args.version
    prev_version = get_previous_version(version)
    print(f"Releasing Jammer v{version} (previous: {prev_version})")

    # Check for uncommitted changes
    status = capture(["git", "status", "--porcelain"])
    if status:
        print(f"WARNING: Uncommitted changes:\n{status}")
        confirm = input("Continue anyway? [y/N] ").strip().lower()
        if confirm != "y":
            print("Aborted.")
            return 1

    # Generate or find release notes
    notes_file = generate_release_notes(version, prev_version)
    print(f"\nReview and edit release notes: {notes_file}")
    confirm = input("Release notes ready? [y/N] ").strip().lower()
    if confirm != "y":
        print("Aborted. Edit the notes and re-run.")
        return 1

    # Git commit, tag, push
    if not args.skip_push:
        require_command("git", "Install git.")
        run(["git", "add", "-A"])
        status = capture(["git", "status", "--porcelain"])
        if status:
            run(["git", "commit", "-m", version])
        else:
            print("No changes to commit before tagging.")
        run(["git", "tag", "-f", version])
        run(["git", "push", "origin", "main"])
        run(["git", "push", "origin", f"refs/tags/{version}", "-f"])

    # Create or edit GitHub release
    gh = require_command("gh", "Install GitHub CLI: https://cli.github.com/")
    
    release_check = subprocess.run([gh, "release", "view", version], capture_output=True)
    if release_check.returncode == 0:
        print(f"\nRelease {version} already exists. Editing notes and re-uploading artifacts.")
        run(
            [
                gh,
                "release",
                "edit",
                version,
                "--title",
                f"v{version}",
                "--notes-file",
                str(notes_file),
            ]
        )
    else:
        run(
            [
                gh,
                "release",
                "create",
                version,
                "--title",
                f"v{version}",
                "--notes-file",
                str(notes_file),
            ]
        )

    # Build and upload artifacts
    if not args.skip_build:
        print("\nBuilding and uploading all targets (including Windows NSIS)...")
        run([sys.executable, str(BUILD_SCRIPT), "-t", "all", "--nsis", "--upload"])
        
        update_homebrew_formula(version)

    print(
        f"\nRelease v{version} created: https://github.com/jooapa/jammer/releases/tag/{version}"
    )
    return 0


if __name__ == "__main__":
    try:
        sys.exit(main())
    except KeyboardInterrupt:
        print("\nAborted.")
        sys.exit(1)
