using DataAccessLayer;
using System.Windows;

namespace WPFApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Khởi tạo DB và seed chỉ 1 lần
            MyDbContext.Initialize();
            base.OnStartup(e);
        }
    }
}
