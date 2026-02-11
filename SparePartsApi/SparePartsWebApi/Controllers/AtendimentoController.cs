using Application;
using Domain;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace SparePartsWebApi.Controllers
{
    [RoutePrefix("Atendimento")]
    public class AtendimentoController : ApiController
    {
        private AtendimentoService service;

        public AtendimentoController(AtendimentoService atendimentoService)
        {
            service = atendimentoService;
        }

        [HttpGet]
        [Route("BuscarReserva/{numReserva}")]
        public async Task<IHttpActionResult> BuscarReserva(string numReserva)
        {
            try
            {
                var materiais = await Task.Run(() => service.BuscarReserva(numReserva));

                return Json(materiais);
            }
            catch (Exception ex)
            {
                return Json(ex.GetInnerExceptionMessage());
            }
        }

        [HttpGet]
        [Route("BuscarTransferencia")]
        public async Task<IHttpActionResult> BuscarTransferencia()
        {
            try
            {
                var materiais = await Task.Run(() => service.BuscarTransferencia());

                return Json(materiais);
            }
            catch (Exception ex)
            {
                return Json(ex.GetInnerExceptionMessage());
            }
        }

        [HttpPost]
        [Route("Atender")]
        public async Task<IHttpActionResult> Atender(IEnumerable<MaterialAtendimento> materiais)
        {
            try
            {
                var retorno = await Task.Run(() => service.Atender(materiais));

                return Json(retorno);
            }
            catch (Exception ex)
            {
                return Json(ex.GetInnerExceptionMessage());
            }
        }


        [HttpPost]
        [Route("AtenderCompraDireta")]
        public async Task<IHttpActionResult> AtenderCompraDireta(IEnumerable<MaterialAtendimento> materiais)
        {
            try
            {
                var retorno = await Task.Run(() => service.AtenderCompraDireta(materiais));

                return Json(retorno);
            }
            catch (Exception ex)
            {
                return Json(ex.GetInnerExceptionMessage());
            }
        }

        [HttpGet]
        [Route("IniciarVisita")]
        public async Task<IHttpActionResult> IniciarVisita()
        {
            try
            {
                var retorno = await Task.Run(() => DateTime.Now);

                return Json(retorno);
            }
            catch (Exception ex)
            {
                return Json(ex.GetInnerExceptionMessage());
            }
        }


        [HttpPost]
        [Route("AtenderVisitante")]
        public async Task<IHttpActionResult> AtenderVisitante(IEnumerable<MaterialAcompanhamento> materiais)
        {
            try
            {
                foreach (var material in materiais)
                {
                    material.DataHoraFim = DateTime.Now;
                }

                var retorno = await Task.Run(() => service.AtenderVisitante(materiais));

                return Json(retorno);
            }
            catch (Exception ex)
            {
                return Json(ex.GetInnerExceptionMessage());
            }
        }

        [HttpPost]
        [Route("SalvarDivergentes")]
        public async Task<IHttpActionResult> SalvarDivergentes(IEnumerable<MaterialAtendimento> materiais)
        {
            try
            {
                await Task.Run(() => service.SalvarDivergentes(materiais));

                return Ok();
            }
            catch (Exception ex)
            {
                return Json(ex.GetInnerExceptionMessage());
            }
        }

        [HttpGet]
        [Route("AtenderPendentes")]
        public void AtenderPendentes()
        {
            try
            {
                service.AtenderPendentes();
            }
            catch (Exception)
            {

            }
        }

        [Route("EnviarAssinatura")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> EnviarAssinatura()
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
    }
}
