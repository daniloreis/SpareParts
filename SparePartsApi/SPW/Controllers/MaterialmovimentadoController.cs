using Domain;
using System;
using System.Web.Mvc;
using Application;
using System.Collections.Generic;
using System.Linq;
using SPW.Models;

public class MaterialmovimentadoController : Controller
{
    private MaterialmovimentadoService service;

    public MaterialmovimentadoController(MaterialmovimentadoService service)
    {
        this.service = service;
    }

    public ActionResult List()
    {
        try
        {
            var models = service.FindAll().OrderByDescending(x => x.DataHoraFim).Take(10).ToList();

            return View(new MovimentacaoViewModel { Materiais = models });
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }



    [HttpPost]
    public ActionResult Search(MovimentacaoViewModel model)
    {
        try
        {
            model.Materiais = service.Filtrar(model.Filtro).Take(10).OrderByDescending(x => x.DataHoraFim).ToList();

            return View("List", model);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}



