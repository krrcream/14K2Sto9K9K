using System.Windows;

namespace WpfApp2
{
    public partial class ProgressWindow : Window
    {
        public ProgressWindow()
        {
            InitializeComponent();
        }

        public void UpdateTotalFiles(int totalFiles)
        {
            Dispatcher.Invoke(() =>
            {
                ProgressBar.Maximum = totalFiles;
            });
        }

        public void UpdateProcessedFiles(int processedFiles)
        {
            Dispatcher.Invoke(() =>
            {
                ProgressBar.Value = processedFiles;
                ProgressText.Text = $"{processedFiles}/{ProgressBar.Maximum}";
            });
        }
    }
}