module GuildTests

open System.Text.Json
open Xunit
open Beforeguard.Frostline.WoW.Guild

let private options =
    let o = JsonSerializerOptions()
    o.PropertyNameCaseInsensitive <- true
    o

let completeGuildJson = """
{
  "id": 1234567,
  "name": "Example Guild",
  "faction": { "type": "HORDE", "name": "Horde" },
  "achievement_points": 5400,
  "member_count": 120,
  "realm": { "id": 3676, "name": "Area 52", "slug": "area-52" },
  "crest": {
    "emblem": {
      "id": 12,
      "media": { "href": "https://example.invalid/media/emblem" },
      "color": { "id": 1, "rgba": { "r": 255, "g": 200, "b": 0, "a": 1 } }
    },
    "border": {
      "id": 3,
      "media": { "href": "https://example.invalid/media/border" },
      "color": { "id": 2, "rgba": { "r": 10, "g": 20, "b": 30, "a": 1 } }
    },
    "background": {
      "color": { "id": 4, "rgba": { "r": 0, "g": 0, "b": 128, "a": 1 } }
    }
  },
  "roster": { "href": "https://example.invalid/roster" },
  "achievements": { "href": "https://example.invalid/achievements" },
  "created_timestamp": 1500000000000,
  "activity": { "href": "https://example.invalid/activity" },
  "name_search": "example-guild"
}
"""

let minimalGuildJson = """
{
  "id": 7654321,
  "name": "Tiny Guild",
  "faction": { "type": "ALLIANCE", "name": "Alliance" },
  "achievement_points": 0,
  "member_count": 1,
  "realm": { "id": 1, "name": "Area 52", "slug": "area-52" },
  "created_timestamp": 1600000000000
}
"""

let rosterJson = """
{
  "guild": {
    "id": 1234567,
    "name": "Example Guild",
    "realm": { "id": 3676, "name": "Area 52", "slug": "area-52" },
    "faction": { "type": "HORDE", "name": "Horde" }
  },
  "members": [
    {
      "character": {
        "name": "Leader",
        "id": 111,
        "realm": { "id": 3676, "slug": "area-52" },
        "level": 80,
        "playable_class": { "id": 7 },
        "playable_race": { "id": 2 }
      },
      "rank": 0
    },
    {
      "character": {
        "name": "Member",
        "id": 222,
        "realm": { "id": 3676, "slug": "area-52" },
        "level": 75,
        "playable_class": { "id": 1 },
        "playable_race": { "id": 10 }
      },
      "rank": 3
    }
  ]
}
"""

[<Fact>]
let ``deserialize complete guild with all fields`` () =
    let guild = JsonSerializer.Deserialize<Guild>(completeGuildJson, options)

    Assert.Equal(1234567L, guild.Id)
    Assert.Equal("Example Guild", guild.Name)
    Assert.Equal("Horde", guild.Faction.Name)
    Assert.Equal(5400, guild.AchievementPoints)
    Assert.Equal(120, guild.MemberCount)
    Assert.Equal("area-52", guild.Realm.Slug)
    Assert.Equal(1500000000000L, guild.CreatedTimestamp)
    Assert.True(guild.Crest.IsSome)
    Assert.Equal(12, guild.Crest.Value.Emblem.Id)
    Assert.Equal(255, guild.Crest.Value.Emblem.Color.Rgba.R)
    Assert.Equal(128, guild.Crest.Value.Background.Color.Rgba.B)
    Assert.True(guild.Roster.IsSome)
    Assert.True(guild.Achievements.IsSome)
    Assert.True(guild.Activity.IsSome)
    Assert.Equal(Some "example-guild", guild.NameSearch)

[<Fact>]
let ``deserialize minimal guild without optional fields`` () =
    let guild = JsonSerializer.Deserialize<Guild>(minimalGuildJson, options)

    Assert.Equal("Tiny Guild", guild.Name)
    Assert.Equal(1, guild.MemberCount)
    Assert.True(guild.Crest.IsNone)
    Assert.True(guild.Roster.IsNone)
    Assert.True(guild.Achievements.IsNone)
    Assert.True(guild.Activity.IsNone)
    Assert.True(guild.NameSearch.IsNone)

[<Fact>]
let ``deserialize guild roster with members`` () =
    let roster = JsonSerializer.Deserialize<GuildRoster>(rosterJson, options)

    Assert.Equal("Example Guild", roster.Guild.Name)
    Assert.Equal(2, roster.Members.Length)
    let leader = roster.Members.Head
    Assert.Equal("Leader", leader.Character.Name)
    Assert.Equal(0, leader.Rank)
    Assert.Equal(7, leader.Character.PlayableClass.Id)
    Assert.Equal(10, roster.Members.[1].Character.PlayableRace.Id)

[<Fact>]
let ``deserialize guild roster with no members`` () =
    let json = """{ "guild": { "id": 1, "name": "Empty", "realm": { "id": 1, "name": "A", "slug": "a" }, "faction": { "type": "HORDE", "name": "Horde" } }, "members": [] }"""
    let roster = JsonSerializer.Deserialize<GuildRoster>(json, options)

    Assert.Empty(roster.Members)
