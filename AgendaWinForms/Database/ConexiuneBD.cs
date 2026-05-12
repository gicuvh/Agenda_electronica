using MySql.Data.MySqlClient;

namespace AgendaWinForms.Database
{
    public class ConexiuneBD
    {
        private string connectionString =
            "server=localhost;" +
            "database=agendaelectronica;" +
            "uid=root;" +
            "pwd=GicuVoloh2008;";

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }
    }
}