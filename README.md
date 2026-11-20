<p align="center">
  <img src="frostline_icon.png" alt="Frostline" width="150"/>
</p>

# Frostline

**A modern, idiomatic F# SDK for Blizzard Entertainment's Battle.net APIs**

Frostline provides type-safe, async-first access to World of Warcraft, Diablo, StarCraft, and other Blizzard game APIs with a clean functional interface.

## Features

✨ **Pure F# Implementation**
- Built from scratch using only .NET 10 built-ins
- No external OAuth libraries - complete control over authentication flow
- Leverages F#'s discriminated unions, records, and async workflows

🔐 **OAuth 2.0 Client Credentials Flow**
- Automatic token acquisition and caching
- Smart token refresh with expiry handling
- Region-aware authentication endpoints

🌍 **Multi-Region Support**
- US, EU, KR, TW, CN regions
- Region-specific API endpoints
- China special handling

🎮 **Game API Coverage** *(in development)*
- World of Warcraft Profile API
- Character, equipment, and media endpoints
- Additional games coming soon

🤝 **C# Interoperability**
- Task-returning client methods and `Result` extensions (`Match`, `TryGetValue`)
- Real .NET optional parameters instead of F#-only optional args
- Human-readable `FrostlineError.Message` for error handling without pattern matching

## Status

**Early Development** - Core OAuth infrastructure complete and tested against live Battle.net APIs. WoW data models in progress.

This is a learning-focused project demonstrating F# best practices for SDK development.

> **⚠️ Disclaimer**: This is an unofficial, community-created project and is not affiliated with, endorsed by, or supported by Blizzard Entertainment, Inc. Battle.net, World of Warcraft, Diablo, StarCraft, and all associated properties are trademarks or registered trademarks of Blizzard Entertainment, Inc.

## Quick Start

### F#

```fsharp
open Beforeguard.Frostline.Core

// Configure with your Battle.net credentials
let config = ClientConfig.create "your-client-id" "your-secret" Region.US

// Authenticate and make API calls
use httpClient = new BattleNetHttpClient(config)

// Call any Battle.net endpoint
let! response = 
    httpClient.getAsync("/data/wow/achievement-category/index?namespace=static-us&locale=en_US")
    |> Async.AwaitTask
```

### C#

```csharp
using Beforeguard.Frostline.Core;
using Beforeguard.Frostline.WoW;

// Configure with your Battle.net credentials
var config = ClientConfig.Create("your-client-id", "your-secret", Region.US);

// Authenticate and make API calls
using var httpClient = new BattleNetHttpClient(config);

// Call a WoW endpoint directly - no F# Result/Async knowledge required
var result = await httpClient.GetCharacterProfileAsync("tichondrius", "charactername");

result.Match(
    onOk: profile => { Console.WriteLine($"Character: {profile.Name}"); return 0; },
    onError: err => { Console.WriteLine($"Error: {err.Message}"); return 1; });
```

## Project Structure

```
src/
  Beforeguard.Frostline.Core/     # Core OAuth and HTTP infrastructure
  Beforeguard.Frostline.WoW/      # World of Warcraft API wrapper

cli/
  Beforeguard.Frostline.Cli/      # Test harness and examples

tests/
  Beforeguard.Frostline.Core.Tests/
  Beforeguard.Frostline.Core.Tests.CSharp/
  Beforeguard.Frostline.WoW.Tests/
  Beforeguard.Frostline.Cli.Tests/
```

## Getting Started

### Prerequisites

- .NET 10 SDK
- Battle.net API credentials (get them at https://develop.battle.net)

### Configuration

The SDK uses environment variables for configuration:

```bash
# Set your Battle.net credentials
export BNET_CLIENT_ID="your-client-id"
export BNET_CLIENT_SECRET="your-client-secret"
export BNET_REGION="US"  # US, EU, KR, TW, or CN
```

For the CLI test harness, use .NET User Secrets:

```bash
dotnet user-secrets set "BattleNet:ClientId" "your-client-id" --project cli/Beforeguard.Frostline.Cli
dotnet user-secrets set "BattleNet:ClientSecret" "your-secret" --project cli/Beforeguard.Frostline.Cli
dotnet user-secrets set "BattleNet:Region" "us" --project cli/Beforeguard.Frostline.Cli
```

### Running the CLI

```bash
dotnet run --project cli/Beforeguard.Frostline.Cli
```

### Capturing test fixtures with `probe`

`frostline probe` fetches a raw Battle.net response so it can be turned into a test fixture:

```text
frostline probe "<path>" [--print] [--sanitize] [--fixture <Name>]
```

- `--print` prints the JSON to stdout instead of saving.
- `--sanitize` replaces character, realm, guild, and account names, slugs, and IDs with deterministic placeholders (the same input always maps to the same placeholder within a file), keeps game data and the JSON structure, and redacts unrecognized string fields (reported as warnings). In `href` and `key` URLs it preserves the host, endpoint shape, namespace, locale, and non-identifying values while replacing only identifying path segments and query values with the same mappings used by the surrounding JSON; non-identifying URLs and non-URL `key` values remain intact.
- `--fixture <Name>` writes to `tests/Beforeguard.Frostline.WoW.Tests/Fixtures/<Name>.json`, which is embedded in the test assembly.
- With neither `--print` nor `--fixture`, the response is saved to the gitignored `samples/` folder. `--print` and `--fixture` cannot be combined.

Example:

```bash
dotnet run --project cli/Beforeguard.Frostline.Cli -- probe "/profile/wow/character/tichondrius/beforeguard/equipment?namespace=profile-us&locale=en_US" --sanitize --fixture CharacterEquipment
```

Workflow: a human with credentials runs `probe --sanitize --fixture <Name>`, reviews the output, and commits it. Contributors and agents work only from committed fixtures and never run `probe` or handle credentials; if a fixture is missing, ask a maintainer to capture one.

## Why F#?

- **Type Safety**: Discriminated unions model API regions and enums perfectly
- **Null Safety**: Option types eliminate null reference errors
- **Async-First**: Native async workflows for API calls
- **Pattern Matching**: Elegant error handling and response parsing
- **Immutability**: Thread-safe by default

## Roadmap

- [x] OAuth 2.0 Client Credentials authentication
- [x] Multi-region support
- [x] Token caching and automatic refresh
- [x] HTTP client with bearer token injection
- [x] Error handling with Result types
- [ ] Retry logic and rate limiting
- [x] WoW Character Profile API
- [x] WoW Character Equipment API
- [ ] WoW Media API
- [ ] Static data endpoints (items, achievements)
- [ ] Additional game APIs (Diablo, StarCraft)

## Contributing

This project is in active development. Contributions, suggestions, and feedback welcome!

## Legal

This project is not affiliated with, endorsed by, or supported by Blizzard Entertainment, Inc. 

Battle.net®, World of Warcraft®, Diablo®, StarCraft®, and related logos are registered trademarks or trademarks of Blizzard Entertainment, Inc. in the U.S. and/or other countries.

This SDK uses the publicly available Battle.net API which requires users to obtain their own API credentials from https://develop.battle.net and agree to Blizzard's API Terms of Use.

## License

MIT

---

*Built with ❄️ by developers who love both F# and Blizzard games*
