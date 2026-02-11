using SparePartsApp.Models;
using System;
using System.Text;
using System.Threading.Tasks;

namespace SparePartsApp.Services
{
    public class InventarioService : BaseRestService<Inventario>
    {
        public virtual async Task<Inventario> BuscarInventario(string numero)
        {
            try
            {
                return await base.Get($@"BuscarInventario/{numero}");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public virtual async Task<string> Inventariar(Inventario inventario)
        {
            try
            {
                return await base.Post(inventario, $@"Inventariar");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
