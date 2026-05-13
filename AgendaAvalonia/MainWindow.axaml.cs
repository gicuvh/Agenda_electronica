using AgendaAvalonia.Data;
using AgendaAvalonia.Models;
using AgendaAvalonia.Services;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Microsoft.EntityFrameworkCore;

namespace AgendaAvalonia;

public partial class MainWindow : Window
{
    private readonly Dictionary<string, Button> _navButtons = new();
    private TextBlock? _pageTitle;
    private StackPanel? _content;
    private bool _initialized;
    private static bool _darkMode;
    private string _currentPage = "Dashboard";
    private bool _registerView;

    private static readonly IBrush Primary = Brush.Parse("#256ed7");
    private static IBrush AppBackground => Brush.Parse(_darkMode ? "#111827" : "#fcfdff");
    private static IBrush Surface => Brush.Parse(_darkMode ? "#1f2937" : "#ffffff");
    private static IBrush TextBrush => Brush.Parse(_darkMode ? "#f9fafb" : "#111827");
    private static IBrush SecondaryText => Brush.Parse(_darkMode ? "#cbd5e1" : "#2d3748");
    private static IBrush Muted => Brush.Parse(_darkMode ? "#94a3b8" : "#718096");
    private static IBrush CardBorder => Brush.Parse(_darkMode ? "#374151" : "#e2e8f0");
    private static readonly IBrush Danger = Brush.Parse("#dc3545");
    private static readonly IBrush Success = Brush.Parse("#198754");
    private static readonly IBrush Warning = Brush.Parse("#fd7e14");
    private static readonly IBrush Purple = Brush.Parse("#6f42c1");

    public MainWindow()
    {
        InitializeComponent();
        Background = AppBackground;
    }

    protected override async void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        if (_initialized)
            return;

