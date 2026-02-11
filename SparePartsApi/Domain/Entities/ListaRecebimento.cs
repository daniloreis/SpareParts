using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class ListaRecebimento
    {
        [Key]
        public int Id { get; set; }

        public string CodLista { get; set; }

        public string Fornecedor { get; set; }

        public string NumeroNfe { get; set; }

        public string RetornoSap { get; set; }

        public string PosicaoDeposito { get; set; }

        public string NumeroDocumento { get; set; }

        public int NumeroVolumes { get; set; }

        [NotMapped]
        public IList<MaterialRecebimento> Materiais { get; set; } = new List<MaterialRecebimento>();
    }
}