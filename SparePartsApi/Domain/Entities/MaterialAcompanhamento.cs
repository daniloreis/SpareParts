using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    [Table("MaterialAcompanhamento")]
    public class MaterialAcompanhamento
    {
        [Key]
        public Int32? Id { get; set; }

        public string CodMaterial { get; set; }

        public DateTime? DataHoraInicio { get; set; }

        public DateTime? DataHoraFim { get; set; }

        public string Usuario { get; set; }

        public string MotivoAcompanhamento { get; set; }

        public string UsuarioAtendido { get; set; }

        public string ImagemAssinatura { get; set; }
    }
}
