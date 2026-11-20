namespace Beforeguard.Frostline.WoW

open System
open System.Text.Json.Serialization

module Guild =
    type Href = {
        [<JsonPropertyName("href")>]
        Href: string
    }

    type GuildRealm = {
        [<JsonPropertyName("id")>]
        Id: int

        [<JsonPropertyName("name")>]
        Name: string

        [<JsonPropertyName("slug")>]
        Slug: string
    }

    type Rgba = {
        [<JsonPropertyName("r")>]
        R: int

        [<JsonPropertyName("g")>]
        G: int

        [<JsonPropertyName("b")>]
        B: int

        [<JsonPropertyName("a")>]
        A: float
    }

    type CrestColor = {
        [<JsonPropertyName("id")>]
        Id: int

        [<JsonPropertyName("rgba")>]
        Rgba: Rgba
    }

    type CrestComponent = {
        [<JsonPropertyName("id")>]
        Id: int

        [<JsonPropertyName("media")>]
        Media: Href option

        [<JsonPropertyName("color")>]
        Color: CrestColor
    }

    type CrestBackground = {
        [<JsonPropertyName("color")>]
        Color: CrestColor
    }

    type Crest = {
        [<JsonPropertyName("emblem")>]
        Emblem: CrestComponent

        [<JsonPropertyName("border")>]
        Border: CrestComponent

        [<JsonPropertyName("background")>]
        Background: CrestBackground
    }

    /// A guild summary returned by the WoW Guild API.
    type Guild = {
        [<JsonPropertyName("id")>]
        Id: int64

        [<JsonPropertyName("name")>]
        Name: string

        [<JsonPropertyName("faction")>]
        Faction: CharacterProfile.Faction

        [<JsonPropertyName("achievement_points")>]
        AchievementPoints: int

        [<JsonPropertyName("member_count")>]
        MemberCount: int

        [<JsonPropertyName("realm")>]
        Realm: GuildRealm

        [<JsonPropertyName("crest")>]
        Crest: Crest option

        [<JsonPropertyName("roster")>]
        Roster: Href option

        [<JsonPropertyName("achievements")>]
        Achievements: Href option

        [<JsonPropertyName("created_timestamp")>]
        CreatedTimestamp: int64

        [<JsonPropertyName("activity")>]
        Activity: Href option

        [<JsonPropertyName("name_search")>]
        NameSearch: string option
    }

    type RosterGuildRef = {
        [<JsonPropertyName("id")>]
        Id: int64

        [<JsonPropertyName("name")>]
        Name: string

        [<JsonPropertyName("realm")>]
        Realm: GuildRealm

        [<JsonPropertyName("faction")>]
        Faction: CharacterProfile.Faction
    }

    type PlayableRef = {
        [<JsonPropertyName("id")>]
        Id: int
    }

    type RosterCharacter = {
        [<JsonPropertyName("id")>]
        Id: int64

        [<JsonPropertyName("name")>]
        Name: string

        [<JsonPropertyName("level")>]
        Level: int

        [<JsonPropertyName("realm")>]
        Realm: GuildRealm

        [<JsonPropertyName("playable_class")>]
        PlayableClass: PlayableRef

        [<JsonPropertyName("playable_race")>]
        PlayableRace: PlayableRef
    }

    type RosterMember = {
        [<JsonPropertyName("character")>]
        Character: RosterCharacter

        [<JsonPropertyName("rank")>]
        Rank: int
    }

    /// A guild roster returned by the WoW Guild API.
    type GuildRoster = {
        [<JsonPropertyName("guild")>]
        Guild: RosterGuildRef

        [<JsonPropertyName("members")>]
        Members: RosterMember list
    }

    let private slugify (value: string) =
        Uri.EscapeDataString(value.Trim().ToLowerInvariant().Replace(" ", "-"))

    let private basePath (region: Beforeguard.Frostline.Core.Region) (realm: string) (guildName: string) =
        sprintf "/data/wow/guild/%s/%s" (slugify realm) (slugify guildName)

    let private query (region: Beforeguard.Frostline.Core.Region) =
        sprintf "?namespace=profile-%s&locale=en_US" (Beforeguard.Frostline.Core.Region.toString region)

    /// Gets a guild summary from the WoW Guild API.
    let get (httpClient: Beforeguard.Frostline.Core.BattleNetHttpClient)
            (region: Beforeguard.Frostline.Core.Region)
            (realm: string)
            (guildName: string) =
        async {
            let path = basePath region realm guildName + query region
            return! httpClient.getAsync<Guild>(path) |> Async.AwaitTask
        }

    /// Gets a guild roster from the WoW Guild API.
    let getRoster (httpClient: Beforeguard.Frostline.Core.BattleNetHttpClient)
                  (region: Beforeguard.Frostline.Core.Region)
                  (realm: string)
                  (guildName: string) =
        async {
            let path = basePath region realm guildName + "/roster" + query region
            return! httpClient.getAsync<GuildRoster>(path) |> Async.AwaitTask
        }
