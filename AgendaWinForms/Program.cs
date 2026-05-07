using System;
using System.Windows.Forms;
using AgendaWinForms.Formulare;

namespace AgendaWinForms
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new FormularPrincipal());
        }
    }
}