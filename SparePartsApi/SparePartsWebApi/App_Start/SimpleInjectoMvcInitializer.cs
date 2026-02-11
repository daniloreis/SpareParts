using Infrastructure;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;
using System.Web.Http;

namespace SparePartsWebApi
{
    public class SimpleInjectoMvcInitializer
    {
        public static void Initialize()
        {
            var container = new Container();

            container.Options.DefaultScopedLifestyle = new SimpleInjector.Lifestyles.AsyncScopedLifestyle();

            container.Register<ISapIntegrationFacade, SapIntegrationFacade>();

            //container.Options.DefaultScopedLifestyle = new WebApiRequestLifestyle();

            InitializeContainer(container);

            container.RegisterWebApiControllers(GlobalConfiguration.Configuration);

            container.Verify();

            GlobalConfiguration.Configuration.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);
        }

        private static void InitializeContainer(Container container)
        {
            BootStrapper.Register(container);
        }
    }
}