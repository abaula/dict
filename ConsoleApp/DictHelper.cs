using System.Collections.Generic;

namespace ConsoleApp
{
    static class DictHelper
    {
        public static TValue GetValue<TKey, TValue>(Dictionary<TKey, TValue> dict, TKey key)
            where TValue : new()
        {
            if (dict.ContainsKey(key))
                return dict[key];

            var value = new TValue();
            dict.Add(key, value);

            return value;
        }


    }
}
