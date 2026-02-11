using Domain;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Application
{
    public class RecebimentoService : ServiceBase<MaterialRecebimento>
    {
        private RecebimentoRepository repository;
        private ISapIntegrationFacade sapFacade;
        private EmailService emailService;

        public RecebimentoService(RecebimentoRepository repository, EmailService emailService, ISapIntegrationFacade sapIntegrationFacade) : base(repository)
        {
            this.repository = repository;
            this.emailService = emailService;

            this.sapFacade = sapIntegrationFacade;
        }

        public ListaRecebimento BuscarListaRecebimento(string id)
        {
            var recebimento = sapFacade.BuscarListaRecebimento(id);

            recebimento.Materiais = recebimento.Materiais.GroupBy(x => x.CodMaterial).Select(x =>
            {
                var ret = x.First();

                foreach (var item in x.ToList())
                {
                    ret.Repetidos.Add(new MaterialRepetido
                    {
                        NumeroPedido = item.NumeroPedido,
                        NumeroPedidoItem = item.NumeroPedidoItem,
                        QuantidadePO = item.QuantidadePO,
                        Quantidade = item.Quantidade.Value,
                        DepositoDestino = item.DepositoDestino,
                        DataCriacaoRC = item.DataCriacaoRC
                    });
                }

                ret.Quantidade = x.Sum(xt => xt.Quantidade);

                return ret;
            }).ToList(); //Agrupa materiais repetidos


            return recebimento;
        }

        /// <summary>
        /// Método da versão do inbound
        /// </summary>
        public WebApiResult CriarMigoInbound(ListaRecebimento lista)
        {
            var retorno = new WebApiResult();

            try
            {
                var emailSolicitante = string.Empty;
                var tableMateriais = new List<string>();

                var materiais = lista.Materiais.ToList();
                var materiaisRecebimento = new List<MaterialRecebimento>();

                foreach (var material in materiais)
                {
                    #region LOTES FALTANTES

                    if (material.Lotes.Any())
                    {
                        var totalLotes = material.Lotes.Sum(x => x.Quantidade);

                        if (totalLotes < material.Quantidade)
                        {
                            material.Lotes.Add(new Lote
                            {
                                CodMaterial = material.CodMaterial,
                                Deposito = material.Deposito,
                                NumeroLote = "FALTANTES",
                                Quantidade = material.Quantidade - totalLotes,
                                Validade = DateTime.Now.AddMonths(3)
                            });
                        }
                    };

                    #endregion

                    #region REPETIDOS

                    foreach (var item in material.Repetidos)
                    {
                        #region DIVERGÊNCIAS

                        if (material.TemDivergencia())
                        {
                            material.Deposito = Deposito.DIVERGENCIA;
                        }
                        else
                        {
                            material.Deposito = Deposito.ARMAZENAGEM;

                            if (material.EhCompraDireta)
                            {
                                material.Deposito = Deposito.COMPRADIRETA;

                                emailSolicitante = material.EmailSolicitante;

                                tableMateriais.Add($"<tr><td>{material.NumeroPedido}</td><td>{material.Quantidade.Value.ToFormatString() + " " + material.UnidadeMedida}</td><td>{material.CodMaterial}</td><td>{material.Nome}</td></tr>");
                            }
                        }

                        #endregion

                        material.NumeroPedido = item.NumeroPedido;
                        material.NumeroPedidoItem = item.NumeroPedidoItem;
                        material.QuantidadePO = item.QuantidadePO;
                        material.DepositoDestino = item.DepositoDestino;//verificar porque nao esta sobrescrevendo o deposito

                        if (material.Lotes.Any())
                        {
                            foreach (var lote in material.Lotes)
                            {
                                if (material.QuantidadePO > 0)
                                {
                                    if (material.QuantidadePO <= lote.Quantidade)
                                    {
                                        material.Quantidade = material.QuantidadePO;
                                        lote.Quantidade -= material.QuantidadePO;
                                        material.QuantidadePO = 0;
                                    }
                                    else
                                    {
                                        material.Quantidade = lote.Quantidade;

                                        if (lote.Quantidade > 0)
                                        {
                                            material.QuantidadePO -= lote.Quantidade.Value;
                                            lote.Quantidade = 0;
                                        }
                                    }

                                    if (material.Quantidade > 0)
                                    {
                                        material.Validade = lote.Validade;
                                        material.quantidadeEstoqueFisico = material.Quantidade;
                                        material.NumeroLote = lote.NumeroLote;

                                        var materialClonado = material.Clone() as MaterialRecebimento;

                                        materiaisRecebimento.Add(materialClonado);

                                    }//end if
                                }//end if
                            }//end foreach
                        }
                        else
                        {
                            //var materialClonado = material.Clone() as MaterialRecebimento;

                            materiaisRecebimento.Add(material);
                        }
                    }

                    #endregion
                }

                retorno = sapFacade.CriarMigo(materiaisRecebimento);

                var novaLista = retorno.Result as List<MaterialRecebimento>;

                if (retorno.Message.StartsWith("OK:"))
                {
                    foreach (var item in novaLista)
                    {
                        item.RetornoSap = retorno.Message;
                        item.DataHoraFim = DateTime.Now;
                        item.NumeroDocumento = retorno.Message.Split(':')[1];
                    }

                    if (retorno.Success)
                        AddRange(novaLista);

                    if (tableMateriais.Any())
                    {
                        string emails = ConfigurationManager.AppSettings["EmailsCompraDireta"].ToString();

                        if (string.IsNullOrWhiteSpace(emailSolicitante))
                        {
                            emailSolicitante = emails.Split(';')[0]; //se não tem email do solicitante, pega o primeiro email das cópias e assume como substituto
                            emails = emails.Replace(emailSolicitante + ";", ""); //remove o substitudo das cópias para não receber duplicado
                        }

                        var mensagem = RetornaTemplate().Replace("@tableMateriais", string.Join("", tableMateriais));

                        emailService.EnviarEmailParaFila(emailSolicitante, "Recebimento de material", mensagem, emails);
                    }
                }
            }
            catch (Exception ex)
            {
                retorno.Message = ex.GetInnerExceptionMessage();
                retorno.Success = false;
            }

            return retorno;
        }

        public WebApiResult CriarMigo(ListaRecebimento lista)
        {
            var retorno = new WebApiResult();

            try
            {
                string emailSolicitante = string.Empty;
                var tableMateriais = new List<string>();
                var tableMateriaisSpareParts = new List<string>();

                var materiais = lista.Materiais.ToList();

                var materiaisRecebimento = RetornaListaTratada(materiais, ref emailSolicitante, tableMateriais, tableMateriaisSpareParts);

                retorno = sapFacade.CriarMigo(materiaisRecebimento);

                if (retorno.Success)
                {
                    if (tableMateriais.Any())
                    {
                        string emails = ConfigurationManager.AppSettings["EmailsCompraDireta"].ToString();

                        if (string.IsNullOrWhiteSpace(emailSolicitante))
                        {
                            emailSolicitante = emails.Split(';')[0]; //se não tem email do solicitante, pega o primeiro email das cópias e assume como substituto
                            emails = emails.Replace(emailSolicitante + ";", ""); //remove o substitudo das cópias para não receber duplicado
                        }

                        var mensagem = RetornaTemplate().Replace("@tableMateriais", string.Join("", tableMateriais));

                        emailService.EnviarEmailParaFila(emailSolicitante, "Recebimento de material", mensagem, emails);
                    }

                    if (tableMateriaisSpareParts.Any())
                    {
                        string[] emails2 = ConfigurationManager.AppSettings["EmailsOrdem"].ToString().Split(';');

                        string destinatario = emails2[0];
                        string copia = emails2[1];

                        var mensagem2 = RetornaTemplate(true).Replace("@tableMateriais", string.Join("", tableMateriaisSpareParts));

                        emailService.EnviarEmailParaFila(destinatario, "Recebimento de material SPARE PARTS", mensagem2, copia);
                    }
                }
            }
            catch (Exception ex)
            {
                retorno.Message = ex.GetInnerExceptionMessage();
                retorno.Success = false;
            }

            return retorno;
        }

        public List<MaterialRecebimento> RetornaListaTratada(List<MaterialRecebimento> materiais, ref string emailSolicitante, List<string> tableMateriais, List<string> tableMateriaisSpareParts)
        {
            var materiaisRecebimento = new List<MaterialRecebimento>();

            foreach (var material in materiais)
            {
                if (string.IsNullOrWhiteSpace(material.DepositoDestino))
                    throw new Exception($"O material {material.CodMaterial} não possui depósito padrão. Solicite o cadastramento no SAP para efetuar o recebimento.");

                #region LOTES FALTANTES PARA DAR ENTRADA NA DIVERGÊNCIA

                if (material.Lotes.Any())
                {
                    var totalLotes = material.Lotes.Sum(x => x.Quantidade);

                    if (totalLotes < material.Quantidade)
                    {
                        material.Lotes.Add(new Lote
                        {
                            CodMaterial = material.CodMaterial,
                            Deposito = material.Deposito,
                            NumeroLote = "FALTANTES",
                            Quantidade = material.Quantidade - totalLotes,
                            Validade = DateTime.Now.AddMonths(3)
                        });
                    }
                };

                #endregion

                #region REPETIDOS

                foreach (var repetido in material.Repetidos)
                {
                    material.NumeroPedido = repetido.NumeroPedido;
                    material.NumeroPedidoItem = repetido.NumeroPedidoItem;
                    material.QuantidadePO = repetido.QuantidadePO;
                    material.Quantidade = repetido.Quantidade;
                    material.DepositoDestino = repetido.DepositoDestino;
                    material.DataCriacaoRC = repetido.DataCriacaoRC;

                    #region DIVERGÊNCIAS

                    string sparePartsOI = ConfigurationManager.AppSettings["SparePartsOI"].ToString();

                    if (material.TemDivergencia())
                    {
                        material.Deposito = Deposito.DIVERGENCIA;
                    }
                    else
                    {
                        material.Deposito = Deposito.ARMAZENAGEM;

                        if (material.EhCompraDireta)
                        {
                            material.Deposito = Deposito.COMPRADIRETA;

                            emailSolicitante = material.EmailSolicitante;

                            if (sparePartsOI.Trim().Equals((material.Ordem ?? "").Trim()))
                            {
                                material.DepositoDestino = Deposito.SPAREPARTS;
                                tableMateriaisSpareParts.Add($"<tr><td>{material.Ordem ?? ""}</td><td>{material.NumeroPedido}</td><td>{material.Quantidade.Value.ToFormatString() + " " + material.UnidadeMedida}</td><td>{material.CodMaterial}</td><td>{material.Nome}</td></tr>");
                            }
                            else
                                tableMateriais.Add($"<tr><td>{material.Ordem ?? ""}</td><td>{material.NumeroPedido}</td><td>{material.QuantidadePO.ToFormatString() + " " + material.UnidadeMedida}</td><td>{material.CodMaterial}</td><td>{material.Nome}</td></tr>");
                        }
                    }

                    #endregion

                    if (material.Lotes.Any())
                    {
                        foreach (var lote in material.Lotes)
                        {
                            var materialClonado = material.Clone() as MaterialRecebimento;

                            if (lote.Quantidade <= material.Quantidade)
                            {
                                materialClonado.Quantidade = lote.Quantidade;
                                materialClonado.NumeroLote = lote.NumeroLote;
                                materialClonado.Validade = lote.Validade;
                                lote.Quantidade = 0;
                            }
                            else
                            {
                                materialClonado.Quantidade = material.Quantidade;
                                materialClonado.NumeroLote = lote.NumeroLote;
                                materialClonado.Validade = lote.Validade;
                                lote.Quantidade = lote.Quantidade - material.Quantidade;
                            }

                            materiaisRecebimento.Add(materialClonado);

                        }//end foreach
                    }
                    else
                    {
                        var materialClonado = material.Clone() as MaterialRecebimento;

                        materiaisRecebimento.Add(materialClonado);
                    }

                }

                #endregion
            }

            return materiaisRecebimento;
        }

        private string RetornaTemplate(bool emailSpareParts = false)
        {
            //var arquivo = Path.Combine(Environment.CurrentDirectory, "..","TemplatesEmail", "CompraDireta.html");
            //return File.ReadAllText(arquivo);

            string texto1 = "Os materiais abaixo estão disponíveis para retirada no almoxarifado.";
            string texto2 = "Todo material de compra direta possui um prazo de 72h para ser retirado, após esse período, caso não tenha sido retirado,<br/>" +
                            " o material será disponibilizado no estoque com visualização para todos usuários.";

            if (emailSpareParts)
            {
                texto1 = "Os materiais abaixo são do projeto SPARE PARTS";
                texto2 = "Por favor lançar de imediato no estoque, os materiais do projeto SPARE PARTS não precisam aguardar 72 horas para serem integrados ao estoque.";
            }

            return $@" <br />{texto1}<br />
                    <table style='border-collapse:collapse;width:100%;text-align:center;' border='1'>
                        <thead><tr style='background-color:#d5edce;'><th>OI</th><th>PC</th><th>QUANTIDADE</th><th>MATERIAL</th><th>TEXTO BREVE MATERIAL</th></tr></thead>
                        <tbody>
                            @tableMateriais
                        </tbody>
                    </table>
                    <br /><br />
                    {texto2}";
        }

        public MaterialRecebimento ConsultarBC(int ano, string documento, string item)
        {
            var material = sapFacade.ConsultarDocumento(ano, documento, item);

            return material;
        }

        public IEnumerable<MaterialRecebimento> BuscarPorCodLista(string codLista)
        {
            return repository.FindByCriteria(x => x.CodLista == codLista);
        }

        public IEnumerable<MaterialRecebimento> Filtrar(MaterialRecebimento filtro)
        {
            return repository.FindByCriteria(x => (x.CodLista == filtro.CodLista || string.IsNullOrEmpty(filtro.CodLista)) &&
                                          (x.Nome == filtro.Nome || string.IsNullOrEmpty(filtro.Nome)) &&
                                          (x.CodMaterial == filtro.CodMaterial || string.IsNullOrEmpty(filtro.CodMaterial)) &&
                                          //(x.TemDivergencia == filtro.TemDivergencia() || !filtro.TemDivergencia()) &&
                                          (x.DataHoraInicio >= filtro.DataHoraInicio || !filtro.DataHoraInicio.HasValue) &&
                                          (x.DataHoraFim <= filtro.DataHoraFim || !filtro.DataHoraFim.HasValue) &&
                                          (x.Usuario == filtro.Usuario || string.IsNullOrEmpty(filtro.Usuario)) &&
                                          (x.NumeroDocumento == filtro.NumeroDocumento || string.IsNullOrEmpty(filtro.NumeroDocumento))
            );
        }
    }


}