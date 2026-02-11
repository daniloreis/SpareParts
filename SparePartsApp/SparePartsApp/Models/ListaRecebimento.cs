using SparePartsApp.Helpers;

namespace SparePartsApp.Models
{
    public class Recebimento : ObservableObject
    {
        public string CodLista { get; set; }

        public string Fornecedor { get; set; }

        public string NumeroNfe { get; set; }

        public int? NumeroVolumes { get; set; }

        public string PosicaoDeposito { get; set; }

        public string NumeroDocumento { get; set; }

        public int Ano { get; set; }

        public ObservableRangeCollection<Material> Materiais { get; set; } = new ObservableRangeCollection<Material>();
    }
}
