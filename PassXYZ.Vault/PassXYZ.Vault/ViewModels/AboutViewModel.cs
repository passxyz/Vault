using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

using KeePassLib;
using PassXYZLib;

using PassXYZ.Vault.Resx;

namespace PassXYZ.Vault.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "About";
            OpenWebCommand = new Command(async () => await Browser.OpenAsync(AppResources.about_url));
        }

        public ICommand OpenWebCommand { get; }
        public string GetStoreName()
        {
            return DataStore.GetStoreName();
        }

        public DateTime GetStoreModifiedTime()
        {
            return DataStore.GetStoreModifiedTime();
        }

        public User GetUser()
        {
            return DataStore.CurrentUser;
        }
    }
}