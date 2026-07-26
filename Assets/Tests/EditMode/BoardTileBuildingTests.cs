using MonopolyPrototype;
using NUnit.Framework;
using UnityEngine;

public class BoardTileBuildingTests
{
    [Test]
    public void ToDefinition_ConvertsBuildingConfigWhenPresent()
    {
        var config = ScriptableObject.CreateInstance<BuildingConfig>();
        config.Configure(
            "Clinic",
            BuildingTriggerMode.Pass,
            new[]
            {
                BuildingEffectConfig.AddMoney(50),
            });
        var gameObject = new GameObject("Tile");
        var tile = gameObject.AddComponent<BoardTile>();
        tile.Configure("Clinic", FacilityInteractionType.None, string.Empty, config);

        var definition = tile.ToDefinition();

        Assert.AreEqual("Clinic", definition.Building.Name);
        Assert.AreEqual(BuildingTriggerMode.Pass, definition.Building.TriggerMode);
        Assert.AreEqual(50, definition.Building.Effects[0].MoneyAmount);
    }
}
