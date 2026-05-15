using Avalonia;
using AgendaAvalonia.Data;
using AgendaAvalonia.Services;
using Microsoft.EntityFrameworkCore;
using System;

namespace AgendaAvalonia;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        if (args.Contains("--diagnose-db"))
        {
            DiagnoseDatabaseAsync().GetAwaiter().GetResult();
            return;
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static async Task DiagnoseDatabaseAsync()
    {
        var reportPath = Path.Combine(AppContext.BaseDirectory, "db-diagnostic.txt");

        try
        {
            using var db = new AppDbContext();
            await AuthService.InitializeAsync();
            var users = await db.Users.CountAsync();
            var orar = await db.OrarEntries.CountAsync();
            var note = await db.Note.CountAsync();
            var teme = await db.Teme.CountAsync();
            var sampleUsers = await db.Users
                .OrderBy(u => u.Id)
                .Take(5)
                .Select(u => $"{u.Id}:{u.NumeComplet}:{u.Email}:{u.Rol}")
                .ToListAsync();
            var sampleOrar = await db.OrarEntries
                .OrderBy(o => o.Id)
                .Take(8)
                .Select(o => $"{o.Id}:{o.ZiSaptamana}:{o.OraInceput}-{o.OraSfarsit}:{o.Materie}:{o.Profesor}")
                .ToListAsync();

            await File.WriteAllTextAsync(reportPath,
                $"Provider: {(AppDbContext.UsesMySql ? "MySQL" : "SQLite")}{Environment.NewLine}" +
                $"Users: {users}{Environment.NewLine}" +
                $"Orar: {orar}{Environment.NewLine}" +
                $"Note: {note}{Environment.NewLine}" +
                $"Teme: {teme}{Environment.NewLine}" +
                $"Sample users: {string.Join(" | ", sampleUsers)}{Environment.NewLine}" +
                $"Sample orar: {string.Join(" | ", sampleOrar)}{Environment.NewLine}");
        }
        catch (Exception ex)
        {
            await File.WriteAllTextAsync(reportPath, ex.ToString());
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}
