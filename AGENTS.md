# Repository Guidelines

## Project Structure & Module Organization
- `src/`: Core library (packable). Domain-specific partials under `Services/PolarClient/*.cs`, models under `Models/*`.
- `samples/`: Runnable examples — `polar.sample` (console) and `polar.webhook` (ASP.NET Core).
- `tests/PolarNet.Tests`: xUnit tests (unit + integration). Uses optional sandbox config.
- `docs/`: Planning and integration notes; assets in `docs/logo-files/`.

## Build, Test, and Development Commands
- Restore: `dotnet restore`
- Build (all TFMs): `dotnet build -c Debug` (or `Release`)
- Tests (all): `dotnet test -c Debug`
- Tests (by trait): `dotnet test --filter "Category=Unit"`
- Run console sample: `cd samples/polar.sample && dotnet run`
- Run webhook sample: `cd samples/polar.webhook && dotnet run`

## Coding Style & Naming Conventions
- Language: C#; 4-space indentation; UTF-8.
- Naming: PascalCase for types/methods; camelCase for locals/parameters; async methods end with `Async`.
- Documentation: All source comments/XML docs must be in English. Use `/// <summary>...</summary>` on public APIs.
- Organization: Add new endpoints as partials in `src/Services/PolarClient/` (one domain per file, e.g., `Payments.cs`). Match file name to domain.

## Testing Guidelines
- Framework: xUnit. Unit tests live in `tests/PolarNet.Tests/*Tests.cs`.
- Traits: mark with `Trait("Category", "Unit")` or `"Integration"`. Integration tests require sandbox config.
- Config for tests: `tests/appsettings.json` or env vars with `POLAR_TEST_` prefix. Do not commit secrets.
- Aim to cover new/changed code paths; validate deserialization shapes.

## Commit & Pull Request Guidelines
- Commits: clear, concise, imperative. Prefer Conventional Commits (e.g., `feat:`, `fix:`, `docs:`). Keep subject ≤ 72 chars; body wraps at 100.
- PRs: describe intent and scope, link issues, include testing notes and screenshots/logs when relevant. Ensure no secrets or binaries are committed.

## Security & Configuration Tips
- Use Sandbox by default. Configure in `samples/*/appsettings.json` and `tests/appsettings.json`.
- Keep tokens/secrets out of VCS; use environment variables locally/CI.
- Base URL should not include trailing `/v1` (library adds paths).
