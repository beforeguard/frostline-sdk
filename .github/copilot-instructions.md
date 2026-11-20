# Frostline repository instructions

Use these rules with the closest existing implementation, project file, tests, README, and workflow. Prefer a small, local change over a new abstraction or unrelated cleanup.

## Architecture and languages

- Use .NET 10. `src/Beforeguard.Frostline.Core/` owns OAuth, region, HTTP, and shared error/result behavior; `src/Beforeguard.Frostline.WoW/` owns WoW response models and endpoint functions; `cli/Beforeguard.Frostline.Cli/` is the executable harness.
- Tests live under `tests/`. Keep the C# interop suite in `tests/Beforeguard.Frostline.Core.Tests.CSharp/` and test each other project in its matching F# test project.
- Write library code primarily in F# using records, discriminated unions, modules, `option`, and async workflows. Keep C# surfaces idiomatic: `Task`-returning endpoint methods plus `ResultExtensions` (`Match` and `TryGetValue`) and `FrostlineError.Message`.
- Preserve API response structure. Annotate every serialized JSON field with `JsonPropertyName`, use PascalCase F# members, model valid absent or nullable values as `option`, and avoid `obj` or untyped public JSON models.
- Reuse `FrostlineError` and the existing `Async<Result<'T, FrostlineError>>`/C# `Task<Result<...>>` boundary. Map authentication, HTTP, and deserialization failures through the shared client; do not introduce endpoint-specific exception flows or log credentials, tokens, or raw responses.

## F# projects

- F# compile order is explicit. Add every new `.fs` file to its `.fsproj`, after definitions it depends on and before files that depend on it; preserve the project's existing ordering style.
- Place source dependencies before consumers and test helpers/fixtures before tests. Do not rely on SDK wildcard ordering.

## Testing and fixtures

- Default tests must be deterministic and require neither network access nor credentials.
- Use committed sanitized WoW fixtures from `tests/Beforeguard.Frostline.WoW.Tests/Fixtures/`, embedded by `Beforeguard.Frostline.WoW.Tests.fsproj` and loaded with `Fixtures.load "<Name>"`. Cover complete and minimal valid responses when optional fields exist.
- A human with Battle.net credentials captures fixtures with `frostline probe "<path>" --sanitize --fixture <Name>` and reviews them before commit. Agents and contributors must not run `probe`, handle credentials/tokens/raw responses, or hand-write a replacement response; request a maintainer capture when a fixture is missing.

## Battle.net contracts and privacy

- Treat official Blizzard documentation linked from `https://community.developer.battle.net/documentation` as the API contract authority. Verify method, route, path/query parameters, namespace, locale, response shape, and permissions; do not infer contracts from endpoint names or third-party sources.
- Never add or expose Battle.net client secrets, access tokens, account identifiers, or private player/guild data. Load local CLI credentials only through the supported user-secrets/environment configuration. Keep identifying data out of fixtures, logs, tests, and documentation.
- For public endpoint additions, keep the established F# endpoint function and add a C#-friendly extension where comparable endpoints do.

## Build and validation

- Run the smallest affected project first. From the repository root, use:
  - `dotnet build src/Beforeguard.Frostline.Core/Beforeguard.Frostline.Core.fsproj`
  - `dotnet build src/Beforeguard.Frostline.WoW/Beforeguard.Frostline.WoW.fsproj`
  - `dotnet test tests/Beforeguard.Frostline.Core.Tests/Beforeguard.Frostline.Core.Tests.fsproj`
  - `dotnet test tests/Beforeguard.Frostline.Core.Tests.CSharp/Beforeguard.Frostline.Core.Tests.CSharp.csproj`
  - `dotnet test tests/Beforeguard.Frostline.WoW.Tests/Beforeguard.Frostline.WoW.Tests.fsproj`
  - `dotnet test tests/Beforeguard.Frostline.Cli.Tests/Beforeguard.Frostline.Cli.Tests.fsproj`
- Add `--no-restore` only after restore is current. For cross-project or repository-wide changes, run `dotnet build Beforeguard.Frostline.slnx -c Release` and `dotnet test Beforeguard.Frostline.slnx --no-build -c Release`.
- Do not change dependency versions or application behavior for a documentation-only task. Address new warnings/failures caused by the change, not unrelated pre-existing warnings.

## Specialized work, PRs, and releases

- For WoW endpoint implementation, use the task-specific guidance in `.github/agents/wow-new-endpoint.agent.md`. For read-only contract coverage audits, use `.github/agents/wow-api-endpoint-coverage.agent.md`. These profiles refine this file; do not copy their full role instructions here.
- PR titles must follow `.github/workflows/pr-title.yml`: `<type>: <description>` or `<type>(<scope>): <description>`, using `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `build`, `ci`, or `chore`.
- Versions, tags, changelog, NuGet publishing, and GitHub releases are release work, not ordinary feature or documentation changes. Follow `scripts/release-start.ps1`, `scripts/release-finish.ps1`, `.versionize`, and `.github/workflows/publish.yml` only for an explicitly requested release. Do not publish packages or push tags from a normal task.

## Completion checks

- Verify every changed path and command against the current repository.
- Run the smallest relevant build/test; for cross-cutting changes, also reproduce the Release CI build/test sequence.
- Confirm `.fsproj` order, JSON field mapping, complete/minimal fixtures, C# interop where applicable, and existing error behavior remain correct.
- Review the diff for secrets, private data, generated artifacts, unrelated changes, dependency/version edits, stale roadmap claims, and contradictions with the code, README, workflows, or specialized agents.
