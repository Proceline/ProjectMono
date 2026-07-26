using UnityEngine;

namespace MonopolyPrototype
{
    [CreateAssetMenu(menuName = "Monopoly Prototype/Building Effects/Show Feedback")]
    public sealed class ShowFeedbackEffectAsset : BuildingEffectAsset
    {
        [SerializeField] private string message;

        public override BuildingEffectType EffectType => BuildingEffectType.ShowFeedback;

        public override BuildingEffectDefinition ToDefinition()
        {
            return BuildingEffectDefinition.ShowFeedback(message);
        }

        public void Configure(string feedbackMessage)
        {
            message = feedbackMessage ?? string.Empty;
        }
    }
}
