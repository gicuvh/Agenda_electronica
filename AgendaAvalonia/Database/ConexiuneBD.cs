using AgendaAvalonia.Data;

namespace AgendaAvalonia.Database;

public static class ConexiuneBD
{
    public static string DatabasePath => AppDbContext.DatabasePath;
}
