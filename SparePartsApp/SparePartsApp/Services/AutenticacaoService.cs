using Newtonsoft.Json;
using SparePartsApp.Models;
using System;
using System.IO;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SparePartsApp.Services
{
    public class AutenticacaoService : BaseRestService<Usuario>
    {
        public override string Controller { get => "Autenticacao"; }

        public async Task<String> Autenticar(Usuario usuario, string action = "Autenticar")
        {
            try
            {
                ShowLoading(); 

                using (var httpClient = new HttpClient() { Timeout = TimeSpan.FromMinutes(5) })
                {
                    var body = JsonConvert.SerializeObject(usuario);
                    var stringContent = new StringContent(body, Encoding.UTF8, "application/json");

                    var requestUri = Path.Combine(BaseUrl, action).Replace("\\", "/");

                    var response = await httpClient.PostAsync(requestUri, stringContent);
                    var message = string.Empty;

                    using (HttpContent content = response.Content)
                    {
                        var result = await content.ReadAsStringAsync();
                        message = result;
                    }

                    return message.Trim('"', '\\').Replace("\\r", "\r").Replace("\\n", "\n");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            } 
            finally
            {
                HideLoading();
            }
        }
    }
}
