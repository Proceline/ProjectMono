using System.Collections.Generic;
using UnityEngine;

namespace MonopolyPrototype
{
    public static class PrototypeBoardRoute
    {
        public readonly struct TileSpec
        {
            public TileSpec(
                string name,
                Vector2 position,
                FacilityInteractionType interactionType,
                string feedbackLog,
                BuildingDefinition building = null)
            {
                Name = name ?? string.Empty;
                Position = position;
                InteractionType = interactionType;
                FeedbackLog = feedbackLog ?? string.Empty;
                Building = building;
            }

            public string Name { get; }
            public Vector2 Position { get; }
            public FacilityInteractionType InteractionType { get; }
            public string FeedbackLog { get; }
            public BuildingDefinition Building { get; }

            public BoardMoveResolver.TileDefinition ToDefinition()
            {
                return new BoardMoveResolver.TileDefinition(Name, InteractionType, FeedbackLog, Building);
            }
        }

        public static IReadOnlyList<TileSpec> Default { get; } = new[]
        {
            new TileSpec("Start", new Vector2(-4.5f, -2.5f), FacilityInteractionType.StopAutoFeedback, "Stopped at Start."),
            new TileSpec("Bank", new Vector2(-2.7f, -2.5f), FacilityInteractionType.PassAutoFeedback, "Passed Bank: auto bonus feedback.", CreateBank()),
            new TileSpec("Blank", new Vector2(-0.9f, -2.5f), FacilityInteractionType.None, string.Empty),
            new TileSpec("Gate", new Vector2(0.9f, -2.5f), FacilityInteractionType.PassConfirmFeedback, "Gate checkpoint: confirm before moving on.", CreateGate()),
            new TileSpec("Shop", new Vector2(2.7f, -2.5f), FacilityInteractionType.StopConfirmFeedback, "Shop visit: confirm the stop action.", CreateShop()),
            new TileSpec("Station", new Vector2(4.5f, -2.5f), FacilityInteractionType.PassConfirmFeedback, "Station crossing: confirm the train signal."),
            new TileSpec("Park", new Vector2(4.5f, -0.8f), FacilityInteractionType.StopAutoFeedback, "Stopped at Park."),
            new TileSpec("Library", new Vector2(4.5f, 0.9f), FacilityInteractionType.PassAutoFeedback, "Passed Library: quiet auto feedback."),
            new TileSpec("Museum", new Vector2(2.7f, 0.9f), FacilityInteractionType.StopConfirmFeedback, "Museum visit: confirm the exhibit action."),
            new TileSpec("Hotel", new Vector2(0.9f, 0.9f), FacilityInteractionType.PassAutoFeedback, "Passed Hotel: lobby feedback."),
            new TileSpec("Market", new Vector2(-0.9f, 0.9f), FacilityInteractionType.StopAutoFeedback, "Stopped at Market."),
            new TileSpec("Clinic", new Vector2(-2.7f, 0.9f), FacilityInteractionType.PassAutoFeedback, "Passed Clinic: auto health feedback."),
            new TileSpec("Theater", new Vector2(-4.5f, 0.9f), FacilityInteractionType.StopConfirmFeedback, "Theater visit: confirm the show action."),
            new TileSpec("Harbor", new Vector2(-4.5f, -0.8f), FacilityInteractionType.PassConfirmFeedback, "Harbor crossing: confirm ship traffic.", CreateHarbor()),
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

        private static BuildingDefinition CreateBank()
        {
            return new BuildingDefinition(
                "Bank",
                BuildingTriggerMode.PassOrStop,
                new[]
                {
                    BuildingEffectDefinition.AddMoney(100),
                    BuildingEffectDefinition.ShowFeedback("Bank bonus: +100 money."),
                });
        }

        private static BuildingDefinition CreateGate()
        {
            return new BuildingDefinition(
                "Gate",
                BuildingTriggerMode.PassOrStop,
                new[]
                {
                    BuildingEffectDefinition.ShowFeedback("Gate checkpoint cleared."),
                });
        }

        private static BuildingDefinition CreateShop()
        {
            return new BuildingDefinition(
                "Shop",
                BuildingTriggerMode.Stop,
                new[]
                {
                    BuildingEffectDefinition.SubtractMoney(40),
                    BuildingEffectDefinition.ShowFeedback("Shop fee: -40 money."),
                });
        }

        private static BuildingDefinition CreateHarbor()
        {
            return new BuildingDefinition(
                "Harbor",
                BuildingTriggerMode.PassOrStop,
                new[]
                {
                    BuildingEffectDefinition.TeleportTo(0),
                });
        }
    }
}
