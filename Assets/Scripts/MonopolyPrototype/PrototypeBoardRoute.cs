using System.Collections.Generic;
using UnityEngine;

namespace MonopolyPrototype
{
    public static class PrototypeBoardRoute
    {
        public readonly struct TileSpec
        {
            public TileSpec(string name, Vector2 position, FacilityInteractionType interactionType, string feedbackLog)
            {
                Name = name ?? string.Empty;
                Position = position;
                InteractionType = interactionType;
                FeedbackLog = feedbackLog ?? string.Empty;
            }

            public string Name { get; }
            public Vector2 Position { get; }
            public FacilityInteractionType InteractionType { get; }
            public string FeedbackLog { get; }

            public BoardMoveResolver.TileDefinition ToDefinition()
            {
                return new BoardMoveResolver.TileDefinition(Name, InteractionType, FeedbackLog);
            }
        }

        public static IReadOnlyList<TileSpec> Default { get; } = new[]
        {
            new TileSpec("Start", new Vector2(-4.5f, -2.5f), FacilityInteractionType.StopAutoFeedback, "Stopped at Start."),
            new TileSpec("Bank", new Vector2(-2.7f, -2.5f), FacilityInteractionType.PassAutoFeedback, "Passed Bank: auto bonus feedback."),
            new TileSpec("Blank", new Vector2(-0.9f, -2.5f), FacilityInteractionType.None, string.Empty),
            new TileSpec("Gate", new Vector2(0.9f, -2.5f), FacilityInteractionType.PassConfirmFeedback, "Gate checkpoint: confirm before moving on."),
            new TileSpec("Shop", new Vector2(2.7f, -2.5f), FacilityInteractionType.StopConfirmFeedback, "Shop visit: confirm the stop action."),
            new TileSpec("Station", new Vector2(4.5f, -2.5f), FacilityInteractionType.PassConfirmFeedback, "Station crossing: confirm the train signal."),
            new TileSpec("Park", new Vector2(4.5f, -0.8f), FacilityInteractionType.StopAutoFeedback, "Stopped at Park."),
            new TileSpec("Library", new Vector2(4.5f, 0.9f), FacilityInteractionType.PassAutoFeedback, "Passed Library: quiet auto feedback."),
            new TileSpec("Museum", new Vector2(2.7f, 0.9f), FacilityInteractionType.StopConfirmFeedback, "Museum visit: confirm the exhibit action."),
            new TileSpec("Hotel", new Vector2(0.9f, 0.9f), FacilityInteractionType.PassAutoFeedback, "Passed Hotel: lobby feedback."),
            new TileSpec("Market", new Vector2(-0.9f, 0.9f), FacilityInteractionType.StopAutoFeedback, "Stopped at Market."),
            new TileSpec("Clinic", new Vector2(-2.7f, 0.9f), FacilityInteractionType.PassAutoFeedback, "Passed Clinic: auto health feedback."),
            new TileSpec("Theater", new Vector2(-4.5f, 0.9f), FacilityInteractionType.StopConfirmFeedback, "Theater visit: confirm the show action."),
            new TileSpec("Harbor", new Vector2(-4.5f, -0.8f), FacilityInteractionType.PassConfirmFeedback, "Harbor crossing: confirm ship traffic."),
        };

        public static IReadOnlyList<BoardMoveResolver.TileDefinition> ToTileDefinitions(IReadOnlyList<TileSpec> route)
        {
            var definitions = new List<BoardMoveResolver.TileDefinition>();
            if (route == null)
            {
                return definitions;
            }

            for (var i = 0; i < route.Count; i++)
            {
                definitions.Add(route[i].ToDefinition());
            }

            return definitions;
        }
    }
}
