using Domain;
using Infrastructure;
using System.Collections.Generic;

namespace Application
{
    public class ArmazenagemService : ServiceBase<MaterialArmazenagem>
    {
        private readonly ArmazenagemRepository repository;
        private SapIntegrationFacade sapService;

        public ArmazenagemService(ArmazenagemRepository repository) : base(repository)
        {
            this.repository = repository;

            if (sapService == null)
                sapService = new SapIntegrationFacade();
        }

        public string Armazenar(MaterialArmazenagem material, string depositoOrigem = Deposito.ARMAZENAGEM)
        {
            return sapService.Armazenar(material);
        }

        public MaterialRecebimento BuscarMaterialPendenteArmazenagem(int ano, string documento, string item)
        {
            var material = sapService.BuscarMaterialPendenteArmazenagem(ano, documento, item);

            if (material == null)
                return null;

            material.InventarioAtivo = sapService.VerificarInventarioAtivo(material.DepositoDestino, material.CodMaterial);

            return material;
        }

        public IEnumerable<MaterialArmazenagem> Filtrar(MaterialArmazenagem filtro)
        {
            return repository.FindByCriteria(x => (x.Nome == filtro.Nome.Trim() || string.IsNullOrEmpty(filtro.Nome)) &&
                                          (x.CodMaterial == filtro.CodMaterial || string.IsNullOrEmpty(filtro.CodMaterial)) &&
                                          (x.Deposito == filtro.Deposito || string.IsNullOrEmpty(filtro.Deposito)) &&
                                          (x.DataHoraInicio >= filtro.DataHoraInicio || !filtro.DataHoraInicio.HasValue) &&
                                          (x.DataHoraFim <= filtro.DataHoraFim || !filtro.DataHoraFim.HasValue) &&
                                          (x.Usuario == filtro.Usuario || string.IsNullOrEmpty(filtro.Usuario)));
        }
    }
}


