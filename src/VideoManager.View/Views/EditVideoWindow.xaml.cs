using System.Windows;
using VideoManager.ViewModel.Services;
using VideoManager.Model;

namespace VideoManager.View.Views
{
    public partial class EditVideoWindow : Window
    {
        private readonly VideoDto _video;
        private readonly VideoApiClient _apiClient;

        public EditVideoWindow(VideoDto video, VideoApiClient apiClient)
        {
            InitializeComponent();
            _video = video;
            _apiClient = apiClient;

            TitleTextBox.Text = video.Title;
            DescriptionTextBox.Text = video.Description;
            TagsTextBox.Text = video.Metadata != null 
                ? string.Join(", ", video.Metadata.Tags) 
                : string.Empty;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Please enter a title", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsEnabled = false;

                var tags = string.IsNullOrWhiteSpace(TagsTextBox.Text)
                    ? new List<string>()
                    : TagsTextBox.Text.Split(',').Select(t => t.Trim()).ToList();

                var updateDto = new UpdateVideoDto
                {
                    Title = TitleTextBox.Text,
                    Description = DescriptionTextBox.Text,
                    Tags = tags
                };

                var result = await _apiClient.UpdateVideoAsync(_video.Id, updateDto);

                if (result.IsSuccess)
                {
                    MessageBox.Show("Video updated successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show(result.Message, "Update Failed", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating video: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsEnabled = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
