using Domain;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infrastructure
{
    public class PosicaoDepositoRepository : RepositoryBase<PosicaoDeposito>
    {
        /// <summary>
        /// Faz INSERT múltiplo de registros exceto os que já existem
        /// </summary>
        public void Importar(IList<PosicaoDeposito> itens)
        {
            try
            {
                if (!itens.Any())
                    return;

                var sql = new StringBuilder("INSERT INTO [PosicaoDeposito] With (ROWLOCK) SELECT * FROM (VALUES ");

                sql.AppendLine(string.Join(",", itens.Select(item => $"('{item.Deposito}','{item.Localizacao}','{item.Area}')")));

                sql.AppendLine(" )sub(Deposito, Localizacao, Area) EXCEPT SELECT Deposito,Localizacao,Area FROM [PosicaoDeposito]");

                dbContext.Database.ExecuteSqlCommand(sql.ToString());

            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
    }


}