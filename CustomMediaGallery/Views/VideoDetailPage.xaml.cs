using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace CustomMediaGallery
{
    public partial class VideoDetailPage : ContentPage
    {
        public VideoDetailPage(string videoPath)
        {
            InitializeComponent();
            videoPlayer.Source = videoPath;
        }
    }
}
