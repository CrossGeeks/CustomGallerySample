using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Android;
using Android.App;
using Android.Content.PM;
using Android.Database;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Support.V4.App;
using CustomMediaGallery.Models;
using CustomMediaGallery.Services;
using Plugin.CurrentActivity;

namespace CustomMediaGallery.Droid.Services
{
    public class MediaService : IMediaService
    {
        bool stopLoad = false;

        bool _isLoading = false;
        public bool IsLoading => _isLoading;

        static TaskCompletionSource<bool> mediaPermissionTcs;

        public const int RequestMedia = 1354;

        public event EventHandler<MediaEventArgs> OnMediaAssetLoaded;

        async void RequestMediaPermissions()
        {
            if (ActivityCompat.ShouldShowRequestPermissionRationale(CrossCurrentActivity.Current.Activity, Manifest.Permission.WriteExternalStorage))
            {

                // Provide an additional rationale to the user if the permission was not granted
                // and the user would benefit from additional context for the use of the permission.
                // For example, if the request has been denied previously.

                await UserDialogs.Instance.AlertAsync("Media Permission", "This action requires external storafge permission", "Ok");
            }
            else
            {
                // Media permissions have not been granted yet. Request them directly.
                ActivityCompat.RequestPermissions(CrossCurrentActivity.Current.Activity,new string []{ Manifest.Permission.WriteExternalStorage }, RequestMedia);
            }
        }

        public static void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            if (requestCode == MediaService.RequestMedia)
            {
                // We have requested multiple permissions for Media, so all of them need to be
                // checked.
                if (PermissionUtil.VerifyPermissions(grantResults))
                {
                    // All required permissions have been granted, display Media fragment.
                    mediaPermissionTcs.TrySetResult(true);
                }
                else
                {
                    mediaPermissionTcs.TrySetResult(false);
                }

            }
        }

        public async Task<bool> RequestPermissionAsync()
        {
            mediaPermissionTcs = new TaskCompletionSource<bool>();
            // Verify that all required Media permissions have been granted.
            if (Android.Support.V4.Content.ContextCompat.CheckSelfPermission(CrossCurrentActivity.Current.Activity, Manifest.Permission.WriteExternalStorage) != (int)Permission.Granted)
            {
                // Media permissions have not been granted.
                RequestMediaPermissions();
            }
            else
            {
                // Media permissions have been granted. 
                mediaPermissionTcs.TrySetResult(true);
            }

            return await mediaPermissionTcs.Task;
        }


        public async Task<IList<MediaAsset>> RetrieveMediaAssetsAsync(CancellationToken? cancelToken = null)
        {
            stopLoad = false;

            if (!cancelToken.HasValue)
                cancelToken = CancellationToken.None;

            // We create a TaskCompletionSource of decimal
            var taskCompletionSource = new TaskCompletionSource<IList<MediaAsset>>();

            // Registering a lambda into the cancellationToken
            cancelToken.Value.Register(() =>
            {
                // We received a cancellation message, cancel the TaskCompletionSource.Task
                stopLoad = true;
                taskCompletionSource.TrySetCanceled();
            });

            _isLoading = true;

            var task = LoadMediaAsync();

            // Wait for the first task to finish among the two
            var completedTask = await Task.WhenAny(task, taskCompletionSource.Task);
            _isLoading = false;

            return await completedTask;
        }

        /*async Task<IList<MediaAsset>> LoadMediaAsync()
        {
            IList<MediaAsset> assets = new List<MediaAsset>();
            var hasPermission = await RequestPermissionAsync();
            if (hasPermission)
            {
                var uri = MediaStore.Images.Media.ExternalContentUri;
                var ctx = Application.Context;
                await Task.Run(() =>
                {
                    var cursor = ctx.ApplicationContext.ContentResolver.Query(uri, new string[]
                    {
                        MediaStore.Images.ImageColumns.Data,
                        MediaStore.Images.ImageColumns.DisplayName,
                        MediaStore.Images.ImageColumns.DateAdded,
                        MediaStore.Images.ImageColumns.Title
                    }, null, null, $"{  MediaStore.Images.ImageColumns.DateAdded} DESC");
                    if (cursor.Count > 0)
                    {
                        while (cursor.MoveToNext())
                        {
                            var path = GetString(cursor, MediaStore.Images.ImageColumns.Data);
                            if (!string.IsNullOrWhiteSpace(path))
                            {
                                var asset = new MediaAsset()
                                {
                                    Name = GetString(cursor, MediaStore.Images.ImageColumns.DisplayName),
                                    PreviewPath = path
                                };

                                OnMediaAssetLoaded?.Invoke(this, new MediaEventArgs(asset));
                                assets.Add(asset);
                            }

                            if (stopLoad)
                                break;
                        }
                    }
                });
            }
            return assets;
        }*/

