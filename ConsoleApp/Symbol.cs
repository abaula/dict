namespace ConsoleApp
{
    public readonly struct Symbol
    {
        public readonly ushort Id;
        public readonly (ushort, ushort[])[] PositionNGrams;
    }
}