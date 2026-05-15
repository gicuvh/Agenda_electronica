using AgendaAvalonia.Models;
using Microsoft.EntityFrameworkCore;

namespace AgendaAvalonia.Data;

public class AppDbContext : DbContext
{
    private const string DefaultMySqlConnectionString = "server=localhost;port=3306;database=agendaelectronica;user=root;password=GicuVoloh2008;";

    public DbSet<User> Users => Set<User>();
    public DbSet<Nota> Note => Set<Nota>();
    public DbSet<Tema> Teme => Set<Tema>();
    public DbSet<OrarEntry> OrarEntries => Set<OrarEntry>();
    public DbSet<Activitate> Activitati => Set<Activitate>();
    public DbSet<Clasa> Clase => Set<Clasa>();

    public static string DatabasePath
    {
        get
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Agenda");
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, "agenda.db");
        }
    }

    public static string? MySqlConnectionString
    {
        get
        {
            var value = Environment.GetEnvironmentVariable("AGENDA_MYSQL_CONNECTION_STRING");
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();

            foreach (var configPath in MySqlConfigPaths())
            {
                if (!File.Exists(configPath))
                    continue;

                value = File.ReadAllText(configPath).Trim();
                if (!string.IsNullOrWhiteSpace(value) && !value.StartsWith("#"))
                    return value;
            }

            return DefaultMySqlConnectionString;
        }
    }

    public static bool UsesMySql => !string.IsNullOrWhiteSpace(MySqlConnectionString);

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (UsesMySql)
        {
            optionsBuilder.UseMySQL(MySqlConnectionString!);
            return;
        }

        optionsBuilder.UseSqlite($"Data Source={DatabasePath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (UsesMySql)
        {
            ConfigureMySqlModel(modelBuilder);
            return;
        }

        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<User>().Property(u => u.Rol).HasDefaultValue("Elev");
        modelBuilder.Entity<User>()
            .HasOne(u => u.Clasa)
            .WithMany(c => c.Elevi)
            .HasForeignKey(u => u.ClasaId)
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Clasa>().HasIndex(c => c.Nume).IsUnique();
        modelBuilder.Entity<Tema>().Property(t => t.Finalizata).HasDefaultValue(false);
    }

    private static IEnumerable<string> MySqlConfigPaths()
    {
        yield return Path.Combine(AppContext.BaseDirectory, "mysql-connection.txt");
        yield return Path.Combine(Directory.GetCurrentDirectory(), "mysql-connection.txt");

        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            yield return Path.Combine(directory.FullName, "mysql-connection.txt");
            directory = directory.Parent;
        }
    }

    private static void ConfigureMySqlModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("utilizatori");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).HasColumnName("IdUtilizator");
            entity.Property(u => u.NumeComplet).HasColumnName("Nume");
            entity.Property(u => u.Email).HasColumnName("Email");
            entity.Property(u => u.PasswordHash).HasColumnName("Parola");
            entity.Property(u => u.Rol).HasColumnName("Rol");
            entity.Property(u => u.DataInregistrare).HasColumnName("DataInregistrare");
            entity.Property(u => u.ClasaId).HasColumnName("ClasaId");
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Clasa>(entity =>
        {
            entity.ToTable("clase");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).HasColumnName("IdClasa");
            entity.Property(c => c.Nume).HasColumnName("Nume");
            entity.Property(c => c.DataCreare).HasColumnName("DataCreare");
            entity.HasIndex(c => c.Nume).IsUnique();
        });

        modelBuilder.Entity<Nota>(entity =>
        {
            entity.ToTable("note");
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Id).HasColumnName("IdNota");
            entity.Property(n => n.UserId).HasColumnName("IdUtilizator");
            entity.Property(n => n.Materie).HasColumnName("Disciplina");
            entity.Property(n => n.Valoare).HasColumnName("Nota");
            entity.Property(n => n.Data).HasColumnName("DataNotare");
            entity.Property(n => n.Descriere).HasColumnName("Descriere");
        });

        modelBuilder.Entity<Tema>(entity =>
        {
            entity.ToTable("teme");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).HasColumnName("IdTema");
            entity.Property(t => t.UserId).HasColumnName("IdUtilizator");
            entity.Property(t => t.Titlu).HasColumnName("Titlu");
            entity.Property(t => t.Materie).HasColumnName("Materie");
            entity.Property(t => t.Deadline).HasColumnName("Deadline");
            entity.Property(t => t.Finalizata)
                .HasColumnName("StatusTema")
                .HasConversion(
                    value => value ? "Finalizata" : "In asteptare",
                    value => value == "Finalizata" || value == "Finalizată" || value == "finalizat" || value == "Done");
        });

        modelBuilder.Entity<OrarEntry>(entity =>
        {
            entity.ToTable("orar");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Id).HasColumnName("id");
            entity.Property(o => o.ZiSaptamana).HasColumnName("zi");
            entity.Property(o => o.Materie).HasColumnName("disciplina");
            entity.Property(o => o.Profesor).HasColumnName("profesor");
            entity.Property(o => o.OraInceput).HasColumnName("ora_inceput");
            entity.Property(o => o.OraSfarsit).HasColumnName("ora_sfarsit");
            entity.Property(o => o.UserId).HasColumnName("IdUtilizator");
        });

        modelBuilder.Entity<Activitate>(entity =>
        {
            entity.ToTable("activitati");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Id).HasColumnName("IdActivitate");
            entity.Property(a => a.UserId).HasColumnName("IdUtilizator");
            entity.Property(a => a.Descriere).HasColumnName("Descriere");
            entity.Property(a => a.Timestamp).HasColumnName("Timestamp");
            entity.Property(a => a.Tip).HasColumnName("Tip");
        });
    }
}
