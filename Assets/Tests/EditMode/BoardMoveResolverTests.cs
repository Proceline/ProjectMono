using System.Collections.Generic;
using MonopolyPrototype;
using NUnit.Framework;

public class BoardMoveResolverTests
{
    [Test]
    public void ResolveMove_CreatesPassEventsForIntermediateBuildings()
    {
        var bank = new BuildingDefinition(
            "Bank",
            BuildingTriggerMode.PassOrStop,
            new[] { BuildingEffectDefinition.ShowFeedback("Passed the bank") });
        var gate = new BuildingDefinition(
            "Gate",
            BuildingTriggerMode.PassOrStop,
            new[] { BuildingEffectDefinition.RequestConfirmation("Confirm the gate") });
        var tiles = new List<BoardMoveResolver.TileDefinition>
        {
            new BoardMoveResolver.TileDefinition("Start"),
            new BoardMoveResolver.TileDefinition("Bank", bank),
            new BoardMoveResolver.TileDefinition("Gate", gate),
            new BoardMoveResolver.TileDefinition("Park"),
        };

        var result = BoardMoveResolver.ResolveMove(tiles, startIndex: 0, steps: 3);

        Assert.AreEqual(2, result.Events.Count);
        Assert.AreEqual(MoveEventTiming.Pass, result.Events[0].Timing);
        Assert.AreSame(bank, result.Events[0].Building);
        Assert.AreEqual(BuildingEffectType.ShowFeedback, result.Events[0].BuildingCommands[0].EffectType);
        Assert.AreEqual("Passed the bank", result.Events[0].BuildingCommands[0].Message);
        Assert.IsFalse(result.Events[0].RequiresConfirmation);
        Assert.AreEqual(MoveEventTiming.Pass, result.Events[1].Timing);
        Assert.AreSame(gate, result.Events[1].Building);
        Assert.AreEqual(BuildingEffectType.RequestConfirmation, result.Events[1].BuildingCommands[0].EffectType);
        Assert.AreEqual("Confirm the gate", result.Events[1].BuildingCommands[0].Message);
        Assert.IsTrue(result.Events[1].RequiresConfirmation);
        Assert.AreEqual(3, result.EndIndex);
    }

    [Test]
    public void ResolveMove_CreatesStopEventForFinalPassOrStopConfirmationBuilding()
    {
        var station = new BuildingDefinition(
            "Station",
            BuildingTriggerMode.PassOrStop,
            new[] { BuildingEffectDefinition.RequestConfirmation("Confirm station") });
        var tiles = new List<BoardMoveResolver.TileDefinition>
        {
            new BoardMoveResolver.TileDefinition("Start"),
            new BoardMoveResolver.TileDefinition("Station", station),
        };

        var result = BoardMoveResolver.ResolveMove(tiles, startIndex: 0, steps: 1);

        Assert.AreEqual(1, result.Events.Count);
        Assert.AreEqual(MoveEventTiming.Stop, result.Events[0].Timing);
        Assert.AreSame(station, result.Events[0].Building);
        Assert.IsTrue(result.Events[0].RequiresConfirmation);
        Assert.AreEqual("Confirm station", result.Events[0].BuildingCommands[0].Message);
        Assert.AreEqual(1, result.EndIndex);
    }

