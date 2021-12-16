using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp
{
    class Index : IIndex
    {
        private readonly IReadOnlyDictionary<ushort, Symbol> _symbolsMap;
        private readonly IReadOnlyDictionary<ushort, NGram> _nGramsMap;
        private readonly IReadOnlyDictionary<ushort, Group> _groupsMap;
        private readonly IReadOnlyDictionary<ushort, Sequence> _sequencesMap;
        private readonly IReadOnlyDictionary<ushort, Property> _propertiesMap;

        public Index(IReadOnlyDictionary<ushort, Symbol> symbolsMap,
            IReadOnlyDictionary<ushort, NGram> nGramsMap,
            IReadOnlyDictionary<ushort, Group> groupsMap,
            IReadOnlyDictionary<ushort, Sequence> sequencesMap,
            IReadOnlyDictionary<ushort, Property> propertiesMap)
        {
            _symbolsMap = symbolsMap;
            _nGramsMap = nGramsMap;
            _groupsMap = groupsMap;
            _sequencesMap = sequencesMap;
            _propertiesMap = propertiesMap;
        }

        // TODO значения специальных символов должны задаваться при построении словаря из любых 2-х свободных символов для конкретного словаря.
        public ushort EndSymbol => ushort.MinValue;
        public ushort Wildcard => '*';

        public Sequence[] SearchSequences(ushort[] symbols, int skip, int take)
        {
            if (symbols?.Any() != true)
                return Array.Empty<Sequence>();

            if (!symbols.Any(_ => _ == Wildcard))
                throw new ArgumentException($"Use this method only for wildcard search. The `{nameof(symbols)}` parameter has no wildcard.");

            /*
                Поиск с Джокером

                1. Получаем все NGram для всех возможных комбинаций:
                    где известный символ в начале NGram,
                    где известный символ в конце NGram (символ + `EndSymbol`),
                    где известный символ НЕ в конце NGram (символ + любой символ кроме `EndSymbol` + [(опционально) любой символ вкл. `EndSymbol`]).
                2. Находим все Sequence, в которые входят все найденные NGram с соблюдением позиций в Sequence:
                    начало Sequence
                    конец Sequence
                    середина Sequence - если таких несколько, то проверяется очерёдность позиций между ними:
                        после NGram(1)
                        после NGram(2)
                        и т.д.
                3. (!!!проверить нужен ли этот шаг!!!) Найденные Sequence проверяются окончательно на соответствие искомому значению
                    это необходимо, чтобы не усложнять логику шагов 1 и 2, т.к. NGram переменной длины
                    и одинаковые символы могут входит в разные NGram, например варианты индексации слова `телега`:
                        [те][лега]
                        [теле][га]
                        [те][ле][га]
            */

            return Array.Empty<Sequence>();
        }

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

        // TODO need optimization for speed improvement
        private bool CheckSequence(in Sequence sequence, ushort[] value)
        {
            var sequenceSymbols = GetSequenceValueSymbols(sequence);

            return sequenceSymbols.SequenceEqual(value);
        }

        private ushort[] GetSequenceValueSymbols(in Sequence sequence)
        {
            return sequence.Grams
                .SelectMany(_ => _nGramsMap[_].Symbols)
                .ToArray();
        }
    }
}