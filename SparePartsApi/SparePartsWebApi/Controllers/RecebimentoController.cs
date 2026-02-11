using Application;
using Domain;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace SparePartsWebApi.Controllers
{
    [RoutePrefix("Recebimento")]
    public class RecebimentoController : ApiController
    {
        private RecebimentoService recebimentoService;
        private ImpressaoService impressaoService;

        public RecebimentoController(RecebimentoService recebimentoService, ImpressaoService impressaoService)
        {
            this.recebimentoService = recebimentoService;
            this.impressaoService = impressaoService;
        }

        [HttpGet]
        public async Task<IHttpActionResult> Get(string id)
        {
            try
            {
                var obj = await Task.Run(() => recebimentoService.BuscarListaRecebimento(id));

                return Json(obj);
            }
            catch (Exception ex)
            {
                return Json(ex.GetInnerExceptionMessage());
            }
        }

        [HttpPost]
        [Route("CriarMigo")]
        public async Task<IHttpActionResult> CriarMigo([FromBody] ListaRecebimento lista)
        {
            try
            {
                if (lista == null || !lista.Materiais.Any())
                    return Json("Lista de materiais vazia!");

                var retorno = await Task.Run(() => recebimentoService.CriarMigoInbound(lista));

                return Json(retorno);
            }
            catch (Exception ex)
            {
                return Json(ex.GetInnerExceptionMessage());
            }
        }

        [HttpPost]
        [Route("EnviarFoto")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> EnviarFoto()
        {
            var message = string.Empty;
            try
            {
                //await Task.Run(() =>
                // {
                var httpRequest = HttpContext.Current.Request;

                foreach (string file in httpRequest.Files)
                {
                    var postedFile = httpRequest.Files[file];

                    if (postedFile != null && postedFile.ContentLength > 0)
                    {
                        int maxContentLength = 1024 * 1024 * 10; //Size = 10 MB  

                        var extension = Path.GetExtension(postedFile.FileName);

                        if (!extension.Equals(".jpg") && !extension.Equals(".jpeg"))
                        {
                            message = "Favor enviar somente o formato .jpg";
                        }
                        else if (postedFile.ContentLength > maxContentLength)
                        {
                            message = "O limite para foto é 10 mb.";
                        }
                        else
                        {
                            var dir = HttpContext.Current.Server.MapPath("~/Uploads/");

                            if (!Directory.Exists(dir))
                                Directory.CreateDirectory(dir);

                            var filePath = dir + Path.GetFileName(postedFile.FileName);

                            postedFile.SaveAs(filePath);
                        }
                    }
                }
                //});
            }
            catch (Exception ex)
            {
                message = ex.GetInnerExceptionMessage();
            }

            return Json(message);
        }


        [HttpPost]
        [Route("ImprimirMaterial")]
        public async Task<IHttpActionResult> ImprimirMaterial(ListaRecebimento lista)
        {
            try
            {
                var dir = HttpContext.Current.Server.MapPath("~/Labels/");
                string ZPLString = File.ReadAllText(Path.Combine(dir, "label_material.prn"));

                var etiquetas = new List<string>();

                foreach (var material in lista.Materiais)
                {
                    if (string.IsNullOrWhiteSpace(material.NumeroDocumento))
                        material.NumeroDocumento = lista.NumeroDocumento;

                    for (var qtdImprimir = material.QuantidadeImpressao; qtdImprimir > 0; qtdImprimir--)
                    {
                        var etiqueta = ZPLString.Replace("@barcode", material.CodMaterial.TrimStart('0'));
                        etiqueta = etiqueta.Replace("@nome", material.Nome ?? "");
                        etiqueta = etiqueta.Replace("@entrada", material.DepositoEtiqueta ?? "");
                        etiqueta = etiqueta.Replace("@destino", material.DepositoDestinoEtiqueta ?? "");
                        etiqueta = etiqueta.Replace("@lote", material.NumeroLote.HasValue() ? "Lote:" + material.NumeroLote : "");
                        etiqueta = etiqueta.Replace("@qrcode", $@"{material.Ano ?? DateTime.Now.Year}/{material.NumeroDocumento}/{material.NumeroItem?.PadLeft(4, '0')}");
                        etiqueta = etiqueta.Replace("@datahora", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

                        etiquetas.Add(etiqueta);
                    }
                };

                impressaoService.Imprimir(etiquetas);

                return Ok();
            }
            catch (Exception ex)
            {
                return Json(ex.GetInnerExceptionMessage());
            }
        }

        [HttpPost]
        [Route("ImprimirVolumes")]
        public async Task<IHttpActionResult> ImprimirVolumes(ListaRecebimento lista)
        {
            try
            {
                var dir = HttpContext.Current.Server.MapPath("~/Labels/");
                string ZPLString = File.ReadAllText(Path.Combine(dir, "label_volumes.prn"));

                var etiquetas = new List<string>();

                for (var qtdImprimir = lista.NumeroVolumes; qtdImprimir > 0; qtdImprimir--)
                {
                    var etiqueta = ZPLString
                        .Replace("@barcode", lista.CodLista)
                        .Replace("@nfe", lista.NumeroNfe)
                        .Replace("@fornecedor", lista.Fornecedor)
                        .Replace("@descarga", lista.PosicaoDeposito)
                        .Replace("@datahora", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

                    etiquetas.Add(etiqueta);
                }

                impressaoService.Imprimir(etiquetas);

                return Ok();

                return BadRequest();
            }
            catch (Exception ex)
            {
                return Json(ex.GetInnerExceptionMessage());
            }
        }

    }
}