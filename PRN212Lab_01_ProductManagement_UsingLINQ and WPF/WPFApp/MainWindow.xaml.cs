using BusinessObjects;
using Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using WPFApp.Dialogs;
using WPFApp.Mvvm;

namespace WPFApp
{
    public partial class MainWindow : Window
    {
        private readonly IProductService iProductService;
        private readonly IAccountService iAccountService;
        private readonly AccountMember? currentUser;
        private readonly MainViewModel vm;
        private readonly DispatcherTimer clock;

        public MainWindow() : this(null) { }

        public MainWindow(AccountMember? user)
        {
            InitializeComponent();
            iProductService = new ProductService();
            iAccountService = new AccountService();
            currentUser = user;

            vm = new MainViewModel(iProductService, new CategoryService());
            DataContext = vm;

            clock = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            clock.Tick += (_, __) => statusTime.Text = DateTime.Now.ToString("HH:mm:ss");
            clock.Start();
            statusTime.Text = DateTime.Now.ToString("HH:mm:ss");

            // Keyboard shortcut: R = reload
            KeyDown += async (_, e) =>
            {
                if (e.Key == Key.R && Keyboard.Modifiers == ModifierKeys.None &&
                    !(Keyboard.FocusedElement is TextBox || Keyboard.FocusedElement is PasswordBox))
                    await ReloadAsync();
            };
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtCurrentUser.Text = currentUser?.FullName ?? "Guest";
            if (currentUser != null && currentUser.MemberRole != Role.Admin)
                btnManageMembers.Visibility = Visibility.Collapsed;

            await ReloadAsync();
            cboCategory.ItemsSource = vm.Categories;
            cboFilterCategory.ItemsSource = vm.Categories;
        }

        private async Task ReloadAsync()
        {
            loadingOverlay.Visibility = Visibility.Visible;
            try
            {
                await vm.ReloadAsync();
                UpdateStatus();
                UpdateEmptyState();
            }
            finally { loadingOverlay.Visibility = Visibility.Collapsed; }
        }

        private void UpdateStatus()
        {
            statusCount.Text = $"{vm.TotalCount} items";
            statusFilter.Text = $"{vm.FilteredCount} shown";
            sideCount.Text = vm.TotalCount.ToString();
            txtSubtitle.Text = $"Manage your inventory · {vm.FilteredCount} of {vm.TotalCount} products";
        }

        private void UpdateEmptyState()
        {
            emptyState.Visibility = vm.FilteredCount == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private bool ValidateForm(bool isUpdate, out int? productId, out string name, out decimal price, out short stock, out int categoryId)
        {
            productId = null; name = ""; price = 0; stock = 0; categoryId = 0;
            HideFormError();

            if (isUpdate)
            {
                if (string.IsNullOrWhiteSpace(txtProductID.Text)) { ShowFormError("Select a product to update."); return false; }
                if (!int.TryParse(txtProductID.Text, out var id)) { ShowFormError("Invalid product ID."); return false; }
                productId = id;
            }
            if (string.IsNullOrWhiteSpace(txtProductName.Text)) { ShowFormError("Name is required."); return false; }
            if (!decimal.TryParse(txtPrice.Text, out var p) || p <= 0) { ShowFormError("Price must be positive."); return false; }
            if (!short.TryParse(txtUnitsInStock.Text, out var s) || s < 0) { ShowFormError("Stock must be >= 0."); return false; }
            if (cboCategory.SelectedValue == null) { ShowFormError("Pick a category."); return false; }

            name = txtProductName.Text.Trim();
            price = p; stock = s; categoryId = Convert.ToInt32(cboCategory.SelectedValue);
            return true;
        }

        private async void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm(false, out _, out var name, out var price, out var stock, out var catId)) return;

            if (vm.Products.Any(p => string.Equals(p.ProductName?.Trim(), name, StringComparison.OrdinalIgnoreCase)))
            { ShowFormError("Product name already exists."); return; }

