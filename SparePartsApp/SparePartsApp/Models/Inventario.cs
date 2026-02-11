using SparePartsApp.Helpers;

namespace SparePartsApp.Models
{
    public class Inventario : ObservableObject
    {
        public string NumeroDocumento { get; set; }

        public string AnoFiscal { get; set; }

        public string Usuario { get; set; }

        public ObservableRangeCollection<Material> Materiais { get; set; } = new ObservableRangeCollection<Material>();
    }
}
