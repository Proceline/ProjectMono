using MonopolyPrototype;
using NUnit.Framework;
using UnityEngine;

public class PrototypeBuildingCatalogTests
{
    [Test]
    public void Find_ReturnsRegisteredBuildingConfigByName()
    {
        var bank = ScriptableObject.CreateInstance<BuildingConfig>();
        bank.Configure("Bank", BuildingTriggerMode.PassOrStop, null);
        var shop = ScriptableObject.CreateInstance<BuildingConfig>();
        shop.Configure("Shop", BuildingTriggerMode.Stop, null);
        var catalog = ScriptableObject.CreateInstance<PrototypeBuildingCatalog>();
        catalog.Configure(new[] { bank, shop });

        Assert.AreSame(bank, catalog.Find("Bank"));
        Assert.AreSame(shop, catalog.Find("Shop"));
        Assert.IsNull(catalog.Find("Unknown"));

        Object.DestroyImmediate(bank);
        Object.DestroyImmediate(shop);
        Object.DestroyImmediate(catalog);
    }

    [Test]
    public void BoardTile_UsesCatalogConfigAsPureBuildingDefinition()
    {
        var bank = ScriptableObject.CreateInstance<BuildingConfig>();
        bank.Configure(
            "Bank",
            BuildingTriggerMode.PassOrStop,
            new[] { BuildingEffectConfig.AddMoney(100) });
        var catalog = ScriptableObject.CreateInstance<PrototypeBuildingCatalog>();
        catalog.Configure(new[] { bank });
        var gameObject = new GameObject("Tile");
        var tile = gameObject.AddComponent<BoardTile>();
        tile.Configure("Bank", FacilityInteractionType.PassAutoFeedback, string.Empty, catalog.Find("Bank"));

        var definition = tile.ToDefinition();

        Assert.AreEqual("Bank", definition.Building.Name);
        Assert.AreEqual(100, definition.Building.Effects[0].MoneyAmount);

        Object.DestroyImmediate(gameObject);
        Object.DestroyImmediate(bank);
        Object.DestroyImmediate(catalog);
    }
}
