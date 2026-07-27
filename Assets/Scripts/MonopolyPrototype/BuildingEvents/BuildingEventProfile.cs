using UnityEngine;

namespace MonopolyPrototype
{
    [CreateAssetMenu(menuName = "Monopoly Prototype/Building Events/Profile")]
    public sealed class BuildingEventProfile : ScriptableObject
    {
        [SerializeField] private BuildingEventSOEvent onBuildingTriggered;
        [SerializeField] private BuildingEventSOEvent onEffectCommandProduced;
        [SerializeField] private BuildingEventSOEvent onConfirmationCompleted;

        public BuildingEventSOEvent OnBuildingTriggered => onBuildingTriggered;
        public BuildingEventSOEvent OnEffectCommandProduced => onEffectCommandProduced;
        public BuildingEventSOEvent OnConfirmationCompleted => onConfirmationCompleted;

        internal void RaiseBuildingTriggered(BuildingEventContext context)
        {
            onBuildingTriggered?.Raise(context);
        }

        internal void RaiseEffectCommandProduced(BuildingEventContext context)
        {
            onEffectCommandProduced?.Raise(context);
        }

        internal void RaiseConfirmationCompleted(BuildingEventContext context)
        {
            onConfirmationCompleted?.Raise(context);
        }
    }
}
