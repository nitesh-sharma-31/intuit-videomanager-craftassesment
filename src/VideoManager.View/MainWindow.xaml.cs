using System.Windows;
using VideoManager.ViewModel;

namespace VideoManager.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var viewModel = new MainViewModel();
            
            // Wire up the view interactions
            viewModel.ShowUploadDialog = (filePath, apiClient) =>
            {
                var window = new Views.UploadVideoWindow(filePath, apiClient);
                window.ShowDialog();
            };
            
            viewModel.ShowEditDialog = (video, apiClient) =>
            {
                var window = new Views.EditVideoWindow(video, apiClient);
                window.ShowDialog();
            };
            
            viewModel.ShowDetailWindow = (video, apiClient) =>
            {
                var window = new Views.VideoDetailWindow(video, apiClient);
                window.Show();
            };
            
            viewModel.ShowMessageBox = (message, title, isError) =>
            {
                var button = MessageBoxButton.OK;
                var icon = isError ? MessageBoxImage.Error : MessageBoxImage.Information;
                MessageBox.Show(message, title, button, icon);
                return true;
            };
            
            viewModel.ShowOpenFileDialog = (filter) =>
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = filter ?? "All Files|*.*",
                    Title = "Select File"
                };
                return dialog.ShowDialog() == true ? dialog.FileName : null;
            };
            
            viewModel.ShowSaveFileDialog = (fileName, filter) =>
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = fileName,
                    Filter = filter ?? "All Files|*.*"
                };
                return dialog.ShowDialog() == true ? dialog.FileName : null;
            };
            
            DataContext = viewModel;
        }
    }
}

