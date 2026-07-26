using System;
using System.Collections.Generic;
using UnityEngine;

namespace MonopolyPrototype
{
    [CreateAssetMenu(menuName = "Monopoly Prototype/Map Data")]
    public sealed class PrototypeMapData : ScriptableObject
    {
        [SerializeField, Min(2)] private int width = 6;
        [SerializeField, Min(2)] private int height = 6;
        [SerializeField] private List<PrototypeMapTileData> tiles = new List<PrototypeMapTileData>();

        public int Width => width;
        public int Height => height;
        public IReadOnlyList<PrototypeMapTileData> Tiles => tiles;

        public void Configure(int size, IReadOnlyList<PrototypeMapTileData> tileData)
        {
            width = Mathf.Max(2, size);
            height = width;
            tiles = tileData == null
                ? new List<PrototypeMapTileData>()
                : new List<PrototypeMapTileData>(tileData);
        }

        public IReadOnlyList<BoardMoveResolver.TileDefinition> ToTileDefinitions()
        {
            var definitions = new List<BoardMoveResolver.TileDefinition>();
            for (var i = 0; i < tiles.Count; i++)
            {
                definitions.Add(tiles[i].ToDefinition());
            }

            return definitions;
        }

        public bool TryValidateClosedLoop(out string error)
        {
            if (width != height)
            {
                error = "Map width and height must be equal for an N x N map.";
                return false;
            }

            if (width < 2)
            {
                error = "Map size must be at least 2 x 2.";
                return false;
            }

            if (tiles == null || tiles.Count < 4)
            {
                error = "A closed map path needs at least four tiles.";
                return false;
            }

            var occupied = new HashSet<Vector2Int>();
            for (var i = 0; i < tiles.Count; i++)
            {
                var tile = tiles[i];
                if (tile == null)
                {
                    error = $"Map tile {i + 1} is missing.";
                    return false;
                }

                var position = tile.GridPosition;
                if (position.x < 0 || position.x >= width || position.y < 0 || position.y >= height)
                {
                    error = $"Map tile {i + 1} is outside the map bounds.";
                    return false;
                }

                if (!occupied.Add(position))
                {
                    error = $"Map tile {i + 1} duplicates an occupied cell.";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(tile.TileName))
                {
                    error = $"Map tile {i + 1} needs a name.";
                    return false;
                }

                if (i > 0 && !AreAdjacent(tiles[i - 1].GridPosition, position))
                {
                    error = $"Map tiles {i} and {i + 1} must be adjacent.";
                    return false;
                }
            }

            if (!AreAdjacent(tiles[tiles.Count - 1].GridPosition, tiles[0].GridPosition))
            {
                error = "The final map tile must be adjacent to the first tile to close the loop.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private static bool AreAdjacent(Vector2Int first, Vector2Int second)
        {
            return Mathf.Abs(first.x - second.x) + Mathf.Abs(first.y - second.y) == 1;
        }
    }

    [Serializable]
    public sealed class PrototypeMapTileData
    {
        [SerializeField] private Vector2Int gridPosition;
        [SerializeField] private string tileName = "Tile";
        [SerializeField] private BuildingConfig buildingConfig;

        public PrototypeMapTileData()
        {
        }

        public PrototypeMapTileData(Vector2Int position, string name, BuildingConfig config = null)
        {
            gridPosition = position;
            tileName = name ?? string.Empty;
            buildingConfig = config;
        }

        public Vector2Int GridPosition => gridPosition;
        public string TileName => tileName;
        public BuildingConfig BuildingConfig => buildingConfig;

        public BoardMoveResolver.TileDefinition ToDefinition()
        {
            return new BoardMoveResolver.TileDefinition(
                tileName,
                buildingConfig != null ? buildingConfig.ToDefinition() : null);
        }
    }
}
