using System.Collections.Generic;

namespace ConsoleApp
{
    static class TwoGramHelper
    {
        public static IList<string> GetTwoGrams(string word)
        {
            const int twoGramSymbolCount = 2;
            var twoGrammBuffer = new List<string>();

            if (word.Length < twoGramSymbolCount)
            {
                twoGrammBuffer.Add(word);
            }

            for (var start = 0; start <= word.Length - twoGramSymbolCount; start++)
            {
                var twoGram = word.Substring(start, twoGramSymbolCount);
                twoGrammBuffer.Add(twoGram);
            }

            return twoGrammBuffer;
        }
    }
}
