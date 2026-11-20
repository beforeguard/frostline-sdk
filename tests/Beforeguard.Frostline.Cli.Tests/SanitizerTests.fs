module SanitizerTests

open System.Text.Json
open Xunit
open Beforeguard.Frostline.Cli

let private run (json: string) =
    use doc = JsonDocument.Parse json
    Sanitizer.sanitize doc.RootElement

let private profileJson = """
{
  "id": 123456789,
  "name": "Thrall",
  "realm": { "id": 3676, "name": "Area 52", "slug": "area-52" },
  "character_class": { "id": 7, "name": "Shaman" },
  "level": 70,
  "guild": { "id": 555, "name": "Secret Guild", "realm": { "id": 3676, "name": "Area 52", "slug": "area-52" } },
  "equipped_items": [ { "item": { "id": 207172 }, "name": "Amirdrassil Vengeance", "quality": { "type": "EPIC", "name": "Epic" } } ]
}
"""

[<Fact>]
let ``removes identifying names and ids`` () =
    let result = run profileJson

    for secret in [ "Thrall"; "Area 52"; "area-52"; "Secret Guild"; "123456789"; "3676"; "555" ] do
        Assert.DoesNotContain(secret, result.Json)

[<Fact>]
let ``keeps game data and structure`` () =
    let result = run profileJson
    use original = JsonDocument.Parse profileJson
    use sanitized = JsonDocument.Parse result.Json

    Assert.Contains("Amirdrassil Vengeance", result.Json)
    Assert.Contains("Shaman", result.Json)
    Assert.Contains("207172", result.Json)

    let rec shape (e: JsonElement) : string =
        match e.ValueKind with
        | JsonValueKind.Object -> e.EnumerateObject() |> Seq.map (fun p -> p.Name + ":" + shape p.Value) |> String.concat ","
        | JsonValueKind.Array -> "[" + (e.EnumerateArray() |> Seq.map shape |> String.concat ",") + "]"
        | k -> string k

    Assert.Equal(shape original.RootElement, shape sanitized.RootElement)

[<Fact>]
let ``maps the same value consistently within a file`` () =
    let result = run profileJson
    use doc = JsonDocument.Parse result.Json
    let root = doc.RootElement
    let realm = root.GetProperty("realm")
    let guildRealm = root.GetProperty("guild").GetProperty("realm")

    Assert.Equal(realm.GetProperty("name").GetString(), guildRealm.GetProperty("name").GetString())
    Assert.Equal(realm.GetProperty("slug").GetString(), guildRealm.GetProperty("slug").GetString())
    Assert.Equal(realm.GetProperty("id").GetInt64(), guildRealm.GetProperty("id").GetInt64())

[<Fact>]
let ``is deterministic`` () =
    Assert.Equal(run(profileJson).Json, run(profileJson).Json)

[<Fact>]
let ``different values map to different placeholders`` () =
    let result = run """{ "members": [ { "character": { "id": 1, "name": "Alpha", "realm": { "id": 1, "slug": "x" } } }, { "character": { "id": 2, "name": "Beta", "realm": { "id": 1, "slug": "x" } } } ] }"""
    use doc = JsonDocument.Parse result.Json
    let names =
        doc.RootElement.GetProperty("members").EnumerateArray()
        |> Seq.map (fun m -> m.GetProperty("character").GetProperty("name").GetString())
        |> Seq.toList

    Assert.Equal<string list>([ "Character1"; "Character2" ], names)

[<Fact>]
let ``redacts unknown string fields and reports them`` () =
    let result = run """{ "note": "Thrall was here", "tags": [ "Thrall" ], "level": 70 }"""

    Assert.DoesNotContain("Thrall", result.Json)
    Assert.Equal<string list>([ "$.note"; "$.tags[0]" ], result.Redacted)
    Assert.Contains("70", result.Json)

[<Fact>]
let ``sanitizes identifying URL path segments with the surrounding mapping`` () =
    let result =
        run
            """{
              "character": {
                "id": 123456789,
                "name": "Thrall",
                "realm": { "id": 3676, "name": "Area 52", "slug": "area-52" }
              },
              "self": { "href": "https://us.api.blizzard.com/profile/wow/character/area-52/thrall?namespace=profile-us&locale=en_US" }
            }"""

    use doc = JsonDocument.Parse result.Json
    let root = doc.RootElement
    let character = root.GetProperty("character")
    let href = root.GetProperty("self").GetProperty("href").GetString()

    Assert.Equal("Character1", character.GetProperty("name").GetString())
    Assert.Contains("/character/realm-1/character-1", href)
    Assert.Contains("?namespace=profile-us&locale=en_US", href)
    Assert.StartsWith("https://us.api.blizzard.com/", href)
    Assert.DoesNotContain("thrall", href)
    Assert.DoesNotContain("area-52", href)

