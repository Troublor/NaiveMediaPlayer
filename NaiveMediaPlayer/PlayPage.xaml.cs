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

        private StorageFolder _saveFolder;

        public PlayPage()
        {
            this.InitializeComponent();
            this.GetOrCreateFolder();

        }

        private async void GetOrCreateFolder()
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
            test();
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

        private async void test()
        {
            //            StorageFolder testFolder = await StorageFolder.GetFolderFromPathAsync(@"C:\Users\troub\Pictures");
            //            StorageFile sourceFile = await testFolder.GetFileAsync("选课.PNG");
            //            StorageFile destinationFile = await KnownFolders.CameraRoll.CreateFileAsync("MyTestImage.png");
            //
            //            using (var sourceStream = await sourceFile.OpenReadAsync())
            //            {
            //                using (var sourceInputStream = sourceStream.GetInputStreamAt(0))
            //                {
            //                    using (var destinationStream = await destinationFile.OpenAsync(FileAccessMode.ReadWrite))
            //                    {
            //                        using (var destinationOutputStream = destinationStream.GetOutputStreamAt(0))
            //                        {
            //                            await RandomAccessStream.CopyAndCloseAsync(sourceInputStream, destinationStream);
            //                        }
            //                    }
            //                }
            //            }

            Uri source = new Uri("http://www.neu.edu.cn/indexsource/neusong.mp3");

            StorageFile destinationFile =
                await this._saveFolder.CreateFileAsync("NeuSong.mp3", CreationCollisionOption.GenerateUniqueName);


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
}
