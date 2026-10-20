# WoW Profile API Coverage Audit

**Audit date:** 2026-10-19
**Scope:** World of Warcraft Profile APIs only. This audit is documentation- and
source-based; it does not make live API requests, use credentials, or retrieve
player data.

## Official documentation

- [WoW Profile APIs](https://community.developer.battle.net/documentation/world-of-warcraft/profile-apis)
- [WoW Profile APIs structured documentation](https://community.developer.battle.net/api/pages/content/documentation/world-of-warcraft/profile-apis.json)

The structured official source documents 44 `GET` endpoints. It specifies
routes, parameters, namespaces, and locale defaults, but it does not expose
field-level response schemas.

## Endpoint coverage

| Endpoint | Method | Namespace | SDK status | SDK evidence | Test evidence | Gap or rationale |
| --- | --- | --- | --- | --- | --- | --- |
| `/profile/user/wow` | GET | `profile-{region}` | Missing | No public endpoint module. | None | Requires `wow.profile` authorization; no callable function or model. |
| `/profile/user/wow/protected-character/{realmId}-{characterId}` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/profile/user/wow/collections` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/profile/user/wow/collections/decor` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/profile/user/wow/collections/heirlooms` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/profile/user/wow/collections/mounts` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/profile/user/wow/collections/pets` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/profile/user/wow/collections/toys` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/profile/user/wow/collections/transmogs` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/profile/wow/character/{realmSlug}/{characterName}/achievements` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/profile/wow/character/{realmSlug}/{characterName}/achievements/statistics` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/profile/wow/character/{realmSlug}/{characterName}/appearance` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/profile/wow/character/{realmSlug}/{characterName}/collections` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/profile/wow/character/{realmSlug}/{characterName}/collections/decor` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/profile/wow/character/{realmSlug}/{characterName}/collections/heirlooms` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/profile/wow/character/{realmSlug}/{characterName}/collections/mounts` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/profile/wow/character/{realmSlug}/{characterName}/collections/pets` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/profile/wow/character/{realmSlug}/{characterName}/collections/toys` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/profile/wow/character/{realmSlug}/{characterName}/collections/transmogs` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/profile/wow/character/{realmSlug}/{characterName}/encounters` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/profile/wow/character/{realmSlug}/{characterName}/encounters/dungeons` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/profile/wow/character/{realmSlug}/{characterName}/encounters/raids` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/profile/wow/character/{realmSlug}/{characterName}/equipment` | GET | `profile-{region}` | Supported | `CharacterEquipment.get`; `src\Beforeguard.Frostline.WoW\CharacterEquipment.fs:6-366`; C# extension in `BattleNetHttpClientExtensions.fs:19-26`. | `tests\Beforeguard.Frostline.WoW.Tests\CharacterEquipmentTests.fs:1-251` | Public route, parameters, typed response, and regional profile namespace are covered. Locale is fixed to documented default `en_US`. |
| `/profile/wow/character/{realmSlug}/{characterName}/house/house-{houseNumber}` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/profile/wow/character/{realmSlug}/{characterName}/hunter-pets` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/profile/wow/character/{realmSlug}/{characterName}/character-media` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/profile/wow/character/{realmSlug}/{characterName}/mythic-keystone-profile` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/profile/wow/character/{realmSlug}/{characterName}/mythic-keystone-profile/season/{seasonId}` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/profile/wow/character/{realmSlug}/{characterName}/professions` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/profile/wow/character/{realmSlug}/{characterName}` | GET | `profile-{region}` | Supported | `CharacterProfile.get`; `src\Beforeguard.Frostline.WoW\CharacterProfile.fs:6-119`; C# extension in `BattleNetHttpClientExtensions.fs:9-17`. | `tests\Beforeguard.Frostline.WoW.Tests\CharacterProfileTests.fs:1-120` | Public route, typed response, lowercased realm/name behavior, regional namespace, and default locale are covered. |
| `/profile/wow/character/{realmSlug}/{characterName}/status` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/profile/wow/character/{realmSlug}/{characterName}/pvp-bracket/{pvpBracket}` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/profile/wow/character/{realmSlug}/{characterName}/pvp-summary` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/profile/wow/character/{realmSlug}/{characterName}/quests` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/profile/wow/character/{realmSlug}/{characterName}/quests/completed` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/profile/wow/character/{realmSlug}/{characterName}/reputations` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/profile/wow/character/{realmSlug}/{characterName}/soulbinds` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/profile/wow/character/{realmSlug}/{characterName}/specializations` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/profile/wow/character/{realmSlug}/{characterName}/statistics` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/profile/wow/character/{realmSlug}/{characterName}/titles` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |
| `/data/wow/guild/{realmSlug}/{nameSlug}` | GET | `profile-{region}` | Missing | No public guild endpoint module. | None | This is a Profile API despite its `/data/wow/` route. |
| `/data/wow/guild/{realmSlug}/{nameSlug}/activity` | GET | `profile-{region}` | Missing | No public guild endpoint module. | None | No callable function or model. |
| `/data/wow/guild/{realmSlug}/{nameSlug}/achievements` | GET | `profile-{region}` | Missing | No public guild endpoint module. | None | No callable function or model. |
| `/data/wow/guild/{realmSlug}/{nameSlug}/roster` | GET | `profile-{region}` | Missing | No public endpoint module. | None | No callable function or model. |

## Summary

| Status | Count |
| --- | ---: |
| Supported | 2 |
| Partial | 0 |
| Missing | 42 |
| Unverified | 0 |
| **Total** | **44** |

The WoW project currently compiles `ItemQuality.fs`, `CharacterProfile.fs`,
`CharacterEquipment.fs`, and `BattleNetHttpClientExtensions.fs`
(`src\Beforeguard.Frostline.WoW\Beforeguard.Frostline.WoW.fsproj:13-18`).
The shared client provides authenticated regional GET and JSON deserialization
in `src\Beforeguard.Frostline.Core\BattleNetHttpClient.fs:23-80`.

## Highest-value gaps

1. **Character Profile Status** — `/profile/wow/character/{realmSlug}/{characterName}/status`; a small, high-confidence availability check before richer requests.
2. **Character Achievements Summary** — `/profile/wow/character/{realmSlug}/{characterName}/achievements`; high user value and a natural complement to the base profile.
3. **Character Specializations Summary** — `/profile/wow/character/{realmSlug}/{characterName}/specializations`; provides more complete build data than the base profile's active specialization.
4. **Character Statistics Summary** — `/profile/wow/character/{realmSlug}/{characterName}/statistics`; high value for character inspection and comparison.
5. **Character Mythic Keystone Profile Index** — `/profile/wow/character/{realmSlug}/{characterName}/mythic-keystone-profile`; valuable directly and supports later seasonal details.

## Evidence limitations

- Blizzard's official structured documentation does not publish response
  object schemas or field-level contracts. Model coverage is therefore based on
  public callable functions, typed response records, and fixture-deserialization
  tests—not undocumented live responses.
- No live API calls were made. This report makes no claims about availability,
  authorization at runtime, serialization behavior against live responses, or
  regional behavior.
- Frostline derives the required `profile-{region}` namespace from the client
  region and fixes optional `locale` to Blizzard's documented default,
  `en_US`.

## Recommended next endpoint

Implement **Character Profile Status**:
`GET /profile/wow/character/{realmSlug}/{characterName}/status`.

It follows the established character route, required realm/name parameters,
profile namespace, and default locale convention. Its limited response surface
and usefulness as a preflight availability check make it the best next addition.
