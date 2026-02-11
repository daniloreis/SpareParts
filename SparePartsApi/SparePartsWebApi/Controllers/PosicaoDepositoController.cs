using Application;
using Domain;
using Infrastructure;
using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Http;

namespace SparePartsWebApi.Controllers
{
    [RoutePrefix("PosicaoDeposito")]
    public class PosicaoDepositoController : ApiController
    {
        private PosicaoDepositoService service;

        private SapIntegrationFacade sapService;
        public PosicaoDepositoController(PosicaoDepositoService service)
        {
            if (sapService == null)
                sapService = new SapIntegrationFacade();

            this.service = service;
        }


    }
}