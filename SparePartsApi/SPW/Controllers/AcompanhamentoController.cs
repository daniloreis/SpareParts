using Application;
using SPW.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SPW.Controllers
{
    public class AcompanhamentoController : Controller
    {
        private AtendimentoService service;

        public AcompanhamentoController(AtendimentoService service)
        {
            this.service = service;
        }

        public ActionResult List()
        {
            try
            {
                var materiais = service.BuscarAcompanhamentos().ToList();

                return View(new AtendimentoViewModel { Acompanhamentos = materiais });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult Search(AtendimentoViewModel model)
        {
            try
            {
                model.Acompanhamentos = service.FiltrarAcompanhamento(model.Filtro).ToList();

                return View("List", model);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}