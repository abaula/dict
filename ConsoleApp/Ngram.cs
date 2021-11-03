namespace ConsoleApp
{
    public readonly struct NGram
    {
        public readonly ushort Id;
        public readonly ushort[] Symbols;
        public readonly (ushort, ushort[])[] PositionSequences;
    }
}