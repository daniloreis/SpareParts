using Application;
using Domain;
using Domain.Entities;
using Infrastructure;
using System;
using System.Configuration;
using System.DirectoryServices;
using System.Threading.Tasks;
using System.Web.Http;

namespace SparePartsWebApi.Controllers
{
    [RoutePrefix("autenticacao")]
    public class AutenticacaoController : ApiController
    {
        [HttpPost]
        [Route("autenticar")]
        public async Task<IHttpActionResult> Autenticar(Usuario usuario)
        {
            try
            {
                if (usuario == null || string.IsNullOrWhiteSpace(usuario.Nome) || string.IsNullOrWhiteSpace(usuario.Senha))
                    return Json("Informe o usuário e a senha!");

                var resourceUri = ConfigurationManager.ConnectionStrings["LDAP"].ToString();

                var dirEntry = await Task.Run(() => new DirectoryEntry(resourceUri, usuario.Nome, usuario.Senha));

                object nativeObject = dirEntry.NativeObject;

                return Json($"OK:{dirEntry.Username}");
            }
            catch (DirectoryServicesCOMException ex)
            {
                return Json("Usuário ou senha incorretos!");
            }
            catch (Exception ex)
            {
                return Json(ex.GetInnerExceptionMessage());
            }
        }
    }
}
