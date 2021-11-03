using System;
using System.Linq;

namespace ConsoleApp
{
    static class PositionHelper
    {
        public static int GetPositionIndex(int length, int position)
        {
            if (length - 1 == position)
                return -1;

            return position;
        }

        // TODO need Unit test for ensure the `collection` is ordered by position with ascending order.
        public static ushort[] GetForPosition(this (ushort, ushort[])[] collection, ushort position)
        {
            if (collection?.Any() != true)
                return Array.Empty<ushort>();

            foreach (var (pos, val) in collection)
            {
                if (pos < position)
                    continue;

                if (pos > position)
                    break;

                return val;
            }

            return Array.Empty<ushort>();
        }
    }
}
