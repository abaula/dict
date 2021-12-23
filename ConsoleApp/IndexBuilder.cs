using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp
{
    public class IndexBuilder
    {
        private readonly Dictionary<uint, int> _biGramCounterMap;
        private readonly Dictionary<ushort, List<(ushort, ushort[])>> _symbolsMap;
        private readonly Dictionary<ushort, NGram> _nGramsMap;
        private readonly Dictionary<ushort, Group> _groupsMap;
        private readonly Dictionary<ushort, Sequence> _sequencesMap;
        private readonly Dictionary<ushort, Property> _propertiesMap;

        public IndexBuilder()
        {
            _biGramCounterMap = new Dictionary<uint, int>();
            _symbolsMap = new Dictionary<ushort, List<(ushort, ushort[])>>();
            _nGramsMap = new Dictionary<ushort, NGram>();
            _groupsMap = new Dictionary<ushort, Group>();
            _sequencesMap = new Dictionary<ushort, Sequence>();
            _propertiesMap = new Dictionary<ushort, Property>();
        }

        public IIndex Build(TextGroup[] groups)
        {
            if (_symbolsMap.Any())
                throw new InvalidOperationException();

            BuildSymbolsMap(groups);
            BuildNGramCountersMap(groups);

            foreach (var group in groups)
                HandleGroupSequences(group);

            return new Index(GetSymbolsMap(), GetNGramsMap(), _groupsMap, _sequencesMap, _propertiesMap);
        }

        private void HandleGroupSequences(TextGroup group)
        {
            foreach (var sequence in group.Sequences)
                HandleSequence(sequence);
        }

        private void HandleSequence(TextSequence sequence)
        {
            // TODO Вычисляем варианты разбиения на n-gram
            // и выбираем наилучший вариант опираясь на статистику из biGramCounterMap
            // - чем больше баллов набрал вариант разбиения тем он лучше.
            // Это позволит минимизировать вхождение символа в разные biGram и тем самым уменьшить количество вычислений при поиске.


        }

        private void BuildNGramCountersMap(TextGroup[] groups)
        {
            foreach (var group in groups)
            {
                foreach (var sequence in group.Sequences)
                {
                    var prevSymbol = (ushort)0;

                    for (var i = 0; i < sequence.Sequence.Length; i++)
                    {
                        var symbol = Convert.ToUInt16(sequence.Sequence[i]);
                        var ngram = ((uint)prevSymbol << 16) | symbol;

                        if (_biGramCounterMap.ContainsKey(ngram))
                        {
                            var counter = _biGramCounterMap[ngram];
                            _biGramCounterMap[ngram] = ++counter;
                        }
                        else
                        {
                            _biGramCounterMap.Add(ngram, 1);
                        }

                        prevSymbol = symbol;
                    }
                }
            }
        }

        private void BuildSymbolsMap(IEnumerable<TextGroup> groups)
        {
            foreach (var group in groups)
            {
                foreach (var sequence in group.Sequences)
                {
                    foreach (var symbol in sequence.Sequence)
                    {
                        var symbolKey = Convert.ToUInt16(symbol);

                        if (_symbolsMap.ContainsKey(symbolKey))
                            continue;

                        _symbolsMap.Add(symbolKey, new List<(ushort, ushort[])>());
                    }
                }
            }
        }

        private IReadOnlyDictionary<ushort, NGram> GetNGramsMap()
        {
            return _nGramsMap;
        }

        private IReadOnlyDictionary<ushort, Symbol> GetSymbolsMap()
        {
            return _symbolsMap.ToDictionary(
                    _ => _.Key,
                    _ => new Symbol(_.Key, _.Value.ToArray())
                );
        }
    }
}