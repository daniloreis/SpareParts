using Application;
using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace SPW.Controllers
{
    public class RecebimentoController : Controller
    {
        private RecebimentoService recebimentoService;

        public RecebimentoController(RecebimentoService recebimentoService)
        {
            this.recebimentoService = recebimentoService;
        }

        public ActionResult Index(string codLista)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(codLista))
                {
                    var lista = recebimentoService.BuscarListaRecebimento(codLista);

                    foreach (var item in lista.Materiais)
                    {
                        item.PosicaoDeposito = "RECEBIMENTO";

                        if (item.TemLote && !string.IsNullOrWhiteSpace(item.UnidadeTempo))
                        {
                            item.Lotes.Add(new Lote
                            {
                                CodMaterial = item.CodMaterial,
                                NumeroLote = "lote " + item.CodMaterial,
                                Quantidade = item.Quantidade,
                                Validade = DateTime.Now.AddYears(1),
                                Deposito = "DM15"
                            });
                        }
                    }

                    return View("Create", lista);
                }
                return View("Create", new ListaRecebimento());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult Post(string codLista, ListaRecebimento listaRec)
        {
            try
            {
                var lista = recebimentoService.BuscarListaRecebimento(codLista);

                for (int i = 0; i < lista.Materiais.Count(); i++)
                {
                    var item = lista.Materiais[i];

                    item.PosicaoDeposito = "RECEBIMENT";
                    item.quantidadeEstoqueFisico = item.Quantidade;

                    if (item.TemLote && !string.IsNullOrWhiteSpace(item.UnidadeTempo))
                    {
                        item.NumeroLote = listaRec.Materiais[i].NumeroLote;
                        item.Validade = listaRec.Materiais[i].Validade;
                    }
                }

                var retorno = recebimentoService.CriarMigo(lista);

                ViewBag.Message = retorno;

                return View("Create", lista);
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }
            return View("Create", new ListaRecebimento());
        }

    }
}