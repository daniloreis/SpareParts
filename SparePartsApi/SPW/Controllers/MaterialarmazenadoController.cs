using Domain;
using System;
using System.Web.Mvc;
using Application;
using Infrastructure;
using System.Linq;
using System.Collections.Generic;
using SPW.Models;

public class MaterialarmazenadoController : Controller
{
    private ArmazenagemService service;

    public MaterialarmazenadoController(ArmazenagemService service)
    {
        this.service = service;
    }

    public ActionResult List()
    {
        try
        {
            var models = service.FindAll().OrderByDescending(x => x.DataHoraFim).Take(10).ToList();

            return View(new ArmazenagemViewModel { Materiais = models });
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }


    [HttpPost]
    public ActionResult Search(ArmazenagemViewModel model)
    {
        try
        {
            model.Materiais = service.Filtrar(model.Filtro).OrderByDescending(x => x.DataHoraFim).ToList();

            return View("List", model);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

}



