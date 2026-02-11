using Infrastructure;
using System;
using System.Configuration;
using System.DirectoryServices;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;

public class HomeController : Controller
{
    public ActionResult Index()
    {
        return View();
    }

    public ActionResult Logout()
    {
        HttpContext.Session.Clear();
        HttpContext.Session.Abandon();

        FormsAuthentication.SignOut();

        return RedirectToAction("Index");
    }

    [HttpPost]
    [AllowAnonymous]
    public ActionResult Login(string username, string password)
    {
        try
        {
            FormsAuthentication.SignOut();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Informe o usuário e senha!");
            }

            var resourceUri = ConfigurationManager.ConnectionStrings["LDAP"].ToString();

            var dirEntry = new DirectoryEntry(resourceUri, username, password);
            object nativeObject = dirEntry.NativeObject;

            //FormsAuthentication.RedirectFromLoginPage(username, true);

            //FormsAuthentication.SetAuthCookie(username, true);

            //var teste = FormsAuthentication.Authenticate(username, password);

            RedirectToAction("List", "Listarecebimento");
        }
        catch (DirectoryServicesCOMException ex)
        {
            ModelState.AddModelError("", ex.GetInnerExceptionMessage());
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.GetInnerExceptionMessage());
        }
        return View("Index");
    }

}
