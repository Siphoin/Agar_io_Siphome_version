using Zenject;

namespace AgarIOSiphome.System.Factories.Installers
{
    public abstract class MonoBehaviourFactoryInstaller<TInterface, TEntity> : MonoInstaller
        where TInterface : class, IMonoBehaviorFactory
        where TEntity : class, TInterface
    {
        public override void InstallBindings()
        {
            Container.Bind<TInterface>().To<TEntity>().AsSingle();
        }
    }
}