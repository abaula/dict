using System;

namespace ConsoleApp
{
    [Serializable]
    class SymbolOld
    {
        public SymbolOld(int hash)
        {
            Hash = hash;
            FirstTwoGrams = Array.Empty<int>();
            LastTwoGrams = Array.Empty<int>();
        }

        public int Hash;
        // 2-граммы у которых символ на 1-й позиции.
        public int[] FirstTwoGrams;
        // 2-граммы у которых символ на 2-й (последней) позиции.
        public int[] LastTwoGrams;
    }
}
