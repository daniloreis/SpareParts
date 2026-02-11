using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    [Table("MaterialArmazenado")]
    public class MaterialArmazenagem : Material
    {
        [NotMapped]
        public double? QuantidadeDivergencia
        {
            get
            {
                return Convert.ToDouble(QuantidadeEstoqueFisico) - QuantidadeEstoqueSAP;
            }
        }


        /// <summary>
        ///Quantidade no estoque do SAP em formato double. 
        /// </summary>
        public double? QuantidadeEstoqueSAP { get; set; }

        /// <summary>
        /// Quantidade contada no estoque físico
        /// </summary>

        public double? QuantidadeEstoqueFisico { get; set; }

        [NotMapped]
        public bool FispqPendente { get; set; }
    }
}
