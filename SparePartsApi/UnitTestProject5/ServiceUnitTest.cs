using Microsoft.VisualStudio.TestTools.UnitTesting;
using Application;
using Infrastructure;
using System.Linq;
using System;
using Domain;
using System.IO;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Collections.Generic;

namespace UnitTestProject5
{
    [TestClass]
    public class ServiceUnitTest
    {
        private AtendimentoService atendimentoService;
        private RecebimentoService recebimentoService;
        private EmailService emailService;
        private ImpressaoService impressaoService;
        
        public ServiceUnitTest()
        {
            atendimentoService = new AtendimentoService(new AtendimentoRepository(), new SapIntegrationFacade(), new AtendimentoTransferenciaRepository());

            recebimentoService = new RecebimentoService(new RecebimentoRepository(), new SapIntegrationFacade(), new EmailService(new EmailEnviadoRepository()));

            impressaoService = new ImpressaoService();

            emailService = new EmailService(new EmailEnviadoRepository());
        }

        [TestMethod]
        [DataRow("180029030")]
        public void BuscarListaRecebimento(string numLista)
        {
            var lista = recebimentoService.BuscarListaRecebimento(numLista);

            Assert.IsTrue(lista.Materiais.Any());
        }

        [TestMethod]
        [DataRow("180029030")]
        public void Receber(string numLista)
        {
            var lista = recebimentoService.BuscarListaRecebimento(numLista);

            Assert.IsTrue(lista.Materiais.Any());

            foreach (var material in lista.Materiais)
            {
                material.PosicaoDeposito = "A1B2C3D4";
                material.Deposito = "DM15";
                material.QuantidadeEstoqueFisico = material.Quantidade;
                material.Usuario = "12004564";

                if (material.TemLote && !string.IsNullOrWhiteSpace(material.UnidadeTempo))
                {
                    material.Lotes.Add(new Lote()
                    {
                        NumeroLote = "Lote1",
                        Validade = !string.IsNullOrWhiteSpace(material.UnidadeTempo) ? (DateTime?)DateTime.Now.AddDays(100) : null,
                        Quantidade = material.Quantidade
                    });
                }
            }

            var resultado = recebimentoService.CriarMigo(lista);

            if (resultado.ToString().StartsWith("OK:"))
            {
                var numDoc = resultado.ToString().Split(':')[1];
                var ano = DateTime.Now.ToString("yyyy");
                File.AppendAllText("log.txt", "RECEBIMENTO: " + numDoc + Environment.NewLine);
                new SapIntegrationFacade().CancelarDocumento(numDoc, ano);
            }

            Assert.IsTrue(resultado.ToString().StartsWith("OK:"));
        }


        [TestMethod]
        [DataRow("3342685")]
        public void BuscarReserva(string reserva)
        {
            var materiais = atendimentoService.BuscarReserva(reserva);

            Assert.IsTrue(materiais.Any());
        }


        [TestMethod]
        [DataRow("3133334")]
        public void AtenderReserva(string reserva)
        {
            try
            {
                var materiais = atendimentoService.BuscarReserva(reserva);

                foreach (var material in materiais)
                {
                    var deposito = material.Depositos.FirstOrDefault();

                    material.QuantidadeRetirada = 1;
                    material.Usuario = "dmreis";
                    material.UsuarioAtendido = "dmreis";
                    material.Deposito = deposito?.Nome;
                    material.QuantidadeEstoqueSAP = deposito?.QuantidadeEstoqueSAP;
                    material.NumeroLote = deposito?.NumeroLote;
                    material.PosicaoDeposito = deposito?.PosicaoDeposito;
                    material.Validade = deposito?.Validade;
                    material.QuantidadeEstoqueFisico = 1;
                }

                var retorno = atendimentoService.Atender(materiais);

                Assert.IsTrue(retorno.StartsWith("OK:"));
            }
            catch (Exception ex)
            {

            }
        }

        [TestMethod]
        public void AtenderPendentes()
        {
            atendimentoService.AtenderPendentes();
        }


        [TestMethod]
        public void AtenderTransferencia()
        {
            var materiais = atendimentoService.BuscarTransferencia();

            foreach (var material in materiais)
            {
                var deposito = material.Depositos.FirstOrDefault();

                material.QuantidadeRetirada = 1;
                material.Usuario = "dmreis";
                material.UsuarioAtendido = "dmreis";
                material.Deposito = deposito?.Nome;
                material.QuantidadeEstoqueSAP = deposito?.QuantidadeEstoqueSAP;
                material.NumeroLote = deposito?.NumeroLote;
                material.PosicaoDeposito = deposito?.PosicaoDeposito;
                material.Validade = deposito?.Validade;
                material.QuantidadeEstoqueFisico = 1;
            }

            var retorno = atendimentoService.Atender(materiais);

            Assert.IsTrue(retorno.StartsWith("OK:"));
        }

        [TestMethod]
        public void EnviarEmail()
        {
            try
            {
                string emails = ConfigurationManager.AppSettings["Emails"].ToString();

                emailService.EnviarEmail(""));
            }
            catch (Exception ex)
            {
                var msg = ex.GetInnerExceptionMessage();
            }
        }

        [TestMethod]
        public void ImprimirMaterial()
        {
            try
            {
                var material = new MaterialRecebimento()
                {
                    CodMaterial = "12345678",
                    Nome = "COLA BASTÃO DE ESCRITÓRIO",
                    Deposito = "DM15",
                    Ano = 2019,
                    NumeroDocumento = "5000000188",
                    NumeroPedidoItem = "00001"
                };

                var dir = @"C:\WorkSpace\ \Micro Services\SparePartsApi\SparePartsWebApi\Labels\";

                string ZPLString = File.ReadAllText(Path.Combine(dir, "label_volumes.prn"));

                var etiquetas = new List<string>();

                var etiqueta = ZPLString
                    .Replace("@barcode", material.CodMaterial.TrimStart('0'))
                    .Replace("@nome", material.Nome ?? "")
                    .Replace("@destino", material.DepositoDestino ?? "")
                    .Replace("@entrada", material.DepositoEtiqueta ?? "")
                    .Replace("@qrcode", $@"{material.Ano}/{material.NumeroDocumento}/{material.NumeroPedidoItem.PadLeft(5, '0')}");

                etiquetas.Add(etiqueta);

                impressaoService.Imprimir(etiquetas);
            }
            catch (Exception ex)
            {
                var msg = ex.GetInnerExceptionMessage();
            }
        }


    }
}
