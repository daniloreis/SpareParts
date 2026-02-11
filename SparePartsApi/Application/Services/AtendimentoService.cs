using Domain;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application
{
    public class AtendimentoService : ServiceBase<MaterialAtendimento>
    {
        private readonly SapIntegrationFacade sapFacade;
        private readonly AtendimentoTransferenciaRepository atendimentoTransferenciaRepository;
        private readonly AtendimentoRepository repository;
        private readonly AcompanhamentoRepository repositoryAcompanhamento;

        public AtendimentoService(AtendimentoRepository repository, AtendimentoTransferenciaRepository atendimentoTransferenciaRepository, AcompanhamentoRepository repositoryAcompanhamento) : base(repository)
        {
            if (sapFacade == null)
                sapFacade = new SapIntegrationFacade();

            this.atendimentoTransferenciaRepository = atendimentoTransferenciaRepository;
            this.repositoryAcompanhamento = repositoryAcompanhamento;
            this.repository = repository;
        }

        public IEnumerable<MaterialAtendimento> BuscarReserva(string numReserva)
        {
            try
            {
                var reserva = sapFacade.BuscarReserva(numReserva);

                var novaLista = new List<MaterialAtendimento>();

                foreach (var material in reserva)
                {
                    var qtdSolicitada = material.Quantidade;

                    foreach (var deposito in material.Depositos.OrderBy(x => x.Validade))
                    {
                        var novoMaterial = new MaterialAtendimento();

                        if (!string.IsNullOrWhiteSpace(material.NumeroReserva))
                            qtdSolicitada -= deposito.QuantidadeEstoqueSAP;

                        novoMaterial.Nome = material.Nome;
                        novoMaterial.Depositos = material.Depositos;
                        novoMaterial.Quantidade = material.Quantidade;
                        novoMaterial.CodMaterial = material.CodMaterial;
                        novoMaterial.NumeroReserva = material.NumeroReserva;
                        novoMaterial.UnidadeMedida = material.UnidadeMedida;
                        novoMaterial.DataHoraInicio = material.DataHoraInicio;
                        novoMaterial.Sequencia = material.Sequencia;
                        novoMaterial.NaoPodeAtender = material.NaoPodeAtender;
                        novoMaterial.Usuario = material.Usuario;

                        novoMaterial.QuantidadeEstoqueSAP = deposito.QuantidadeEstoqueSAP;
                        novoMaterial.Deposito = deposito.Nome;
                        novoMaterial.PosicaoDeposito = deposito.PosicaoDeposito;
                        novoMaterial.NumeroLote = deposito.NumeroLote;
                        novoMaterial.Validade = deposito.Validade;

                        novaLista.Add(novoMaterial);

                        if (qtdSolicitada <= 0)
                            break;
                    }
                }

                return novaLista;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<MaterialAtendimento> BuscarTransferencia()
        {
            try
            {
                var codsTransf = atendimentoTransferenciaRepository.FindAll().ToList();

                var materiais = sapFacade.BuscarTransferencia(codsTransf.Select(x => x.CodMaterial).ToArray());

                materiais.OrderBy(x => x.Validade).ToList();

                return materiais;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<MaterialAcompanhamento> BuscarAcompanhamentos()
        {
            try
            {
                var materiais = repositoryAcompanhamento.FindAll();

                return materiais;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string Atender(IEnumerable<MaterialAtendimento> materiais)
        {
            try
            {
                var retorno = string.Empty;
                var numReserva = materiais.First().NumeroReserva;

                if (!string.IsNullOrWhiteSpace(numReserva))
                    retorno = sapFacade.AtenderPorReserva(materiais);
                else
                    retorno = sapFacade.AtenderPorTransferencia(materiais);

                foreach (var material in materiais)
                {
                    material.RetornoSap = retorno;
                }

                repositoryBase.AddRange(materiais.Where(x => !x.Id.HasValue));
                repositoryBase.UpdateRange(materiais.Where(x => x.Id.HasValue));

                return retorno;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string AtenderCompraDireta(IEnumerable<MaterialAtendimento> materiais)
        {
            try
            {
                if (materiais == null || !materiais.Any())
                {
                    return "Não foram enviados materiais para atendimento!";
                }

                foreach (var material in materiais)
                {
                    material.DataHoraFim = DateTime.Now;
                    material.TipoAtendimento = TipoAtendimento.COMPRADIRETA;
                    material.Situacao = Situacao.CONCLUIDO;
                }

                repositoryBase.AddRange(materiais);

                return "OK";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string AtenderVisitante(IEnumerable<MaterialAcompanhamento> materiais)
        {
            try
            {
                repositoryAcompanhamento.AddRange(materiais);

                return "OK";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void AtenderPendentes()
        {
            var pendentes = repositoryBase.FindByCriteria(x => x.Situacao == Situacao.PENDENTE && x.TipoAtendimento != TipoAtendimento.COMPRADIRETA).ToList();

            if (pendentes.Any())
                Atender(pendentes);
        }

        public void SalvarDivergentes(IEnumerable<MaterialAtendimento> materiais)
        {
            foreach (var material in materiais)
            {
                var existente = repositoryBase.FindByCriteria(x => x.NumeroReserva == material.NumeroReserva && x.CodMaterial == material.CodMaterial && x.NumeroLote == material.NumeroLote).FirstOrDefault();

                material.RetornoSap = "O material não cumpre os requisitos de quantidade ou validade.";

                if (!existente.IsNotNull())
                    repositoryBase.Add(material);
            }
        }

        public IEnumerable<MaterialAcompanhamento> FiltrarAcompanhamento(MaterialAtendimento filtro)
        {
            return repositoryAcompanhamento.FindByCriteria(x =>
                              (x.CodMaterial == filtro.CodMaterial || string.IsNullOrEmpty(filtro.CodMaterial)) &&
                              (x.DataHoraInicio >= filtro.DataHoraInicio || !filtro.DataHoraInicio.HasValue) &&
                              (x.DataHoraFim <= filtro.DataHoraFim || !filtro.DataHoraFim.HasValue) &&
                              (x.Usuario == filtro.Usuario || string.IsNullOrEmpty(filtro.Usuario)));
        }

        public IEnumerable<MaterialAtendimento> Filtrar(MaterialAtendimento filtro)
        {
            return repository.FindByCriteria(x =>
                              (x.Nome == filtro.Nome.Trim() || string.IsNullOrEmpty(filtro.Nome)) &&
                              (x.CodMaterial == filtro.CodMaterial || string.IsNullOrEmpty(filtro.CodMaterial)) &&
                              (x.NumeroReserva == filtro.NumeroReserva || string.IsNullOrEmpty(filtro.NumeroReserva)) &&
                              (x.DataHoraInicio >= filtro.DataHoraInicio || !filtro.DataHoraInicio.HasValue) &&
                              (x.DataHoraFim <= filtro.DataHoraFim || !filtro.DataHoraFim.HasValue) &&
                              (x.Situacao == filtro.Situacao || string.IsNullOrEmpty(filtro.Situacao)) &&
                              (x.TipoAtendimento == filtro.TipoAtendimento || string.IsNullOrEmpty(filtro.TipoAtendimento)) &&
                              (x.Usuario == filtro.Usuario || string.IsNullOrEmpty(filtro.Usuario)));
        }
    }
}


