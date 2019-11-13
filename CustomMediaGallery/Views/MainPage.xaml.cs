using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomMediaGallery.Models;
using CustomMediaGallery.Services;
using CustomMediaGallery.ViewModels;
using Xamarin.Forms;

namespace CustomMediaGallery
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage(IMediaService mediaService)
        {
            BindingContext = new MainViewModel(mediaService);
            InitializeComponent();
        }
    }
}
