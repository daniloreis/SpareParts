using Infrastructure;
using SimpleInjector;

namespace SPW.App_Start
{
    public static class BootStrapper
    {
        public static void Register(Container container)
        {
            container.Register<ISapIntegrationFacade, SapIntegrationFacade>();
        }
    }
}