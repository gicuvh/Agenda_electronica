using AgendaWinForms.Data;
using AgendaWinForms.Models;
using Microsoft.EntityFrameworkCore;

namespace AgendaWinForms.Services;

public static class AuthService
{
    public static User? CurrentUser { get; private set; }

    public static async Task InitializeAsync()
    {
        using var db = new AppDbContext();
        await db.Database.EnsureCreatedAsync();
        await SeedUserAsync(db, "Admin Principal", "admin@gmail.com", "Admin123!", "Admin");
        await SeedUserAsync(db, "Gheorghe", "gicu@gmail.com", "user123!", "Elev");
        await db.SaveChangesAsync();
        await SeedDemoDataAsync(db);
    }

    public static async Task<(bool Success, string Message)> RegisterAsync(string numeComplet, string email, string parola)
    {
        email = email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(numeComplet) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(parola))
            return (false, "Completează toate câmpurile.");

        using var db = new AppDbContext();
        if (await db.Users.AnyAsync(u => u.Email == email))
            return (false, "Există deja un cont cu acest email.");

        var user = new User
        {
            NumeComplet = numeComplet.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(parola),
            Rol = "Elev"
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();
        CurrentUser = user;
        return (true, "Cont creat cu succes!");
    }

    public static async Task<(bool Success, string Message)> LoginAsync(string email, string parola)
    {
        email = email.Trim().ToLowerInvariant();
        using var db = new AppDbContext();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(parola, user.PasswordHash))
            return (false, "Email sau parolă incorectă.");

        CurrentUser = user;
        return (true, "Autentificare reușită!");
    }

    public static void Logout() => CurrentUser = null;

    private static async Task SeedUserAsync(AppDbContext db, string nume, string email, string parola, string rol)
    {
        if (await db.Users.AnyAsync(u => u.Email == email))
            return;

        db.Users.Add(new User
        {
            NumeComplet = nume,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(parola),
            Rol = rol
        });
    }

    private static async Task SeedDemoDataAsync(AppDbContext db)
    {
        var elev = await db.Users.FirstOrDefaultAsync(u => u.Email == "gicu@gmail.com");
        if (elev is null)
            return;

        if (!await db.Note.AnyAsync(n => n.UserId == elev.Id))
        {
            db.Note.AddRange(
                new Nota { UserId = elev.Id, Materie = "Matematică", Valoare = 9, Descriere = "Test funcții", Data = DateTime.Today.AddDays(-5) },
                new Nota { UserId = elev.Id, Materie = "Informatică", Valoare = 10, Descriere = "Proiect C#", Data = DateTime.Today.AddDays(-3) },
                new Nota { UserId = elev.Id, Materie = "Limba română", Valoare = 8, Descriere = "Eseu argumentativ", Data = DateTime.Today.AddDays(-1) }
            );
        }

        if (!await db.Teme.AnyAsync(t => t.UserId == elev.Id))
        {
            db.Teme.AddRange(
                new Tema { UserId = elev.Id, Titlu = "Exercițiile 1-12", Materie = "Matematică", Deadline = DateTime.Today.AddDays(2) },
                new Tema { UserId = elev.Id, Titlu = "Referat despre baze de date", Materie = "Informatică", Deadline = DateTime.Today.AddDays(5) },
                new Tema { UserId = elev.Id, Titlu = "Comentariu literar", Materie = "Limba română", Deadline = DateTime.Today.AddDays(7) }
            );
        }

        if (!await db.OrarEntries.AnyAsync(o => o.UserId == elev.Id))
        {
            db.OrarEntries.AddRange(
                new OrarEntry { UserId = elev.Id, ZiSaptamana = "Luni", OraInceput = new TimeSpan(8, 0, 0), OraSfarsit = new TimeSpan(8, 45, 0), Materie = "Matematică", Profesor = "Popescu A." },
                new OrarEntry { UserId = elev.Id, ZiSaptamana = "Luni", OraInceput = new TimeSpan(9, 0, 0), OraSfarsit = new TimeSpan(9, 45, 0), Materie = "Informatică", Profesor = "Ionescu M." },
                new OrarEntry { UserId = elev.Id, ZiSaptamana = "Marți", OraInceput = new TimeSpan(10, 0, 0), OraSfarsit = new TimeSpan(10, 45, 0), Materie = "Limba română", Profesor = "Rusu E." },
                new OrarEntry { UserId = elev.Id, ZiSaptamana = "Miercuri", OraInceput = new TimeSpan(8, 0, 0), OraSfarsit = new TimeSpan(8, 45, 0), Materie = "Istorie", Profesor = "Munteanu V." }
            );
        }

        if (!await db.Activitati.AnyAsync(a => a.UserId == elev.Id))
        {
            db.Activitati.AddRange(
                new Activitate { UserId = elev.Id, Tip = "nota", Descriere = "A fost adăugată nota 10 la Informatică", Timestamp = DateTime.Now.AddDays(-3) },
                new Activitate { UserId = elev.Id, Tip = "tema", Descriere = "Tema la Matematică este aproape de deadline", Timestamp = DateTime.Now.AddDays(-1) },
                new Activitate { UserId = elev.Id, Tip = "orar", Descriere = "Orarul săptămânal a fost actualizat", Timestamp = DateTime.Now.AddHours(-6) }
            );
        }

        await db.SaveChangesAsync();
    }
}
