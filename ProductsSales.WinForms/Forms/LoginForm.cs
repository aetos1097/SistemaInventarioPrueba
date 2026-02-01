using ProductsSales.WinForms.Services;

namespace ProductsSales.WinForms.Forms;

public partial class LoginForm : Form
{
    private readonly AuthService _authService;
    private TextBox? _txtUsername;
    private TextBox? _txtPassword;
    private Button? _btnLogin;

    public LoginForm(AuthService authService)
    {
        _authService = authService;
        InitializeComponent();
        AttachEvents();
    }

    private void InitializeComponent()
    {
        this.Text = "Ingreso";
        this.Size = new Size(450, 280);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.BackColor = Color.FromArgb(250, 248, 240);

        // Header
        var panelHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 50,
            BackColor = Color.FromArgb(85, 107, 85)
        };

        var lblTitle = new Label
        {
            Text = "Ingreso",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            Location = new Point(15, 12),
            AutoSize = true
        };
        panelHeader.Controls.Add(lblTitle);

        var lblUsername = new Label
        {
            Text = "Usuario:",
            Location = new Point(30, 80),
            Size = new Size(90, 25),
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(60, 60, 60)
        };

        var lblPassword = new Label
        {
            Text = "Contraseña:",
            Location = new Point(30, 130),
            Size = new Size(100, 25),
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(60, 60, 60)
        };

        _txtUsername = new TextBox
        {
            Location = new Point(130, 77),
            Size = new Size(280, 30),
            Name = "txtUsername",
            Font = new Font("Segoe UI", 11),
            BorderStyle = BorderStyle.FixedSingle
        };

        _txtPassword = new TextBox
        {
            Location = new Point(130, 127),
            Size = new Size(280, 30),
            PasswordChar = '*',
            Name = "txtPassword",
            Font = new Font("Segoe UI", 11),
            BorderStyle = BorderStyle.FixedSingle
        };

        _btnLogin = new Button
        {
            Text = "Ingresar",
            Location = new Point(130, 185),
            Size = new Size(130, 40),
            DialogResult = DialogResult.None,
            BackColor = Color.FromArgb(85, 107, 85),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        _btnLogin.FlatAppearance.BorderSize = 0;

        var btnCancel = new Button
        {
            Text = "Cancelar",
            Location = new Point(280, 185),
            Size = new Size(130, 40),
            DialogResult = DialogResult.Cancel,
            BackColor = Color.FromArgb(230, 225, 210),
            ForeColor = Color.FromArgb(85, 107, 85),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10),
            Cursor = Cursors.Hand
        };
        btnCancel.FlatAppearance.BorderColor = Color.FromArgb(85, 107, 85);
        btnCancel.FlatAppearance.BorderSize = 1;

        this.Controls.Add(panelHeader);
        this.Controls.Add(lblUsername);
        this.Controls.Add(_txtUsername);
        this.Controls.Add(lblPassword);
        this.Controls.Add(_txtPassword);
        this.Controls.Add(_btnLogin);
        this.Controls.Add(btnCancel);

        this.AcceptButton = _btnLogin;
        this.CancelButton = btnCancel;
    }

    private void AttachEvents()
    {
        if (_btnLogin != null)
        {
            _btnLogin.Click += async (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_txtUsername?.Text))
                {
                    MessageBox.Show("Por favor ingrese el nombre de usuario", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(_txtPassword?.Text))
                {
                    MessageBox.Show("Por favor ingrese la contraseña", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _btnLogin.Enabled = false;

                try
                {
                    var success = await _authService.LoginAsync(_txtUsername.Text, _txtPassword.Text);

                    if (success)
                    {
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Usuario o contraseña incorrectos", "Error de autenticación", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al conectar con el servidor: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    _btnLogin.Enabled = true;
                }
            };
        }
    }
}

