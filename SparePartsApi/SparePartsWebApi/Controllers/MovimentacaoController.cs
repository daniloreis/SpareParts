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
    public class MovimentacaoController : ApiController
    {
        private SapIntegrationFacade sapService;
        private MaterialmovimentadoService materialmovimentadoservice;

        public MovimentacaoController(MaterialmovimentadoService materialmovimentadoservice)
        {
            if (sapService == null)
                sapService = new SapIntegrationFacade();

            this.materialmovimentadoservice = materialmovimentadoservice;
        }

        [HttpGet]
        [Route("Armazenados/{posicaoDeposito}")]
        public async Task<IHttpActionResult> Armazenados(string posicaoDeposito)
        {
            try
            {
                var materiais = await Task.Run(() => sapService.BuscarMateriaisArmazenados(posicaoDeposito));

                return Json(materiais);
            }
            catch (Exception ex)
            {
                return Json(ex.GetInnerExceptionMessage());
            }
        }

        [HttpPost]
        [Route("Movimentar")]
        public async Task<IHttpActionResult> Movimentar([FromBody] MaterialMovimentacao material)
        {
            try
            {
                var retorno = await Task.Run(() => sapService.Movimentar(material));

                var materialmovimentado = new MaterialMovimentacao()
                {
                    CodMaterial = material.CodMaterial,
                    Nome = material.Nome,
                    PosicaoDeposito = material.PosicaoDeposito,
                    Deposito = material.Deposito,
                    Quantidade = material.Quantidade,
                    UnidadeMedida = material.UnidadeMedida,
                    Usuario = material.Usuario,
                    DataHoraInicio = material.DataHoraInicio,
                    DataHoraFim = DateTime.Now,
                    RetornoSap = retorno
                };

                materialmovimentadoservice.Save(materialmovimentado);

                return Json(retorno);
            }
            catch (Exception ex)
            {
                return Json(ex.GetInnerExceptionMessage());
            }
        }
    }
}