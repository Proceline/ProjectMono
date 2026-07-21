using System.Collections.Generic;
using MonopolyPrototype;
using NUnit.Framework;

public class BoardMoveResolverTests
{
    [Test]
    public void ResolveMove_CreatesPassEventsForIntermediatePassFacilities()
    {
        var tiles = new List<BoardMoveResolver.TileDefinition>
        {
            new BoardMoveResolver.TileDefinition("Start", FacilityInteractionType.StopAutoFeedback, "Stopped at start"),
            new BoardMoveResolver.TileDefinition("Bank", FacilityInteractionType.PassAutoFeedback, "Passed the bank"),
            new BoardMoveResolver.TileDefinition("Gate", FacilityInteractionType.PassConfirmFeedback, "Confirm the gate"),
            new BoardMoveResolver.TileDefinition("Park", FacilityInteractionType.None, ""),
        };

        var result = BoardMoveResolver.ResolveMove(tiles, startIndex: 0, steps: 3);

        Assert.AreEqual(2, result.Events.Count);
        Assert.AreEqual(MoveEventTiming.Pass, result.Events[0].Timing);
        Assert.AreEqual(FacilityInteractionType.PassAutoFeedback, result.Events[0].InteractionType);
        Assert.AreEqual("Passed the bank", result.Events[0].Message);
        Assert.IsFalse(result.Events[0].RequiresConfirmation);
        Assert.AreEqual(MoveEventTiming.Pass, result.Events[1].Timing);
        Assert.AreEqual(FacilityInteractionType.PassConfirmFeedback, result.Events[1].InteractionType);
        Assert.AreEqual("Confirm the gate", result.Events[1].Message);
        Assert.IsTrue(result.Events[1].RequiresConfirmation);
        Assert.AreEqual(3, result.EndIndex);
    }

    [Test]
    public void ResolveMove_CreatesStopEventForFinalPassConfirmFacility()
    {
        var tiles = new List<BoardMoveResolver.TileDefinition>
        {
            new BoardMoveResolver.TileDefinition("Start", FacilityInteractionType.None, ""),
            new BoardMoveResolver.TileDefinition("Station", FacilityInteractionType.PassConfirmFeedback, "Confirm station"),
        };

        var result = BoardMoveResolver.ResolveMove(tiles, startIndex: 0, steps: 1);

        Assert.AreEqual(1, result.Events.Count);
        Assert.AreEqual(MoveEventTiming.Stop, result.Events[0].Timing);
        Assert.AreEqual(FacilityInteractionType.PassConfirmFeedback, result.Events[0].InteractionType);
        Assert.AreEqual("Confirm station", result.Events[0].Message);
        Assert.IsTrue(result.Events[0].RequiresConfirmation);
        Assert.AreEqual(1, result.EndIndex);
    }

    [Test]
    public void ResolveMove_CreatesStopEventForStopAutoFacility()
    {
        var tiles = new List<BoardMoveResolver.TileDefinition>
        {
            new BoardMoveResolver.TileDefinition("Start", FacilityInteractionType.PassAutoFeedback, "Passed start"),
            new BoardMoveResolver.TileDefinition("Bank", FacilityInteractionType.StopAutoFeedback, "Stopped at bank"),
            new BoardMoveResolver.TileDefinition("Shop", FacilityInteractionType.PassAutoFeedback, "Passed shop"),
        };

        var result = BoardMoveResolver.ResolveMove(tiles, startIndex: 2, steps: 2);

        Assert.AreEqual(2, result.Events.Count);
        Assert.AreEqual(MoveEventTiming.Pass, result.Events[0].Timing);
        Assert.AreEqual("Passed start", result.Events[0].Message);
        Assert.AreEqual(MoveEventTiming.Stop, result.Events[1].Timing);
        Assert.AreEqual(FacilityInteractionType.StopAutoFeedback, result.Events[1].InteractionType);
        Assert.AreEqual("Stopped at bank", result.Events[1].Message);
        Assert.IsFalse(result.Events[1].RequiresConfirmation);
        Assert.AreEqual(1, result.EndIndex);
    }

    [Test]
    public void ResolveMove_CreatesConfirmingStopEventForStopConfirmFacility()
    {
        var tiles = new List<BoardMoveResolver.TileDefinition>
        {
            new BoardMoveResolver.TileDefinition("Start", FacilityInteractionType.None, ""),
            new BoardMoveResolver.TileDefinition("Office", FacilityInteractionType.StopConfirmFeedback, "Confirm office"),
        };

        var result = BoardMoveResolver.ResolveMove(tiles, startIndex: 0, steps: 1);

        Assert.AreEqual(1, result.Events.Count);
        Assert.AreEqual(MoveEventTiming.Stop, result.Events[0].Timing);
        Assert.AreEqual(FacilityInteractionType.StopConfirmFeedback, result.Events[0].InteractionType);
        Assert.IsTrue(result.Events[0].RequiresConfirmation);
    }

    [Test]
    public void ResolveMove_CreatesNoEventsForBlankTiles()
    {
        var tiles = new List<BoardMoveResolver.TileDefinition>
        {
            new BoardMoveResolver.TileDefinition("Start", FacilityInteractionType.None, ""),
            new BoardMoveResolver.TileDefinition("Blank 1", FacilityInteractionType.None, ""),
            new BoardMoveResolver.TileDefinition("Blank 2", FacilityInteractionType.None, ""),
        };

        var result = BoardMoveResolver.ResolveMove(tiles, startIndex: 0, steps: 2);

        CollectionAssert.IsEmpty(result.Events);
        Assert.AreEqual(2, result.EndIndex);
    }
}