[<Fact>]
let ``keeps links distinct and related to their surrounding values`` () =
    let result =
        run
            """{
              "guild": {
                "id": 555,
                "name": "Secret Guild",
                "realm": { "id": 3676, "name": "Area 52", "slug": "area-52" },
                "key": { "href": "https://eu.api.blizzard.com/data/wow/guild/area-52/secret-guild?namespace=profile-eu" }
              },
              "members": [
                { "character": {
                    "id": 111,
                    "name": "Alpha",
                    "realm": { "id": 3676, "slug": "area-52" },
                    "key": { "href": "https://eu.api.blizzard.com/profile/wow/character/area-52/alpha?namespace=profile-eu" }
                  } },
                { "character": {
                    "id": 222,
                    "name": "Beta",
                    "realm": { "id": 3676, "slug": "area-52" },
                    "key": { "href": "https://eu.api.blizzard.com/profile/wow/character/area-52/beta?namespace=profile-eu" }
                  } }
              ],
              "key": "GUILD_CREED"
            }"""

    use doc = JsonDocument.Parse result.Json
    let members = doc.RootElement.GetProperty("members").EnumerateArray() |> Seq.toArray
    let firstHref = members[0].GetProperty("character").GetProperty("key").GetProperty("href").GetString() |> nonNull
    let secondHref = members[1].GetProperty("character").GetProperty("key").GetProperty("href").GetString() |> nonNull
    let guildHref = doc.RootElement.GetProperty("guild").GetProperty("key").GetProperty("href").GetString() |> nonNull

    Assert.Contains("/character/realm-1/character-1", firstHref)
    Assert.Contains("/character/realm-1/character-2", secondHref)
    Assert.Contains("/guild/realm-1/guild-1", guildHref)
    Assert.NotEqual<string>(firstHref, secondHref)
    Assert.Equal("GUILD_CREED", doc.RootElement.GetProperty("key").GetString())

[<Fact>]
let ``sanitizes identifying query values without changing non identifying values`` () =
    let result =
        run
            """{
              "character": { "id": 123, "name": "Thrall", "realm": { "id": 3676, "slug": "area-52" } },
              "href": "https://us.api.blizzard.com/data/wow/search?character_id=123&character=Thrall&realm=area-52&namespace=profile-us&locale=en_US"
            }"""

    use doc = JsonDocument.Parse result.Json
    let href = doc.RootElement.GetProperty("href").GetString()

    Assert.Contains("character_id=900000001", href)
    Assert.Contains("character=character-1", href)
    Assert.Contains("realm=realm-1", href)
    Assert.Contains("namespace=profile-us", href)
    Assert.Contains("locale=en_US", href)

[<Fact>]
let ``leaves non identifying URLs intact`` () =
    let url = "https://us.api.blizzard.com/data/wow/media/item/207172?namespace=static-us&locale=en_US"
    let result = run ($"""{{ "href": "{url}", "key": "{url}" }}""")

    use doc = JsonDocument.Parse result.Json
    Assert.Equal(url, doc.RootElement.GetProperty("href").GetString())
    Assert.Equal(url, doc.RootElement.GetProperty("key").GetString())

[<Fact>]
let ``leaves index endpoints intact`` () =
    let realmIndex = "https://us.api.blizzard.com/data/wow/realm/index?namespace=dynamic-us"
    let connectedRealmIndex = "https://us.api.blizzard.com/data/wow/connected-realm/index?namespace=dynamic-us"
    let result = run ($"""{{ "links": [ {{ "href": "{realmIndex}" }}, {{ "href": "{connectedRealmIndex}" }} ] }}""")

    use doc = JsonDocument.Parse result.Json
    let links = doc.RootElement.GetProperty("links").EnumerateArray() |> Seq.toArray

    Assert.Equal(realmIndex, links[0].GetProperty("href").GetString())
    Assert.Equal(connectedRealmIndex, links[1].GetProperty("href").GetString())

[<Fact>]
let ``redacts a malformed href but preserves a non URL key`` () =
    let result = run """{ "href": "not a URL", "key": "NON_URL_KEY" }"""

    use doc = JsonDocument.Parse result.Json
    Assert.Equal(Sanitizer.redactedValue, doc.RootElement.GetProperty("href").GetString())
    Assert.Equal("NON_URL_KEY", doc.RootElement.GetProperty("key").GetString())
    Assert.Equal<string list>([ "$.href" ], result.Redacted)
