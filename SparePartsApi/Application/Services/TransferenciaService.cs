
using Domain;
using Infrastructure;

namespace Application
{
    public class TransferenciaService : ServiceBase<MaterialAtendimentoTransferencia>
    {
        private readonly TransferenciaRepository repository;

        public TransferenciaService(TransferenciaRepository repository) : base(repository)
        {
            this.repository = repository;
        }

    }
}


