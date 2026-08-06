namespace CrystalEngineEditor
{
    public interface ISubViewSupport
    {
        void SetSubViewMode(byte modeId);
        void NotifyPathChanged();
    }
}