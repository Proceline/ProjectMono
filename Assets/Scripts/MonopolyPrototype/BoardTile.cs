using UnityEngine;

namespace MonopolyPrototype
{
    public sealed class BoardTile : MonoBehaviour
    {
        [SerializeField] private string tileName = "Tile";
        [SerializeField] private FacilityInteractionType interactionType = FacilityInteractionType.None;
        [SerializeField] private string feedbackLog = string.Empty;
        [SerializeField] private BuildingConfig buildingConfig;

        public string TileName => tileName;
        public FacilityInteractionType InteractionType => interactionType;
        public string FeedbackLog => feedbackLog;
        public BuildingConfig BuildingConfig => buildingConfig;

        public BoardMoveResolver.TileDefinition ToDefinition()
        {
            return new BoardMoveResolver.TileDefinition(tileName, interactionType, feedbackLog, GetBuildingDefinition());
        }

        public void Configure(
            string name,
            FacilityInteractionType type,
            string log,
            BuildingConfig config = null)
        {
            tileName = name;
            interactionType = type;
            feedbackLog = log;
            buildingConfig = config;
            gameObject.name = $"Tile - {tileName}";
        }

        private BuildingDefinition GetBuildingDefinition()
        {
            return buildingConfig != null ? buildingConfig.ToDefinition() : null;
        }
    }
}
