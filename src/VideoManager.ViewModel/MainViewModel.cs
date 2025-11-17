using System.Collections.ObjectModel;
using System.Windows.Input;
using VideoManager.Model;
using VideoManager.ViewModel.Commands;
using VideoManager.ViewModel.Services;

namespace VideoManager.ViewModel
{
    /// <summary>
    /// Main ViewModel for the application
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly VideoApiClient _apiClient;
        private ObservableCollection<VideoDto> _videos;
        private VideoDto? _selectedVideo;
        private string _searchText;
        private bool _isLoading;
        private string _statusMessage;
        private int _currentPage;
        private int _totalPages;
        private int _pageSize;
        private const string _videoApiClientBaseUrl = "https://localhost:5052";

        // Action delegates for view interactions
        public Action<string, VideoApiClient>? ShowUploadDialog { get; set; }
        public Action<VideoDto, VideoApiClient>? ShowEditDialog { get; set; }
        public Action<VideoDto, VideoApiClient>? ShowDetailWindow { get; set; }
        public Func<string, string, bool, bool>? ShowMessageBox { get; set; }
        public Func<string?, string?>? ShowOpenFileDialog { get; set; }
        public Func<string, string, string?>? ShowSaveFileDialog { get; set; }

        public MainViewModel()
        {
            _apiClient = new VideoApiClient(_videoApiClientBaseUrl);
            _videos = new ObservableCollection<VideoDto>();
            _searchText = string.Empty;
            _statusMessage = "Ready";
            _currentPage = 1;
            _pageSize = 20;

            // Initialize commands
            LoadVideosCommand = new AsyncRelayCommand(async _ => await LoadVideosAsync());
            SearchCommand = new AsyncRelayCommand(async _ => await SearchVideosAsync());
            RefreshCommand = new AsyncRelayCommand(async _ => await RefreshAsync());
            UploadVideoCommand = new AsyncRelayCommand(async _ => await UploadVideoAsync());
            DeleteVideoCommand = new AsyncRelayCommand(async _ => await DeleteVideoAsync(), _ => SelectedVideo != null);
            DownloadVideoCommand = new AsyncRelayCommand(async _ => await DownloadVideoAsync(), _ => SelectedVideo != null);
            ViewVideoCommand = new AsyncRelayCommand(async _ => await ViewVideoAsync(), _ => SelectedVideo != null);
            EditVideoCommand = new AsyncRelayCommand(async _ => await EditVideoAsync(), _ => SelectedVideo != null);
            NextPageCommand = new AsyncRelayCommand(async _ => await NextPageAsync(), _ => CurrentPage < TotalPages);
            PreviousPageCommand = new AsyncRelayCommand(async _ => await PreviousPageAsync(), _ => CurrentPage > 1);

            // Load initial data
            _ = LoadVideosAsync();
        }

        // Properties
        public ObservableCollection<VideoDto> Videos
        {
            get => _videos;
            set => SetProperty(ref _videos, value);
        }

        public VideoDto? SelectedVideo
        {
            get => _selectedVideo;
            set
            {
                if (SetProperty(ref _selectedVideo, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public int CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        public int TotalPages
        {
            get => _totalPages;
            set => SetProperty(ref _totalPages, value);
        }

        public int PageSize
        {
            get => _pageSize;
            set => SetProperty(ref _pageSize, value);
        }

        // Commands
        public ICommand LoadVideosCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand UploadVideoCommand { get; }
        public ICommand DeleteVideoCommand { get; }
        public ICommand DownloadVideoCommand { get; }
        public ICommand ViewVideoCommand { get; }
        public ICommand EditVideoCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }

        // Methods
        private async Task LoadVideosAsync()
        {
            IsLoading = true;
            StatusMessage = "Loading videos...";

            try
            {
                var parameters = new QueryParameters
                {
                    PageNumber = CurrentPage,
                    PageSize = PageSize,
                    SearchTerm = SearchText,
                    SortBy = "CreatedDate",
                    SortDescending = true
                };

                var result = await _apiClient.GetVideosAsync(parameters);

                if (result.IsSuccess && result.Data != null)
                {
                    Videos.Clear();
                    foreach (var video in result.Data.Items)
                    {
                        Videos.Add(video);
                    }

                    TotalPages = result.Data.TotalPages;
                    StatusMessage = $"Loaded {result.Data.Items.Count} videos (Page {CurrentPage} of {TotalPages})";
                }
                else
                {
                    StatusMessage = $"Error: {result.Message}";
                    ShowMessageBox?.Invoke(result.Message, "Error", true);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                ShowMessageBox?.Invoke($"Failed to load videos: {ex.Message}", "Error", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchVideosAsync()
        {
            CurrentPage = 1;
            await LoadVideosAsync();
        }

        private async Task RefreshAsync()
        {
            await LoadVideosAsync();
        }

        private async Task UploadVideoAsync()
        {
            var filePath = ShowOpenFileDialog?.Invoke("Video Files|*.mp4;*.avi;*.mkv;*.mov;*.wmv|All Files|*.*");
            if (!string.IsNullOrEmpty(filePath))
            {
                ShowUploadDialog?.Invoke(filePath, _apiClient);
                await RefreshAsync();
            }
        }

        private async Task DeleteVideoAsync()
        {
            if (SelectedVideo == null) return;

            var confirmed = ShowMessageBox?.Invoke(
                $"Are you sure you want to delete '{SelectedVideo.Title}'?",
                "Confirm Delete",
                false) ?? false;

            if (confirmed)
            {
                IsLoading = true;
                StatusMessage = "Deleting video...";

                var deleteResult = await _apiClient.DeleteVideoAsync(SelectedVideo.Id);

                if (deleteResult.IsSuccess)
                {
                    StatusMessage = "Video deleted successfully";
                    await RefreshAsync();
                }
                else
                {
                    StatusMessage = $"Error: {deleteResult.Message}";
                    ShowMessageBox?.Invoke(deleteResult.Message, "Error", true);
                }

                IsLoading = false;
            }
        }

        private async Task DownloadVideoAsync()
        {
            if (SelectedVideo == null) return;

            var savePath = ShowSaveFileDialog?.Invoke(
                SelectedVideo.OriginalFileName,
                $"Video File|*{SelectedVideo.FileFormat}|All Files|*.*");

            if (!string.IsNullOrEmpty(savePath))
            {
                IsLoading = true;
                StatusMessage = "Downloading video...";

                var result = await _apiClient.DownloadVideoAsync(SelectedVideo.Id, savePath);

                if (result.IsSuccess)
                {
                    StatusMessage = "Video downloaded successfully";
                    ShowMessageBox?.Invoke($"Video saved to: {savePath}", "Download Complete", false);
                }
                else
                {
                    StatusMessage = $"Error: {result.Message}";
                    ShowMessageBox?.Invoke(result.Message, "Error", true);
                }

                IsLoading = false;
            }
        }

        private async Task ViewVideoAsync()
        {
            if (SelectedVideo == null) return;
            ShowDetailWindow?.Invoke(SelectedVideo, _apiClient);
        }

        private async Task EditVideoAsync()
        {
            if (SelectedVideo == null) return;
            ShowEditDialog?.Invoke(SelectedVideo, _apiClient);
            await RefreshAsync();
        }

        private async Task NextPageAsync()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                await LoadVideosAsync();
            }
        }

        private async Task PreviousPageAsync()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await LoadVideosAsync();
            }
        }
    }
}
