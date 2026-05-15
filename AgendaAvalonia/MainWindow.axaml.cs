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

        var form = new StackPanel
        {
            Spacing = 9,
            Children =
            {
                message,
                RegisterField("Adresa de e-mail sau Utilizator", "@", email),
                RegisterField("Parola", IconGlyph("Parola"), password),
                ActionButton("Autentifica-te", async () =>
                {
                    var result = await AuthService.LoginAsync(email.Text ?? "", password.Text ?? "");
                    if (!result.Success)
                    {
                        message.Text = result.Message;
                        return;
                    }

                    ShowShell(IsAdmin(AuthService.CurrentUser) ? "Admin" : "Dashboard");
                }),
                GoogleButton("Autentificare rapida cu Google")
            }
        };

        var card = AuthCard(form);
        Content = AuthPageLayout(
            "Autentificare",
            "Bine ai revenit la Agenda!",
            card,
            LinkButton("Nu ai cont? Creeaza unul", ShowRegister),
            "Utilizator\nGuest");
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
                RegisterField("Nume complet", IconGlyph("User"), name),
                RegisterField("Adresa de e-mail sau Utilizator", "@", email),
                RegisterField("Parola", IconGlyph("Parola"), password),
                RegisterField("Confirma parola", IconGlyph("Parola"), confirm),
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

        var card = AuthCard(form);
        Content = AuthPageLayout(
            "Creare cont",
            "Alatura-te comunitatii Agenda!",
            card,
            LinkButton("Ai deja un cont? Autentifica-te", ShowLogin),
            "Utilizator Nou\nGuest");
    }

    private Control AuthPageLayout(Control card, Control footerLink, string userText)
        => AuthPageLayout("Autentificare", "Bine ai revenit la Agenda!", card, footerLink, userText);

    private Control AuthPageLayout(string title, string subtitle, Control card, Control footerLink, string userText)
    {
        var center = new StackPanel
        {
            Width = 420,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 9,
            Children =
            {
                Text(title, TextBrush, 28, FontWeight.Bold, HorizontalAlignment.Center),
                Text(subtitle, Muted, 14, FontWeight.Normal, HorizontalAlignment.Center),
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
                RegisterTopBar(userText),
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

    private Control RegisterSidebar()
    {
        var panel = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*,Auto"),
            Background = Surface
        };

        var top = new StackPanel
        {
            Spacing = 12,
            Margin = new Thickness(16, 18, 12, 0),
            Children =
            {
                BrandTitle(15),
                StudentSidebarButton("Dashboard", "Dashboard", false, () => { }),
                StudentSidebarButton("Note", "Note", false, () => { }),
                StudentSidebarButton("Teme", "Teme", false, () => { }),
                StudentSidebarButton("Orar", "Orar", false, () => { }),
                new Border { Height = 1, Background = CardBorder, Margin = new Thickness(0, 6, 0, 0) }
            }
        };
        panel.Children.Add(top);

        return panel;
    }

    private Control RegisterTopBar(string userText)
    {
        var header = new Grid
        {
            [Grid.ColumnProperty] = 1,
            ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto,Auto"),
            Background = Surface,
            Margin = new Thickness(18, 0, 18, 0)
        };

        header.Children.Add(BrandTitle(18));
        var notify = IconText("Notificari", 16, Muted);
        notify.SetValue(Grid.ColumnProperty, 1);
        notify.Margin = new Thickness(0, 18, 14, 0);
        header.Children.Add(notify);

        var theme = ThemeButton();
        theme.SetValue(Grid.ColumnProperty, 2);
        theme.Margin = new Thickness(0, 9, 14, 9);
        header.Children.Add(theme);

        var user = Text(userText, SecondaryText, 11, FontWeight.Bold);
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
        _pageTitle = Text(PageTitle(page), TextBrush, 26, FontWeight.Bold);
        _content = new StackPanel { Spacing = 22, Margin = new Thickness(36, 24, 36, 28) };

        var layout = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("220,*"),
            RowDefinitions = new RowDefinitions(IsAdmin(user) && page == "Admin" ? "72,*" : "88,*"),
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
        _ = NavigateSafely(page);
    }

    private Control BuildSidebar(User user)
    {
        if (IsAdmin(user) && _currentPage == "Admin")
            return BuildAdminSidebar();

        var sidebar = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*,Auto"),
            Width = 220,
            Background = Surface,
        };

        var top = new StackPanel
        {
            Spacing = 9,
            Margin = new Thickness(14, 24, 14, 0)
        };
        top.Children.Add(BrandTitle(22));
        AddNav(top, "Dashboard");
        AddNav(top, "Note");
        AddNav(top, "Teme");
        AddNav(top, "Orar");
        if (IsAdmin(user))
            AddNav(top, "Admin");
        top.Children.Add(new Border { Height = 1, Margin = new Thickness(0, 14, 0, 8), Background = CardBorder });
        sidebar.Children.Add(top);
        return sidebar;
    }

    private Control BuildAdminSidebar()
    {
        var sidebar = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*,Auto"),
            Width = 220,
            Background = Surface
        };

        var top = new StackPanel
        {
            Spacing = 8,
            Margin = new Thickness(14, 22, 14, 0)
        };
        top.Children.Add(BrandTitle("Admin", "Admin", 20));
        AddAdminNav(top, "Admin", "Dashboard", "Dashboard");
        AddAdminNav(top, "Utilizatori", "Utilizatori", "User");
        AddAdminNav(top, "Materii", "Materii", "Agenda");
        AddAdminNav(top, "AdminOrar", "Orar & Calendar", "Orar");
        AddAdminNav(top, "Rapoarte", "Rapoarte", "Rapoarte");
        AddAdminNav(top, "Aprobari", "Aprobari", "Aprobari");
        top.Children.Add(new Border { Height = 1, Background = CardBorder, Margin = new Thickness(0, 12, 0, 8) });
        sidebar.Children.Add(top);

        return sidebar;
    }

    private void AddAdminNav(StackPanel sidebar, string key, string text, string icon)
    {
        var button = AdminSidebarButton(text, icon, key == _currentPage, () => _ = NavigateSafely(key));
        _navButtons[key] = button;
        sidebar.Children.Add(button);
    }

    private Control BuildHeader(User user)
    {
        if (IsAdmin(user) && IsAdminPage(_currentPage))
            return BuildAdminHeader(user);

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto,Auto,Auto"),
            Background = Surface,
            Margin = new Thickness(36, 22, 24, 12)
        };

        grid.Children.Add(new StackPanel
        {
            Spacing = 2,
            Children =
            {
                _pageTitle!,
                Text("Bine ai revenit!", Muted, 12)
            }
        });

        var bell = NotificationButton("2");
        bell.SetValue(Grid.ColumnProperty, 1);
        bell.Margin = new Thickness(0, 0, 14, 0);
        grid.Children.Add(bell);

        var theme = HeaderActionButton(_darkMode ? "Light" : "Dark", ToggleTheme, Brush.Parse(_darkMode ? "#111827" : "#f8fafc"), TextBrush);
        theme.SetValue(Grid.ColumnProperty, 2);
        theme.Margin = new Thickness(0, 0, 12, 0);
        grid.Children.Add(theme);

        var logout = HeaderActionButton("Log out", Logout, Brush.Parse("#feecec"), Danger);
        logout.SetValue(Grid.ColumnProperty, 3);
        logout.Margin = new Thickness(0, 0, 24, 0);
        grid.Children.Add(logout);

        var profile = ProfileBlock(user, false);
        profile.SetValue(Grid.ColumnProperty, 4);
        grid.Children.Add(profile);
        return grid;
    }

    private Control BuildAdminHeader(User user)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto,Auto,Auto"),
            Background = Surface,
            Margin = new Thickness(36, 20, 16, 14)
        };

        grid.Children.Add(new StackPanel
        {
            Spacing = 2,
            Children =
            {
                Text(PageTitle(_currentPage), TextBrush, 24, FontWeight.Bold),
                Text(AdminPageSubtitle(_currentPage), Muted, 11)
            }
        });

        var bell = new Border
        {
            [Grid.ColumnProperty] = 1,
            Width = 36,
            Height = 36,
            CornerRadius = new CornerRadius(18),
            Background = Brush.Parse(_darkMode ? "#111827" : "#f8fafc"),
            BorderBrush = CardBorder,
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 0, 14, 0),
            Child = new Grid
            {
                Children =
                {
                    IconText("Notificari", 15, Muted),
                    new Border
                    {
                        Width = 13,
                        Height = 13,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Top,
                        Background = Brush.Parse("#ff3b30"),
                        CornerRadius = new CornerRadius(7),
                        Child = Text("3", Brushes.White, 8, FontWeight.Bold, HorizontalAlignment.Center)
                    }
                }
            }
        };
        grid.Children.Add(bell);

        var theme = HeaderActionButton(_darkMode ? "Light" : "Dark", ToggleTheme, Brush.Parse(_darkMode ? "#111827" : "#f8fafc"), TextBrush);
        theme.SetValue(Grid.ColumnProperty, 2);
        theme.Margin = new Thickness(0, 0, 12, 0);
        grid.Children.Add(theme);

        var logout = HeaderActionButton("Log out", Logout, Brush.Parse("#feecec"), Danger);
        logout.SetValue(Grid.ColumnProperty, 3);
        logout.Margin = new Thickness(0, 0, 22, 0);
        grid.Children.Add(logout);

        var profile = ProfileBlock(user, true);
        profile.SetValue(Grid.ColumnProperty, 4);
        grid.Children.Add(profile);
        return grid;
    }

    private void Logout()
    {
        AuthService.Logout();
        ShowLogin();
    }

    private void AddNav(StackPanel sidebar, string key)
    {
        var button = StudentSidebarButton(key, IconKeyForPage(key), false, () => _ = NavigateSafely(key));
        button.HorizontalAlignment = HorizontalAlignment.Stretch;
        button.HorizontalContentAlignment = HorizontalAlignment.Left;
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
            var activeBrush = IsAdminPage(page) ? Brush.Parse("#a700ff") : Brush.Parse("#1f5cff");
            button.Background = active ? activeBrush : Brushes.Transparent;
            button.Foreground = active ? Brushes.White : Muted;
            button.FontWeight = active ? FontWeight.Bold : FontWeight.Normal;
            SetButtonContentForeground(button, active ? Brushes.White : Muted);
        }

        _pageTitle.Text = PageTitle(page);
        _content.Children.Clear();
        _content.Spacing = 22;

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
            case "AdminOrar":
                await ShowAdminOrarAsync();
                break;
            case "Rapoarte":
                await ShowRapoarteAsync();
                break;
            case "Aprobari":
                await ShowAprobariAsync();
                break;
            case "SetariSistem":
                await ShowSetariSistemAsync();
                break;
            case "Suport":
                await ShowSuportAsync();
                break;
            case "Utilizatori":
                await ShowUtilizatoriAsync();
                break;
            case "Materii":
                await ShowMateriiAsync();
                break;
            case "Admin":
                await ShowAdminAsync();
                break;
            default:
                await ShowDashboardAsync();
                break;
        }
    }

    private async Task NavigateSafely(string page)
    {
        try
        {
            await Navigate(page);
        }
        catch (Exception ex)
        {
            ShowPageError(ex);
        }
    }

    private void ShowPageError(Exception ex)
    {
        if (_content is null)
            return;

        _content.Children.Clear();
        _content.Children.Add(new Border
        {
            Background = Surface,
            BorderBrush = Danger,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(24),
            Child = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    Text("Nu s-a putut incarca pagina.", Danger, 18, FontWeight.Bold),
                    Text(ex.Message, TextBrush, 13)
                }
            }
        });
    }

    private async Task ShowDashboardAsync()
    {
        var user = AuthService.CurrentUser!;
        var isAdmin = IsAdmin();
        using var db = new AppDbContext();

        var note = await db.Note.Where(n => isAdmin || n.UserId == user.Id).GroupBy(n => n.Materie)
            .Select(g => new Pair(g.Key, g.Average(n => n.Valoare).ToString("0.0"))).Take(3).ToListAsync();

        var temeList = await db.Teme.Where(t => (isAdmin || t.UserId == user.Id) && !t.Finalizata && t.Deadline >= DateTime.Today)
            .OrderBy(t => t.Deadline)
            .Take(3)
            .ToListAsync();
        var teme = temeList
            .Select(t => new Pair(t.Titlu, t.Deadline == DateTime.Today ? "Azi" : t.Deadline == DateTime.Today.AddDays(1) ? "Maine" : $"{(t.Deadline - DateTime.Today).Days} zile"))
            .ToList();

        var zi = ZiRomaneasca(DateTime.Today.DayOfWeek);
        var orar = (await db.OrarEntries.Where(o => isAdmin || AppDbContext.UsesMySql || o.UserId == user.Id).ToListAsync())
            .Where(o => ZiEquals(o.ZiSaptamana, zi))
            .OrderBy(o => o.OraInceput)
            .Take(3)
            .Select(o => new Pair($"{o.OraInceput:hh\\:mm} - {o.OraSfarsit:hh\\:mm}", o.Materie))
            .ToList();

        var activities = await db.Activitati.Where(a => isAdmin || a.UserId == user.Id).OrderByDescending(a => a.Timestamp).Take(6).ToListAsync();

        var cards = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*"),
            ColumnSpacing = 28
        };
        cards.Children.Add(StudentSummaryCard("Note recente", "Note", note, "Vezi toate notele", Primary, 0));
        cards.Children.Add(StudentSummaryCard("Teme urgente", "Teme", teme, "Vezi toate temele", Warning, 1));
        cards.Children.Add(StudentSummaryCard("Orarul de azi", "Orar", orar, "Vezi orarul complet", Success, 2));

        _content!.Children.Add(cards);
        _content.Children.Add(StudentActivityCard(activities));
    }

    private async Task ShowNoteAsync()
    {
        var canEdit = IsAdmin();
        var user = AuthService.CurrentUser!;
        using var db = new AppDbContext();
        var notes = canEdit
            ? await db.Note.Include(n => n.User).OrderByDescending(n => n.Data).ToListAsync()
            : await db.Note.Where(n => n.UserId == user.Id).Include(n => n.User).OrderByDescending(n => n.Data).ToListAsync();

        _content!.Spacing = 28;
        _content.Children.Add(new StackPanel
        {
            Spacing = 6,
            Children =
            {
                Text("Note", TextBrush, 28, FontWeight.Bold),
                Text("Vizualizeaza si urmareste-ti evolutia academica", Muted, 14)
            }
        });

        _content.Children.Add(NotesStatsGrid(notes));

        var details = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("2.2*,1*"),
            ColumnSpacing = 28
        };
        details.Children.Add(RecentNotesPanel(notes));
        var averages = SubjectAveragesPanel(notes);
        averages.SetValue(Grid.ColumnProperty, 1);
        details.Children.Add(averages);
        _content.Children.Add(details);

        if (canEdit)
        {
            _content!.Children.Add(await NoteFormAsync());
            await LoadNoteListAsync(canEdit);
        }
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

    private static Grid NotesStatsGrid(List<Nota> notes)
    {
        var average = notes.Count == 0 ? 0 : notes.Average(n => n.Valoare);
        var aboveNine = notes.Count(n => n.Valoare >= 9);
        var thisMonth = notes.Count(n => n.Data.Year == DateTime.Today.Year && n.Data.Month == DateTime.Today.Month);
        var bestSubject = notes
            .GroupBy(n => n.Materie)
            .Select(g => new { Materie = g.Key, Media = g.Average(n => n.Valoare) })
            .OrderByDescending(g => g.Media)
            .FirstOrDefault()?.Materie ?? "-";

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*,*"),
            ColumnSpacing = 24
        };
        grid.Children.Add(GradeStatCard("Media generala", notes.Count == 0 ? "-" : average.ToString("0.0"), "Note", Primary, true, 0));
        grid.Children.Add(GradeStatCard("Note peste 9", aboveNine.ToString(), "Activitate", Success, false, 1));
        grid.Children.Add(GradeStatCard("Note luna aceasta", thisMonth.ToString(), "Orar", Warning, false, 2));
        grid.Children.Add(GradeStatCard("Cea mai buna materie", bestSubject, "Note", Purple, false, 3));
        return grid;
    }

    private static Border GradeStatCard(string label, string value, string icon, IBrush accent, bool primary, int column)
    {
        return new Border
        {
            [Grid.ColumnProperty] = column,
            Background = primary ? Primary : Surface,
            BorderBrush = primary ? Primary : CardBorder,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(22),
            MinHeight = 148,
            BoxShadow = primary ? SoftShadow : default,
            Child = new Grid
            {
                RowDefinitions = new RowDefinitions("Auto,*,Auto"),
                Children =
                {
                    new Grid
                    {
                        ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                        Children =
                        {
                            IconText(icon, 22, primary ? Brushes.White : accent),
                            new TextBlock
                            {
                                [Grid.ColumnProperty] = 1,
                                Text = IconGlyph("Activitate"),
                                FontFamily = IconFont,
                                Foreground = primary ? Brushes.White : accent,
                                FontSize = 16
                            }
                        }
                    },
                    new TextBlock
                    {
                        [Grid.RowProperty] = 1,
                        Text = value,
                        Foreground = primary ? Brushes.White : TextBrush,
                        FontSize = value.Length > 8 ? 26 : 30,
                        FontWeight = FontWeight.Bold,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        TextWrapping = TextWrapping.Wrap
                    },
                    new TextBlock
                    {
                        [Grid.RowProperty] = 2,
                        Text = label,
                        Foreground = primary ? Brushes.White : Muted,
                        FontSize = 12
                    }
                }
            }
        };
    }

    private static Border RecentNotesPanel(List<Nota> notes)
    {
        var panel = new StackPanel { Spacing = 22 };
        panel.Children.Add(new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto"),
            Children =
            {
                Text("Note recente", TextBrush, 18, FontWeight.Bold),
                new TextBlock
                {
                    [Grid.ColumnProperty] = 1,
                    Text = IconGlyph("Cautare"),
                    FontFamily = IconFont,
                    Foreground = Muted,
                    FontSize = 18
                },
                new TextBlock
                {
                    [Grid.ColumnProperty] = 2,
                    Text = IconGlyph("Filtru"),
                    FontFamily = IconFont,
                    Foreground = Muted,
                    FontSize = 18,
                    Margin = new Thickness(18, 0, 0, 0)
                }
            }
        });

        foreach (var note in notes.Take(8))
            panel.Children.Add(RecentNoteRow(note));

        if (notes.Count == 0)
            panel.Children.Add(Text("Nu exista note.", Muted, 13));

        return new Border
        {
            Background = Surface,
            BorderBrush = CardBorder,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(24, 26),
            MinHeight = 430,
            Child = panel
        };
    }

    private static Control RecentNoteRow(Nota note)
    {
        var good = note.Valoare >= 9;
        return new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("68,*,Auto,Auto"),
            Margin = new Thickness(16, 2),
            Children =
            {
                new Border
                {
                    Width = 52,
                    Height = 52,
                    CornerRadius = new CornerRadius(11),
                    Background = Brush.Parse(good ? "#dcfce7" : "#dbeafe"),
                    Child = Text(note.Valoare.ToString(), good ? Success : Primary, 18, FontWeight.Bold, HorizontalAlignment.Center)
                },
                new StackPanel
                {
                    [Grid.ColumnProperty] = 1,
                    VerticalAlignment = VerticalAlignment.Center,
                    Spacing = 3,
                    Children =
                    {
                        Text(note.Materie, TextBrush, 15, FontWeight.Bold),
                        Text(NoteDescription(note), Muted, 12)
                    }
                },
                new TextBlock
                {
                    [Grid.ColumnProperty] = 2,
                    Text = note.Data.ToString("yyyy-MM-dd"),
                    Foreground = Muted,
                    FontSize = 12,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(16, 0, 18, 0)
                },
                new TextBlock
                {
                    [Grid.ColumnProperty] = 3,
                    Text = IconGlyph(good ? "TrendUp" : "TrendDown"),
                    FontFamily = IconFont,
                    Foreground = good ? Success : Danger,
                    FontSize = 17,
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
    }

    private static Border SubjectAveragesPanel(List<Nota> notes)
    {
        var panel = new StackPanel { Spacing = 18 };
        panel.Children.Add(Text("Medii pe materii", TextBrush, 18, FontWeight.Bold));

        var colors = new[] { Primary, Purple, Success, Warning, Brush.Parse("#ec2f91"), Brush.Parse("#e2a500") };
        var index = 0;
        foreach (var subject in notes
            .GroupBy(n => n.Materie)
            .Select(g => new SubjectAverage(g.Key, g.Average(n => n.Valoare), g.Count()))
            .OrderByDescending(s => s.Average))
        {
            panel.Children.Add(SubjectAverageRow(subject, colors[index % colors.Length]));
            index++;
        }

        if (index == 0)
            panel.Children.Add(Text("Nu exista medii disponibile.", Muted, 13));

        return new Border
        {
            Background = Surface,
            BorderBrush = CardBorder,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(24),
            MinHeight = 430,
            Child = panel
        };
    }

    private static Control SubjectAverageRow(SubjectAverage subject, IBrush accent)
    {
        return new StackPanel
        {
            Spacing = 7,
            Children =
            {
                new Grid
                {
                    ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                    Children =
                    {
                        Text(subject.Name, TextBrush, 13, FontWeight.Bold),
                        new TextBlock
                        {
                            [Grid.ColumnProperty] = 1,
                            Text = subject.Average.ToString("0.0"),
                            Foreground = TextBrush,
                            FontSize = 13,
                            FontWeight = FontWeight.Bold,
                            HorizontalAlignment = HorizontalAlignment.Right
                        }
                    }
                },
                new ProgressBar
                {
                    Minimum = 0,
                    Maximum = 10,
                    Value = subject.Average,
                    Height = 7,
                    Foreground = accent,
                    Background = Brush.Parse("#eef2f7")
                },
                Text(subject.Count.ToString(), Muted, 11, FontWeight.Normal, HorizontalAlignment.Right)
            }
        };
    }

    private static string NoteDescription(Nota note)
    {
        var details = string.IsNullOrWhiteSpace(note.Descriere) ? "Nota" : note.Descriere!;
        if (note.User is not null && IsAdmin())
            return $"{details} • {note.User.NumeComplet}";
        return details;
    }

    private async Task ShowTemeAsync()
    {
        var canEdit = IsAdmin();
        var user = AuthService.CurrentUser!;
        using var db = new AppDbContext();
        var teme = canEdit
            ? await db.Teme.Include(t => t.User).OrderBy(t => t.Deadline).ToListAsync()
            : await db.Teme.Where(t => t.UserId == user.Id).Include(t => t.User).OrderBy(t => t.Deadline).ToListAsync();

        _content!.Spacing = 28;
        _content.Children.Add(new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            Children =
            {
                new StackPanel
                {
                    Spacing = 6,
                    Children =
                    {
                        Text("Teme", TextBrush, 28, FontWeight.Bold),
                        Text("Gestioneaza-ti sarcinile si respecta termenele limita", Muted, 14)
                    }
                },
                canEdit ? NewHomeworkButton() : new Border()
            }
        });

        _content.Children.Add(HomeworkStatsGrid(teme));
        _content.Children.Add(HomeworkListPanel(teme, canEdit));

        if (canEdit)
        {
            _content!.Children.Add(await TemaFormAsync());
            await LoadTemaListAsync(canEdit);
        }
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

    private Button NewHomeworkButton()
    {
        var button = HeaderActionButton("+  Tema noua", () => { }, Primary, Brushes.White);
        button.SetValue(Grid.ColumnProperty, 1);
        button.Padding = new Thickness(22, 12);
        button.Margin = new Thickness(0, 4, 0, 0);
        return button;
    }

    private static Grid HomeworkStatsGrid(List<Tema> teme)
    {
        var total = teme.Count;
        var done = teme.Count(t => t.Finalizata);
        var inProgress = teme.Count(t => !t.Finalizata && t.Deadline >= DateTime.Today);
        var todo = teme.Count(t => !t.Finalizata && t.Deadline < DateTime.Today);

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*,*"),
            ColumnSpacing = 24
        };
        grid.Children.Add(HomeworkStatCard("Total teme", total.ToString(), "Teme", Primary, "#e7f0ff", 0));
        grid.Children.Add(HomeworkStatCard("Finalizate", done.ToString(), "Teme", Success, "#dcfce7", 1));
        grid.Children.Add(HomeworkStatCard("In lucru", inProgress.ToString(), "Ceas", Primary, "#dbeafe", 2));
        grid.Children.Add(HomeworkStatCard("De facut", todo.ToString(), "Alerta", Warning, "#fff0df", 3));
        return grid;
    }

    private static Border HomeworkStatCard(string label, string value, string icon, IBrush accent, string background, int column)
    {
        return new Border
        {
            [Grid.ColumnProperty] = column,
            Background = Surface,
            BorderBrush = CardBorder,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(22),
            MinHeight = 124,
            Child = new StackPanel
            {
                Spacing = 14,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 13,
                        Children =
                        {
                            new Border
                            {
                                Width = 34,
                                Height = 34,
                                CornerRadius = new CornerRadius(8),
                                Background = Brush.Parse(background),
                                Child = IconText(icon, 17, accent)
                            },
                            Text(label, TextBrush, 13, FontWeight.Bold)
                        }
                    },
                    Text(value, TextBrush, 28, FontWeight.Bold)
                }
            }
        };
    }

    private Border HomeworkListPanel(List<Tema> teme, bool canEdit)
    {
        var panel = new StackPanel { Spacing = 16 };
        panel.Children.Add(new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto"),
            Children =
            {
                Text("Toate temele", TextBrush, 20, FontWeight.Bold),
                new TextBlock
                {
                    [Grid.ColumnProperty] = 1,
                    Text = IconGlyph("Cautare"),
                    FontFamily = IconFont,
                    Foreground = Muted,
                    FontSize = 18
                },
                new TextBlock
                {
                    [Grid.ColumnProperty] = 2,
                    Text = IconGlyph("Filtru"),
                    FontFamily = IconFont,
                    Foreground = Muted,
                    FontSize = 18,
                    Margin = new Thickness(22, 0, 0, 0)
                }
            }
        });

        foreach (var tema in teme)
            panel.Children.Add(HomeworkRow(tema, canEdit));

        if (teme.Count == 0)
            panel.Children.Add(Text("Nu exista teme.", Muted, 13));

        return new Border
        {
            Background = Surface,
            BorderBrush = CardBorder,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(22),
            MinHeight = 430,
            Child = panel
        };
    }

    private Control HomeworkRow(Tema tema, bool canEdit)
    {
        var status = HomeworkStatus(tema);
        var statusBrush = status switch
        {
            "Finalizata" => Success,
            "In lucru" => Primary,
            "Expirata" => Danger,
            _ => Warning
        };
        var statusBg = status switch
        {
            "Finalizata" => "#dcfce7",
            "In lucru" => "#dbeafe",
            "Expirata" => "#feecec",
            _ => "#fff0df"
        };

        var checkbox = new CheckBox
        {
            IsChecked = tema.Finalizata,
            VerticalAlignment = VerticalAlignment.Center,
            IsEnabled = !tema.Finalizata || canEdit
        };
        checkbox.Click += (_, _) => _ = CompleteTemaAsync(tema.Id);

        return new Border
        {
            BorderBrush = tema.Finalizata ? CardBorder : Brush.Parse("#d6e8ff"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(11),
            Padding = new Thickness(20),
            MinHeight = 112,
            Child = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto"),
                ColumnSpacing = 18,
                Children =
                {
                    new StackPanel
                    {
                        Spacing = 10,
                        Children =
                        {
                            new StackPanel
                            {
                                Orientation = Orientation.Horizontal,
                                Spacing = 10,
                                Children =
                                {
                                    Text(tema.Titlu, TextBrush, 16, FontWeight.Bold),
                                    Badge(status, statusBrush, statusBg)
                                }
                            },
                            Text(tema.User is not null && canEdit ? $"{tema.Materie} • {tema.User.NumeComplet}" : tema.Materie, Muted, 12),
                            new StackPanel
                            {
                                Orientation = Orientation.Horizontal,
                                Spacing = 12,
                                Children =
                                {
                                    Text(tema.Materie, TextBrush, 12, FontWeight.Bold),
                                    Badge(HomeworkPriority(tema), Danger, "#feecec")
                                }
                            }
                        }
                    },
                    new StackPanel
                    {
                        [Grid.ColumnProperty] = 1,
                        Orientation = Orientation.Horizontal,
                        Spacing = 8,
                        VerticalAlignment = VerticalAlignment.Top,
                        Children =
                        {
                            IconText("Orar", 14, Muted),
                            Text(tema.Deadline.ToString("yyyy-MM-dd"), Muted, 12)
                        }
                    },
                    new StackPanel
                    {
                        [Grid.ColumnProperty] = 2,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children = { checkbox }
                    }
                }
            }
        };
    }

    private static Border Badge(string text, IBrush foreground, string background)
    {
        return new Border
        {
            Background = Brush.Parse(background),
            BorderBrush = foreground,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(10, 4),
            Child = Text(text, foreground, 11, FontWeight.Bold)
        };
    }

    private static string HomeworkStatus(Tema tema)
    {
        if (tema.Finalizata)
            return "Finalizata";
        if (tema.Deadline < DateTime.Today)
            return "Expirata";
        return tema.Deadline <= DateTime.Today.AddDays(3) ? "De facut" : "In lucru";
    }

    private static string HomeworkPriority(Tema tema)
    {
        if (tema.Finalizata)
            return "Finalizata";
        var days = (tema.Deadline - DateTime.Today).Days;
        return days <= 2 ? "Prioritate inalta" : days <= 5 ? "Prioritate medie" : "Prioritate normala";
    }

    private async Task ShowOrarAsync()
    {
        var canEdit = IsAdmin();
        var user = AuthService.CurrentUser!;
        using var db = new AppDbContext();
        IQueryable<OrarEntry> query = db.OrarEntries;
        if (!canEdit && !AppDbContext.UsesMySql)
            query = query.Where(o => o.UserId == user.Id);
        var items = await query.ToListAsync();

        _content!.Spacing = 28;
        _content.Children.Add(new StackPanel
        {
            Spacing = 6,
            Children =
            {
                Text("Orar", TextBrush, 28, FontWeight.Bold),
                Text("Vizualizeaza programul tau saptamanal", Muted, 14)
            }
        });

        _content.Children.Add(WeekSelectorPanel());
        _content.Children.Add(ScheduleGrid(items, canEdit));
        _content.Children.Add(ScheduleStats(items));

        if (canEdit)
        {
            _content!.Children.Add(await OrarFormAsync());
            await LoadOrarListAsync(canEdit);
        }
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
            db.OrarEntries.Add(new OrarEntry { ZiSaptamana = zi.SelectedItem?.ToString() ?? "Luni", OraInceput = oraStart, OraSfarsit = oraEnd, Materie = materie.Text!.Trim(), Profesor = profesor.Text?.Trim(), UserId = AppDbContext.UsesMySql ? null : elevId });
            db.Activitati.Add(new Activitate { Descriere = "Adminul a actualizat orarul", Tip = "orar", UserId = elevId });
            await db.SaveChangesAsync();
            await Navigate("Orar");
        }));

        return Card(form);
    }

    private async Task LoadOrarListAsync(bool canEdit)
    {
        using var db = new AppDbContext();
        IQueryable<OrarEntry> query = db.OrarEntries;
        if (!canEdit && !AppDbContext.UsesMySql)
            query = query.Where(o => o.UserId == AuthService.CurrentUser!.Id);
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

    private static Border WeekSelectorPanel()
    {
        var monday = DateTime.Today.AddDays(-(((int)DateTime.Today.DayOfWeek + 6) % 7));
        var friday = monday.AddDays(4);
        var todayName = ZiRomaneasca(DateTime.Today.DayOfWeek);
        var activeDay = new[] { "Luni", "Marti", "Miercuri", "Joi", "Vineri" }.Contains(todayName) ? todayName : "Miercuri";

        var dayTabs = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*,*,*"),
            ColumnSpacing = 8
        };

        var days = new[] { "Luni", "Marti", "Miercuri", "Joi", "Vineri" };
        for (var i = 0; i < days.Length; i++)
            dayTabs.Children.Add(DayTab(days[i], days[i] == activeDay, i));

        return new Border
        {
            Background = Surface,
            BorderBrush = CardBorder,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(24, 18),
            Child = new StackPanel
            {
                Spacing = 18,
                Children =
                {
                    new Grid
                    {
                        ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                        Children =
                        {
                            new StackPanel
                            {
                                Orientation = Orientation.Horizontal,
                                Spacing = 12,
                                Children =
                                {
                                    IconText("Orar", 18, Primary),
                                    new StackPanel
                                    {
                                        Children =
                                        {
                                            Text("Saptamana curenta", TextBrush, 16, FontWeight.Bold),
                                            Text($"{monday:dd} - {friday:dd} {MonthName(friday)} {friday:yyyy}", Muted, 12)
                                        }
                                    }
                                }
                            },
                            new StackPanel
                            {
                                [Grid.ColumnProperty] = 1,
                                Orientation = Orientation.Horizontal,
                                Spacing = 14,
                                Children =
                                {
                                    IconText("ChevronLeft", 14, Muted),
                                    HeaderActionButton("Astazi", () => { }, Primary, Brushes.White),
                                    IconText("ChevronRight", 14, Muted)
                                }
                            }
                        }
                    },
                    dayTabs
                }
            }
        };
    }

    private static Border DayTab(string day, bool active, int column)
    {
        return new Border
        {
            [Grid.ColumnProperty] = column,
            Background = active ? Primary : Brush.Parse(_darkMode ? "#111827" : "#f8fafc"),
            CornerRadius = new CornerRadius(9),
            Padding = new Thickness(16, 11),
            BoxShadow = active ? SoftShadow : default,
            Child = Text(day, active ? Brushes.White : TextBrush, 12, FontWeight.Bold, HorizontalAlignment.Center)
        };
    }

    private static Grid ScheduleGrid(List<OrarEntry> entries, bool canEdit)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*,*,*"),
            ColumnSpacing = 18
        };

        var weekDays = new[] { "Luni", "Marti", "Miercuri", "Joi", "Vineri" };
        for (var i = 0; i < weekDays.Length; i++)
        {
            var dayEntries = entries
                .Where(e => ZiEquals(e.ZiSaptamana, weekDays[i]))
                .OrderBy(e => e.OraInceput)
                .ToList();
            grid.Children.Add(DayScheduleColumn(weekDays[i], dayEntries, canEdit, i));
        }

        return grid;
    }

    private static Border DayScheduleColumn(string day, List<OrarEntry> entries, bool canEdit, int column)
    {
        var panel = new StackPanel { Spacing = 12 };
        panel.Children.Add(new Border
        {
            BorderBrush = CardBorder,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(0, 0, 0, 12),
            Child = Text(day, TextBrush, 14, FontWeight.Bold)
        });

        var addedPause = false;
        foreach (var entry in entries)
        {
            if (!addedPause && entry.OraInceput >= new TimeSpan(10, 0, 0))
            {
                panel.Children.Add(PauseBlock());
                addedPause = true;
            }

            panel.Children.Add(ScheduleLessonCard(entry, canEdit));
        }

        if (entries.Count == 0)
            panel.Children.Add(Text("Nu sunt ore programate.", Muted, 12));

        return new Border
        {
            [Grid.ColumnProperty] = column,
            Background = Surface,
            BorderBrush = CardBorder,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(18),
            MinHeight = 490,
            Child = panel
        };
    }

    private static Border ScheduleLessonCard(OrarEntry entry, bool canEdit)
    {
        var accent = SubjectColor(entry.Materie);
        return new Border
        {
            BorderBrush = CardBorder,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(9),
            Padding = new Thickness(12),
            Child = new StackPanel
            {
                Spacing = 7,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 8,
                        Children =
                        {
                            new Border
                            {
                                Width = 10,
                                Height = 10,
                                CornerRadius = new CornerRadius(5),
                                Background = accent,
                                VerticalAlignment = VerticalAlignment.Center
                            },
                            Text(entry.Materie, TextBrush, 12, FontWeight.Bold)
                        }
                    },
                    LessonMeta("Ceas", $"{entry.OraInceput:hh\\:mm} - {entry.OraSfarsit:hh\\:mm}"),
                    LessonMeta("User", entry.Profesor ?? "Profesor neatribuit"),
                    LessonMeta("Agenda", canEdit && entry.User is not null ? entry.User.NumeComplet : "Sala neatribuita")
                }
            }
        };
    }

    private static StackPanel LessonMeta(string icon, string text)
    {
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 7,
            Children =
            {
                IconText(icon, 11, Muted),
                Text(text, Muted, 11)
            }
        };
    }

    private static Border PauseBlock()
    {
        return new Border
        {
            Background = Brush.Parse(_darkMode ? "#111827" : "#f8fafc"),
            BorderBrush = CardBorder,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(9),
            Padding = new Thickness(12),
            Child = new StackPanel
            {
                Children =
                {
                    Text("Pauza", Muted, 12, FontWeight.Bold, HorizontalAlignment.Center),
                    Text("10:00 - 11:00", Muted, 10, FontWeight.Normal, HorizontalAlignment.Center)
                }
            }
        };
    }

    private static Grid ScheduleStats(List<OrarEntry> entries)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*"),
            ColumnSpacing = 18
        };
        grid.Children.Add(ScheduleStatCard("Ore pe saptamana", entries.Count.ToString(), Primary, 0));
        grid.Children.Add(ScheduleStatCard("Materii", entries.Select(e => e.Materie).Distinct(StringComparer.OrdinalIgnoreCase).Count().ToString(), Purple, 1));
        grid.Children.Add(ScheduleStatCard("Profesori", entries.Select(e => e.Profesor).Where(p => !string.IsNullOrWhiteSpace(p)).Distinct(StringComparer.OrdinalIgnoreCase).Count().ToString(), Success, 2));
        return grid;
    }

    private static Border ScheduleStatCard(string label, string value, IBrush brush, int column)
    {
        return new Border
        {
            [Grid.ColumnProperty] = column,
            Background = brush,
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(22, 18),
            MinHeight = 70,
            Child = new StackPanel
            {
                Spacing = 7,
                Children =
                {
                    Text(label, Brushes.White, 11, FontWeight.Bold),
                    Text(value, Brushes.White, 24, FontWeight.Bold)
                }
            }
        };
    }

    private static IBrush SubjectColor(string subject)
    {
        var colors = new[]
        {
            Primary,
            Purple,
            Success,
            Warning,
            Danger,
            Brush.Parse("#16a3b8"),
            Brush.Parse("#84cc16"),
            Brush.Parse("#ec4899")
        };
        var index = Math.Abs(StringComparer.OrdinalIgnoreCase.GetHashCode(subject)) % colors.Length;
        return colors[index];
    }

    private static string MonthName(DateTime date) => date.Month switch
    {
        1 => "Ian",
        2 => "Feb",
        3 => "Mar",
        4 => "Apr",
        5 => "Mai",
        6 => "Iun",
        7 => "Iul",
        8 => "Aug",
        9 => "Sep",
        10 => "Oct",
        11 => "Noi",
        _ => "Dec"
    };

    private async Task ShowAdminAsync()
    {
        if (!IsAdmin())
        {
            await ShowDashboardAsync();
            return;
        }

        using var db = new AppDbContext();
        var users = await db.Users.OrderBy(u => u.NumeComplet).ToListAsync();
        var materii = await db.Note.Select(n => n.Materie).Distinct().CountAsync();
        var activitati = await db.Activitati.CountAsync();
        var temeNerezolvate = await db.Teme.CountAsync(t => !t.Finalizata && t.Deadline < DateTime.Today);
        var recent = await db.Activitati
            .Include(a => a.User)
            .OrderByDescending(a => a.Timestamp)
            .Take(4)
            .ToListAsync();

        var root = new StackPanel { Spacing = 28 };
        var stats = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*,*"),
            ColumnSpacing = 24
        };
        stats.Children.Add(AdminStatCard("Utilizatori activi", users.Count.ToString("N0"), "+12%", "User", Primary, 0));
        stats.Children.Add(AdminStatCard("Materii inregistrate", materii.ToString("N0"), "45 active", "Agenda", Brush.Parse("#00b956"), 1));
        stats.Children.Add(AdminStatCard("Activitati recente", activitati.ToString("N0"), "Astazi", "Activitate", Brush.Parse("#ff4b00"), 2));
        stats.Children.Add(AdminStatCard("Rata de utilizare", $"{Math.Min(94, users.Count * 3)}%", "+8%", "Rapoarte", Brush.Parse("#a217f2"), 3));
        root.Children.Add(stats);

        var lower = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("2.1*,1*"),
            ColumnSpacing = 28
        };
        lower.Children.Add(AdminActivityCard(recent));
        var alerts = AdminAlertsCard(temeNerezolvate, activitati);
        alerts.SetValue(Grid.ColumnProperty, 1);
        lower.Children.Add(alerts);
        root.Children.Add(lower);

        _content!.Spacing = 0;
        _content.Children.Add(root);
    }

    private async Task ShowAdminOrarAsync()
    {
        await ShowOrarAsync();
    }

    private async Task ShowRapoarteAsync()
    {
        if (!IsAdmin())
        {
            await ShowDashboardAsync();
            return;
        }

        using var db = new AppDbContext();
        var users = await db.Users.CountAsync(u => u.Rol.ToLower() != "admin");
        var notes = await db.Note.Include(n => n.User).ToListAsync();
        var themes = await db.Teme.Include(t => t.User).ToListAsync();
        var classes = await db.Clase.Include(c => c.Elevi).OrderBy(c => c.Nume).ToListAsync();

        var avg = notes.Count == 0 ? "0.00" : notes.Average(n => n.Valoare).ToString("0.00");
        var completion = themes.Count == 0 ? "0%" : $"{themes.Count(t => t.Finalizata) * 100 / themes.Count}%";

        var stats = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*,*,*"), ColumnSpacing = 24 };
        stats.Children.Add(AdminStatCard("Elevi monitorizati", users.ToString(), "total", "User", Primary, 0));
        stats.Children.Add(AdminStatCard("Media generala", avg, "note", "Note", Success, 1));
        stats.Children.Add(AdminStatCard("Teme finalizate", completion, "progres", "Teme", Warning, 2));
        stats.Children.Add(AdminStatCard("Clase active", classes.Count.ToString(), "clase", "Agenda", Purple, 3));

        var subjectRows = notes
            .GroupBy(n => n.Materie)
            .OrderByDescending(g => g.Average(n => n.Valoare))
            .Select((g, index) => new RowAction(index, new[]
            {
                g.Key,
                g.Count().ToString(),
                g.Select(n => n.UserId).Distinct().Count().ToString(),
                g.Average(n => n.Valoare).ToString("0.00")
            }))
            .ToList();

        var classRows = classes.Select(c =>
        {
            var classNotes = notes.Where(n => n.User?.ClasaId == c.Id).ToList();
            return new RowAction(c.Id, new[]
            {
                c.Nume,
                c.Elevi.Count.ToString(),
                classNotes.Count == 0 ? "-" : classNotes.Average(n => n.Valoare).ToString("0.00"),
                themes.Count(t => t.User?.ClasaId == c.Id && !t.Finalizata).ToString()
            });
        }).ToList();

        _content!.Children.Add(new StackPanel
        {
            Spacing = 24,
            Children =
            {
                stats,
                AdminDataPanel("Performanta pe materii", "Medii calculate automat din toate notele existente.", TableCard(new[] { "Materie", "Note", "Elevi", "Media" }, subjectRows)),
                AdminDataPanel("Situatie pe clase", "Clase, elevi si teme active.", TableCard(new[] { "Clasa", "Elevi", "Media", "Teme deschise" }, classRows))
            }
        });
    }

    private async Task ShowAprobariAsync()
    {
        if (!IsAdmin())
        {
            await ShowDashboardAsync();
            return;
        }

        using var db = new AppDbContext();
        var pending = await db.Teme
            .Include(t => t.User)
            .ThenInclude(u => u!.Clasa)
            .Where(t => !t.Finalizata)
            .OrderBy(t => t.Deadline)
            .ToListAsync();

        var stats = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*,*,*"), ColumnSpacing = 24 };
        stats.Children.Add(HomeworkStatCard("In asteptare", pending.Count.ToString(), "Aprobari", Purple, "#f3e8ff", 0));
        stats.Children.Add(HomeworkStatCard("Expirate", pending.Count(t => t.Deadline < DateTime.Today).ToString(), "Alerta", Danger, "#feecec", 1));
        stats.Children.Add(HomeworkStatCard("Scadente azi", pending.Count(t => t.Deadline == DateTime.Today).ToString(), "Ceas", Warning, "#fff0df", 2));
        stats.Children.Add(HomeworkStatCard("Elevi implicati", pending.Select(t => t.UserId).Distinct().Count().ToString(), "User", Primary, "#e7f0ff", 3));

        var panel = new StackPanel { Spacing = 14 };
        panel.Children.Add(Text("Aprobari si teme deschise", TextBrush, 20, FontWeight.Bold));
        foreach (var tema in pending)
            panel.Children.Add(AdminApprovalRow(tema));
        if (pending.Count == 0)
            panel.Children.Add(Text("Nu exista teme in asteptare.", Muted, 13));

        _content!.Children.Add(new StackPanel
        {
            Spacing = 24,
            Children =
            {
                stats,
                new Border
                {
                    Background = Surface,
                    BorderBrush = CardBorder,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(12),
                    Padding = new Thickness(22),
                    Child = panel
                }
            }
        });
    }

    private Control AdminApprovalRow(Tema tema)
    {
        var complete = HeaderActionButton("Aproba", () => _ = CompleteTemaAsync(tema.Id), Brush.Parse("#dcfce7"), Success);
        var delete = HeaderActionButton("Respinge", () => _ = DeleteTemaAsync(tema.Id), Brush.Parse("#feecec"), Danger);

        return new Border
        {
            BorderBrush = CardBorder,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(18),
            Child = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                Children =
                {
                    new StackPanel
                    {
                        Spacing = 8,
                        Children =
                        {
                            Text(tema.Titlu, TextBrush, 16, FontWeight.Bold),
                            Text($"{tema.User?.NumeComplet ?? "Elev"} • {tema.User?.Clasa?.Nume ?? "Fara clasa"} • {tema.Materie}", Muted, 12),
                            Badge(tema.Deadline < DateTime.Today ? "Expirata" : $"Deadline {tema.Deadline:dd.MM.yyyy}", tema.Deadline < DateTime.Today ? Danger : Warning, tema.Deadline < DateTime.Today ? "#feecec" : "#fff0df")
                        }
                    },
                    new StackPanel
                    {
                        [Grid.ColumnProperty] = 1,
                        Orientation = Orientation.Horizontal,
                        Spacing = 10,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children = { complete, delete }
                    }
                }
            }
        };
    }

    private Task ShowSetariSistemAsync()
    {
        if (!IsAdmin())
            return ShowDashboardAsync();

        _content!.Children.Add(new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            ColumnSpacing = 24,
            Children =
            {
                AdminSettingsCard("Aspect", "Pastreaza functionalitatile Light/Dark pentru intreaga aplicatie.", HeaderActionButton(_darkMode ? "Comuta pe Light" : "Comuta pe Dark", ToggleTheme, Brush.Parse("#f3e8ff"), Purple), 0),
                AdminSettingsCard("Sesiune", "Iesire sigura din contul curent.", HeaderActionButton("Log out", Logout, Brush.Parse("#feecec"), Danger), 1)
            }
        });
        return Task.CompletedTask;
    }

    private Task ShowSuportAsync()
    {
        if (!IsAdmin())
            return ShowDashboardAsync();

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*"), ColumnSpacing = 24 };
        grid.Children.Add(AdminSettingsCard("Suport tehnic", "Pentru probleme de autentificare, baza de date sau export PDF.", Text("Contact: admin@agenda.local", TextBrush, 14, FontWeight.Bold), 0));
        grid.Children.Add(AdminSettingsCard("Stare sistem", $"Baza de date locala: {AppDbContext.DatabasePath}", Text("Operational", Success, 18, FontWeight.Bold), 1));
        _content!.Children.Add(grid);
        return Task.CompletedTask;
    }

    private async Task ShowUtilizatoriAsync()
    {
        if (!IsAdmin())
        {
            await ShowDashboardAsync();
            return;
        }

        using var db = new AppDbContext();
        var clase = await db.Clase.OrderBy(c => c.Nume).ToListAsync();
        var elevi = await db.Users
            .Where(u => u.Rol.ToLower() != "admin")
            .Include(u => u.Clasa)
            .Include(u => u.Note)
            .OrderBy(u => u.Clasa!.Nume)
            .ThenBy(u => u.NumeComplet)
            .ToListAsync();

        var page = new StackPanel { Spacing = 18 };
        page.Children.Add(AdminClassesPanel(clase));

        if (elevi.Count == 0)
        {
            page.Children.Add(Card(Text("Nu exista elevi in baza de date.", Muted, 14)));
        }
        else
        {
            foreach (var elev in elevi)
                page.Children.Add(StudentGradesPanel(elev, clase));
        }

        _content!.Children.Add(page);
    }

    private Control AdminClassesPanel(List<Clasa> clase)
    {
        var name = Input("ex. Clasa a 10-a");
        var message = Text("", Danger, 12);
        var form = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            ColumnSpacing = 12,
            Children =
            {
                name,
                ActionButton("Creeaza clasa", async () =>
                {
                    var clasaNoua = (name.Text ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(clasaNoua))
                    {
                        message.Text = "Introdu numele clasei.";
                        return;
                    }

                    using var db = new AppDbContext();
                    if (await db.Clase.AnyAsync(c => c.Nume == clasaNoua))
                    {
                        message.Text = "Exista deja o clasa cu acest nume.";
                        return;
                    }

                    db.Clase.Add(new Clasa { Nume = clasaNoua });
                    await db.SaveChangesAsync();
                    await Navigate("Utilizatori");
                })
            }
        };
        form.Children[1].SetValue(Grid.ColumnProperty, 1);

        var claseText = clase.Count == 0
            ? "Nu exista clase create."
            : string.Join("   |   ", clase.Select(c => c.Nume));

        return Card(new StackPanel
        {
            Spacing = 12,
            Children =
            {
                Text("Clase", TextBrush, 18, FontWeight.Bold),
                Text("Creeaza clase si asociaza elevii la ele.", Muted, 12),
                form,
                message,
                Text(claseText, SecondaryText, 13)
            }
        });
    }

    private Control StudentGradesPanel(User elev, List<Clasa> clase)
    {
        var average = elev.Note.Count == 0 ? "-" : elev.Note.Average(n => n.Valoare).ToString("0.00");
        var classOptions = new List<ClassOption> { new(null, "Fara clasa") };
        classOptions.AddRange(clase.Select(c => new ClassOption(c.Id, c.Nume)));

        var classCombo = new ComboBox
        {
            ItemsSource = classOptions,
            SelectedItem = classOptions.FirstOrDefault(c => c.Id == elev.ClasaId) ?? classOptions[0],
            MinWidth = 170
        };

        var exportMessage = Text("", Success, 12);
        var header = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto,Auto"),
            ColumnSpacing = 12
        };
        header.Children.Add(new StackPanel
        {
            Spacing = 3,
            Children =
            {
                Text(elev.NumeComplet, TextBrush, 17, FontWeight.Bold),
                Text($"{elev.Email} • {elev.Clasa?.Nume ?? "Fara clasa"}", Muted, 12)
            }
        });

        var avg = new Border
        {
            [Grid.ColumnProperty] = 1,
            Background = Brush.Parse("#e7f0ff"),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(14, 9),
            Child = Text($"Media: {average}", Primary, 13, FontWeight.Bold)
        };
        header.Children.Add(avg);

        classCombo.SetValue(Grid.ColumnProperty, 2);
        header.Children.Add(classCombo);

        var saveClass = HeaderActionButton("Salveaza clasa", () => _ = UpdateStudentClassAsync(elev.Id, classCombo), Brush.Parse("#f8fafc"), TextBrush);
        saveClass.SetValue(Grid.ColumnProperty, 3);
        header.Children.Add(saveClass);

        var notes = elev.Note
            .OrderBy(n => n.Materie)
            .ThenByDescending(n => n.Data)
            .Select(n => new RowAction(n.Id, new[]
            {
                n.Materie,
                n.Valoare.ToString(),
                n.Data.ToString("dd.MM.yyyy"),
                n.Descriere ?? ""
            }))
            .ToList();

        var export = HeaderActionButton("Export PDF", () =>
        {
            var path = ExportStudentNotesPdf(elev);
            exportMessage.Text = $"Exportat: {path}";
        }, Brush.Parse("#eef5ff"), Primary);

        var content = new StackPanel
        {
            Spacing = 14,
            Children =
            {
                header,
                TableCard(new[] { "Materie", "Nota", "Data", "Descriere" }, notes),
                export,
                exportMessage
            }
        };

        return Card(content);
    }

    private async Task UpdateStudentClassAsync(int userId, ComboBox combo)
    {
        var option = combo.SelectedItem as ClassOption;
        using var db = new AppDbContext();
        var user = await db.Users.FindAsync(userId);
        if (user is not null)
        {
            user.ClasaId = option?.Id;
            await db.SaveChangesAsync();
        }
        await Navigate("Utilizatori");
    }

    private async Task ShowMateriiAsync()
    {
        if (!IsAdmin())
        {
            await ShowDashboardAsync();
            return;
        }

        using var db = new AppDbContext();
        var note = await db.Note.Include(n => n.User).ToListAsync();
        var teme = await db.Teme.ToListAsync();
        var orar = await db.OrarEntries.ToListAsync();

        var materii = note.Select(n => n.Materie)
            .Concat(teme.Select(t => t.Materie))
            .Concat(orar.Select(o => o.Materie))
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(m => m)
            .ToList();

        var rows = materii.Select((materie, index) =>
        {
            var noteMaterie = note.Where(n => string.Equals(n.Materie, materie, StringComparison.OrdinalIgnoreCase)).ToList();
            var media = noteMaterie.Count == 0 ? "-" : noteMaterie.Average(n => n.Valoare).ToString("0.00");
            return new RowAction(index, new[]
            {
                materie,
                noteMaterie.Select(n => n.UserId).Distinct().Count().ToString(),
                noteMaterie.Count.ToString(),
                media,
                teme.Count(t => string.Equals(t.Materie, materie, StringComparison.OrdinalIgnoreCase)).ToString(),
                orar.Count(o => string.Equals(o.Materie, materie, StringComparison.OrdinalIgnoreCase)).ToString()
            });
        }).ToList();

        _content!.Children.Add(Card(new StackPanel
        {
            Spacing = 14,
            Children =
            {
                Text("Materii si note", TextBrush, 18, FontWeight.Bold),
                Text("Toate materiile sunt citite din note, teme si orar.", Muted, 12),
                TableCard(new[] { "Materie", "Elevi cu note", "Total note", "Media", "Teme", "Ore in orar" }, rows)
            }
        }));
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

    private static Border StudentSummaryCard(string title, string icon, IEnumerable<Pair> rows, string linkText, IBrush accent, int column)
    {
        var panel = new StackPanel { Spacing = 20 };
        panel.Children.Add(new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
            Children =
            {
                new Border
                {
                    Width = 32,
                    Height = 32,
                    CornerRadius = new CornerRadius(8),
                    Background = Brush.Parse(icon == "Teme" ? "#fff0df" : icon == "Orar" ? "#dcfce7" : "#e7f0ff"),
                    Child = IconText(icon, 16, accent)
                },
                new TextBlock
                {
                    [Grid.ColumnProperty] = 1,
                    Text = title,
                    Foreground = TextBrush,
                    FontSize = 16,
                    FontWeight = FontWeight.Bold,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(12, 0, 0, 0)
                },
                new TextBlock
                {
                    [Grid.ColumnProperty] = 2,
                    Text = IconGlyph("ChevronRight"),
                    FontFamily = IconFont,
                    Foreground = Muted,
                    FontSize = 13,
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        });

        var any = false;
        foreach (var row in rows)
        {
            any = true;
            panel.Children.Add(new Border
            {
                BorderBrush = CardBorder,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(0, 0, 0, 10),
                Child = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                    Children =
                    {
                        Text(row.Left, TextBrush, 13),
                        new Border
                        {
                            [Grid.ColumnProperty] = 1,
                            Background = icon == "Note" ? Brush.Parse("#eef5ff") : Brushes.Transparent,
                            CornerRadius = new CornerRadius(8),
                            Padding = new Thickness(10, 4),
                            Child = Text(row.Right, accent, 12, FontWeight.Bold)
                        }
                    }
                }
            });
        }

        if (!any)
            panel.Children.Add(Text("Nu exista date.", Muted, 13));

        panel.Children.Add(new TextBlock
        {
            Text = linkText,
            Foreground = accent,
            FontSize = 12,
            FontWeight = FontWeight.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 8, 0, 0)
        });

        return new Border
        {
            [Grid.ColumnProperty] = column,
            Background = Surface,
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(22),
            MinHeight = 268,
            BoxShadow = SoftShadow,
            Child = panel
        };
    }

    private static Border StudentActivityCard(IEnumerable<Activitate> activities)
    {
        var panel = new StackPanel { Spacing = 22 };
        panel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 9,
            Children =
            {
                IconText("Ceas", 20, Primary),
                Text("Activitate recenta", TextBrush, 18, FontWeight.Bold)
            }
        });

        var any = false;
        foreach (var activity in activities)
        {
            any = true;
            panel.Children.Add(StudentActivityRow(activity));
        }

        if (!any)
            panel.Children.Add(Text("Nu exista activitate recenta.", Muted, 13));

        panel.Children.Add(Text("Vezi toata activitatea", Primary, 12, FontWeight.Bold, HorizontalAlignment.Center));

        return new Border
        {
            Background = Surface,
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(26, 26),
            MinHeight = 330,
            BoxShadow = SoftShadow,
            Child = panel
        };
    }

    private static Control StudentActivityRow(Activitate activity)
    {
        var iconKey = activity.Tip switch
        {
            "nota" => "Note",
            "tema" => "Teme",
            "orar" => "Orar",
            _ => "User"
        };
        var accent = activity.Tip switch
        {
            "nota" => Success,
            "tema" => Brush.Parse("#f2aa00"),
            "orar" => Primary,
            _ => Primary
        };

        return new Border
        {
            BorderBrush = CardBorder,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(0, 0, 0, 16),
            Child = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("48,*,Auto"),
                Children =
                {
                    new Border
                    {
                        Width = 34,
                        Height = 34,
                        CornerRadius = new CornerRadius(17),
                        Background = accent,
                        Child = IconText(iconKey, 15, Brushes.White)
                    },
                    new TextBlock
                    {
                        [Grid.ColumnProperty] = 1,
                        Text = activity.Descriere,
                        Foreground = TextBrush,
                        FontSize = 13,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextWrapping = TextWrapping.Wrap
                    },
                    new TextBlock
                    {
                        [Grid.ColumnProperty] = 2,
                        Text = activity.Timestamp.Date == DateTime.Today ? $"Astazi, {activity.Timestamp:HH:mm}" : activity.Timestamp.ToString("dd.MM.yyyy, HH:mm"),
                        Foreground = Muted,
                        FontSize = 12,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(18, 0, 0, 0)
                    }
                }
            }
        };
    }

    private static Border AdminStatCard(string label, string value, string badge, string icon, IBrush brush, int column)
    {
        var card = new Border
        {
            [Grid.ColumnProperty] = column,
            MinHeight = 130,
            Background = brush,
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(24, 18),
            BoxShadow = new BoxShadows(new BoxShadow
            {
                OffsetY = 8,
                Blur = 18,
                Color = Color.FromArgb(45, 15, 23, 42)
            })
        };

        card.Child = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*,Auto"),
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            Children =
            {
                IconText(icon, 24, Brushes.White),
                new Border
                {
                    [Grid.ColumnProperty] = 1,
                    Background = Brush.Parse("#ffffff33"),
                    CornerRadius = new CornerRadius(12),
                    Padding = new Thickness(9, 4),
                    Child = Text(badge, Brushes.White, 10, FontWeight.Bold)
                },
                new TextBlock
                {
                    [Grid.RowProperty] = 1,
                    [Grid.ColumnSpanProperty] = 2,
                    Text = value,
                    Foreground = Brushes.White,
                    FontSize = 30,
                    FontWeight = FontWeight.Bold,
                    VerticalAlignment = VerticalAlignment.Bottom
                },
                new TextBlock
                {
                    [Grid.RowProperty] = 2,
                    [Grid.ColumnSpanProperty] = 2,
                    Text = label,
                    Foreground = Brushes.White,
                    FontSize = 11,
                    Opacity = 0.9
                }
            }
        };
        return card;
    }

    private static Border AdminActivityCard(IEnumerable<Activitate> activities)
    {
        var panel = new StackPanel { Spacing = 18 };
        panel.Children.Add(new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            Children =
            {
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 8,
                    Children =
                    {
                        IconText("Activitate", 15, Purple),
                        Text("Activitate recenta", TextBrush, 15, FontWeight.Bold)
                    }
                },
                new TextBlock
                {
                    [Grid.ColumnProperty] = 1,
                    Text = "Vezi tot",
                    Foreground = Purple,
                    FontSize = 11,
                    FontWeight = FontWeight.Bold
                }
            }
        });

        var any = false;
        foreach (var item in activities)
        {
            any = true;
            panel.Children.Add(AdminActivityRow(item));
        }

        if (!any)
            panel.Children.Add(Text("Nu exista activitate recenta.", Muted, 13));

        return AdminPanel(panel, 0);
    }

    private static Control AdminActivityRow(Activitate activity)
    {
        var iconKey = activity.Tip switch
        {
            "nota" => "Rapoarte",
            "tema" => "Teme",
            "orar" => "Orar",
            _ => "User"
        };
        var iconBrush = activity.Tip switch
        {
            "nota" => Primary,
            "tema" => Warning,
            "orar" => Purple,
            _ => Success
        };
        var iconBackground = activity.Tip switch
        {
            "nota" => "#e7f0ff",
            "tema" => "#fff0df",
            "orar" => "#f3e8ff",
            _ => "#dcfce7"
        };

        return new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("36,*"),
            Children =
            {
                new Border
                {
                    Width = 28,
                    Height = 28,
                    CornerRadius = new CornerRadius(7),
                    Background = Brush.Parse(iconBackground),
                    Child = IconText(iconKey, 14, iconBrush)
                },
                new StackPanel
                {
                    [Grid.ColumnProperty] = 1,
                    Spacing = 2,
                    Children =
                    {
                        Text($"{activity.User?.NumeComplet ?? "Sistem"} {activity.Descriere}", TextBrush, 12, FontWeight.Bold),
                        new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Spacing = 5,
                            Children =
                            {
                                IconText("Ceas", 10, Muted),
                                Text(RelativeTime(activity.Timestamp), Muted, 10)
                            }
                        }
                    }
                }
            }
        };
    }

    private static Border AdminAlertsCard(int temeNerezolvate, int activitati)
    {
        var panel = new StackPanel { Spacing = 17 };
        panel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                IconText("Alerta", 15, Danger),
                Text("Alerte sistem", TextBrush, 15, FontWeight.Bold)
            }
        });
        panel.Children.Add(AlertBox("Server backup programat pentru diseara la 22:00", Primary, "#e7f0ff"));
        panel.Children.Add(AlertBox($"{Math.Max(temeNerezolvate, 3)} teme necorectate de peste 7 zile", Warning, "#fff7e6"));
        panel.Children.Add(AlertBox($"Stocare utilizata: {Math.Min(85, 45 + activitati)}% din capacitate", Danger, "#feecec"));
        return AdminPanel(panel, 1);
    }

    private static Border AlertBox(string message, IBrush accent, string background)
    {
        return new Border
        {
            Background = Brush.Parse(background),
            BorderBrush = accent,
            BorderThickness = new Thickness(3, 0, 0, 0),
            CornerRadius = new CornerRadius(9),
            Padding = new Thickness(14, 12),
            Child = Text(message, accent, 12, FontWeight.Bold)
        };
    }

    private static Border AdminPanel(Control child, int column)
    {
        return new Border
        {
            [Grid.ColumnProperty] = column,
            Background = Surface,
            BorderBrush = Brush.Parse(_darkMode ? "#1f2937" : "#eef2f7"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(26, 22),
            MinHeight = 340,
            Child = child
        };
    }

    private static Border AdminDataPanel(string title, string subtitle, Control child)
    {
        return new Border
        {
            Background = Surface,
            BorderBrush = CardBorder,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(22),
            Child = new StackPanel
            {
                Spacing = 12,
                Children =
                {
                    Text(title, TextBrush, 18, FontWeight.Bold),
                    Text(subtitle, Muted, 12),
                    child
                }
            }
        };
    }

    private static Border AdminSettingsCard(string title, string subtitle, Control action, int column)
    {
        return new Border
        {
            [Grid.ColumnProperty] = column,
            Background = Surface,
            BorderBrush = CardBorder,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(24),
            MinHeight = 180,
            Child = new StackPanel
            {
                Spacing = 14,
                Children =
                {
                    Text(title, TextBrush, 20, FontWeight.Bold),
                    Text(subtitle, Muted, 13),
                    action
                }
            }
        };
    }

    private static string RelativeTime(DateTime timestamp)
    {
        var diff = DateTime.Now - timestamp;
        if (diff.TotalMinutes < 1)
            return "Acum";
        if (diff.TotalMinutes < 60)
            return $"Acum {(int)diff.TotalMinutes} min";
        if (diff.TotalHours < 24)
            return $"Acum {(int)diff.TotalHours} ora";
        return $"Acum {(int)diff.TotalDays} zile";
    }

    private static string ExportStudentNotesPdf(User elev)
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
            "AgendaExports");
        Directory.CreateDirectory(folder);

        var fileName = $"Note_{SafeFileName(elev.NumeComplet)}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        var path = Path.Combine(folder, fileName);
        var average = elev.Note.Count == 0 ? "-" : elev.Note.Average(n => n.Valoare).ToString("0.00");

        var lines = new List<string>
        {
            "Agenda - tabel note elev",
            $"Elev: {elev.NumeComplet}",
            $"Email: {elev.Email}",
            $"Clasa: {elev.Clasa?.Nume ?? "Fara clasa"}",
            $"Media generala: {average}",
            "",
            "Materie | Nota | Data | Descriere"
        };

        lines.AddRange(elev.Note
            .OrderBy(n => n.Materie)
            .ThenByDescending(n => n.Data)
            .Select(n => $"{n.Materie} | {n.Valoare} | {n.Data:dd.MM.yyyy} | {n.Descriere ?? ""}"));

        if (elev.Note.Count == 0)
            lines.Add("Nu exista note pentru acest elev.");

        File.WriteAllBytes(path, BuildSimplePdf(lines));
        return path;
    }

    private static byte[] BuildSimplePdf(IReadOnlyList<string> lines)
    {
        var objects = new List<string>
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"
        };

        var content = new System.Text.StringBuilder();
        content.AppendLine("BT");
        content.AppendLine("/F1 12 Tf");
        content.AppendLine("50 792 Td");
        foreach (var line in lines.Take(42))
        {
            content.Append('(').Append(EscapePdfText(line)).AppendLine(") Tj");
            content.AppendLine("0 -18 Td");
        }
        content.AppendLine("ET");

        var stream = content.ToString();
        objects.Add($"<< /Length {System.Text.Encoding.ASCII.GetByteCount(stream)} >>\nstream\n{stream}endstream");

        var pdf = new System.Text.StringBuilder();
        var offsets = new List<int> { 0 };
        pdf.AppendLine("%PDF-1.4");
        for (var i = 0; i < objects.Count; i++)
        {
            offsets.Add(System.Text.Encoding.ASCII.GetByteCount(pdf.ToString()));
            pdf.AppendLine($"{i + 1} 0 obj");
            pdf.AppendLine(objects[i]);
            pdf.AppendLine("endobj");
        }

        var xref = System.Text.Encoding.ASCII.GetByteCount(pdf.ToString());
        pdf.AppendLine("xref");
        pdf.AppendLine($"0 {objects.Count + 1}");
        pdf.AppendLine("0000000000 65535 f ");
        foreach (var offset in offsets.Skip(1))
            pdf.AppendLine($"{offset:0000000000} 00000 n ");

        pdf.AppendLine("trailer");
        pdf.AppendLine($"<< /Size {objects.Count + 1} /Root 1 0 R >>");
        pdf.AppendLine("startxref");
        pdf.AppendLine(xref.ToString());
        pdf.AppendLine("%%EOF");

        return System.Text.Encoding.ASCII.GetBytes(pdf.ToString());
    }

    private static string EscapePdfText(string value)
    {
        var normalized = new System.Text.StringBuilder();
        foreach (var ch in value)
        {
            var c = ch switch
            {
                'ă' or 'â' => 'a',
                'Ă' or 'Â' => 'A',
                'î' => 'i',
                'Î' => 'I',
                'ș' or 'ş' => 's',
                'Ș' or 'Ş' => 'S',
                'ț' or 'ţ' => 't',
                'Ț' or 'Ţ' => 'T',
                _ => ch
            };

            if (c is '(' or ')' or '\\')
                normalized.Append('\\');
            normalized.Append(c <= 127 ? c : '?');
        }
        return normalized.ToString();
    }

    private static string SafeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(value.Select(ch => invalid.Contains(ch) ? '_' : ch)).Replace(' ', '_');
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
            .Where(u => u.Rol.ToLower() != "admin")
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

    private static Control RegisterField(string label, string icon, TextBox input)
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

        var iconText = IconText(icon, 13, Muted);
        iconText.HorizontalAlignment = HorizontalAlignment.Center;
        input.SetValue(Grid.ColumnProperty, 1);

        box.Children.Add(new Border
        {
            BorderBrush = CardBorder,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Child = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("34,*"),
                Children =
                {
                    iconText,
                    input
                }
            }
        });

        field.Children.Add(box);
        return field;
    }

    private static Border AuthCard(Control form)
    {
        return new Border
        {
            Width = 360,
            Background = Surface,
            BorderBrush = Brush.Parse(_darkMode ? "#4ade80" : "#9fd7b2"),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(14, 24, 14, 14),
            Child = form
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
        => NavButton(text, () => { }, Brushes.Transparent, Muted);

    private static Button NavButton(string text, Action action, IBrush background, IBrush foreground)
    {
        var button = SmallButton("", action, background, foreground);
        button.Content = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children =
            {
                new TextBlock
                {
                    Text = IconGlyph(text),
                    FontFamily = IconFont,
                    FontSize = 15,
                    VerticalAlignment = VerticalAlignment.Center
                },
                new TextBlock
                {
                    Text = text,
                    FontSize = 13,
                    FontWeight = FontWeight.Bold,
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
        return button;
    }

    private static Button StudentSidebarButton(string text, string icon, bool active, Action action)
    {
        var foreground = active ? Brushes.White : Muted;
        var button = SmallButton("", action, active ? Brush.Parse("#1f5cff") : Brushes.Transparent, foreground);
        button.HorizontalAlignment = HorizontalAlignment.Stretch;
        button.HorizontalContentAlignment = HorizontalAlignment.Left;
        button.CornerRadius = new CornerRadius(9);
        button.Padding = new Thickness(14, 12);
        button.Margin = new Thickness(0, 0, 0, 2);
        button.Content = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 11,
            Children =
            {
                IconText(icon, 15, foreground),
                Text(text, foreground, 13, FontWeight.Bold)
            }
        };
        return button;
    }

    private static Button AdminSidebarButton(string text, string icon, bool active, Action action)
    {
        var foreground = active ? Brushes.White : Muted;
        var button = SmallButton("", action, active ? Brush.Parse("#a700ff") : Brushes.Transparent, foreground);
        button.HorizontalAlignment = HorizontalAlignment.Stretch;
        button.HorizontalContentAlignment = HorizontalAlignment.Left;
        button.CornerRadius = new CornerRadius(9);
        button.Padding = new Thickness(14, 12);
        button.Margin = new Thickness(0, 0, 0, 2);
        button.Content = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children =
            {
                IconText(icon, 15, foreground),
                Text(text, foreground, 13, FontWeight.Bold)
            }
        };
        return button;
    }

    private static Border NotificationButton(string count)
    {
        return new Border
        {
            Width = 40,
            Height = 40,
            CornerRadius = new CornerRadius(20),
            Background = Brush.Parse(_darkMode ? "#111827" : "#ffffff"),
            BorderBrush = CardBorder,
            BorderThickness = new Thickness(1),
            Child = new Grid
            {
                Children =
                {
                    IconText("Notificari", 16, Muted),
                    new Border
                    {
                        Width = 16,
                        Height = 16,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Top,
                        Background = Brush.Parse("#1f5cff"),
                        BorderBrush = Surface,
                        BorderThickness = new Thickness(2),
                        CornerRadius = new CornerRadius(8),
                        Child = Text(count, Brushes.White, 8, FontWeight.Bold, HorizontalAlignment.Center)
                    }
                }
            }
        };
    }

    private static Button HeaderActionButton(string text, Action action, IBrush background, IBrush foreground)
    {
        var button = SmallButton(text, action, background, foreground);
        button.BorderBrush = CardBorder;
        button.CornerRadius = new CornerRadius(10);
        button.Padding = new Thickness(14, 9);
        button.VerticalAlignment = VerticalAlignment.Center;
        button.HorizontalContentAlignment = HorizontalAlignment.Center;
        return button;
    }

    private static StackPanel ProfileBlock(User user, bool admin)
    {
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children =
            {
                new Border
                {
                    Width = 40,
                    Height = 40,
                    CornerRadius = new CornerRadius(20),
                    Background = Brush.Parse(admin ? "#f3e8ff" : "#dbeafe"),
                    Child = IconText(admin ? "Admin" : "User", 18, admin ? Purple : Primary)
                },
                new StackPanel
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Children =
                    {
                        Text(user.NumeComplet, TextBrush, 12, FontWeight.Bold),
                        Text(admin ? "Acces complet" : "Elev", Muted, 10)
                    }
                },
                IconText("ChevronDown", 11, Muted)
            }
        };
    }

    private static void SetButtonContentForeground(Button button, IBrush foreground)
    {
        if (button.Content is not StackPanel panel)
            return;

        foreach (var child in panel.Children)
        {
            if (child is TextBlock text)
                text.Foreground = foreground;
        }
    }

    private static string IconKeyForPage(string page) => page switch
    {
        "Dashboard" => "Dashboard",
        "Note" => "Note",
        "Teme" => "Teme",
        "Orar" => "Orar",
        "Admin" => "Admin",
        _ => page
    };

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

    private static StackPanel BrandTitle(double size)
        => BrandTitle("Agenda", "Agenda", size);

    private static StackPanel BrandTitle(string icon, string title, double size)
    {
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 9,
            Children =
            {
                IconText(icon, size, title == "Admin" ? Purple : Primary),
                Text(title, TextBrush, size, FontWeight.Bold)
            }
        };
    }

    private static TextBlock IconText(string icon, double size, IBrush color)
    {
        var useIconFont = icon != "@";
        return new TextBlock
        {
            Text = useIconFont ? IconGlyph(icon) : icon,
            Foreground = color,
            FontSize = size,
            FontFamily = useIconFont ? IconFont : FontFamily.Default,
            FontWeight = FontWeight.Normal,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center
        };
    }

    private static string IconGlyph(string name) => name switch
    {
        "Agenda" => "\uE82D",
        "Dashboard" => "\uE80F",
        "Note" => "\uE9D2",
        "Teme" => "\uE73A",
        "Orar" => "\uE787",
        "Admin" => "\uE83D",
        "Setari" => "\uE713",
        "Ajutor" => "\uE897",
        "User" => "\uE77B",
        "Parola" => "\uE72E",
        "Notificari" => "\uEA8F",
        "Activitate" => "\uE9D9",
        "Rapoarte" => "\uE9D2",
        "Aprobari" => "\uE8A5",
        "Alerta" => "\uEA39",
        "Ceas" => "\uE121",
        "Cautare" => "\uE721",
        "Filtru" => "\uE71C",
        "TrendUp" => "\uE8E5",
        "TrendDown" => "\uE8E3",
        "ChevronLeft" => "\uE973",
        "ChevronRight" => "\uE974",
        "ChevronDown" => "\uE70D",
        _ => name
    };

    private static readonly FontFamily IconFont = new("Segoe MDL2 Assets");

    private static BoxShadows SoftShadow => new(new BoxShadow
    {
        OffsetY = 10,
        Blur = 24,
        Color = Color.FromArgb(18, 15, 23, 42)
    });

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

    private static bool IsAdmin() => IsAdmin(AuthService.CurrentUser);
    private static bool IsAdmin(User? user) => string.Equals(user?.Rol, "Admin", StringComparison.OrdinalIgnoreCase);

    private static bool IsAdminPage(string page) => page is
        "Admin" or
        "Utilizatori" or
        "Materii" or
        "AdminOrar" or
        "Rapoarte" or
        "Aprobari" or
        "Suport";

    private static string PageTitle(string page) => page switch
    {
        "Admin" => "Panou Administrare",
        "AdminOrar" => "Orar & Calendar",
        "SetariSistem" => "Setari sistem",
        _ => page
    };

    private static string AdminPageSubtitle(string page) => page switch
    {
        "Admin" => "Gestioneaza platforma Agenda",
        "Utilizatori" => "Elevi, clase si situatia notelor",
        "Materii" => "Materii, note si medii centralizate",
        "AdminOrar" => "Programul saptamanal pentru elevi",
        "Rapoarte" => "Indicatori academici si statistici",
        "Aprobari" => "Teme in asteptare si actiuni rapide",
        "SetariSistem" => "Configurari rapide pentru aplicatie",
        "Suport" => "Informatii de suport si stare sistem",
        _ => "Administrare"
    };

    private static string IconFor(string tip) => tip switch
    {
        "nota" => "Note:",
        "tema" => "Teme:",
        "orar" => "Orar:",
        _ => "-"
    };

    private static bool ZiEquals(string left, string right) => NormalizeZi(left) == NormalizeZi(right);
    private static int IndexZi(string zi) => Array.IndexOf(Days, NormalizeZi(zi));

    private static string NormalizeZi(string zi)
    {
        var value = zi.Trim()
            .Replace("ă", "a").Replace("â", "a").Replace("î", "i").Replace("ș", "s").Replace("ş", "s").Replace("ț", "t").Replace("ţ", "t")
            .Replace("Ă", "A").Replace("Â", "A").Replace("Î", "I").Replace("Ș", "S").Replace("Ş", "S").Replace("Ț", "T").Replace("Ţ", "T")
            .ToLowerInvariant();

        if (value.StartsWith("lun")) return "Luni";
        if (value.StartsWith("mar")) return "Marti";
        if (value.StartsWith("mie")) return "Miercuri";
        if (value.StartsWith("joi")) return "Joi";
        if (value.StartsWith("vin")) return "Vineri";
        if (value.StartsWith("sam") || value.StartsWith("sâm")) return "Sambata";
        if (value.StartsWith("dum")) return "Duminica";

        return zi.Trim();
    }

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

    private sealed record ClassOption(int? Id, string Text)
    {
        public override string ToString() => Text;
    }

    private sealed record SubjectAverage(string Name, double Average, int Count);
    private sealed record Pair(string Left, string Right);
    private sealed record RowAction(int Id, string[] Values);
}
