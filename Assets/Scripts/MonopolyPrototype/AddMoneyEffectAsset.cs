using UnityEngine;

namespace MonopolyPrototype
{
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
}
