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
    public class SapIntegrationFacade
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["SAPR3"].ConnectionString;
        private R3Connection connection;
        private const string plant = "CA01";

        public SapIntegrationFacade()
        {
            LIC.SetLic("V922C1ZY97"); //license key - theobald
        }

        private R3Connection OpenConnection()
        {
            if (connection == null)
                connection = new R3Connection(connectionString);

            connection.MultithreadingEnvironment = true;
            connection.Open(false);

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

                function.Exports["PI_NUM_LISTA_CEGA"].ParamValue = numero.PadLeft(10, '0');

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
                    material.NumeroItem = Convert.ToString(materialRow["ITEM_LISTA_CEGA"]);
                    material.CodMaterial = Convert.ToString(materialRow["COD_MATERIAL"]);
                    material.Nome = Convert.ToString(materialRow["DESC_MATERIAL"]);
                    material.Quantidade = Convert.ToDouble(materialRow["QTDE_FORNECIDA"], new CultureInfo("en-US"));
                    material.UnidadeMedida = Convert.ToString(materialRow["UNIDADE_MED"]);
                    material.NumeroPedido = Convert.ToString(materialRow["NUM_PEDIDO"]);
                    material.NumeroPedidoItem = Convert.ToString(materialRow["ITEM_PEDIDO"]);
                    material.Deposito = Convert.ToString(materialRow["DEPOSITO"]);
                    material.Descricao = Convert.ToString(materialRow["TEXTO_MATERIAL"]);
                    material.ClassContabil = Convert.ToString(materialRow["CLASS_CONTABIL"]);
                    material.UnidadeTempo = Convert.ToString(materialRow["VALIDADE"]);
                    material.EmailSolicitante = Convert.ToString(materialRow["MAIL_SOLICITANTE"]);
                    material.TemLote = !string.IsNullOrWhiteSpace(Convert.ToString(materialRow["ADMIN_LOTE_OBRIG"]));

                    if (material.TemLote && string.IsNullOrWhiteSpace(material.UnidadeTempo))
                    {
                        material.NumeroLote = "MANUTENÇÃO";
                    }

                    material.NumeroNota = listarecebimento.NumeroNfe;
                    material.DataHoraInicio = DateTime.Now;

                    listarecebimento.Materiais.Add(material);
                }

                listarecebimento.Materiais = listarecebimento.Materiais.OrderBy(x => x.CodMaterial).ToList();

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

        public string CriarMigo(List<MaterialRecebimento> materiais)
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

#if DEBUG
                GOODSMVT_HEADER["PSTNG_DATE"] = DateTime.Now.ToString("20180201");
                GOODSMVT_HEADER["DOC_DATE"] = DateTime.Now.ToString("20180201");
#else
                GOODSMVT_HEADER["PSTNG_DATE"] = DateTime.Now.ToString("yyyyMMdd");
                GOODSMVT_HEADER["DOC_DATE"] = DateTime.Now.ToString("yyyyMMdd");
