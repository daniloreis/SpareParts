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
    public class MaterialController : ApiController
    {
        private SapIntegrationFacade sapService;
        private ArmazenagemService armazenagemService;
        private RecebimentoService recebimentoService;

        public MaterialController(ArmazenagemService materialarmazenadoService, RecebimentoService recebimentoService)
        {
            if (sapService == null)
                sapService = new SapIntegrationFacade();

            this.armazenagemService = materialarmazenadoService;
            this.recebimentoService = recebimentoService;
        }

        [HttpGet]
        [Route("Detalhe/{codMaterial}")]
        public async Task<IHttpActionResult> Detalhe(string codMaterial)
        {
            try
            {
                var material = await Task.Run(() => sapService.BuscarMaterialDetalhe(codMaterial));

                return Json(material);
            }
            catch (Exception ex)
            {
                return Json(ex.GetInnerExceptionMessage());
            }
        }


        [HttpGet]
        [Route("ConsultarBC/{ano}/{documento}/{item}")]
        public async Task<IHttpActionResult> ConsultarBC(int ano, string documento, string item)
        {
            try
            {
                var material = await Task.Run(() => recebimentoService.ConsultarBC(ano, documento, item));

                return Json(material);
            }
            catch (Exception ex)
            {
                return Json(ex.GetInnerExceptionMessage());
            }
        }


    }

}