using BusinessObjects;
using Services;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WPFApp.Dialogs;

namespace WPFApp
{
    public partial class ChangePasswordWindow : Window
    {
        private readonly string memberId;
        private readonly IAccountService iAccountService;

        public ChangePasswordWindow(string memberId)
        {
            InitializeComponent();
            this.memberId = memberId;
            iAccountService = new AccountService();
            MouseLeftButtonDown += (_, e) => { if (e.ChangedButton == MouseButton.Left) DragMove(); };
        }

        private void txtNew_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var s = PasswordPolicy.Evaluate(txtNew.Password);
            var (filled, brush, label) = s switch
            {
                PasswordStrength.TooShort => (0, "#21262D", "too short"),
                PasswordStrength.Weak => (1, "#F85149", "weak"),
                PasswordStrength.Medium => (2, "#D29922", "medium"),
                PasswordStrength.Strong => (4, "#3FB950", "strong"),
                _ => (0, "#21262D", "")
            };
            var bars = new[] { sb1, sb2, sb3, sb4 };
            var on = (SolidColorBrush)new BrushConverter().ConvertFromString(brush)!;
            var off = (SolidColorBrush)new BrushConverter().ConvertFromString("#21262D")!;
            for (int i = 0; i < bars.Length; i++) bars[i].Background = i < filled ? on : off;
            strengthLabel.Text = string.IsNullOrEmpty(txtNew.Password) ? "" : $"strength · {label}";
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            HideMsg();
            if (string.IsNullOrEmpty(txtOld.Password)) { ShowMsg("Nhập mật khẩu hiện tại."); return; }
            if (string.IsNullOrEmpty(txtNew.Password)) { ShowMsg("Nhập mật khẩu mới."); return; }
            if (PasswordPolicy.Evaluate(txtNew.Password) == PasswordStrength.TooShort)
            { ShowMsg($"Mật khẩu mới tối thiểu {PasswordPolicy.MinLength} ký tự."); return; }
            if (txtNew.Password != txtConfirm.Password) { ShowMsg("Mật khẩu xác nhận không khớp."); return; }
            if (txtOld.Password == txtNew.Password) { ShowMsg("Mật khẩu mới phải khác mật khẩu cũ."); return; }

            if (!iAccountService.ChangePassword(memberId, txtOld.Password, txtNew.Password))
            { ShowMsg("Mật khẩu hiện tại không đúng."); return; }

            DialogResult = true;
            Close();
            AppDialog.Success("Đổi mật khẩu thành công.", owner: Application.Current.MainWindow);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) => Close();

        private void ShowMsg(string m) { txtMessage.Text = m; msgBox.Visibility = Visibility.Visible; }
        private void HideMsg() => msgBox.Visibility = Visibility.Collapsed;
    }
}
