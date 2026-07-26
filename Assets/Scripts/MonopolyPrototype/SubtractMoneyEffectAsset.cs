using UnityEngine;

namespace MonopolyPrototype
{
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
}
