using Domain;
using Infrastructure;

namespace Application
{

    public class ListarecebimentoService : ServiceBase<ListaRecebimento>
    {
        private readonly ListarecebimentoRepository repository;

        public ListarecebimentoService(ListarecebimentoRepository repository) : base(repository)
        {
            this.repository = repository;
        }

    }
}

