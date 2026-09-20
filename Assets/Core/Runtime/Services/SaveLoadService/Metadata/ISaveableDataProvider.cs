namespace CrystalEngine.Services
{
    public interface ISaveableDataProvider
    {
        string DataKey { get; }
        SaveContext Context { get; }
    }
}