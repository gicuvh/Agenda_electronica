using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using MySql.Data.MySqlClient;
using AgendaWinForms.Database;

namespace AgendaWinForms.Formulare
{
    public class FormularLogin : Form
    {
        private Guna2TextBox? txtEmail;
        private Guna2TextBox? txtParola;

        public FormularLogin()
        {
            InitializareFereastra();
            InitializareLayout();
        }

        private void InitializareFereastra()
        {
            this.Text = "Agenda | Autentificare";
            this.Size = new Size(1200, 760);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 247, 250);
        }

        private void InitializareLayout()
        {
            Guna2Panel leftPanel = new Guna2Panel
            {
                Dock = DockStyle.Left,
                Width = 420,
                FillColor = Color.FromArgb(27, 95, 255)
            };

            Label logo = new Label
            {
                Text = "Agenda",
                Font = new Font("Segoe UI", 30, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(40, 40),
                AutoSize = true
            };

            Label slogan = new Label
            {
                Text = "Gestionați notițele și programările\nintr-un singur loc.",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(220, 230, 255),
                Location = new Point(40, 120),
                Size = new Size(340, 80)
            };

            leftPanel.Controls.Add(logo);
            leftPanel.Controls.Add(slogan);
            this.Controls.Add(leftPanel);

            Guna2Panel card = new Guna2Panel
            {
                Size = new Size(520, 520),
                Location = new Point(520, 120),
                BorderRadius = 30,
                FillColor = Color.White,
            };
            card.ShadowDecoration.Enabled = true;
            card.ShadowDecoration.Shadow = new Padding(10);

            Label title = new Label
            {
                Text = "Autentificare",
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.FromArgb(34, 45, 67),
                Location = new Point(40, 40),
                AutoSize = true
            };

            Label subtitle = new Label
            {
                Text = "Introduceți datele pentru a vă conecta.",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.Gray,
                Location = new Point(40, 100),
                AutoSize = true
            };

            txtEmail = new Guna2TextBox
            {
                PlaceholderText = "Adresă e-mail",
                Size = new Size(440, 50),
                Location = new Point(40, 150),
                BorderRadius = 15
            };

            txtParola = new Guna2TextBox
            {
                PlaceholderText = "Parolă",
                PasswordChar = '●',
                Size = new Size(440, 50),
                Location = new Point(40, 220),
                BorderRadius = 15
            };

            Guna2Button btnLogin = new Guna2Button
            {
                Text = "Autentificare",
                Size = new Size(440, 55),
                Location = new Point(40, 300),
                BorderRadius = 15,
                FillColor = Color.FromArgb(27, 95, 255),
                ForeColor = Color.White
            };
            btnLogin.Click += BtnLogin_Click;

            Label bottomText = new Label
            {
                Text = "Nu ai cont?",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.Gray,
                Location = new Point(40, 380),
                AutoSize = true
            };

            LinkLabel linkRegister = new LinkLabel
            {
                Text = "Creează cont nou",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                LinkColor = Color.FromArgb(27, 95, 255),
                Location = new Point(115, 378),
                AutoSize = true
            };
            linkRegister.Click += (s, e) =>
            {
                using FormularRegister register = new FormularRegister();
                register.ShowDialog();
            };

            card.Controls.Add(title);
            card.Controls.Add(subtitle);
            card.Controls.Add(txtEmail);
            card.Controls.Add(txtParola);
            card.Controls.Add(btnLogin);
            card.Controls.Add(bottomText);
            card.Controls.Add(linkRegister);

            this.Controls.Add(card);
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmail?.Text) || string.IsNullOrWhiteSpace(txtParola?.Text))
            {
                MessageBox.Show("Introduceți e-mail și parolă.", "Atenție", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                ConexiuneBD bd = new ConexiuneBD();
                using MySqlConnection conexiune = bd.GetConnection();
                conexiune.Open();

                string query =
                    "SELECT * FROM utilizatori " +
                    "WHERE email=@email AND parola=@parola";

                using MySqlCommand cmd = new MySqlCommand(query, conexiune);
                cmd.Parameters.AddWithValue("@email", txtEmail!.Text);
                cmd.Parameters.AddWithValue("@parola", txtParola!.Text);

                using MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    FormularPrincipal principal = new FormularPrincipal();
                    principal.FormClosed += (s, args) => this.Close();
                    this.Hide();
                    principal.Show();
                }
                else
                {
                    MessageBox.Show("Date invalide!", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}