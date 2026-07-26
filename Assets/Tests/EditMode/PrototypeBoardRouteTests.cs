using System.Linq;
using MonopolyPrototype;
using NUnit.Framework;
using UnityEngine;

public class PrototypeBoardRouteTests
{
    [Test]
    public void DefaultRoute_ContainsTileSpecsForEveryPrototypeTile()
    {
        var route = PrototypeBoardRoute.Default;

        Assert.AreEqual(14, route.Count);
        Assert.AreEqual("Start", route[0].Name);
        Assert.AreEqual(FacilityInteractionType.StopAutoFeedback, route[0].InteractionType);
        Assert.AreEqual(new Vector2(-4.5f, -2.5f), route[0].Position);
        Assert.AreEqual("Harbor", route[13].Name);
        Assert.AreEqual(FacilityInteractionType.PassConfirmFeedback, route[13].InteractionType);
    }

    [Test]
    public void ToTileDefinitions_PreservesRouteOrderAndFacilityBehavior()
    {
        var definitions = PrototypeBoardRoute.ToTileDefinitions(PrototypeBoardRoute.Default).ToList();

        Assert.AreEqual(14, definitions.Count);
        Assert.AreEqual("Bank", definitions[1].Name);
        Assert.AreEqual(FacilityInteractionType.PassAutoFeedback, definitions[1].InteractionType);
        Assert.AreEqual("Passed Bank: auto bonus feedback.", definitions[1].FeedbackLog);
        Assert.AreEqual("Gate", definitions[3].Name);
        Assert.AreEqual(FacilityInteractionType.PassConfirmFeedback, definitions[3].InteractionType);
    }

    [Test]
    public void ToTileDefinitions_PreservesPrototypeBuildingDefinitions()
    {
        var definitions = PrototypeBoardRoute.ToTileDefinitions(PrototypeBoardRoute.Default).ToList();

        Assert.IsNotNull(definitions[1].Building);
        Assert.AreEqual("Bank", definitions[1].Building.Name);
        Assert.AreEqual(BuildingTriggerMode.PassOrStop, definitions[1].Building.TriggerMode);
        Assert.AreEqual(BuildingEffectType.AddMoney, definitions[1].Building.Effects[0].EffectType);
        Assert.AreEqual(100, definitions[1].Building.Effects[0].MoneyAmount);
        Assert.IsNotNull(definitions[3].Building);
        Assert.AreEqual(BuildingEffectType.ShowFeedback, definitions[3].Building.Effects[0].EffectType);
    }
}
