using Domain;
using Infrastructure;
using System;
using System.Collections.Generic;

namespace Application
{
    public class InventarioService : ServiceBase<MaterialInventario>
    {
        private readonly InventarioRepository repository;

        public InventarioService(InventarioRepository repository) : base(repository)
        {
            this.repository = repository;
        }

        public void AdicionarLista(IList<MaterialInventario> itens)
        {
            try
            {
                repository.AdicionarLista(itens);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<MaterialInventario> Filtrar(MaterialInventario filtro)
        {
            return repository.FindByCriteria(x => (x.Nome == filtro.Nome || string.IsNullOrEmpty(filtro.Nome)) &&
                                   (x.CodMaterial == filtro.CodMaterial || string.IsNullOrEmpty(filtro.CodMaterial)) &&
                                   (x.Deposito == filtro.Deposito || string.IsNullOrEmpty(filtro.Deposito)) &&
                                   (x.NumeroDocumento == filtro.NumeroDocumento || string.IsNullOrEmpty(filtro.NumeroDocumento)) &&
                                   (x.AnoFiscal == filtro.AnoFiscal || !filtro.AnoFiscal.HasValue) &&
                                   (x.DataHoraInicio >= filtro.DataHoraInicio || !filtro.DataHoraInicio.HasValue) &&
                                   (x.DataHoraFim <= filtro.DataHoraFim || !filtro.DataHoraFim.HasValue) &&
                                   (x.Usuario == filtro.Usuario || string.IsNullOrEmpty(filtro.Usuario)));
        }
    }
}