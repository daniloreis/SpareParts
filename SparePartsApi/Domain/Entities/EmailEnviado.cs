
using System;
using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class EmailEnviado
    {
        [Key]
        public Int32? Id { get; set; }

        [Required(ErrorMessage = "Obrigatório")]
        public String Destinatario { get; set; }

        [Required(ErrorMessage = "Obrigatório")]
        public String Assunto { get; set; }

        [Required(ErrorMessage = "Obrigatório")]
        public String Mensagem { get; set; }

        public DateTime? DataHoraCriado { get; set; }

        public DateTime? DataHoraEnviado { get; set; }

        public String EmailsCC { get; set; }

        public String Erro { get; set; }
    }
}