    [Test]
    public void ResolveMove_CreatesStopEventForStopBuilding()
    {
        var start = new BuildingDefinition(
            "Start",
            BuildingTriggerMode.PassOrStop,
            new[] { BuildingEffectDefinition.ShowFeedback("Passed start") });
        var bank = new BuildingDefinition(
            "Bank",
            BuildingTriggerMode.Stop,
            new[] { BuildingEffectDefinition.ShowFeedback("Stopped at bank") });
        var shop = new BuildingDefinition(
            "Shop",
            BuildingTriggerMode.PassOrStop,
            new[] { BuildingEffectDefinition.ShowFeedback("Passed shop") });
        var tiles = new List<BoardMoveResolver.TileDefinition>
        {
            new BoardMoveResolver.TileDefinition("Start", start),
            new BoardMoveResolver.TileDefinition("Bank", bank),
            new BoardMoveResolver.TileDefinition("Shop", shop),
        };

        var result = BoardMoveResolver.ResolveMove(tiles, startIndex: 2, steps: 2);

        Assert.AreEqual(2, result.Events.Count);
        Assert.AreEqual(MoveEventTiming.Pass, result.Events[0].Timing);
        Assert.AreEqual("Passed start", result.Events[0].BuildingCommands[0].Message);
        Assert.AreEqual(MoveEventTiming.Stop, result.Events[1].Timing);
        Assert.AreSame(bank, result.Events[1].Building);
        Assert.AreEqual("Stopped at bank", result.Events[1].BuildingCommands[0].Message);
        Assert.IsFalse(result.Events[1].RequiresConfirmation);
        Assert.AreEqual(1, result.EndIndex);
    }

    [Test]
    public void ResolveMove_CreatesConfirmingStopEventForStopBuilding()
    {
        var office = new BuildingDefinition(
            "Office",
            BuildingTriggerMode.Stop,
            new[] { BuildingEffectDefinition.RequestConfirmation("Confirm office") });
        var tiles = new List<BoardMoveResolver.TileDefinition>
        {
            new BoardMoveResolver.TileDefinition("Start"),
            new BoardMoveResolver.TileDefinition("Office", office),
        };

        var result = BoardMoveResolver.ResolveMove(tiles, startIndex: 0, steps: 1);

        Assert.AreEqual(1, result.Events.Count);
        Assert.AreEqual(MoveEventTiming.Stop, result.Events[0].Timing);
        Assert.AreSame(office, result.Events[0].Building);
        Assert.IsTrue(result.Events[0].RequiresConfirmation);
    }

    [Test]
    public void ResolveMove_CreatesNoEventsForBlankTiles()
    {
        var tiles = new List<BoardMoveResolver.TileDefinition>
        {
            new BoardMoveResolver.TileDefinition("Start"),
            new BoardMoveResolver.TileDefinition("Blank 1"),
            new BoardMoveResolver.TileDefinition("Blank 2"),
        };

        var result = BoardMoveResolver.ResolveMove(tiles, startIndex: 0, steps: 2);

        CollectionAssert.IsEmpty(result.Events);
        Assert.AreEqual(2, result.EndIndex);
    }

    [Test]
    public void ResolveMove_PreservesOrderedBuildingCommands()
    {
        var bank = new BuildingDefinition(
            "Bank",
            BuildingTriggerMode.Pass,
            new[]
            {
                BuildingEffectDefinition.AddMoney(100),
                BuildingEffectDefinition.ShowFeedback("Passed bank."),
            });
        var tiles = new List<BoardMoveResolver.TileDefinition>
        {
            new BoardMoveResolver.TileDefinition("Start"),
            new BoardMoveResolver.TileDefinition("Bank", bank),
            new BoardMoveResolver.TileDefinition("Blank"),
        };

        var result = BoardMoveResolver.ResolveMove(tiles, startIndex: 0, steps: 2);

        Assert.AreEqual(1, result.Events.Count);
        Assert.AreEqual(MoveEventTiming.Pass, result.Events[0].Timing);
        Assert.AreSame(bank, result.Events[0].Building);
        Assert.AreEqual(2, result.Events[0].BuildingCommands.Count);
        Assert.AreEqual(BuildingEffectType.AddMoney, result.Events[0].BuildingCommands[0].EffectType);
        Assert.AreEqual(100, result.Events[0].BuildingCommands[0].MoneyDelta);
        Assert.AreEqual(BuildingEffectType.ShowFeedback, result.Events[0].BuildingCommands[1].EffectType);
        Assert.AreEqual("Passed bank.", result.Events[0].BuildingCommands[1].Message);
    }
}
