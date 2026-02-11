using Application;
using Domain;
using SPW.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

public class MaterialinventariadoController : Controller
{
    private InventarioService service;

    public MaterialinventariadoController(InventarioService service)
    {
        this.service = service;
    }

    public ActionResult List()
    {
        try
        {
            var materiais = service.FindAll().OrderByDescending(x => x.DataHoraFim).Take(10).ToList();

            return View(new InventarioViewModel { Materiais = materiais });
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }


    [HttpPost]
    public ActionResult Search(InventarioViewModel model)
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



