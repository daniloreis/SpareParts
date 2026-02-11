using Application;
using Domain;
using SPW.Models;
using System;
using System.Linq;
using System.Web.Mvc;

public class ListarecebimentoController : Controller
{
    private RecebimentoService recebimentoService;

    public ListarecebimentoController(RecebimentoService recebimentoService)
    {
        this.recebimentoService = recebimentoService;
    }

    public ActionResult List()
    {
        try
        {
            var materiais = recebimentoService.FindAll().OrderByDescending(x => x.DataHoraFim).Take(10).ToList();

            return View("List", new ListaRecebimentoViewModel() { Materiais = materiais });
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }


    [HttpPost]
    public ActionResult Search(ListaRecebimentoViewModel model)
    {
        try
        {
            //model.Filtro.TemDivergencia = model.TemDivergencia;

            model.Materiais = recebimentoService.Filtrar(model.Filtro).ToList().OrderByDescending(x => x.DataHoraFim);

            return View("List", model);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }


}



