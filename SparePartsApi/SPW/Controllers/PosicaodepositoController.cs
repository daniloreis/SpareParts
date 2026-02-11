using Application;
using Domain;
using Infrastructure;
using SPW.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SPW.Controllers
{
    public class PosicaodepositoController : Controller
    {
        private PosicaoDepositoService service;
        private ImpressaoService impressaoService;

        public PosicaodepositoController(PosicaoDepositoService service, ImpressaoService impressaoService)
        {
            this.service = service;
            this.impressaoService = impressaoService;
        }

        public ActionResult List()
        {
            try
            {
                var model = new PosicaoDepositoViewModel();

                var models = service.FindAll();

                model.Items = models.ToList().OrderBy(x => x.Localizacao).Take(10).ToList();

                return View(model);
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
                var model = new PosicaoDeposito();

                if (id.HasValue)
                {
                    model = service.FindById(id.Value);
                }

                return View(model);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(PosicaoDeposito model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    service.Save(model);

                    return RedirectToAction("List");
                }

                return View("Crud", model);
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

        //public ActionResult Import()
        //{
        //    HttpPostedFileBase arquivo = Request.Files[0];

        //    var retorno = new DataSet();
        //    var items = new List<PosicaoDeposito>();

        //    try
        //    {
        //        if (arquivo != null && Path.GetExtension(arquivo.FileName).Equals(".xlsx"))
        //        {
        //            using (var stream = arquivo.InputStream)
        //            {
        //                using (var excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream))
        //                {
        //                    excelReader.IsFirstRowAsColumnNames = true;
        //                    retorno = excelReader.AsDataSet();

        //                    var tabela = retorno.Tables[0];

        //                    foreach (DataRow row in tabela.Rows)
        //                    {
        //                        var item = new PosicaoDeposito();

        //                        item.Deposito = Convert.ToString(row["Depósito"] ?? "");
        //                        item.Localizacao = Convert.ToString(row["Endereço"] ?? "");
        //                        item.Area = Convert.ToString(row["Área"] ?? "");

        //                        items.Add(item);
        //                    }
        //                }
        //            }//end using

        //            service.Importar(items);
        //        }//end if
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }

        //    return RedirectToAction("List");
        //}

        [HttpPost]
        public ActionResult Search(PosicaoDepositoViewModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Deposito) && string.IsNullOrWhiteSpace(model.Area))
                    model.Items = service.FindAll().OrderBy(x => x.Localizacao).Take(10).ToList();
                else
                    model.Items = service.BuscarPor(model.Deposito, model.Area).OrderBy(x => x.Localizacao).ToList();

                return View("List", model);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult Print(PosicaoDepositoViewModel model)
        {
            try
            {
                var dir = System.Web.HttpContext.Current.Server.MapPath("~/Labels/");
                string ZPLString = System.IO.File.ReadAllText(Path.Combine(dir, "label_endereco.prn"));

                var etiquetas = new List<string>();

                var items = new List<PosicaoDeposito>();

                if (!string.IsNullOrWhiteSpace(model.Localizacao))
                    items.Add(new PosicaoDeposito() { Localizacao = model.Localizacao });
                else
                    items = service.BuscarPor(model.Deposito, model.Area).OrderBy(x => x.Localizacao).ToList();

                foreach (var item in items.OrderBy(x => x.Localizacao))
                {
                    var temp = ZPLString.Replace("%barcode%", item.Localizacao);
                    temp = temp.Replace("@datahora", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

                    etiquetas.Add(temp);
                };

                impressaoService.Imprimir(etiquetas);
            }
            catch (Exception ex)
            {
                return View(ex.GetInnerExceptionMessage());
            }

            return null;
        }
    }
}



