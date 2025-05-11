using System.Windows;
using _14k2sTO9K9K.Services;

namespace _14k2sTO9K9K
{
    public partial class MainWindow : Window
    {
        private readonly Services.Services _services;

        // 通过构造函数注入 Services
        public MainWindow(Services.Services services)
        {
            InitializeComponent();
            this.Title = "14K2S to 9K9K";
            this.Width = 450;
            this.Height = 450;

            _services = services;

            // 添加拖拽事件处理
            this.AllowDrop = true;
            this.Drop += MainWindow_Drop;
        }

        private async void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                await _services.ProcessFilesAsync(files);
            }
        }
    }
}