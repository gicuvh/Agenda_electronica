using System;
using System.Drawing;
using System.Windows.Forms;
using AgendaWinForms.Services;

namespace AgendaWinForms.Formulare;

public class FormularRegister : Form
{
    public FormularRegister()
    {
        // Setări Formular
        Text = "Agenda — Înregistrare";
        Size = new Size(1100, 750);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.White; // Fundal alb curat ca în imagine
        FormBorderStyle = FormBorderStyle.Sizable;

        // 1. SIDEBAR (Stânga)
        var sidebar = new Panel { Dock = DockStyle.Left, Width = 220, BackColor = Color.White };
        sidebar.Paint += (s, e) => e.Graphics.DrawLine(Pens.LightGray, 219, 0, 219, Height);

        sidebar.Controls.Add(Ui.Label("📖 Agenda", 20, 20, 160, 32, 14, FontStyle.Bold, Color.FromArgb(44, 62, 80)));
        
        int startTop = 80;
        string[] navItems = { "🏠 Dashboard", "📊 Note", "✅ Teme", "📅 Orar" };
        foreach (var item in navItems) {
            sidebar.Controls.Add(NavButton(item, startTop));
            startTop += 45;
        }

        // Butoane Jos Sidebar
        sidebar.Controls.Add(NavButton("⚙️ Setări", Height - 120));
        sidebar.Controls.Add(NavButton("❓ Ajutor", Height - 80));
        Controls.Add(sidebar);

        // 2. HEADER (Sus - Dreapta)
        var header = new Panel { Dock = DockStyle.Top, Height = 60 };
        var userProfile = Ui.Label("👤 Utilizator Nou\nGuest", Width - 380, 15, 150, 40, 8);
        userProfile.TextAlign = ContentAlignment.MiddleRight;
        header.Controls.Add(userProfile);
        header.Controls.Add(Ui.Label("🔔", Width - 240, 20, 30, 30, 12));
        Controls.Add(header);

        // 3. CONTINUT CENTRAL
        var mainContent = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(252, 253, 255) };
        
        // Titlu Central
        var lblTitle = Ui.Label("Creare cont", 0, 40, 800, 50, 28, FontStyle.Bold);
        lblTitle.TextAlign = ContentAlignment.MiddleCenter;
        lblTitle.Dock = DockStyle.Top;
        mainContent.Controls.Add(lblTitle);

        var lblSub = Ui.Label("Alătură-te comunității Agenda!", 0, 90, 800, 30, 11, FontStyle.Regular, Ui.Muted);
        lblSub.TextAlign = ContentAlignment.MiddleCenter;
        lblSub.Dock = DockStyle.Top;
        mainContent.Controls.Add(lblSub);

        // CARDUL VERDE
        var card = new Panel { 
            Size = new Size(420, 520), 
            Location = new Point(0, 0), 
            BackColor = Color.White 
        };
        card.Paint += (s, e) => {
            // Chenar 
            using var pen = new Pen(Color.FromArgb(200, 230, 201), 2);
            Ui.PaintRoundedBorder(card, e); 
        };

        // Iconița Check
        var checkCircle = new Label { 
            Text = "✔️", 
            Size = new Size(50, 50), 
            Location = new Point(185, -25), 
            BackColor = Color.FromArgb(232, 245, 233),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 15, FontStyle.Bold),
            ForeColor = Color.Green
        };
       
        card.Controls.Add(checkCircle);

        // Inputuri stilizate 
        card.Controls.Add(Ui.InputGroup("Nume complet", "ex. Andrei Popescu", 40, 60, "👤"));
        card.Controls.Add(Ui.InputGroup("Adresă de e-mail sau Utilizator", "exemplu@email.com", 40, 135, "@"));
        card.Controls.Add(Ui.InputGroup("Parolă", "••••••••", 40, 210, "🔒"));
        card.Controls.Add(Ui.InputGroup("Confirmă parola", "••••••••", 40, 285, "🔒"));

        // Buton Albastru
        var btnCreare = Ui.Button("Creează cont", 40, 370, 340, 45);
        btnCreare.BackColor = Color.FromArgb(37, 110, 215); // Albastru Google/Modern
        card.Controls.Add(btnCreare);

        // Buton Google (Alb/Griu)
        var btnGoogle = Ui.Button("Înregistrare rapidă cu Google", 40, 425, 340, 45, Color.FromArgb(235, 243, 255), Color.Black);
        card.Controls.Add(btnGoogle);

        mainContent.Controls.Add(card);

        // Footer link
        var footerLink = new LinkLabel {
            Text = "Ai deja un cont? Autentifică-te →",
            Location = new Point(320, 680),
            AutoSize = true,
            LinkColor = Color.FromArgb(37, 110, 215),
            Font = Ui.Font(10)
        };
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

    private static Button NavButton(string text, int top)
    {
        var btn = new Button {
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
}
