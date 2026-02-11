using Domain;
using Infrastructure;
using System.Collections.Generic;

namespace Application
{
    public class PosicaoDepositoService : ServiceBase<PosicaoDeposito>
    {
        private readonly PosicaoDepositoRepository repository;

        public PosicaoDepositoService(PosicaoDepositoRepository repository) : base(repository)
        {
            this.repository = repository;
        }

        public void Importar(IList<PosicaoDeposito> itens)
        {
            repository.Importar(itens);
        }

        public IEnumerable<PosicaoDeposito> BuscarPor(string deposito, string area)
        {
            return repository.FindByCriteria(d => (d.Deposito.Equals(deposito) || deposito.Equals("")) || (d.Area.Equals(area) || area.Equals("")));
        }

    }


}