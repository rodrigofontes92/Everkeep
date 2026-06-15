using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Everkeep.Models;

namespace Everkeep.Data
{
    /*
     * ApplicationUser substitui o user padrão do Identity
     * Permite agora que  EFCore reconheça os novos campos
     * Integra tudo automaticamente no sistema de autenticação
     */
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Conexao> Conexoes { get; set; }
        public DbSet<Mensagem> Mensagens { get; set; }
        public DbSet<Diario> Diarios { get; set; }

        // Método do EF Core utilizado para configuração manual de relacionamentos.
        // Opção por definir as regras de relacionamento de maneira mais explícita.
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Mantém as configurações padrão do ASP.NET Identity.
            base.OnModelCreating(builder);

            /*
             * RELACIONAMENTO:
             * Mensagem -> Remetente
             * 
             * Uma mensagem possui um remetente (ApplicationUser).
             */
            builder.Entity<Mensagem>()

                // Define que Mensagem possui relação com ApplicationUser e o user possui várias mensagens
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(m => m.RemetenteId)

                // Restrict impede exclusão automática em cascata.
                .OnDelete(DeleteBehavior.Restrict);

            /*
             * RELACIONAMENTO:
             * Mensagem -> Destinatário
             * 
             * Define o utilizador que recebe a mensagem.
             */
            builder.Entity<Mensagem>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(m => m.DestinatarioId)
                .OnDelete(DeleteBehavior.Restrict);

            /*
             * RELACIONAMENTO:
             * Diario -> User
             * 
             * Cada entrada do diário pertence a um utilizador.
             */
            builder.Entity<Diario>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(d => d.UserId)

                // Cascade: ao eliminar o user, também elimina os diários associados
                .OnDelete(DeleteBehavior.Cascade);

            /*
             * RELACIONAMENTO:
             * Conexao -> User1
             * 
             * Representa o primeiro utilizador da conexão.
             */
            builder.Entity<Conexao>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(c => c.User1Id)
                .OnDelete(DeleteBehavior.Restrict);

            /*
             * RELACIONAMENTO:
             * Conexao -> User2
             * 
             * Representa o segundo utilizador da conexão.
             */
            builder.Entity<Conexao>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(c => c.User2Id)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
