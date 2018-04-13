using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using Windows.Media.Core;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.System.Display;
using Windows.UI.Popups;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace NaiveMediaPlayer
{

    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class PlayPage : Page
    {
        private ObservableCollection<PlayHistory> _playHistory = new ObservableCollection<PlayHistory>();

        private ObservableCollection<CachedItem> _cachedItems = new ObservableCollection<CachedItem>();

        private ObservableCollection<CloudResource> _cloudResources = new ObservableCollection<CloudResource>();

        private StorageFolder _saveFolder;

        public PlayPage()
        {
            this.InitializeComponent();
            this.InitializeLibrary();
            _cloudResources.Add(new CloudResource("http://www.neu.edu.cn/indexsource/neusong.mp3", "东大校歌"));
        }

        private async void LoadCachedItems()
        {
            var fileList = await this._saveFolder.GetFilesAsync();
            foreach (var file in fileList)
            {
                if (file.FileType == ".mp3" || file.FileType == ".mp4")
                {
                    this._cachedItems.Add(new CachedItem(file.Name, file.Path));
                }
            }
        }

        private async void InitializeLibrary()
        {
            var myMusic = await Windows.Storage.StorageLibrary.GetLibraryAsync
                (Windows.Storage.KnownLibraryId.Music);
            Windows.Storage.StorageFolder saveMusicFolder = myMusic.SaveFolder;
            try
            {
                this._saveFolder = await StorageFolder.GetFolderFromPathAsync(saveMusicFolder.Path + @"\naive_media");
            }
            catch (Exception e)
            {
                this._saveFolder = await saveMusicFolder.CreateFolderAsync("naive_media");
            }
            this.LoadCachedItems();
        }

        private async void OnChooseFileButtonClicked(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.VideosLibrary;
            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Add(".mp4");
            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                _playHistory.Add(new PlayHistory(file.Name, file.Path));
                // Application now has read/write access to the picked file
                var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                MediaElement.SetSource(stream, file.ContentType);
                MediaElement.Play();
            }


        }
        private DisplayRequest appDisplayRequest = null;

        private void MediaElement_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            MediaElement mediaElement = sender as MediaElement;
            if (mediaElement != null && mediaElement.IsAudioOnly == false)
            {
                if (mediaElement.CurrentState == Windows.UI.Xaml.Media.MediaElementState.Playing)
                {
                    if (appDisplayRequest == null)
                    {
                        // This call creates an instance of the DisplayRequest object. 
                        appDisplayRequest = new DisplayRequest();
                        appDisplayRequest.RequestActive();
                    }
                }
                else // CurrentState is Buffering, Closed, Opening, Paused, or Stopped. 
                {
                    if (appDisplayRequest != null)
                    {
                        // Deactivate the display request and set the var to null.
                        appDisplayRequest.RequestRelease();
                        appDisplayRequest = null;
                    }
                }
            }
        }

        private async void Download(String url, String name)
        {
            Uri source = new Uri(url);
            StorageFile destinationFile =
                await this._saveFolder.CreateFileAsync(name, CreationCollisionOption.GenerateUniqueName);
            BackgroundDownloader downloader = new BackgroundDownloader();
            DownloadOperation download = downloader.CreateDownload(source, destinationFile);
            await download.StartAsync();

        }

        private void MenuFlyoutItem_OnClick(object sender, RoutedEventArgs e)
        {
            MediaElement.Source = new Uri("http://www.neu.edu.cn/indexsource/neusong.mp3");
            MediaElement.Play();
            _playHistory.Add(new PlayHistory("neusong.mp3", "http://www.neu.edu.cn/indexsource/neusong.mp3"));
        }

        private async void CachedItemsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = CachedItemsListView.SelectedItem as CachedItem;
            if (selectedItem == null)
            {
                return;
            }
            StorageFile file = await this._saveFolder.GetFileAsync(selectedItem.FileName);
            var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
            MediaElement.SetSource(stream, file.ContentType);
            MediaElement.Play();
            _playHistory.Add(new PlayHistory(selectedItem.FileName, selectedItem.FilePath));
        }

        private void CloudResourcesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = CloudResourcesListView.SelectedItem as CloudResource;
            if (selectedItem == null)
            {
                return;
            }
            MediaElement.Source = new Uri(selectedItem.Uri);
            MediaElement.Play();
            _playHistory.Add(new PlayHistory(selectedItem.Name, selectedItem.Uri));
        }
    }

    class PlayHistory
    {
        public PlayHistory(String name = "", String path = "")
        {
            this.Time = DateTime.Now;
            this.FileName = name;
            this.FilePath = path;
        }


        public DateTime Time { get; set; }

        public String FileName { get; set; }

        public String FilePath { get; set; }

        public override string ToString()
        {
            return this.FileName + "   (" + this.FilePath + ")" + "\t\t\t" + this.Time.ToLocalTime().TimeOfDay;
        }
    }

    class CachedItem
    {
        public CachedItem(String name = "", String path = "")
        {
            this.FileName = name;
            this.FilePath = path;
        }

        public String FileName { get; set; }

        public String FilePath { get; set; }
    }

    class CloudResource
    {
        public CloudResource(string uri, string name)
        {
            Uri = uri;
            Name = name;
        }

        public String Uri { get; set; }

        public String Name { get; set; }
    }
}
