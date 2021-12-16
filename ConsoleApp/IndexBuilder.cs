using System;
using System.Collections.Generic;

namespace ConsoleApp
{
    public class IndexBuilder
    {
        public IIndex Build(IEnumerable<TextGroup> groups)
        {
            var symbolsMap = new Dictionary<ushort, Symbol>();
            var nGramsMap = new Dictionary<ushort, NGram>();
            var groupsMap = new Dictionary<ushort, Group>();
            var sequencesMap = new Dictionary<ushort, Sequence>();
            var propertiesMap = new Dictionary<ushort, Property>();

            foreach (var group in groups)
            {

            }

            return new Index(symbolsMap, nGramsMap, groupsMap, sequencesMap, propertiesMap);
        }
    }
}