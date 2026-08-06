namespace CrystalEngine.DI.Tests
{
    public class GameStageTwoInstaller : MonoInstaller
    {
        protected override void InstallBindings()
        {
            Container.Bind<IAudioService>().To<UnityAudioService>().AsSingle();
            Container.Bind<IScoreManager>().To<GameplayScoreManager>().AsSingle();
        }
    }
}
