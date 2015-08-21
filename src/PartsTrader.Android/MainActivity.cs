using System;
using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;

namespace PartsTrader.Android
{
    [Activity(Label = "PartsTrader Quote App", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        public static readonly int PickImageId = 1000;
        private ImageView _imageView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // wire up button click
            _imageView = FindViewById<ImageView>(Resource.Id.partImage);
            var button = FindViewById<Button>(Resource.Id.btnAttachImage);
            button.Click += BtnAttachImageOnClick;
        }

        private void BtnAttachImageOnClick(object sender, EventArgs eventArgs)
        {
            Intent = new Intent();
            Intent.SetType("image/*");
            Intent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(Intent, "Select Picture"), PickImageId);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if ((requestCode == PickImageId) && (resultCode == Result.Ok) && (data != null))
            {
                global::Android.Net.Uri uri = data.Data;
                _imageView.SetImageURI(uri);

                string path = GetPathToImage(uri);
                Toast.MakeText(this, path, ToastLength.Long);
            }
        }

        private string GetPathToImage(global::Android.Net.Uri uri)
        {
            string path = null;
            // The projection contains the columns we want to return in our query.
            var projection = new[] { global::Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data };
            using (var cursor = ContentResolver.Query(uri, projection, null, null, null))
            {
                if (cursor != null)
                {
                    var columnIndex = cursor.GetColumnIndexOrThrow(global::Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data);
                    cursor.MoveToFirst();
                    path = cursor.GetString(columnIndex);
                }
            }
            return path;
        }


    }
}

