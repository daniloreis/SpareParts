using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    [Table("MaterialAtendido")]
    public class MaterialAtendimento : Material
    {
        public String UsuarioAtendido { get; set; }

        public String ImagemAssinatura { get; set; }

        [NotMapped]
        public bool Critico { get; set; }

        [Required]
        public string NumeroReserva { get; set; }

        public string TipoAtendimento { get; set; }

        /// <summary>
        /// Quantidade a ser consumida no SAP
        /// </summary>
        [Required]
        public double? QuantidadeRetirada { get; set; }

        /// <summary>
        ///Quantidade no estoque do SAP em formato double. 
        /// </summary>
        public double? QuantidadeEstoqueSAP { get; set; }

        /// <summary>
        /// Quantidade contada no estoque físico
        /// </summary>

        public double? QuantidadeEstoqueFisico { get; set; }

        [NotMapped]
        public double? DiferencaEstoque { get { return QuantidadeEstoqueFisico - QuantidadeRetirada; } }

        [NotMapped]
        public bool Selecionado { get; set; }

        [NotMapped]
        public double? QuantidadeDivergencia
        {
            get
            {
                if (QuantidadeEstoqueFisico.HasValue)
                    return Convert.ToDouble(QuantidadeEstoqueFisico) - QuantidadeEstoqueSAP;
                else

                    return null;
            }
        }

        [NotMapped]
        public double? QuantidadeDisponivel { get; set; }

        [NotMapped]
        public IList<Deposito> Depositos { get; set; } = new List<Deposito>();

        public string Situacao { get; set; }

        [NotMapped]
        public bool NaoPodeAtender { get; set; }

        public string Sequencia { get; set; }
    }

    public class TipoAtendimento
    {
        public const string COMPRADIRETA = "COMPRA DIRETA", TRANSFERENCIA = "TRANSFERENCIA", RESERVA = "RESERVA";
    }

    public class Situacao
    {
        public const string PENDENTE = "PENDENTE", CONCLUIDO = "CONCLUIDO";
    }
}
