using System.IO;
using System.Windows;
using VideoManager.ViewModel.Services;
using VideoManager.Model;

namespace VideoManager.View.Views
{
    public partial class UploadVideoWindow : Window
    {
        private readonly string _filePath;
        private readonly VideoApiClient _apiClient;

        public UploadVideoWindow(string filePath, VideoApiClient apiClient)
        {
            InitializeComponent();
            _filePath = filePath;
            _apiClient = apiClient;

            FilePathTextBox.Text = filePath;
            TitleTextBox.Text = Path.GetFileNameWithoutExtension(filePath);
        }

        private async void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Please enter a title", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                UploadProgressBar.Visibility = Visibility.Visible;
                StatusTextBlock.Text = "Uploading...";
                IsEnabled = false;

                var tags = string.IsNullOrWhiteSpace(TagsTextBox.Text)
                    ? new List<string>()
                    : TagsTextBox.Text.Split(',').Select(t => t.Trim()).ToList();

                var result = await _apiClient.UploadVideoAsync(
                    _filePath,
                    TitleTextBox.Text,
                    DescriptionTextBox.Text,
                    tags);

                if (result.IsSuccess)
                {
                    MessageBox.Show("Video uploaded successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show(result.Message, "Upload Failed", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error uploading video: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsEnabled = true;
                UploadProgressBar.Visibility = Visibility.Collapsed;
                StatusTextBlock.Text = "";
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
