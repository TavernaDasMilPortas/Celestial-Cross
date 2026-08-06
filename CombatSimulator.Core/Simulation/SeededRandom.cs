using System;

namespace CombatSimulator.Core.Simulation
{
    public class SeededRandom
    {
        private Random _random;
        public long CurrentSeed { get; private set; }

        public SeededRandom(long seed)
        {
            CurrentSeed = seed;
            // .NET Random uses int for seed, so we cast it down. 
            // In a real deterministic scenario we might want a custom PRNG.
            _random = new Random((int)seed);
        }

        public int Next(int minValue, int maxValue)
        {
            CurrentSeed = _random.Next(); // just mutating the state to keep track
            return _random.Next(minValue, maxValue);
        }

        public float NextFloat()
        {
            CurrentSeed = _random.Next();
            return (float)_random.NextDouble();
        }
    }
}
