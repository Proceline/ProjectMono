using MonopolyPrototype;
using NUnit.Framework;
using UnityEngine;

public class BoardTileBuildingTests
{
    [Test]
    public void ToDefinition_ConvertsBuildingConfigWhenPresent()
    {
        var adjustMoney = ScriptableObject.CreateInstance<AdjustMoneyEffectAsset>();
        adjustMoney.Configure(50);
        var config = ScriptableObject.CreateInstance<BuildingConfig>();
        config.Configure(
            "Clinic",
            BuildingTriggerMode.Pass,
            new BuildingEffectAsset[]
            {
                adjustMoney,
            });
        var gameObject = new GameObject("Tile");
        var tile = gameObject.AddComponent<BoardTile>();
        tile.Configure("Clinic", config);

        var definition = tile.ToDefinition();

        Assert.AreEqual("Clinic", definition.Building.Name);
        Assert.AreEqual(BuildingTriggerMode.Pass, definition.Building.TriggerMode);
        Assert.AreEqual(50, definition.Building.Effects[0].MoneyDelta);

        Object.DestroyImmediate(gameObject);
        Object.DestroyImmediate(config);
        Object.DestroyImmediate(adjustMoney);
    }
}
