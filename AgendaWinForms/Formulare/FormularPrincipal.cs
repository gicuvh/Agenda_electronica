using System.Drawing;
using System.Windows.Forms;
using AgendaWinForms.Data;
using AgendaWinForms.Models;
using AgendaWinForms.Services;
using Microsoft.EntityFrameworkCore;

namespace AgendaWinForms.Formulare;

public class FormularPrincipal : Form
{
    private readonly Panel _content = new() { Dock = DockStyle.Fill, BackColor = Ui.Background, AutoScroll = true };
    private readonly Label _pageTitle = Ui.Label("Dashboard", 28, 16, 360, 36, 22, FontStyle.Bold);
    private readonly Dictionary<string, Button> _navButtons = new();

    public FormularPrincipal()
    {
        Text = "Agenda";
        Size = new Size(1100, 720);
        MinimumSize = new Size(1000, 680);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Ui.Background;

        var user = AuthService.CurrentUser ?? throw new InvalidOperationException("Utilizator neautentificat.");
        Controls.Add(_content);
        Controls.Add(BuildSidebar(user));
        Controls.Add(BuildHeader(user));
        Navigate("Dashboard");
    }

    private Control BuildSidebar(User user)
    {
        var sidebar = new Panel { Dock = DockStyle.Left, Width = 200, BackColor = Ui.Sidebar, Padding = new Padding(0, 16, 0, 16) };
        sidebar.Controls.Add(Ui.Label("📖 Agenda", 20, 24, 160, 34, 16, FontStyle.Bold));
        AddNav(sidebar, "Dashboard", "🏠  Dashboard", 86);
        AddNav(sidebar, "Note", "📊  Note", 134);
        AddNav(sidebar, "Teme", "✅  Teme", 182);
        AddNav(sidebar, "Orar", "📅  Orar", 230);
        if (user.Rol == "Admin") AddNav(sidebar, "Admin", "🛡️  Admin", 278);
        sidebar.Controls.Add(Ui.Label("⚙️  Setări", 24, 585, 150, 24, 10, FontStyle.Regular, Ui.Muted));
        sidebar.Controls.Add(Ui.Label("❓  Ajutor", 24, 615, 150, 24, 10, FontStyle.Regular, Ui.Muted));
        return sidebar;
    }

    private Control BuildHeader(User user)
    {
        var header = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Color.White, Padding = new Padding(200, 0, 0, 0) };
        _pageTitle.Location = new Point(228, 18);
        header.Controls.Add(_pageTitle);
        var accountMenu = new ContextMenuStrip();
        accountMenu.Items.Add($"{user.Email}");
        accountMenu.Items[0].Enabled = false;
        accountMenu.Items.Add(new ToolStripSeparator());
        accountMenu.Items.Add("Log out", null, (_, _) => Logout());

        var accountButton = Ui.Button($"👤 {user.NumeComplet} ▾", 680, 16, 240, 38, Color.White, Color.FromArgb(34, 45, 67));
        accountButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        accountButton.FlatAppearance.BorderColor = Ui.Border;
        accountButton.TextAlign = ContentAlignment.MiddleLeft;
        accountButton.Padding = new Padding(10, 0, 0, 0);
        accountButton.Click += (_, _) => accountMenu.Show(accountButton, new Point(0, accountButton.Height));
        header.Controls.Add(accountButton);

