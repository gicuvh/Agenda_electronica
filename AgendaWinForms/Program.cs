using System;
using System.Windows.Forms;

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