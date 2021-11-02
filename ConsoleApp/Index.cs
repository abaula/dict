using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp
{
    public class Index
    {
        private readonly IReadOnlyDictionary<ushort, Symbol> _symbolsMap;
        private readonly IReadOnlyDictionary<ushort, NGram> _nGramsMap;
        private readonly IReadOnlyDictionary<ushort, Group> _groupsMap;
        private readonly IReadOnlyDictionary<ushort, Sequence> _sequencesMap;

        public Sequence GetSequenceEqualToSymbols(ushort[] symbols)
        {
            if (symbols?.Any() != true)
                return default;

            var sequences = GetSequencesStartsWith(symbols.First());
            var result = sequences.SingleOrDefault(_ => CheckSequence(_, symbols));

            return result;
        }

        public Sequence[] GetSequencesForGroup(ushort groupId)
        {
            if (!_groupsMap.ContainsKey(groupId))
                return Array.Empty<Sequence>();

            var group = _groupsMap[groupId];

            return group.Sequences
                .Select(_ => _sequencesMap[_])
                .ToArray();
        }

        private Sequence[] GetSequencesStartsWith(ushort value)
        {
            if (!_symbolsMap.ContainsKey(value))
                return Array.Empty<Sequence>();

            var symbol = _symbolsMap[value];
            // Получаем NGrams где указанный символ стоит в начале.
            var ngramIds = symbol.PositionNGrams.GetForPosition(0);
            var ngrams = ngramIds.Select(_ => _nGramsMap[_]).ToArray();
            // Получаем Sequences где найденные NGrams стоят в начале.
            var sequenceIds = ngrams.SelectMany(_ => _.PositionSequences.GetForPosition(0)).ToArray();
            var sequences = sequenceIds.Select(_ => _sequencesMap[_]).ToArray();

            return sequences;
        }

        private bool CheckSequence(Sequence sequence, ushort[] value)
        {
            var sequenceSymbols = GetValueSymbols(sequence);

            return sequenceSymbols.SequenceEqual(value);
        }

        private ushort[] GetValueSymbols(Sequence sequence)
        {
            return sequence.Grams
                .SelectMany(_ => _nGramsMap[_].Symbols)
                .ToArray();
        }
    }
}