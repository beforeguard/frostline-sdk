---
name: wow-new-endpoint
description: Generates new Battle.net World of Warcraft API endpoints, models, and fixture-based tests using Frostline conventions
applyTo:
  - "src/Beforeguard.Frostline.WoW/**"
  - "tests/Beforeguard.Frostline.WoW.Tests/**"
---

# WoW New Endpoint Generator

Generate new Battle.net World of Warcraft API endpoints in Frostline. Follow the established patterns in the WoW project before adding new abstractions.

## Using This Agent

In GitHub Copilot CLI, select this agent:

```text
/agent wow-new-endpoint
```

Then provide the endpoint details and a sanitized response fixture:

```text
Add the WoW character media endpoint. Use the endpoint path
/profile/wow/character/{realmSlug}/{characterName}/character-media, namespace
profile-us, and the following sanitized JSON response:

{ ... }
```

## Scope

This agent may add:

- World of Warcraft API data models
- Endpoint modules that call `BattleNetHttpClient`
- Fixture-based JSON deserialization tests
- Project-file entries required to compile new F# files
- Small, directly related documentation updates

Do not change shared OAuth, HTTP, error-mapping, or token-management behavior in `Beforeguard.Frostline.Core` unless the task explicitly requires it.

## Required Inputs

Before implementing an endpoint, obtain or identify:

1. The Battle.net endpoint path and HTTP method.
2. The required namespace query parameter, such as `profile-us` or `static-us`.
3. Supported region, locale, and path parameters.
4. A sanitized, representative JSON response captured with `frostline probe`, when live credentials are available, or from official API documentation.
5. Whether the endpoint is character/profile data, static data, game data, or media.
6. Any required API permissions or OAuth scopes.

Do not invent response fields or infer JSON shapes from endpoint names. If a sample response is unavailable, ask for one or implement only the endpoint shell with the uncertainty clearly stated.

To capture a live response for scaffolding, use:

```text
frostline probe "<endpoint-path-and-query>" --print
```

The CLI requires Battle.net credentials for this command. If a capture is saved instead of printed, it is written under `samples/`, which is gitignored. Never commit a raw response: trim it to the required shape and sanitize character, realm, guild, account, and other personal data before using it as a test fixture.

## Repository Patterns

Review the closest existing implementation before editing:

- `src/Beforeguard.Frostline.WoW/CharacterProfile.fs`
- `src/Beforeguard.Frostline.WoW/CharacterEquipment.fs`
- `tests/Beforeguard.Frostline.WoW.Tests/CharacterProfileTests.fs`
- `tests/Beforeguard.Frostline.WoW.Tests/CharacterEquipmentTests.fs`

Reuse existing shared types when they accurately model a nested API object. Do not duplicate types solely because they appear in more than one response.

## Data Model Conventions

All WoW API types must be nested within a module matching the resource name.

```fsharp
namespace Beforeguard.Frostline.WoW

open System.Text.Json.Serialization

module CharacterProfile =
    /// A World of Warcraft character profile returned by Battle.net.
    type CharacterProfile = {
        [<JsonPropertyName("id")>]
        Id: int64

        [<JsonPropertyName("name")>]
        Name: string

        [<JsonPropertyName("character_class")>]
        CharacterClass: CharacterClass
    }
```

Follow these rules:

- Use F# records for API response types.
- Add XML documentation to public types where the purpose is not obvious from the name.
- Use `[<JsonPropertyName("...")]` on every serialized field.
- Convert JSON `snake_case` names to PascalCase F# properties.
- Use `int64` for Blizzard entity IDs unless the existing model for that API field establishes another type.
- Use `int` for bounded numeric values such as level, item level, or quantity.
- Use `string` for API strings and identifiers unless an existing discriminated union already models the value.
- Use `option` for fields that are absent or nullable in valid API responses.
- Model nested API objects as nested records or reuse an existing shared record.
- Preserve API response structure; do not flatten nested objects merely for convenience.
- Avoid `obj`, dictionaries, or untyped JSON access in public response models.

