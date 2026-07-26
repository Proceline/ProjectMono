using System;
using System.Collections.Generic;

namespace MonopolyPrototype
{
    public enum FacilityInteractionType
    {
        None,
        StopAutoFeedback,
        StopConfirmFeedback,
        PassConfirmFeedback,
        PassAutoFeedback
    }

    public enum MoveEventTiming
    {
        Pass,
        Stop
    }

    public static class BoardMoveResolver
    {
        public readonly struct TileDefinition
        {
            public TileDefinition(string name, FacilityInteractionType interactionType, string feedbackLog, BuildingDefinition building = null)
            {
                Name = name ?? string.Empty;
                InteractionType = interactionType;
                FeedbackLog = feedbackLog ?? string.Empty;
                Building = building;
            }

            public string Name { get; }
            public FacilityInteractionType InteractionType { get; }
            public string FeedbackLog { get; }
            public BuildingDefinition Building { get; }
        }

        public readonly struct MoveEvent
        {
            public MoveEvent(
                int tileIndex,
                MoveEventTiming timing,
                FacilityInteractionType interactionType,
                string message,
                BuildingDefinition building = null,
                IReadOnlyList<BuildingEffectCommand> buildingCommands = null)
            {
                TileIndex = tileIndex;
                Timing = timing;
                InteractionType = interactionType;
                Message = message ?? string.Empty;
                Building = building;
                BuildingCommands = buildingCommands ?? new List<BuildingEffectCommand>();
            }

            public int TileIndex { get; }
            public MoveEventTiming Timing { get; }
            public FacilityInteractionType InteractionType { get; }
            public string Message { get; }
            public BuildingDefinition Building { get; }
            public IReadOnlyList<BuildingEffectCommand> BuildingCommands { get; }
            public bool RequiresConfirmation => InteractionType == FacilityInteractionType.PassConfirmFeedback
                || InteractionType == FacilityInteractionType.StopConfirmFeedback
                || HasConfirmingBuildingCommand(BuildingCommands);
        }

        public readonly struct MoveResult
        {
            public MoveResult(int endIndex, IReadOnlyList<MoveEvent> events)
            {
                EndIndex = endIndex;
                Events = events;
            }

            public int EndIndex { get; }
            public IReadOnlyList<MoveEvent> Events { get; }
        }

        public static MoveResult ResolveMove(IReadOnlyList<TileDefinition> tiles, int startIndex, int steps)
        {
            if (tiles == null)
            {
                throw new ArgumentNullException(nameof(tiles));
            }

            if (tiles.Count == 0)
            {
                throw new ArgumentException("Board must contain at least one tile.", nameof(tiles));
            }

            if (startIndex < 0 || startIndex >= tiles.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }

            if (steps < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(steps));
            }

            var events = new List<MoveEvent>();
            var currentIndex = startIndex;

            for (var step = 1; step <= steps; step++)
            {
                currentIndex = (currentIndex + 1) % tiles.Count;
                var isFinalStep = step == steps;
                var tile = tiles[currentIndex];

                if (!isFinalStep)
                {
                    AddMoveEventIfNeeded(events, currentIndex, MoveEventTiming.Pass, tile);
                }
                else
                {
                    AddMoveEventIfNeeded(events, currentIndex, MoveEventTiming.Stop, tile);
                }
            }

            return new MoveResult(currentIndex, events);
        }

        private static void AddMoveEventIfNeeded(List<MoveEvent> events, int tileIndex, MoveEventTiming timing, TileDefinition tile)
        {
            var hasFacilityEvent = HasFacilityEvent(timing, tile);
            var buildingCommands = BuildingRuleResolver.Resolve(tile.Building, timing);
            if (!hasFacilityEvent && buildingCommands.Count == 0)
            {
                return;
            }

            var message = hasFacilityEvent ? tile.FeedbackLog : string.Empty;
            events.Add(new MoveEvent(tileIndex, timing, tile.InteractionType, message, tile.Building, buildingCommands));
        }

        private static bool HasFacilityEvent(MoveEventTiming timing, TileDefinition tile)
        {
            if (string.IsNullOrWhiteSpace(tile.FeedbackLog))
            {
                return false;
            }

            return timing == MoveEventTiming.Pass
                ? IsPassInteraction(tile.InteractionType)
                : IsStopInteraction(tile.InteractionType);
        }

        private static bool IsPassInteraction(FacilityInteractionType interactionType)
        {
            return interactionType == FacilityInteractionType.PassConfirmFeedback
                || interactionType == FacilityInteractionType.PassAutoFeedback;
        }

        private static bool IsStopInteraction(FacilityInteractionType interactionType)
        {
            return interactionType == FacilityInteractionType.StopAutoFeedback
                || interactionType == FacilityInteractionType.StopConfirmFeedback
                || interactionType == FacilityInteractionType.PassConfirmFeedback
                || interactionType == FacilityInteractionType.PassAutoFeedback;
        }

        private static bool HasConfirmingBuildingCommand(IReadOnlyList<BuildingEffectCommand> commands)
        {
            if (commands == null)
            {
                return false;
            }

            for (var i = 0; i < commands.Count; i++)
            {
                if (commands[i].RequiresConfirmation)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
