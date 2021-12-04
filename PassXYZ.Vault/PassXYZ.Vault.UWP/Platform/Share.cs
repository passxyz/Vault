using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;


using PassXYZ.Utils;
using PassXYZLib;

namespace PassXYZ.UWP
{
	public class Share : IShare
	{
		private string _filePath;
		private string _title;
		private string _message;
		private readonly DataTransferManager _dataTransferManager;

		public Share()
		{
			_dataTransferManager = DataTransferManager.GetForCurrentView();
			_dataTransferManager.DataRequested += new TypedEventHandler<DataTransferManager, DataRequestedEventArgs>(ShareTextHandler);
            Debug.WriteLine("IShare: Share()");
        }

        public string GetPlatformPath(string fileName)
        {
            Debug.WriteLine($"IShare: GetPlatformPath({fileName})");
            return Path.Combine(PxDataFile.TmpFilePath, fileName);
            // return Path.Combine(Package.Current.InstalledLocation.Path, fileName);
        }

        public string BaseUrl
        {
            get { return "ms-appx-web:///Assets/"; }
        }

        /// <summary>
        /// MUST BE CALLED FROM THE UI THREAD
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task Show(string title, string message, string filePath)
		{
			_title = title ?? string.Empty;
			_filePath = filePath;
			_message = message ?? string.Empty;
			
			DataTransferManager.ShowShareUI();

            Debug.WriteLine("IShare: Show()");
            return Task.FromResult(true);
		}

		private async void ShareTextHandler(DataTransferManager sender, DataRequestedEventArgs e)
		{
			DataRequest request = e.Request;

			// Title is mandatory
			request.Data.Properties.Title = string.IsNullOrEmpty(_title) ? Windows.ApplicationModel.Package.Current.DisplayName : _title;

			DataRequestDeferral deferral = request.GetDeferral();

            Debug.WriteLine("IShare: ShareTextHandler()");
            try
            {
				if (!string.IsNullOrWhiteSpace(_filePath))
				{
					StorageFile attachment = await StorageFile.GetFileFromPathAsync(_filePath);
					List<IStorageItem> storageItems = new List<IStorageItem>();
					storageItems.Add(attachment);
					request.Data.SetStorageItems(storageItems);
				}
				request.Data.SetText(_message ?? string.Empty);
			}
			finally
			{
				deferral.Complete();
			}
		}
	}
}