            try
            {
                iProductService.SaveProduct(new Product { ProductName = name, UnitPrice = price, UnitsInStock = stock, CategoryId = catId });
                AppDialog.Success("Product created.", owner: this);
                ResetInput();
                await ReloadAsync();
            }
            catch (Exception ex) { AppDialog.Error(ex.Message, owner: this); }
        }

        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm(true, out var productId, out var name, out var price, out var stock, out var catId)) return;

            if (vm.Products.Any(p => p.ProductId != productId &&
                                     string.Equals(p.ProductName?.Trim(), name, StringComparison.OrdinalIgnoreCase)))
            { ShowFormError("Product name already exists."); return; }

            try
            {
                iProductService.UpdateProduct(new Product { ProductId = productId!.Value, ProductName = name, UnitPrice = price, UnitsInStock = stock, CategoryId = catId });
                AppDialog.Success("Product updated.", owner: this);
                ResetInput();
                await ReloadAsync();
            }
            catch (Exception ex) { AppDialog.Error(ex.Message, owner: this); }
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProductID.Text)) { ShowFormError("Select a product to delete."); return; }
            if (!int.TryParse(txtProductID.Text, out var id)) return;
            if (!AppDialog.Confirm($"Delete product #{id:00000}? This cannot be undone.", owner: this)) return;

            try
            {
                var p = iProductService.GetProductById(id);
                if (p != null) iProductService.DeleteProduct(p);
                ResetInput();
                await ReloadAsync();
            }
            catch (Exception ex) { AppDialog.Error(ex.Message, owner: this); }
        }

        private void dgData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgData.SelectedItem is Product p)
            {
                txtProductID.Text = p.ProductId.ToString();
                txtProductName.Text = p.ProductName;
                txtPrice.Text = p.UnitPrice.ToString("0.##");
                txtUnitsInStock.Text = p.UnitsInStock.ToString();
                cboCategory.SelectedValue = p.CategoryId;
                inspectorTitle.Text = p.ProductName;
            }
            else
            {
                inspectorTitle.Text = "No selection";
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            dgData.SelectedItem = null;
            ResetInput();
        }

        private async void btnReload_Click(object sender, RoutedEventArgs e) => await ReloadAsync();

        private void btnManageMembers_Click(object sender, RoutedEventArgs e)
            => new ManageMembersWindow { Owner = this }.ShowDialog();

        private void Sidebar_Members(object sender, MouseButtonEventArgs e) => btnManageMembers_Click(sender, e);

        private void btnChangePass_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser == null) return;
            new ChangePasswordWindow(currentUser.MemberId) { Owner = this }.ShowDialog();
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            if (!AppDialog.Confirm("Sign out of the workspace?", owner: this)) return;
            new LoginWindow().Show();
            Close();
        }

        private void ChkChanged(object sender, RoutedEventArgs e)
        {
            vm.InStockOnly = chkInStockOnly.IsChecked == true;
            UpdateStatus();
            UpdateEmptyState();
        }

        private void cboFilterCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.CategoryFilter = cboFilterCategory.SelectedValue as int?;
            UpdateStatus();
            UpdateEmptyState();
        }

        private void btnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            chkInStockOnly.IsChecked = false;
            cboFilterCategory.SelectedValue = null;
            txtSearch.Text = "";
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            vm.SearchText = txtSearch.Text;
            searchPlaceholder.Visibility = string.IsNullOrEmpty(txtSearch.Text) ? Visibility.Visible : Visibility.Collapsed;
            UpdateStatus();
            UpdateEmptyState();
        }

        private void ResetInput()
        {
            txtProductID.Text = "";
            txtProductName.Text = "";
            txtPrice.Text = "";
            txtUnitsInStock.Text = "";
            cboCategory.SelectedValue = null;
            inspectorTitle.Text = "No selection";
            HideFormError();
        }

        private void ShowFormError(string msg) { txtFormError.Text = msg; formError.Visibility = Visibility.Visible; }
        private void HideFormError() => formError.Visibility = Visibility.Collapsed;
    }
}
