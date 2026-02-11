using Application;
using Domain;
using Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitTestProject
{
    [TestClass]
    public class ServiceUnitTestMocked
    {
        [TestMethod]
        public void ReceberSemLote()
        {

            var listaRecebimento = new ListaRecebimento();

            var semLote1 = new MaterialRecebimento()
            {
                CodMaterial = "000000007300039043",
                Nome = "TELHA CALANDRADA;00060577;DEMUTH",
                NumeroItem = "0001",
                Quantidade = 20,
                UnidadeMedida = "UN",
                NumeroPedido = "4500459484",
                NumeroPedidoItem = "00010",
                NumeroLote = "MANUTENÇÃO",
                TemLote = false,
                QuantidadePO = 20,
                NumeroNota = "1",
                DataCriacaoRC = DateTime.Now.AddDays(-1),
                DataHoraInicio = DateTime.Now,
            };

            var semLote2 = new MaterialRecebimento()
            {
                CodMaterial = "000000007300039044",
                Nome = "PARAFUSO AUTOBROCANTE;5MM;PB12-24 1.1/2",
                NumeroItem = "0002",
                Quantidade = 100,
                UnidadeMedida = "UN",
                NumeroPedido = "4500459484",
                NumeroPedidoItem = "00020",
                NumeroLote = "MANUTENÇÃO",
                TemLote = false,
                QuantidadePO = 100,
                NumeroNota = "1",
                DataCriacaoRC = DateTime.Now,
                DataHoraInicio = DateTime.Now,
            };

            var semLote3 = new MaterialRecebimento()
            {
                CodMaterial = "000000007300039043",
                Nome = "TELHA CALANDRADA;00060577;DEMUTH",
                NumeroItem = "0003",
                Quantidade = 20,
                UnidadeMedida = "UN",
                NumeroPedido = "4500459484",
                NumeroPedidoItem = "00030",
                NumeroLote = "MANUTENÇÃO",
                TemLote = false,
                QuantidadePO = 20,
                NumeroNota = "1",
                DataCriacaoRC = DateTime.Now,
                DataHoraInicio = DateTime.Now,
            };

            var semLote4 = new MaterialRecebimento()
            {
                CodMaterial = "000000007300039044",
                Nome = "PARAFUSO AUTOBROCANTE;5MM;PB12-24 1.1/2",
                NumeroItem = "0004",
                Quantidade = 100,
                UnidadeMedida = "UN",
                NumeroPedido = "4500459484",
                NumeroPedidoItem = "00040",
                NumeroLote = "MANUTENÇÃO",
                TemLote = false,
                QuantidadePO = 100,
                NumeroNota = "1",
                DataCriacaoRC = DateTime.Now,
                DataHoraInicio = DateTime.Now,
            };

            listaRecebimento.Materiais.Add(semLote1);
            listaRecebimento.Materiais.Add(semLote2);
            listaRecebimento.Materiais.Add(semLote3);
            listaRecebimento.Materiais.Add(semLote4);

            var sapFacadeMocked = new Mock<ISapIntegrationFacade>();

            var recebimentoRepository = new Mock<RecebimentoRepository>();

            var emailService = new Mock<EmailService>();

            var emailEnviadoRepository = new Mock<EmailEnviadoRepository>();

            sapFacadeMocked.Setup(x => x.BuscarListaRecebimento(It.IsAny<string>())).Returns(listaRecebimento);

            var recebimentoService = new RecebimentoService(recebimentoRepository.Object, new EmailService(emailEnviadoRepository.Object), sapFacadeMocked.Object);

            var materiais = recebimentoService.BuscarListaRecebimento("1").Materiais.ToList();

            Assert.IsTrue(materiais.Count == 2);

            string email = string.Empty;

            var resultado = recebimentoService.RetornaListaTratada(materiais, ref email, new List<string>(), new List<string>());

            Assert.IsTrue(resultado.Count == 4);

            var mat1 = resultado.Single(x => x.NumeroPedidoItem == "00010");
            var mat2 = resultado.Single(x => x.NumeroPedidoItem == "00020");
            var mat3 = resultado.Single(x => x.NumeroPedidoItem == "00030");
            var mat4 = resultado.Single(x => x.NumeroPedidoItem == "00040");

            Assert.AreEqual(Deposito.ARMAZENAGEM, mat1.Deposito);
            Assert.IsTrue(string.IsNullOrWhiteSpace(mat1.DepositoDestino));
            Assert.AreEqual(20, mat1.Quantidade);

            Assert.AreEqual(Deposito.ARMAZENAGEM, mat2.Deposito);
            Assert.IsTrue(string.IsNullOrWhiteSpace(mat2.DepositoDestino));
            Assert.AreEqual(100, mat2.Quantidade);

            Assert.AreEqual(Deposito.ARMAZENAGEM, mat3.Deposito);
            Assert.IsTrue(string.IsNullOrWhiteSpace(mat3.DepositoDestino));
            Assert.AreEqual(20, mat3.Quantidade);

            Assert.AreEqual(Deposito.ARMAZENAGEM, mat4.Deposito);
            Assert.IsTrue(string.IsNullOrWhiteSpace(mat4.DepositoDestino));
            Assert.AreEqual(100, mat4.Quantidade);

        }

        [TestMethod]
        public void ReceberSemLote2()
        {
            try
            {
                var listaRecebimento = new ListaRecebimento();

                var material1 = new MaterialRecebimento()
                {
                    CodMaterial = "000000000000596355",
                    Nome = "PARAFUSO ALLEN;M42X4.50;240MM;CL8.8",
                    NumeroItem = "0001",
                    Quantidade = 5,
                    UnidadeMedida = "CDA",
                    NumeroPedido = "4500462949",
                    NumeroPedidoItem = "00010",
                    DepositoDestino = "DM02",
                    TemLote = false,
                    DepositoRC = "DM02",
                    QuantidadePO = 8,
                    NumeroNota = "1",
                    DataCriacaoRC = DateTime.Now,
                    DataHoraInicio = DateTime.Now,
                };

                var material2 = new MaterialRecebimento()
                {
                    CodMaterial = "000000000000596355",
                    Nome = "PARAFUSO ALLEN;M42X4.50;240MM;CL8.8",
                    NumeroItem = "0002",
                    Quantidade = 16,
                    UnidadeMedida = "CDA",
                    NumeroPedido = "4500462949",
                    NumeroPedidoItem = "00020",
                    DepositoDestino = "DM10",
                    TemLote = false,
                    DepositoRC = "DM10",
                    QuantidadePO = 16,
                    NumeroNota = "1",
                    DataCriacaoRC = DateTime.Now,
                    DataHoraInicio = DateTime.Now,
                };

                listaRecebimento.Materiais.Add(material1);
                listaRecebimento.Materiais.Add(material2);

                var sapFacadeMocked = new Mock<ISapIntegrationFacade>();

                sapFacadeMocked.Setup(x => x.BuscarListaRecebimento(It.IsAny<string>())).Returns(listaRecebimento);

                var recebimentoService = new RecebimentoService(new RecebimentoRepository(), new EmailService(new EmailEnviadoRepository()), sapFacadeMocked.Object);

                var materiais = recebimentoService.BuscarListaRecebimento("1").Materiais.ToList();

                Assert.IsTrue(materiais.Count == 1);

                string email = string.Empty;

                var resultado = recebimentoService.RetornaListaTratada(materiais, ref email, new List<string>(), new List<string>());

                Assert.IsTrue(resultado.Count == 2);

                var mat1 = resultado.Single(x => x.NumeroPedidoItem == "00010");
                var mat2 = resultado.Single(x => x.NumeroPedidoItem == "00020");

                Assert.AreEqual(Deposito.ARMAZENAGEM, mat1.Deposito);
                Assert.AreEqual("DM02", mat1.DepositoDestino);
                Assert.AreEqual(5, mat1.Quantidade);

                Assert.AreEqual(Deposito.ARMAZENAGEM, mat2.Deposito);
                Assert.AreEqual("DM10", mat2.DepositoDestino);
                Assert.AreEqual(16, mat2.Quantidade);
            }
            catch (Exception ex)
            {
                var msg = ex.GetInnerExceptionMessage();
            }
        }

        [TestMethod]
        public void ReceberSemLote3()
        {
            try
            {
                var listaRecebimento = new ListaRecebimento();

                var material1 = new MaterialRecebimento()
                {
                    CodMaterial = "000000000000738786",
                    Nome = "ROLAMENTO DE ESFERAS;LAT SKF/7314BECBM",
                    NumeroItem = "0003",
                    Quantidade = 2,
                    quantidadeEstoqueFisico = 6,
                    UnidadeMedida = "CDA",
                    NumeroPedido = "4500462814",
                    NumeroPedidoItem = "00030",
                    DepositoDestino = "DPRO",
                    TemLote = true,
                    NumeroLote = "MANUTENÇÃO",
                    DepositoRC = "DPRO",
                    QuantidadePO = 2,
                    NumeroNota = "1",
                    DataCriacaoRC = DateTime.Now,
                    DataHoraInicio = DateTime.Now,
                };

                var material2 = new MaterialRecebimento()
                {
                    CodMaterial = "000000000000738786",
                    Nome = "ROLAMENTO DE ESFERAS;LAT SKF/7314BECBM",
                    NumeroItem = "0004",
                    Quantidade = 2,
                    quantidadeEstoqueFisico = 6,
                    UnidadeMedida = "CDA",
                    NumeroPedido = "4500462814",
                    NumeroPedidoItem = "00040",
                    DepositoDestino = "DPRO",
                    TemLote = true,
                    NumeroLote = "MANUTENÇÃO",
                    DepositoRC = "DPRO",
                    QuantidadePO = 2,
                    NumeroNota = "1",
                    DataCriacaoRC = DateTime.Now,
                    DataHoraInicio = DateTime.Now,
                };

                var material3 = new MaterialRecebimento()
                {
                    CodMaterial = "000000000000738786",
                    Nome = "ROLAMENTO DE ESFERAS;LAT SKF/7314BECBM",
                    NumeroItem = "0005",
                    Quantidade = 2,
                    quantidadeEstoqueFisico = 6,
                    UnidadeMedida = "CDA",
                    NumeroPedido = "4500462814",
                    NumeroPedidoItem = "00050",
                    DepositoDestino = "DPRO",
                    TemLote = true,
                    NumeroLote = "MANUTENÇÃO",
                    DepositoRC = "DPRO",
                    QuantidadePO = 2,
                    NumeroNota = "1",
                    DataCriacaoRC = DateTime.Now,
                    DataHoraInicio = DateTime.Now,
                };

                listaRecebimento.Materiais.Add(material1);
                listaRecebimento.Materiais.Add(material2);
                listaRecebimento.Materiais.Add(material3);

                var sapFacadeMocked = new Mock<ISapIntegrationFacade>();

                sapFacadeMocked.Setup(x => x.BuscarListaRecebimento(It.IsAny<string>())).Returns(listaRecebimento);

                var recebimentoService = new RecebimentoService(new RecebimentoRepository(), new EmailService(new EmailEnviadoRepository()), sapFacadeMocked.Object);

                var materiais = recebimentoService.BuscarListaRecebimento("1").Materiais.ToList();

                Assert.IsTrue(materiais.Count == 1);

                string email = string.Empty;

                var resultado = recebimentoService.RetornaListaTratada(materiais, ref email, new List<string>(), new List<string>());

                Assert.IsTrue(resultado.Count == 3);

                var mat1 = resultado.Single(x => x.NumeroPedidoItem == "00030");
                var mat2 = resultado.Single(x => x.NumeroPedidoItem == "00040");
                var mat3 = resultado.Single(x => x.NumeroPedidoItem == "00050");

                Assert.AreEqual(Deposito.ARMAZENAGEM, mat1.Deposito);
                Assert.AreEqual("DPRO", mat1.DepositoDestino);
                Assert.AreEqual(2, mat1.Quantidade);

                Assert.AreEqual(Deposito.ARMAZENAGEM, mat2.Deposito);
                Assert.AreEqual("DPRO", mat2.DepositoDestino);
                Assert.AreEqual(2, mat2.Quantidade);

                Assert.AreEqual(Deposito.ARMAZENAGEM, mat2.Deposito);
                Assert.AreEqual("DPRO", mat2.DepositoDestino);
                Assert.AreEqual(2, mat2.Quantidade);
            }
            catch (Exception ex)
            {
                var msg = ex.GetInnerExceptionMessage();
            }
        }

        [TestMethod]
        public void ReceberComLote3()
        {
            try
            {
                var listaRecebimento = new ListaRecebimento();

                var material1 = new MaterialRecebimento()
                {
                    CodMaterial = "000000000000596355",
                    Nome = "PARAFUSO ALLEN;M42X4.50;240MM;CL8.8",
                    NumeroItem = "0001",
                    Quantidade = 5,
                    UnidadeMedida = "CDA",
                    NumeroPedido = "4500462949",
                    NumeroPedidoItem = "00010",
                    DepositoDestino = "DM02",
                    TemLote = true,
                    DepositoRC = "DM02",
                    QuantidadePO = 8,
                    NumeroNota = "1",
                    DataCriacaoRC = DateTime.Now,
                    DataHoraInicio = DateTime.Now,
                };

                var material2 = new MaterialRecebimento()
                {
                    CodMaterial = "000000000000596355",
                    Nome = "PARAFUSO ALLEN;M42X4.50;240MM;CL8.8",
                    NumeroItem = "0002",
                    Quantidade = 16,
                    UnidadeMedida = "CDA",
                    NumeroPedido = "4500462949",
                    NumeroPedidoItem = "00020",
                    DepositoDestino = "DM10",
                    TemLote = true,
                    DepositoRC = "DM10",
                    QuantidadePO = 16,
                    NumeroNota = "1",
                    DataCriacaoRC = DateTime.Now,
                    DataHoraInicio = DateTime.Now,
                };

                var lote1 = new Lote()
                {
                    CodMaterial = "000000000000596355",
                    NumeroLote = "LOTE1",
                    Quantidade = 10,
                    Validade = DateTime.Now.AddYears(1)
                };

                var lote2 = new Lote()
                {
                    CodMaterial = "000000000000596355",
                    NumeroLote = "LOTE2",
                    Quantidade = 11,
                    Validade = DateTime.Now.AddYears(1)
                };

                listaRecebimento.Materiais.Add(material1);
                listaRecebimento.Materiais.Add(material2);

                var sapFacadeMocked = new Mock<ISapIntegrationFacade>();

                sapFacadeMocked.Setup(x => x.BuscarListaRecebimento(It.IsAny<string>())).Returns(listaRecebimento);

                var recebimentoService = new RecebimentoService(new RecebimentoRepository(), new EmailService(new EmailEnviadoRepository()), sapFacadeMocked.Object);

                var materiais = recebimentoService.BuscarListaRecebimento("1").Materiais.ToList();

                Assert.IsTrue(materiais.Count == 1);

                materiais.First().Lotes.Add(lote1);
                materiais.First().Lotes.Add(lote2);

                string email = string.Empty;

                var resultado = recebimentoService.RetornaListaTratada(materiais, ref email, new List<string>(), new List<string>());

                Assert.IsTrue(resultado.Count >= 3);

                var mat1 = resultado.First(x => x.NumeroPedidoItem == "00010");
                var mat2 = resultado.Last(x => x.NumeroPedidoItem == "00010");
                var mat3 = resultado.First(x => x.NumeroPedidoItem == "00020");
                var mat4 = resultado.Last(x => x.NumeroPedidoItem == "00020");

                Assert.AreEqual(Deposito.ARMAZENAGEM, mat1.Deposito);
                Assert.AreEqual("DM02", mat1.DepositoDestino);
                Assert.AreEqual(5, mat1.Quantidade);

                Assert.AreEqual(Deposito.ARMAZENAGEM, mat2.Deposito);
                Assert.AreEqual("DM02", mat2.DepositoDestino);
                Assert.AreEqual(5, mat2.Quantidade);

                Assert.AreEqual(Deposito.ARMAZENAGEM, mat3.Deposito);
                Assert.AreEqual("DM10", mat3.DepositoDestino);
                Assert.AreEqual(5, mat3.Quantidade);

                Assert.AreEqual(Deposito.ARMAZENAGEM, mat4.Deposito);
                Assert.AreEqual("DM10", mat4.DepositoDestino);
                Assert.AreEqual(6, mat4.Quantidade);
            }
            catch (Exception ex)
            {
                var msg = ex.GetInnerExceptionMessage();
            }
        }

    }
}
