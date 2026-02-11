using SparePartsApp.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SparePartsApp.Services
{
    public class MaterialService : BaseRestService<Material>
    {
        public virtual async Task<IEnumerable<Material>> Pendentes()
        {
            try
            {
                return await base.GetList("Pendentes");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual async Task<string> Armazenar(Material material)
        {
            try
            {
                return await base.Post(material, "Armazenar");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual async Task<string> Movimentar(Material material)
        {
            try
            {
                return await base.Post(material, "Movimentar");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
