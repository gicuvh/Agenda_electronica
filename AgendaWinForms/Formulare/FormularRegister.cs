using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using MySql.Data.MySqlClient;
using AgendaWinForms.Database;

namespace AgendaWinForms.Formulare
{
    public class FormularRegister : Form
    {
        private Guna2TextBox? txtNume;
        private Guna2TextBox? txtEmail;
        private Guna2TextBox? txtParola;
        private Guna2TextBox? txtConfirmParola;

        public FormularRegister()
        {
            InitializareFereastra();
            InitializareRegister();
        }

        private void InitializareFereastra()
        {
            this.Text = "Agenda | Creare cont";
            this.Size = new Size(780, 760);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 247, 250);
        }

        private void InitializareRegister()
        {
            Guna2Panel card = new Guna2Panel
            {
                Size = new Size(520, 620),
                Location = new Point(125, 70),
                BorderRadius = 30,
                FillColor = Color.White
            };
            card.ShadowDecoration.Enabled = true;
            card.ShadowDecoration.Shadow = new Padding(10);

            Label title = new Label
            {
                Text = "Creare cont",
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.FromArgb(34, 45, 67),
                Location = new Point(40, 40),
                AutoSize = true
            };

            Label subtitle = new Label
            {
                Text = "Alătură-te comunității Agenda.",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.Gray,
                Location = new Point(40, 100),
                AutoSize = true
            };

            txtNume = new Guna2TextBox
            {
                PlaceholderText = "Nume complet",
                Size = new Size(440, 50),
                Location = new Point(40, 150),
                BorderRadius = 15
            };

            txtEmail = new Guna2TextBox
            {
                PlaceholderText = "Adresă e-mail",
                Size = new Size(440, 50),
                Location = new Point(40, 220),
                BorderRadius = 15
            };

            txtParola = new Guna2TextBox
            {
                PlaceholderText = "Parolă",
                PasswordChar = '●',
                Size = new Size(440, 50),
                Location = new Point(40, 290),
                BorderRadius = 15
            };

            txtConfirmParola = new Guna2TextBox
            {
                PlaceholderText = "Confirmă parola",
                PasswordChar = '●',
                Size = new Size(440, 50),
                Location = new Point(40, 360),
                BorderRadius = 15
            };

            Guna2Button btnRegister = new Guna2Button
            {
                Text = "Creează cont",
                Size = new Size(440, 55),
                Location = new Point(40, 440),
                BorderRadius = 15,
                FillColor = Color.FromArgb(27, 95, 255),
                ForeColor = Color.White
            };
            btnRegister.Click += BtnRegister_Click;

            Label loginText = new Label
            {
                Text = "Ai deja un cont?",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.Gray,
                Location = new Point(40, 520),
                AutoSize = true
            };

            LinkLabel linkLogin = new LinkLabel
            {
                Text = "Autentifică-te",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                LinkColor = Color.FromArgb(27, 95, 255),
                Location = new Point(125, 518),
                AutoSize = true
            };
            linkLogin.Click += (s, e) => this.Close();

            card.Controls.Add(title);
            card.Controls.Add(subtitle);
            card.Controls.Add(txtNume);
            card.Controls.Add(txtEmail);
            card.Controls.Add(txtParola);
            card.Controls.Add(txtConfirmParola);
            card.Controls.Add(btnRegister);
            card.Controls.Add(loginText);
            card.Controls.Add(linkLogin);

            this.Controls.Add(card);
        }

        private void BtnRegister_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNume?.Text) || string.IsNullOrWhiteSpace(txtEmail?.Text) || string.IsNullOrWhiteSpace(txtParola?.Text) || string.IsNullOrWhiteSpace(txtConfirmParola?.Text))
            {
                MessageBox.Show("Completați toate câmpurile.", "Atenție", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtParola!.Text != txtConfirmParola!.Text)
            {
                MessageBox.Show("Parolele nu coincid.", "Atenție", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                ConexiuneBD bd = new ConexiuneBD();
                using MySqlConnection conexiune = bd.GetConnection();
                conexiune.Open();

                string query =
                    "INSERT INTO utilizatori(nume,email,parola,rol) " +
                    "VALUES(@nume,@email,@parola,'user')";

                using MySqlCommand cmd = new MySqlCommand(query, conexiune);
                cmd.Parameters.AddWithValue("@nume", txtNume!.Text);
                cmd.Parameters.AddWithValue("@email", txtEmail!.Text);
                cmd.Parameters.AddWithValue("@parola", txtParola!.Text);

                cmd.ExecuteNonQuery();

                MessageBox.Show("Cont creat!\nPuteți reveni la autentificare.", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}