        var logoutButton = Ui.Button("Log out", 936, 16, 94, 38, Ui.Danger, Color.White);
        logoutButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        logoutButton.FlatAppearance.BorderColor = Ui.Danger;
        logoutButton.Click += (_, _) => Logout();
        header.Controls.Add(logoutButton);
        return header;
    }

    private void Logout()
    {
        AuthService.Logout();
        new FormularLogin().Show();
        Close();
    }

    private void AddNav(Control sidebar, string key, string text, int top)
    {
        var button = Ui.Button(text, 8, top, 184, 38, Color.Transparent, Color.FromArgb(85, 85, 85));
        button.TextAlign = ContentAlignment.MiddleLeft;
        button.FlatAppearance.BorderSize = 0;
        button.Click += (_, _) => Navigate(key);
        _navButtons[key] = button;
        sidebar.Controls.Add(button);
    }

    private async void Navigate(string page)
    {
        foreach (var (key, button) in _navButtons)
        {
            var active = key == page;
            button.BackColor = active ? Color.FromArgb(232, 240, 254) : Color.Transparent;
            button.ForeColor = active ? Ui.Primary : Color.FromArgb(85, 85, 85);
            button.Font = Ui.Font(10, active ? FontStyle.Bold : FontStyle.Regular);
        }

        _pageTitle.Text = page == "Admin" ? "Panou Administrare" : page;
        _content.Controls.Clear();
        switch (page)
        {
            case "Note": await ShowNoteAsync(); break;
            case "Teme": await ShowTemeAsync(); break;
            case "Orar": await ShowOrarAsync(); break;
            case "Admin": await ShowAdminAsync(); break;
            default: await ShowDashboardAsync(); break;
        }
    }

    private async Task ShowDashboardAsync()
    {
        var user = AuthService.CurrentUser!;
        var isAdmin = IsAdmin();
        using var db = new AppDbContext();
        _content.Controls.Add(Ui.Label($"Bine ai revenit, {user.NumeComplet}!", 28, 28, 600, 24, 10, FontStyle.Regular, Ui.Muted));

        var noteCard = Ui.Card(28, 70, 260, 210);
        noteCard.Controls.Add(Ui.Label("⭐ Note recente", 14, 12, 220, 26, 11, FontStyle.Bold));
        var note = await db.Note.Where(n => isAdmin || n.UserId == user.Id).GroupBy(n => n.Materie)
            .Select(g => new { Materie = g.Key, Media = g.Average(n => n.Valoare) }).Take(3).ToListAsync();
        AddRows(noteCard, note.Select(n => ($"{n.Materie}", $"{n.Media:0.0}")), Ui.Primary);
        _content.Controls.Add(noteCard);

        var temeCard = Ui.Card(312, 70, 260, 210);
        temeCard.Controls.Add(Ui.Label("🔖 Teme urgente", 14, 12, 220, 26, 11, FontStyle.Bold));
        var teme = await db.Teme.Where(t => (isAdmin || t.UserId == user.Id) && !t.Finalizata && t.Deadline >= DateTime.Today)
            .OrderBy(t => t.Deadline).Take(3).ToListAsync();
        AddRows(temeCard, teme.Select(t => (t.Titlu, t.Deadline == DateTime.Today ? "Azi" : $"{(t.Deadline - DateTime.Today).Days} zile")), Ui.Warning);
        _content.Controls.Add(temeCard);

        var orarCard = Ui.Card(596, 70, 260, 210);
        orarCard.Controls.Add(Ui.Label("📅 Orarul de azi", 14, 12, 220, 26, 11, FontStyle.Bold));
        var zi = ZiRomaneasca(DateTime.Today.DayOfWeek);
        var orar = (await db.OrarEntries.Where(o => (isAdmin || o.UserId == user.Id) && o.ZiSaptamana == zi).ToListAsync())
            .OrderBy(o => o.OraInceput)
            .Take(4)
            .ToList();
        AddRows(orarCard, orar.Select(o => ($"{o.OraInceput:hh\\:mm}-{o.OraSfarsit:hh\\:mm}", o.Materie)), Ui.Muted);
        _content.Controls.Add(orarCard);

        var activity = Ui.Card(28, 306, 828, 250);
        activity.Controls.Add(Ui.Label("🕐 Activitate recentă", 14, 12, 300, 28, 12, FontStyle.Bold));
        var activities = await db.Activitati.Where(a => isAdmin || a.UserId == user.Id).OrderByDescending(a => a.Timestamp).Take(6).ToListAsync();
        int top = 52;
        if (!activities.Any()) activity.Controls.Add(Ui.Label("Nu există activitate recentă.", 18, top, 450, 24, 10, FontStyle.Regular, Ui.Muted));
        foreach (var a in activities)
        {
            activity.Controls.Add(Ui.Label($"{IconFor(a.Tip)} {a.Descriere}", 18, top, 620, 24, 10));
            activity.Controls.Add(Ui.Label(a.Timestamp.ToString("dd.MM.yyyy HH:mm"), 650, top, 150, 24, 9, FontStyle.Regular, Ui.Muted));
            top += 30;
        }
        _content.Controls.Add(activity);
    }

    private async Task ShowNoteAsync()
    {
        var canEdit = IsAdmin();
        TextBox? materie = null;
        TextBox? valoare = null;
        TextBox? descriere = null;
        ComboBox? elev = null;
        Button? add = null;

        if (canEdit)
        {
            var form = Ui.Card(28, 28, 850, 112);
            elev = await BuildElevComboAsync(18, 44, 180);
            materie = Ui.TextBox("Materie", 214, 44, 160);
            valoare = Ui.TextBox("Notă (1-10)", 390, 44, 100);
            descriere = Ui.TextBox("Descriere opțională", 506, 44, 170);
            add = Ui.Button("Adaugă", 700, 42, 120, 36);
            form.Controls.AddRange(new Control[] { Ui.Label("Adaugă notă nouă", 18, 12, 250, 24, 12, FontStyle.Bold), elev, materie, valoare, descriere, add });
            _content.Controls.Add(form);
        }

        var grid = Ui.Grid();
        if (canEdit) grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Elev", DataPropertyName = "Elev" });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Materie", DataPropertyName = "Materie" });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Notă", DataPropertyName = "Valoare", Width = 70 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Data", DataPropertyName = "DataFormatata" });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Descriere", DataPropertyName = "Descriere" });
        if (canEdit) grid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Acțiuni", Text = "Șterge", UseColumnTextForButtonValue = true, Width = 90 });
        var gridPanel = Ui.Card(28, canEdit ? 166 : 28, 850, canEdit ? 390 : 528);
        gridPanel.Controls.Add(grid);
        _content.Controls.Add(gridPanel);

        async Task LoadGrid()
        {
            using var db = new AppDbContext();
            if (canEdit)
            {
                grid.DataSource = await db.Note.Include(n => n.User).OrderByDescending(n => n.Data)
                    .Select(n => new { n.Id, Elev = n.User!.NumeComplet, n.Materie, n.Valoare, DataFormatata = n.Data.ToString("dd.MM.yyyy"), n.Descriere }).ToListAsync();
            }
            else
            {
                grid.DataSource = await db.Note.Where(n => n.UserId == AuthService.CurrentUser!.Id).OrderByDescending(n => n.Data)
                    .Select(n => new { n.Id, n.Materie, n.Valoare, DataFormatata = n.Data.ToString("dd.MM.yyyy"), n.Descriere }).ToListAsync();
            }
        }
        if (canEdit && add is not null && elev is not null && materie is not null && valoare is not null && descriere is not null)
        {
            add.Click += async (_, _) =>
            {
                if (!int.TryParse(valoare.Text, out var val) || val is < 1 or > 10 || string.IsNullOrWhiteSpace(materie.Text))
                { MessageBox.Show("Completează materia și nota (1-10).", "Atenție"); return; }
                var elevId = SelectedElevId(elev);
                if (elevId is null) { MessageBox.Show("Selectează elevul."); return; }
                using var db = new AppDbContext();
                db.Note.Add(new Nota { Materie = materie.Text.Trim(), Valoare = val, Descriere = descriere.Text.Trim(), UserId = elevId.Value });
                db.Activitati.Add(new Activitate { Descriere = $"Adminul a adăugat o notă la {materie.Text.Trim()}", Tip = "nota", UserId = elevId.Value });
                await db.SaveChangesAsync();
                materie.Clear(); valoare.Clear(); descriere.Clear();
                await LoadGrid();
            };
            grid.CellContentClick += async (_, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex != grid.Columns.Count - 1) return;
                var id = RowId(grid, e.RowIndex);
                using var db = new AppDbContext();
                var nota = await db.Note.FindAsync(id);
                if (nota is not null) { db.Note.Remove(nota); await db.SaveChangesAsync(); }
                await LoadGrid();
            };
        }
        await LoadGrid();
    }

    private async Task ShowTemeAsync()
    {
        var canEdit = IsAdmin();
        TextBox? titlu = null;
        TextBox? materie = null;
        DateTimePicker? deadline = null;
        ComboBox? elev = null;
        Button? add = null;

        if (canEdit)
        {
            var form = Ui.Card(28, 28, 850, 112);
            elev = await BuildElevComboAsync(18, 44, 180);
            titlu = Ui.TextBox("Titlu temă", 214, 44, 180);
            materie = Ui.TextBox("Materie", 410, 44, 150);
            deadline = new DateTimePicker { Location = new Point(576, 44), Size = new Size(120, 32), Format = DateTimePickerFormat.Short };
            add = Ui.Button("Adaugă", 716, 42, 100, 36);
            form.Controls.AddRange(new Control[] { Ui.Label("Adaugă temă nouă", 18, 12, 250, 24, 12, FontStyle.Bold), elev, titlu, materie, deadline, add });
            _content.Controls.Add(form);
        }

        var grid = Ui.Grid();
        if (canEdit) grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Elev", DataPropertyName = "Elev" });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Titlu", DataPropertyName = "Titlu" });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Materie", DataPropertyName = "Materie" });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Deadline", DataPropertyName = "DeadlineFormatat" });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", DataPropertyName = "Status" });
        if (canEdit)
        {
            grid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Finalizare", Text = "✓ Finalizat", UseColumnTextForButtonValue = true });
            grid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Ștergere", Text = "Șterge", UseColumnTextForButtonValue = true });
        }
        var gridPanel = Ui.Card(28, canEdit ? 166 : 28, 850, canEdit ? 390 : 528);
        gridPanel.Controls.Add(grid);
        _content.Controls.Add(gridPanel);

        async Task LoadGrid()
        {
            using var db = new AppDbContext();
            if (canEdit)
            {
                var teme = await db.Teme.Include(t => t.User).OrderBy(t => t.Deadline).ToListAsync();
                grid.DataSource = teme.Select(t => new { t.Id, Elev = t.User?.NumeComplet ?? "-", t.Titlu, t.Materie, DeadlineFormatat = t.Deadline.ToString("dd.MM.yyyy"), Status = t.Finalizata ? "✅ Finalizată" : t.Deadline < DateTime.Today ? "❌ Expirată" : "⏳ În așteptare" }).ToList();
            }
            else
            {
                var teme = await db.Teme.Where(t => t.UserId == AuthService.CurrentUser!.Id).OrderBy(t => t.Deadline).ToListAsync();
                grid.DataSource = teme.Select(t => new { t.Id, t.Titlu, t.Materie, DeadlineFormatat = t.Deadline.ToString("dd.MM.yyyy"), Status = t.Finalizata ? "✅ Finalizată" : t.Deadline < DateTime.Today ? "❌ Expirată" : "⏳ În așteptare" }).ToList();
            }
        }
        if (canEdit && add is not null && elev is not null && titlu is not null && materie is not null && deadline is not null)
        {
            add.Click += async (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(titlu.Text)) { MessageBox.Show("Completează titlul temei."); return; }
                var elevId = SelectedElevId(elev);
                if (elevId is null) { MessageBox.Show("Selectează elevul."); return; }
                using var db = new AppDbContext();
                db.Teme.Add(new Tema { Titlu = titlu.Text.Trim(), Materie = materie.Text.Trim(), Deadline = deadline.Value.Date, UserId = elevId.Value });
                db.Activitati.Add(new Activitate { Descriere = $"Adminul a adăugat tema \"{titlu.Text.Trim()}\"", Tip = "tema", UserId = elevId.Value });
                await db.SaveChangesAsync();
                titlu.Clear(); materie.Clear();
                await LoadGrid();
            };
            grid.CellContentClick += async (_, e) =>
            {
                var finalizareColumnIndex = grid.Columns.Count - 2;
                var stergereColumnIndex = grid.Columns.Count - 1;
                if (e.RowIndex < 0 || (e.ColumnIndex != finalizareColumnIndex && e.ColumnIndex != stergereColumnIndex)) return;
                var id = RowId(grid, e.RowIndex);
                using var db = new AppDbContext();
                var tema = await db.Teme.FindAsync(id);
                if (tema is null) return;
                if (e.ColumnIndex == finalizareColumnIndex)
                {
                    tema.Finalizata = true;
                    db.Activitati.Add(new Activitate { Descriere = $"Adminul a marcat tema \"{tema.Titlu}\" ca finalizată", Tip = "tema", UserId = tema.UserId });
                }
                else db.Teme.Remove(tema);
                await db.SaveChangesAsync();
                await LoadGrid();
            };
        }
        await LoadGrid();
    }

    private async Task ShowOrarAsync()
    {
        var canEdit = IsAdmin();
        ComboBox? zi = null;
        TextBox? start = null;
        TextBox? end = null;
        TextBox? materie = null;
        TextBox? profesor = null;
        ComboBox? elev = null;
        Button? add = null;

        if (canEdit)
        {
            var form = Ui.Card(28, 28, 850, 112);
            elev = await BuildElevComboAsync(18, 44, 150);
            zi = new ComboBox { Location = new Point(184, 44), Size = new Size(105, 32), DropDownStyle = ComboBoxStyle.DropDownList };
            zi.Items.AddRange(new object[] { "Luni", "Marti", "Miercuri", "Joi", "Vineri", "Sambata", "Duminica" }); zi.SelectedIndex = 0;
            start = Ui.TextBox("08:00", 305, 44, 76);
            end = Ui.TextBox("09:00", 397, 44, 76);
            materie = Ui.TextBox("Materie", 489, 44, 140);
            profesor = Ui.TextBox("Profesor", 645, 44, 100);
            add = Ui.Button("Adaugă", 760, 42, 76, 36);
            form.Controls.AddRange(new Control[] { Ui.Label("Adaugă oră nouă", 18, 12, 250, 24, 12, FontStyle.Bold), elev, zi, start, end, materie, profesor, add });
            _content.Controls.Add(form);
        }

        var grid = Ui.Grid();
        if (canEdit) grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Elev", DataPropertyName = "Elev" });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Zi", DataPropertyName = "ZiSaptamana" });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Interval", DataPropertyName = "Interval" });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Materie", DataPropertyName = "Materie" });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Profesor", DataPropertyName = "Profesor" });
        if (canEdit) grid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Acțiuni", Text = "Șterge", UseColumnTextForButtonValue = true, Width = 90 });
        var gridPanel = Ui.Card(28, canEdit ? 166 : 28, 850, canEdit ? 390 : 528);
        gridPanel.Controls.Add(grid);
        _content.Controls.Add(gridPanel);

        async Task LoadGrid()
        {
            using var db = new AppDbContext();
            var query = canEdit ? db.OrarEntries.Include(o => o.User) : db.OrarEntries.Where(o => o.UserId == AuthService.CurrentUser!.Id).Include(o => o.User);
            var items = await query.ToListAsync();
            grid.DataSource = items.OrderBy(o => IndexZi(o.ZiSaptamana)).ThenBy(o => o.OraInceput)
                .Select(o => canEdit
                    ? new { o.Id, Elev = o.User?.NumeComplet ?? "-", o.ZiSaptamana, Interval = $"{o.OraInceput:hh\\:mm} - {o.OraSfarsit:hh\\:mm}", o.Materie, o.Profesor }
                    : new { o.Id, Elev = string.Empty, o.ZiSaptamana, Interval = $"{o.OraInceput:hh\\:mm} - {o.OraSfarsit:hh\\:mm}", o.Materie, o.Profesor })
                .ToList();
        }
        if (canEdit && add is not null && elev is not null && zi is not null && start is not null && end is not null && materie is not null && profesor is not null)
        {
            add.Click += async (_, _) =>
            {
                if (!TimeSpan.TryParse(start.Text, out var oraStart) || !TimeSpan.TryParse(end.Text, out var oraEnd) || string.IsNullOrWhiteSpace(materie.Text))
                { MessageBox.Show("Completează ora în format HH:MM și materia."); return; }
                var elevId = SelectedElevId(elev);
                if (elevId is null) { MessageBox.Show("Selectează elevul."); return; }
                using var db = new AppDbContext();
                db.OrarEntries.Add(new OrarEntry { ZiSaptamana = zi.Text, OraInceput = oraStart, OraSfarsit = oraEnd, Materie = materie.Text.Trim(), Profesor = profesor.Text.Trim(), UserId = elevId.Value });
                db.Activitati.Add(new Activitate { Descriere = "Adminul a actualizat orarul", Tip = "orar", UserId = elevId.Value });
                await db.SaveChangesAsync();
                start.Clear(); end.Clear(); materie.Clear(); profesor.Clear();
                await LoadGrid();
            };
            grid.CellContentClick += async (_, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex != grid.Columns.Count - 1) return;
                var id = RowId(grid, e.RowIndex);
                using var db = new AppDbContext();
                var entry = await db.OrarEntries.FindAsync(id);
                if (entry is not null) { db.OrarEntries.Remove(entry); await db.SaveChangesAsync(); }
                await LoadGrid();
            };
        }
        await LoadGrid();
    }

    private async Task ShowAdminAsync()
    {
        if (AuthService.CurrentUser?.Rol != "Admin") { await ShowDashboardAsync(); return; }
        using var db = new AppDbContext();
        var users = await db.Users.ToListAsync();
        var totalNote = await db.Note.CountAsync();
        var totalTeme = await db.Teme.CountAsync();
        var totalOrar = await db.OrarEntries.CountAsync();
        AddStat("👥", users.Count.ToString(), "Utilizatori activi", 28, Ui.Primary);
        AddStat("📖", (await db.Note.Select(n => n.Materie).Distinct().CountAsync()).ToString(), "Materii înregistrate", 240, Ui.Success);
        AddStat("📈", (await db.Activitati.CountAsync()).ToString(), "Activități", 452, Ui.Warning);
        AddStat("📊", $"{Math.Min(94, users.Count * 3)}%", "Rata de utilizare", 664, Ui.Purple);

        var activity = Ui.Card(28, 170, 530, 210);
        activity.Controls.Add(Ui.Label("⚡ Activitate recentă", 14, 12, 260, 26, 12, FontStyle.Bold));
        var acts = await db.Activitati.Include(a => a.User).OrderByDescending(a => a.Timestamp).Take(5).ToListAsync();
        int top = 48;
        foreach (var a in acts)
        {
            activity.Controls.Add(Ui.Label($"👤 {a.User?.NumeComplet ?? "?"} — {a.Descriere}", 18, top, 470, 24, 9));
            top += 28;
        }
        _content.Controls.Add(activity);
        var alerts = Ui.Card(582, 170, 274, 210);
        alerts.Controls.Add(Ui.Label("⚠️ Alerte sistem", 14, 12, 220, 26, 12, FontStyle.Bold));
        alerts.Controls.Add(Ui.Label("ℹ️ Backup local activ", 18, 52, 230, 24, 9, FontStyle.Regular, Ui.Primary));
        alerts.Controls.Add(Ui.Label($"📌 {totalTeme} teme înregistrate", 18, 84, 230, 24, 9, FontStyle.Regular, Ui.Warning));
        alerts.Controls.Add(Ui.Label($"🔴 Date: {totalNote + totalOrar} înregistrări", 18, 116, 230, 24, 9, FontStyle.Regular, Ui.Danger));
        _content.Controls.Add(alerts);

        var grid = Ui.Grid();
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nume", DataPropertyName = "NumeComplet" });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Email", DataPropertyName = "Email" });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Rol", DataPropertyName = "Rol" });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Înregistrat", DataPropertyName = "DataInregistrare" });
        grid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Acțiuni", Text = "Șterge", UseColumnTextForButtonValue = true });
        var gridPanel = Ui.Card(28, 410, 828, 260);
        gridPanel.Controls.Add(grid);
        _content.Controls.Add(gridPanel);

        async Task LoadUsers()
        {
            using var db2 = new AppDbContext();
            grid.DataSource = await db2.Users.Select(u => new { u.Id, u.NumeComplet, u.Email, u.Rol, DataInregistrare = u.DataInregistrare.ToString("dd.MM.yyyy") }).ToListAsync();
        }
        grid.CellContentClick += async (_, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 4) return;
            var id = RowId(grid, e.RowIndex);
            if (id == AuthService.CurrentUser!.Id) { MessageBox.Show("Nu poți șterge propriul cont."); return; }
            using var db2 = new AppDbContext();
            var user = await db2.Users.FindAsync(id);
            if (user is not null) { db2.Users.Remove(user); await db2.SaveChangesAsync(); }
            await LoadUsers();
        };
        await LoadUsers();
    }

    private void AddStat(string icon, string value, string label, int left, Color color)
    {
        var card = Ui.Card(left, 28, 190, 110);
        card.BackColor = color;
        card.Controls.Add(Ui.Label(icon, 16, 10, 60, 30, 18, FontStyle.Regular, Color.White));
        card.Controls.Add(Ui.Label(value, 16, 42, 120, 36, 24, FontStyle.Bold, Color.White));
        card.Controls.Add(Ui.Label(label, 16, 78, 150, 22, 9, FontStyle.Regular, Color.White));
        _content.Controls.Add(card);
    }

    private static void AddRows(Control card, IEnumerable<(string Left, string Right)> rows, Color color)
    {
        int top = 52;
        var any = false;
        foreach (var row in rows)
        {
            any = true;
            card.Controls.Add(Ui.Label(row.Left, 18, top, 145, 24, 10));
            var badge = Ui.Label(row.Right, 168, top, 70, 24, 9, FontStyle.Bold, Color.White);
            badge.TextAlign = ContentAlignment.MiddleCenter;
            badge.BackColor = color;
            card.Controls.Add(badge);
            top += 34;
        }
        if (!any) card.Controls.Add(Ui.Label("Nu există date.", 18, top, 180, 24, 10, FontStyle.Regular, Ui.Muted));
    }

    private static int RowId(DataGridView grid, int rowIndex)
    {
        var item = grid.Rows[rowIndex].DataBoundItem;
        return (int)(item?.GetType().GetProperty("Id")?.GetValue(item) ?? 0);
    }

    private static async Task<ComboBox> BuildElevComboAsync(int x, int y, int width)
    {
        using var db = new AppDbContext();
        var elevi = await db.Users
            .Where(u => u.Rol != "Admin")
            .OrderBy(u => u.NumeComplet)
            .Select(u => new ElevOption(u.Id, $"{u.NumeComplet} ({u.Email})"))
            .ToListAsync();

        var combo = new ComboBox
        {
            Location = new Point(x, y),
            Size = new Size(width, 32),
            DropDownStyle = ComboBoxStyle.DropDownList,
            DataSource = elevi,
            DisplayMember = nameof(ElevOption.Text),
            ValueMember = nameof(ElevOption.Id)
        };

        return combo;
    }

    private static int? SelectedElevId(ComboBox elev)
    {
        return elev.SelectedValue is int id ? id : null;
    }

    private static string IconFor(string tip) => tip switch { "nota" => "📊", "tema" => "✅", "orar" => "📅", _ => "•" };
    private static bool IsAdmin() => AuthService.CurrentUser?.Rol == "Admin";
    private static int IndexZi(string zi) => Array.IndexOf(new[] { "Luni", "Marti", "Miercuri", "Joi", "Vineri", "Sambata", "Duminica" }, zi);
    private static string ZiRomaneasca(DayOfWeek zi) => zi switch
    {
        DayOfWeek.Monday => "Luni",
        DayOfWeek.Tuesday => "Marti",
        DayOfWeek.Wednesday => "Miercuri",
        DayOfWeek.Thursday => "Joi",
        DayOfWeek.Friday => "Vineri",
        DayOfWeek.Saturday => "Sambata",
        _ => "Duminica"
    };

    private sealed record ElevOption(int Id, string Text);
}
