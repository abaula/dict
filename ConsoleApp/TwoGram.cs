using System;
using System.Collections.Generic;

namespace ConsoleApp
{
    [Serializable]
    struct TwoGram
    {
        public TwoGram(int hash)
        {
            Hash = hash;
            TwoGramPositionToWords = new Dictionary<int, long[]>();
            FirstSymbol = 0;
            SecondSymbol = 0;
        }

        public int Hash;
        public int FirstSymbol;
        public int SecondSymbol;

        // Список слов в которые входит 2-грамма.
        // Ключ - указатель позиции вхождения 2-граммы в слово.
        public Dictionary<int, long[]> TwoGramPositionToWords;

        // Двуграмма с единственным символом, наприме "с", "к", "в", "и" и т.д.
        public bool IsSingleSymbolGram()
        {
            return FirstSymbol == SecondSymbol;
        }
    }
}
