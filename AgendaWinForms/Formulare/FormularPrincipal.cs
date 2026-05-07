using System;
using System.Drawing;
using System.Windows.Forms;

namespace AgendaWinForms.Formulare
{
    public class FormularPrincipal : Form
    {
        public FormularPrincipal()
        {
            this.Text = "Agenda Electronica";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
        }
    }
}