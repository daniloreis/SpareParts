using System;
using System.Web.Mvc;
using Application;
using System.Linq;
using SPW.Models;
using Domain;
using System.Collections.Generic;

public class AtendimentoController : Controller
{
    private AtendimentoService service;

    public AtendimentoController(AtendimentoService service)
    {
        this.service = service;
    }

    public ActionResult List()
    {
        try
        {
            var materiais = service.FindAll().OrderByDescending(x => x.DataHoraFim).Take(10).ToList();

            return View(new AtendimentoViewModel { Materiais = materiais });
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
            model.Materiais = service.Filtrar(model.Filtro).ToList();

            return View("List", model);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

}



