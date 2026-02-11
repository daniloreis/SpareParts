using Domain;
using ERPConnect;
using ERPConnect.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Infrastructure
{
    public interface ISapIntegrationFacade
    {
        ListaRecebimento BuscarListaRecebimento(string id);
        WebApiResult CriarMigo(List<MaterialRecebimento> materiais);
        MaterialRecebimento ConsultarDocumento(int ano, string documento, string item, string depositoOrigem = "", bool buscarLotes = false);
        string CancelarDocumento(string numDocumento, string ano);
        MaterialRecebimento BuscarMaterialPendenteArmazenagem(int ano, string documento, string item);
        string Armazenar(MaterialArmazenagem material, string depositoOrigem = Deposito.ARMAZENAGEM, bool comUnidadeMedida = false);
        List<MaterialMovimentacao> BuscarMateriaisArmazenados(string posicaoDeposito);
        string Movimentar(MaterialMovimentacao material);
        string Inventariar(string numeroInventario, string anoFiscal, IList<MaterialInventario> materiais);
        IEnumerable<MaterialAtendimento> BuscarReserva(string numReserva);
        string AtenderPorReserva(IEnumerable<MaterialAtendimento> materiais);
        IEnumerable<MaterialAtendimento> BuscarTransferencia(string[] codMateriais);
        string AtenderPorTransferencia(IEnumerable<MaterialAtendimento> materiais, string depositoDestino = Deposito.TRANSFERENCIA);
        MaterialArmazenagem BuscarMaterialDetalhe(string codMaterial);

        bool VerificarInventarioAtivo(string deposito, string codMaterial);
        List<MaterialInventario> BuscarInventario(string numeroInventario, string anoFiscal);
    }

    /// <summary>
    /// FAÇADE DO SAP
    /// </summary>
    public class SapIntegrationFacade : ISapIntegrationFacade
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["SAPR3"].ConnectionString;
        private R3Connection connection;
        private const string plant = "CA01";

        private const string ENTRADA_DE_MATERIAL = "101";
        private const string TRASFERENCIA_DE_DEPOSITO = "311";
        private const string BAIXA_DE_MATERIAL = "991";

        public SapIntegrationFacade()
        {
            LIC.SetLic("V922C1ZY97"); //license key - theobald
        }

        private R3Connection OpenConnection()
        {
            if (connection == null)
                connection = new R3Connection(connectionString);

            connection.MultithreadingEnvironment = false;
            connection.Open(false);
            connection.HttpTimeout = int.MaxValue;

            if (!connection.Ping())
                throw new Exception("Não foi possível conectar ao SAP!");

            return connection;
        }

        #region RECEBIMENTO

        public ListaRecebimento BuscarListaRecebimento(string numero)
        {
            var listarecebimento = new ListaRecebimento() { CodLista = numero };

            try
            {
                connection = OpenConnection();

                var function = connection.CreateFunction("ZMM_SP_GET_LISTA_RECEBIMENTO");

                function.Exports["PI_NUM_LISTA_CEGA"].ParamValue = numero.FormatarNumeroLista();

                function.Execute();

                var structure = function.Imports["PO_CABECALHO_LISTA_CEGA"].ToStructure();

                if (structure.Columns.Count > 0)
                {
                    listarecebimento.Fornecedor = Convert.ToString(structure["FORNECEDOR"]);
                    listarecebimento.NumeroNfe = Convert.ToString(structure["NFE"]);
                    listarecebimento.NumeroVolumes = Convert.ToInt32(structure["QTDE_VOLUMES"]);
                }

                var materiais = function.Tables["PO_ITEMS_LISTA_CEGA"];

                foreach (RFCStructure materialRow in materiais.Rows)
                {
                    var material = new MaterialRecebimento();

                    material.CodLista = Convert.ToString(materialRow["NUM_LISTA_CEGA"]);
                    material.NumeroItem = Convert.ToString(materialRow["ITEM_LISTA_CEGA"]).FormatarNumeroItem();
                    material.CodMaterial = Convert.ToString(materialRow["COD_MATERIAL"]);
                    material.Nome = Convert.ToString(materialRow["DESC_MATERIAL"]);
                    material.Quantidade = Convert.ToDouble(materialRow["QTDE_FORNECIDA"], new CultureInfo("en-US"));
                    material.UnidadeMedida = Convert.ToString(materialRow["UNIDADE_MED"]);
                    material.NumeroPedido = Convert.ToString(materialRow["NUM_PEDIDO"]);
                    material.NumeroPedidoItem = Convert.ToString(materialRow["ITEM_PEDIDO"]);
                    material.Descricao = Convert.ToString(materialRow["TEXTO_MATERIAL"]);
                    material.ClassContabil = Convert.ToString(materialRow["CLASS_CONTABIL"]);
                    material.UnidadeTempo = Convert.ToString(materialRow["VALIDADE"]);
                    material.EmailSolicitante = Convert.ToString(materialRow["MAIL_SOLICITANTE"]);
                    material.TemLote = !string.IsNullOrWhiteSpace(Convert.ToString(materialRow["ADMIN_LOTE_OBRIG"]));
                    material.Fispq = Convert.ToString(materialRow["FISPQ"]);
                    material.RequerPlacaAtivo = Convert.ToString(materialRow["STS_CONTROLE_ATIVO"]).Equals("BRC-0001 REQ");

                    try { material.DataCriacaoRC = DateTime.ParseExact(Convert.ToString(materialRow["DATA_RC"]), "yyyyMMdd", new CultureInfo("pt-BR")); } catch { }

                    material.RevisaoOM = Convert.ToString(materialRow["REVISAO"]);
                    material.Deposito = Convert.ToString(materialRow["DEPOSITO"]);
                    material.DepositoDestino = material.DepositoRC = Convert.ToString(materialRow["DEPOSITO_RC"]);
                    material.DefinirDepositoDestino();

                    material.NumeroNota = listarecebimento.NumeroNfe;
                    material.DataHoraInicio = DateTime.Now;

                    if (material.TemLote && string.IsNullOrWhiteSpace(material.UnidadeTempo))
                    {
                        material.NumeroLote = "MANUTENÇÃO";
                    }

                    ReadTable ekpo = new ReadTable(connection);

                    ekpo.AddField("MENGE");
                    ekpo.AddCriteria($"EBELN = '{material.NumeroPedido}' AND EBELP = '{material.NumeroPedidoItem}' ");
                    ekpo.TableName = "EKPO";
                    ekpo.RowCount = 1;

                    ekpo.Run();

                    if (ekpo.Result.Rows.Count > 0)
                    {
                        material.QuantidadePO = Convert.ToDouble(ekpo.Result.Rows[0]["MENGE"], new CultureInfo("en-US"));
                    }

                    listarecebimento.Materiais.Add(material);

                }//end foreach
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }

            return listarecebimento;
        }

        public WebApiResult CriarMigo(List<MaterialRecebimento> materiais)
        {
            var retorno = new StringBuilder();
            var resultado = new WebApiResult();
            string documento = string.Empty;

            string function = "BAPI_TRANSACTION_COMMIT";

            try
            {
                connection = OpenConnection();

                RFCFunction functionBAPI = connection.CreateFunction("BAPI_GOODSMVT_CREATE");

                RFCStructure GOODSMVT_HEADER = functionBAPI.Exports["GOODSMVT_HEADER"].ToStructure();
                RFCStructure GOODSMVT_CODE = functionBAPI.Exports["GOODSMVT_CODE"].ToStructure();
                RFCTable GOODSMVT_ITEM = functionBAPI.Tables["GOODSMVT_ITEM"];
                RFCTable EXTENSIONIN = functionBAPI.Tables["EXTENSIONIN"];

                var data = DateTime.Now.ToString("yyyyMMdd");

                GOODSMVT_HEADER["PSTNG_DATE"] = data;
                GOODSMVT_HEADER["DOC_DATE"] = data;
                GOODSMVT_HEADER["HEADER_TXT"] = "ALMOXARIFADO";

                GOODSMVT_CODE["GM_CODE"] = "01";

                string notaFiscal = string.Empty;
                string usuario = string.Empty;
                string refDocNo = string.Empty;

                foreach (var material in materiais)
                {
                    RFCStructure GOODSMVT_ITEM_LINHA = new RFCStructure();

                    GOODSMVT_ITEM_LINHA = GOODSMVT_ITEM.AddRow();

                    GOODSMVT_ITEM_LINHA["MATERIAL"] = material.CodMaterial.ToFormatCode();
                    GOODSMVT_ITEM_LINHA["PLANT"] = plant;
                    GOODSMVT_ITEM_LINHA["STGE_LOC"] = material.Deposito;
                    GOODSMVT_ITEM_LINHA["UNLOAD_PT"] = material.PosicaoDeposito;
                    GOODSMVT_ITEM_LINHA["UNLOAD_PTX"] = "X";
                    GOODSMVT_ITEM_LINHA["MOVE_TYPE"] = ENTRADA_DE_MATERIAL;
                    GOODSMVT_ITEM_LINHA["ENTRY_QNT"] = material.Quantidade.Value.ToFormatString();
                    //GOODSMVT_ITEM_LINHA["ENTRY_UOM"] = material.UnidadeMedida;
                    GOODSMVT_ITEM_LINHA["PO_NUMBER"] = material.NumeroPedido?.PadLeft(10, '0');
                    GOODSMVT_ITEM_LINHA["PO_ITEM"] = material.NumeroPedidoItem?.PadLeft(5, '0');
                    GOODSMVT_ITEM_LINHA["MVT_IND"] = "B";
                    GOODSMVT_ITEM_LINHA["STGE_TYPE"] = "x";
                    GOODSMVT_ITEM_LINHA["GR_RCPT"] = material.Usuario;
                    GOODSMVT_ITEM_LINHA["GR_RCPTX"] = "x";

                    //GOODSMVT_ITEM_LINHA["DELIV_NUMB"] = material.CodLista.FormatarNumeroLista(); 
                    //GOODSMVT_ITEM_LINHA["DELIV_ITEM"] = material.NumeroItem;                     

                    if (material.Deposito.Equals(Deposito.DIVERGENCIA))
                        GOODSMVT_ITEM_LINHA["MOVE_REAS"] = "02";

                    if (material.TemLote && string.IsNullOrWhiteSpace(material.UnidadeTempo) && string.IsNullOrWhiteSpace(material.NumeroLote))
                        material.NumeroLote = "MANUTENÇÃO";

                    if (!string.IsNullOrWhiteSpace(material.NumeroLote))
                        GOODSMVT_ITEM_LINHA["BATCH"] = material.NumeroLote.ToUpper();

                    if (material.Validade.HasValue)
                        GOODSMVT_ITEM_LINHA["EXPIRYDATE"] = material.Validade.Value.ToString("yyyyMMdd");

                    refDocNo = material.NumeroPedido;
                    usuario = material.Usuario;
                    notaFiscal = material.NumeroNota;

                    if (material.RequerPlacaAtivo)
                        foreach (var ativo in material.Ativos)
                        {
                            RFCStructure EXTENSIONIN_LINHA =  new RFCStructure();
                            EXTENSIONIN_LINHA = EXTENSIONIN.AddRow();
                            EXTENSIONIN_LINHA["STRUCTURE"] = "ZPLACA_ATIVO";
                            EXTENSIONIN_LINHA["VALUEPART1"] = ativo.Sequencia;
                            EXTENSIONIN_LINHA["VALUEPART2"] = ativo.Placa;
                        }

                }

                GOODSMVT_HEADER["REF_DOC_NO_LONG"] = notaFiscal;
                GOODSMVT_HEADER["PR_UNAME"] = usuario;
                GOODSMVT_HEADER["REF_DOC_NO"] = refDocNo;

                functionBAPI.Exports["GOODSMVT_HEADER"].ParamValue = GOODSMVT_HEADER;
                functionBAPI.Exports["GOODSMVT_CODE"].ParamValue = GOODSMVT_CODE;
                functionBAPI.Exports["TESTRUN"].ParamValue = "";

                functionBAPI.Tables["GOODSMVT_ITEM"] = GOODSMVT_ITEM;
                functionBAPI.Tables["EXTENSIONIN"] = EXTENSIONIN;

                functionBAPI.Execute();

                RFCStructure GOODSMVT_HEADRET = functionBAPI.Imports["GOODSMVT_HEADRET"].ToStructure();

                if (!string.IsNullOrWhiteSpace(Convert.ToString(GOODSMVT_HEADRET["MAT_DOC"])))
                {
                    retorno.Append($"OK:{GOODSMVT_HEADRET["MAT_DOC"]}");
                    documento = Convert.ToString(GOODSMVT_HEADRET["MAT_DOC"]);
                }
                else
                {
                    function = "BAPI_TRANSACTION_ROLLBACK";
                    retorno.AppendLine("Mensagem do SAP:");

                    foreach (RFCStructure row in functionBAPI.Tables["RETURN"].Rows)
                    {
                        retorno.AppendLine(Convert.ToString(row["MESSAGE"]));
                    }
                }

                resultado.Success = !string.IsNullOrWhiteSpace(documento);
            }
            catch (Exception ex)
            {
                function = "BAPI_TRANSACTION_ROLLBACK";
                retorno = new StringBuilder(ex.GetInnerExceptionMessage());
                resultado.Success = false;
            }
            finally
            {
#if !DEBUG
                connection.CreateFunction(function).Execute();
#endif
                connection.Close();
                connection.Dispose();
            }

            if (resultado.Success)
            {
                var migoResult = ConsultarMigo(documento, DateTime.Now.Year);

                foreach (var material in materiais)
                {
                    var itemTemp = migoResult.FirstOrDefault(x => x.NumeroPedido == material.NumeroPedido && x.NumeroPedidoItem == material.NumeroPedidoItem);
                    if (itemTemp.IsNotNull())
                    {
                        if (!string.IsNullOrWhiteSpace(itemTemp.NumeroItem))
                            material.NumeroItem = itemTemp.NumeroItem;
                        if (!string.IsNullOrWhiteSpace(itemTemp.Ordem))
                            material.Ordem = itemTemp.Ordem;
                    }
                    material.Ano = DateTime.Now.Year;
                    material.NumeroDocumento = documento;
                }

                resultado.Result = materiais;
            }

            resultado.Message = retorno.ToString();

            return resultado;
        }

        private List<MaterialRecebimento> ConsultarMigo(string documento, int ano)
        {
            var lista = new List<MaterialRecebimento>();

            connection = OpenConnection();

            var functionBAPI = connection.CreateFunction("BAPI_GOODSMVT_GETDETAIL");

            functionBAPI.Exports["MATERIALDOCUMENT"].ParamValue = documento;
            functionBAPI.Exports["MATDOCUMENTYEAR"].ParamValue = ano.ToString();

            functionBAPI.Execute();

            var materiais = functionBAPI.Tables["GOODSMVT_ITEMS"];

            foreach (RFCStructure materialRow in materiais.Rows)
            {
                var material = new MaterialRecebimento();

                material.NumeroItem = Convert.ToString(materialRow["MATDOC_ITM"]).FormatarNumeroItem();
                material.NumeroPedido = Convert.ToString(materialRow["PO_NUMBER"]);
                material.NumeroPedidoItem = Convert.ToString(materialRow["PO_ITEM"]);
                material.Ordem = Convert.ToString(materialRow["ORDERID"]);

                lista.Add(material);
            }

            return lista;
        }

        public string CancelarDocumento(string numDocumento, string ano)
        {
            var retorno = string.Empty;
            string function = "BAPI_TRANSACTION_COMMIT";

            try
            {
                connection = OpenConnection();

                var functionBAPI = connection.CreateFunction("BAPI_GOODSMVT_CANCEL");

                functionBAPI.Exports["MATERIALDOCUMENT"].ParamValue = numDocumento;
                functionBAPI.Exports["MATDOCUMENTYEAR"].ParamValue = ano;
                functionBAPI.Exports["GOODSMVT_PSTNG_DATE"].ParamValue = DateTime.Now.ToString("yyyyMMdd");

                functionBAPI.Execute();

                foreach (RFCStructure row in functionBAPI.Tables["RETURN"].Rows)
                {
                    retorno += Convert.ToString(row["MESSAGE"]) + Environment.NewLine;
                }

                if (string.IsNullOrWhiteSpace(retorno))
                    retorno = "OK";

                return retorno;
            }
            catch (Exception ex)
            {
                function = "BAPI_TRANSACTION_ROLLBACK";
                retorno = ex.GetInnerExceptionMessage();
            }
            finally
            {
                connection.CreateFunction(function).Execute();
                connection.Close();
            }

            return retorno;
        }

        public MaterialRecebimento ConsultarDocumento(int ano, string documento, string item, string depositoOrigem = "", bool buscarLotes = false)
        {
            MaterialRecebimento material = null;

            try
            {
                connection = OpenConnection();

                var functionBAPI = connection.CreateFunction("BAPI_GOODSMVT_GETDETAIL");

                functionBAPI.Exports["MATERIALDOCUMENT"].ParamValue = documento;
                functionBAPI.Exports["MATDOCUMENTYEAR"].ParamValue = ano.ToString();

                functionBAPI.Execute();

                var header = functionBAPI.Imports["GOODSMVT_HEADER"].ToStructure();

                var materiais = functionBAPI.Tables["GOODSMVT_ITEMS"];

                foreach (RFCStructure materialRow in materiais.Rows)
                {
                    var numItem = Convert.ToString(materialRow["MATDOC_ITM"]).FormatarNumeroItem();

                    if (numItem == item.FormatarNumeroItem())
                    {
                        material = new MaterialRecebimento();

                        material.Ano = ano;
                        material.NumeroDocumento = documento;
                        material.NumeroItem = numItem;

                        try { material.DataEntrada = DateTime.ParseExact(Convert.ToString(header["ENTRY_DATE"]), "yyyyMMdd", new CultureInfo("pt-BR")); } catch { }

                        material.NumeroNota = Convert.ToString(header["REF_DOC_NO_LONG"]); //header
                        material.Usuario = Convert.ToString(header["USERNAME"]); //header

                        material.CodMaterial = Convert.ToString(materialRow["MATERIAL"]);
                        material.NumeroPedido = Convert.ToString(materialRow["PO_NUMBER"]);
                        material.NumeroPedidoItem = Convert.ToString(materialRow["PO_ITEM"]);
                        material.Centro = Convert.ToString(materialRow["PLANT"]);
                        material.PosicaoDepositoOrigem = Convert.ToString(materialRow["UNLOAD_PT"]);
                        material.NumeroLote = Convert.ToString(materialRow["BATCH"]);
                        material.Deposito = Convert.ToString(materialRow["STGE_LOC"]);
                        material.Fornecedor = Convert.ToString(materialRow["VENDOR"]);
                        material.Quantidade = Convert.ToDouble(materialRow["ENTRY_QNT"], new CultureInfo("en-US"));
                        material.TipoMovimento = Convert.ToString(materialRow["MOVE_TYPE"]);
                        material.UsuarioAtendido = Convert.ToString(materialRow["GR_RCPT"]);
                        material.Ordem = Convert.ToString(materialRow["ORDERID"]);

                        try { material.Validade = DateTime.ParseExact(Convert.ToString(header["EXPIRYDATE"]), "yyyyMMdd", new CultureInfo("pt-BR")); } catch { }

                        var function = connection.CreateFunction("ZMM_SP_MATERIAL_GET_LIST");

                        function.Exports["PI_WERKS"].ParamValue = plant;
                        function.Exports["PI_STOCK_MIN"].ParamValue = -1;

                        #region FILTRO DE DEPÓSITO

                        if (!string.IsNullOrWhiteSpace(depositoOrigem))
                        {
                            RFCTable STORAGELOCATION = function.Tables["STORAGELOCATION"];

                            RFCStructure STORAGELOCATION_ITEM_LINHA = new RFCStructure();

                            STORAGELOCATION_ITEM_LINHA = STORAGELOCATION.AddRow();

                            STORAGELOCATION_ITEM_LINHA["SIGN"] = "I";
                            STORAGELOCATION_ITEM_LINHA["OPTION"] = "EQ";
                            STORAGELOCATION_ITEM_LINHA["STLOC_LOW"] = depositoOrigem;
                            STORAGELOCATION_ITEM_LINHA["STLOC_HIGH"] = depositoOrigem;

                            function.Tables["STORAGELOCATION"] = STORAGELOCATION;
                        }

                        #endregion

                        #region FILTRO DE MATERIAL

                        RFCTable MATERIALSELECTION = function.Tables["MATERIALSELECTION"];

                        RFCStructure MATERIALSELECTION_ITEM_LINHA = new RFCStructure();

                        MATERIALSELECTION_ITEM_LINHA = MATERIALSELECTION.AddRow();

                        MATERIALSELECTION_ITEM_LINHA["SIGN"] = "I"; //I-Include E-Exclude
                        MATERIALSELECTION_ITEM_LINHA["OPTION"] = "EQ"; //EQ-Equals
                        MATERIALSELECTION_ITEM_LINHA["MATNR_LOW"] = material.CodMaterial.ToFormatCode();
                        MATERIALSELECTION_ITEM_LINHA["MATNR_HIGH"] = material.CodMaterial.ToFormatCode();

                        function.Tables["MATERIALSELECTION"] = MATERIALSELECTION;

                        #endregion

                        function.Execute();

                        RFCTable MATERIALLIST = function.Tables["MATERIALLIST"];

                        if (MATERIALLIST.Rows.Count > 0)
                            foreach (RFCStructure row in MATERIALLIST.Rows)
                            {
                                material.Nome = Convert.ToString(row["MAKTX"]);
                                material.CondicaoArmazenagem = Convert.ToString(row["RBTXT"]);
                                material.CondicaoTemperatura = Convert.ToString(row["TBTXT"]);
                                material.DepositoDestino = Convert.ToString(row["LGFSB"]);
                                material.Fispq = Convert.ToString(row["PROFL"]);
                                material.UnidadeMedida = Convert.ToString(row["MSEH3"]);
                                material.DataHoraInicio = DateTime.Now;
                            }
                        else
                            material = null;

                        if (material == null)
                            return null;

                        #region BUSCA A DESCRIÇÃO DO MATERIAL

                        var descMaterial = new StringBuilder();

                        RFCFunction functionReadText = connection.CreateFunction("RFC_READ_TEXT");

                        RFCStructure newrow = functionReadText.Tables["TEXT_LINES"].Rows.Add();

                        newrow["TDOBJECT"] = "MATERIAL"; //Text object
                        newrow["TDNAME"] = material.CodMaterial.ToFormatCode(); // Key
                        newrow["TDID"] = "BEST"; // Text-ID
                        newrow["TDSPRAS"] = "PT"; // Language

                        functionReadText.Execute();

                        foreach (RFCStructure row in functionReadText.Tables["TEXT_LINES"].Rows)
                            descMaterial.AppendLine(row["TDLINE"].ToString());

                        material.Descricao = descMaterial.ToString();

                        #endregion

                        #region BUSCAR DEPÓSITO DA ORDEM E DA RC

                        if (material.Ordem.HasValue())
                        {
                            ReadTable afih = new ReadTable(connection);

                            afih.AddField("REVNR");
                            afih.AddCriteria($"AUFNR = '{material.Ordem}'");
                            afih.TableName = "AFIH";
                            afih.RowCount = 1;

                            afih.Run();

                            if (afih.Result.Rows.Count > 0)
                            {
                                material.RevisaoOM = Convert.ToString(afih.Result.Rows[0]["REVNR"]);
                            }
                        }

                        ReadTable eban = new ReadTable(connection);

                        eban.AddField("LGORT");
                        eban.AddCriteria($"WERKS = '{plant}'");
                        eban.AddCriteria($"AND EBELN = '{material.NumeroPedido}'");
                        eban.AddCriteria($"AND EBELP = '{material.NumeroPedidoItem}'");
                        eban.TableName = "EBAN";
                        eban.RowCount = 1;

                        eban.Run();

                        if (eban.Result.Rows.Count > 0)
                        {
                            material.DepositoRC = Convert.ToString(eban.Result.Rows[0]["LGORT"]);
                        }

                        #endregion

                        #region BUSCAR NO DEPÓSITO PADRÃO: ESTOQUE ATUAL, ENDEREÇO PADRÃO E INVENTÁRIO ATIVO

                        material.DefinirDepositoDestino();

                        if (material.DepositoDestino.HasValue())
                        {
                            ReadTable mard = new ReadTable(connection);

                            mard.AddField("LGPBE");
                            mard.AddField("LABST");
                            mard.AddField("SPERR");
                            mard.AddCriteria($"MATNR = '{ material.CodMaterial.ToFormatCode()}'");
                            mard.AddCriteria($"AND WERKS = '{plant}'");
                            mard.AddCriteria($"AND LGORT = '{material.DepositoDestino}'");
                            mard.TableName = "MARD";
                            mard.RowCount = 1;

                            mard.Run();

                            if (mard.Result.Rows.Count > 0)
                            {
                                material.QuantidadeEstoqueSAP = Convert.ToDouble(mard.Result.Rows[0]["LABST"], new CultureInfo("en-US"));
                                material.PosicaoDeposito = Convert.ToString(mard.Result.Rows[0]["LGPBE"]);
                                material.InventarioAtivo = !string.IsNullOrWhiteSpace(Convert.ToString(mard.Result.Rows[0]["SPERR"]));
                            }
                        }
                        material.DefinirDepositoDestino();
                        #endregion
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }

            return material;
        }

        #endregion

        #region ARMAZENAGEM

        public MaterialRecebimento BuscarMaterialPendenteArmazenagem(int ano, string documento, string item)
        {
            var material = new MaterialRecebimento();

            try
            {
                material = ConsultarDocumento(ano, documento, item, Deposito.ARMAZENAGEM, true);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return material;
        }

        public string Armazenar(MaterialArmazenagem material, string depositoOrigem = Deposito.ARMAZENAGEM, bool comUnidadeMedida = false)
        {
            var retorno = new StringBuilder();
            string function = "BAPI_TRANSACTION_COMMIT";

            try
            {

                connection = OpenConnection();

                RFCFunction functionBAPI = connection.CreateFunction("BAPI_GOODSMVT_CREATE");

                RFCStructure GOODSMVT_HEADER = functionBAPI.Exports["GOODSMVT_HEADER"].ToStructure();

                RFCStructure GOODSMVT_CODE = functionBAPI.Exports["GOODSMVT_CODE"].ToStructure();
                RFCTable GOODSMVT_ITEM = functionBAPI.Tables["GOODSMVT_ITEM"];
                RFCStructure GOODSMVT_ITEM_LINHA = new RFCStructure();

                GOODSMVT_HEADER["PSTNG_DATE"] = DateTime.Now.ToString("yyyyMMdd");
                GOODSMVT_HEADER["DOC_DATE"] = DateTime.Now.ToString("yyyyMMdd");
                GOODSMVT_HEADER["HEADER_TXT"] = "ALMOXARIFADO";
                GOODSMVT_HEADER["REF_DOC_NO_LONG"] = "ALMOXARIFADO AUTOMAÇÃO";

                GOODSMVT_CODE["GM_CODE"] = "04";

                GOODSMVT_ITEM_LINHA = GOODSMVT_ITEM.AddRow();

                GOODSMVT_ITEM_LINHA["MATERIAL"] = material.CodMaterial.ToFormatCode();
                GOODSMVT_ITEM_LINHA["PLANT"] = plant;
                GOODSMVT_ITEM_LINHA["STGE_LOC"] = depositoOrigem;
                GOODSMVT_ITEM_LINHA["UNLOAD_PT"] = material.PosicaoDeposito;
                GOODSMVT_ITEM_LINHA["UNLOAD_PTX"] = "X";
                GOODSMVT_ITEM_LINHA["MOVE_TYPE"] = TRASFERENCIA_DE_DEPOSITO;
                GOODSMVT_ITEM_LINHA["MOVE_STLOC"] = material.DepositoDestino;
                GOODSMVT_ITEM_LINHA["ENTRY_QNT"] = material.Quantidade.Value.ToFormatString();

                if (comUnidadeMedida)
                    GOODSMVT_ITEM_LINHA["ENTRY_UOM"] = material.UnidadeMedida;

                if (material.NumeroLote.HasValue())
                {
                    GOODSMVT_ITEM_LINHA["BATCH"] = material.NumeroLote;
                }

                functionBAPI.Exports["GOODSMVT_HEADER"].ParamValue = GOODSMVT_HEADER;
                functionBAPI.Exports["GOODSMVT_CODE"].ParamValue = GOODSMVT_CODE;

                functionBAPI.Tables["GOODSMVT_ITEM"] = GOODSMVT_ITEM;

                functionBAPI.Execute();

                RFCStructure GOODSMVT_HEADRET = functionBAPI.Imports["GOODSMVT_HEADRET"].ToStructure();

                if (!string.IsNullOrWhiteSpace(Convert.ToString(GOODSMVT_HEADRET["MAT_DOC"])))
                {
                    retorno.Append($"OK:{GOODSMVT_HEADRET["MAT_DOC"]}");
                }
                else
                {
                    function = "BAPI_TRANSACTION_ROLLBACK";
                    retorno.AppendLine("Mensagem do SAP:");

                    foreach (RFCStructure row in functionBAPI.Tables["RETURN"].Rows)
                    {
                        retorno.AppendLine(Convert.ToString(row["MESSAGE"]));
                    }
                }

            }
            catch (Exception ex)
            {
                function = "BAPI_TRANSACTION_ROLLBACK";
                retorno = new StringBuilder(ex.GetInnerExceptionMessage());
            }
            finally
            {
#if !DEBUG
                connection.CreateFunction(function).Execute();
#endif
                connection.Close();
            }

            if (retorno.ToString().Contains("UM NÃO PODE SER CONVERTIDA EM UNIDADE"))
                Armazenar(material, Deposito.ARMAZENAGEM, comUnidadeMedida: true);

            return retorno.ToString();
        }

        #endregion

        #region MOVIMENTAÇÃO

        public List<MaterialMovimentacao> BuscarMateriaisArmazenados(string posicaoDeposito)
        {
            var materiais = new List<MaterialMovimentacao>();
            var codigos = new List<string>();

            try
            {
                connection = OpenConnection();

                ReadTable mard = new ReadTable(connection);

                mard.AddField("MATNR");
                mard.AddCriteria($"LGPBE = '{posicaoDeposito}' AND WERKS = '{plant}'");
                mard.TableName = "MARD";

                mard.Run();

                foreach (DataRow mardRow in mard.Result.Rows)
                {
                    var codMaterial = Convert.ToString(mardRow["MATNR"]);
                    codigos.Add(codMaterial);
                }

                var function = connection.CreateFunction("ZMM_SP_MATERIAL_GET_LIST");

                function.Exports["PI_WERKS"].ParamValue = plant;

                #region FILTRO DE MATERIAIS

                RFCTable MATERIALSELECTION = function.Tables["MATERIALSELECTION"];

                RFCStructure MATERIALSELECTION_ITEM_LINHA = new RFCStructure();

                foreach (var codMaterial in codigos)
                {
                    MATERIALSELECTION_ITEM_LINHA = MATERIALSELECTION.AddRow();

                    MATERIALSELECTION_ITEM_LINHA["SIGN"] = "I";
                    MATERIALSELECTION_ITEM_LINHA["OPTION"] = "EQ";
                    MATERIALSELECTION_ITEM_LINHA["MATNR_LOW"] = codMaterial.ToFormatCode();
                    MATERIALSELECTION_ITEM_LINHA["MATNR_HIGH"] = codMaterial.ToFormatCode();
                }

                function.Tables["MATERIALSELECTION"] = MATERIALSELECTION;

                #endregion

                function.Execute();

                RFCTable MATERIALLIST = function.Tables["MATERIALLIST"];

                foreach (RFCStructure row in MATERIALLIST.Rows)
                {
                    var material = new MaterialMovimentacao();

                    material.CodMaterial = Convert.ToString(row["MATNR"]);
                    material.Nome = Convert.ToString(row["MAKTX"]);
                    material.Quantidade = Convert.ToDouble(row["LABST"], new CultureInfo("en-US"));
                    material.UnidadeMedida = Convert.ToString(row["MSEH3"]);
                    material.Deposito = Convert.ToString(row["LGORT"]);

                    material.DataHoraInicio = DateTime.Now;

                    if (material.Quantidade > 0)
                        materiais.Add(material);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }

            return materiais;
        }

        public string Movimentar(MaterialMovimentacao material)
        {
            var retorno = new StringBuilder();
            string function = "BAPI_TRANSACTION_COMMIT";

            try
            {
                connection = OpenConnection();

                RFCFunction functionBAPI = connection.CreateFunction("BAPI_MATERIAL_SAVEDATA");

                RFCStructure HEADDATA = functionBAPI.Exports["HEADDATA"].ToStructure();
                RFCStructure STORAGELOCATIONDATA = functionBAPI.Exports["STORAGELOCATIONDATA"].ToStructure();
                RFCStructure STORAGELOCATIONDATAX = functionBAPI.Exports["STORAGELOCATIONDATAX"].ToStructure();

                functionBAPI.Exports["HEADDATA"].ParamValue = HEADDATA;
                functionBAPI.Exports["STORAGELOCATIONDATA"].ParamValue = STORAGELOCATIONDATA;
                functionBAPI.Exports["STORAGELOCATIONDATAX"].ParamValue = STORAGELOCATIONDATAX;

                HEADDATA["MATERIAL"] = material.CodMaterial.ToFormatCode();

                STORAGELOCATIONDATA["PLANT"] = plant;
                STORAGELOCATIONDATA["STGE_LOC"] = material.Deposito;
                STORAGELOCATIONDATA["STGE_BIN"] = material.PosicaoDeposito;

                STORAGELOCATIONDATAX["PLANT"] = plant;
                STORAGELOCATIONDATAX["STGE_LOC"] = material.Deposito;
                STORAGELOCATIONDATAX["STGE_BIN"] = "x";

                functionBAPI.Execute();

                RFCStructure RETURN = functionBAPI.Imports["RETURN"].ToStructure();

                if (!string.IsNullOrWhiteSpace(Convert.ToString(RETURN["MESSAGE"])))
                {
                    var temErro = Convert.ToString(RETURN["TYPE"]).Equals("E");

                    if (temErro)
                        retorno.Append($"{RETURN["MESSAGE"]}");
                    else
                        retorno.Append($"OK:{RETURN["MESSAGE"]}");
                }

            }
            catch (Exception ex)
            {
                function = "BAPI_TRANSACTION_ROLLBACK";
                retorno = new StringBuilder(ex.GetInnerExceptionMessage());
            }
            finally
            {
                connection.CreateFunction(function).Execute();
                connection.Close();
            }

            return retorno.ToString();
        }

        #endregion

        #region INVENTÁRIO

        public List<MaterialInventario> BuscarInventario(string numeroInventario, string anoFiscal)
        {
            var materiais = new List<MaterialInventario>();

            try
            {
                connection = OpenConnection();

                var function = connection.CreateFunction("BAPI_MATPHYSINV_GETDETAIL");

                function.Exports["PHYSINVENTORY"].ParamValue = numeroInventario.PadLeft(10, '0');
                function.Exports["FISCALYEAR"].ParamValue = anoFiscal;
                RFCStructure HEAD = function.Imports["HEAD"].ToStructure();

                function.Execute();

                if ((Convert.ToString(HEAD["COUNT_STATUS"]) ?? "").Equals("X"))
                    return null;

                var tabelaMateriais = function.Tables["ITEMS"];

                foreach (RFCStructure row in tabelaMateriais.Rows)
                {
                    if (string.IsNullOrEmpty(Convert.ToString(row["COUNTED"]))) //se material não foi contado
                    {
                        var material = new MaterialInventario
                        {
                            CodMaterial = Convert.ToString(row["MATERIAL"]),
                            NumeroPedidoItem = Convert.ToString(row["ITEM"]),
                            NumeroLote = Convert.ToString(row["BATCH"]),
                            UnidadeMedida = Convert.ToString(row["ENTRY_UOM_ISO"]),
                            UnidadeMedidaOriginal = Convert.ToString(row["ENTRY_UOM"]),
                            Deposito = Convert.ToString(row["STGE_LOC"])
                        };

                        #region BUSCAR NOME

                        ReadTable makt = new ReadTable(connection);

                        makt.AddField("MAKTX");
                        makt.AddCriteria($"MATNR = '{material.CodMaterial.ToFormatCode()}' AND SPRAS = 'PT'");
                        makt.TableName = "MAKT";
                        makt.RowCount = 1;

                        makt.Run();

                        if (makt.Result.Rows.Count > 0)
                            material.Nome = Convert.ToString(makt.Result.Rows[0]["MAKTX"]);

                        #endregion

                        #region BUSCAR POSICAO

                        ReadTable mard = new ReadTable(connection);

                        mard.AddField("LGPBE");
                        mard.AddCriteria($"MATNR = '{material.CodMaterial.ToFormatCode()}' AND LGORT = '{material.Deposito}' AND WERKS = '{plant}'");
                        mard.TableName = "MARD";
                        mard.RowCount = 1;

                        mard.Run();

                        if (mard.Result.Rows.Count > 0)
                            material.PosicaoDeposito = Convert.ToString(mard.Result.Rows[0]["LGPBE"]);

                        #endregion

                        material.DataHoraInicio = DateTime.Now;

                        materiais.Add(material);
                    }
                }//end for

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }

            return materiais;
        }

        public string Inventariar(string numeroInventario, string anoFiscal, IList<MaterialInventario> materiais)
        {
            var retorno = new StringBuilder();
            string function = "BAPI_TRANSACTION_COMMIT";

            try
            {
                connection = OpenConnection();

                RFCFunction functionBAPI = connection.CreateFunction("BAPI_MATPHYSINV_COUNT");

                functionBAPI.Exports["PHYSINVENTORY"].ParamValue = numeroInventario.PadLeft(10, '0');
                functionBAPI.Exports["FISCALYEAR"].ParamValue = anoFiscal;
                functionBAPI.Exports["COUNT_DATE"].ParamValue = DateTime.Now.ToString("yyyyMMdd");

                RFCTable ITEMS = functionBAPI.Tables["ITEMS"];
                RFCStructure ITEM_LINHA = new RFCStructure();

                foreach (var item in materiais)
                {
                    ITEM_LINHA = ITEMS.AddRow();

                    ITEM_LINHA["MATERIAL"] = item.CodMaterial.ToFormatCode();
                    ITEM_LINHA["ITEM"] = item.NumeroPedidoItem;
                    ITEM_LINHA["BATCH"] = item.NumeroLote;
                    ITEM_LINHA["ENTRY_QNT"] = item.Quantidade;
                    ITEM_LINHA["ENTRY_UOM"] = item.UnidadeMedidaOriginal;

                    if (item.Quantidade == 0)
                        ITEM_LINHA["ZERO_COUNT"] = "X";
                }

                functionBAPI.Tables["ITEMS"] = ITEMS;

                functionBAPI.Execute();

                bool temAlgumComErro = false;

                foreach (RFCStructure row in functionBAPI.Tables["RETURN"].Rows)
                {
                    temAlgumComErro = Convert.ToString(row["TYPE"]).Equals("E");
                    retorno.AppendLine($"Linha: {Convert.ToString(row["ROW"])} Mensagem: {Convert.ToString(row["MESSAGE"])}");
                }

                if (temAlgumComErro)
                {
                    function = "BAPI_TRANSACTION_ROLLBACK";
                    retorno.Insert(0, "Mensagem do SAP: " + Environment.NewLine);
                }
                else
                {
                    retorno = new StringBuilder("OK:Contagem enviada com sucesso!");
                }

            }
            catch (Exception ex)
            {
                function = "BAPI_TRANSACTION_ROLLBACK";
                retorno = new StringBuilder(ex.GetInnerExceptionMessage());
            }
            finally
            {
#if !DEBUG
                connection.CreateFunction(function).Execute();
#endif
                connection.Close();
            }

            return retorno.ToString();
        }

        #endregion

        #region ATENDIMENTO

        public IEnumerable<MaterialAtendimento> BuscarReserva(string numReserva)
        {
            var materiais = new List<MaterialAtendimento>();

            try
            {
                connection = OpenConnection();

                RFCFunction bapi_reserva1 = connection.CreateFunction("BAPI_RESERVATION_GETITEMS");

                bapi_reserva1.Exports["RES_NO"].ParamValue = numReserva.PadLeft(10, '0');
                bapi_reserva1.Exports["PLANT"].ParamValue = plant;

                bapi_reserva1.Execute();

                RFCTable items2 = bapi_reserva1.Tables["RESERVATION_ITEMS"];

                var nomes = new Dictionary<string, string>();

                foreach (RFCStructure item in items2.Rows)
                {
                    var codigo = Convert.ToString((item["MATERIAL"])).TrimStart('0');
                    var nome = Convert.ToString(item["SHORT_TEXT"]);

                    if (!nomes.ContainsKey(codigo))
                        nomes.Add(codigo, nome);
                }

                RFCFunction bapi_reserva = connection.CreateFunction("BAPI_RESERVATION_GETDETAIL1");

                bapi_reserva.Exports["RESERVATION"].ParamValue = numReserva.PadLeft(10, '0');

                bapi_reserva.Execute();

                RFCTable items = bapi_reserva.Tables["RESERVATION_ITEMS"];

                foreach (RFCStructure item in items.Rows)
                {
                    var naoEstaEliminado = string.IsNullOrWhiteSpace(Convert.ToString(item["DELETE_IND"]));

                    if (naoEstaEliminado)
                    {
                        var material = new MaterialAtendimento();

                        material.NumeroReserva = numReserva;
                        material.CodMaterial = Convert.ToString(item["MATERIAL"]);
                        material.Quantidade = Convert.ToDouble(item["QUANTITY"], new CultureInfo("en-US"));
                        material.UnidadeMedida = Convert.ToString(item["BASE_UOM_ISO"]);
                        material.Sequencia = Convert.ToString(item["RES_ITEM"]);
                        material.Deposito = Convert.ToString(item["STORE_LOC"]);
                        material.NumeroLote = Convert.ToString(item["BATCH"]);

                        if (nomes.ContainsKey(material.CodMaterial))
                            material.Nome = nomes[material.CodMaterial];

                        var quantidadeConsumida = Convert.ToDouble(item["WITHD_QUAN"], new CultureInfo("en-US")); //Quantidade já retirada.

                        material.Quantidade = material.QuantidadeDisponivel = material.Quantidade - quantidadeConsumida;

                        material.DataHoraInicio = DateTime.Now;

                        materiais.Add(material);
                    }

                }//end foreach materiais

                var codigos = string.Join(",", materiais.Select(x => "'" + x.CodMaterial.ToFormatCode() + "'"));

                if (codigos.HasValue())
                {
                    #region BUSCAR CRITICIDADE DO MATERIAL

                    ReadTable marc = new ReadTable(connection);

                    marc.AddField("MATNR");
                    marc.AddField("KZKRI");
                    marc.WhereClause = $"WERKS = '{plant}' AND MATNR IN ({codigos})";
                    marc.TableName = "MARC";
                    marc.Run();

                    var criticos = new Dictionary<string, bool>();

                    foreach (DataRow marcRow in marc.Result.Rows) //procura lotes
                    {
                        var codigo = Convert.ToString((marcRow["MATNR"]));
                        var critico = Convert.ToBoolean((marcRow["KZKRI"] ?? "").ToString().ToUpper().Equals("X"));

                        if (!criticos.ContainsKey(codigo.ToFormatCode()))
                            criticos.Add(codigo.ToFormatCode(), critico);
                    }

                    #endregion

                    #region BUSCAR LOTES DOS MATERIAIS

                    ReadTable mchb = new ReadTable(connection);

                    mchb.AddField("MATNR");
                    mchb.AddField("CHARG");
                    mchb.AddField("CLABS");
                    mchb.AddField("LGORT");
                    mchb.WhereClause = $"WERKS = '{plant}' AND MATNR IN ({codigos}) AND CLABS > 0 AND LGORT <> 'DMMV' AND LGORT <> 'DM04' AND LGORT <> 'DM14' AND LGORT <> 'DM15'";
                    mchb.TableName = "MCHB";
                    mchb.Run();

                    var lotes = new List<MaterialAtendimento>();

                    foreach (DataRow mchbRow in mchb.Result.Rows)
                    {
                        var lote = new MaterialAtendimento();

                        lote.CodMaterial = Convert.ToString(mchbRow["MATNR"]);
                        lote.NumeroLote = Convert.ToString(mchbRow["CHARG"]);
                        lote.QuantidadeEstoqueSAP = Convert.ToDouble(mchbRow["CLABS"], new CultureInfo("en-US"));
                        lote.Deposito = Convert.ToString(mchbRow["LGORT"]);

                        lotes.Add(lote);
                    }

                    #endregion

                    #region BUSCAR VALIDADES DOS LOTES

                    var numLotes = string.Join(",", lotes.Select(x => "'" + x.NumeroLote + "'"));

                    ReadTable mcha = new ReadTable(connection);

                    mcha.AddField("MATNR");
                    mcha.AddField("VFDAT");
                    mcha.AddField("CHARG");
                    mcha.WhereClause = $"WERKS = '{plant}' AND MATNR IN ({codigos}) AND CHARG IN ({numLotes})";
                    mcha.TableName = "MCHA";
                    mcha.Run();

                    var validades = new List<MaterialAtendimento>();

                    foreach (DataRow mchaRow in mcha.Result.Rows)
                    {
                        var validade = new MaterialAtendimento();

                        validade.CodMaterial = Convert.ToString(mchaRow["MATNR"]);
                        validade.NumeroLote = Convert.ToString(mchaRow["CHARG"]);

                        var data = Convert.ToString(mchaRow["VFDAT"]);
                        try { validade.Validade = DateTime.ParseExact(data, "yyyyMMdd", new CultureInfo("pt-BR")); } catch { }

                        validades.Add(validade);
                    }

                    #endregion

                    #region BUSCAR DEPÓSITOS DOS MATERIAIS

                    ReadTable mard = new ReadTable(connection);

                    mard.AddField("MATNR");
                    mard.AddField("LABST");
                    mard.AddField("LGPBE");
                    mard.AddField("LGORT");
                    mard.WhereClause = $"WERKS = '{plant}' AND MATNR IN ({codigos}) AND LABST > 0 AND LGORT <> 'DMMV' AND LGORT <> 'DM04' AND LGORT <> 'DM14' AND LGORT <> 'DM15'";
                    mard.TableName = "MARD";
                    mard.Run();

                    var depositos = new List<Deposito>();

                    foreach (DataRow mardRow in mard.Result.Rows)
                    {
                        var deposito = new Deposito();

                        deposito.CodMaterial = Convert.ToString(mardRow["MATNR"]);
                        deposito.Nome = Convert.ToString(mardRow["LGORT"]);
                        deposito.PosicaoDeposito = Convert.ToString(mardRow["LGPBE"]);
                        deposito.QuantidadeEstoqueSAP = Convert.ToDouble(mardRow["LABST"], new CultureInfo("en-US"));

                        depositos.Add(deposito);
                    }

                    #endregion

                    foreach (var material in materiais)
                    {
                        if (criticos.ContainsKey(material.CodMaterial.ToFormatCode()))
                            material.Critico = criticos[material.CodMaterial.ToFormatCode()];

                        var lots = lotes.Where(x => x.CodMaterial.ToFormatCode() == material.CodMaterial.ToFormatCode());
                        var deps = depositos.Where(x => x.CodMaterial.ToFormatCode() == material.CodMaterial.ToFormatCode());

                        if (lots.Any())
                        {
                            foreach (var lote in lots)
                            {
                                var deposito = new Deposito();

                                deposito.CodMaterial = lote.CodMaterial;

                                deposito.QuantidadeEstoqueSAP = lote.QuantidadeEstoqueSAP;
                                deposito.PosicaoDeposito = depositos.FirstOrDefault(x => x.CodMaterial.ToFormatCode() == lote.CodMaterial.ToFormatCode() && x.Nome == lote.Deposito)?.PosicaoDeposito;

                                deposito.Nome = lote.Deposito;

                                deposito.NumeroLote = lote.NumeroLote;
                                deposito.Validade = validades.FirstOrDefault(x => x.CodMaterial.ToFormatCode() == lote.CodMaterial.ToFormatCode() && x.NumeroLote == lote.NumeroLote)?.Validade;

                                material.Depositos.Add(deposito);
                            }
                        }
                        else
                        {
                            foreach (var dep in deps)
                            {
                                var deposito = new Deposito();

                                deposito.CodMaterial = dep.CodMaterial;

                                deposito.QuantidadeEstoqueSAP = dep.QuantidadeEstoqueSAP;
                                deposito.PosicaoDeposito = dep.PosicaoDeposito;

                                deposito.Nome = dep.Nome;

                                material.Depositos.Add(deposito);
                            }
                        }

                        if (material.Depositos.Any())
                        {
                            var depositoDefault = new Deposito();

                            depositoDefault = (material.Depositos.OrderBy(x => x.Validade).FirstOrDefault()); //define o mais antigo como padrão

                            if (depositoDefault != null)
                            {
                                if (material.NumeroLote.HasValue())
                                {
                                    if (material.NumeroLote != depositoDefault.NumeroLote || depositoDefault.QuantidadeEstoqueSAP < material.Quantidade)
                                        material.NaoPodeAtender = true;
                                    else
                                        material.Depositos.ToList().RemoveAll(x => x.NumeroLote != material.NumeroLote);
                                }

                                material.PosicaoDeposito = depositoDefault.PosicaoDeposito;
                                material.NumeroLote = depositoDefault.NumeroLote;
                                material.Validade = depositoDefault.Validade;
                                material.Deposito = depositoDefault.Nome;
                            }
                        }
                    }

                    materiais.RemoveAll(x => !x.Depositos.Any() || x.Quantidade < 1);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }

            return materiais;
        }

        public string AtenderPorReserva(IEnumerable<MaterialAtendimento> materiais)
        {
            var retorno = new StringBuilder();
            string function = "BAPI_TRANSACTION_COMMIT";

            try
            {
                connection = OpenConnection();

                #region VERIFICAR QUAIS ESTÃO EM INVENTÁRIO ATIVO

                var codigos = string.Join(",", materiais.Select(x => "'" + x.CodMaterial.ToFormatCode() + "'"));
                var depositos = string.Join(",", materiais.Select(x => "'" + x.Deposito + "'"));
                var codMateriaisInventarioAtivo = new List<string>();

                ReadTable mard = new ReadTable(connection);

                mard.AddField("MATNR");
                mard.AddField("SPERR");
                mard.WhereClause = $"WERKS = '{plant}' AND MATNR IN ({codigos}) AND LGORT IN ({depositos})";
                mard.TableName = "MARD";
                mard.Run();

                foreach (DataRow mardRow in mard.Result.Rows)
                {
                    var inventarioAtivo = !string.IsNullOrWhiteSpace(Convert.ToString(mard.Result.Rows[0]["SPERR"]));

                    var codMaterial = Convert.ToString(mardRow["MATNR"]);

                    if (inventarioAtivo)
                        codMateriaisInventarioAtivo.Add(codMaterial);
                }

                #endregion


                RFCFunction functionBAPI = connection.CreateFunction("BAPI_GOODSMVT_CREATE");

                RFCStructure GOODSMVT_HEADER = functionBAPI.Exports["GOODSMVT_HEADER"].ToStructure();

                RFCStructure GOODSMVT_CODE = functionBAPI.Exports["GOODSMVT_CODE"].ToStructure();
                RFCTable GOODSMVT_ITEM = functionBAPI.Tables["GOODSMVT_ITEM"];
                RFCStructure GOODSMVT_ITEM_LINHA = new RFCStructure();

                GOODSMVT_HEADER["PSTNG_DATE"] = DateTime.Now.ToString("yyyyMMdd");
                GOODSMVT_HEADER["DOC_DATE"] = DateTime.Now.ToString("yyyyMMdd");
                GOODSMVT_HEADER["HEADER_TXT"] = "ALMOXARIFADO";
                GOODSMVT_HEADER["REF_DOC_NO"] = "RESERVA";
                GOODSMVT_HEADER["REF_DOC_NO_LONG"] = "ALMOXARIFADO AUTOMAÇÃO";

                GOODSMVT_CODE["GM_CODE"] = "03";

                foreach (var material in materiais)
                {
                    if (codMateriaisInventarioAtivo.Contains(material.CodMaterial))
                    {
                        material.Situacao = Situacao.PENDENTE;
                        material.TipoAtendimento = TipoAtendimento.RESERVA;
                        material.DataHoraFim = DateTime.Now;
                        material.RetornoSap = "OK: Material em inventário físico ativo!";
                        continue; //pulo do gato
                    }

                    GOODSMVT_ITEM_LINHA = GOODSMVT_ITEM.AddRow();

                    GOODSMVT_ITEM_LINHA["MATERIAL"] = material.CodMaterial.ToFormatCode();
                    GOODSMVT_ITEM_LINHA["PLANT"] = plant;
                    GOODSMVT_ITEM_LINHA["STGE_LOC"] = material.Deposito ?? "";
                    GOODSMVT_ITEM_LINHA["MOVE_TYPE"] = BAIXA_DE_MATERIAL;
                    GOODSMVT_ITEM_LINHA["RESERV_NO"] = material.NumeroReserva;
                    GOODSMVT_ITEM_LINHA["RES_ITEM"] = material.Sequencia;
                    GOODSMVT_ITEM_LINHA["ENTRY_QNT"] = material.QuantidadeRetirada;
                    GOODSMVT_ITEM_LINHA["GR_RCPT"] = material.UsuarioAtendido;
                    GOODSMVT_ITEM_LINHA["GR_RCPTX"] = "X";

                    if (material.TemLote)
                        GOODSMVT_ITEM_LINHA["BATCH"] = material.NumeroLote;

                    functionBAPI.Exports["GOODSMVT_HEADER"].ParamValue = GOODSMVT_HEADER;
                    functionBAPI.Exports["GOODSMVT_CODE"].ParamValue = GOODSMVT_CODE;

                    functionBAPI.Tables["GOODSMVT_ITEM"] = GOODSMVT_ITEM;

                    material.Situacao = Situacao.CONCLUIDO;
                    material.TipoAtendimento = TipoAtendimento.RESERVA;
                    material.DataHoraFim = DateTime.Now;

                }//end foreach

                functionBAPI.Execute();

                RFCStructure GOODSMVT_HEADRET = functionBAPI.Imports["GOODSMVT_HEADRET"].ToStructure();

                if (!string.IsNullOrWhiteSpace(Convert.ToString(GOODSMVT_HEADRET["MAT_DOC"])))
                {
                    retorno.Append($"OK:{GOODSMVT_HEADRET["MAT_DOC"]}");
                }
                else
                {
                    function = "BAPI_TRANSACTION_ROLLBACK";
                    retorno.AppendLine("Mensagem do SAP:");

                    foreach (RFCStructure row in functionBAPI.Tables["RETURN"].Rows)
                    {
                        retorno.AppendLine(Convert.ToString(row["MESSAGE"]));
                    }
                }
            }
            catch (Exception ex)
            {
                function = "BAPI_TRANSACTION_ROLLBACK";
                retorno = new StringBuilder(ex.GetInnerExceptionMessage());
            }
            finally
            {
#if !DEBUG
                connection.CreateFunction(function).Execute();
#endif
                connection.Close();
            }

            return retorno.ToString();
        }

        public IEnumerable<MaterialAtendimento> BuscarTransferencia(string[] codMateriais)
        {
            var materiais = new List<MaterialAtendimento>();

            try
            {
                connection = OpenConnection();

                if (codMateriais.IsNotNull())
                {
                    var function = connection.CreateFunction("ZMM_SP_MATERIAL_GET_LIST");

                    function.Exports["PI_WERKS"].ParamValue = plant;

                    #region FILTRO DE DEPÓSITO

                    RFCTable STORAGELOCATION = function.Tables["STORAGELOCATION"];

                    RFCStructure STORAGELOCATION_ITEM_LINHA = new RFCStructure();

                    var depositoExcluidos = new string[] { Deposito.VENCIDOS, Deposito.TRANSFERENCIA, Deposito.DIVERGENCIA };

                    foreach (var deposito in depositoExcluidos)
                    {
                        STORAGELOCATION_ITEM_LINHA = STORAGELOCATION.AddRow();

                        STORAGELOCATION_ITEM_LINHA["SIGN"] = "E";
                        STORAGELOCATION_ITEM_LINHA["OPTION"] = "EQ";
                        STORAGELOCATION_ITEM_LINHA["STLOC_LOW"] = deposito;
                        STORAGELOCATION_ITEM_LINHA["STLOC_HIGH"] = deposito;
                    }

                    function.Tables["STORAGELOCATION"] = STORAGELOCATION;

                    #endregion

                    #region FILTRO DE MATERIAL

                    RFCTable MATERIALSELECTION = function.Tables["MATERIALSELECTION"];

                    RFCStructure MATERIALSELECTION_ITEM_LINHA = new RFCStructure();

                    foreach (var codMaterial in codMateriais)
                    {
                        MATERIALSELECTION_ITEM_LINHA = MATERIALSELECTION.AddRow();

                        MATERIALSELECTION_ITEM_LINHA["SIGN"] = "I";
                        MATERIALSELECTION_ITEM_LINHA["OPTION"] = "EQ";
                        MATERIALSELECTION_ITEM_LINHA["MATNR_LOW"] = codMaterial.ToFormatCode();
                        MATERIALSELECTION_ITEM_LINHA["MATNR_HIGH"] = codMaterial.ToFormatCode();
                    }

                    function.Tables["MATERIALSELECTION"] = MATERIALSELECTION;

                    #endregion

                    function.Execute();

                    RFCTable MATERIALLIST = function.Tables["MATERIALLIST"];

                    var depositos = new List<MaterialAtendimento>();

                    foreach (RFCStructure row in MATERIALLIST.Rows)
                    {
                        var material = new MaterialAtendimento();

                        material.CodMaterial = Convert.ToString(row["MATNR"]);
                        material.Nome = Convert.ToString(row["MAKTX"]);
                        material.QuantidadeEstoqueSAP = Convert.ToDouble(row["LABST"], new CultureInfo("en-US"));
                        material.UnidadeMedida = Convert.ToString(row["MSEH3"]);
                        material.Deposito = Convert.ToString(row["LGORT"]);
                        material.PosicaoDeposito = Convert.ToString(row["LGPBE"]);
                        material.Critico = !string.IsNullOrWhiteSpace(Convert.ToString(row["KZKRI"]));

                        material.DataHoraInicio = DateTime.Now;

                        depositos.Add(material);
                    }

                    RFCTable BATCHLIST = function.Tables["BATCHLIST"];

                    var lotes = new List<MaterialAtendimento>();

                    foreach (RFCStructure mchbRow in BATCHLIST.Rows)
                    {
                        var lote = new MaterialAtendimento();

                        lote.CodMaterial = Convert.ToString(mchbRow["MATNR"]);
                        lote.NumeroLote = Convert.ToString(mchbRow["CHARG"]);
                        lote.QuantidadeEstoqueSAP = Convert.ToDouble(mchbRow["CLABS"], new CultureInfo("en-US"));
                        lote.Deposito = Convert.ToString(mchbRow["LGORT"]);

                        var data = Convert.ToString(mchbRow["VFDAT"]);
                        try { lote.Validade = DateTime.ParseExact(data, "yyyyMMdd", new CultureInfo("pt-BR")); } catch { }

                        lotes.Add(lote);
                    }

                    foreach (var deposito in depositos)
                    {
                        var lots = lotes.Where(x => x.CodMaterial.ToFormatCode() == deposito.CodMaterial.ToFormatCode());

                        if (lots.Any())
                        {
                            foreach (var lote in lots)
                            {
                                var material = new MaterialAtendimento();

                                material.Nome = deposito.Nome;
                                material.CodMaterial = lote.CodMaterial;
                                material.QuantidadeEstoqueSAP = lote.QuantidadeEstoqueSAP;
                                material.Deposito = lote.Deposito;
                                material.Critico = deposito.Critico;
                                material.UnidadeMedida = deposito.UnidadeMedida;
                                material.DataHoraInicio = DateTime.Now;
                                material.PosicaoDeposito = deposito.PosicaoDeposito;

                                material.NumeroLote = lote.NumeroLote;

                                material.Depositos.Add(new Deposito
                                {
                                    Nome = material.Deposito,
                                    NumeroLote = material.NumeroLote,
                                    PosicaoDeposito = material.PosicaoDeposito,
                                    QuantidadeEstoqueSAP = material.QuantidadeEstoqueSAP,
                                    Validade = material.Validade
                                });

                                materiais.Add(material);
                            }
                        }
                        else
                        {
                            var material = new MaterialAtendimento();

                            material.Nome = deposito.Nome;
                            material.CodMaterial = deposito.CodMaterial;
                            material.QuantidadeEstoqueSAP = deposito.QuantidadeEstoqueSAP;
                            material.PosicaoDeposito = deposito.PosicaoDeposito;
                            material.Deposito = deposito.Deposito;
                            material.Critico = deposito.Critico;
                            material.UnidadeMedida = deposito.UnidadeMedida;
                            material.DataHoraInicio = DateTime.Now;

                            material.Depositos.Add(new Deposito
                            {
                                Nome = material.Deposito,
                                NumeroLote = material.NumeroLote,
                                PosicaoDeposito = material.PosicaoDeposito,
                                QuantidadeEstoqueSAP = material.QuantidadeEstoqueSAP,
                                Validade = material.Validade
                            });

                            materiais.Add(material);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }

            return materiais;
        }

        public string AtenderPorTransferencia(IEnumerable<MaterialAtendimento> materiais, string depositoDestino = Deposito.TRANSFERENCIA)
        {
            var retorno = new StringBuilder();
            string function = "BAPI_TRANSACTION_COMMIT";

            try
            {
                connection = OpenConnection();

                RFCFunction functionBAPI = connection.CreateFunction("BAPI_GOODSMVT_CREATE");

                RFCStructure GOODSMVT_HEADER = functionBAPI.Exports["GOODSMVT_HEADER"].ToStructure();

                RFCStructure GOODSMVT_CODE = functionBAPI.Exports["GOODSMVT_CODE"].ToStructure();
                RFCTable GOODSMVT_ITEM = functionBAPI.Tables["GOODSMVT_ITEM"];
                RFCStructure GOODSMVT_ITEM_LINHA = new RFCStructure();

                GOODSMVT_HEADER["PSTNG_DATE"] = DateTime.Now.ToString("yyyyMMdd");
                GOODSMVT_HEADER["DOC_DATE"] = DateTime.Now.ToString("yyyyMMdd");
                GOODSMVT_HEADER["HEADER_TXT"] = "ALMOXARIFADO";
                GOODSMVT_HEADER["REF_DOC_NO"] = "TRANSFERÊNCIA";
                GOODSMVT_HEADER["REF_DOC_NO_LONG"] = "ALMOXARIFADO AUTOMAÇÃO";

                GOODSMVT_CODE["GM_CODE"] = "04";

                foreach (var material in materiais)
                {
                    GOODSMVT_ITEM_LINHA = GOODSMVT_ITEM.AddRow();

                    GOODSMVT_ITEM_LINHA["MATERIAL"] = material.CodMaterial.ToFormatCode();
                    GOODSMVT_ITEM_LINHA["PLANT"] = plant;
                    GOODSMVT_ITEM_LINHA["STGE_LOC"] = material.Deposito; //ORIGEM
                    GOODSMVT_ITEM_LINHA["MOVE_TYPE"] = TRASFERENCIA_DE_DEPOSITO;
                    GOODSMVT_ITEM_LINHA["MOVE_STLOC"] = depositoDestino; //DESTINO
                    GOODSMVT_ITEM_LINHA["ENTRY_QNT"] = material.QuantidadeRetirada;

                    if (material.TemLote)
                        GOODSMVT_ITEM_LINHA["BATCH"] = material.NumeroLote;

                    functionBAPI.Exports["GOODSMVT_HEADER"].ParamValue = GOODSMVT_HEADER;
                    functionBAPI.Exports["GOODSMVT_CODE"].ParamValue = GOODSMVT_CODE;

                    functionBAPI.Tables["GOODSMVT_ITEM"] = GOODSMVT_ITEM;

                    material.Situacao = Situacao.CONCLUIDO;
                    material.TipoAtendimento = TipoAtendimento.TRANSFERENCIA;
                    material.DataHoraFim = DateTime.Now;
                }//end foreach

                functionBAPI.Execute();

                RFCStructure GOODSMVT_HEADRET = functionBAPI.Imports["GOODSMVT_HEADRET"].ToStructure();

                if (!string.IsNullOrWhiteSpace(Convert.ToString(GOODSMVT_HEADRET["MAT_DOC"])))
                {
                    retorno.Append($"OK:{GOODSMVT_HEADRET["MAT_DOC"]}");
                }
                else
                {
                    function = "BAPI_TRANSACTION_ROLLBACK";
                    retorno.AppendLine("Mensagem do SAP:");

                    foreach (RFCStructure row in functionBAPI.Tables["RETURN"].Rows)
                    {
                        retorno.AppendLine(Convert.ToString(row["MESSAGE"]));
                    }
                }

            }
            catch (Exception ex)
            {
                function = "BAPI_TRANSACTION_ROLLBACK";
                retorno = new StringBuilder(ex.GetInnerExceptionMessage());
            }
            finally
            {
#if !DEBUG
                connection.CreateFunction(function).Execute();
#endif
                connection.Close();
            }

            return retorno.ToString();
        }

        #endregion

        public MaterialArmazenagem BuscarMaterialDetalhe(string codMaterial)
        {
            var material = new MaterialArmazenagem();

            try
            {
                connection = OpenConnection();

                var function = connection.CreateFunction("ZMM_SP_MATERIAL_GET_LIST");

                function.Exports["PI_WERKS"].ParamValue = plant;

                #region FILTRO DE MATERIAL

                RFCTable MATERIALSELECTION = function.Tables["MATERIALSELECTION"];

                RFCStructure MATERIALSELECTION_ITEM_LINHA = new RFCStructure();

                MATERIALSELECTION_ITEM_LINHA = MATERIALSELECTION.AddRow();

                MATERIALSELECTION_ITEM_LINHA["SIGN"] = "I";
                MATERIALSELECTION_ITEM_LINHA["OPTION"] = "EQ";
                MATERIALSELECTION_ITEM_LINHA["MATNR_LOW"] = codMaterial.ToFormatCode();
                MATERIALSELECTION_ITEM_LINHA["MATNR_HIGH"] = codMaterial.ToFormatCode();

                function.Tables["MATERIALSELECTION"] = MATERIALSELECTION;

                #endregion

                function.Execute();

                RFCTable MATERIALLIST = function.Tables["MATERIALLIST"];

                if (MATERIALLIST.Rows.Count > 0)
                    foreach (RFCStructure row in MATERIALLIST.Rows)
                    {
                        material.CodMaterial = Convert.ToString(row["MATNR"]);
                        material.Nome = Convert.ToString(row["MAKTX"]);
                        material.UnidadeMedida = Convert.ToString(row["MSEH3"]);
                    }
                else
                    material = null;

                if (material != null)
                {
                    RFCTable BATCHLIST = function.Tables["BATCHLIST"];

                    var lotes = new List<Lote>();

                    foreach (RFCStructure mchbRow in BATCHLIST.Rows)
                    {
                        var lote = new Lote();

                        lote.NumeroLote = Convert.ToString(mchbRow["CHARG"]);

                        var data = Convert.ToString(mchbRow["VFDAT"]);
                        try { lote.Validade = DateTime.ParseExact(data, "yyyyMMdd", new CultureInfo("pt-BR")); } catch { }

                        lotes.Add(lote);
                    }

                    material.Lotes = lotes;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }

            return material;
        }

        public bool VerificarInventarioAtivo(string deposito, string codMaterial)
        {
            try
            {
                connection = OpenConnection();

                var function = connection.CreateFunction("BAPI_MATPHYSINV_GETITEMS");

                #region filtro de planta

                RFCTable PLANT_RA = function.Tables["PLANT_RA"];

                RFCStructure PLANT_RA_LINHA = new RFCStructure();

                PLANT_RA_LINHA = PLANT_RA.AddRow();

                PLANT_RA_LINHA["SIGN"] = "I";
                PLANT_RA_LINHA["OPTION"] = "EQ";
                PLANT_RA_LINHA["LOW"] = plant;
                PLANT_RA_LINHA["HIGH"] = plant;

                function.Tables["PLANT_RA"] = PLANT_RA;

                #endregion

                #region filtro de depósito

                RFCTable STGE_LOC_RA = function.Tables["STGE_LOC_RA"];

                RFCStructure STGE_LOC_RA_LINHA = new RFCStructure();

                STGE_LOC_RA_LINHA = STGE_LOC_RA.AddRow();

                STGE_LOC_RA_LINHA["SIGN"] = "I";
                STGE_LOC_RA_LINHA["OPTION"] = "EQ";
                STGE_LOC_RA_LINHA["LOW"] = deposito;
                STGE_LOC_RA_LINHA["HIGH"] = deposito;

                function.Tables["STGE_LOC_RA"] = STGE_LOC_RA;

                #endregion

                #region filtro de material

                RFCTable MATERIAL_RA = function.Tables["MATERIAL_RA"];

                RFCStructure MATERIAL_RA_LINHA = new RFCStructure();

                MATERIAL_RA_LINHA = MATERIAL_RA.AddRow();

                MATERIAL_RA_LINHA["SIGN"] = "I";
                MATERIAL_RA_LINHA["OPTION"] = "EQ";
                MATERIAL_RA_LINHA["LOW"] = codMaterial.ToFormatCode();
                STGE_LOC_RA_LINHA["HIGH"] = codMaterial.ToFormatCode();

                function.Tables["MATERIAL_RA"] = MATERIAL_RA;

                #endregion

                function.Execute();

                RFCTable ITEMS = function.Tables["ITEMS"];

                return (ITEMS.Rows.Count > 0);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }
    }//end class

}//end namespace
