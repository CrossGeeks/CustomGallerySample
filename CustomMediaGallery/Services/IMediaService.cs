using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CustomMediaGallery.Models;

namespace CustomMediaGallery.Services
{
    public class MediaEventArgs : EventArgs
    {
        public MediaAsset Media { get; }
        public MediaEventArgs(MediaAsset media)
        {
            Media = media;
        }
    }
    public interface IMediaService
    {
        event EventHandler<MediaEventArgs> OnMediaAssetLoaded;
        bool IsLoading { get; }
        Task<IList<MediaAsset>> RetrieveMediaAssetsAsync(CancellationToken? token = null);
    }
}
