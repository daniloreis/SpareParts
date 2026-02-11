using System;

namespace Domain
{
    public class Lote
    {
        public string CodMaterial { get; set; }

        public string NumeroLote { get; set; }

        public double? Quantidade { get; set; }

        public string Deposito { get; set; }

        public DateTime? Validade { get; set; }
    }
}
