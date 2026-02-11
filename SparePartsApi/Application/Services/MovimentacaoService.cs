
using Domain;
using Infrastructure;
using System.Collections.Generic;

namespace Application
{
    public class MaterialmovimentadoService : ServiceBase<MaterialMovimentacao>
    {
        private readonly MaterialmovimentadoRepository repository;

        public MaterialmovimentadoService(MaterialmovimentadoRepository repository) : base(repository)
        {
            this.repository = repository;
        }

        public IEnumerable<MaterialMovimentacao> Filtrar(MaterialMovimentacao filtro)
        {
            return repository.FindByCriteria(x => (x.Nome == filtro.Nome || string.IsNullOrEmpty(filtro.Nome)) &&
                                   (x.CodMaterial == filtro.CodMaterial || string.IsNullOrEmpty(filtro.CodMaterial)) &&
                                   (x.Deposito == filtro.Deposito || string.IsNullOrEmpty(filtro.Deposito)) &&
                                   (x.DataHoraInicio >= filtro.DataHoraInicio || !filtro.DataHoraInicio.HasValue) &&
                                   (x.DataHoraFim <= filtro.DataHoraFim || !filtro.DataHoraFim.HasValue) &&
                                   (x.Usuario == filtro.Usuario || string.IsNullOrEmpty(filtro.Usuario)));
        }
    }
}


