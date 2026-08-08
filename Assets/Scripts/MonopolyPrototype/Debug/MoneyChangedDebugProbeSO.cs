using UnityEngine;

namespace MonopolyPrototype
{
    [CreateAssetMenu(menuName = "Monopoly Prototype/Debug/Money Changed Debug Probe")]
    public sealed class MoneyChangedDebugProbeSO : ScriptableObject
    {
        [SerializeField] private int addAmount = 25;
        [SerializeField] private int subtractAmount = 10;
        [SerializeField] private int overrideAmount;
        [SerializeField] private string logPrefix = "Money debug amount";

        public int AddAmount => addAmount;
        public int SubtractAmount => subtractAmount;
        public int OverrideAmount => overrideAmount;

        public void Configure(int addAmount, int subtractAmount, int overrideAmount)
        {
            this.addAmount = addAmount;
            this.subtractAmount = subtractAmount;
            this.overrideAmount = overrideAmount;
        }

        public void AddMoney(MoneyChangeRequest request)
        {
            if (request != null)
            {
                AddMoney(request.CurrentDeltaPayload);
            }
        }

        public void SubtractMoney(MoneyChangeRequest request)
        {
            if (request != null)
            {
                SubtractMoney(request.CurrentDeltaPayload);
            }
        }

        public void OverrideMoney(MoneyChangeRequest request)
        {
            if (request != null)
            {
                OverrideMoney(request.CurrentDeltaPayload);
            }
        }

        public void LogMoney(MoneyChangeRequest request)
        {
            if (request != null)
            {
                LogMoney(request.CurrentDeltaPayload);
            }
        }

        public void AddMoney(int[] payload)
        {
            if (TryGetAmount(payload, out var amount))
            {
                payload[0] = amount + addAmount;
            }
        }

        public void SubtractMoney(int[] payload)
        {
            if (TryGetAmount(payload, out var amount))
            {
                payload[0] = amount - subtractAmount;
            }
        }

        public void OverrideMoney(int[] payload)
        {
            if (TryGetAmount(payload, out _))
            {
                payload[0] = overrideAmount;
            }
        }

        public void LogMoney(int[] payload)
        {
            if (TryGetAmount(payload, out var amount))
            {
                Debug.Log($"{logPrefix}: {amount}.", this);
            }
        }

        private static bool TryGetAmount(int[] payload, out int amount)
        {
            if (payload == null || payload.Length == 0)
            {
                amount = 0;
                return false;
            }

            amount = payload[0];
            return true;
        }
    }
}
