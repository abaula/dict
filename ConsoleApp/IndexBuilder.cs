using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp
{
    class IndexBuilder
    {
        private readonly Dictionary<int, List<int>> _symbolFirstTwoGrams;
        private readonly Dictionary<int, List<int>> _symbolLastTwoGrams;
        private readonly Dictionary<long, List<int>> _wordTwoGrams;
        private readonly Dictionary<int, HashSet<int>> _twoGramToPosition;
        private readonly Dictionary<(int, int), List<long>> _twoGramToPositionToWords;

        public IndexBuilder()
        {
            _symbolFirstTwoGrams = new Dictionary<int, List<int>>();
            _symbolLastTwoGrams = new Dictionary<int, List<int>>();
            _wordTwoGrams = new Dictionary<long, List<int>>();
            _twoGramToPosition = new Dictionary<int, HashSet<int>>();
            _twoGramToPositionToWords = new Dictionary<(int, int), List<long>>();
        }

        public IndexOld Build(IEnumerable<string> words)
        {
            var index = new IndexOld();

            foreach (var word in words)
            {
                var symbols = GetWordSymbols(word, index);
                var twoGrams = GetWordTwoGrams(word, symbols, index);
                IndexWord(word, twoGrams, index);
            }

            // проставляем ссылки
            foreach (var symbol in index.Symbols.Values)
            {

                symbol.FirstTwoGrams = _symbolFirstTwoGrams.ContainsKey(symbol.Hash)
                    ? _symbolFirstTwoGrams[symbol.Hash].OrderBy(v => v).ToArray()
                    : Array.Empty<int>();

                symbol.LastTwoGrams = _symbolLastTwoGrams.ContainsKey(symbol.Hash)
                    ? _symbolLastTwoGrams[symbol.Hash].OrderBy(v => v).ToArray()
                    : Array.Empty<int>();
            }

            foreach (var twoGram in index.TwoGrams.Values)
            {
                if (!_twoGramToPosition.ContainsKey(twoGram.Hash))
                    continue;

                var positions = _twoGramToPosition[twoGram.Hash];

                foreach (var position in positions)
                {
                    twoGram.TwoGramPositionToWords[position] =
                        _twoGramToPositionToWords.ContainsKey((twoGram.Hash, position))
                            ? _twoGramToPositionToWords[(twoGram.Hash, position)].OrderBy(v => v).ToArray()
                            : Array.Empty<long>();
                }
            }

            foreach (var (hash, word) in index.Words)
            {
                var newValue = _wordTwoGrams.ContainsKey(word.Hash)
                    ? _wordTwoGrams[word.Hash].OrderBy(v => v).ToArray()
                    : Array.Empty<int>();

                Array.Resize(ref word.TwoGrams, newValue.Length);
            }

            return index;
        }

        private ICollection<SymbolOld> GetWordSymbols(string word, IndexOld index)
        {
            var symbols = new List<SymbolOld>();
            var chars = word.ToCharArray().Distinct();

            foreach (var ch in chars)
            {
                var hash = GetSymbolHash(ch, index);

                if (index.Symbols.ContainsKey(hash))
                {
                    symbols.Add(index.Symbols[hash]);
                    continue;
                }

                var symbol = new SymbolOld(hash);
                index.Symbols.Add(hash, symbol);
                symbols.Add(symbol);
            }

            return symbols;
        }

        private ICollection<TwoGram> GetWordTwoGrams(string word, ICollection<SymbolOld> wordSymbols, IndexOld index)
        {
            var twoGrams = new List<TwoGram>();
            var twoGrammBuffer = TwoGramHelper.GetTwoGrams(word);

            foreach (var tg in twoGrammBuffer)
            {
                var hash = GetTwoGramHash(tg, index);

                if (index.TwoGrams.ContainsKey(hash))
                {
                    twoGrams.Add(index.TwoGrams[hash]);
                    continue;
                }

                var firstSymbol = wordSymbols.Single(ws => ws.Hash == GetSymbolHash(tg[0], index));
                var secondSymbol = wordSymbols.Single(ws => ws.Hash == GetSymbolHash(tg[tg.Length - 1], index));

                var twoGram = new TwoGram(hash)
                {
                    FirstSymbol = firstSymbol.Hash,
                    SecondSymbol = secondSymbol.Hash
                };

                DictHelper.GetValue(_symbolFirstTwoGrams, firstSymbol.Hash).Add(twoGram.Hash);
                DictHelper.GetValue(_symbolLastTwoGrams, secondSymbol.Hash).Add(twoGram.Hash);
                index.TwoGrams.Add(hash, twoGram);
                twoGrams.Add(twoGram);
            }

            return twoGrams;
        }

        private void IndexWord(string word, ICollection<TwoGram> twoGrams, IndexOld index)
        {
            var hash = word.GetCombinedHashCode();
            var w = new Word(hash){ Value = word };

            var position = 0;
            foreach (var twoGram in twoGrams)
            {
                DictHelper.GetValue(_wordTwoGrams, w.Hash).Add(twoGram.Hash);
                DictHelper.GetValue(_twoGramToPosition, twoGram.Hash).Add(PositionHelper.GetPositionIndex(twoGrams.Count, position));
                DictHelper.GetValue(_twoGramToPositionToWords, (twoGram.Hash, PositionHelper.GetPositionIndex(twoGrams.Count, position))).Add(w.Hash);
                position++;
            }

            if (index.Words.ContainsKey(w.Hash))
            {
                var ww = index.Words[w.Hash];
                var h1 = word.GetDeterministicHashCode();
                var h2 = ww.Value.GetDeterministicHashCode();
                return;
            }

            index.Words.Add(w.Hash, w);
        }

        private int GetSymbolHash(char symbol, IndexOld index)
        {
            if (index.SymbolsHash.ContainsKey(symbol))
                return index.SymbolsHash[symbol];

            var hash = index.SymbolsHash.Count + 1;
            index.SymbolsHash.Add(symbol, hash);

            return hash;
        }

        private int GetTwoGramHash(string twoGram, IndexOld index)
        {
            if (index.TwoGramsHash.ContainsKey(twoGram))
                return index.TwoGramsHash[twoGram];

            var hash = index.TwoGramsHash.Count + 1;
            index.TwoGramsHash.Add(twoGram, hash);

            return hash;
        }
    }
}
