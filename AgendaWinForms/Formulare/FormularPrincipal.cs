using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace AgendaWinForms.Formulare
{
    public class FormularPrincipal : Form
    {
        public FormularPrincipal()
        {
            this.Text = "Agenda Principală";
            this.Size = new Size(1280, 820);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 247, 250);

            InitializeLayout();
        }

        private void InitializeLayout()
        {
            Guna2Panel sidebar = new Guna2Panel
            {
                Dock = DockStyle.Left,
                Width = 260,
                FillColor = Color.White
            };
            sidebar.ShadowDecoration.Enabled = true;
            sidebar.ShadowDecoration.Shadow = new Padding(8);

            Label logo = new Label
            {
                Text = "Agenda",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(27, 95, 255),
                Location = new Point(30, 30),
                AutoSize = true
            };

            sidebar.Controls.Add(logo);

            string[] items = { "Dashboard", "Note", "Teme", "Orar" };
            int top = 110;
            foreach (string item in items)
            {
                Guna2Button button = new Guna2Button
                {
                    Text = item,
                    Size = new Size(220, 45),
                    Location = new Point(20, top),
                    BorderRadius = 15,
                    FillColor = Color.FromArgb(245, 247, 250),
                    ForeColor = Color.FromArgb(34, 45, 67)
                };
                button.HoverState.FillColor = Color.FromArgb(230, 240, 255);
                sidebar.Controls.Add(button);
                top += 60;
            }

            Label settings = new Label
            {
                Text = "Setări",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.Gray,
                Location = new Point(30, 560),
                AutoSize = true
            };

            Label help = new Label
            {
                Text = "Ajutor",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.Gray,
                Location = new Point(30, 590),
                AutoSize = true
            };

            sidebar.Controls.Add(settings);
            sidebar.Controls.Add(help);
            this.Controls.Add(sidebar);

            Guna2Panel content = new Guna2Panel
            {
                Dock = DockStyle.Fill,
                FillColor = Color.FromArgb(245, 247, 250)
            };

            Label title = new Label
            {
                Text = "Bun venit în Agenda ta",
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.FromArgb(34, 45, 67),
                Location = new Point(40, 30),
                AutoSize = true
            };

            Label subtitle = new Label
            {
                Text = "Aici poți vedea notițele, teme și orarul tău.",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.Gray,
                Location = new Point(40, 90),
                AutoSize = true
            };

            Guna2Panel card1 = new Guna2Panel
            {
                Size = new Size(330, 180),
                Location = new Point(40, 150),
                BorderRadius = 25,
                FillColor = Color.White
            };
            card1.ShadowDecoration.Enabled = true;
            card1.ShadowDecoration.Shadow = new Padding(8);

            Label card1Title = new Label
            {
                Text = "Notițe",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(34, 45, 67),
                Location = new Point(20, 20),
                AutoSize = true
            };

            Label card1Text = new Label
            {
                Text = "Vezi ultimele tale notițe și adaugă altele noi.",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.Gray,
                Location = new Point(20, 60),
                Size = new Size(290, 90)
            };

            card1.Controls.Add(card1Title);
            card1.Controls.Add(card1Text);

            Guna2Panel card2 = new Guna2Panel
            {
                Size = new Size(330, 180),
                Location = new Point(400, 150),
                BorderRadius = 25,
                FillColor = Color.White
            };
            card2.ShadowDecoration.Enabled = true;
            card2.ShadowDecoration.Shadow = new Padding(8);

            Label card2Title = new Label
            {
                Text = "Teme",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(34, 45, 67),
                Location = new Point(20, 20),
                AutoSize = true
            };

            Label card2Text = new Label
            {
                Text = "Urmărește termenele limită și proiectele importante.",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.Gray,
                Location = new Point(20, 60),
                Size = new Size(290, 90)
            };

            card2.Controls.Add(card2Title);
            card2.Controls.Add(card2Text);

            Guna2Panel card3 = new Guna2Panel
            {
                Size = new Size(330, 180),
                Location = new Point(760, 150),
                BorderRadius = 25,
                FillColor = Color.White
            };
            card3.ShadowDecoration.Enabled = true;
            card3.ShadowDecoration.Shadow = new Padding(8);

            Label card3Title = new Label
            {
                Text = "Orar",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(34, 45, 67),
                Location = new Point(20, 20),
                AutoSize = true
            };

            Label card3Text = new Label
            {
                Text = "Verifică programul zilei și planifică-ți activitățile.",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.Gray,
                Location = new Point(20, 60),
                Size = new Size(290, 90)
            };

            card3.Controls.Add(card3Title);
            card3.Controls.Add(card3Text);

            content.Controls.Add(title);
            content.Controls.Add(subtitle);
            content.Controls.Add(card1);
            content.Controls.Add(card2);
            content.Controls.Add(card3);

            this.Controls.Add(content);
        }
    }
}
