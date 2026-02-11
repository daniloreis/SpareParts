using Domain;
using System;
using System.Web.Mvc;
using Application;

public class MaterialtransferenciaController : Controller
{
    private TransferenciaService service;

    public MaterialtransferenciaController(TransferenciaService service)
    {
        this.service = service;
    }

    public ActionResult List()
    {
        try
        {
            var models = service.FindAll();

            return View(models);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public ActionResult Crud(int? id)
    {
        try
        {
            var model = new MaterialAtendimentoTransferencia();

            if (id.HasValue)
            {
                model = service.FindById(id.Value);
            }

            return PartialView(model);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Save(MaterialAtendimentoTransferencia model)
    {
        try
        {
            if (ModelState.IsValid)
            {
                    service.Save(model);
            }

            return RedirectToAction("List");

        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public ActionResult Delete(int id)
    {
        try
        {
            var model = service.FindById(id);

            if (model != null)
                service.Remove(model);

            return RedirectToAction("List");
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}



