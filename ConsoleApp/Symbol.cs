using System;

namespace ConsoleApp
{
    public readonly struct Symbol
    {
        public Symbol(ushort id)
        {
            Id = id;
            PositionNGrams = Array.Empty<(ushort, ushort[])>();
        }

        public Symbol(ushort id, (ushort, ushort[])[] positionNGrams)
        {
            Id = id;
            PositionNGrams = positionNGrams;
        }

        public readonly ushort Id;
        public readonly (ushort, ushort[])[] PositionNGrams;
    }
}