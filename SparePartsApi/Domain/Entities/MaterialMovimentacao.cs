using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    [Table("MaterialMovimentado")]
    public class MaterialMovimentacao : Material
    {


        /// <summary>
        ///Quantidade no estoque do SAP em formato double. 
        /// </summary>
        public double? QuantidadeEstoqueSAP { get; set; }

        /// <summary>
        /// Quantidade contada no estoque físico
        /// </summary>

        public double? QuantidadeEstoqueFisico { get; set; }

    }
}
