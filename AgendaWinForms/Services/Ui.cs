using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AgendaWinForms.Services;

public static class Ui
{
    // Culori extrase direct din design-ul image_55273b.jpg
    public static readonly Color Primary = Color.FromArgb(37, 110, 215); // Albastru buton
    public static readonly Color Background = Color.FromArgb(252, 253, 255);
    public static readonly Color Sidebar = Color.White;
    public static readonly Color Border = Color.FromArgb(226, 232, 240);
    public static readonly Color BorderSuccess = Color.FromArgb(200, 230, 201); // Verdele deschis
    public static readonly Color Muted = Color.FromArgb(113, 128, 150);
    public static readonly Color Success = Color.FromArgb(25, 135, 84);
    public static readonly Color Warning = Color.FromArgb(253, 126, 20);
    public static readonly Color Danger = Color.FromArgb(220, 53, 69);
    public static readonly Color Purple = Color.FromArgb(111, 66, 193);
    public static readonly Color LightGrayBg = Color.FromArgb(248, 249, 250);

    public static Font Font(float size, FontStyle style = FontStyle.Regular) => new("Segoe UI", size, style);

    public static Panel Card(int x, int y, int width, int height)
    {
        return new Panel
        {
            Location = new Point(x, y),
            Size = new Size(width, height),
            BackColor = Color.White,
            Padding = new Padding(20)
        };
    }

    public static Label Label(string text, int x, int y, int width, int height, float size = 10, FontStyle style = FontStyle.Regular, Color? color = null)
    {
        return new Label
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(width, height),
            Font = Font(size, style),
            ForeColor = color ?? Color.FromArgb(45, 55, 72),
            BackColor = Color.Transparent
        };
    }

    // Buton cu stil modern
    public static Button Button(string text, int x, int y, int width, int height, Color? fill = null, Color? fore = null)
    {
        var btn = new Button
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(width, height),
            FlatStyle = FlatStyle.Flat,
            BackColor = fill ?? Primary,
            ForeColor = fore ?? Color.White,
            Font = Font(10, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }

    // Input stilizat cu fundal gri deschis
    public static TextBox TextBox(string placeholder, int x, int y, int width, bool password = false)
    {
        return new TextBox
        {
            PlaceholderText = placeholder,
            Location = new Point(x, y),
            Size = new Size(width, 36),
            Font = Font(10),
            UseSystemPasswordChar = password,
            BorderStyle = BorderStyle.None, // bordura 
            BackColor = LightGrayBg
        };
    }

    
    public static Panel InputGroup(string labelText, string placeholder, int x, int y, string icon)
    {
        var container = new Panel { Location = new Point(x, y), Size = new Size(320, 65), BackColor = Color.Transparent };
        
        container.Controls.Add(new Label { 
            Text = labelText, 
            Location = new Point(0, 0), 
            AutoSize = true, 
            Font = Font(9, FontStyle.Bold), 
            ForeColor = Color.FromArgb(74, 85, 104) 
        });

        var inputField = new Panel {
            Location = new Point(0, 22),
            Size = new Size(310, 38),
            BackColor = LightGrayBg,
            Padding = new Padding(8, 6, 8, 6)
        };

        // Iconita
        inputField.Controls.Add(new Label { 
            Text = icon, 
            Location = new Point(5, 8), 
            Width = 25, 
            Font = Font(11), 
            ForeColor = Muted 
        });

        var txt = new TextBox {
            Text = placeholder,
            Location = new Point(35, 9),
            Width = 240,
            BorderStyle = BorderStyle.None,
            BackColor = LightGrayBg,
            Font = Font(10),
            ForeColor = Color.DimGray
        };
        
        inputField.Controls.Add(txt);
        container.Controls.Add(inputField);

        // bordura gri 
        inputField.Paint += (s, e) => {
            using var pen = new Pen(Color.FromArgb(226, 232, 240));
            e.Graphics.DrawRectangle(pen, 0, 0, inputField.Width - 1, inputField.Height - 1);
        };

        return container;
    }

    public static DataGridView Grid()
    {
        return new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
    }

    public static void PaintRoundedBorder(object? sender, PaintEventArgs e)
    {
        if (sender is not Control control) return;
        
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        
        
        Color borderColor = control.Tag?.ToString() == "SuccessCard" ? BorderSuccess : Border;
        
        using var path = RoundedRectangle(control.ClientRectangle, 12);
        using var pen = new Pen(borderColor, 2);
        e.Graphics.DrawPath(pen, path);
    }

    public static GraphicsPath RoundedRectangle(Rectangle rectangle, int radius)
    {
        var path = new GraphicsPath();
        int diameter = radius * 2;
        Rectangle arc = new Rectangle(rectangle.X, rectangle.Y, diameter, diameter);

        path.AddArc(arc, 180, 90);
        arc.X = rectangle.Right - diameter - 1;
        path.AddArc(arc, 270, 90);
        arc.Y = rectangle.Bottom - diameter - 1;
        path.AddArc(arc, 0, 90);
        arc.X = rectangle.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();
        return path;
    }
}
