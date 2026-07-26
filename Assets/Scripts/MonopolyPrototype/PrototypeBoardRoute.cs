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
                Vector2 position)
            {
                Name = name ?? string.Empty;
                Position = position;
            }

            public string Name { get; }
            public Vector2 Position { get; }

            public BoardMoveResolver.TileDefinition ToDefinition()
            {
                return new BoardMoveResolver.TileDefinition(Name);
            }
        }

        public static IReadOnlyList<TileSpec> Default { get; } = new[]
        {
            new TileSpec("Start", new Vector2(-4.5f, -2.5f)),
            new TileSpec("Bank", new Vector2(-2.7f, -2.5f)),
            new TileSpec("Blank", new Vector2(-0.9f, -2.5f)),
            new TileSpec("Gate", new Vector2(0.9f, -2.5f)),
            new TileSpec("Shop", new Vector2(2.7f, -2.5f)),
            new TileSpec("Station", new Vector2(4.5f, -2.5f)),
            new TileSpec("Park", new Vector2(4.5f, -0.8f)),
            new TileSpec("Library", new Vector2(4.5f, 0.9f)),
            new TileSpec("Museum", new Vector2(2.7f, 0.9f)),
            new TileSpec("Hotel", new Vector2(0.9f, 0.9f)),
            new TileSpec("Market", new Vector2(-0.9f, 0.9f)),
            new TileSpec("Clinic", new Vector2(-2.7f, 0.9f)),
            new TileSpec("Theater", new Vector2(-4.5f, 0.9f)),
            new TileSpec("Harbor", new Vector2(-4.5f, -0.8f)),
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
