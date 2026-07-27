using System;
using UnityEngine;

namespace MonopolyPrototype
{
    public enum BuildingEventPhase
    {
        BuildingTriggered,
        EffectCommandProduced,
        ConfirmationCompleted
    }

    [Serializable]
    public sealed class BuildingEventContext
    {
        [SerializeField] private string buildingName;
        [SerializeField] private int tileIndex;
        [SerializeField] private MoveEventTiming timing;
        [SerializeField] private BuildingEventPhase phase;
        [SerializeField] private BuildingEffectType effectType;
        [SerializeField] private int moneyDelta;
        [SerializeField] private int targetTileIndex;
        [SerializeField] private string message;

        public BuildingEventContext(
            string buildingName,
            int tileIndex,
            MoveEventTiming timing,
            BuildingEventPhase phase,
            BuildingEffectType effectType,
            int moneyDelta,
            int targetTileIndex,
            string message)
        {
            this.buildingName = buildingName ?? string.Empty;
            this.tileIndex = tileIndex;
            this.timing = timing;
            this.phase = phase;
            this.effectType = effectType;
            this.moneyDelta = moneyDelta;
            this.targetTileIndex = targetTileIndex;
            this.message = message ?? string.Empty;
        }

        public string BuildingName => buildingName;
        public int TileIndex => tileIndex;
        public MoveEventTiming Timing => timing;
        public BuildingEventPhase Phase => phase;
        public BuildingEffectType EffectType => effectType;
        public int MoneyDelta => moneyDelta;
        public int TargetTileIndex => targetTileIndex;
        public string Message => message;
    }
}
