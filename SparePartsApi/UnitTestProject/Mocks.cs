using Domain;
using System;
using System.Collections.Generic;

namespace UnitTestProject
{
    public class Mocks
    {
        public static List<MaterialRecebimento> materiaisCeanario1
        {
            get
            {
                List<MaterialRecebimento> materiais = new List<MaterialRecebimento>();

                var material1 = new MaterialRecebimento()
                {
                    CodMaterial = "000001",
                    Nome = "MATERIAL1",
                    NumeroItem = "0001",
                    Quantidade = 1,
                    UnidadeMedida = "UN",
                    NumeroPedido = "0000001",
                    NumeroPedidoItem = "00010",
                    Deposito = "DM02",
                    ClassContabil = "",
                    UnidadeTempo = "",
                    TemLote = true,
                    RevisaoOM = "PG19",
                    QuantidadePO = 1,
                    DataCriacaoRC = DateTime.Now,
                    DataHoraInicio = DateTime.Now
                };

                var material2 = new MaterialRecebimento()
                {
                    CodMaterial = "000002",
                    Nome = "MATERIAL2",
                    NumeroItem = "",
                    Quantidade = 1,
                    UnidadeMedida = "CDA",
                    NumeroPedido = "0000001",
                    NumeroPedidoItem = "00020",
                    Deposito = "DM10",
                    ClassContabil = "",
                    UnidadeTempo = "",
                    TemLote = true,
                    RevisaoOM = "PG19",
                    QuantidadePO = 1,
                    DataCriacaoRC = DateTime.Now,
                    DataHoraInicio = DateTime.Now
                };

                materiais.Add(material1);
                materiais.Add(material2);

                return materiais;
            }
        }
        public static List<MaterialRecebimento> materiais
        {
            get
            {
                List<MaterialRecebimento> materiais = new List<MaterialRecebimento>();

                var lote1 = new Lote()
                {
                    CodMaterial = "000003",
                    Deposito = "",
                    NumeroLote = "LOTE1",
                    Quantidade = 1,
                    Validade = DateTime.Now.AddYears(1)
                };

                var lote2 = new Lote()
                {
                    CodMaterial = "000003",
                    Deposito = "",
                    NumeroLote = "LOTE2",
                    Quantidade = 1,
                    Validade = DateTime.Now.AddYears(1)
                };

                var lote3 = new Lote()
                {
                    CodMaterial = "000003",
                    Deposito = "",
                    NumeroLote = "LOTE3",
                    Quantidade = 1,
                    Validade = DateTime.Now.AddYears(1)
                };

                var lote4 = new Lote()
                {
                    CodMaterial = "000003",
                    Deposito = "",
                    NumeroLote = "LOTE4",
                    Quantidade = 1,
                    Validade = DateTime.Now.AddYears(1)
                };

                var lotes = new List<Lote>();

                lotes.Add(lote1);
                lotes.Add(lote2);
                lotes.Add(lote3);
                lotes.Add(lote4);

                var material1 = new MaterialRecebimento()
                {
                    CodMaterial = "000001",
                    Nome = "MATERIAL1",
                    NumeroItem = "",
                    Quantidade = 1,
                    UnidadeMedida = "UN",
                    NumeroPedido = "",
                    NumeroPedidoItem = "",
                    Deposito = "DM02",
                    Descricao = "",
                    ClassContabil = "",
                    UnidadeTempo = "",
                    EmailSolicitante = "",
                    TemLote = true,
                    Fispq = "",
                    RevisaoOM = "",
                    DepositoRC = "",
                    QuantidadePO = 1,
                    NumeroLote = "",
                    NumeroNota = "",
                    DataCriacaoRC = DateTime.Now,
                    DataHoraInicio = DateTime.Now
                };

                var material2 = new MaterialRecebimento()
                {
                    CodMaterial = "000002",
                    Nome = "MATERIAL2",
                    NumeroItem = "",
                    Quantidade = 1,
                    UnidadeMedida = "CDA",
                    NumeroPedido = "",
                    NumeroPedidoItem = "",
                    Deposito = "",
                    Descricao = "",
                    ClassContabil = "",
                    UnidadeTempo = "",
                    EmailSolicitante = "",
                    TemLote = true,
                    Fispq = "",
                    RevisaoOM = "",
                    DepositoRC = "",
                    QuantidadePO = 1,
                    NumeroLote = "MANUTENÇÃO",
                    NumeroNota = "",
                    DataCriacaoRC = DateTime.Now,
                    DataHoraInicio = DateTime.Now
                };

                var material3 = new MaterialRecebimento()
                {
                    CodMaterial = "000003",
                    Nome = "MATERIAL3",
                    NumeroItem = "",
                    Quantidade = 1,
                    UnidadeMedida = "CDA",
                    NumeroPedido = "",
                    NumeroPedidoItem = "",
                    Deposito = "",
                    Descricao = "",
                    ClassContabil = "",
                    UnidadeTempo = "",
                    EmailSolicitante = "",
                    TemLote = true,
                    Fispq = "",
                    RevisaoOM = "",
                    DepositoRC = "",
                    QuantidadePO = 1,
                    NumeroLote = "MANUTENÇÃO",
                    NumeroNota = "",
                    DataCriacaoRC = DateTime.Now,
                    DataHoraInicio = DateTime.Now
                };

                var material4 = new MaterialRecebimento()
                {
                    CodMaterial = "000003",
                    Nome = "MATERIAL4",
                    NumeroItem = "",
                    Quantidade = 1,
                    UnidadeMedida = "CDA",
                    NumeroPedido = "",
                    NumeroPedidoItem = "",
                    Deposito = "",
                    Descricao = "",
                    ClassContabil = "",
                    UnidadeTempo = "",
                    EmailSolicitante = "",
                    TemLote = true,
                    Fispq = "",
                    RevisaoOM = "",
                    DepositoRC = "",
                    QuantidadePO = 1,
                    NumeroLote = "MANUTENÇÃO",
                    NumeroNota = "",
                    DataCriacaoRC = DateTime.Now,
                    DataHoraInicio = DateTime.Now
                };

                material4.Lotes = lotes;

                materiais.Add(material1);
                materiais.Add(material2);
                materiais.Add(material3);
                materiais.Add(material4);

                return materiais;
            }
        }

