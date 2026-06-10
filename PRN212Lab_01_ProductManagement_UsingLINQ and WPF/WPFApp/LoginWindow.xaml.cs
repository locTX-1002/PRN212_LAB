using BusinessObjects;
using Services;
using System.Windows;
using System.Windows.Input;
using WPFApp.Dialogs;

namespace WPFApp
{
    public partial class LoginWindow : Window
    {
        private readonly IAccountService iAccountService;

        public LoginWindow()
        {
            InitializeComponent();
            iAccountService = new AccountService();
            MouseLeftButtonDown += (_, e) => { if (e.ChangedButton == MouseButton.Left) DragMove(); };
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            HideError();

            var login = txtUser.Text?.Trim();
            var password = txtPass.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ShowError("Vui lòng nhập tài khoản và mật khẩu.");
                return;
            }

            var outcome = iAccountService.AttemptLogin(login, password, Role.Admin);
            switch (outcome.Result)
            {
                case LoginResult.LockedOut:
                    ShowError($"Tài khoản bị khóa. Thử lại sau {outcome.RemainingLockSeconds / 60 + 1} phút.");
                    return;

                case LoginResult.InvalidCredentials:
                    ShowError("Sai tài khoản hoặc mật khẩu.");
                    return;

                case LoginResult.NoPermission:
                    ShowError("Tài khoản không có quyền truy cập.");
                    return;

                case LoginResult.Success:
                    Hide();
                    var main = new MainWindow(outcome.Account!);
                    main.Show();
                    Close();
                    return;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e) => Close();

        private void LinkForgot_Click(object sender, RoutedEventArgs e)
            => new ForgotPasswordWindow { Owner = this }.ShowDialog();

        private void ShowError(string message)
        {
            txtError.Text = message;
            errorBox.Visibility = Visibility.Visible;
        }

        private void HideError() => errorBox.Visibility = Visibility.Collapsed;
    }
}
