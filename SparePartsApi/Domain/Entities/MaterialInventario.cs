using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    [Table("MaterialInventariado")]
    public class MaterialInventario : Material
    {
        public String NumeroDocumento { get; set; }

        public Int32? AnoFiscal { get; set; }

        public String NumeroPedidoItem { get; set; }

    }

}