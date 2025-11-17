using System.Windows;
using VideoManager.View.Converters;
using VideoManager.ViewModel.Services;
using VideoManager.Model;

namespace VideoManager.View.Views
{
    public partial class VideoDetailWindow : Window
    {
        private readonly VideoDto _video;
        private readonly VideoApiClient _apiClient;
        private readonly FileSizeConverter _fileSizeConverter;

        public VideoDetailWindow(VideoDto video, VideoApiClient apiClient)
        {
            InitializeComponent();
            _video = video;
            _apiClient = apiClient;
            _fileSizeConverter = new FileSizeConverter();

            LoadVideoDetails();
            _ = LoadVersionsAsync();
        }

        private void LoadVideoDetails()
        {
            TitleTextBlock.Text = _video.Title;
            FileNameTextBlock.Text = _video.OriginalFileName;
            DescriptionTextBlock.Text = string.IsNullOrWhiteSpace(_video.Description) 
                ? "No description" 
                : _video.Description;
            FileSizeTextBlock.Text = _fileSizeConverter.Convert(_video.FileSizeBytes, typeof(string), null!, null!)?.ToString() ?? "Unknown";
            DurationTextBlock.Text = TimeSpan.FromSeconds(_video.DurationSeconds).ToString(@"hh\:mm\:ss");
            FormatTextBlock.Text = _video.FileFormat;
            CreatedTextBlock.Text = _video.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss");
            CreatedByTextBlock.Text = _video.CreatedBy;

            if (_video.Metadata != null)
            {
                ResolutionTextBlock.Text = string.IsNullOrEmpty(_video.Metadata.Resolution) 
                    ? "Unknown" 
                    : _video.Metadata.Resolution;
                CodecTextBlock.Text = $"{_video.Metadata.VideoCodec} / {_video.Metadata.AudioCodec}";
                BitrateTextBlock.Text = _video.Metadata.BitRate.HasValue 
                    ? $"{_video.Metadata.BitRate} kbps" 
                    : "Unknown";
                ViewCountTextBlock.Text = _video.Metadata.ViewCount.ToString();
                TagsTextBlock.Text = _video.Metadata.Tags.Any() 
                    ? string.Join(", ", _video.Metadata.Tags) 
                    : "No tags";
            }
        }

        private async Task LoadVersionsAsync()
        {
            var result = await _apiClient.GetVideoVersionsAsync(_video.Id);
            if (result.IsSuccess && result.Data != null)
            {
                VersionsListBox.ItemsSource = result.Data;
            }
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = _video.OriginalFileName,
                Filter = $"Video File|*{_video.FileFormat}|All Files|*.*"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                IsEnabled = false;
                try
                {
                    var result = await _apiClient.DownloadVideoAsync(_video.Id, saveFileDialog.FileName);
                    if (result.IsSuccess)
                    {
                        MessageBox.Show($"Video saved to: {saveFileDialog.FileName}", "Download Complete", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(result.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                finally
                {
                    IsEnabled = true;
                }
            }
        }

        private async void UploadVersionButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Video Files|*.mp4;*.avi;*.mkv;*.mov;*.wmv|All Files|*.*",
                Title = "Select Video File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var changeDescription = Microsoft.VisualBasic.Interaction.InputBox(
                    "Enter a description for this version:", 
                    "Version Description", 
                    "Updated version");

                if (!string.IsNullOrWhiteSpace(changeDescription))
                {
                    IsEnabled = false;
                    try
                    {
                        var result = await _apiClient.UploadVideoVersionAsync(
                            _video.Id, 
                            openFileDialog.FileName, 
                            changeDescription);

                        if (result.IsSuccess)
                        {
                            MessageBox.Show("Version uploaded successfully!", "Success", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            await LoadVersionsAsync();
                        }
                        else
                        {
                            MessageBox.Show(result.Message, "Upload Failed", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    finally
                    {
                        IsEnabled = true;
                    }
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
