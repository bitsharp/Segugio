using Audit.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace SenderClient.Data
{
    public class AppDbContext: AuditDbContext
    {
        public DbSet<Persona> Tests { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Persona>(entity =>
            {
                entity.ToTable("Persone", "BusinessModel"); // Configura la tabella Tests
                entity.HasKey(e => e.Id); // Configura Id come chiave primaria
                entity.Property(e => e.Descrizione).IsRequired(); // Configura Descrizione come campo obbligatorio
            });
        }
    }

    // Classe che rappresenta la tabella Test
    public class Persona
    {
        public long Id { get; set; }
        public string Descrizione { get; set; }
    }
}