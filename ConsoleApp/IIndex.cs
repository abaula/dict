namespace ConsoleApp
{
    public interface IIndex
    {
        Sequence[] SearchSequences(ushort[] symbols, int skip, int take);
        Sequence GetSequenceEqualToSymbols(ushort[] symbols);
        Sequence[] GetSequencesForGroup(ushort groupId);
    }
}