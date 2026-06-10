using BusinessObjects;
using Services;
using System.Windows;

namespace WPFApp;

/// <summary>
/// Interaction logic for LoginWindow.xaml
/// </summary>
public partial class LoginWindow : Window
{
    private readonly IAccountService iAccountService;

    public LoginWindow()
    {
        InitializeComponent();
        iAccountService = new AccountService();
    }

    private void btnLogin_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtUser.Text))
        {
            MessageBox.Show("Vui lòng nhập Username.");
            return;
        }

        AccountMember? account = iAccountService.GetAccountById(txtUser.Text.Trim());

        if (account != null
            && PasswordHasher.Verify(txtPass.Password, account.MemberPassword)
            && account.MemberRole == 1)
        {
            this.Hide();
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
        else
        {
            MessageBox.Show("You are not permission !");
        }
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}
