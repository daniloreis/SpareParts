using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SPW.Models
{
    public class AtendimentoViewModel
    {
        public MaterialAtendimento Filtro { get; set; } = new MaterialAtendimento();

        public IEnumerable<MaterialAtendimento> Materiais { get; set; }

        public IEnumerable<MaterialAcompanhamento> Acompanhamentos { get; set; }
    }
}