using System.Windows.Forms;
using ProductsSales.WinForms.Forms;
using ProductsSales.WinForms.Services;

namespace ProductsSales.WinForms;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var apiClient = new ApiClient("http://localhost:5134");
        var authService = new AuthService(apiClient);
        var productService = new ProductService(apiClient);
        var saleService = new SaleService(apiClient);

        var loginForm = new LoginForm(authService);
        if (loginForm.ShowDialog() == DialogResult.OK)
        {
            System.Windows.Forms.Application.Run(new MainForm(authService, productService, saleService));
        }
    }
}