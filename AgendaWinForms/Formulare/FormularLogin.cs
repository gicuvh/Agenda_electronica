using System;
using System.Drawing;
using System.Windows.Forms;
using AgendaWinForms.Services;

namespace AgendaWinForms.Formulare;

public class FormularLogin : Form
{
    private readonly TextBox _txtEmail;
    private readonly TextBox _txtParola;
    private readonly Label _errorText;

    public FormularLogin()
    {
        Text = "Agenda — Autentificare";
        Size = new Size(1100, 750);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.White;
        FormBorderStyle = FormBorderStyle.Sizable;
        MinimumSize = new Size(900, 650);

        var sidebar = new Panel { Dock = DockStyle.Left, Width = 220, BackColor = Color.White };
        sidebar.Paint += (s, e) => e.Graphics.DrawLine(Pens.LightGray, 219, 0, 219, Height);
        sidebar.Controls.Add(Ui.Label("📖 Agenda", 20, 20, 160, 32, 14, FontStyle.Bold, Color.FromArgb(44, 62, 80)));

        int startTop = 80;
        string[] navItems = { "🏠 Dashboard", "📊 Note", "✅ Teme", "📅 Orar" };
        foreach (var item in navItems)
        {
            sidebar.Controls.Add(NavButton(item, startTop));
            startTop += 45;
        }

        sidebar.Controls.Add(NavButton("⚙️ Setări", Height - 120));
        sidebar.Controls.Add(NavButton("❓ Ajutor", Height - 80));
        Controls.Add(sidebar);

        var header = new Panel { Dock = DockStyle.Top, Height = 60 };
        var userProfile = Ui.Label("👤 Utilizator\nGuest", Width - 380, 15, 150, 40, 8);
        userProfile.TextAlign = ContentAlignment.MiddleRight;
        header.Controls.Add(userProfile);
        header.Controls.Add(Ui.Label("🔔", Width - 240, 20, 30, 30, 12));
        Controls.Add(header);

        var mainContent = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(252, 253, 255) };

        var lblTitle = Ui.Label("Autentificare", 0, 40, 800, 50, 28, FontStyle.Bold);
        lblTitle.TextAlign = ContentAlignment.MiddleCenter;
        lblTitle.Dock = DockStyle.Top;
        mainContent.Controls.Add(lblTitle);

        var lblSub = Ui.Label("Bine ai revenit la Agenda!", 0, 90, 800, 30, 11, FontStyle.Regular, Ui.Muted);
        lblSub.TextAlign = ContentAlignment.MiddleCenter;
        lblSub.Dock = DockStyle.Top;
        mainContent.Controls.Add(lblSub);

        var card = new Panel
        {
            Size = new Size(420, 390),
            Location = new Point(0, 0),
            BackColor = Color.White,
            Tag = "SuccessCard"
        };
        card.Paint += Ui.PaintRoundedBorder;

        var emailGroup = AuthInputGroup("Adresă de e-mail", "admin@gmail.com", 40, 60, "@", false);
        _txtEmail = (TextBox)emailGroup.Tag!;
        card.Controls.Add(emailGroup);

        var passwordGroup = AuthInputGroup("Parolă", "Admin123!", 40, 135, "🔒", true);
        _txtParola = (TextBox)passwordGroup.Tag!;
        card.Controls.Add(passwordGroup);

        _errorText = Ui.Label(string.Empty, 40, 210, 340, 36, 9, FontStyle.Regular, Ui.Danger);
        _errorText.TextAlign = ContentAlignment.MiddleCenter;
        card.Controls.Add(_errorText);

        var login = Ui.Button("Autentifică-te", 40, 260, 340, 45);
        login.BackColor = Color.FromArgb(37, 110, 215);
        login.Click += Login_Click;
        card.Controls.Add(login);

        var google = Ui.Button("Autentificare rapidă cu Google", 40, 315, 340, 45, Color.FromArgb(235, 243, 255), Color.Black);
        card.Controls.Add(google);

        mainContent.Controls.Add(card);

        var footerLink = new LinkLabel
        {
            Text = "Nu ai cont? Creează unul →",
            Location = new Point(320, 680),
            AutoSize = true,
            LinkColor = Color.FromArgb(37, 110, 215),
            Font = Ui.Font(10)
        };
        footerLink.Click += (_, _) => { new FormularRegister().Show(); Hide(); };
        mainContent.Controls.Add(footerLink);

        Controls.Add(mainContent);

        mainContent.Resize += (_, _) => CenterContent(mainContent, card, footerLink);
        CenterContent(mainContent, card, footerLink);
    }

    private static void CenterContent(Control container, Control card, Control footerLink)
    {
        var cardX = Math.Max(24, (container.ClientSize.Width - card.Width) / 2);
        var cardY = Math.Max(120, (container.ClientSize.Height - card.Height) / 2);
        card.Location = new Point(cardX, cardY);

        footerLink.Location = new Point(
            Math.Max(24, cardX + (card.Width - footerLink.Width) / 2),
            card.Bottom + 22);
    }

    private static Panel AuthInputGroup(string labelText, string placeholder, int x, int y, string icon, bool password)
    {
        var container = new Panel { Location = new Point(x, y), Size = new Size(320, 65), BackColor = Color.Transparent };

        container.Controls.Add(new Label
        {
            Text = labelText,
            Location = new Point(0, 0),
            AutoSize = true,
            Font = Ui.Font(9, FontStyle.Bold),
            ForeColor = Color.FromArgb(74, 85, 104)
        });

        var inputField = new Panel
        {
            Location = new Point(0, 22),
            Size = new Size(310, 38),
            BackColor = Ui.LightGrayBg,
            Padding = new Padding(8, 6, 8, 6)
        };

        inputField.Controls.Add(new Label
        {
            Text = icon,
            Location = new Point(5, 8),
            Width = 25,
            Font = Ui.Font(11),
            ForeColor = Ui.Muted
        });

        var txt = new TextBox
        {
            PlaceholderText = placeholder,
            Location = new Point(35, 9),
            Width = 240,
            BorderStyle = BorderStyle.None,
            BackColor = Ui.LightGrayBg,
            Font = Ui.Font(10),
            UseSystemPasswordChar = password
        };

        inputField.Controls.Add(txt);
        inputField.Paint += (s, e) =>
        {
            using var pen = new Pen(Color.FromArgb(226, 232, 240));
            e.Graphics.DrawRectangle(pen, 0, 0, inputField.Width - 1, inputField.Height - 1);
        };

        container.Controls.Add(inputField);
        container.Tag = txt;
        return container;
    }

    private static Button NavButton(string text, int top)
    {
        var btn = new Button
        {
            Text = "   " + text,
            Location = new Point(10, top),
            Size = new Size(200, 40),
            FlatStyle = FlatStyle.Flat,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = Ui.Font(10),
            ForeColor = Color.FromArgb(100, 100, 100),
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 240, 240);
        return btn;
    }

    private async void Login_Click(object? sender, EventArgs e)
    {
        var (success, message) = await AuthService.LoginAsync(_txtEmail.Text, _txtParola.Text);
        if (!success)
        {
            _errorText.Text = message;
            return;
        }

        var principal = new FormularPrincipal();
        principal.FormClosed += (_, _) => Close();
        principal.Show();
        Hide();
    }
}
