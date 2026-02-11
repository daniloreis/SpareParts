using Application;
using Domain;
using Infrastructure;
using System;
using System.Configuration;
using System.Web.Mvc;

public class MaterialController : Controller
{
    private RecebimentoService service;
    private SapIntegrationFacade sapService;

    public MaterialController(RecebimentoService service)
    {
        this.service = service;

        if (sapService == null)
            sapService = new SapIntegrationFacade();
    }
     
    public ActionResult Search(string codLista)
    {
        try
        {
            var models = service.BuscarPorCodLista(codLista);

            return View("Materiais", models);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
     
}



