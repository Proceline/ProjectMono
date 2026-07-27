using System.Collections.Generic;
using MonopolyPrototype;
using NUnit.Framework;

public class BuildingRuleResolverTests
{
    [Test]
    public void Resolve_IgnoresPassOnlyBuildingWhenTokenStops()
    {
        var building = new BuildingDefinition(
            "Bank",
            BuildingTriggerMode.Pass,
            new[]
            {
                BuildingEffectDefinition.AdjustMoney(100),
            });

        var commands = BuildingRuleResolver.Resolve(building, MoveEventTiming.Stop);

        CollectionAssert.IsEmpty(commands);
    }

    [Test]
    public void Resolve_RunsStopOnlyBuildingWhenTokenStops()
    {
        var building = new BuildingDefinition(
            "Shop",
            BuildingTriggerMode.Stop,
            new[]
            {
                BuildingEffectDefinition.AdjustMoney(-40),
                BuildingEffectDefinition.ShowFeedback("Paid shop fee."),
            });

        var commands = BuildingRuleResolver.Resolve(building, MoveEventTiming.Stop);

        Assert.AreEqual(2, commands.Count);
        Assert.AreEqual(BuildingEffectType.AdjustMoney, commands[0].EffectType);
        Assert.AreEqual(-40, commands[0].MoneyDelta);
        Assert.AreEqual(-1, commands[0].EffectIndex);
        Assert.AreEqual(BuildingEffectType.ShowFeedback, commands[1].EffectType);
        Assert.AreEqual("Paid shop fee.", commands[1].Message);
    }

    [Test]
    public void Resolve_RunsPassOrStopBuildingForPassAndKeepsEffectOrder()
    {
        var building = new BuildingDefinition(
            "Gate",
            BuildingTriggerMode.PassOrStop,
            new[]
            {
                BuildingEffectDefinition.ShowFeedback("Gate checkpoint."),
                BuildingEffectDefinition.RequestConfirmation("Confirm gate."),
                BuildingEffectDefinition.TeleportTo(5),
            });

        var commands = BuildingRuleResolver.Resolve(building, MoveEventTiming.Pass);

        Assert.AreEqual(3, commands.Count);
        Assert.AreEqual(BuildingEffectType.ShowFeedback, commands[0].EffectType);
        Assert.AreEqual("Gate checkpoint.", commands[0].Message);
        Assert.AreEqual(BuildingEffectType.RequestConfirmation, commands[1].EffectType);
        Assert.IsTrue(commands[1].RequiresConfirmation);
        Assert.AreEqual("Confirm gate.", commands[1].Message);
        Assert.AreEqual(BuildingEffectType.Teleport, commands[2].EffectType);
        Assert.AreEqual(5, commands[2].TargetTileIndex);
    }

    [Test]
    public void Resolve_AllowsNullBuildingAsNoEffectTile()
    {
        var commands = BuildingRuleResolver.Resolve(null, MoveEventTiming.Stop);

        CollectionAssert.IsEmpty(commands);
    }
}
