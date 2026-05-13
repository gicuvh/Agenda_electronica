using AgendaWinForms.Formulare;
using AgendaWinForms.Services;
using System.Windows.Forms;

namespace AgendaWinForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        AuthService.InitializeAsync().GetAwaiter().GetResult();
        Application.Run(new FormularLogin());
    }
}
