using System.Web.Mvc;
using System.Web.Routing;

namespace SparePartsWebApi
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.IgnoreRoute("Uploads/{*pathInfo}");

            routes.IgnoreRoute("{Uploads}", new { Uploads = @".*\.(jpeg|gif|jpg)(/.)?" });

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
                );
        }
    }
}
