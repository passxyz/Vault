using Android.App;
using Android.Content;
using Android.OS;
using System.IO;
using System.Threading.Tasks;

using Serilog;

using PassXYZ.Utils;
using PassXYZLib;

namespace PassXYZ.Droid
{
	public class Share: IShare
	{
        // we will use FileProvider for api >= 19
        BuildVersionCodes sdkVersion = BuildVersionCodes.Kitkat;

        private readonly Context _context;
		public Share()
		{
			_context = Application.Context;
		}

        public string GetPlatformPath(string fileName)
        {
            if (Build.VERSION.SdkInt >= sdkVersion)
            {
                return Path.Combine(PxDataFile.TmpFilePath, fileName);
            }
            else
            {
                //var tmpPath = _context.GetExternalFilesDir(Environment.DirectoryDownloads).AbsolutePath;
                return Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, fileName);
            }
        }

        public string BaseUrl
        {
            get
            {
                return "file:///android_asset/";
            }
        }

        public Task Show( string title, string message, string filePath)
		{
			var extension = filePath.Substring(filePath.LastIndexOf(".") + 1).ToLower();
			var contentType = string.Empty;
		
			switch (extension)
			{
				case "pdf":
					contentType = "application/pdf";
					break;
				case "png":
					contentType = "image/png";
					break;
                case "jpg":
                    contentType = "image/jpg";
                    break;
                default:
                    // Set contentType to image so we can always use Bluetooth
                    // contentType = "application/octetstream";
                    contentType = "image/*";
                    break;
			}

			var intent = new Intent(Intent.ActionSend);
			intent.SetType(contentType);

            var file = new Java.IO.File(filePath);
            Android.Net.Uri uri = GetURI_FileProvider(file);
            if (uri != null)
            {
                //this is the stuff that was missing - but only if you get the uri from FileProvider
                intent.SetFlags(Android.Content.ActivityFlags.GrantReadUriPermission);

            }
            else
            {
                uri = GetUriSimple(file);
            }

            if (uri == null)
            {
                return Task.FromResult(false);
            }

            intent.PutExtra(Intent.ExtraStream, uri);
			intent.PutExtra(Intent.ExtraText, message ?? string.Empty);
			intent.PutExtra(Intent.ExtraSubject, title ?? string.Empty);

			var chooserIntent = Intent.CreateChooser(intent, title ?? string.Empty);
			chooserIntent.SetFlags(ActivityFlags.ClearTop);
			chooserIntent.SetFlags(ActivityFlags.NewTask);
			_context.StartActivity(chooserIntent);

			return Task.FromResult(true);
		}

        Android.Net.Uri GetURI_FileProvider(Java.IO.File file)
        {
            Android.Net.Uri uri = null;

            if (Build.VERSION.SdkInt >= sdkVersion || uri == null)
            {
                try
                {
                    uri = Android.Support.V4.Content.FileProvider.GetUriForFile(_context, _context.PackageName + ".fileprovider", file);
                }
                catch (System.Exception ex)
                {
                    //trace
                    Log.Error("Error in GetURI_FileProvider. {Message}", ex.Message);
                }
            }

            return uri;
        }

        Android.Net.Uri GetUriSimple(Java.IO.File file)
        {
            Android.Net.Uri uri = null;

            if (Build.VERSION.SdkInt < sdkVersion)
            {
                try
                {
                    uri = Android.Net.Uri.FromFile(file);
                }
                catch (System.Exception ex)
                {
                    //trace
                    Log.Error("Error in GetUriSimple. {Message}", ex.Message);
                }
            }

            return uri;
        }
        // Add new functions here
    }
}