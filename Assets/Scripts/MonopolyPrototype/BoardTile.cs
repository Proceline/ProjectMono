using UnityEngine;

namespace MonopolyPrototype
{
    public sealed class BoardTile : MonoBehaviour
    {
        [SerializeField] private string tileName = "Tile";
        [SerializeField] private BuildingConfig buildingConfig;

        public string TileName => tileName;
        public BuildingConfig BuildingConfig => buildingConfig;

        public BoardMoveResolver.TileDefinition ToDefinition()
        {
            return new BoardMoveResolver.TileDefinition(tileName, GetBuildingDefinition());
        }

        public void Configure(
            string name,
            BuildingConfig config = null)
        {
            tileName = name;
            buildingConfig = config;
            gameObject.name = $"Tile - {tileName}";
        }

        private BuildingDefinition GetBuildingDefinition()
        {
            return buildingConfig != null ? buildingConfig.ToDefinition() : null;
        }
    }
}
