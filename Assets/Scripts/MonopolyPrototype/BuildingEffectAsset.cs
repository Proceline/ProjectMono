using UnityEngine;

namespace MonopolyPrototype
{
    public abstract class BuildingEffectAsset : ScriptableObject
    {
        public abstract BuildingEffectType EffectType { get; }

        public abstract BuildingEffectDefinition ToDefinition();

        public BuildingEffectCommand ToCommand()
        {
            return BuildingRuleResolver.CreateCommand(ToDefinition());
        }
    }

    [CreateAssetMenu(menuName = "Monopoly Prototype/Building Effects/Add Money")]
    public sealed class AddMoneyEffectAsset : BuildingEffectAsset
    {
        [SerializeField] private int moneyAmount;

        public override BuildingEffectType EffectType => BuildingEffectType.AddMoney;

        public override BuildingEffectDefinition ToDefinition()
        {
            return BuildingEffectDefinition.AddMoney(moneyAmount);
        }

        public void Configure(int amount)
        {
            moneyAmount = amount;
        }
    }

    [CreateAssetMenu(menuName = "Monopoly Prototype/Building Effects/Subtract Money")]
    public sealed class SubtractMoneyEffectAsset : BuildingEffectAsset
    {
        [SerializeField] private int moneyAmount;

        public override BuildingEffectType EffectType => BuildingEffectType.SubtractMoney;

        public override BuildingEffectDefinition ToDefinition()
        {
            return BuildingEffectDefinition.SubtractMoney(moneyAmount);
        }

        public void Configure(int amount)
        {
            moneyAmount = amount;
        }
    }

    [CreateAssetMenu(menuName = "Monopoly Prototype/Building Effects/Teleport")]
    public sealed class TeleportEffectAsset : BuildingEffectAsset
    {
        [SerializeField] private int targetTileIndex;

        public override BuildingEffectType EffectType => BuildingEffectType.Teleport;

        public override BuildingEffectDefinition ToDefinition()
        {
            return BuildingEffectDefinition.TeleportTo(targetTileIndex);
        }

        public void Configure(int targetIndex)
        {
            targetTileIndex = targetIndex;
        }
    }

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
