using System;

namespace MonopolyPrototype
{
    /// <summary>
    /// Applies an already-modified money request and produces the post-application result.
    /// This is deliberately a pure C# application boundary so presentation listeners stay optional.
    /// </summary>
    public sealed class MoneyStateAdapter
    {
        private int balance;

        public MoneyStateAdapter(int initialBalance = 0)
        {
            balance = initialBalance;
        }

        public int Balance => balance;

        public MoneyChangeResult Apply(MoneyChangeRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var balanceBefore = balance;
            var appliedDelta = request.CurrentDelta;
            balance += appliedDelta;

            return new MoneyChangeResult(
                request,
                appliedDelta,
                balanceBefore,
                balance,
                succeeded: true);
        }
    }
}
