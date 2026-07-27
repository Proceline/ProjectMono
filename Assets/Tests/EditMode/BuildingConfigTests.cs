using MonopolyPrototype;
using NUnit.Framework;
using UnityEngine;

public class BuildingConfigTests
{
    [Test]
    public void ToDefinition_ConvertsScriptableObjectAuthoringDataToPureBuildingDefinition()
    {
        var adjustMoney = ScriptableObject.CreateInstance<AdjustMoneyEffectAsset>();
        adjustMoney.Configure(80);
        var confirmation = ScriptableObject.CreateInstance<RequestConfirmationEffectAsset>();
        confirmation.Configure("Confirm clinic visit.");
        var feedback = ScriptableObject.CreateInstance<ShowFeedbackEffectAsset>();
        feedback.Configure("Clinic helped you recover.");
        var config = ScriptableObject.CreateInstance<BuildingConfig>();
        config.Configure(
            "Clinic",
            BuildingTriggerMode.PassOrStop,
            new BuildingEffectAsset[]
            {
                adjustMoney,
                confirmation,
                feedback,
            });

        var definition = config.ToDefinition();

        Assert.AreEqual("Clinic", definition.Name);
        Assert.AreEqual(BuildingTriggerMode.PassOrStop, definition.TriggerMode);
        Assert.AreEqual(3, definition.Effects.Count);
        Assert.AreEqual(0, definition.Effects[0].EffectIndex);
        Assert.AreEqual(BuildingEffectType.AdjustMoney, definition.Effects[0].EffectType);
        Assert.AreEqual(80, definition.Effects[0].MoneyDelta);
        Assert.AreEqual(1, definition.Effects[1].EffectIndex);
        Assert.AreEqual(BuildingEffectType.RequestConfirmation, definition.Effects[1].EffectType);
        Assert.AreEqual("Confirm clinic visit.", definition.Effects[1].Message);
        Assert.AreEqual(2, definition.Effects[2].EffectIndex);
        Assert.AreEqual(BuildingEffectType.ShowFeedback, definition.Effects[2].EffectType);
        Assert.AreEqual("Clinic helped you recover.", definition.Effects[2].Message);

        Object.DestroyImmediate(config);
        Object.DestroyImmediate(adjustMoney);
        Object.DestroyImmediate(confirmation);
        Object.DestroyImmediate(feedback);
    }

    [Test]
    public void ToDefinition_ConvertsTeleportEffectTargetTile()
    {
        var teleport = ScriptableObject.CreateInstance<TeleportEffectAsset>();
        teleport.Configure(9);
        var config = ScriptableObject.CreateInstance<BuildingConfig>();
        config.Configure(
            "Harbor",
            BuildingTriggerMode.Stop,
            new BuildingEffectAsset[]
            {
                teleport,
            });

        var definition = config.ToDefinition();

        Assert.AreEqual(1, definition.Effects.Count);
        Assert.AreEqual(BuildingEffectType.Teleport, definition.Effects[0].EffectType);
        Assert.AreEqual(9, definition.Effects[0].TargetTileIndex);

        Object.DestroyImmediate(config);
        Object.DestroyImmediate(teleport);
    }

    [Test]
    public void TryValidate_RejectsMultipleConfirmationEffects()
    {
        var firstConfirmation = ScriptableObject.CreateInstance<RequestConfirmationEffectAsset>();
        var secondConfirmation = ScriptableObject.CreateInstance<RequestConfirmationEffectAsset>();
        var config = ScriptableObject.CreateInstance<BuildingConfig>();
        config.Configure(
            "Gate",
            BuildingTriggerMode.PassOrStop,
            new BuildingEffectAsset[]
            {
                firstConfirmation,
                secondConfirmation,
            });

        Assert.IsFalse(config.TryValidate(out var error));
        StringAssert.Contains("at most one RequestConfirmation", error);

        Object.DestroyImmediate(config);
        Object.DestroyImmediate(firstConfirmation);
        Object.DestroyImmediate(secondConfirmation);
    }
}
