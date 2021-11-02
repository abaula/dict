
using System;

namespace ConsoleApp
{
    static class HashHelper
    {
        public static long GetCombinedHashCode(this string value)
        {
            return (long)value.GetPersistentHashCode() << 32 | (uint)value.GetDeterministicHashCode();
        }


        //[Obsolete("Не использовать, хранится только для примера", true)]
        /// <see cref="https://andrewlock.net/why-is-string-gethashcode-different-each-time-i-run-my-program-in-net-core/"/>
        public static int GetDeterministicHashCode(this string value)
        {
            unchecked
            {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;

                for (var i = 0; i < value.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ value[i];

                    if (i == value.Length - 1)
                        break;

                    hash2 = ((hash2 << 5) + hash2) ^ value[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }

        /// <summary>
        /// Gets a hash code for the specified <see cref="string"/>; this hash code is guaranteed not to change in the future.
        /// </summary>
        /// <param name="value">The <see cref="string"/> to hash.</param>
        /// <returns>A hash code for the specified <see cref="string"/>.</returns>
        /// <remarks>Based on <a href="http://www.azillionmonkeys.com/qed/hash.html">SuperFastHash</a>.</remarks>
        /// <see cref="https://github.com/LogosBible/Logos.Utility/blob/master/src/Logos.Utility/HashCodeUtility.cs"/>
        public static int GetPersistentHashCode(this string value)
        {
            unchecked
            {
                // check for degenerate input
                if (string.IsNullOrEmpty(value))
                    return 0;

                int length = value.Length;
                uint hash = (uint)length;

                int remainder = length & 1;
                length >>= 1;

                // main loop
                int index = 0;
                for (; length > 0; length--)
                {
                    hash += value[index];
                    uint temp = (uint)(value[index + 1] << 11) ^ hash;
                    hash = (hash << 16) ^ temp;
                    index += 2;
                    hash += hash >> 11;
                }

                // handle odd string length
                if (remainder == 1)
                {
                    hash += value[index];
                    hash ^= hash << 11;
                    hash += hash >> 17;
                }

                // force "avalanching" of final 127 bits
                hash ^= hash << 3;
                hash += hash >> 5;
                hash ^= hash << 4;
                hash += hash >> 17;
                hash ^= hash << 25;
                hash += hash >> 6;

                return (int)hash;
            }
        }
    }
}
