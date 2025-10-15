# Repository Guidelines

## Project Structure & Module Organization
The codebase targets GTK# on Mono/.NET. Core application logic lives in `src/`, organised by feature (buffers, gui, plugins, util). UI assets and packaging scripts reside under `data/` and `doc/`. Translations are stored in `po/`. Acceptance and unit tests sit in `tests/`, mirroring the namespace layout; helper fixtures live in `tests/tools`, with sample data managed by `tests/copy_test_data.py`.

## Build, Test, and Development Commands
Set up a fresh build directory with `meson setup build --warnlevel=3` (reuse `meson setup build --reconfigure` when dependencies change). Compile via `meson compile -C build` (alias `ninja -C build`). Run `meson test -C build` for the full NUnit suite or `meson test -C build --suite buffers` for a focused run, adding `--print-errorlogs` to surface stack traces. Launch the editor locally through `mono build/src/bless.exe`.

## Coding Style & Naming Conventions
C# sources use tab-indented blocks with opening braces on the same line. Prefer PascalCase for types and public members, camelCase for locals and fields, and `I` prefixes for interfaces. Group `using` directives by framework, third-party, and internal namespaces. Leverage existing helpers in `Bless.Util` before adding new utilities, and keep plugin names descriptive (`ConversionTable`, `HexEditor`).

## Testing Guidelines
Tests rely on NUnit attributes. Name test files `<Subject>Tests.cs` and individual methods using the `Method_State_Expected` pattern already present. Extend fixtures in `tests/buffers` or `tests/util` when covering new functionality. Run `meson test -C build --print-errorlogs` to inspect failures. No fixed coverage gate exists; aim to exercise new branches and serialization paths at least once.

## Commit & Pull Request Guidelines
Commits follow the `area: Imperative summary` style (`plugins: Add 64bit support to ConversionTable`). Keep subjects under 72 characters and elaborate in the body when needed (focus on motivations). Pull requests should describe the problem, outline validation steps, and link GitHub issues when available. Include screenshots or GIFs for UI updates and call out documentation or translation files that reviewers need to regenerate.

## Localization & Packaging Tips
Strings surfaced to users belong in `.po` catalogs—update them with `ninja -C build bless-pot` before submitting localization work. Packaging metadata resides in `README.packaging`; coordinate release changes there and bump `NEWS` alongside version updates.
