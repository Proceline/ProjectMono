using NUnit.Framework;

namespace MonopolyPrototype
{
    public sealed class MoneyStateAdapterTests
    {
        [Test]
        public void Apply_UsesMutatedRequestDelta_UpdatesBalanceAndResult()
        {
            var adapter = new MoneyStateAdapter(initialBalance: 100);
            var request = CreateRequest(80);
            request.AddToCurrentDelta(5);

            var result = adapter.Apply(request);

            Assert.That(result.RequestedDelta, Is.EqualTo(85));
            Assert.That(result.AppliedDelta, Is.EqualTo(85));
            Assert.That(result.BalanceBefore, Is.EqualTo(100));
            Assert.That(result.BalanceAfter, Is.EqualTo(185));
            Assert.That(result.Succeeded, Is.True);
            Assert.That(adapter.Balance, Is.EqualTo(185));
        }

        [Test]
        public void Apply_NullRequest_ThrowsArgumentNullException()
        {
            var adapter = new MoneyStateAdapter();

            Assert.Throws<System.ArgumentNullException>(() => adapter.Apply(null));
        }

        private static MoneyChangeRequest CreateRequest(int delta)
        {
            return new MoneyChangeRequest(
                "Bank",
                "Bank_AdjustMoney",
                BuildingEffectType.AdjustMoney,
                1,
                4,
                MoveEventTiming.Stop,
                0,
                delta);
        }
    }
}
