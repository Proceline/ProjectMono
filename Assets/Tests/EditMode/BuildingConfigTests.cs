using MonopolyPrototype;
using NUnit.Framework;
using UnityEngine;

public class BuildingConfigTests
{
    [Test]
    public void ToDefinition_ConvertsScriptableObjectAuthoringDataToPureBuildingDefinition()
    {
        var config = ScriptableObject.CreateInstance<BuildingConfig>();
        config.Configure(
            "Clinic",
            BuildingTriggerMode.PassOrStop,
            new[]
            {
                BuildingEffectConfig.AddMoney(80),
                BuildingEffectConfig.RequestConfirmation("Confirm clinic visit."),
                BuildingEffectConfig.ShowFeedback("Clinic helped you recover."),
            });

        var definition = config.ToDefinition();

        Assert.AreEqual("Clinic", definition.Name);
        Assert.AreEqual(BuildingTriggerMode.PassOrStop, definition.TriggerMode);
        Assert.AreEqual(3, definition.Effects.Count);
        Assert.AreEqual(BuildingEffectType.AddMoney, definition.Effects[0].EffectType);
        Assert.AreEqual(80, definition.Effects[0].MoneyAmount);
        Assert.AreEqual(BuildingEffectType.RequestConfirmation, definition.Effects[1].EffectType);
        Assert.AreEqual("Confirm clinic visit.", definition.Effects[1].Message);
        Assert.AreEqual(BuildingEffectType.ShowFeedback, definition.Effects[2].EffectType);
        Assert.AreEqual("Clinic helped you recover.", definition.Effects[2].Message);
    }

    [Test]
    public void ToDefinition_ConvertsTeleportEffectTargetTile()
    {
        var config = ScriptableObject.CreateInstance<BuildingConfig>();
        config.Configure(
            "Harbor",
            BuildingTriggerMode.Stop,
            new[]
            {
                BuildingEffectConfig.TeleportTo(9),
            });

        var definition = config.ToDefinition();

        Assert.AreEqual(1, definition.Effects.Count);
        Assert.AreEqual(BuildingEffectType.Teleport, definition.Effects[0].EffectType);
        Assert.AreEqual(9, definition.Effects[0].TargetTileIndex);
    }
}
