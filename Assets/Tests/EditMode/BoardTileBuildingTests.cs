using MonopolyPrototype;
using NUnit.Framework;
using UnityEngine;

public class BoardTileBuildingTests
{
    [Test]
    public void ToDefinition_ConvertsBuildingConfigWhenPresent()
    {
        var addMoney = ScriptableObject.CreateInstance<AddMoneyEffectAsset>();
        addMoney.Configure(50);
        var config = ScriptableObject.CreateInstance<BuildingConfig>();
        config.Configure(
            "Clinic",
            BuildingTriggerMode.Pass,
            new BuildingEffectAsset[]
            {
                addMoney,
            });
        var gameObject = new GameObject("Tile");
        var tile = gameObject.AddComponent<BoardTile>();
        tile.Configure("Clinic", config);

        var definition = tile.ToDefinition();

        Assert.AreEqual("Clinic", definition.Building.Name);
        Assert.AreEqual(BuildingTriggerMode.Pass, definition.Building.TriggerMode);
        Assert.AreEqual(50, definition.Building.Effects[0].MoneyAmount);

        Object.DestroyImmediate(gameObject);
        Object.DestroyImmediate(config);
        Object.DestroyImmediate(addMoney);
    }
}
