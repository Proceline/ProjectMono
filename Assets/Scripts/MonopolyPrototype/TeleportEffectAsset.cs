using UnityEngine;

namespace MonopolyPrototype
{
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
}
