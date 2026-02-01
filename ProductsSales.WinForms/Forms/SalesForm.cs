using ProductsSales.Application.DTOs;
using ProductsSales.WinForms.Services;

namespace ProductsSales.WinForms.Forms;

public partial class SalesForm : Form
{
    private readonly SaleService _saleService;
    private readonly ProductService _productService;
    private readonly Guid _userId;
    private ComboBox? _cmbProducts;
    private NumericUpDown? _numQuantity;
    private DataGridView? _dgvItems;
    private List<SaleItemViewModel> _saleItems = new();
    private List<ProductDto>? _products;
    private Panel? _headerPanel;
    private Button? _btnAdd;
    private Button? _btnRemove;
    private Button? _btnRegister;

    public SalesForm(SaleService saleService, ProductService productService, Guid userId)
    {
        _saleService = saleService;
        _productService = productService;
        _userId = userId;
        InitializeComponent();
        AttachEvents();
        LoadHeaderImage();
        LoadProducts();
    }

    private void InitializeComponent()
    {
        this.Text = "Registrar Venta";
        this.Size = new Size(800, 600);
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
            Text = "Ventas",
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

        var lblProduct = new Label { Text = "Producto:", Location = new Point(20, 20), Size = new Size(80, 20) };
        _cmbProducts = new ComboBox { Location = new Point(100, 18), Size = new Size(300, 20), DropDownStyle = ComboBoxStyle.DropDownList };

        var lblQuantity = new Label { Text = "Cantidad:", Location = new Point(420, 20), Size = new Size(80, 20) };
        _numQuantity = new NumericUpDown { Location = new Point(500, 18), Size = new Size(100, 20), Minimum = 1, Maximum = 9999, Value = 1 };

        _btnAdd = new Button { Text = "Agregar Ítem", Location = new Point(620, 17), Size = new Size(120, 25) };
        _btnRemove = new Button { Text = "Quitar Ítem", Location = new Point(620, 47), Size = new Size(120, 25) };

        _dgvItems = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            MultiSelect = false,
            RowHeadersVisible = false,
            ColumnHeadersVisible = true,
            EnableHeadersVisualStyles = false,
            ColumnHeadersHeight = 35,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.Fixed3D,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Padding = new Padding(5)
            },
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Segoe UI", 9),
                SelectionBackColor = Color.FromArgb(0, 122, 204),
                SelectionForeColor = Color.White,
                Padding = new Padding(3)
            },
            AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(245, 245, 245)
            },
            RowTemplate = { Height = 30 }
        };

        var panelBottom = new Panel { Dock = DockStyle.Bottom, Height = 50 };

        var lblTotal = new Label { Text = "Total: $0.00", Location = new Point(20, 15), Size = new Size(200, 20), Font = new Font("Arial", 10, FontStyle.Bold), Name = "lblTotal" };
        _btnRegister = new Button { Text = "Registrar Venta", Location = new Point(600, 10), Size = new Size(150, 30) };

        panelTop.Controls.AddRange(new Control[] { lblProduct, _cmbProducts, lblQuantity, _numQuantity, _btnAdd, _btnRemove });
        panelBottom.Controls.AddRange(new Control[] { lblTotal, _btnRegister });

        this.Controls.Add(panelBottom);
        this.Controls.Add(_dgvItems);
        this.Controls.Add(panelTop);
        this.Controls.Add(_headerPanel);
    }

    private void AttachEvents()
    {
        if (_btnAdd != null) _btnAdd.Click += (s, e) => AddItem();
        
        if (_btnRemove != null)
        {
            _btnRemove.Click += (s, e) =>
            {
                if (_dgvItems?.SelectedRows.Count > 0)
                {
                    var index = _dgvItems.SelectedRows[0].Index;
                    _saleItems.RemoveAt(index);
                    RefreshItemsGrid();
                }
            };
        }

        if (_btnRegister != null) _btnRegister.Click += async (s, e) => await RegisterSale();
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

    private async void LoadProducts()
    {
        try
        {
            _products = await _productService.GetAllAsync();
            if (_cmbProducts != null)
            {
                _cmbProducts.DataSource = _products;
                _cmbProducts.DisplayMember = "Name";
                _cmbProducts.ValueMember = "Id";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar productos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AddItem()
    {
        if (_cmbProducts?.SelectedValue is Guid productId && _products != null)
        {
            var product = _products.FirstOrDefault(p => p.Id == productId);
            if (product == null) return;

            var quantity = (int)(_numQuantity?.Value ?? 1);

            if (quantity > product.Stock)
            {
                MessageBox.Show($"No hay suficiente stock. Stock disponible: {product.Stock}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var existingItem = _saleItems.FirstOrDefault(i => i.ProductId == productId);
            if (existingItem != null)
            {
                if (existingItem.Quantity + quantity > product.Stock)
                {
                    MessageBox.Show($"No hay suficiente stock. Stock disponible: {product.Stock}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                existingItem.Quantity += quantity;
                existingItem.LineTotal = existingItem.Quantity * existingItem.UnitPrice;
            }
            else
            {
                _saleItems.Add(new SaleItemViewModel
                {
                    ProductId = productId,
                    ProductName = product.Name,
                    Quantity = quantity,
                    UnitPrice = product.Price,
                    LineTotal = quantity * product.Price
                });
            }

            RefreshItemsGrid();
            if (_numQuantity != null)
            {
                _numQuantity.Value = 1;
            }
        }
    }

    private void RefreshItemsGrid()
    {
        if (_dgvItems != null)
        {
            _dgvItems.DataSource = null;
            _dgvItems.DataSource = _saleItems.ToList();
            
            // Configurar columnas en español
            if (_dgvItems.Columns.Count > 0)
            {
                if (_dgvItems.Columns.Contains("ProductId"))
                    _dgvItems.Columns["ProductId"].Visible = false;
                
                if (_dgvItems.Columns.Contains("ProductName"))
                    _dgvItems.Columns["ProductName"].HeaderText = "Producto";
                
                if (_dgvItems.Columns.Contains("Quantity"))
                    _dgvItems.Columns["Quantity"].HeaderText = "Cantidad";
                
                if (_dgvItems.Columns.Contains("UnitPrice"))
                {
                    _dgvItems.Columns["UnitPrice"].HeaderText = "Precio Unitario";
                    _dgvItems.Columns["UnitPrice"].DefaultCellStyle.Format = "C2";
                }
                
                if (_dgvItems.Columns.Contains("LineTotal"))
                {
                    _dgvItems.Columns["LineTotal"].HeaderText = "Total";
                    _dgvItems.Columns["LineTotal"].DefaultCellStyle.Format = "C2";
                }
            }

            var total = _saleItems.Sum(i => i.LineTotal);
            var lblTotal = this.Controls.Find("lblTotal", true).FirstOrDefault() as Label;
            if (lblTotal != null)
            {
                lblTotal.Text = $"Total: ${total:F2}";
            }
        }
    }

    private async Task RegisterSale()
    {
        if (_saleItems.Count == 0)
        {
            MessageBox.Show("Agregue al menos un ítem a la venta", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            var createSaleDto = new CreateSaleDto(
                _userId,
                _saleItems.Select(i => new CreateSaleItemDto(i.ProductId, i.Quantity)).ToList()
            );

            var result = await _saleService.CreateAsync(createSaleDto);
            if (result != null)
            {
                MessageBox.Show($"Venta registrada correctamente.\nID: {result.Id}\nTotal: ${result.Total:F2}", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _saleItems.Clear();
                RefreshItemsGrid();
                // Recarga productos
                LoadProducts();
            }
            else
            {
                MessageBox.Show("Error al registrar la venta. La respuesta del servidor fue nula.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (UnauthorizedAccessException)
        {
            MessageBox.Show("Sesión expirada. Por favor inicie sesión nuevamente", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al registrar la venta:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private class SaleItemViewModel
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}

