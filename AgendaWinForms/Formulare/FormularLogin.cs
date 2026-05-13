using System.Drawing;
using System.Windows.Forms;
using AgendaWinForms.Services;

namespace AgendaWinForms.Formulare;

public class FormularLogin : Form
{
    private const int SidebarWidth = 200;
    private readonly TextBox _txtEmail;
    private readonly TextBox _txtParola;
    private readonly Label _errorText;
    private readonly Panel _card;

    public FormularLogin()
    {
        Text = "Agenda — Autentificare";
        Size = new Size(900, 600);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(240, 244, 255);
        MinimumSize = new Size(900, 600);

        var sidebar = new Panel { Dock = DockStyle.Left, Width = SidebarWidth, BackColor = Ui.Sidebar };
        sidebar.Controls.Add(Ui.Label("📖 Agenda", 20, 24, 160, 32, 16, FontStyle.Bold));
        sidebar.Controls.Add(NavPreview("🏠  Dashboard", 86));
        sidebar.Controls.Add(NavPreview("📊  Note", 134));
        sidebar.Controls.Add(NavPreview("✅  Teme", 182));
        sidebar.Controls.Add(NavPreview("📅  Orar", 230));
        Controls.Add(sidebar);

        _card = Ui.Card(0, 0, 400, 390);
        _card.Paint += Ui.PaintRoundedBorder;
        var title = Ui.Label("Autentificare", 0, 22, 368, 36, 24, FontStyle.Bold);
        title.TextAlign = ContentAlignment.MiddleCenter;
        _card.Controls.Add(title);
        var subtitle = Ui.Label("Bine ai revenit la Agenda!", 0, 62, 368, 26, 10, FontStyle.Regular, Ui.Muted);
        subtitle.TextAlign = ContentAlignment.MiddleCenter;
        _card.Controls.Add(subtitle);
        _errorText = Ui.Label(string.Empty, 32, 98, 304, 44, 9, FontStyle.Regular, Ui.Danger);
        _card.Controls.Add(_errorText);
        _card.Controls.Add(Ui.Label("Email", 32, 146, 304, 20, 9, FontStyle.Regular, Color.FromArgb(85, 85, 85)));
        _txtEmail = Ui.TextBox("admin@gmail.com", 32, 168, 304);
        _card.Controls.Add(_txtEmail);
        _card.Controls.Add(Ui.Label("Parolă", 32, 210, 304, 20, 9, FontStyle.Regular, Color.FromArgb(85, 85, 85)));
        _txtParola = Ui.TextBox("Admin123!", 32, 232, 304, true);
        _card.Controls.Add(_txtParola);
        var login = Ui.Button("Autentifică-te", 32, 286, 304, 42);
        login.Click += Login_Click;
        _card.Controls.Add(login);
        var register = new LinkLabel
        {
            Text = "Nu ai cont? Creează unul →",
            Location = new Point(92, 344),
            AutoSize = true,
            LinkColor = Ui.Primary,
            Font = Ui.Font(9)
        };
        register.Click += (_, _) => { new FormularRegister().Show(); Hide(); };
        _card.Controls.Add(register);
        Controls.Add(_card);

        Resize += (_, _) => CenterCard();
        CenterCard();
    }

    private void CenterCard()
    {
        var contentWidth = ClientSize.Width - SidebarWidth;
        var x = SidebarWidth + Math.Max(0, (contentWidth - _card.Width) / 2);
        var y = Math.Max(0, (ClientSize.Height - _card.Height) / 2);
        _card.Location = new Point(x, y);
    }

    private static Button NavPreview(string text, int top)
    {
        var button = Ui.Button(text, 8, top, 184, 38, Color.Transparent, Color.FromArgb(85, 85, 85));
        button.FlatAppearance.BorderSize = 0;
        button.TextAlign = ContentAlignment.MiddleLeft;
        return button;
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
