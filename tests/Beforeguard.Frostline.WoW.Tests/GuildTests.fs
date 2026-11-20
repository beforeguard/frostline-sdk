module GuildTests

open System.Text.Json
open Xunit
open Beforeguard.Frostline.WoW.Guild

let private options =
    let o = JsonSerializerOptions()
    o.PropertyNameCaseInsensitive <- true
    o

let completeGuildJson = Fixtures.load "Guild"
let minimalGuildJson = Fixtures.load "GuildMinimal"
let rosterJson = Fixtures.load "GuildRoster"




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
