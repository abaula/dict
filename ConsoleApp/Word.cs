using System;

namespace ConsoleApp
{
    [Serializable]
    class Word
    {
        public Word(long hash)
        {
            Hash = hash;
            TwoGrams = null;
            Value = null;
        }

        public long Hash;
        public int[] TwoGrams;
        public string Value;
    }
}