        async Task<IList<MediaAsset>> LoadMediaAsync()
        {
            IList<MediaAsset> assets = new List<MediaAsset>();
            var hasPermission = await RequestPermissionAsync();
            if (hasPermission)
            {
                var uri = MediaStore.Files.GetContentUri("external");
                var ctx = Application.Context;
                await Task.Run(async() =>
                {
                    var cursor = ctx.ApplicationContext.ContentResolver.Query(uri, new string[]
                    {
                        MediaStore.Files.FileColumns.Id,
                        MediaStore.Files.FileColumns.Data,
                        MediaStore.Files.FileColumns.DateAdded,
                        MediaStore.Files.FileColumns.MediaType,
                        MediaStore.Files.FileColumns.MimeType,
                        MediaStore.Files.FileColumns.Title,
                        MediaStore.Files.FileColumns.Parent,
                        MediaStore.Files.FileColumns.DisplayName,
                        MediaStore.Files.FileColumns.Size
                    }, $"{MediaStore.Files.FileColumns.MediaType} = {(int)MediaType.Image} OR {MediaStore.Files.FileColumns.MediaType} = {(int)MediaType.Video}", null, $"{MediaStore.Files.FileColumns.DateAdded} DESC");
                    if (cursor.Count > 0)
                    {
                        while (cursor.MoveToNext())
                        {
                            try
                            {
                                var id = cursor.GetInt(cursor.GetColumnIndex(MediaStore.Files.FileColumns.Id));
                                var mediaType = cursor.GetInt(cursor.GetColumnIndex(MediaStore.Files.FileColumns.MediaType));
                                Bitmap bitmap = null;
                                switch (mediaType)
                                {
                                    case (int)MediaType.Image:
                                        bitmap = MediaStore.Images.Thumbnails.GetThumbnail(CrossCurrentActivity.Current.Activity.ContentResolver, id, ThumbnailKind.MiniKind, new BitmapFactory.Options()
                                        {
                                            InSampleSize = 4,
                                            InPurgeable = true
                                        });
                                        break;
                                    case (int)MediaType.Video:
                                        bitmap = MediaStore.Video.Thumbnails.GetThumbnail(CrossCurrentActivity.Current.Activity.ContentResolver, id, VideoThumbnailKind.MiniKind, new BitmapFactory.Options()
                                        {
                                            InSampleSize = 4,
                                            InPurgeable = true
                                        });
                                        break;
                                }
                                var tmpPath = System.IO.Path.GetTempPath();
                                var name = GetString(cursor, MediaStore.Files.FileColumns.DisplayName);
                                var filePath = System.IO.Path.Combine(tmpPath, $"tmp_{name}");

                                var path = GetString(cursor, MediaStore.Files.FileColumns.Data);

                                //filePath = path;
                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    bitmap?.Compress(Bitmap.CompressFormat.Png, 100, stream);
                                    stream.Close();
                                }


                                if (!string.IsNullOrWhiteSpace(filePath))
                                {
                                    var asset = new MediaAsset()
                                    {
                                        Id = $"{id}",
                                        Type = mediaType == (int)MediaType.Video ? MediaAssetType.Video : MediaAssetType.Image,
                                        Name = name,
                                        PreviewPath = filePath,
                                        Path = path
                                    };
                                   // if(assets.Count % 10 == 0)
                                    //{
                                        using (var h = new Handler(Looper.MainLooper))
                                            h.Post(async () => {  OnMediaAssetLoaded?.Invoke(this, new MediaEventArgs(asset)); });

                                   // }

                                    assets.Add(asset);
                                }

                                if (stopLoad)
                                    break;
                            }
                            catch(Exception ex)
                            {
                                await UserDialogs.Instance.AlertAsync(ex.StackTrace.ToString(), "error", "ok");
                            }
                           
                        }
                    }
                });
            }
            return assets;
        }


        string GetString(ICursor cursor, string key)
        {
            return cursor.GetString(cursor.GetColumnIndex(key));
        }
    }
}
