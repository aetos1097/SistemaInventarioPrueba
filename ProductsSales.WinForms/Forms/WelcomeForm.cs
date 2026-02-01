namespace ProductsSales.WinForms.Forms;

public partial class WelcomeForm : Form
{
    private Panel? _textPanel;

    public WelcomeForm(string username)
    {
        InitializeComponent(username);
        AttachEvents();
        LoadBackgroundImage();
    }

    private void InitializeComponent(string username)
    {
        this.Text = "Bienvenido";
        this.FormBorderStyle = FormBorderStyle.None;
        this.WindowState = FormWindowState.Maximized;
        this.BackColor = Color.FromArgb(30, 60, 40);

        // Panel central
        _textPanel = new Panel
        {
            Size = new Size(500, 120),
            BackColor = Color.FromArgb(180, 30, 50, 35)
        };

        var lblWelcome = new Label
        {
            Text = "Bienvenido, " + username,
            Font = new Font("Georgia", 28, FontStyle.Bold | FontStyle.Italic),
            ForeColor = Color.FromArgb(218, 165, 32),
            AutoSize = false,
            Size = new Size(480, 50),
            Location = new Point(10, 15),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent
        };

        var lblSubtitle = new Label
        {
            Text = "Sistema de Productos y Ventas",
            Font = new Font("Segoe UI", 16),
            ForeColor = Color.FromArgb(218, 165, 32),
            AutoSize = false,
            Size = new Size(480, 35),
            Location = new Point(10, 70),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent
        };

        _textPanel.Controls.Add(lblWelcome);
        _textPanel.Controls.Add(lblSubtitle);

        this.Controls.Add(_textPanel);
    }

    private void AttachEvents()
    {
        this.Resize += (s, e) =>
        {
            if (_textPanel != null)
            {
                _textPanel.Location = new Point(
                    (this.ClientSize.Width - _textPanel.Width) / 2,
                    (this.ClientSize.Height - _textPanel.Height) / 2
                );
            }
        };
    }

    private void LoadBackgroundImage()
    {
        var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "background.png");
        if (File.Exists(imagePath))
        {
            try
            {
                this.BackgroundImage = Image.FromFile(imagePath);
                this.BackgroundImageLayout = ImageLayout.Stretch;
            }
            catch { }
        }
    }
}
