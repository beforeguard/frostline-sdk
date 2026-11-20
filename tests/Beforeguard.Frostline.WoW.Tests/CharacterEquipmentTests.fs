module CharacterEquipmentTests

open System
open System.Text.Json
open Xunit
open Beforeguard.Frostline.WoW.CharacterEquipment

// Sample JSON response from Battle.net API with complete equipment
let sampleEquipmentJson = Fixtures.load "CharacterEquipment"
let minimalEquipmentJson = Fixtures.load "CharacterEquipmentMinimal"

[<Fact>]
let ``deserialize complete character equipment with all fields`` () =
    // Arrange
    let options = JsonSerializerOptions()
    options.PropertyNameCaseInsensitive <- true
    
    // Act
    let equipment = JsonSerializer.Deserialize<CharacterEquipment>(sampleEquipmentJson, options)
    
    // Assert
    Assert.Equal(123456789L, equipment.Character.Id)
    Assert.Equal("Thrall", equipment.Character.Name)
    Assert.Equal("area-52", equipment.Character.Realm.Slug)
    Assert.Equal(2, equipment.EquippedItems.Length)
    
    // Check first item (head with enchantment)
    let headItem = equipment.EquippedItems.[0]
    Assert.Equal("Amirdrassil Vengeance", headItem.Name)
    Assert.Equal("HEAD", headItem.Slot.Type)
    Assert.Equal("Epic", headItem.Quality.Name)
    Assert.Equal(528, headItem.Level.Value)
    Assert.True(headItem.Enchantments.IsSome)
    Assert.Single(headItem.Enchantments.Value) |> ignore
    Assert.Equal("Enchanted: +50 Intellect", headItem.Enchantments.Value.[0].DisplayString)
    
    // Check second item (chest without enchantment)
    let chestItem = equipment.EquippedItems.[1]
    Assert.Equal("Obsidian Cobraskin Vest", chestItem.Name)
    Assert.Equal("CHEST", chestItem.Slot.Type)
    Assert.Equal("Rare", chestItem.Quality.Name)
    Assert.Equal(424, chestItem.Level.Value)
    Assert.True(chestItem.Enchantments.IsNone)

// Minimal equipment response

[<Fact>]
let ``deserialize minimal character equipment without optional fields`` () =
    // Arrange
    let options = JsonSerializerOptions()
    options.PropertyNameCaseInsensitive <- true
    
    // Act
    let equipment = JsonSerializer.Deserialize<CharacterEquipment>(sampleEquipmentJson, options)
    
    // Assert
    Assert.NotNull(equipment)
    Assert.NotNull(equipment.Character)
    Assert.NotEmpty(equipment.EquippedItems)
    
    // Verify optional fields can be absent
    let item = equipment.EquippedItems |> List.find (fun i -> i.Slot.Type = "CHEST")
    Assert.True(item.Enchantments.IsNone || item.Enchantments.Value.IsEmpty)

[<Fact>]
let ``equipment items have correct slot types`` () =
    // Arrange
    let options = JsonSerializerOptions()
    options.PropertyNameCaseInsensitive <- true
    
    // Act
    let equipment = JsonSerializer.Deserialize<CharacterEquipment>(sampleEquipmentJson, options)
    
    // Assert
    let slots = equipment.EquippedItems |> List.map (fun item -> item.Slot.Type)
    Assert.Contains("HEAD", slots)
    Assert.Contains("CHEST", slots)

[<Fact>]
let ``deserialize minimal equipment fixture`` () =
    let options = JsonSerializerOptions()
    options.PropertyNameCaseInsensitive <- true

    let equipment = JsonSerializer.Deserialize<CharacterEquipment>(minimalEquipmentJson, options)

    Assert.Single(equipment.EquippedItems) |> ignore
    Assert.Equal("Worn Shortsword", equipment.EquippedItems.Head.Name)
    Assert.True(equipment.EquippedItems.Head.Enchantments.IsNone)