        //public Mocks()
        //{
        //    var lote1 = new Lote()
        //    {
        //        CodMaterial = "000003",
        //        Deposito = "",
        //        NumeroLote = "LOTE1",
        //        Quantidade = 1,
        //        Validade = DateTime.Now.AddYears(1)
        //    };

        //    var lote2 = new Lote()
        //    {
        //        CodMaterial = "000003",
        //        Deposito = "",
        //        NumeroLote = "LOTE2",
        //        Quantidade = 1,
        //        Validade = DateTime.Now.AddYears(1)
        //    };

        //    var lote3 = new Lote()
        //    {
        //        CodMaterial = "000003",
        //        Deposito = "",
        //        NumeroLote = "LOTE3",
        //        Quantidade = 1,
        //        Validade = DateTime.Now.AddYears(1)
        //    };

        //    var lote4 = new Lote()
        //    {
        //        CodMaterial = "000003",
        //        Deposito = "",
        //        NumeroLote = "LOTE4",
        //        Quantidade = 1,
        //        Validade = DateTime.Now.AddYears(1)
        //    };

        //    var lotes = new List<Lote>();

        //    lotes.Add(lote1);
        //    lotes.Add(lote2);
        //    lotes.Add(lote3);
        //    lotes.Add(lote4);

        //    var material1 = new MaterialRecebimento()
        //    {
        //        CodMaterial = "000001",
        //        Nome = "MATERIAL1",
        //        NumeroItem = "",
        //        Quantidade = 1,
        //        UnidadeMedida = "UN",
        //        NumeroPedido = "",
        //        NumeroPedidoItem = "",
        //        Deposito = "",
        //        Descricao = "",
        //        ClassContabil = "",
        //        UnidadeTempo = "",
        //        EmailSolicitante = "",
        //        TemLote = true,
        //        Fispq = "",
        //        RevisaoOM = "",
        //        DepositoRC = "",
        //        QuantidadeRC = 1,
        //        NumeroLote = "",
        //        NumeroNota = "",
        //        DataCriacaoRC = DateTime.Now,
        //        DataHoraInicio = DateTime.Now
        //    };

        //    var material2 = new MaterialRecebimento()
        //    {
        //        CodMaterial = "000002",
        //        Nome = "MATERIAL2",
        //        NumeroItem = "",
        //        Quantidade = 1,
        //        UnidadeMedida = "CDA",
        //        NumeroPedido = "",
        //        NumeroPedidoItem = "",
        //        Deposito = "",
        //        Descricao = "",
        //        ClassContabil = "",
        //        UnidadeTempo = "",
        //        EmailSolicitante = "",
        //        TemLote = true,
        //        Fispq = "",
        //        RevisaoOM = "",
        //        DepositoRC = "",
        //        QuantidadeRC = 1,
        //        NumeroLote = "MANUTENÇÃO",
        //        NumeroNota = "",
        //        DataCriacaoRC = DateTime.Now,
        //        DataHoraInicio = DateTime.Now
        //    };

        //    var material3 = new MaterialRecebimento()
        //    {
        //        CodMaterial = "000003",
        //        Nome = "MATERIAL3",
        //        NumeroItem = "",
        //        Quantidade = 1,
        //        UnidadeMedida = "CDA",
        //        NumeroPedido = "",
        //        NumeroPedidoItem = "",
        //        Deposito = "",
        //        Descricao = "",
        //        ClassContabil = "",
        //        UnidadeTempo = "",
        //        EmailSolicitante = "",
        //        TemLote = true,
        //        Fispq = "",
        //        RevisaoOM = "",
        //        DepositoRC = "",
        //        QuantidadeRC = 1,
        //        NumeroLote = "MANUTENÇÃO",
        //        NumeroNota = "",
        //        DataCriacaoRC = DateTime.Now,
        //        DataHoraInicio = DateTime.Now
        //    };

        //    var material4 = new MaterialRecebimento()
        //    {
        //        CodMaterial = "000003",
        //        Nome = "MATERIAL4",
        //        NumeroItem = "",
        //        Quantidade = 1,
        //        UnidadeMedida = "CDA",
        //        NumeroPedido = "",
        //        NumeroPedidoItem = "",
        //        Deposito = "",
        //        Descricao = "",
        //        ClassContabil = "",
        //        UnidadeTempo = "",
        //        EmailSolicitante = "",
        //        TemLote = true,
        //        Fispq = "",
        //        RevisaoOM = "",
        //        DepositoRC = "",
        //        QuantidadeRC = 1,
        //        NumeroLote = "MANUTENÇÃO",
        //        NumeroNota = "",
        //        DataCriacaoRC = DateTime.Now,
        //        DataHoraInicio = DateTime.Now
        //    };

        //    material4.Lotes = lotes;

        //    materiais.Add(material1);
        //    materiais.Add(material2);
        //    materiais.Add(material3);
        //    materiais.Add(material4);
        //}
    }
}
