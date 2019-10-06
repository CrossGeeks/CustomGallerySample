using System;
using CustomMediaGallery.Services;
using DLToolkit.Forms.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CustomMediaGallery
{
    public partial class App : Application
    {
        IMediaService _mediaService;

        public App(IMediaService mediaService)
        {
            _mediaService = mediaService;
            InitializeComponent();
            FlowListView.Init();
            MainPage = new NavigationPage(new MainPage(_mediaService));
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
