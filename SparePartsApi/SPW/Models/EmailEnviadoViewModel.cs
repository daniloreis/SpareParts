using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace SPW.Models
{
    public class EmailEnviadoViewModel
    {
        public string Mensagem { get; set; }

        public IEnumerable<EmailEnviado> Emails { get; set; } = new List<EmailEnviado>();
    }
}