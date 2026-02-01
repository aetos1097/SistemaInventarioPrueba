using ProductsSales.Application.DTOs;
using ProductsSales.WinForms.Services;

namespace ProductsSales.WinForms.Forms;

public partial class ProductsForm : Form
{
    private readonly ProductService _productService;
    private DataGridView? _dgvProducts;
    private Panel? _headerPanel;
    private Button? _btnNew;
    private Button? _btnEdit;
    private Button? _btnDelete;
    private Button? _btnRefresh;

    public ProductsForm(ProductService productService)
    {
        _productService = productService;
        InitializeComponent();
        AttachEvents();
        LoadHeaderImage();
        LoadProducts();
    }

    private void InitializeComponent()
    {
        this.Text = "Gestión de Productos";
        this.Size = new Size(900, 600);
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
            Text = "Productos",
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

        _dgvProducts = new DataGridView
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

        var panel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 50,
            BackColor = Color.FromArgb(240, 240, 240)
        };

        _btnNew = new Button
        {
            Text = "Nuevo",
            Location = new Point(10, 10),
            Size = new Size(100, 30)
        };

        _btnEdit = new Button
        {
            Text = "Editar",
            Location = new Point(120, 10),
            Size = new Size(100, 30)
        };

        _btnDelete = new Button
        {
            Text = "Eliminar",
            Location = new Point(230, 10),
            Size = new Size(100, 30)
        };

        _btnRefresh = new Button
        {
            Text = "Actualizar",
            Location = new Point(340, 10),
            Size = new Size(100, 30)
        };

        panel.Controls.Add(_btnNew);
        panel.Controls.Add(_btnEdit);
        panel.Controls.Add(_btnDelete);
        panel.Controls.Add(_btnRefresh);

        // Dock: Fill primero, luego Top
        this.Controls.Add(_dgvProducts);
        this.Controls.Add(panel);
        this.Controls.Add(_headerPanel);
    }

    private void AttachEvents()
    {
        if (_btnNew != null) _btnNew.Click += (s, e) => ShowProductDialog(null);
        if (_btnRefresh != null) _btnRefresh.Click += (s, e) => LoadProducts();

        if (_btnEdit != null)
        {
            _btnEdit.Click += (s, e) =>
            {
                if (_dgvProducts?.SelectedRows.Count > 0)
                {
                    var product = _dgvProducts.SelectedRows[0].DataBoundItem as ProductDto;
                    if (product != null)
                        ShowProductDialog(product);
                    else
                        MessageBox.Show("Seleccione un producto para editar", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                    MessageBox.Show("Seleccione un producto para editar", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
        }

        if (_btnDelete != null)
        {
            _btnDelete.Click += async (s, e) =>
            {
                if (_dgvProducts?.SelectedRows.Count > 0)
                {
                    var product = _dgvProducts.SelectedRows[0].DataBoundItem as ProductDto;
                    if (product != null)
                    {
                        if (MessageBox.Show("¿Está seguro de eliminar el producto " + product.Name + "?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            try
                            {
                                await _productService.DeleteAsync(product.Id);
                                MessageBox.Show("Producto eliminado correctamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadProducts();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Error al eliminar el producto: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                else
                    MessageBox.Show("Seleccione un producto para eliminar", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
        }
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
            var products = await _productService.GetAllAsync();
            if (_dgvProducts != null)
            {
                _dgvProducts.DataSource = products;
                ConfigureColumns();
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
            MessageBox.Show($"Error al cargar productos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ConfigureColumns()
    {
        if (_dgvProducts == null || _dgvProducts.Columns.Count == 0) return;

        // Oculta Id
        if (_dgvProducts.Columns.Contains("Id"))
            _dgvProducts.Columns["Id"].Visible = false;

        // Nombres de columnas
        if (_dgvProducts.Columns.Contains("Name"))
        {
            _dgvProducts.Columns["Name"].HeaderText = "Nombre";
            _dgvProducts.Columns["Name"].FillWeight = 25;
        }

        if (_dgvProducts.Columns.Contains("Price"))
        {
            _dgvProducts.Columns["Price"].HeaderText = "Precio";
            _dgvProducts.Columns["Price"].DefaultCellStyle.Format = "C2";
            _dgvProducts.Columns["Price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _dgvProducts.Columns["Price"].FillWeight = 15;
        }

        if (_dgvProducts.Columns.Contains("Stock"))
        {
            _dgvProducts.Columns["Stock"].HeaderText = "Stock";
            _dgvProducts.Columns["Stock"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _dgvProducts.Columns["Stock"].FillWeight = 10;
        }

        if (_dgvProducts.Columns.Contains("ImagePath"))
        {
            _dgvProducts.Columns["ImagePath"].HeaderText = "Imagen";
            _dgvProducts.Columns["ImagePath"].FillWeight = 20;
        }

        if (_dgvProducts.Columns.Contains("CreatedAt"))
        {
            _dgvProducts.Columns["CreatedAt"].HeaderText = "Fecha Creación";
            _dgvProducts.Columns["CreatedAt"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            _dgvProducts.Columns["CreatedAt"].FillWeight = 15;
        }

        if (_dgvProducts.Columns.Contains("UpdatedAt"))
        {
            _dgvProducts.Columns["UpdatedAt"].HeaderText = "Última Actualización";
            _dgvProducts.Columns["UpdatedAt"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            _dgvProducts.Columns["UpdatedAt"].FillWeight = 15;
        }
    }

    private void ShowProductDialog(ProductDto? product)
    {
        var form = new ProductEditForm(_productService, product);
        if (form.ShowDialog() == DialogResult.OK)
        {
            LoadProducts();
        }
    }
}

