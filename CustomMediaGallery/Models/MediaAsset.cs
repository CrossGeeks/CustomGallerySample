using System;
namespace CustomMediaGallery.Models
{
    public enum MediaAssetType
    {
        Image, Video
    }

    public class MediaAsset
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public MediaAssetType Type { get; set; }
        public string PreviewPath { get; set; }
        public string Path { get; set; }
    }
}
