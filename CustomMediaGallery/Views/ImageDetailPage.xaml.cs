using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace CustomMediaGallery
{
    public partial class ImageDetailPage : ContentPage
    {
        public ImageDetailPage(string imagePath)
        {
            InitializeComponent();
            image.Source = imagePath;
        }
    }
}
