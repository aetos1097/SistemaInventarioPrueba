using System.Windows.Forms;
using ProductsSales.WinForms.Services;

namespace ProductsSales.WinForms.Forms;

public partial class MainForm : Form
{
    private readonly AuthService _authService;
    private readonly ProductService _productService;
    private readonly SaleService _saleService;
    private WelcomeForm? _welcomeForm;
    private ToolStripMenuItem? _productsMenu;
    private ToolStripMenuItem? _salesMenu;
    private ToolStripMenuItem? _reportsMenu;
    private ToolStripMenuItem? _logoutMenu;
    private ToolStripMenuItem? _exitMenu;

    public MainForm(AuthService authService, ProductService productService, SaleService saleService)
    {
        _authService = authService;
        _productService = productService;
        _saleService = saleService;
        
        this.IsMdiContainer = true;
        InitializeComponent();
        AttachEvents();
        
        this.Text = $"Sistema de Productos y Ventas - Usuario: {authService.CurrentUsername}";
        
        // Muestra WelcomeForm al iniciar
        this.Load += MainForm_Load;
    }
    
    private void MainForm_Load(object? sender, EventArgs e)
    {
        ShowWelcomeForm();
    }
    
    private void ShowWelcomeForm()
    {
        if (_welcomeForm == null || _welcomeForm.IsDisposed)
        {
            _welcomeForm = new WelcomeForm(_authService.CurrentUsername ?? "Usuario");
            _welcomeForm.MdiParent = this;
            _welcomeForm.Show();
        }
        else
        {
            _welcomeForm.Show();
            _welcomeForm.BringToFront();
        }
    }
    
    private void HideWelcomeForm()
    {
        if (_welcomeForm != null && !_welcomeForm.IsDisposed)
        {
            _welcomeForm.Hide();
        }
    }
    
    private void ShowWelcomeIfNoChildren()
    {
        // Muestra bienvenida cuando no hay ventanas abiertas
        var otherChildren = this.MdiChildren.Where(f => f != _welcomeForm && !f.IsDisposed).Count();
        if (otherChildren == 0)
        {
            ShowWelcomeForm();
        }
    }

    private void InitializeComponent()
    {
        this.Size = new Size(1200, 700);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.WindowState = FormWindowState.Maximized;

        var menuStrip = new MenuStrip();

        _productsMenu = new ToolStripMenuItem("Productos");
        _salesMenu = new ToolStripMenuItem("Ventas");
        _reportsMenu = new ToolStripMenuItem("Reportes");
        _logoutMenu = new ToolStripMenuItem("Cerrar Sesión");
        _exitMenu = new ToolStripMenuItem("Salir");

        menuStrip.Items.Add(_productsMenu);
        menuStrip.Items.Add(_salesMenu);
        menuStrip.Items.Add(_reportsMenu);
        menuStrip.Items.Add(_logoutMenu);
        menuStrip.Items.Add(_exitMenu);

        this.MainMenuStrip = menuStrip;
        this.Controls.Add(menuStrip);
    }

    private void AttachEvents()
    {
        if (_productsMenu != null)
        {
            _productsMenu.Click += (s, e) =>
            {
                HideWelcomeForm();
                var form = new ProductsForm(_productService);
                form.MdiParent = this;
                form.FormClosed += (sender, args) => ShowWelcomeIfNoChildren();
                form.Show();
            };
        }

        if (_salesMenu != null)
        {
            _salesMenu.Click += (s, e) =>
            {
                if (_authService.CurrentUserId.HasValue)
                {
                    HideWelcomeForm();
                    var form = new SalesForm(_saleService, _productService, _authService.CurrentUserId.Value);
                    form.MdiParent = this;
                    form.FormClosed += (sender, args) => ShowWelcomeIfNoChildren();
                    form.Show();
                }
                else
                {
                    MessageBox.Show("No se ha identificado el usuario", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
        }

        if (_reportsMenu != null)
        {
            _reportsMenu.Click += (s, e) =>
            {
                HideWelcomeForm();
                var form = new ReportForm(_saleService);
                form.MdiParent = this;
                form.FormClosed += (sender, args) => ShowWelcomeIfNoChildren();
                form.Show();
            };
        }

        if (_logoutMenu != null)
        {
            _logoutMenu.Click += (s, e) =>
            {
                _authService.Logout();
                System.Windows.Forms.Application.Restart();
            };
        }

        if (_exitMenu != null)
        {
            _exitMenu.Click += (s, e) => System.Windows.Forms.Application.Exit();
        }
    }
}
