using Newtonsoft.Json;
using SparePartsApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SparePartsApp.Services
{
    public class RecebimentoService : BaseRestService<Recebimento>
    {
        public virtual async Task<WebApiResult<List<Material>>> CriarMigo(Recebimento lista)
        {
            var resultado = new WebApiResult<List<Material>>();
            try
            {
                foreach (var material in lista.Materiais)
                {
                    if (!string.IsNullOrWhiteSpace(material.Foto))
                        FileUpload("EnviarFoto", material.Foto);
                }

                try
                {
                    ShowLoading();

                    var handler = new Xamarin.Android.Net.AndroidClientHandler();
                    handler.ConnectTimeout = TimeSpan.FromMinutes(10);
                    handler.ReadTimeout = TimeSpan.FromMinutes(10);

                    using (var httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(10) })
                    {
                        var body = JsonConvert.SerializeObject(lista);
                        var stringContent = new StringContent(body, Encoding.UTF8, "application/json");

                        var requestUri = Path.Combine(BaseUrl, "CriarMigo").Replace("\\", "/");

                        var response = await httpClient.PostAsync(requestUri, stringContent);
                        var message = string.Empty;

                        using (HttpContent content = response.Content)
                        {
                            var result = await content.ReadAsStringAsync();
                            resultado = JsonConvert.DeserializeObject<WebApiResult<List<Material>>>(result);
                        }
                    }
                }
                catch (Exception ex)
                {
                    resultado.Message = ex.GetInnerExceptionMessage();
                    resultado.Success = false;
                }
                finally
                {
                    HideLoading();
                }
                return resultado;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual async Task<string> ImprimirMaterial(Recebimento lista)
        {
            try
            {
                return await base.Post(lista, "ImprimirMaterial");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual async Task<string> ImprimirVolumes(Recebimento lista)
        {
            try
            {
                return await base.Post(lista, "ImprimirVolumes");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
