using System;
using UnityEngine;

namespace MonopolyPrototype
{
    [Serializable]
    public sealed class MoneyChangeRequest
    {
        [SerializeField] private string buildingName;
        [SerializeField] private string effectName;
        [SerializeField] private BuildingEffectType effectType;
        [SerializeField] private int effectIndex;
        [SerializeField] private int tileIndex;
        [SerializeField] private MoveEventTiming timing;
        [SerializeField] private int transactionId;
        [SerializeField] private int baseDelta;
        [SerializeField] private int[] currentDeltaPayload = new int[1];

        public MoneyChangeRequest(
            string buildingName,
            string effectName,
            BuildingEffectType effectType,
            int effectIndex,
            int tileIndex,
            MoveEventTiming timing,
            int transactionId,
            int baseDelta)
        {
            this.buildingName = buildingName ?? string.Empty;
            this.effectName = effectName ?? string.Empty;
            this.effectType = effectType;
            this.effectIndex = effectIndex;
            this.tileIndex = tileIndex;
            this.timing = timing;
            this.transactionId = transactionId;
            this.baseDelta = baseDelta;
            currentDeltaPayload = new[] { baseDelta };
        }

        public string BuildingName => buildingName;
        public string EffectName => effectName;
        public BuildingEffectType EffectType => effectType;
        public int EffectIndex => effectIndex;
        public int TileIndex => tileIndex;
        public MoveEventTiming Timing => timing;
        public int TransactionId => transactionId;
        public int BaseDelta => baseDelta;
        public int CurrentDelta => CurrentDeltaPayload[0];
        public int[] CurrentDeltaPayload
        {
            get
            {
                if (currentDeltaPayload == null || currentDeltaPayload.Length == 0)
                {
                    currentDeltaPayload = new[] { baseDelta };
                }

                return currentDeltaPayload;
            }
        }

        public void SetCurrentDelta(int delta)
        {
            CurrentDeltaPayload[0] = delta;
        }

        public void AddToCurrentDelta(int delta)
        {
            CurrentDeltaPayload[0] += delta;
        }
    }
}
