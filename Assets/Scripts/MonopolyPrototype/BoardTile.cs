using UnityEngine;

namespace MonopolyPrototype
{
    public sealed class BoardTile : MonoBehaviour
    {
        [SerializeField] private string tileName = "Tile";
        [SerializeField] private FacilityInteractionType interactionType = FacilityInteractionType.None;
        [SerializeField] private string feedbackLog = string.Empty;

        public string TileName => tileName;
        public FacilityInteractionType InteractionType => interactionType;
        public string FeedbackLog => feedbackLog;

        public BoardMoveResolver.TileDefinition ToDefinition()
        {
            return new BoardMoveResolver.TileDefinition(tileName, interactionType, feedbackLog);
        }

        public void Configure(string name, FacilityInteractionType type, string log)
        {
            tileName = name;
            interactionType = type;
            feedbackLog = log;
            gameObject.name = $"Tile - {tileName}";
        }
    }
}
