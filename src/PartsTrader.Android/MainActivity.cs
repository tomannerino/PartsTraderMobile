using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Widget;
using Android.OS;
using Newtonsoft.Json;
using Stream = Android.Media.Stream;

namespace PartsTrader.Android
{
    [Activity(Label = "PartsTrader Quote App", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        public static readonly int PickImageId = 1000;
        private ImageView _imageView;
		//public string imageUri = String.Empty;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // wire up btnAttachImage button click
            _imageView = FindViewById<ImageView>(Resource.Id.partImage);
            var button = FindViewById<Button>(Resource.Id.btnAttachImage);
            button.Click += BtnAttachImageOnClick;

            // wire up btnUpload button click
            var uploadButton = FindViewById<Button>(Resource.Id.btnUpload);

            uploadButton.Click += async (sender, e) =>
            {
                var txtQuoteId = FindViewById<EditText>(Resource.Id.txtQuoteId);
                var txtPartNumber = FindViewById<EditText>(Resource.Id.txtPartNumber);
				var txtImageUri = FindViewById<EditText>(Resource.Id.txtImageUri);
				var imageBytes = GetImageFromUri(txtImageUri.Text);

                var parameters = new Dictionary<string, string>();

                parameters.Add("QuotePartNumber", txtPartNumber.Text);
                parameters.Add("QuoteId", txtQuoteId.Text);
                //parameters.Add("Image", base64Image);

                var resp =
                    await PostMultiPartForm("http://qafedex1internal.partstrader.us.com/procurement/api/quotepartimage", imageBytes, "Image", "image/jpg", parameters, "partsTraderImageCookie");


                //var model = JsonConvert.DeserializeObject<string[]>(resp);

                //return model;
                //var url = new StringBuilder();
                //url.Append("http://qafedex1internal.partstrader.us.com/procurement/api/upload/?quoteId=")
                //    .Append(txtQuoteId.Text)
                //    .Append("&partNumber=")
                //    .Append(txtPartNumber.Text)
                //    .Append("&Image=")


                //// Fetch the weather information asynchronously, 
                //// parse the results, then update the screen:
                //JsonValue json = await FetchWeatherAsync(url);
                //// ParseAndDisplay (json);
            };
        }

        private static byte[] GetImageFromUri(string uri)
        {
			byte[] imageBytes = null;
            //Bitmap imageBitmap = null;

//            using (var webClient = new WebClient())
//            {
//                imageBytes = webClient.DownloadData(uri);
//                //if (imageBytes != null && imageBytes.Length > 0)
//                //{
//                //    imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
//                //}
//            }
//			if (System.IO.File.Exists (uri))
//			{
//				var imageFile = new Java.IO.File (uri);
//				imageBytes = imageFile.ToArray<byte>();
//			}

//			Stream stream = ContentResolver.OpenInputStream(global::Android.Net.Uri.Parse(uri));
//			byte[] imageBytes = ReadFully(stream);

			var isoStoreFile = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForDomain();

			if (isoStoreFile.FileExists(uri))
			{
				using (var isoStream = new System.IO.IsolatedStorage.IsolatedStorageFileStream(uri, FileMode.Open, FileAccess.Read, isoStoreFile))
				{
					isoStream.Read(imageBytes, 0, (int)(isoStream.Length));
				}
			}

            return imageBytes;
            //return Base64.EncodeToString(imageBytes, Base64Flags.Default);
        }

//		public static byte[] ReadFully(Stream input) {
//			byte[] buffer = new byte[16*1024];
//			using(MemoryStream ms = new MemoryStream()) {
//				int read;
//				while((read = input.Read(buffer, 0, buffer.Length)) > 0) {
//					ms.Write(buffer, 0, read);
//				}
//				return ms.ToArray();
//			}
//		}

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
                //_imageView.SetTag(1, uri.ToString());

				var path = GetPathToImage(uri);
				var txtImageUri = FindViewById<EditText>(Resource.Id.txtImageUri);
				txtImageUri.Text = uri.Path;
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

        public static async Task<string> PostMultiPartForm(string url, byte[] file, string paramName, string contentType, Dictionary<String, string> nvc, string cookie)
        {
            // log.Debug(string.Format("Uploading {0} to {1}", file, url));
            var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");

            var wr = (HttpWebRequest)WebRequest.Create(url);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.Headers["Cookie"] = cookie;
            //wr.KeepAlive = true;
            //wr.Credentials = System.Net.CredentialCache.DefaultCredentials;

            var rs = await wr.GetRequestStreamAsync();

            const string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (var key in nvc.Keys)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                var formitem = string.Format(formdataTemplate, key, nvc[key]);
                var formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }

            rs.Write(boundarybytes, 0, boundarybytes.Length);

            const string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            var header = string.Format(headerTemplate, paramName, file, contentType);
            var headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            rs.Write(file, 0, file.Length);

            var trailer = System.Text.Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            //rs.Close();
            var responseString = String.Empty;
            WebResponse wresp = null;

            try
            {
                wresp = await wr.GetResponseAsync();
                var respStream = wresp.GetResponseStream();
                var respReader = new StreamReader(respStream);
                responseString = respReader.ReadToEnd();
                //log.Debug(string.Format("File uploaded, server response is: {0}", reader2.ReadToEnd()));
            }
            catch (Exception)
            {
                //log.Error("Error uploading file", ex);
                if (wresp != null)
                {
                    //wresp.Close();
                    wresp = null;
                }
            }
            finally
            {
                wr = null;
            }

            return responseString;
        }
    }
}

