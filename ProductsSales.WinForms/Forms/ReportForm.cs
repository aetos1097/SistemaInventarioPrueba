using ProductsSales.Application.DTOs;
using ProductsSales.WinForms.Services;

namespace ProductsSales.WinForms.Forms;

public partial class ReportForm : Form
{
    private readonly SaleService _saleService;
    private DataGridView? _dgvSales;
    private Label? _lblTotal;
    private Label? _lblPageInfo;
    private Button? _btnFirst;
    private Button? _btnPrev;
    private Button? _btnNext;
    private Button? _btnLast;
    private ComboBox? _cmbPageSize;
    private DateTimePicker? _dtpFrom;
    private DateTimePicker? _dtpTo;
    private Panel? _headerPanel;
    private Button? _btnQuery;
    
    private int _currentPage = 1;
    private int _pageSize = 10;
    private int _totalPages = 1;

    public ReportForm(SaleService saleService)
    {
        _saleService = saleService;
        InitializeComponent();
        AttachEvents();
        LoadHeaderImage();
    }

    private void InitializeComponent()
    {
        this.Text = "Reporte de Ventas";
        this.Size = new Size(1000, 700);
        this.StartPosition = FormStartPosition.CenterScreen;

        // Header (imagen en LoadHeaderImage)
        _headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 120,
            BackColor = Color.FromArgb(30, 60, 40),
            Name = "headerPanel"
        };

        var titlePanel = new Panel
        {
            Size = new Size(400, 50),
            BackColor = Color.FromArgb(200, 30, 50, 35),
            Location = new Point(20, 35)
        };

        var lblTitle = new Label
        {
            Text = "Reportes",
            Font = new Font("Georgia", 22, FontStyle.Bold | FontStyle.Italic),
            ForeColor = Color.FromArgb(218, 165, 32),
            AutoSize = false,
            Size = new Size(380, 45),
            Location = new Point(10, 2),
            TextAlign = ContentAlignment.MiddleLeft,
            BackColor = Color.Transparent
        };

        titlePanel.Controls.Add(lblTitle);
        _headerPanel.Controls.Add(titlePanel);

        var panelTop = new Panel { Dock = DockStyle.Top, Height = 80 };

        var lblFrom = new Label { Text = "Desde:", Location = new Point(20, 20), Size = new Size(60, 20) };
        _dtpFrom = new DateTimePicker { Location = new Point(90, 18), Size = new Size(150, 20), Value = DateTime.Now.AddDays(-30) };

        var lblTo = new Label { Text = "Hasta:", Location = new Point(260, 20), Size = new Size(60, 20) };
        _dtpTo = new DateTimePicker { Location = new Point(330, 18), Size = new Size(150, 20), Value = DateTime.Now };

        var lblPageSize = new Label { Text = "Registros:", Location = new Point(500, 20), Size = new Size(70, 20) };
        _cmbPageSize = new ComboBox 
        { 
            Location = new Point(575, 17), 
            Size = new Size(60, 25), 
            DropDownStyle = ComboBoxStyle.DropDownList 
        };
        _cmbPageSize.Items.AddRange(new object[] { "5", "10", "20", "50" });
        _cmbPageSize.SelectedIndex = 1; // 10 por defecto

        _btnQuery = new Button { Text = "Consultar", Location = new Point(650, 17), Size = new Size(100, 30) };

