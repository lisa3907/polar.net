# Release Notes Directory

This folder contains release notes for each published version of the Polar.Net library.

## Structure
- Each file is named after the version it describes, e.g. `v1.1.1.md`.
- Files are written in Markdown and should summarize key changes, migration notes, and highlights for that release.

## How to Add Release Notes
1. Create a new Markdown file named after the version (e.g., `v1.2.0.md`).
2. Summarize the main changes, breaking changes, and any migration steps.
3. Commit the new file along with your release changes.

## Usage
- These files are referenced by automation scripts (e.g., `publish-github-release.ps1`) to populate GitHub Releases.
- They serve as a changelog for users and maintainers.

## Example
```
# v1.2.0

## Highlights
- Added new webhook event types
- Improved error handling in PolarClient

## Migration Notes
- No breaking changes
```

---

For more information, see the main project [README.md](../README.md).
