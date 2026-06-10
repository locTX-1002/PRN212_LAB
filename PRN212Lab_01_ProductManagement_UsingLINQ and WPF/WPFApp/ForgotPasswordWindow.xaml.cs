using BusinessObjects;
using Services;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WPFApp.Dialogs;

namespace WPFApp
{
    public partial class ForgotPasswordWindow : Window
    {
        private readonly IAccountService iAccountService;

        public ForgotPasswordWindow()
        {
            InitializeComponent();
            iAccountService = new AccountService();
            MouseLeftButtonDown += (_, e) => { if (e.ChangedButton == MouseButton.Left) DragMove(); };
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            HideMessage();

            var memberId = txtMemberId.Text?.Trim();
            var email = txtEmail.Text?.Trim();
            var pwd = txtNewPassword.Password;
            var confirm = txtConfirmPassword.Password;

            if (string.IsNullOrEmpty(memberId) || string.IsNullOrEmpty(email))
            { ShowError("Vui lòng nhập Member ID và Email."); return; }
            if (PasswordPolicy.Evaluate(pwd) == PasswordStrength.TooShort)
            { ShowError($"Mật khẩu tối thiểu {PasswordPolicy.MinLength} ký tự."); return; }
            if (pwd != confirm)
            { ShowError("Mật khẩu xác nhận không khớp."); return; }

            try
            {
                var account = iAccountService.GetAccountById(memberId);
                if (account == null || !string.Equals(account.EmailAddress?.Trim(), email, StringComparison.OrdinalIgnoreCase))
                { ShowError("Member ID và Email không khớp với tài khoản nào."); return; }

                account.MemberPassword = PasswordHasher.Hash(pwd);
                account.FailedAttempts = 0;
                account.LockedUntil = null;
                iAccountService.UpdateAccount(account);

                Close();
                AppDialog.Success("Đặt lại mật khẩu thành công. Bạn có thể đăng nhập với mật khẩu mới.",
                                  owner: Application.Current.MainWindow);
            }
            catch (Exception ex) { ShowError($"Lỗi: {ex.Message}"); }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) => Close();

        private void ShowError(string message)
        {
            txtMessage.Text = message;
            txtMessage.Foreground = (Brush)FindResource("DangerBrush");
            msgBox.Background = (Brush)new BrushConverter().ConvertFromString("#FEE2E2")!;
            msgBox.Visibility = Visibility.Visible;
        }

        private void HideMessage() => msgBox.Visibility = Visibility.Collapsed;
    }
}
