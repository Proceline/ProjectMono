using System;
using UnityEngine;

namespace MonopolyPrototype
{
    [Serializable]
    public sealed class MoneyChangeResult
    {
        [SerializeField] private string buildingName;
        [SerializeField] private string effectName;
        [SerializeField] private BuildingEffectType effectType;
        [SerializeField] private int effectIndex;
        [SerializeField] private int tileIndex;
        [SerializeField] private MoveEventTiming timing;
        [SerializeField] private int transactionId;
        [SerializeField] private int baseDelta;
        [SerializeField] private int requestedDelta;
        [SerializeField] private int appliedDelta;
        [SerializeField] private int balanceBefore;
        [SerializeField] private int balanceAfter;
        [SerializeField] private bool succeeded;
        [SerializeField] private string failureReason;

        public MoneyChangeResult(
            MoneyChangeRequest request,
            int appliedDelta,
            int balanceBefore,
            int balanceAfter,
            bool succeeded,
            string failureReason = "")
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            buildingName = request.BuildingName;
            effectName = request.EffectName;
            effectType = request.EffectType;
            effectIndex = request.EffectIndex;
            tileIndex = request.TileIndex;
            timing = request.Timing;
            transactionId = request.TransactionId;
            baseDelta = request.BaseDelta;
            requestedDelta = request.CurrentDelta;
            this.appliedDelta = appliedDelta;
            this.balanceBefore = balanceBefore;
            this.balanceAfter = balanceAfter;
            this.succeeded = succeeded;
            this.failureReason = failureReason ?? string.Empty;
        }

        public string BuildingName => buildingName;
        public string EffectName => effectName;
        public BuildingEffectType EffectType => effectType;
        public int EffectIndex => effectIndex;
        public int TileIndex => tileIndex;
        public MoveEventTiming Timing => timing;
        public int TransactionId => transactionId;
        public int BaseDelta => baseDelta;
        public int RequestedDelta => requestedDelta;
        public int AppliedDelta => appliedDelta;
        public int BalanceBefore => balanceBefore;
        public int BalanceAfter => balanceAfter;
        public bool Succeeded => succeeded;
        public string FailureReason => failureReason;
    }
}
