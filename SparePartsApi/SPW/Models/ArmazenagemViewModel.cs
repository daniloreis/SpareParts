using Domain;
using System;
using System.Collections.Generic;

namespace SPW.Models
{
    public class ArmazenagemViewModel
    {
        public MaterialArmazenagem Filtro { get; set; } = new MaterialArmazenagem();

        public IEnumerable<MaterialArmazenagem> Materiais { get; set; }
    }
}