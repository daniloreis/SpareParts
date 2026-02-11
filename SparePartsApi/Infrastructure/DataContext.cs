using Domain;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace Infrastructure
{
    public class DataContext : DbContext
    {
        public DataContext() : base("bd_almoxarifado")
        {
            Configuration.AutoDetectChangesEnabled = true;
            Configuration.LazyLoadingEnabled = true;
            Configuration.ValidateOnSaveEnabled = false;
        }

        public virtual DbSet<MaterialRecebimento> MaterialRecebido { get; set; }

        public virtual DbSet<MaterialArmazenagem> MaterialArmazenagem { get; set; }

        public virtual DbSet<MaterialMovimentacao> MaterialMovimentado { get; set; }

        public virtual DbSet<MaterialInventario> MaterialInventario { get; set; }

        public virtual DbSet<MaterialAtendimento> MaterialAtendimento { get; set; }

        public virtual DbSet<MaterialAcompanhamento> MaterialAcompanhamento { get; set; }

        public virtual DbSet<ListaRecebimento> ListaRecebimento { get; set; }

        public virtual DbSet<PosicaoDeposito> PosicaoDeposito { get; set; }

        public virtual DbSet<EmailEnviado> EmailEnviado { get; set; }

        public virtual DbSet<MaterialAtendimentoTransferencia> MaterialAtendimentoTransferencia { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

            base.OnModelCreating(modelBuilder);
        }
    }
}
