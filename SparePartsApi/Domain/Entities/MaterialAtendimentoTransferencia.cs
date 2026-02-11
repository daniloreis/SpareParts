
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    [Table("MaterialAtendimentoTransferencia")]
    public class MaterialAtendimentoTransferencia
    {
        [Key]
        public Int32? Id { get; set; }

        [Required(ErrorMessage = "Obrigatório")]
        public String CodMaterial { get; set; }

    }
}
