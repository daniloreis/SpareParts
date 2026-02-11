using Domain;
using System.Collections.Generic;

namespace SPW.Models
{
    public class PosicaoDepositoViewModel
    {
        public IList<PosicaoDeposito> Items { get; set; }

        public string Deposito { get; set; }

        public string Area { get; set; }

        public string Localizacao { get; set; }

        public string ImagemCodBarra { get; set; }
    }
}