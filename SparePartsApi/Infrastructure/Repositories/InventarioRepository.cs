using Domain;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Infrastructure
{
    public class InventarioRepository : RepositoryBase<MaterialInventario>
    {/// <summary>
     /// Faz INSERT múltiplo de registros
     /// </summary>
        public void AdicionarLista(IList<MaterialInventario> itens)
        {
            try
            {
                if (!itens.Any())
                    return;

                var sql = new StringBuilder(@"INSERT INTO [Materialinventariado] ([CodMaterial],
                                                                                [Nome],
                                                                                [Quantidade],
                                                                                [UnidadeMedida], 
                                                                                [PosicaoDeposito],
                                                                                [Deposito],
                                                                                [NumeroLote],
                                                                                [DataHoraInicio],
                                                                                [DataHoraFim],
                                                                                [Usuario],
                                                                                [RetornoSap],
                                                                                [NumeroDocumento],
                                                                                [AnoFiscal]) VALUES ");


                sql.AppendLine(string.Join(",", itens.Select(item => $@"('{item.CodMaterial}',
                '{item.Nome}',                 
                 '{string.Format(new CultureInfo("en-US"), "{0}", item.Quantidade)}',
                 '{item.UnidadeMedida}',
                '{item.PosicaoDeposito}',
                '{item.Deposito}',
                '{item.NumeroLote}',
                 CONVERT(datetime,'{item.DataHoraInicio}',103),
                 CONVERT(datetime,'{item.DataHoraFim}',103),
                '{item.Usuario}',
                '{item.RetornoSap}',
                '{item.NumeroDocumento}',
                '{item.AnoFiscal}')")));

                dbContext.Database.ExecuteSqlCommand(sql.ToString());
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
    }


}