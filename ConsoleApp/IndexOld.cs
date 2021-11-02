using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ConsoleApp
{
    /*
     * Compression
     * https://badevlad.livejournal.com/39585.html
     *
     * Serialization
     * https://docs.microsoft.com/ru-ru/dotnet/api/system.runtime.serialization.iserializable?view=netcore-2.2
     *
     */


    [Serializable]
    class IndexOld : ISerializable
    {
        public IndexOld()
        {
            SymbolsHash = new Dictionary<char, int>();
            Symbols = new Dictionary<int, SymbolOld>();
            TwoGramsHash = new Dictionary<string, int>();
            TwoGrams = new Dictionary<int, TwoGram>();
            Words = new Dictionary<long, Word>();
        }

        public IndexOld(SerializationInfo info, StreamingContext context)
        {
            // SymbolsHash
            var symbolsHash = (KeyValuePair<char, int>[])info.GetValue("SymbolsHash", typeof(KeyValuePair<char, int>[]));
            SymbolsHash = new Dictionary<char, int>(symbolsHash);

            // Symbols
            var symbols = (SymbolOld[])info.GetValue("Symbols", typeof(SymbolOld[]));
            Symbols = symbols.ToDictionary(k => k.Hash);

            // TwoGramsHash
            var twoGramsHash = (KeyValuePair<string, int>[])info.GetValue("TwoGramsHash", typeof(KeyValuePair<string, int>[]));
            TwoGramsHash = new Dictionary<string, int>(twoGramsHash);

            // TwoGrams
            var twoGrams = (TwoGram[])info.GetValue("TwoGrams", typeof(TwoGram[]));
            TwoGrams = twoGrams.ToDictionary(k => k.Hash);

            // Words
            var words = (Word[])info.GetValue("Words", typeof(Word[]));
            Words = words.ToDictionary(k => k.Hash);
        }

        public Dictionary<char, int> SymbolsHash { get; set; }
        public Dictionary<int, SymbolOld> Symbols { get; set; }
        public Dictionary<string, int> TwoGramsHash { get; set; }
        public Dictionary<int, TwoGram> TwoGrams { get; set; }
        public Dictionary<long, Word> Words { get; set; }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // SymbolsHash
            info.AddValue("SymbolsHash", SymbolsHash.ToArray());
            // Symbols
            info.AddValue("Symbols", Symbols.Values.ToArray());
            // TwoGramsHash
            info.AddValue("TwoGramsHash", TwoGramsHash.ToArray());
            // TwoGrams
            info.AddValue("TwoGrams", TwoGrams.Values.ToArray());
            // Words
            info.AddValue("Words", Words.Values.ToArray());
        }

        public bool Check(string word)
        {
            var hash = word.GetCombinedHashCode();

            return Words.ContainsKey(hash);
        }

        public bool Search(string word)
        {
            var twoGrams = TwoGramHelper.GetTwoGrams(word);
            var twoGramsHashes = new List<(int, int)>();
            var tgc = Math.Ceiling((double)word.Length / 2);

            for (var i = 0; i < twoGrams.Count; i+=2)
            {
                var twoGram = twoGrams[i];

                if (!TwoGramsHash.ContainsKey(twoGram))
                    return false;

                twoGramsHashes.Add((PositionHelper.GetPositionIndex(twoGrams.Count, i), TwoGramsHash[twoGram]));
            }

            if (twoGramsHashes.Count < tgc)
            {
                twoGramsHashes.Add((-1, TwoGramsHash[twoGrams[twoGrams.Count - 1]]));
            }

            var allPossibleWords = GetWordsFromPosition(twoGramsHashes[0].Item2, 0);

            if (!allPossibleWords.Any())
                return false;

            //for (var i = 1; i < twoGramsHashes.Count; i++)
            for (var i = twoGramsHashes.Count - 1; i > 0; i--)
            {
                var (twoGramPosition, twoGramHash) = twoGramsHashes[i];
                allPossibleWords = UpdatePossibleWords(twoGramHash, twoGramPosition, allPossibleWords);

                if (!allPossibleWords.Any())
                    return false;
            }

            return true;
        }

        private HashSet<long> UpdatePossibleWords(int twoGramHash, int position, HashSet<long> checkSet)
        {
            var wordsSet = GetWordsFromPosition(twoGramHash, position);
            wordsSet.IntersectWith(checkSet);

            return wordsSet;
        }

        private HashSet<long> GetWordsFromPosition(int twoGramHash, int position)
        {
            var twoGram = TwoGrams[twoGramHash];

            if (!twoGram.TwoGramPositionToWords.ContainsKey(position))
                return new HashSet<long>();

            return twoGram.TwoGramPositionToWords[position].ToHashSet();
        }


        private bool Contains(int[] values, int value)
        {
            var end = values.Length - 1;

            if (end < 0)
                return false;

            var start = 0;

            while (end >= start)
            {
                var i = start + ((end - start) / 2);
                var v = values[i];

                if (v == value)
                    return true;

                if (value < v)
                {
                    end = i - 1;
                }
                else
                {
                    start = i + 1;
                }
            }

            return false;
        }
    }
}
