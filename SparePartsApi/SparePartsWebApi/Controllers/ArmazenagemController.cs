using Application;
using Domain;
using Infrastructure;
using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Http;

namespace SparePartsWebApi.Controllers
{
    [RoutePrefix("Material")]
    public class ArmazenagemController : ApiController
    {
        private SapIntegrationFacade sapService;
        private ArmazenagemService service;
        private EmailService emailService;

        public ArmazenagemController(ArmazenagemService materialarmazenadoService, EmailService emailService)
        {
            if (sapService == null)
                sapService = new SapIntegrationFacade();

            this.service = materialarmazenadoService;
            this.emailService = emailService;
        }

        [HttpGet]
        [Route("Pendente/{ano}/{documento}/{item}")]
        public async Task<IHttpActionResult> Pendente(int ano, string documento, string item)
        {
            try
            {
                var material = await Task.Run(() => service.BuscarMaterialPendenteArmazenagem(ano, documento, item));

                if (material == null)
                    return NotFound();

                return Json(material);
            }
            catch (Exception ex)
            {
                return Json(ex.GetInnerExceptionMessage());
            }
        }

        [HttpPost]
        [Route("Armazenar")]
        public async Task<IHttpActionResult> Armazenar([FromBody] MaterialArmazenagem material)
        {
            try
            {
                var retorno = await Task.Run(() => sapService.Armazenar(material));

                if (retorno.Contains("OK:"))
                {
                    var materialarmazenado = new MaterialArmazenagem()
                    {
                        CodMaterial = material.CodMaterial,
                        Nome = material.Nome,
                        PosicaoDeposito = material.PosicaoDeposito,
                        Quantidade = material.Quantidade,
                        QuantidadeEstoqueFisico = material.QuantidadeEstoqueFisico, //Convert.ToDouble(material.QuantidadeEstoqueFisico, new CultureInfo("pt-BR")),
                        UnidadeMedida = material.UnidadeMedida,
                        QuantidadeEstoqueSAP = material.QuantidadeEstoqueSAP,
                        Usuario = material.Usuario,
                        DataHoraInicio = material.DataHoraInicio,
                        Deposito = material.Deposito,
                        DataHoraFim = DateTime.Now,
                        RetornoSap = retorno
                    };

                    service.Save(materialarmazenado);

                    if (material.FispqPendente)
                    {
                        string emailFispqPendente = ConfigurationManager.AppSettings["EmailFispqPendente"].ToString();

                        emailService.EnviarEmailParaFila(emailFispqPendente, "Fispq Pendente", $@"Material {materialarmazenado.CodMaterial} está pendente de FISPQ. *FAVOR ATUALIZAR PASTA*", "");
                    }

                }

                return Json(retorno);
            }
            catch (Exception ex)
            {
                return Json(ex.GetInnerExceptionMessage());
            }
        }
    }
}