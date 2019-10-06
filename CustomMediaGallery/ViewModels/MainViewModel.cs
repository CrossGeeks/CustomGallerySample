using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CustomMediaGallery.Models;
using CustomMediaGallery.Services;
using Xamarin.Forms;

namespace CustomMediaGallery.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        IMediaService _mediaService;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Title => "Custom Media Gallery";

        public string SearchText { get; set; }
        public ObservableCollection<MediaAsset> MediaAssets { get; set; }

        public ICommand ItemTappedCommand { get; set; }

        public MainViewModel(IMediaService mediaService)
        {
            ItemTappedCommand = new Command<MediaAsset>(OnItemTapped);
            _mediaService = mediaService;
            MediaAssets = new ObservableCollection<MediaAsset>();
            Xamarin.Forms.BindingBase.EnableCollectionSynchronization(MediaAssets, null, ObservableCollectionCallback);
            _mediaService.OnMediaAssetLoaded += OnMediaAssetLoaded;
            LoadMediaAssets();
        }

        async void OnItemTapped(MediaAsset mediaAsset)
        {
            if (mediaAsset.Type == MediaAssetType.Video)
            {
                await App.Current.MainPage.Navigation.PushAsync(new VideoDetailPage(mediaAsset.Path));
            }
            else
            {
                await App.Current.MainPage.Navigation.PushAsync(new ImageDetailPage(mediaAsset.Path));
            }
        }

        void ObservableCollectionCallback(IEnumerable collection, object context, Action accessMethod, bool writeAccess)
        {
            // `lock` ensures that only one thread access the collection at a time
            lock (collection)
            {
                accessMethod?.Invoke();
            }
        }

        private void OnMediaAssetLoaded(object sender, MediaEventArgs e)
        {
            MediaAssets.Add(e.Media);
        }

        async Task LoadMediaAssets()
        {
            try
            {
                await _mediaService.RetrieveMediaAssetsAsync();
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Task was cancelled");
            }
        }
    }
}
