using Application;
using Domain;
using Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace UnitTestProject
{
    [TestClass]
    public class ServiceUnitTest
    {
        private AtendimentoService atendimentoService;
        private RecebimentoService recebimentoService;
        private EmailService emailService;
        private ImpressaoService impressaoService;
        private ISapIntegrationFacade sap;

        public ServiceUnitTest()
        {
            sap = new SapIntegrationFacade();

            atendimentoService = new AtendimentoService(new AtendimentoRepository(), new AtendimentoTransferenciaRepository(), new AcompanhamentoRepository());

            recebimentoService = new RecebimentoService(new RecebimentoRepository(), new EmailService(new EmailEnviadoRepository()), sap);

            emailService = new EmailService(new EmailEnviadoRepository());

            impressaoService = new ImpressaoService();
        }

        #region RECEBIMENTO


        [TestMethod]
        [DataRow("180174942")]
        public void BuscarLista(string numLista)
        {
            try
            {
                var lista = recebimentoService.BuscarListaRecebimento(numLista);

                Assert.IsTrue(lista.Materiais.Any());
            }
            catch (Exception e)
            {
                var erro = e.GetInnerExceptionMessage();
                Assert.Fail(erro);
            }
        }

        [TestMethod]
        [DataRow("180174998")]
        public void Receber(string numLista)
        {

            try
            {
                var lista = recebimentoService.BuscarListaRecebimento(numLista);

                Assert.IsTrue(lista.Materiais.Any());

                foreach (var material in lista.Materiais)
                {
                    material.PosicaoDeposito = "PALETE A";
                    material.quantidadeEstoqueFisico = material.Quantidade;
                    material.Usuario = "debug";

                    if (material.TemLote && !string.IsNullOrWhiteSpace(material.UnidadeTempo))
                    {
                        material.Lotes.Add(new Lote()
                        {
                            NumeroLote = "Lote1",
                            Validade = !string.IsNullOrWhiteSpace(material.UnidadeTempo) ? (DateTime?)DateTime.Now.AddDays(100) : null,
                            Quantidade = material.Quantidade / 2
                        });

                        material.Lotes.Add(new Lote()
                        {
                            NumeroLote = "Lote2",
                            Validade = !string.IsNullOrWhiteSpace(material.UnidadeTempo) ? (DateTime?)DateTime.Now.AddDays(100) : null,
                            Quantidade = material.Quantidade / 2
                        });
                    }
                }

                var resultado = recebimentoService.CriarMigo(lista);

                if (resultado.Success)
                {
                    var numDoc = resultado.Message.Split(':')[1];
                    var ano = DateTime.Now.ToString("yyyy");
                    File.AppendAllText("log.txt", "RECEBIMENTO: " + numDoc + Environment.NewLine);
                    var docCancel = sap.CancelarDocumento(numDoc, ano);
                }
                else
                    Assert.Fail(resultado.Message);

                Assert.IsTrue(resultado.Success);
            }
            catch (Exception e)
            {
                var erro = e.GetInnerExceptionMessage();
                Assert.Fail(erro);
            }
        }

        [TestMethod]
        [DataRow(2020, "5000002850", "0001")]
        public void ConsultarDocumento(int ano, string documento, string item)
        {
            var material = sap.ConsultarDocumento(ano, documento, item);

            Assert.IsNotNull(material);
        }

        [TestMethod]
        [DataRow("5000000078", "2020")]
        public void CancelarDocumento(string numDoc, string ano)
        {
            var retorno = sap.CancelarDocumento(numDoc, ano);

            Assert.AreEqual("OK", retorno);
        }

        #endregion

        #region ARMAZENAGEM

        [TestMethod]
        //[DataRow(2020, "5000001593", "0001")]
        [DataRow(2020, "5000121358", "0001")]
        public void BuscarPendente(int ano, string documento, string item)
        {
            var material = sap.BuscarMaterialPendenteArmazenagem(ano, documento, item);
        }

        [TestMethod]
        [DataRow(2020, "5000121358", "0001")]
        public void Armazenar(int ano, string documento, string item)
        {
            try
            {
                var material = sap.BuscarMaterialPendenteArmazenagem(ano, documento, item);

                var matArm = new MaterialArmazenagem();

                matArm.CodMaterial = material.CodMaterial;
                matArm.PosicaoDeposito = material.PosicaoDeposito;
                matArm.DepositoDestino = material.DepositoDestino;
                matArm.Quantidade = material.Quantidade;
                matArm.UnidadeMedida = material.UnidadeMedida;

                var resultado = sap.Armazenar(matArm, "DM15");

                if (resultado.ToString().StartsWith("OK:"))
                {
                    var numDoc = resultado.ToString().Split(':')[1];
                    var anoDoc = DateTime.Now.ToString("yyyy");
                    File.AppendAllText(Environment.CurrentDirectory + "log.txt", "ARMAZENAGEM: " + numDoc + Environment.NewLine);
                    var docCancel = sap.CancelarDocumento(numDoc, anoDoc);
                }

                Assert.IsTrue(resultado.ToString().StartsWith("OK:"));
            }
            catch (Exception e)
            {
                var erro = e.GetInnerExceptionMessage();
                Assert.Fail(erro);
            }
        }

        #endregion

        #region MOVIMENTAÇÃO

        [TestMethod]
        [DataRow("ABCD1234")]
        public void BuscarMateriaisArmazenados(string posicaoOrigem)
        {
            try
            {
                var materiais = sap.BuscarMateriaisArmazenados(posicaoOrigem);

                Assert.IsTrue(materiais.Any());

            }
            catch (Exception ex)
            {

            }
        }

        [TestMethod]
        [DataRow("738134", "DM14", "01C04A26A")]
        public void Movimentar(string codMaterial, string depositoDestino, string posicaoDestino)
        {
            var material = new MaterialMovimentacao() { CodMaterial = codMaterial, Deposito = depositoDestino, PosicaoDeposito = posicaoDestino };

            var retorno = sap.Movimentar(material);
        }

        #endregion

        #region INVENTÁRIO

        [TestMethod]
        [DataRow("100000003")]
        public void BuscarInventario(string documentoInventario)
        {
            var anoFiscal = DateTime.Now.Year.ToString();

            var materiais = sap.BuscarInventario(documentoInventario, anoFiscal);

            Assert.IsTrue(materiais.Any());
        }

        [TestMethod]
        [DataRow("100000334")]
        public void Inventariar(string documentoInventario)
        {
            var anoFiscal = DateTime.Now.Year.ToString();

            var materiais = sap.BuscarInventario(documentoInventario, anoFiscal);

            Assert.IsTrue(materiais.Any());

            //materiais.ForEach(x => { x.Quantidade = 1; x.Usuario = "dmreis"; });

            var retorno = sap.Inventariar(documentoInventario, anoFiscal, materiais);

            //Assert.IsTrue(retorno.ToString().StartsWith("OK:"));
        }

        #endregion

        #region ATENDIMENTO

        [TestMethod]
        //[DataRow("4027825")]
        [DataRow("3806829")]
        public void BuscarReserva(string reserva)
        {
            var materiais = atendimentoService.BuscarReserva(reserva);

            Assert.IsTrue(materiais.Any());
        }

        [TestMethod]
        [DataRow("4027825")]
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


        //[TestMethod]
        //[DataRow("3189266")]
        //public void AtenderCompraDireta(string reserva)
        //{
        //    try
        //    {
        //        var materiais = atendimentoService.AtenderCompraDireta(reserva);

        //        foreach (var material in materiais)
        //        {
        //            var deposito = material.Depositos.FirstOrDefault();

        //            material.QuantidadeRetirada = 1;
        //            material.Usuario = "dmreis";
        //            material.UsuarioAtendido = "dmreis";
        //            material.Deposito = deposito?.Nome;
        //            material.QuantidadeEstoqueSAP = deposito?.QuantidadeEstoqueSAP;
        //            material.NumeroLote = deposito?.NumeroLote;
        //            material.PosicaoDeposito = deposito?.PosicaoDeposito;
        //            material.Validade = deposito?.Validade;
        //            material.QuantidadeEstoqueFisico = 1;
        //        }

        //        var retorno = atendimentoService.Atender(materiais);

        //        Assert.IsTrue(retorno.StartsWith("OK:"));
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}

        #endregion

        #region GERAL

        [TestMethod]
        public void Autenticar()
        {
            try
            {
                var username = "servicecolheita";

                var resourceUri = ConfigurationManager.ConnectionStrings["LDAP"].ToString();

                var dirEntry = new System.DirectoryServices.DirectoryEntry(resourceUri, username, "s!8232.8");

                Assert.AreEqual(username, dirEntry.Username);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [TestMethod]
        //[DataRow("http://spa.bahiapulp.com/api/atendimento/EnviarAssinatura/", @"C:\WorkSpace\ \Micro Services\SparePartsApi\UnitTestProject\foto.jpg")]
        [DataRow("http://srv14/api/atendimento/EnviarAssinatura/", @"C:\WorkSpace\ \Micro Services\SparePartsApi\UnitTestProject\foto.jpg")]
        public void FileUpload(string requestUri, string filePath)
        {
            try
            {
                using (HttpClient httpClient = new HttpClient() { Timeout = TimeSpan.FromMinutes(5) })
                using (MultipartFormDataContent content = new MultipartFormDataContent())
                using (FileStream fileStream = File.OpenRead(filePath))
                using (StreamContent fileContent = new StreamContent(fileStream))
                {
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                    fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                    {
                        FileName = filePath
                    };

                    fileContent.Headers.Add("name", Path.GetFileName(filePath));
                    content.Add(fileContent);

                    var result = httpClient.PostAsync(requestUri, content).Result;

                    result.EnsureSuccessStatusCode();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [TestMethod]
        public void EnviarEmail()
        {
            try
            {
                string emails = ConfigurationManager.AppSettings["Emails"].ToString();

                emailService.EnviarEmail("");
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
                    Nome = "ATENÇÃO É BASTÃO",
                    Deposito = "DM15",
                    Ano = 2020,
                    NumeroDocumento = "5000000188",
                    NumeroItem = "00001"
                };

                var teste = Path.Combine(Environment.CurrentDirectory, "testfile.txt");
                var dir = @"C:\WorkSpace\ \Micro Services\SparePartsApi\SparePartsWebApi\Labels\";

                string ZPLString = File.ReadAllText(Path.Combine(dir, "label_material.prn"));

                var etiquetas = new List<string>();

                var etiqueta = ZPLString.Replace("@barcode", material.CodMaterial.TrimStart('0'));
                etiqueta = etiqueta.Replace("@nome", material.Nome ?? "");
                etiqueta = etiqueta.Replace("@entrada", material.DepositoEtiqueta ?? "");
                etiqueta = etiqueta.Replace("@destino", material.DepositoDestinoEtiqueta ?? "");
                etiqueta = etiqueta.Replace("@lote", material.NumeroLote.HasValue() ? "Lote:" + material.NumeroLote : "");
                etiqueta = etiqueta.Replace("@qrcode", $@"{material.Ano}/{material.NumeroDocumento}/{material.NumeroItem.PadLeft(5, '0')}");
                etiqueta = etiqueta.Replace("@datahora", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

                etiquetas.Add(etiqueta);

                impressaoService.Imprimir(etiquetas);
            }
            catch (Exception ex)
            {
                var msg = ex.GetInnerExceptionMessage();
            }
        }

        [TestMethod]
        public void GetMaterial()
        {
            try
            {

            }
            catch (Exception ex)
            {
                var msg = ex.GetInnerExceptionMessage();
            }
        }


        #endregion

    }
}

