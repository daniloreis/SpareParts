using Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public abstract class Material
    {
        [Key]
        public int? Id { get; set; }

        private string codMaterial;
        public string CodMaterial { get { return (codMaterial ?? "").TrimStart('0'); } set { codMaterial = value; } }

        public string Nome { get; set; }

        public double? Quantidade { get; set; }

        public string NumeroLote { get; set; }

        public string UnidadeMedida { get; set; }

        public string Deposito { get; set; }

        [MaxLength(20)]
        public string PosicaoDeposito { get; set; }

        public DateTime? DataHoraInicio { get; set; }

        public DateTime? DataHoraFim { get; set; }

        /// <summary>
        /// Operador do coletor
        /// </summary>
        public string Usuario { get; set; }

        public string RetornoSap { get; set; }

        [NotMapped]
        public string DepositoRC { get; set; }

        [NotMapped]
        public string DepositoDestino { get; set; }

        public void DefinirDepositoDestino()
        {
            if (!string.IsNullOrWhiteSpace(RevisaoOM) && RevisaoOM.StartsWith("PG"))
                DepositoDestino = Domain.Deposito.PARADAGERAL;
            else if (!string.IsNullOrWhiteSpace(DepositoRC))
                DepositoDestino = DepositoRC;
            else
                DepositoDestino = Deposito;
        }

        [NotMapped]
        public bool EhCompraDireta { get { return !string.IsNullOrEmpty(ClassContabil); } }

        [NotMapped]
        public string DepositoEtiqueta
        {
            get { return TextoDeposito(Deposito); }
        }

        [NotMapped]
        public string DepositoDestinoEtiqueta
        {
            get
            {
                if (string.IsNullOrWhiteSpace(DepositoDestino))
                    return "";

                return TextoDeposito(DepositoDestino);
            }
        }

        public string TextoDeposito(string deposito)
        {
            switch (deposito)
            {
                case Domain.Deposito.COMPRADIRETA:
                    return "COMPRA DIRETA";
                case Domain.Deposito.DIVERGENCIA:
                    return "DIVERGÊNCIA";
                case Domain.Deposito.ARMAZENAGEM:
                    return "ARMAZENAGEM";
                case Domain.Deposito.LABORATORIO:
                    return "LABORATÓRIO";
                case Domain.Deposito.PARADAGERAL:
                    return "PARADA GERAL";
                case Domain.Deposito.SPAREPARTS:
                    return "SPARE PARTS";
                default:
                    return deposito;
            }
        }

        [NotMapped]
        public string RevisaoOM { get; set; }

        /// <summary>
        /// Ficha de Informação de Segurança para Produtos Químicos
        /// </summary>
        [NotMapped]
        public string Fispq { get; set; }

        [NotMapped]
        public bool RequerPlacaAtivo { get; set; }

        [NotMapped]
        public List<Ativo> Ativos { get; set; }

        [NotMapped]
        public DateTime? Validade { get; set; }

        [NotMapped]
        public string EmailSolicitante { get; set; }

        [NotMapped]
        public string NumeroPedido { get; set; }

        [NotMapped]
        public string NumeroPedidoItem { get; set; }

        [NotMapped]
        public string NumeroRC { get; set; }

        [NotMapped]
        public string ClassContabil { get; set; }

        [NotMapped]
        public string CondicaoTemperatura { get; set; }

        [NotMapped]
        public string CondicaoArmazenagem { get; set; }

        [NotMapped]
        public string UnidadeTempo { get; set; }

        [NotMapped]
        public string UnidadeMedidaOriginal { get; set; }

        [NotMapped]
        public string PosicaoDepositoOrigem { get; set; }

        [NotMapped]
        public bool InventarioAtivo { get; set; }

        [NotMapped]
        public IList<Lote> Lotes { get; set; } = new List<Lote>();

        [NotMapped]
        private bool temLote = false;
        [NotMapped]
        public bool TemLote
        {
            get { return temLote || !string.IsNullOrWhiteSpace(NumeroLote); }
            set { temLote = value; }
        }

    }

}