        _initialized = true;
        Content = CenterMessage("Se incarca baza de date...");
        await AuthService.InitializeAsync();
        ShowLogin();
    }

    private void ShowLogin()
    {
        _registerView = false;
        Title = "Agenda - Autentificare";
        _navButtons.Clear();

        var email = Input("admin@gmail.com");
        var password = Input("Admin123!", true);
        var message = Text("", Danger, 12);

        var card = Card(400);
        card.Child = new StackPanel
        {
            Spacing = 12,
            Children =
            {
                Text("Autentificare", TextBrush, 28, FontWeight.Bold, HorizontalAlignment.Center),
                Text("Bine ai revenit la Agenda!", Muted, 14, FontWeight.Normal, HorizontalAlignment.Center),
                message,
                Field("Email", email),
                Field("Parola", password),
                ActionButton("Autentifica-te", async () =>
                {
                    var result = await AuthService.LoginAsync(email.Text ?? "", password.Text ?? "");
                    if (!result.Success)
                    {
                        message.Text = result.Message;
                        return;
                    }

                    ShowShell("Dashboard");
                }),
                LinkButton("Nu ai cont? Creeaza unul", ShowRegister)
            }
        };

        Content = AuthLayout(card);
    }

    private void ShowRegister()
    {
        _registerView = true;
        Title = "Agenda - Inregistrare";
        _navButtons.Clear();

        var name = Input("ex. Andrei Popescu");
        var email = Input("exemplu@email.com");
        var password = Input("********", true);
        var confirm = Input("********", true);
        var message = Text("", Danger, 12);

        var form = new StackPanel
        {
            Spacing = 7,
            Children =
            {
                message,
                RegisterField("Nume complet", "👤", name),
                RegisterField("Adresa de e-mail sau Utilizator", "@", email),
                RegisterField("Parola", "🔒", password, true),
                RegisterField("Confirma parola", "🔒", confirm, true),
                ActionButton("Creeaza cont", async () =>
                {
                    if ((password.Text ?? "") != (confirm.Text ?? ""))
                    {
                        message.Text = "Parolele nu coincid.";
                        return;
                    }

                    var result = await AuthService.RegisterAsync(name.Text ?? "", email.Text ?? "", password.Text ?? "");
                    if (!result.Success)
                    {
                        message.Text = result.Message;
                        return;
                    }

                    ShowShell("Dashboard");
                }),
                GoogleButton("Inregistrare rapida cu Google")
            }
        };

        var card = RegisterCard(form);
        Content = RegisterLayout(card, LinkButton("Ai deja un cont? Autentifica-te", ShowLogin));
    }

    private Control RegisterLayout(Control card, Control footerLink)
    {
        var center = new StackPanel
        {
            Width = 420,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 8,
            Children =
            {
                Text("Creare cont", TextBrush, 28, FontWeight.Bold, HorizontalAlignment.Center),
                Text("Alatura-te comunitatii Agenda!", Muted, 14, FontWeight.Normal, HorizontalAlignment.Center),
                card,
                new Border { Height = 22 },
                footerLink
            }
        };

        return new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("150,*"),
            RowDefinitions = new RowDefinitions("54,*"),
            Background = AppBackground,
            Children =
            {
                RegisterSidebar(),
                RegisterTopBar(),
                new Grid
                {
                    [Grid.ColumnProperty] = 1,
                    [Grid.RowProperty] = 1,
                    Children =
                    {
                        RegisterDecorations(),
                        center
                    }
                }
            }
        };
    }

    private Control AuthLayout(Control card)
    {
        return new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("220,*"),
            Background = AppBackground,
            Children =
            {
                SidebarPreview(),
                new Border
                {
                    [Grid.ColumnProperty] = 1,
                    Child = new Grid
                    {
                        Children =
                        {
                            ThemeButton(),
                            new Border
                            {
                                Width = 480,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center,
                                Child = card
                            }
                        }
                    }
                }
            }
        };
    }

    private Control SidebarPreview()
    {
        var panel = new StackPanel
        {
            Background = Surface,
            Spacing = 12,
            Margin = new Thickness(18, 24)
        };

        panel.Children.Add(Text("Agenda", TextBrush, 22, FontWeight.Bold));
        foreach (var item in new[] { "Dashboard", "Note", "Teme", "Orar" })
            panel.Children.Add(NavPreview(item));

        return panel;
    }

    private Control RegisterSidebar()
    {
        var panel = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*,Auto"),
            Background = Surface
        };

        var top = new StackPanel
        {
            Spacing = 14,
            Margin = new Thickness(16, 18, 12, 0),
            Children =
            {
                Text("Agenda", TextBrush, 15, FontWeight.Bold),
                NavPreview("Dashboard"),
                NavPreview("Note"),
                NavPreview("Teme"),
                NavPreview("Orar"),
                new Border { Height = 1, Background = CardBorder, Margin = new Thickness(0, 6, 0, 0) }
            }
        };
        panel.Children.Add(top);

        var bottom = new StackPanel
        {
            [Grid.RowProperty] = 2,
            Spacing = 10,
            Margin = new Thickness(16, 0, 12, 18),
            Children =
            {
                Text("Setari", Muted, 12),
                Text("Ajutor", Muted, 12)
            }
        };
        panel.Children.Add(bottom);
        return panel;
    }

    private Control RegisterTopBar()
    {
        var header = new Grid
        {
            [Grid.ColumnProperty] = 1,
            ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto,Auto"),
            Background = Surface,
            Margin = new Thickness(18, 0, 18, 0)
        };

        header.Children.Add(Text("Agenda", TextBrush, 18, FontWeight.Bold, HorizontalAlignment.Left));
        var notify = Text("🔔", Muted, 12);
        notify.SetValue(Grid.ColumnProperty, 1);
        notify.Margin = new Thickness(0, 18, 14, 0);
        header.Children.Add(notify);

        var theme = ThemeButton();
        theme.SetValue(Grid.ColumnProperty, 2);
        theme.Margin = new Thickness(0, 9, 14, 9);
        header.Children.Add(theme);

        var user = Text("Utilizator Nou\nGuest", SecondaryText, 11, FontWeight.Bold);
        user.SetValue(Grid.ColumnProperty, 3);
        user.Margin = new Thickness(0, 12, 0, 0);
        header.Children.Add(user);
        return header;
    }

    private Control RegisterDecorations()
    {
        var canvas = new Canvas { Opacity = _darkMode ? 0.18 : 0.35 };
        AddDecoration(canvas, "□", 110, 70, 34, "#9cc7ff");
        AddDecoration(canvas, "✎", 240, 50, 25, "#9cc7ff");
        AddDecoration(canvas, "▦", 70, 165, 31, "#9cc7ff");
        AddDecoration(canvas, "◷", 195, 220, 31, "#9cc7ff");
        AddDecoration(canvas, "▦", 735, 55, 33, "#9be1a8");
        AddDecoration(canvas, "✎", 855, 58, 25, "#9be1a8");
        AddDecoration(canvas, "□", 805, 180, 34, "#9be1a8");
        AddDecoration(canvas, "◷", 900, 205, 39, "#9be1a8");
        AddDecoration(canvas, "○", 145, 125, 16, "#9cc7ff");
        AddDecoration(canvas, "○", 875, 128, 16, "#9be1a8");
        return canvas;
    }

    private static void AddDecoration(Canvas canvas, string text, double left, double top, double size, string color)
    {
        var item = new TextBlock
        {
            Text = text,
            Foreground = Brush.Parse(color),
            FontSize = size,
            FontWeight = FontWeight.Normal
        };
        Canvas.SetLeft(item, left);
        Canvas.SetTop(item, top);
        canvas.Children.Add(item);
    }

    private void ShowShell(string page)
    {
        var user = AuthService.CurrentUser ?? throw new InvalidOperationException("Utilizator neautentificat.");
        Title = "Agenda";
        _navButtons.Clear();
        _currentPage = page;
        _pageTitle = Text(page == "Admin" ? "Panou Administrare" : page, TextBrush, 26, FontWeight.Bold);
        _content = new StackPanel { Spacing = 22, Margin = new Thickness(28) };

        var layout = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("220,*"),
            RowDefinitions = new RowDefinitions("72,*"),
            Background = AppBackground
        };

        var sidebar = BuildSidebar(user);
        sidebar.SetValue(Grid.RowSpanProperty, 2);
        layout.Children.Add(sidebar);

        var header = BuildHeader(user);
        header.SetValue(Grid.ColumnProperty, 1);
        layout.Children.Add(header);

        var scroll = new ScrollViewer
        {
            Content = _content,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        };
        scroll.SetValue(Grid.ColumnProperty, 1);
        scroll.SetValue(Grid.RowProperty, 1);
        layout.Children.Add(scroll);

        Content = layout;
        _ = Navigate(page);
    }

    private Control BuildSidebar(User user)
    {
        var sidebar = new StackPanel
        {
            Background = Surface,
            Spacing = 10,
            Margin = new Thickness(16, 24)
        };

        sidebar.Children.Add(Text("Agenda", TextBrush, 22, FontWeight.Bold));
        AddNav(sidebar, "Dashboard");
        AddNav(sidebar, "Note");
        AddNav(sidebar, "Teme");
        AddNav(sidebar, "Orar");
        if (user.Rol == "Admin")
            AddNav(sidebar, "Admin");

        sidebar.Children.Add(new Border { Height = 1, Margin = new Thickness(0, 12), Background = CardBorder });
        sidebar.Children.Add(Text("Setari", Muted, 14));
        sidebar.Children.Add(Text("Ajutor", Muted, 14));
        return sidebar;
    }

    private Control BuildHeader(User user)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto,Auto"),
            Background = Surface,
            Margin = new Thickness(28, 16)
        };

        grid.Children.Add(_pageTitle!);
        var account = Text($"{user.NumeComplet} ({user.Rol})", Muted, 14);
        account.SetValue(Grid.ColumnProperty, 1);
        account.Margin = new Thickness(0, 8, 18, 0);
        grid.Children.Add(account);

        var theme = ThemeButton();
        theme.SetValue(Grid.ColumnProperty, 2);
        theme.Margin = new Thickness(0, 0, 12, 0);
        grid.Children.Add(theme);

        var logout = SmallButton("Log out", Logout, Danger, Brushes.White);
        logout.SetValue(Grid.ColumnProperty, 3);
        grid.Children.Add(logout);
        return grid;
    }

    private void Logout()
    {
        AuthService.Logout();
        ShowLogin();
    }

    private void AddNav(StackPanel sidebar, string key)
    {
        var button = SmallButton(key, () => _ = Navigate(key), Brushes.Transparent, Muted);
        button.HorizontalAlignment = HorizontalAlignment.Stretch;
        button.HorizontalContentAlignment = HorizontalAlignment.Left;
        button.Padding = new Thickness(14, 10);
        _navButtons[key] = button;
        sidebar.Children.Add(button);
    }

    private async Task Navigate(string page)
    {
        if (_content is null || _pageTitle is null)
            return;

        _currentPage = page;
        foreach (var (key, button) in _navButtons)
        {
            var active = key == page;
            button.Background = active ? Brush.Parse("#e8f0fe") : Brushes.Transparent;
            button.Foreground = active ? Primary : Muted;
            button.FontWeight = active ? FontWeight.Bold : FontWeight.Normal;
        }

        _pageTitle.Text = page == "Admin" ? "Panou Administrare" : page;
        _content.Children.Clear();

        switch (page)
        {
            case "Note":
                await ShowNoteAsync();
                break;
            case "Teme":
                await ShowTemeAsync();
                break;
            case "Orar":
                await ShowOrarAsync();
                break;
            case "Admin":
                await ShowAdminAsync();
                break;
            default:
                await ShowDashboardAsync();
                break;
        }
    }

    private async Task ShowDashboardAsync()
    {
        var user = AuthService.CurrentUser!;
        var isAdmin = IsAdmin();
        using var db = new AppDbContext();

        _content!.Children.Add(Text($"Bine ai revenit, {user.NumeComplet}!", Muted, 15));

        var cards = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Stretch };
        var note = await db.Note.Where(n => isAdmin || n.UserId == user.Id).GroupBy(n => n.Materie)
            .Select(g => new Pair(g.Key, g.Average(n => n.Valoare).ToString("0.0"))).Take(3).ToListAsync();
        cards.Children.Add(SummaryCard("Note recente", note, Primary));

        var teme = await db.Teme.Where(t => (isAdmin || t.UserId == user.Id) && !t.Finalizata && t.Deadline >= DateTime.Today)
            .OrderBy(t => t.Deadline)
            .Take(3)
            .Select(t => new Pair(t.Titlu, t.Deadline == DateTime.Today ? "Azi" : $"{(t.Deadline - DateTime.Today).Days} zile"))
            .ToListAsync();
        cards.Children.Add(SummaryCard("Teme urgente", teme, Warning));

        var zi = ZiRomaneasca(DateTime.Today.DayOfWeek);
        var orar = (await db.OrarEntries.Where(o => (isAdmin || o.UserId == user.Id) && o.ZiSaptamana == zi).ToListAsync())
            .OrderBy(o => o.OraInceput)
            .Take(4)
            .Select(o => new Pair($"{o.OraInceput:hh\\:mm}-{o.OraSfarsit:hh\\:mm}", o.Materie))
            .ToList();
        cards.Children.Add(SummaryCard("Orarul de azi", orar, Muted));
        _content.Children.Add(cards);

        var activities = await db.Activitati.Where(a => isAdmin || a.UserId == user.Id).OrderByDescending(a => a.Timestamp).Take(6).ToListAsync();
        _content.Children.Add(ListCard("Activitate recenta", activities.Select(a => new[]
        {
            $"{IconFor(a.Tip)} {a.Descriere}",
            a.Timestamp.ToString("dd.MM.yyyy HH:mm")
        })));
    }

    private async Task ShowNoteAsync()
    {
        var canEdit = IsAdmin();
        if (canEdit)
            _content!.Children.Add(await NoteFormAsync());

        await LoadNoteListAsync(canEdit);
    }

    private async Task<Control> NoteFormAsync()
    {
        var elev = await ElevComboAsync();
        var materie = Input("Materie");
        var valoare = Input("Nota (1-10)");
        var descriere = Input("Descriere optionala");
        var message = Text("", Danger, 12);

        var form = FormCard("Adauga nota noua", message, elev, materie, valoare, descriere);
        form.Children.Add(ActionButton("Adauga", async () =>
        {
            if (!int.TryParse(valoare.Text, out var val) || val is < 1 or > 10 || string.IsNullOrWhiteSpace(materie.Text))
            {
                message.Text = "Completeaza materia si nota (1-10).";
                return;
            }

            if (SelectedElevId(elev) is not { } elevId)
            {
                message.Text = "Selecteaza elevul.";
                return;
            }

            using var db = new AppDbContext();
            db.Note.Add(new Nota { Materie = materie.Text!.Trim(), Valoare = val, Descriere = descriere.Text?.Trim(), UserId = elevId });
            db.Activitati.Add(new Activitate { Descriere = $"Adminul a adaugat o nota la {materie.Text!.Trim()}", Tip = "nota", UserId = elevId });
            await db.SaveChangesAsync();
            await Navigate("Note");
        }));

        return Card(form);
    }

    private async Task LoadNoteListAsync(bool canEdit)
    {
        using var db = new AppDbContext();
        var rows = canEdit
            ? await db.Note.Include(n => n.User).OrderByDescending(n => n.Data)
                .Select(n => new RowAction(n.Id, new[] { n.User!.NumeComplet, n.Materie, n.Valoare.ToString(), n.Data.ToString("dd.MM.yyyy"), n.Descriere ?? "" })).ToListAsync()
            : await db.Note.Where(n => n.UserId == AuthService.CurrentUser!.Id).OrderByDescending(n => n.Data)
                .Select(n => new RowAction(n.Id, new[] { n.Materie, n.Valoare.ToString(), n.Data.ToString("dd.MM.yyyy"), n.Descriere ?? "" })).ToListAsync();

        _content!.Children.Add(TableCard(canEdit ? new[] { "Elev", "Materie", "Nota", "Data", "Descriere" } : new[] { "Materie", "Nota", "Data", "Descriere" }, rows, canEdit ? DeleteNotaAsync : null));
    }

    private async Task DeleteNotaAsync(int id)
    {
        using var db = new AppDbContext();
        var nota = await db.Note.FindAsync(id);
        if (nota is not null)
        {
            db.Note.Remove(nota);
            await db.SaveChangesAsync();
        }
        await Navigate("Note");
    }

    private async Task ShowTemeAsync()
    {
        var canEdit = IsAdmin();
        if (canEdit)
            _content!.Children.Add(await TemaFormAsync());

        await LoadTemaListAsync(canEdit);
    }

    private async Task<Control> TemaFormAsync()
    {
        var elev = await ElevComboAsync();
        var titlu = Input("Titlu tema");
        var materie = Input("Materie");
        var deadline = Input(DateTime.Today.AddDays(7).ToString("yyyy-MM-dd"));
        var message = Text("", Danger, 12);

        var form = FormCard("Adauga tema noua", message, elev, titlu, materie, deadline);
        form.Children.Add(ActionButton("Adauga", async () =>
        {
            if (string.IsNullOrWhiteSpace(titlu.Text) || !DateTime.TryParse(deadline.Text, out var data))
            {
                message.Text = "Completeaza titlul si deadline-ul in format yyyy-MM-dd.";
                return;
            }

            if (SelectedElevId(elev) is not { } elevId)
            {
                message.Text = "Selecteaza elevul.";
                return;
            }

            using var db = new AppDbContext();
            db.Teme.Add(new Tema { Titlu = titlu.Text!.Trim(), Materie = materie.Text?.Trim() ?? "", Deadline = data.Date, UserId = elevId });
            db.Activitati.Add(new Activitate { Descriere = $"Adminul a adaugat tema \"{titlu.Text!.Trim()}\"", Tip = "tema", UserId = elevId });
            await db.SaveChangesAsync();
            await Navigate("Teme");
        }));

        return Card(form);
    }

    private async Task LoadTemaListAsync(bool canEdit)
    {
        using var db = new AppDbContext();
        var teme = canEdit
            ? await db.Teme.Include(t => t.User).OrderBy(t => t.Deadline).ToListAsync()
            : await db.Teme.Where(t => t.UserId == AuthService.CurrentUser!.Id).OrderBy(t => t.Deadline).ToListAsync();

        var rows = teme.Select(t => new RowAction(t.Id, canEdit
            ? new[] { t.User?.NumeComplet ?? "-", t.Titlu, t.Materie, t.Deadline.ToString("dd.MM.yyyy"), TemaStatus(t) }
            : new[] { t.Titlu, t.Materie, t.Deadline.ToString("dd.MM.yyyy"), TemaStatus(t) })).ToList();

        _content!.Children.Add(TableCard(canEdit ? new[] { "Elev", "Titlu", "Materie", "Deadline", "Status" } : new[] { "Titlu", "Materie", "Deadline", "Status" }, rows, canEdit ? DeleteTemaAsync : null, canEdit ? CompleteTemaAsync : null));
    }

    private async Task CompleteTemaAsync(int id)
    {
        using var db = new AppDbContext();
        var tema = await db.Teme.FindAsync(id);
        if (tema is not null)
        {
            tema.Finalizata = true;
            db.Activitati.Add(new Activitate { Descriere = $"Adminul a marcat tema \"{tema.Titlu}\" ca finalizata", Tip = "tema", UserId = tema.UserId });
            await db.SaveChangesAsync();
        }
        await Navigate("Teme");
    }

    private async Task DeleteTemaAsync(int id)
    {
        using var db = new AppDbContext();
        var tema = await db.Teme.FindAsync(id);
        if (tema is not null)
        {
            db.Teme.Remove(tema);
            await db.SaveChangesAsync();
        }
        await Navigate("Teme");
    }

    private async Task ShowOrarAsync()
    {
        var canEdit = IsAdmin();
        if (canEdit)
            _content!.Children.Add(await OrarFormAsync());

        await LoadOrarListAsync(canEdit);
    }

    private async Task<Control> OrarFormAsync()
    {
        var elev = await ElevComboAsync();
        var zi = new ComboBox { ItemsSource = Days, SelectedIndex = 0, MinWidth = 120 };
        var start = Input("08:00");
        var end = Input("09:00");
        var materie = Input("Materie");
        var profesor = Input("Profesor");
        var message = Text("", Danger, 12);

        var form = FormCard("Adauga ora noua", message, elev, zi, start, end, materie, profesor);
        form.Children.Add(ActionButton("Adauga", async () =>
        {
            if (!TimeSpan.TryParse(start.Text, out var oraStart) || !TimeSpan.TryParse(end.Text, out var oraEnd) || string.IsNullOrWhiteSpace(materie.Text))
            {
                message.Text = "Completeaza ora in format HH:MM si materia.";
                return;
            }

            if (SelectedElevId(elev) is not { } elevId)
            {
                message.Text = "Selecteaza elevul.";
                return;
            }

            using var db = new AppDbContext();
            db.OrarEntries.Add(new OrarEntry { ZiSaptamana = zi.SelectedItem?.ToString() ?? "Luni", OraInceput = oraStart, OraSfarsit = oraEnd, Materie = materie.Text!.Trim(), Profesor = profesor.Text?.Trim(), UserId = elevId });
            db.Activitati.Add(new Activitate { Descriere = "Adminul a actualizat orarul", Tip = "orar", UserId = elevId });
            await db.SaveChangesAsync();
            await Navigate("Orar");
        }));

        return Card(form);
    }

    private async Task LoadOrarListAsync(bool canEdit)
    {
        using var db = new AppDbContext();
        var query = canEdit ? db.OrarEntries.Include(o => o.User) : db.OrarEntries.Where(o => o.UserId == AuthService.CurrentUser!.Id).Include(o => o.User);
        var items = await query.ToListAsync();
        var rows = items.OrderBy(o => IndexZi(o.ZiSaptamana)).ThenBy(o => o.OraInceput)
            .Select(o => new RowAction(o.Id, canEdit
                ? new[] { o.User?.NumeComplet ?? "-", o.ZiSaptamana, $"{o.OraInceput:hh\\:mm} - {o.OraSfarsit:hh\\:mm}", o.Materie, o.Profesor ?? "" }
                : new[] { o.ZiSaptamana, $"{o.OraInceput:hh\\:mm} - {o.OraSfarsit:hh\\:mm}", o.Materie, o.Profesor ?? "" }))
            .ToList();

        _content!.Children.Add(TableCard(canEdit ? new[] { "Elev", "Zi", "Interval", "Materie", "Profesor" } : new[] { "Zi", "Interval", "Materie", "Profesor" }, rows, canEdit ? DeleteOrarAsync : null));
    }

    private async Task DeleteOrarAsync(int id)
    {
        using var db = new AppDbContext();
        var item = await db.OrarEntries.FindAsync(id);
        if (item is not null)
        {
            db.OrarEntries.Remove(item);
            await db.SaveChangesAsync();
        }
        await Navigate("Orar");
    }

    private async Task ShowAdminAsync()
    {
        if (!IsAdmin())
        {
            await ShowDashboardAsync();
            return;
        }

        using var db = new AppDbContext();
        var users = await db.Users.OrderBy(u => u.NumeComplet).ToListAsync();
        var stats = new WrapPanel();
        stats.Children.Add(StatCard(users.Count.ToString(), "Utilizatori activi", Primary));
        stats.Children.Add(StatCard((await db.Note.Select(n => n.Materie).Distinct().CountAsync()).ToString(), "Materii", Success));
        stats.Children.Add(StatCard((await db.Activitati.CountAsync()).ToString(), "Activitati", Warning));
        stats.Children.Add(StatCard($"{Math.Min(94, users.Count * 3)}%", "Rata utilizare", Purple));
        _content!.Children.Add(stats);

        var rows = users.Select(u => new RowAction(u.Id, new[] { u.NumeComplet, u.Email, u.Rol, u.DataInregistrare.ToString("dd.MM.yyyy") })).ToList();
        _content.Children.Add(TableCard(new[] { "Nume", "Email", "Rol", "Inregistrat" }, rows, DeleteUserAsync));
    }

    private async Task DeleteUserAsync(int id)
    {
        if (id == AuthService.CurrentUser!.Id)
            return;

        using var db = new AppDbContext();
        var user = await db.Users.FindAsync(id);
        if (user is not null)
        {
            db.Users.Remove(user);
            await db.SaveChangesAsync();
        }
        await Navigate("Admin");
    }

    private Border TableCard(string[] headers, List<RowAction> rows, Func<int, Task>? delete = null, Func<int, Task>? complete = null)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions(string.Join(",", headers.Select(_ => "*").Concat(delete is null && complete is null ? Array.Empty<string>() : new[] { "Auto" }))),
            RowDefinitions = new RowDefinitions("Auto"),
            Margin = new Thickness(0)
        };

        for (var i = 0; i < headers.Length; i++)
            grid.Children.Add(Cell(headers[i], 0, i, true));

        var rowIndex = 1;
        foreach (var row in rows)
        {
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            for (var i = 0; i < row.Values.Length; i++)
                grid.Children.Add(Cell(row.Values[i], rowIndex, i));

            if (delete is not null || complete is not null)
            {
                var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
                if (complete is not null)
                    actions.Children.Add(SmallButton("Finalizat", () => _ = complete(row.Id), Success, Brushes.White));
                if (delete is not null)
                    actions.Children.Add(SmallButton("Sterge", () => _ = delete(row.Id), Danger, Brushes.White));
                actions.SetValue(Grid.RowProperty, rowIndex);
                actions.SetValue(Grid.ColumnProperty, headers.Length);
                actions.Margin = new Thickness(8);
                grid.Children.Add(actions);
            }

            rowIndex++;
        }

        if (rows.Count == 0)
        {
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            var empty = Text("Nu exista date.", Muted, 14);
            empty.Margin = new Thickness(10);
            empty.SetValue(Grid.RowProperty, 1);
            empty.SetValue(Grid.ColumnSpanProperty, headers.Length + 1);
            grid.Children.Add(empty);
        }

        return Card(grid);
    }

    private static Control Cell(string value, int row, int column, bool header = false)
    {
        var border = new Border
        {
            BorderBrush = CardBorder,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(10),
            Child = Text(value, header ? TextBrush : SecondaryText, 13, header ? FontWeight.Bold : FontWeight.Normal)
        };
        border.SetValue(Grid.RowProperty, row);
        border.SetValue(Grid.ColumnProperty, column);
        return border;
    }

    private Border SummaryCard(string title, IEnumerable<Pair> rows, IBrush badgeBrush)
    {
        var panel = new StackPanel { Spacing = 10 };
        panel.Children.Add(Text(title, TextBrush, 16, FontWeight.Bold));
        var any = false;
        foreach (var row in rows)
        {
            any = true;
            panel.Children.Add(new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                Children =
                {
                    Text(row.Left, TextBrush, 13),
                    new Border
                    {
                        [Grid.ColumnProperty] = 1,
                        Background = badgeBrush,
                        CornerRadius = new CornerRadius(5),
                        Padding = new Thickness(10, 4),
                        Child = Text(row.Right, Brushes.White, 12, FontWeight.Bold)
                    }
                }
            });
        }

        if (!any)
            panel.Children.Add(Text("Nu exista date.", Muted, 13));

        var card = Card(panel);
        card.Width = 270;
        card.Margin = new Thickness(0, 0, 18, 18);
        return card;
    }

    private Border ListCard(string title, IEnumerable<string[]> rows)
    {
        var panel = new StackPanel { Spacing = 10 };
        panel.Children.Add(Text(title, TextBrush, 16, FontWeight.Bold));
        var any = false;
        foreach (var row in rows)
        {
            any = true;
            panel.Children.Add(new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                Children =
                {
                    Text(row[0], TextBrush, 13),
                    new TextBlock
                    {
                        [Grid.ColumnProperty] = 1,
                        Text = row[1],
                        Foreground = Muted,
                        FontSize = 12,
                        Margin = new Thickness(12, 0, 0, 0)
                    }
                }
            });
        }

        if (!any)
            panel.Children.Add(Text("Nu exista activitate recenta.", Muted, 13));

        return Card(panel);
    }

    private Border StatCard(string value, string label, IBrush brush)
    {
        var panel = new StackPanel { Spacing = 6 };
        panel.Children.Add(Text(value, Brushes.White, 28, FontWeight.Bold));
        panel.Children.Add(Text(label, Brushes.White, 13));

        return new Border
        {
            Width = 200,
            Margin = new Thickness(0, 0, 16, 16),
            Padding = new Thickness(18),
            CornerRadius = new CornerRadius(8),
            Background = brush,
            Child = panel
        };
    }

    private static StackPanel FormCard(string title, TextBlock message, params Control[] controls)
    {
        var wrap = new WrapPanel { VerticalAlignment = VerticalAlignment.Center };
        foreach (var control in controls)
        {
            control.Margin = new Thickness(0, 0, 12, 12);
            wrap.Children.Add(control);
        }

        return new StackPanel
        {
            Spacing = 10,
            Children =
            {
                Text(title, TextBrush, 16, FontWeight.Bold),
                message,
                wrap
            }
        };
    }

    private async Task<ComboBox> ElevComboAsync()
    {
        using var db = new AppDbContext();
        var elevi = await db.Users
            .Where(u => u.Rol != "Admin")
            .OrderBy(u => u.NumeComplet)
            .Select(u => new ElevOption(u.Id, $"{u.NumeComplet} ({u.Email})"))
            .ToListAsync();

        return new ComboBox
        {
            ItemsSource = elevi,
            SelectedIndex = elevi.Count > 0 ? 0 : -1,
            MinWidth = 220
        };
    }

    private static int? SelectedElevId(ComboBox elev)
        => elev.SelectedItem is ElevOption option ? option.Id : null;

    private static TextBox Input(string watermark, bool password = false)
    {
        return new TextBox
        {
            PlaceholderText = watermark,
            PasswordChar = password ? '*' : '\0',
            MinWidth = 180,
            Height = 38
        };
    }

    private static StackPanel Field(string label, TextBox input)
    {
        return new StackPanel
        {
            Spacing = 5,
            Children =
            {
                Text(label, Muted, 13, FontWeight.Bold),
                input
            }
        };
    }

    private static Control RegisterField(string label, string icon, TextBox input, bool showCheck = false)
    {
        input.Height = 34;
        input.MinWidth = 0;
        input.Background = Brush.Parse(_darkMode ? "#111827" : "#eef2f7");
        input.BorderBrush = CardBorder;
        input.Foreground = TextBrush;

        var field = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,Auto"),
            Children =
            {
                Text(label, SecondaryText, 12, FontWeight.Bold)
            }
        };

        var box = new Grid
        {
            [Grid.RowProperty] = 1,
            Height = 36,
            Background = input.Background,
            Margin = new Thickness(0, 3, 0, 0)
        };

        var iconText = Text(icon, Muted, 11, FontWeight.Normal, HorizontalAlignment.Center);
        var checkText = showCheck
            ? Text("✓", Success, 13, FontWeight.Bold, HorizontalAlignment.Center)
            : Text("", Success, 11);
        input.SetValue(Grid.ColumnProperty, 1);
        checkText.SetValue(Grid.ColumnProperty, 2);

        box.Children.Add(new Border
        {
            BorderBrush = CardBorder,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Child = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("34,*,28"),
                Children =
                {
                    iconText,
                    input,
                    checkText
                }
            }
        });

        field.Children.Add(box);
        return field;
    }

    private static Border RegisterCard(Control form)
    {
        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("26,*"),
            Children =
            {
                new Border
                {
                    [Grid.RowProperty] = 1,
                    Background = Surface,
                    BorderBrush = Brush.Parse(_darkMode ? "#4ade80" : "#9fd7b2"),
                    BorderThickness = new Thickness(2),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(14, 28, 14, 12),
                    Child = form
                },
                new Border
                {
                    Width = 58,
                    Height = 58,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Background = Brush.Parse(_darkMode ? "#123524" : "#e7f7ea"),
                    BorderBrush = Brush.Parse(_darkMode ? "#4ade80" : "#66b67b"),
                    BorderThickness = new Thickness(2),
                    CornerRadius = new CornerRadius(29),
                    Child = Text("✓", Success, 22, FontWeight.Bold, HorizontalAlignment.Center)
                }
            }
        };

        return new Border
        {
            Width = 360,
            Background = Brushes.Transparent,
            Child = grid
        };
    }

    private static Button GoogleButton(string text)
    {
        var button = SmallButton(text, () => { }, Brush.Parse(_darkMode ? "#111827" : "#eef4ff"), TextBrush);
        button.BorderBrush = CardBorder;
        button.HorizontalAlignment = HorizontalAlignment.Stretch;
        button.HorizontalContentAlignment = HorizontalAlignment.Center;
        return button;
    }

    private static Button ActionButton(string text, Func<Task> action)
    {
        var button = SmallButton(text, () => _ = action(), Primary, Brushes.White);
        button.HorizontalAlignment = HorizontalAlignment.Stretch;
        button.HorizontalContentAlignment = HorizontalAlignment.Center;
        return button;
    }

    private static Button LinkButton(string text, Action action)
        => SmallButton(text, action, Brushes.Transparent, Primary);

    private static Button NavPreview(string text)
        => SmallButton(text, () => { }, Brushes.Transparent, Muted);

    private Button ThemeButton()
    {
        var button = SmallButton(_darkMode ? "Light" : "Dark", ToggleTheme, Surface, TextBrush);
        button.BorderBrush = CardBorder;
        button.HorizontalAlignment = HorizontalAlignment.Right;
        button.VerticalAlignment = VerticalAlignment.Top;
        button.Margin = new Thickness(0, 18, 24, 0);
        return button;
    }

    private void ToggleTheme()
    {
        _darkMode = !_darkMode;
        if (Application.Current is not null)
            Application.Current.RequestedThemeVariant = _darkMode ? ThemeVariant.Dark : ThemeVariant.Light;

        if (AuthService.CurrentUser is null)
        {
            if (_registerView) ShowRegister();
            else ShowLogin();
            return;
        }

        ShowShell(_currentPage);
    }

    private static Button SmallButton(string text, Action action, IBrush background, IBrush foreground)
    {
        var button = new Button
        {
            Content = text,
            Background = background,
            Foreground = foreground,
            BorderBrush = background == Brushes.Transparent ? Brushes.Transparent : background,
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(14, 8),
            FontWeight = FontWeight.Bold
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static Border Card(Control child)
    {
        return new Border
        {
            Background = Surface,
            BorderBrush = CardBorder,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(18),
            Child = child
        };
    }

    private static Border Card(double width)
    {
        return new Border
        {
            Width = width,
            Background = Surface,
            BorderBrush = CardBorder,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(26)
        };
    }

    private static TextBlock Text(string text, IBrush color, double size, FontWeight weight = FontWeight.Normal, HorizontalAlignment alignment = HorizontalAlignment.Left)
    {
        return new TextBlock
        {
            Text = text,
            Foreground = color,
            FontSize = size,
            FontWeight = weight,
            HorizontalAlignment = alignment,
            TextWrapping = TextWrapping.Wrap
        };
    }

    private static Control CenterMessage(string message)
    {
        return new Grid
        {
            Background = AppBackground,
            Children =
            {
                Text(message, Muted, 18, FontWeight.Bold, HorizontalAlignment.Center)
            }
        };
    }

    private static string TemaStatus(Tema t)
        => t.Finalizata ? "Finalizata" : t.Deadline < DateTime.Today ? "Expirata" : "In asteptare";

    private static bool IsAdmin() => AuthService.CurrentUser?.Rol == "Admin";

    private static string IconFor(string tip) => tip switch
    {
        "nota" => "Note:",
        "tema" => "Teme:",
        "orar" => "Orar:",
        _ => "-"
    };

    private static int IndexZi(string zi) => Array.IndexOf(Days, zi);

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

    private static readonly string[] Days = { "Luni", "Marti", "Miercuri", "Joi", "Vineri", "Sambata", "Duminica" };

    private sealed record ElevOption(int Id, string Text)
    {
        public override string ToString() => Text;
    }

    private sealed record Pair(string Left, string Right);
    private sealed record RowAction(int Id, string[] Values);
}