## Endpoint Function Conventions

When adding a client-facing endpoint function, follow the existing `CharacterProfile.get` style.

- Construct the Battle.net path, including required namespace and locale parameters.
- Accept `BattleNetHttpClient` and `Region` as dependencies rather than creating a client internally.
- Return `Async<Result<'T, FrostlineError>>`.
- Delegate authentication, HTTP execution, deserialization, and HTTP-status error mapping to `BattleNetHttpClient.getAsync`.
- Do not log credentials, access tokens, or raw response content.
- Use URI-safe handling for user-controlled path values where the existing codebase does so; do not concatenate unescaped values into a URL.

Example shape:

```fsharp
/// Gets a character profile from the WoW Profile API.
let get
    (httpClient: BattleNetHttpClient)
    (region: Region)
    (realm: string)
    (characterName: string)
    : Async<Result<CharacterProfile, FrostlineError>> =
    let path =
        sprintf
            "/profile/wow/character/%s/%s?namespace=profile-%s&locale=en_US"
            realm
            characterName
            (Region.toString region)

    httpClient.getAsync<CharacterProfile>(path)
    |> Async.AwaitTask
```

Match the actual endpoint's namespace, locale behavior, and route structure rather than copying this example unchanged.

## Test Conventions

Create fixture-based tests in the corresponding WoW test project.

```fsharp
module CharacterProfileTests

open System.Text.Json
open Xunit
open Beforeguard.Frostline.WoW.CharacterProfile

let sampleCharacterProfileJson = """
{
  "id": 123456789,
  "name": "Example",
  "character_class": {
    "id": 7,
    "name": "Shaman"
  }
}
"""

[<Fact>]
let ``deserialize character profile with required fields`` () =
    let options = JsonSerializerOptions()
    options.PropertyNameCaseInsensitive <- true

    let profile =
        JsonSerializer.Deserialize<CharacterProfile>(
            sampleCharacterProfileJson,
            options
        )

    Assert.NotNull(profile)
    Assert.Equal(123456789L, profile.Id)
    Assert.Equal("Example", profile.Name)
    Assert.Equal("Shaman", profile.CharacterClass.Name)
```

Tests must:

- Use representative, sanitized response fixtures.
- Cover a complete response with all modeled optional fields.
- Cover a minimal valid response where optional fields are omitted.
- Assert important field mappings, nested records, lists, and `option` behavior.
- Remain deterministic and run without network access or credentials.
- Avoid embedding access tokens, client secrets, account identifiers, or private character data.

Do not add live API tests unless the task explicitly requests them. If live tests are added, they must be opt-in, excluded from the default test run, and load credentials only from supported secret configuration.

## F# Project Files

F# compilation order is significant.

When adding a source or test file:

1. Add the file to its `.fsproj`.
2. Place shared types before endpoint modules that use them.
3. Place tests after the files they reference.
4. Preserve the existing project-file ordering style.

Do not assume SDK-style wildcard inclusion will compile F# files in the needed order.

## Validation

For a normal model or endpoint addition, run the smallest relevant test project:

```powershell
dotnet test tests\Beforeguard.Frostline.WoW.Tests\Beforeguard.Frostline.WoW.Tests.fsproj --no-restore
```

Run the affected project build when changes include source files, module dependencies, or `.fsproj` entries:

```powershell
dotnet build src\Beforeguard.Frostline.WoW\Beforeguard.Frostline.WoW.fsproj --no-restore
```

Address new warnings or failures caused by the change. Do not modify unrelated code solely to remove pre-existing warnings.

## Completion Checklist

Before completing the task, verify that:

- The endpoint path and namespace match the Battle.net API contract.
- Every modeled JSON field has the correct `JsonPropertyName`.
- Optional API fields use `option`.
- IDs use types consistent with existing Frostline models.
- New `.fs` files are included in the correct compilation order.
- Fixture tests cover complete and minimal valid responses.
- Default tests do not require Battle.net credentials or network access.
- No secrets, tokens, or private player data were added to the repository.
