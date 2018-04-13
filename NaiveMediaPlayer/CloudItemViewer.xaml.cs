using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Timers;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using NaiveMediaPlayer.Models;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace NaiveMediaPlayer
{
    public sealed partial class CloudItemViewer : UserControl
    {
        public String Name { get; set; }

        public String Uri { get; set; }

        private DownloadOperation _downloadOperation;

        public CloudItemViewer(CloudResource cloudResource)
        {
            this.InitializeComponent();
            this.Name = cloudResource.Name;
            this.Uri = cloudResource.Uri;
        }

        private void PlayButton_OnClick(object sender, RoutedEventArgs e)
        {
            MediaManager.Instance.MediaElement.Source = new Uri(this.Uri);
            MediaManager.Instance.MediaElement.Play();
            MediaManager.Instance.PlayHistories.Add(new PlayHistory(this.Name, this.Uri));
        }



        private async void Download(String url, String name)
        {
            Uri source = new Uri(url);
            StorageFile destinationFile =
                await MediaManager.Instance.SaveFolder.CreateFileAsync(name, CreationCollisionOption.GenerateUniqueName);
            BackgroundDownloader downloader = new BackgroundDownloader();

            _downloadOperation = downloader.CreateDownload(source, destinationFile);
            Timer timer = new System.Timers.Timer(50);
            // Hook up the Elapsed event for the timer. 
            //            timer.Elapsed += ShowCacheProgress;
            //            timer.AutoReset = true;
            //            timer.Enabled = false;
            await _downloadOperation.StartAsync();
            //            timer.Stop();
            //            timer.Dispose();
            CacheButton.Content = "取消缓存";
            CacheButton.IsEnabled = true;
            MediaManager.Instance.LoadCachedItems();
        }

        private void ShowCacheProgress(Object source, ElapsedEventArgs e)
        {
            var a = _downloadOperation.Progress;
            if (a.TotalBytesToReceive != 0)
            {
                Double percentage = _downloadOperation.Progress.BytesReceived /
                                                _downloadOperation.Progress.TotalBytesToReceive;
                CacheProgressBar.Value = CacheProgressBar.Width * percentage;
            }

        }

        private async void CacheButton_OnClick(object sender, RoutedEventArgs e)
        {
            if ((String)CacheButton.Content == "缓存")
            {
                CacheButton.IsEnabled = false;
                var progressRing = new ProgressRing();
                progressRing.IsActive = true;
                CacheButton.Content = progressRing;
                CacheProgressBar.Visibility = Visibility.Visible;
                Download(this.Uri, this.Name);
            }
            else if ((String)CacheButton.Content == "取消缓存")
            {
                CacheButton.IsEnabled = false;
                var progressRing = new ProgressRing();
                progressRing.IsActive = true;
                CacheButton.Content = progressRing;
                CacheProgressBar.Visibility = Visibility.Collapsed;
                var file = await MediaManager.Instance.SaveFolder.GetFileAsync(this.Name);
                await file.DeleteAsync();
                CacheButton.Content = "缓存";
                CacheButton.IsEnabled = true;
                MediaManager.Instance.LoadCachedItems();
            }

        }
    }
}
