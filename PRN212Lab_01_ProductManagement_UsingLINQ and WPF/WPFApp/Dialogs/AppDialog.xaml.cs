using System.Windows;
using System.Windows.Media;

namespace WPFApp.Dialogs
{
    public enum DialogKind { Info, Success, Warning, Error, Confirm }

    public partial class AppDialog : Window
    {
        public AppDialog() { InitializeComponent(); }

        private void btnOk_Click(object sender, RoutedEventArgs e) { DialogResult = true; Close(); }
        private void btnCancel_Click(object sender, RoutedEventArgs e) { DialogResult = false; Close(); }

        public static void Info(string message, string title = "Info", Window? owner = null) => Show(DialogKind.Info, title, message, owner);
        public static void Success(string message, string title = "Success", Window? owner = null) => Show(DialogKind.Success, title, message, owner);
        public static void Warn(string message, string title = "Warning", Window? owner = null) => Show(DialogKind.Warning, title, message, owner);
        public static void Error(string message, string title = "Error", Window? owner = null) => Show(DialogKind.Error, title, message, owner);

        public static bool Confirm(string message, string title = "Confirm", Window? owner = null)
        {
            var d = Build(DialogKind.Confirm, title, message, owner);
            d.btnCancel.Visibility = Visibility.Visible;
            d.btnOk.Content = "Confirm";
            return d.ShowDialog() == true;
        }

        private static void Show(DialogKind kind, string title, string message, Window? owner)
            => Build(kind, title, message, owner).ShowDialog();

        private static AppDialog Build(DialogKind kind, string title, string message, Window? owner)
        {
            var d = new AppDialog
            {
                Owner = owner ?? Application.Current.MainWindow,
                Title = title
            };
            d.titleText.Text = title;
            d.messageText.Text = message;

            (string glyph, string brush, string bg, string border, string label) = kind switch
            {
                DialogKind.Success => ("✓", "SuccessFg", "#0F2A1A", "#1F4D2E", "success"),
                DialogKind.Warning => ("!", "AttentionFg", "#2C2113", "#9E6A03", "warning"),
                DialogKind.Error   => ("✕", "DangerFg",  "#3B1518", "#DA3633", "error"),
                DialogKind.Confirm => ("?", "AccentFg",  "#0C1B2E", "#1F6FEB", "confirm"),
                _                  => ("i", "AccentFg",  "#0C1B2E", "#1F6FEB", "info")
            };
            d.icon.Text = glyph;
            d.icon.Foreground = (Brush)Application.Current.FindResource(brush);
            d.iconBg.Background = (Brush)new BrushConverter().ConvertFromString(bg)!;
            d.iconBg.BorderBrush = (Brush)new BrushConverter().ConvertFromString(border)!;
            d.kindLabel.Text = label;
            return d;
        }
    }
}
