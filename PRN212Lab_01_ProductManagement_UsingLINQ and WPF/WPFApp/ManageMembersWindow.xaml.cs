using BusinessObjects;
using Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WPFApp.Dialogs;

namespace WPFApp
{
    public partial class ManageMembersWindow : Window
    {
        private const string AdminId = "PS0001";
        private readonly IAccountService iAccountService;

        public ManageMembersWindow()
        {
            InitializeComponent();
            iAccountService = new AccountService();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cboRole.ItemsSource = Enum.GetValues(typeof(Role));
            LoadMemberList();
        }

        private void LoadMemberList()
        {
            try { dgMembers.ItemsSource = iAccountService.GetAccounts(); }
            catch (Exception ex) { AppDialog.Error($"Lỗi tải danh sách: {ex.Message}", owner: this); }
            finally { ResetInput(); }
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            HideErr();
            if (string.IsNullOrWhiteSpace(txtMemberID.Text)) { Err("Vui lòng nhập Member ID."); return; }
            if (string.IsNullOrWhiteSpace(txtFullName.Text)) { Err("Vui lòng nhập họ tên."); return; }
            if (string.IsNullOrEmpty(txtPassword.Password)) { Err("Vui lòng nhập mật khẩu."); return; }
            if (PasswordPolicy.Evaluate(txtPassword.Password) == PasswordStrength.TooShort)
            { Err($"Mật khẩu tối thiểu {PasswordPolicy.MinLength} ký tự."); return; }
            if (cboRole.SelectedItem == null) { Err("Vui lòng chọn vai trò."); return; }

            try
            {
                if (iAccountService.GetAccountById(txtMemberID.Text.Trim()) != null) { Err("Member ID đã tồn tại."); return; }
                var email = txtEmail.Text?.Trim();
                if (!ValidateEmail(email)) return;
                if (!string.IsNullOrEmpty(email) && IsEmailUsedByOther(email, null)) { Err("Email đã được dùng bởi tài khoản khác."); return; }

                iAccountService.SaveAccount(new AccountMember
                {
                    MemberId = txtMemberID.Text.Trim(),
                    MemberPassword = PasswordHasher.Hash(txtPassword.Password),
                    FullName = txtFullName.Text.Trim(),
                    EmailAddress = email,
                    MemberRole = (Role)cboRole.SelectedItem
                });
                AppDialog.Success("Thêm tài khoản thành công.", owner: this);
                LoadMemberList();
            }
            catch (Exception ex) { Err(ex.Message); }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            HideErr();
            if (string.IsNullOrWhiteSpace(txtMemberID.Text)) { Err("Chọn 1 tài khoản để cập nhật."); return; }
            if (string.IsNullOrWhiteSpace(txtFullName.Text)) { Err("Vui lòng nhập họ tên."); return; }
            if (cboRole.SelectedItem == null) { Err("Vui lòng chọn vai trò."); return; }

            try
            {
                var existing = iAccountService.GetAccountById(txtMemberID.Text.Trim());
                if (existing == null) { Err("Tài khoản không tồn tại."); return; }

                var email = txtEmail.Text?.Trim();
                if (!ValidateEmail(email)) return;
                if (!string.IsNullOrEmpty(email) && IsEmailUsedByOther(email, existing.MemberId))
                { Err("Email đã được dùng bởi tài khoản khác."); return; }

                existing.FullName = txtFullName.Text.Trim();
                existing.EmailAddress = email;
                existing.MemberRole = (Role)cboRole.SelectedItem;
                if (!string.IsNullOrEmpty(txtPassword.Password))
                {
                    if (PasswordPolicy.Evaluate(txtPassword.Password) == PasswordStrength.TooShort)
                    { Err($"Mật khẩu tối thiểu {PasswordPolicy.MinLength} ký tự."); return; }
                    existing.MemberPassword = PasswordHasher.Hash(txtPassword.Password);
                }
                iAccountService.UpdateAccount(existing);
                AppDialog.Success("Cập nhật thành công.", owner: this);
                LoadMemberList();
            }
            catch (Exception ex) { Err(ex.Message); }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            HideErr();
            if (string.IsNullOrWhiteSpace(txtMemberID.Text)) { Err("Chọn tài khoản cần xóa."); return; }
            if (txtMemberID.Text == AdminId) { AppDialog.Warn("Không thể xóa tài khoản Admin hệ thống.", owner: this); return; }
            if (!AppDialog.Confirm("Bạn chắc chắn muốn xóa tài khoản này?", owner: this)) return;

            try
            {
                var member = iAccountService.GetAccountById(txtMemberID.Text);
                if (member != null) iAccountService.DeleteAccount(member);
                AppDialog.Success("Xóa tài khoản thành công.", owner: this);
                LoadMemberList();
            }
            catch (Exception ex) { Err(ex.Message); }
        }

        private void dgMembers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgMembers.SelectedItem is AccountMember m)
            {
                txtMemberID.Text = m.MemberId;
                txtMemberID.IsEnabled = false;
                txtPassword.Password = "";
                txtFullName.Text = m.FullName;
                txtEmail.Text = m.EmailAddress;
                cboRole.SelectedItem = m.MemberRole ?? Role.Staff;
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e) { dgMembers.SelectedItem = null; ResetInput(); }

        private void ResetInput()
        {
            txtMemberID.Text = ""; txtMemberID.IsEnabled = true;
            txtPassword.Password = ""; txtFullName.Text = ""; txtEmail.Text = "";
            cboRole.SelectedIndex = -1;
            HideErr();
        }

        private bool ValidateEmail(string? email)
        {
            if (string.IsNullOrEmpty(email)) return true;
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                if (addr.Address != email) { Err("Địa chỉ email không hợp lệ."); return false; }
                return true;
            }
            catch { Err("Địa chỉ email không hợp lệ."); return false; }
        }

        private bool IsEmailUsedByOther(string email, string? selfId)
            => iAccountService.GetAccounts().Any(a =>
                string.Equals(a.EmailAddress?.Trim(), email, StringComparison.OrdinalIgnoreCase) && a.MemberId != selfId);

        private void Err(string m) { txtFormError.Text = m; formError.Visibility = Visibility.Visible; }
        private void HideErr() => formError.Visibility = Visibility.Collapsed;
    }
}