        _dgvSales = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            RowHeadersVisible = false,
            ColumnHeadersVisible = true,
            EnableHeadersVisualStyles = false,
            ColumnHeadersHeight = 35,
            BackgroundColor = Color.White,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            },
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Segoe UI", 9),
                SelectionBackColor = Color.FromArgb(0, 122, 204),
                SelectionForeColor = Color.White
            },
            AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(245, 245, 245)
            },
            RowTemplate = { Height = 28 }
        };

        var panelBottom = new Panel { Dock = DockStyle.Bottom, Height = 50 };
        _lblTotal = new Label
        {
            Text = "Total Vendido: $0.00",
            Location = new Point(20, 15),
            Size = new Size(250, 20),
            Font = new Font("Arial", 10, FontStyle.Bold)
        };

        _btnFirst = new Button { Text = "<<", Location = new Point(500, 10), Size = new Size(40, 30), Enabled = false };
        _btnPrev = new Button { Text = "<", Location = new Point(545, 10), Size = new Size(40, 30), Enabled = false };
        _lblPageInfo = new Label 
        { 
            Text = "Página 0 de 0", 
            Location = new Point(590, 15), 
            Size = new Size(150, 20),
            TextAlign = ContentAlignment.MiddleCenter
        };
        _btnNext = new Button { Text = ">", Location = new Point(745, 10), Size = new Size(40, 30), Enabled = false };
        _btnLast = new Button { Text = ">>", Location = new Point(790, 10), Size = new Size(40, 30), Enabled = false };

        panelTop.Controls.AddRange(new Control[] { lblFrom, _dtpFrom, lblTo, _dtpTo, lblPageSize, _cmbPageSize, _btnQuery });
        panelBottom.Controls.AddRange(new Control[] { _lblTotal, _btnFirst, _btnPrev, _lblPageInfo, _btnNext, _btnLast });

        this.Controls.Add(panelBottom);
        this.Controls.Add(_dgvSales);
        this.Controls.Add(panelTop);
        this.Controls.Add(_headerPanel);
    }

    private void AttachEvents()
    {
        if (_cmbPageSize != null)
        {
            _cmbPageSize.SelectedIndexChanged += (s, e) => 
            {
                _pageSize = int.Parse(_cmbPageSize.SelectedItem?.ToString() ?? "10");
                _currentPage = 1;
            };
        }

        if (_btnFirst != null) _btnFirst.Click += async (s, e) => { _currentPage = 1; await LoadReport(); };
        if (_btnPrev != null) _btnPrev.Click += async (s, e) => { if (_currentPage > 1) _currentPage--; await LoadReport(); };
        if (_btnNext != null) _btnNext.Click += async (s, e) => { if (_currentPage < _totalPages) _currentPage++; await LoadReport(); };
        if (_btnLast != null) _btnLast.Click += async (s, e) => { _currentPage = _totalPages; await LoadReport(); };
        if (_btnQuery != null) _btnQuery.Click += async (s, e) => { _currentPage = 1; await LoadReport(); };
    }

    private void LoadHeaderImage()
    {
        if (_headerPanel == null) return;
        var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "header.png");
        if (File.Exists(imagePath))
        {
            try
            {
                _headerPanel.BackgroundImage = Image.FromFile(imagePath);
                _headerPanel.BackgroundImageLayout = ImageLayout.Stretch;
            }
            catch { }
        }
    }

    private async Task LoadReport()
    {
        if (_dtpFrom == null || _dtpTo == null) return;
        
        try
        {
            var report = await _saleService.GetReportAsync(_dtpFrom.Value, _dtpTo.Value, _currentPage, _pageSize);
            
            if (_dgvSales == null || _lblTotal == null || _lblPageInfo == null) return;

            if (report == null || report.Sales == null || report.Sales.Count == 0)
            {
                _dgvSales.DataSource = null;
                _lblTotal.Text = "Total Vendido: $0.00";
                UpdatePaginationControls(0, 0);
                if (_currentPage == 1)
                {
                    MessageBox.Show("No se encontraron ventas en el rango de fechas seleccionado", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return;
            }

            _totalPages = report.TotalPages;
            _dgvSales.DataSource = report.Sales;
            ConfigureReportColumns();
            _lblTotal.Text = $"Total Vendido: ${report.TotalSold:N2} ({report.TotalRecords} registros)";
            UpdatePaginationControls(report.CurrentPage, report.TotalPages);
        }
        catch (UnauthorizedAccessException)
        {
            MessageBox.Show("Sesión expirada. Por favor inicie sesión nuevamente", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar el reporte: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void UpdatePaginationControls(int currentPage, int totalPages)
    {
        if (_lblPageInfo == null || _btnFirst == null || _btnPrev == null || _btnNext == null || _btnLast == null) return;
        
        _lblPageInfo.Text = $"Página {currentPage} de {totalPages}";
        
        _btnFirst.Enabled = currentPage > 1;
        _btnPrev.Enabled = currentPage > 1;
        _btnNext.Enabled = currentPage < totalPages;
        _btnLast.Enabled = currentPage < totalPages;
    }

    private void ConfigureReportColumns()
    {
        if (_dgvSales == null || _dgvSales.Columns.Count == 0) return;

        // Columnas ocultas
        if (_dgvSales.Columns.Contains("Id"))
            _dgvSales.Columns["Id"].Visible = false;

        if (_dgvSales.Columns.Contains("UserId"))
            _dgvSales.Columns["UserId"].Visible = false;

        if (_dgvSales.Columns.Contains("Items"))
            _dgvSales.Columns["Items"].Visible = false;

        // Encabezados
        if (_dgvSales.Columns.Contains("Date"))
        {
            _dgvSales.Columns["Date"].HeaderText = "Fecha";
            _dgvSales.Columns["Date"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
        }

        if (_dgvSales.Columns.Contains("Total"))
        {
            _dgvSales.Columns["Total"].HeaderText = "Total";
            _dgvSales.Columns["Total"].DefaultCellStyle.Format = "C2";
            _dgvSales.Columns["Total"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        if (_dgvSales.Columns.Contains("Username"))
        {
            _dgvSales.Columns["Username"].HeaderText = "Usuario";
        }
    }
}

