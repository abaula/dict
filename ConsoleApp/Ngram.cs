namespace ConsoleApp
{
    public struct NGram
    {
        public ushort Id;
        public ushort[] Symbols;
        public (ushort, ushort[])[] PositionSequences;
    }
}