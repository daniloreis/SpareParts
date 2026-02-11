using Newtonsoft.Json;
using SparePartsApp.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace SparePartsApp.Models
{
    public class Material : ObservableObject
    {
        public Int32? Id { get; set; }
        public String CodMaterial { get; set; }

        public String CodLista { get; set; }

        private string nome = string.Empty;
        public string Nome { get { return nome; } set { SetProperty(ref nome, value); } }

        private string descricao = string.Empty;
        public string Descricao { get { return descricao; } set { SetProperty(ref descricao, value); } }

        private string unidadeMedida = string.Empty;
        public string UnidadeMedida { get { return unidadeMedida; } set { SetProperty(ref unidadeMedida, value); } }

        private string unidadeTempo = string.Empty;
        public string UnidadeTempo { get { return unidadeTempo; } set { SetProperty(ref unidadeTempo, value); } }

        private string unidadeMedidaOriginal = string.Empty;
        public string UnidadeMedidaOriginal { get { return unidadeMedidaOriginal; } set { SetProperty(ref unidadeMedidaOriginal, value); } }


        public DateTime? DataEntrada { get; set; }

        public string NumeroDocumento { get; set; }

        public string NumeroItem { get; set; }

        public string Centro { get; set; }

        public string Fornecedor { get; set; }

        public string TipoMovimento { get; set; }

        public string MotivoAcompanhamento { get; set; }

        public string Ordem { get; set; }

        public string Fispq { get; set; }

        public bool FispqPendente { get; set; }

        #region QUANTIDADES

        public double? Quantidade = null;

        public string QuantidadeString
        {
            get { return string.Format(new CultureInfo("pt-BR"), "{0}", Quantidade); }

            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    SetProperty(ref Quantidade, null);
                else
                    SetProperty(ref Quantidade, Convert.ToDouble(value, new CultureInfo("pt-BR")));
            }
        }

        /// <summary>
        ///Quantidade no estoque do SAP em formato double. 
        /// </summary>
        public double? QuantidadeEstoqueSAP = null;

        public string QuantidadeEstoqueSAPString
        {
            get { return string.Format(new CultureInfo("pt-BR"), "{0}", QuantidadeEstoqueSAP); }

            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    SetProperty(ref QuantidadeEstoqueSAP, null);
                else
                    SetProperty(ref QuantidadeEstoqueSAP, Convert.ToDouble(value, new CultureInfo("pt-BR")));
            }
        }


        /// <summary>
        /// Quantidade contada no estoque físico
        /// </summary>
        public double? quantidadeEstoqueFisico = null;
        public string QuantidadeEstoqueFisico
        {
            get
            {
                return string.Format(new CultureInfo("pt-BR"), "{0}", quantidadeEstoqueFisico);

            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    SetProperty(ref quantidadeEstoqueFisico, null);
                else
                    SetProperty(ref quantidadeEstoqueFisico, Convert.ToDouble(value, new CultureInfo("pt-BR")));
            }
        }

        /// <summary>
        /// Quantidade a ser consumida no SAP
        /// </summary>
        public double? quantidadeRetirada = null;
        public string QuantidadeRetirada
        {
            get
            {
                return string.Format(new CultureInfo("pt-BR"), "{0}", quantidadeRetirada);

            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    SetProperty(ref quantidadeRetirada, null);
                else
                    SetProperty(ref quantidadeRetirada, Convert.ToDouble(value, new CultureInfo("pt-BR")));
            }
        }

        /// <summary>
        /// Saldo disponivel a ser consumido pela reserva
        /// </summary>
        public double? QuantidadeDisponivel = null;

        #endregion

        private string foto = string.Empty;
        public string Foto
        {
            get { return foto; }
            set
            {
                SetProperty(ref foto, value);
                if (!string.IsNullOrWhiteSpace(value))
                    ImagemBotaoFoto = "@drawable/cameraOK.png";
            }
        }

        private string deposito = string.Empty;
        public string Deposito { get { return deposito; } set { SetProperty(ref deposito, value); } }


        private string depositoDestino = string.Empty;
        public string DepositoDestino { get { return depositoDestino; } set { SetProperty(ref depositoDestino, value); } }

        public string DepositoDestinoEtiqueta { get; set; }

        private string depositoEtiqueta = string.Empty;
        public string DepositoEtiqueta { get { return depositoEtiqueta; } set { SetProperty(ref depositoEtiqueta, value); } }

        public bool NaoPodeAtender { get; set; }

        public string PosicaoDeposito { get; set; }

        public string PosicaoDepositoOrigem { get; set; }


        [JsonIgnore]
        public string PosicaoBipada { get; set; }

        public IList<Deposito> Depositos { get; set; } = new List<Deposito>();


        private DateTime? validade = null;
        public DateTime? Validade { get { return validade; } set { SetProperty(ref validade, value); } }

        public bool TemValidade
        {
            get { return Validade.HasValue; }
        }

        public string DiasVencimento
        {
            get
            {
                var dias = string.Empty;

                if (validade.HasValue)
                    dias = $"{validade.Value.Subtract(DateTime.Now).Days} dias";

                return dias;
            }
        }

        private bool temLote = false;
        public bool TemLote { get { return temLote; } set { SetProperty(ref temLote, value); } }

        public bool NaoTemLote { get { return !TemLote; } }

        public string RevisaoOM { get; set; }

        public string DepositoRC { get; set; }

        public bool Selecionado { get; set; }

        public String CondicaoTemperatura { get; set; }

        public String CondicaoArmazenagem { get; set; }

        public String NumeroPedido { get; set; }

        public String NumeroPedidoItem { get; set; }

        public String NumeroRC { get; set; }

        public string NumeroReserva { get; set; }

        public String Sequencia { get; set; }

        public String NumeroLote { get; set; }

        public String NumeroNota { get; set; }

        public String ClassContabil { get; set; }

        public String EmailSolicitante { get; set; }

        public bool? TemDivergencia { get; set; }

        public bool Critico { get; set; }

        public DateTime? DataHoraInicio { get; set; }

        public DateTime? DataHoraFim { get; set; }

        public String Usuario { get; set; }

        public String UsuarioAtendido { get; set; }

        public String ImagemAssinatura { get; set; }

        private string corTexto = "Black";

        public int? Ano { get; set; }

        [JsonIgnore]
        public string CorTexto { get { return corTexto; } set { SetProperty(ref corTexto, value); } }

        private string imagemBotaoFoto = "@drawable/camera.png";

        [JsonIgnore]
        public string ImagemBotaoFoto
        {
            get { return imagemBotaoFoto; }
            set { SetProperty(ref imagemBotaoFoto, value); }
        }

        private int quantidadeImpressao = 1;

        public int QuantidadeImpressao { get { return quantidadeImpressao; } set { SetProperty(ref quantidadeImpressao, value); } }

        [JsonIgnore]
        public bool ExigeValidade
        {
            get { return !string.IsNullOrWhiteSpace(UnidadeTempo); }
        }

        [JsonIgnore]
        public bool ExigeFispq
        {
            get { return !string.IsNullOrWhiteSpace(Fispq); }
        }

        [JsonIgnore]
        public bool NaoExigeValidade
        {
            get { return string.IsNullOrWhiteSpace(UnidadeTempo); }
        }

        public double QuantidadePO { get; set; }

        public DateTime DataCriacaoRC { get; set; }


        private ObservableRangeCollection<Lote> lotes = new ObservableRangeCollection<Lote>();

        public ObservableRangeCollection<Lote> Lotes { get { return lotes; } set { SetProperty(ref lotes, value); } }


        private ObservableRangeCollection<MaterialRepetido> repetidos = new ObservableRangeCollection<MaterialRepetido>();

        public ObservableRangeCollection<MaterialRepetido> Repetidos { get { return repetidos; } set { SetProperty(ref repetidos, value); } }


        private bool inventarioAtivo = false;
        public bool InventarioAtivo { get { return inventarioAtivo; } set { SetProperty(ref inventarioAtivo, value); } }

        public bool RequerPlacaAtivo { get; set; }

        public List<Ativo> Ativos { get; set; }

        public string NumeroBC
        {
            get
            {
                return $"{Ano}/{NumeroDocumento}/{NumeroItem}";
            }
        }

    }

    public class Lote
    {
        public string CodMaterial { get; set; }

        public string NumeroLote { get; set; }

        public double? Quantidade { get; set; }

        public string Deposito { get; set; }

        public DateTime? Validade { get; set; }
    }

    public class MaterialRepetido
    {
        public string NumeroPedido { get; set; }

        public string NumeroPedidoItem { get; set; }

        public double Quantidade { get; set; }

        public double QuantidadePO { get; set; }

        public string DepositoDestino { get; set; }
    }

    public class Deposito
    {
        public string Nome { get; set; }

        public string PosicaoDeposito { get; set; }

        public string NumeroLote { get; set; }

        public DateTime? Validade { get; set; }

        public double? QuantidadeEstoqueSAP { get; set; }

        public string Exibir { get { return Nome + (string.IsNullOrWhiteSpace(NumeroLote) ? "" : " LOTE: " + NumeroLote); } }
    }

    public class Ativo
    {
        public int Sequencia { get; set; }

        public string Placa { get; set; }
    }
}
