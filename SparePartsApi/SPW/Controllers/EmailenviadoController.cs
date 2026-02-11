
using Domain;
using System;
using System.Web.Mvc;
using Application;
using System.Configuration;
using SPW.Models;
using System.Collections.Generic;
using Infrastructure;

public class EmailenviadoController : Controller
{
    private EmailService service;

    public EmailenviadoController(EmailService service)
    {
        this.service = service;
    }

    public ActionResult List()
    {
        try
        {
            var emails = service.FindAll();

            return View(new EmailEnviadoViewModel { Emails = emails });
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public ActionResult Test()
    {
        try
        {
            string emails = ConfigurationManager.AppSettings["Emails"].ToString();

            service.EnviarEmail();
        }
        catch (Exception ex)
        {
            return View("List", new EmailEnviadoViewModel() { Mensagem = ex.GetInnerExceptionMessage() });
        }

        return View("List", new EmailEnviadoViewModel());
    }
}



