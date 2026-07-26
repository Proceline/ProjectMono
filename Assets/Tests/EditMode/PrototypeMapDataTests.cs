using MonopolyPrototype;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class PrototypeMapDataTests
{
    [Test]
    public void ValidateClosedLoop_AcceptsOrderedAdjacentPath()
    {
        var map = ScriptableObject.CreateInstance<PrototypeMapData>();
        map.Configure(3, new[]
        {
            new PrototypeMapTileData(new Vector2Int(0, 0), "Start"),
            new PrototypeMapTileData(new Vector2Int(1, 0), "Bank"),
            new PrototypeMapTileData(new Vector2Int(1, 1), "Gate"),
            new PrototypeMapTileData(new Vector2Int(0, 1), "Harbor"),
        });

        Assert.IsTrue(map.TryValidateClosedLoop(out var error), error);
        Assert.AreEqual(3, map.Width);
        Assert.AreEqual(4, map.Tiles.Count);
        Assert.AreEqual(new Vector2Int(0, 1), map.Tiles[3].GridPosition);
    }

    [Test]
    public void ValidateClosedLoop_RejectsNonAdjacentOrOpenPath()
    {
        var map = ScriptableObject.CreateInstance<PrototypeMapData>();
        map.Configure(4, new[]
        {
            new PrototypeMapTileData(new Vector2Int(0, 0), "Start"),
            new PrototypeMapTileData(new Vector2Int(2, 0), "Bank"),
            new PrototypeMapTileData(new Vector2Int(2, 1), "Gate"),
            new PrototypeMapTileData(new Vector2Int(0, 1), "Harbor"),
        });

        Assert.IsFalse(map.TryValidateClosedLoop(out var error));
        StringAssert.Contains("adjacent", error);
    }

    [Test]
    public void ValidateClosedLoop_RejectsDuplicateAndOutOfBoundsCells()
    {
        var map = ScriptableObject.CreateInstance<PrototypeMapData>();
        map.Configure(2, new[]
        {
            new PrototypeMapTileData(new Vector2Int(0, 0), "Start"),
            new PrototypeMapTileData(new Vector2Int(1, 0), "Bank"),
            new PrototypeMapTileData(new Vector2Int(1, 0), "Gate"),
            new PrototypeMapTileData(new Vector2Int(0, 2), "Harbor"),
        });

        Assert.IsFalse(map.TryValidateClosedLoop(out var error));
        StringAssert.Contains("duplicate", error);

        map.Configure(2, new[]
        {
            new PrototypeMapTileData(new Vector2Int(0, 0), "Start"),
            new PrototypeMapTileData(new Vector2Int(1, 0), "Bank"),
            new PrototypeMapTileData(new Vector2Int(1, 1), "Gate"),
            new PrototypeMapTileData(new Vector2Int(0, 2), "Harbor"),
        });

        Assert.IsFalse(map.TryValidateClosedLoop(out error));
        StringAssert.Contains("outside", error);
    }

    [Test]
    public void ToTileDefinitions_PreservesMapOrderAndBuildingData()
    {
        var config = ScriptableObject.CreateInstance<BuildingConfig>();
        config.Configure("Shop", BuildingTriggerMode.Stop, new[]
        {
            BuildingEffectConfig.SubtractMoney(40),
        });

        var map = ScriptableObject.CreateInstance<PrototypeMapData>();
        map.Configure(2, new[]
        {
            new PrototypeMapTileData(new Vector2Int(0, 0), "Start"),
            new PrototypeMapTileData(new Vector2Int(1, 0), "Shop", config),
            new PrototypeMapTileData(new Vector2Int(1, 1), "Park"),
            new PrototypeMapTileData(new Vector2Int(0, 1), "Harbor"),
        });

        var definitions = map.ToTileDefinitions();

        Assert.AreEqual(4, definitions.Count);
        Assert.AreEqual("Start", definitions[0].Name);
        Assert.AreEqual("Shop", definitions[1].Name);
        Assert.IsNotNull(definitions[1].Building);
        Assert.AreEqual(BuildingEffectType.SubtractMoney, definitions[1].Building.Effects[0].EffectType);
        Assert.IsNull(definitions[0].Building);
    }

    [Test]
    public void DefaultMapDataAsset_IsClosedLoopAndMatchesPrototypeMap()
    {
        var map = AssetDatabase.LoadAssetAtPath<PrototypeMapData>(
            "Assets/Data/Maps/PrototypeMapData.asset");

        Assert.IsNotNull(map);
        Assert.AreEqual(6, map.Width);
        Assert.AreEqual(6, map.Height);
        Assert.AreEqual(14, map.Tiles.Count);
        Assert.IsTrue(map.TryValidateClosedLoop(out var error), error);
        Assert.AreEqual("Start", map.Tiles[0].TileName);
        Assert.AreEqual("Harbor", map.Tiles[13].TileName);
    }
}
