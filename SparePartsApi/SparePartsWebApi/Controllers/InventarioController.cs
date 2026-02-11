using Application;
using Domain;
using Infrastructure;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace SparePartsWebApi.Controllers
{
    [RoutePrefix("Inventario")]
    public class InventarioController : ApiController
    {
        private SapIntegrationFacade sapService;
        private InventarioService MaterialinventariadoService;
        public InventarioController(InventarioService MaterialinventariadoService)
        {
            if (sapService == null)
                sapService = new SapIntegrationFacade();

            this.MaterialinventariadoService = MaterialinventariadoService;
        }

        [HttpGet]
        [Route("BuscarInventario/{numero}")]
        public async Task<IHttpActionResult> BuscarInventario(string numero)
        {
            try
            {
                string ano = DateTime.Now.Year.ToString();

                var materiais = await Task.Run(() => sapService.BuscarInventario(numero, ano));

                var inventario = new Inventario() { NumeroDocumento = numero, AnoFiscal = ano, Materiais = materiais };

                return Json(inventario);
            }
            catch (Exception ex)
            {
                return Json(ex.GetInnerExceptionMessage());
            }
        }

        [HttpPost]
        [Route("Inventariar")]
        public async Task<IHttpActionResult> Inventariar(Inventario inventario)
        {
            try
            {
                var materiaisContados = inventario.Materiais.Where(x => x.Quantidade.HasValue).ToList();

                var retorno = await Task.Run(() => sapService.Inventariar(inventario.NumeroDocumento, inventario.AnoFiscal, materiaisContados));

                if (retorno.ToString().StartsWith("OK:"))
                {
                    materiaisContados.ToList().ForEach(x =>
                    {
                        x.NumeroDocumento = inventario.NumeroDocumento;
                        x.AnoFiscal = Convert.ToInt32(inventario.AnoFiscal);
                        x.Usuario = inventario.Usuario;
                        x.DataHoraFim = DateTime.Now;
                    });

                    MaterialinventariadoService.AddRange(materiaisContados);
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
