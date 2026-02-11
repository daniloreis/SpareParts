using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    [Table("PosicaoDeposito")]
    public class PosicaoDeposito
    {
        [Key]
        public Int32? Id { get; set; }

        [Required(ErrorMessage = "Obrigatório")]
        public String Deposito { get; set; }

        [Required(ErrorMessage = "Obrigatório")]
        public String Localizacao { get; set; }

        [Required(ErrorMessage = "Obrigatório")]
        public String Area { get; set; }
    }

}