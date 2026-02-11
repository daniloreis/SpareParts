using Domain;

namespace Infrastructure
{
    public interface ISapIntegrationFacade
    {
        ListaRecebimento BuscarListaRecebimento(string id);
    }
}