using System.Collections.Generic;

namespace Domain
{
    public class Inventario
    {
        public string NumeroDocumento { get; set; }

        public string AnoFiscal { get; set; }

        public string Usuario { get; set; }

        public IList<MaterialInventario> Materiais { get; set; } = new List<MaterialInventario>();
    }
}
