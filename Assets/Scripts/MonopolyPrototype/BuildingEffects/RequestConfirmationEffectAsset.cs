using UnityEngine;

namespace MonopolyPrototype
{
    [CreateAssetMenu(menuName = "Monopoly Prototype/Building Effects/Request Confirmation")]
    public sealed class RequestConfirmationEffectAsset : BuildingEffectAsset
    {
        [SerializeField] private string message;

        public override BuildingEffectType EffectType => BuildingEffectType.RequestConfirmation;

        public override BuildingEffectDefinition ToDefinition()
        {
            return BuildingEffectDefinition.RequestConfirmation(message);
        }

        public void Configure(string confirmationMessage)
        {
            message = confirmationMessage ?? string.Empty;
        }
    }
}
