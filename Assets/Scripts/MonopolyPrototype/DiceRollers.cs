using UnityEngine;

namespace MonopolyPrototype
{
    public interface IDiceRoller
    {
        int Roll();
    }

    public sealed class UnityRandomDiceRoller : IDiceRoller
    {
        public int Roll()
        {
            return Random.Range(1, 7);
        }
    }
}
