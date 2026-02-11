using SparePartsApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SparePartsApp.Services
{
    public class AtendimentoService : BaseRestService<IEnumerable<Material>>
    {
        public override string Controller { get => "Atendimento"; }

        public virtual async Task<IEnumerable<Material>> BuscarReserva(string numero)
        {
            try
            {
                return await base.Get($@"BuscarReserva/{numero}");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual async Task<IEnumerable<Material>> BuscarTransferencia()
        {
            try
            {
                return await base.Get($@"BuscarTransferencia");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual async Task<string> Atender(IEnumerable<Material> materiais)
        {
            try
            {
                if (materiais.Any(x => !string.IsNullOrWhiteSpace(x.NumeroReserva) && !string.IsNullOrWhiteSpace(x.ImagemAssinatura)))
                {
                    var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), materiais.First(x => !string.IsNullOrWhiteSpace(x.ImagemAssinatura)).ImagemAssinatura);
                    FileUpload("EnviarAssinatura", filePath);
                }

                return await base.Post(materiais, $@"Atender");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual async Task<IEnumerable<Material>> BuscarCompraDireta(string[] codMateriais)
        {
            try
            {
                string codigos = string.Join(",", codMateriais);

                return await base.Get($@"BuscarCompraDireta/{codigos}");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual async Task<string> AtenderCompraDireta(IEnumerable<Material> materiais)
        {
            try
            {
                if (materiais != null && materiais.Any(x => !string.IsNullOrWhiteSpace(x.ImagemAssinatura)))
                {
                    var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), materiais.First(x => !string.IsNullOrWhiteSpace(x.ImagemAssinatura)).ImagemAssinatura);
                    FileUpload("EnviarAssinatura", filePath);
                }

                return await base.Post(materiais, $@"AtenderCompraDireta");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual async Task<string> IniciarVisita()
        {
            try
            {
                var atendimentoInicial = new AtendimentoInicialService();

                return await atendimentoInicial.Get($@"IniciarVisita");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public virtual async Task<string> AtenderVisitante(IEnumerable<Material> materiais)
        {
            try
            {
                if (materiais != null && materiais.Any(x => !string.IsNullOrWhiteSpace(x.ImagemAssinatura)))
                {
                    var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), materiais.First(x => !string.IsNullOrWhiteSpace(x.ImagemAssinatura)).ImagemAssinatura);
                    FileUpload("EnviarAssinatura", filePath);
                }

                return await base.Post(materiais, $@"AtenderVisitante");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
    public class AtendimentoInicialService : BaseRestService<string>
    {
        public override string Controller { get => "Atendimento"; }
    }
}
