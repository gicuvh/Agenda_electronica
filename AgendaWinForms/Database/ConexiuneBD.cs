using AgendaWinForms.Data;

namespace AgendaWinForms.Database;

public static class ConexiuneBD
{
    public static string DatabasePath => AppDbContext.DatabasePath;
    public static string? MySqlConnectionString => AppDbContext.MySqlConnectionString;
    public static bool UsesMySql => AppDbContext.UsesMySql;
}