#endif

                GOODSMVT_HEADER["HEADER_TXT"] = "ALMOXARIFADO";

                GOODSMVT_CODE["GM_CODE"] = "01";

                string notaFiscal = string.Empty;
                string usuario = string.Empty;

                foreach (var material in materiais)
                {
                    if (material.Lotes.Any())
                    {
                        foreach (var lote in material.Lotes)
                        {
                            material.Quantidade = lote.Quantidade;
                            material.Validade = lote.Validade;
                            material.NumeroLote = lote.NumeroLote;

                            MapearMaterial(GOODSMVT_ITEM, material);

                            if (string.IsNullOrWhiteSpace(notaFiscal) && !string.IsNullOrWhiteSpace(material.NumeroNota))
                                notaFiscal = material.NumeroNota;

                            if (string.IsNullOrWhiteSpace(usuario) && !string.IsNullOrWhiteSpace(material.Usuario))
                                usuario = material.Usuario;

                        }
                    }
                    else
                    {
                        MapearMaterial(GOODSMVT_ITEM, material);

                        if (string.IsNullOrWhiteSpace(notaFiscal) && !string.IsNullOrWhiteSpace(material.NumeroNota))
                            notaFiscal = material.NumeroNota;

                        if (string.IsNullOrWhiteSpace(usuario) && !string.IsNullOrWhiteSpace(material.Usuario))
                            usuario = material.Usuario;
                    }
                }

                GOODSMVT_HEADER["REF_DOC_NO_LONG"] = notaFiscal;
                GOODSMVT_HEADER["PR_UNAME"] = usuario;

                functionBAPI.Exports["GOODSMVT_HEADER"].ParamValue = GOODSMVT_HEADER;
                functionBAPI.Exports["GOODSMVT_CODE"].ParamValue = GOODSMVT_CODE;
                functionBAPI.Exports["TESTRUN"].ParamValue = "";

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
                connection.CreateFunction(function).Execute();
                connection.Close();
            }

            return retorno.ToString();
        }

        private void MapearMaterial(RFCTable GOODSMVT_ITEM, MaterialRecebimento material)
        {
            RFCStructure GOODSMVT_ITEM_LINHA = new RFCStructure();

            GOODSMVT_ITEM_LINHA = GOODSMVT_ITEM.AddRow();

            GOODSMVT_ITEM_LINHA["MATERIAL"] = material.CodMaterial.ToFormatCode();
            GOODSMVT_ITEM_LINHA["PLANT"] = plant;
            GOODSMVT_ITEM_LINHA["STGE_LOC"] = material.Deposito;
            GOODSMVT_ITEM_LINHA["UNLOAD_PT"] = material.PosicaoDeposito;
            GOODSMVT_ITEM_LINHA["UNLOAD_PTX"] = "X";
            GOODSMVT_ITEM_LINHA["MOVE_TYPE"] = "101"; //ENTRADA DE MATERIAL
            GOODSMVT_ITEM_LINHA["ENTRY_QNT"] = material.Quantidade.Value.ToFormatString();
            GOODSMVT_ITEM_LINHA["PO_NUMBER"] = material.NumeroPedido?.PadLeft(10, '0');
            GOODSMVT_ITEM_LINHA["PO_ITEM"] = material.NumeroPedidoItem?.PadLeft(5, '0');
            GOODSMVT_ITEM_LINHA["PO_PR_QNT"] = material.Quantidade.Value.ToFormatString();

            //GOODSMVT_ITEM_LINHA["ENTRY_UOM"] = material.UnidadeMedida;
            //GOODSMVT_ITEM_LINHA["ENTRY_UOM_ISO"] = material.UnidadeMedida;
            //GOODSMVT_ITEM_LINHA["ORDERPR_UN"] = material.UnidadeMedida;
            //GOODSMVT_ITEM_LINHA["ORDERPR_UN_ISO"] = material.UnidadeMedida;

            if (material.TemDivergencia)
                GOODSMVT_ITEM_LINHA["MOVE_REAS"] = "02";

            if (material.TemLote && string.IsNullOrWhiteSpace(material.UnidadeTempo) && string.IsNullOrWhiteSpace(material.NumeroLote))
                material.NumeroLote = "MANUTENÇÃO";

            if (!string.IsNullOrWhiteSpace(material.NumeroLote))
                GOODSMVT_ITEM_LINHA["BATCH"] = material.NumeroLote;

            if (material.Validade.HasValue)
                GOODSMVT_ITEM_LINHA["EXPIRYDATE"] = material.Validade.Value.ToString("yyyyMMdd");

            GOODSMVT_ITEM_LINHA["MVT_IND"] = "B";
            GOODSMVT_ITEM_LINHA["STCK_TYPE"] = "x";
        }

        /// <summary>
        /// Usado somente para teste
        /// </summary>
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
#if DEBUG
                functionBAPI.Exports["GOODSMVT_PSTNG_DATE"].ParamValue = DateTime.Now.ToString("20180201");
