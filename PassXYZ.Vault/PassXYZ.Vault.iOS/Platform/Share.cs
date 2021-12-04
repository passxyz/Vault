using Foundation;
using System.IO;
using System.Threading.Tasks;
using UIKit;
using System.Diagnostics;

using PassXYZ.Utils;
using PassXYZLib;

namespace PassXYZ.iOS
{
	public class Share : IShare
	{
        public string GetPlatformPath(string fileName)
        {
            return Path.Combine(PxDataFile.TmpFilePath, fileName);
        }

        public string BaseUrl
        {
            get
            {
                return NSBundle.MainBundle.BundlePath;
            }
        }

        /// <summary>
        /// MUST BE CALLED FROM THE UI THREAD
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task Show(string title, string message, string filePath)
		{
			var items = new NSObject[] { NSUrl.FromFilename(filePath) };
            Debug.WriteLine($"IShare: Show({title}, {message}, {filePath})");
            var activityController = new UIActivityViewController(items, null);
			var vc = GetVisibleViewController();

			NSString[] excludedActivityTypes = null;

			if (excludedActivityTypes != null && excludedActivityTypes.Length > 0)
				activityController.ExcludedActivityTypes = excludedActivityTypes;

			if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
			{
				if (activityController.PopoverPresentationController != null)
				{
					activityController.PopoverPresentationController.SourceView = vc.View;
				}
			}

			await vc.PresentViewControllerAsync(activityController, true);
        }

        UIViewController GetVisibleViewController()
		{
            var rootController = UIApplication.SharedApplication.KeyWindow.RootViewController;

            if (rootController.PresentedViewController == null)
            {
                return rootController;
            }

            if (rootController.PresentedViewController is UINavigationController)
            {
                return ((UINavigationController)rootController.PresentedViewController).TopViewController;
            }

            if (rootController.PresentedViewController is UITabBarController)
            {
                return ((UITabBarController)rootController.PresentedViewController).SelectedViewController;
            }

            return rootController.PresentedViewController;
        }
    }
}
