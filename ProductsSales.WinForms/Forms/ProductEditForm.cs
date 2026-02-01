using ProductsSales.Application.DTOs;
using ProductsSales.WinForms.Services;

namespace ProductsSales.WinForms.Forms;

public partial class ProductEditForm : Form
{
    private readonly ProductService _productService;
    private readonly ProductDto? _product;
    private TextBox? _txtName;
    private NumericUpDown? _numPrice;
    private NumericUpDown? _numStock;
    private TextBox? _txtImagePath;
    private Button? _btnSelectImage;
    private Button? _btnSave;

    public ProductEditForm(ProductService productService, ProductDto? product = null)
    {
        _productService = productService;
        _product = product;
        InitializeComponent();
        AttachEvents();
    }

    private void InitializeComponent()
    {
        this.Text = _product == null ? "Nuevo Producto" : "Editar Producto";
        this.Size = new Size(400, 300);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        var lblName = new Label { Text = "Nombre:", Location = new Point(20, 20), Size = new Size(100, 20) };
        _txtName = new TextBox { Location = new Point(130, 18), Size = new Size(220, 20) };

        var lblPrice = new Label { Text = "Precio:", Location = new Point(20, 60), Size = new Size(100, 20) };
        _numPrice = new NumericUpDown { Location = new Point(130, 58), Size = new Size(220, 20), DecimalPlaces = 2, Minimum = 0, Maximum = 999999 };

        var lblStock = new Label { Text = "Stock:", Location = new Point(20, 100), Size = new Size(100, 20) };
        _numStock = new NumericUpDown { Location = new Point(130, 98), Size = new Size(220, 20), Minimum = 0, Maximum = 999999 };

        var lblImage = new Label { Text = "Imagen:", Location = new Point(20, 140), Size = new Size(100, 20) };
        _txtImagePath = new TextBox { Location = new Point(130, 138), Size = new Size(150, 20), ReadOnly = true };
        _btnSelectImage = new Button { Text = "...", Location = new Point(290, 137), Size = new Size(30, 23) };

        _btnSave = new Button { Text = "Guardar", Location = new Point(130, 180), Size = new Size(100, 30) };
        var btnCancel = new Button { Text = "Cancelar", Location = new Point(240, 180), Size = new Size(100, 30), DialogResult = DialogResult.Cancel };

        if (_product != null)
        {
            _txtName.Text = _product.Name;
            _numPrice.Value = _product.Price;
            _numStock.Value = _product.Stock;
            _txtImagePath.Text = _product.ImagePath ?? string.Empty;
        }

        this.Controls.AddRange(new Control[] { lblName, _txtName, lblPrice, _numPrice, lblStock, _numStock, lblImage, _txtImagePath, _btnSelectImage, _btnSave, btnCancel });
        this.AcceptButton = _btnSave;
        this.CancelButton = btnCancel;
    }

    private void AttachEvents()
    {
        if (_btnSelectImage != null)
        {
            _btnSelectImage.Click += (s, e) =>
            {
                using var dialog = new OpenFileDialog
                {
                    Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.gif|Todos los archivos|*.*"
                };
                if (dialog.ShowDialog() == DialogResult.OK && _txtImagePath != null)
                {
                    _txtImagePath.Text = dialog.FileName;
                }
            };
        }

        if (_btnSave != null)
        {
            _btnSave.Click += async (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_txtName?.Text))
                {
                    MessageBox.Show("El nombre es requerido", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _btnSave.Enabled = false;
                try
                {
                    if (_product == null)
                    {
                        var createDto = new CreateProductDto(
                            _txtName.Text,
                            _numPrice?.Value ?? 0,
                            (int)(_numStock?.Value ?? 0),
                            string.IsNullOrWhiteSpace(_txtImagePath?.Text) ? null : _txtImagePath.Text
                        );
                        var result = await _productService.CreateAsync(createDto);
                        if (result != null)
                        {
                            MessageBox.Show("Producto creado correctamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Error al crear el producto", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        var updateDto = new UpdateProductDto(
                            _txtName.Text,
                            _numPrice?.Value ?? 0,
                            (int)(_numStock?.Value ?? 0),
                            string.IsNullOrWhiteSpace(_txtImagePath?.Text) ? null : _txtImagePath.Text
                        );
                        var result = await _productService.UpdateAsync(_product.Id, updateDto);
                        if (result != null)
                        {
                            MessageBox.Show("Producto actualizado correctamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Error al actualizar el producto", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
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
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    _btnSave.Enabled = true;
                }
            };
        }
    }
}

