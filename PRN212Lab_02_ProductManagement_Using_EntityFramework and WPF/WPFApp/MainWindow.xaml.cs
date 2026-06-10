using System;
using System.Windows;
using System.Windows.Controls;
using BusinessObjects;
using Services;

namespace WPFApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly IProductService iProductService;
    private readonly ICategoryService iCategoryService;

    public MainWindow()
    {
        InitializeComponent();
        iProductService = new ProductService();
        iCategoryService = new CategoryService();
    }

    public void LoadCategoryList()
    {
        try
        {
            var catList = iCategoryService.GetCategories();
            cboCategory.ItemsSource = catList;
            cboCategory.DisplayMemberPath = "CategoryName";
            cboCategory.SelectedValuePath = "CategoryId";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error on load list of categories");
        }
    }

    public void LoadProductList()
    {
        try
        {
            var productList = iProductService.GetProducts();
            dgData.ItemsSource = productList;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error on load list of products");
        }
        finally
        {
            resetInput();
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        LoadCategoryList();
        LoadProductList();
    }

    /// <summary>
    /// Đọc input UI và build Product. Trả về false + báo lỗi cụ thể nếu invalid.
    /// </summary>
    private bool TryReadInput(out Product product, bool requireId)
    {
        product = new Product();

        if (requireId)
        {
            if (!int.TryParse(txtProductID.Text, out int pid))
            {
                MessageBox.Show("You must select a Product !");
                return false;
            }
            product.ProductId = pid;
        }

        if (string.IsNullOrWhiteSpace(txtProductName.Text))
        {
            MessageBox.Show("Product Name không được rỗng.");
            return false;
        }
        product.ProductName = txtProductName.Text.Trim();

        if (!decimal.TryParse(txtPrice.Text, out decimal price) || price < 0)
        {
            MessageBox.Show("Price phải là số ≥ 0.");
            return false;
        }
        product.UnitPrice = price;

        if (!short.TryParse(txtUnitsInStock.Text, out short stock) || stock < 0)
        {
            MessageBox.Show("Units In Stock phải là số nguyên ≥ 0.");
            return false;
        }
        product.UnitsInStock = stock;

        if (cboCategory.SelectedValue is null
            || !int.TryParse(cboCategory.SelectedValue.ToString(), out int catId))
        {
            MessageBox.Show("Vui lòng chọn Category.");
            return false;
        }
        product.CategoryId = catId;

        return true;
    }

    private void btnCreate_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!TryReadInput(out Product product, requireId: false)) return;
            iProductService.SaveProduct(product);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
        finally
        {
            LoadProductList();
        }
    }

    private void dgData_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not DataGrid dataGrid || dataGrid.SelectedItem is not Product selected)
            return;

        var product = iProductService.GetProductById(selected.ProductId);
        if (product == null) return;

        txtProductID.Text = product.ProductId.ToString();
        txtProductName.Text = product.ProductName;
        txtPrice.Text = product.UnitPrice?.ToString();
        txtUnitsInStock.Text = product.UnitsInStock?.ToString();
        cboCategory.SelectedValue = product.CategoryId;
    }

    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void btnUpdate_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!TryReadInput(out Product product, requireId: true)) return;
            iProductService.UpdateProduct(product);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
        finally
        {
            LoadProductList();
        }
    }

    private void btnDelete_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!int.TryParse(txtProductID.Text, out int productId))
            {
                MessageBox.Show("You must select a Product !");
                return;
            }
            iProductService.DeleteProduct(productId);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
        finally
        {
            LoadProductList();
        }
    }

    private void resetInput()
    {
        txtProductID.Text = "";
        txtProductName.Text = "";
        txtPrice.Text = "";
        txtUnitsInStock.Text = "";
        cboCategory.SelectedIndex = -1;
    }
}
