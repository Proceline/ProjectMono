using System;
using System.Collections.Generic;

namespace MonopolyPrototype
{
    public enum MoveEventTiming
    {
        Pass,
        Stop
    }

    public static class BoardMoveResolver
    {
        public readonly struct TileDefinition
        {
            public TileDefinition(string name, BuildingDefinition building = null)
            {
                Name = name ?? string.Empty;
                Building = building;
            }

            public string Name { get; }
            public BuildingDefinition Building { get; }
        }

        public readonly struct MoveEvent
        {
            public MoveEvent(
                int tileIndex,
                MoveEventTiming timing,
                BuildingDefinition building = null,
                IReadOnlyList<BuildingEffectCommand> buildingCommands = null)
            {
                TileIndex = tileIndex;
                Timing = timing;
                Building = building;
                BuildingCommands = buildingCommands ?? new List<BuildingEffectCommand>();
            }

            public int TileIndex { get; }
            public MoveEventTiming Timing { get; }
            public BuildingDefinition Building { get; }
            public IReadOnlyList<BuildingEffectCommand> BuildingCommands { get; }
            public bool RequiresConfirmation => HasConfirmingBuildingCommand(BuildingCommands);
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
                var timing = step == steps ? MoveEventTiming.Stop : MoveEventTiming.Pass;
                AddMoveEventIfNeeded(events, currentIndex, timing, tiles[currentIndex]);
            }

            return new MoveResult(currentIndex, events);
        }

        private static void AddMoveEventIfNeeded(
            List<MoveEvent> events,
            int tileIndex,
            MoveEventTiming timing,
            TileDefinition tile)
        {
            var buildingCommands = BuildingRuleResolver.Resolve(tile.Building, timing);
            if (buildingCommands.Count == 0)
            {
                return;
            }

            events.Add(new MoveEvent(tileIndex, timing, tile.Building, buildingCommands));
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
