using System;
using System.Collections.Generic;
using System.Linq;
using Domain;
using Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject5
{
    [TestClass]
    public class RepositoryUnitTest1
    {
        private AtendimentoRepository atendimentoRepository;
        private IList<MaterialAtendimento> lista = new List<MaterialAtendimento>();

        #region MOCKS

        private MaterialAtendimento material1 = new MaterialAtendimento()
        {
            CodMaterial = "0001",
            DataHoraInicio = new DateTime(2018, 09, 11, 1, 1, 1),
            Deposito = "DM02",
            Nome = "material 1",
            PosicaoDeposito = "ABCD1234",
            Quantidade = 1,
            QuantidadeEstoqueFisico = 1,
            QuantidadeEstoqueSAP = 1,
            QuantidadeRetirada = 1,
            Situacao = Situacao.PENDENTE
        };

        MaterialAtendimento material2 = new MaterialAtendimento()
        {
            CodMaterial = "0002",
            DataHoraInicio = new DateTime(2018, 09, 11, 1, 1, 1),
            Deposito = "DM02",
            Nome = "material 2",
            PosicaoDeposito = "ABCD1234",
            Quantidade = 2,
            QuantidadeEstoqueFisico = 2,
            QuantidadeEstoqueSAP = 2,
            QuantidadeRetirada = 2,
            Situacao = Situacao.PENDENTE
        };

        #endregion
        public RepositoryUnitTest1()
        {
            try
            {
                atendimentoRepository = new AtendimentoRepository();
            }
            catch (Exception ex)
            {

            }
        }

        [TestMethod]
        public void Add()
        {
            atendimentoRepository.Add(material1);
        }

        [TestMethod]
        public void AddRange()
        {
            lista.Add(material1);
            lista.Add(material2);

            atendimentoRepository.AddRange(lista);
        }

        [TestMethod]
        public void Update()
        {
            try
            {
                material1.Id = 1;
                material1.Nome = "MATERIAL1_UPDATED";

                atendimentoRepository.Update(material1);
            }
            catch (Exception ex)
            {

            }
        }

        [TestMethod]
        public void UpdateRange()
        {
            material1.Nome = "MATERIAL1_UPDATED";
            material2.Nome = "MATERIAL2_UPDATED";

            lista.Add(material1);
            lista.Add(material2);

            atendimentoRepository.UpdateRange(lista);
        }

        [TestMethod]
        public void Remove()
        {
            atendimentoRepository.Remove(material1);
        }


        [TestMethod]
        public void RemoveRange()
        {
            lista.Add(material1);
            lista.Add(material2);

            atendimentoRepository.RemoveRange(lista.ToArray());
        }

        [TestMethod]
        public void FindById()
        {
            var material = atendimentoRepository.FindById("0001");
        }
    }
}
