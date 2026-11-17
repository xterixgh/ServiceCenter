using Microsoft.EntityFrameworkCore;
using ServiceCenter.Data.Models;

namespace ServiceCenter.Data
{
    public class AppDbContext : DbContext
    {
        public const string ConnectionString = "Host=localhost;Port=5432;Database=ServiceCenter;Username=postgres;Password=Bill0405h/";

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public AppDbContext() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseNpgsql(ConnectionString);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<DeviceType> DeviceTypes { get; set; }
        public DbSet<ServiceItem> Services { get; set; }
        public DbSet<TicketStatus> TicketStatuses { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketComment> Comments { get; set; }
        public DbSet<StatusLog> StatusLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Master)
                .WithMany()
                .HasForeignKey(t => t.Master_Id)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<TicketComment>()
                .HasOne(c => c.Ticket)
                .WithMany()
                .HasForeignKey(c => c.Ticket_Id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StatusLog>()
                .HasOne(c => c.Ticket)
                .WithMany()
                .HasForeignKey(c => c.Ticket_Id)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
