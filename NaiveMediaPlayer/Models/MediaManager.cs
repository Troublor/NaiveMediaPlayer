using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace NaiveMediaPlayer.Models
{
    class MediaManager
    {
        public static MediaManager Instance = new MediaManager();

        private MediaManager()
        {

        }

        public MediaElement MediaElement { get; set; }

        public ObservableCollection<PlayHistory> PlayHistories { get; set; }

        public ObservableCollection<CachedItem> CachedItems { get; set; }

        public ObservableCollection<CloudResource> CloudResources { get; set; }

        public StorageFolder SaveFolder { get; set; }

        public async void LoadCachedItems()
        {
            var fileList = await SaveFolder.GetFilesAsync();
            CachedItems.Clear();
            foreach (var file in fileList)
            {
                if (file.FileType == ".mp3" || file.FileType == ".mp4")
                {
                    CachedItems.Add(new CachedItem(file.Name, file.Path));
                }
            }
        }
    }
}
