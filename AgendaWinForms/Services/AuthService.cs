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
}
