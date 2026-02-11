using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SPW.Models
{
    public class MovimentacaoViewModel
    {
        public MaterialMovimentacao Filtro { get; set; } = new MaterialMovimentacao();

        public IEnumerable<MaterialMovimentacao> Materiais { get; set; }
    }
}