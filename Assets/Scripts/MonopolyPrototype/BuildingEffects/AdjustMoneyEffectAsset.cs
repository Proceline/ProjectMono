using UnityEngine;

namespace MonopolyPrototype
{
    [CreateAssetMenu(menuName = "Monopoly Prototype/Building Effects/Adjust Money")]
    public sealed class AdjustMoneyEffectAsset : BuildingEffectAsset
    {
        [SerializeField] private int moneyDelta;
        [SerializeField] private MoneyChangeRequestedSOEvent onMoneyChangeRequested;
        [SerializeField] private MoneyChangedSOEvent onMoneyChanged;

        public override BuildingEffectType EffectType => BuildingEffectType.AdjustMoney;
        public MoneyChangeRequestedSOEvent OnMoneyChangeRequested => onMoneyChangeRequested;
        public MoneyChangedSOEvent OnMoneyChanged => onMoneyChanged;

        public override BuildingEffectDefinition ToDefinition()
        {
            return BuildingEffectDefinition.AdjustMoney(moneyDelta);
        }

        public void Configure(int delta)
        {
            moneyDelta = delta;
        }

        internal void RaiseMoneyChangeRequested(MoneyChangeRequest request)
        {
            onMoneyChangeRequested?.Raise(request);
        }

        internal void RaiseMoneyChanged(MoneyChangeResult result)
        {
            onMoneyChanged?.Raise(result);
        }
    }
}
