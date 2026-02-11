using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Infrastructure;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.IO;
using System.Net.Http.Headers;
using Domain;

namespace UnitTestProject5
{
    [TestClass]
    public class SAPUnitTest
    {
        SapIntegrationFacade sap = new SapIntegrationFacade();

        #region RECEBIMENTO

        [TestMethod]
        [DataRow("180174876")]
        public void Receber(string CodListaRecebimento)
        {
            var lista = sap.BuscarListaRecebimento(CodListaRecebimento);

            Assert.IsTrue(lista.Materiais.Any());

            foreach (var material in lista.Materiais)
            {
                material.PosicaoDeposito = "A1B2C3D4";
                material.Deposito = "DM15";

                if (material.TemLote && !string.IsNullOrWhiteSpace(material.UnidadeTempo))
                {
                    material.Lotes.Add(new Lote()
                    {
                        CodMaterial = material.CodMaterial,
                        Deposito = material.Deposito,
                        NumeroLote = "Lote1",
                        Validade = !string.IsNullOrWhiteSpace(material.UnidadeTempo) ? (DateTime?)DateTime.Now.AddDays(100) : null,
                        Quantidade = material.Quantidade
                    });
                }
            }

            var resultado = sap.CriarMigo(lista.Materiais.ToList());

            if (resultado.ToString().StartsWith("OK:"))
            {
                var numDoc = resultado.ToString().Split(':')[1];
                var ano = DateTime.Now.ToString("yyyy");
                File.AppendAllText("log.txt", "RECEBIMENTO: " + numDoc + Environment.NewLine);
                sap.CancelarDocumento(numDoc, ano);
            }

            Assert.IsTrue(resultado.ToString().StartsWith("OK:"));
        }

        [TestMethod]
        [DataRow("180173771")]
        public void BuscarLista(string CodListaRecebimento)
        {
            var lista = sap.BuscarListaRecebimento(CodListaRecebimento);

            Assert.IsTrue(lista.Materiais.Any());
        }

        [TestMethod]
        [DataRow("5000001676", "2019")]
        public void CancelarDocumento(string numDoc, string ano)
        {
            var retorno = sap.CancelarDocumento(numDoc, ano);

            Assert.AreEqual("OK", retorno);
        }


        [TestMethod]
        [DataRow(2018, "5000009328", "0001")]
        public void ConsultarDocumento(int ano, string documento, string item)
        {
            var retorno = sap.ConsultarDocumento(ano, documento, item);

            Assert.IsNotNull(retorno);
        }

        #endregion

        #region ARMAZENAGEM

        [TestMethod]
        public void ListarPendentes()
        {
            var materiais = sap.ListarPendentesArmazenagem();
        }

        [TestMethod]
        [DataRow("583029")]
        [DataRow("583091")]
        public void Armazenar(string codMaterial)
        {
            //var material = sap.BuscarMaterialPendenteArmazenagem(codMaterial, "DM02");

            var material = new MaterialArmazenagem() { CodMaterial = codMaterial };

            material.Deposito = "DM15";
            material.PosicaoDeposito = "ABCD1234";
            material.Quantidade = 1;
            //material.Lotes.Add(new Lote() { Quantidade = 1, NumeroLote = "Manutenção" });

            Assert.IsNotNull(material);

            var resultado = sap.Armazenar(material, "DPRI");

            if (resultado.ToString().StartsWith("OK:"))
            {
                var numdDoc = resultado.ToString().Split(':')[1];
                var ano = DateTime.Now.ToString("yyyy");
                File.AppendAllText(Environment.CurrentDirectory + "log.txt", "ARMAZENAGEM: " + numdDoc + Environment.NewLine);
            }

            Assert.IsTrue(resultado.ToString().StartsWith("OK:"));
        }

        [TestMethod]
        [DataRow("575794")]
        public void BuscarMaterialPendente(string codMaterial)
        {
            var material = sap.BuscarMaterialPendenteArmazenagem(codMaterial.PadLeft(18, '0'));
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
        [DataRow("http://srv14/sparepartsapi/listaRecebimento/EnviarFoto/", @"C:\WorkSpace\BSC\WebApi\SparePartsApi\UnitTestProject5\foto.jpg")]
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

        #endregion

        #region ATENDIMENTO

        [TestMethod]
        [DataRow("3133355")]
        public void BuscarPorReserva(string numReserva)
        {
            var materiais = sap.BuscarReserva(numReserva);

            Assert.IsTrue(materiais.Any());
        }

        [TestMethod]
        [DataRow("3102933")]
        public void AtendimentoPorReserva(string numReserva)
        {
            var materiais = sap.BuscarReserva(numReserva);

            materiais.ToList().RemoveAll(x => string.IsNullOrWhiteSpace(x.Deposito));

            Assert.IsTrue(materiais.Any());

            var resultado = sap.AtenderPorReserva(materiais);

            Assert.IsTrue(resultado.StartsWith("OK:"));

            var numDoc = resultado.Split(':')?[1];

            sap.CancelarDocumento(numDoc, DateTime.Now.Year.ToString());
        }

        [TestMethod]
        [DataRow(new string[] { "543744" })]
        public void BuscarPorTransferencia(string[] codMateriais)
        {
            var materiais = sap.BuscarTransferencia(codMateriais);

            Assert.IsTrue(materiais.Any());
        }

        [TestMethod]
        [DataRow(new string[] { "555016", "543744", "543773", "549900", "743345", })]
        public void AtendimentoPorTransferencia(string[] codMateriais)
        {
            var materiais = sap.BuscarTransferencia(codMateriais);

            var resultado = sap.AtenderPorTransferencia(materiais);

            Assert.IsTrue(resultado.ToString().StartsWith("OK:"));
        }

        #endregion

    }
}
