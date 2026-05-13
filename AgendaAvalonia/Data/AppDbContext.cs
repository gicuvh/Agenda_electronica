using AgendaAvalonia.Models;
using Microsoft.EntityFrameworkCore;

namespace AgendaAvalonia.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Nota> Note => Set<Nota>();
    public DbSet<Tema> Teme => Set<Tema>();
    public DbSet<OrarEntry> OrarEntries => Set<OrarEntry>();
    public DbSet<Activitate> Activitati => Set<Activitate>();

    public static string DatabasePath
    {
        get
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Agenda");
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, "agenda.db");
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite($"Data Source={DatabasePath}");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<User>().Property(u => u.Rol).HasDefaultValue("Elev");
        modelBuilder.Entity<Tema>().Property(t => t.Finalizata).HasDefaultValue(false);
    }
}
