using System;

namespace Domain
{
    public class Deposito
    {
        /// <summary>
        /// compra direta não tem depósito
        /// </summary>
        public const string COMPRADIRETA = "";
        public const string LABORATORIO = "DM05";
        public const string PARADAGERAL = "DM10";
        public const string DIVERGENCIA = "DM14";
        public const string ARMAZENAGEM = "DM15";
        public const string VENCIDOS = "DMMV";
        public const string TRANSFERENCIA = "DM04";
        public const string SPAREPARTS = "SPARE PARTS";


        public string CodMaterial { get; set; }

        public string Nome { get; set; }

        public string PosicaoDeposito { get; set; }

        public string NumeroLote { get; set; }

        public DateTime? Validade { get; set; }

        public double? QuantidadeEstoqueSAP { get; set; }
    }
}
