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
            public TileDefinition(string name, FacilityInteractionType interactionType, string feedbackLog)
            {
                Name = name ?? string.Empty;
                InteractionType = interactionType;
                FeedbackLog = feedbackLog ?? string.Empty;
            }

            public string Name { get; }
            public FacilityInteractionType InteractionType { get; }
            public string FeedbackLog { get; }
        }

        public readonly struct MoveEvent
        {
            public MoveEvent(int tileIndex, MoveEventTiming timing, FacilityInteractionType interactionType, string message)
            {
                TileIndex = tileIndex;
                Timing = timing;
                InteractionType = interactionType;
                Message = message ?? string.Empty;
            }

            public int TileIndex { get; }
            public MoveEventTiming Timing { get; }
            public FacilityInteractionType InteractionType { get; }
            public string Message { get; }
            public bool RequiresConfirmation => InteractionType == FacilityInteractionType.PassConfirmFeedback
                || InteractionType == FacilityInteractionType.StopConfirmFeedback;
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

                if (!isFinalStep && IsPassInteraction(tile.InteractionType) && !string.IsNullOrWhiteSpace(tile.FeedbackLog))
                {
                    events.Add(new MoveEvent(currentIndex, MoveEventTiming.Pass, tile.InteractionType, tile.FeedbackLog));
                }
                else if (isFinalStep && IsStopInteraction(tile.InteractionType) && !string.IsNullOrWhiteSpace(tile.FeedbackLog))
                {
                    events.Add(new MoveEvent(currentIndex, MoveEventTiming.Stop, tile.InteractionType, tile.FeedbackLog));
                }
            }

            return new MoveResult(currentIndex, events);
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
    }
}
