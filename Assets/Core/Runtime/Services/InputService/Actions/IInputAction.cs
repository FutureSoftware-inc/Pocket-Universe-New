namespace CrystalEngine.Services
{
    public interface IInputAction
    {
        string Name { get; }
        bool WasPressedThisFrame();
        bool IsPressed();
        bool WasReleasedThisFrame();
    }
}
