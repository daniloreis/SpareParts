using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Domain
{
    [Table("MaterialRecebido")]
    public class MaterialRecebimento : Material, ICloneable
    {
        public int? Ano { get; set; }

        public string NumeroDocumento { get; set; }

        public string NumeroItem { get; set; }


        [NotMapped]
        public int QuantidadeImpressao { get; set; } = 1;

        public bool temDivergencia { get; set; }

        public bool TemDivergencia()
        {
            temDivergencia = false;

            if (!string.IsNullOrEmpty(Foto))
                temDivergencia = true;

            if (Repetidos.Count > 1)
            {
                if (quantidadeEstoqueFisico.HasValue && Repetidos.Sum(x => x.Quantidade) != quantidadeEstoqueFisico)
                    temDivergencia = true;
            }
            else
            {
                if (quantidadeEstoqueFisico.HasValue && Quantidade != quantidadeEstoqueFisico)
                    temDivergencia = true;
            }

            return temDivergencia;
        }

        public string CodLista { get; set; }

        public string Foto { get; set; }

        [NotMapped]
        public DateTime? DataEntrada { get; set; }

        [NotMapped]
        public string Centro { get; set; }

        [NotMapped]
        public string Fornecedor { get; set; }

        [NotMapped]
        public string NumeroNota { get; set; }

        [NotMapped]
        public string TipoMovimento { get; set; }

        [NotMapped]
        public string UsuarioAtendido { get; set; }

        public string Ordem { get; set; }

        public string PlanejadorOrdem { get; set; }

        [NotMapped]
        public string Descricao { get; set; }

        [NotMapped]
        public double? quantidadeEstoqueFisico { get; set; }

        [NotMapped]
        public string QuantidadeEstoqueFisico { get; set; }

        [NotMapped]
        public double QuantidadePO { get; set; }

        [NotMapped]
        public double QuantidadeRC { get; set; }

        [NotMapped]
        public DateTime DataCriacaoRC { get; set; }

        [NotMapped]
        public IList<MaterialRepetido> Repetidos { get; set; } = new List<MaterialRepetido>();

        [NotMapped]
        public double QuantidadeEstoqueSAP { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public class MaterialRepetido
    {
        [NotMapped]
        public string NumeroPedido { get; set; }

        [NotMapped]
        public string NumeroPedidoItem { get; set; }

        [NotMapped]
        public double Quantidade { get; set; }

        [NotMapped]
        public double QuantidadePO { get; set; }

        [NotMapped]
        public string DepositoDestino { get; set; }

        [NotMapped]
        public DateTime DataCriacaoRC { get; set; }
    }
}