#else
                functionBAPI.Exports["GOODSMVT_PSTNG_DATE"].ParamValue = DateTime.Now.ToString("yyyyMMdd");
#endif

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

        public MaterialRecebimento ConsultarDocumento(int ano, string documento, string item)
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
                    var itemDoc = Convert.ToString(materialRow["MATDOC_ITM"]).PadLeft(4, '0');

                    if (itemDoc == item.PadLeft(4, '0'))
                    {
                        material = new MaterialRecebimento();

                        material.Ano = ano;
                        material.NumeroDocumento = documento;
                        material.NumeroItem = itemDoc;

                        var data = Convert.ToString(header["ENTRY_DATE"]);  //header
                        try { material.DataEntrada = DateTime.ParseExact(data, "yyyyMMdd", new CultureInfo("pt-BR")); } catch { }

                        material.NumeroNota = Convert.ToString(header["REF_DOC_NO_LONG"]); //header
                        material.Usuario = Convert.ToString(header["USERNAME"]); //header

                        material.CodMaterial = Convert.ToString(materialRow["MATERIAL"]);
                        material.NumeroPedido = Convert.ToString(materialRow["PO_NUMBER"]);
                        material.NumeroPedidoItem = Convert.ToString(materialRow["PO_ITEM"]);
                        material.Centro = Convert.ToString(materialRow["PLANT"]);
                        material.PosicaoDeposito = Convert.ToString(materialRow["UNLOAD_PT"]);
                        material.NumeroLote = Convert.ToString(materialRow["BATCH"]);
                        material.Deposito = Convert.ToString(materialRow["STGE_LOC"]);
                        material.Fornecedor = Convert.ToString(materialRow["VENDOR"]);
                        material.Quantidade = Convert.ToDouble(materialRow["ENTRY_QNT"], new CultureInfo("en-US"));
                        material.UnidadeMedida = Convert.ToString(materialRow["ENTRY_UOM"]);
                        material.TipoMovimento = Convert.ToString(materialRow["MOVE_TYPE"]);
                        material.UsuarioAtendido = Convert.ToString(materialRow["GR_RCPT"]);
                        material.Ordem = Convert.ToString(materialRow["ORDERID"]);

                        if (string.IsNullOrEmpty(material.Deposito))
                            material.DepositoEtiqueta = "COMPRA DIRETA";
                        else if (material.Deposito == "DM14")
                            material.DepositoEtiqueta = "DIVERGÊNCIA";
                        else
                            material.DepositoEtiqueta = "ARMAZENAGEM";

                        var function = connection.CreateFunction("ZMM_SP_MATERIAL_GET_LIST");

                        function.Exports["PI_WERKS"].ParamValue = plant;

                        #region FILTRO DE MATERIAL

                        RFCTable MATERIALSELECTION = function.Tables["MATERIALSELECTION"];

                        RFCStructure MATERIALSELECTION_ITEM_LINHA = new RFCStructure();

                        MATERIALSELECTION_ITEM_LINHA = MATERIALSELECTION.AddRow();

                        MATERIALSELECTION_ITEM_LINHA["SIGN"] = "I";
                        MATERIALSELECTION_ITEM_LINHA["OPTION"] = "EQ";
                        MATERIALSELECTION_ITEM_LINHA["MATNR_LOW"] = material.CodMaterial.ToFormatCode();
                        MATERIALSELECTION_ITEM_LINHA["MATNR_HIGH"] = material.CodMaterial.ToFormatCode();

                        function.Tables["MATERIALSELECTION"] = MATERIALSELECTION;

                        #endregion

                        function.Execute();

                        RFCTable MATERIALLIST = function.Tables["MATERIALLIST"];

                        foreach (RFCStructure row in MATERIALLIST.Rows)
                        {
                            material.Nome = Convert.ToString(row["MAKTX"]);
                            material.CondicaoArmazenagem = Convert.ToString(row["RBTXT"]);
                            material.CondicaoTemperatura = Convert.ToString(row["TBTXT"]);
                        }

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

                        if (string.IsNullOrWhiteSpace(material.Nome))
                        {
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

            return material;
        }

        #endregion

        #region ARMAZENAGEM

        public List<MaterialArmazenagem> ListarPendentesArmazenagem(string depositoOrigem = "DM15")
        {
            var materiais = new List<MaterialArmazenagem>();

            try
            {
                connection = OpenConnection();

                var function = connection.CreateFunction("ZMM_SP_MATERIAL_GET_LIST");

                function.Exports["PI_WERKS"].ParamValue = plant;

                #region FILTRO DE DEPÓSITO

                RFCTable STORAGELOCATION = function.Tables["STORAGELOCATION"];

                RFCStructure STORAGELOCATION_ITEM_LINHA = new RFCStructure();

                STORAGELOCATION_ITEM_LINHA = STORAGELOCATION.AddRow();

                STORAGELOCATION_ITEM_LINHA["SIGN"] = "I";
                STORAGELOCATION_ITEM_LINHA["OPTION"] = "EQ";
                STORAGELOCATION_ITEM_LINHA["STLOC_LOW"] = depositoOrigem;
                STORAGELOCATION_ITEM_LINHA["STLOC_HIGH"] = depositoOrigem;

                function.Tables["STORAGELOCATION"] = STORAGELOCATION;

                #endregion

                function.Execute();

                RFCTable MATERIALLIST = function.Tables["MATERIALLIST"];

                foreach (RFCStructure row in MATERIALLIST.Rows)
                {
                    var material = new MaterialArmazenagem();

                    material.CodMaterial = Convert.ToString(row["MATNR"]);
                    material.Nome = Convert.ToString(row["MAKTX"]);
                    material.Quantidade = Convert.ToDouble(row["LABST"], new CultureInfo("en-US"));
                    material.UnidadeMedida = Convert.ToString(row["MSEH3"]);

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

        public MaterialArmazenagem BuscarMaterialPendenteArmazenagem(string codMaterial, string depositoOrigem = "DM15")
        {
            var material = new MaterialArmazenagem();

            try
            {
                connection = OpenConnection();

                var function = connection.CreateFunction("ZMM_SP_MATERIAL_GET_LIST");

                function.Exports["PI_WERKS"].ParamValue = plant;

                #region FILTRO DE DEPÓSITO

                RFCTable STORAGELOCATION = function.Tables["STORAGELOCATION"];

                RFCStructure STORAGELOCATION_ITEM_LINHA = new RFCStructure();

                STORAGELOCATION_ITEM_LINHA = STORAGELOCATION.AddRow();

                STORAGELOCATION_ITEM_LINHA["SIGN"] = "I";
                STORAGELOCATION_ITEM_LINHA["OPTION"] = "EQ";
                STORAGELOCATION_ITEM_LINHA["STLOC_LOW"] = depositoOrigem;
                STORAGELOCATION_ITEM_LINHA["STLOC_HIGH"] = depositoOrigem;

                function.Tables["STORAGELOCATION"] = STORAGELOCATION;

                #endregion

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
                        material.Quantidade = Convert.ToDouble(row["LABST"], new CultureInfo("en-US"));

                        if (material.Quantidade == 0)
                            return null;

                        material.CodMaterial = Convert.ToString(row["MATNR"]);
                        material.Nome = Convert.ToString(row["MAKTX"]);
                        material.UnidadeMedida = Convert.ToString(row["MSEH3"]);
                        material.Deposito = Convert.ToString(row["LGFSB"]);
                        material.CondicaoArmazenagem = Convert.ToString(row["RBTXT"]);
                        material.CondicaoTemperatura = Convert.ToString(row["TBTXT"]);

                        material.DataHoraInicio = DateTime.Now;
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

                        lote.CodMaterial = Convert.ToString(mchbRow["MATNR"]);
                        lote.NumeroLote = Convert.ToString(mchbRow["CHARG"]);
                        lote.Quantidade = Convert.ToDouble(mchbRow["CLABS"], new CultureInfo("en-US"));
                        lote.Deposito = Convert.ToString(mchbRow["LGORT"]);

                        var data = Convert.ToString(mchbRow["VFDAT"]);
                        try { lote.Validade = DateTime.ParseExact(data, "yyyyMMdd", new CultureInfo("pt-BR")); } catch { }

                        if (lote.Quantidade > 0)
                            lotes.Add(lote);
                    }

                    material.Lotes = lotes;

                    #region BUSCAR ESTOQUE ATUAL E ENDEREÇO PADRÃO

                    if (!string.IsNullOrWhiteSpace(material.Deposito))
                    {
                        ReadTable mard = new ReadTable(connection);

                        mard.AddField("LGPBE");
                        mard.AddField("LABST");
                        mard.AddCriteria($"MATNR = '{ material.CodMaterial.ToFormatCode()}'");
                        mard.AddCriteria($"AND WERKS = '{plant}'");
                        mard.AddCriteria($"AND LGORT = '{material.Deposito}'");
                        mard.TableName = "MARD";
                        mard.RowCount = 1;

                        mard.Run();

                        if (mard.Result.Rows.Count > 0)
                        {
                            material.QuantidadeEstoqueSAP = Convert.ToDouble(mard.Result.Rows[0]["LABST"], new CultureInfo("en-US"));
                            material.PosicaoDeposito = Convert.ToString(mard.Result.Rows[0]["LGPBE"]);
                        }
                    }

                    #endregion
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

        public string Armazenar(MaterialArmazenagem material, string depositoOrigem = "DM15")
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

                if (material.Lotes.Any())
                {
                    foreach (var lote in material.Lotes)
                    {
                        GOODSMVT_ITEM_LINHA = GOODSMVT_ITEM.AddRow();

                        GOODSMVT_ITEM_LINHA["MATERIAL"] = material.CodMaterial.ToFormatCode();
                        GOODSMVT_ITEM_LINHA["PLANT"] = plant;
                        GOODSMVT_ITEM_LINHA["STGE_LOC"] = depositoOrigem;
                        GOODSMVT_ITEM_LINHA["UNLOAD_PT"] = material.PosicaoDeposito;
                        GOODSMVT_ITEM_LINHA["UNLOAD_PTX"] = "X";
                        GOODSMVT_ITEM_LINHA["MOVE_TYPE"] = "311"; //TRANSFERÊNCIA DE DEPÓSITO
                        GOODSMVT_ITEM_LINHA["MOVE_STLOC"] = material.Deposito;

                        GOODSMVT_ITEM_LINHA["ENTRY_QNT"] = lote.Quantidade;
                        GOODSMVT_ITEM_LINHA["BATCH"] = lote.NumeroLote;
                    }
                }
                else
                {
                    GOODSMVT_ITEM_LINHA = GOODSMVT_ITEM.AddRow();

                    GOODSMVT_ITEM_LINHA["MATERIAL"] = material.CodMaterial.ToFormatCode();
                    GOODSMVT_ITEM_LINHA["PLANT"] = plant;
                    GOODSMVT_ITEM_LINHA["STGE_LOC"] = depositoOrigem;
                    GOODSMVT_ITEM_LINHA["UNLOAD_PT"] = material.PosicaoDeposito;
                    GOODSMVT_ITEM_LINHA["UNLOAD_PTX"] = "X";
                    GOODSMVT_ITEM_LINHA["MOVE_TYPE"] = "311"; //TRANSFERÊNCIA DE DEPÓSITO
                    GOODSMVT_ITEM_LINHA["MOVE_STLOC"] = material.Deposito;
                    GOODSMVT_ITEM_LINHA["ENTRY_QNT"] = material.Quantidade.Value.ToFormatString();
                }

                functionBAPI.Exports["GOODSMVT_HEADER"].ParamValue = GOODSMVT_HEADER;
                functionBAPI.Exports["GOODSMVT_CODE"].ParamValue = GOODSMVT_CODE;

                functionBAPI.Tables["GOODSMVT_ITEM"] = GOODSMVT_ITEM;

                functionBAPI.Execute();

                RFCStructure GOODSMVT_HEADRET = functionBAPI.Imports["GOODSMVT_HEADRET"].ToStructure();

                if (!string.IsNullOrWhiteSpace(Convert.ToString(GOODSMVT_HEADRET["MAT_DOC"])))
                {
                    retorno.Append($"OK:{GOODSMVT_HEADRET["MAT_DOC"]}");
                    //AlterarPosicao(material, connection); //TODO: TESTAR SE FUNCIONA SEM
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
                connection.CreateFunction(function).Execute();
                connection.Close();
            }

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
                mard.AddField("LABST");
                mard.AddField("LGORT");
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
                    throw new Exception("A contagem dessa lista já foi realizada!");

                var tabelaMateriais = function.Tables["ITEMS"];

                foreach (RFCStructure row in tabelaMateriais.Rows)
                {
                    if (string.IsNullOrEmpty(Convert.ToString(row["COUNTED"]))) //se não foi contado
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
                connection.CreateFunction(function).Execute();
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
                    mchb.WhereClause = $"WERKS = '{plant}' AND MATNR IN ({codigos}) AND CLABS > 0 AND LGORT <> 'DMMV' AND LGORT <> 'DM04' AND LGORT <> 'DM14'";
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
                    mard.WhereClause = $"WERKS = '{plant}' AND MATNR IN ({codigos}) AND LABST > 0 AND LGORT <> 'DMMV' AND LGORT <> 'DM04' AND LGORT <> 'DM14'";
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
                    GOODSMVT_ITEM_LINHA = GOODSMVT_ITEM.AddRow();

                    GOODSMVT_ITEM_LINHA["MATERIAL"] = material.CodMaterial.ToFormatCode();
                    GOODSMVT_ITEM_LINHA["PLANT"] = plant;
                    GOODSMVT_ITEM_LINHA["STGE_LOC"] = material.Deposito ?? "";
                    GOODSMVT_ITEM_LINHA["MOVE_TYPE"] = "991"; //BAIXA DE MATERIAL
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
                connection.CreateFunction(function).Execute();
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

                if (codMateriais.HasValue())
                {
                    var function = connection.CreateFunction("ZMM_SP_MATERIAL_GET_LIST");

                    function.Exports["PI_WERKS"].ParamValue = plant;

                    #region FILTRO DE DEPÓSITO

                    RFCTable STORAGELOCATION = function.Tables["STORAGELOCATION"];

                    RFCStructure STORAGELOCATION_ITEM_LINHA = new RFCStructure();

                    var depositoExcluidos = new string[] { "DMMV", "DM04", "DM14" };

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
                connection.Clone();
            }

            return materiais;
        }

        public string AtenderPorTransferencia(IEnumerable<MaterialAtendimento> materiais, string depositoDestino = "DM04")
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
                    GOODSMVT_ITEM_LINHA["MOVE_TYPE"] = "311"; //TRANSFERÊNCIA DE DEPÓSITO
                    GOODSMVT_ITEM_LINHA["MOVE_STLOC"] = depositoDestino; //DESTINO
                    GOODSMVT_ITEM_LINHA["ENTRY_QNT"] = material.QuantidadeRetirada;

                    if (material.TemLote)
                        GOODSMVT_ITEM_LINHA["BATCH"] = material.NumeroLote;

                    functionBAPI.Exports["GOODSMVT_HEADER"].ParamValue = GOODSMVT_HEADER;
                    functionBAPI.Exports["GOODSMVT_CODE"].ParamValue = GOODSMVT_CODE;

                    functionBAPI.Tables["GOODSMVT_ITEM"] = GOODSMVT_ITEM;
                }

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
                connection.CreateFunction(function).Execute();
                connection.Clone();
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

    }//end class

}//end namespace
