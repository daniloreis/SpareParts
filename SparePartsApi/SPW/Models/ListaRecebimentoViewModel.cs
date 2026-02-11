using Domain;
using System;
using System.Collections.Generic;

namespace SPW.Models
{
    public class ListaRecebimentoViewModel
    {
        public MaterialRecebimento Filtro { get; set; } = new MaterialRecebimento();

        public IEnumerable<MaterialRecebimento> Materiais { get; set; }

        public bool? TemDivergencia { get; set; }

    }
}