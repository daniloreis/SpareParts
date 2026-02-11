using Domain;
using System.Collections.Generic;

namespace SPW.Models
{
    public class InventarioViewModel
    {
        public MaterialInventario Filtro { get; set; } = new MaterialInventario();

        public IEnumerable<MaterialInventario> Materiais { get; set; }
    }
}