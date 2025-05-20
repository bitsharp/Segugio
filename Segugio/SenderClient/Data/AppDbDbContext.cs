using Audit.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Segugio.Context;

namespace SenderClient.Data
{
    public class AppDbDbContext: SegugioDbContext
    {
        public DbSet<Persona> Tests { get; set; }

        public AppDbDbContext(DbContextOptions<AppDbDbContext> options) : base(options) { }

    
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