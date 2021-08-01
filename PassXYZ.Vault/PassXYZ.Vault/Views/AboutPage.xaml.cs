using System;
using System.ComponentModel;
using System.Diagnostics;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using PassXYZ.Vault.ViewModels;
using PassXYZ.Vault.Resx;

namespace PassXYZ.Vault.Views
{
    public partial class AboutPage : ContentPage
    {
        AboutViewModel viewModel;
        public AboutPage()
        {
            InitializeComponent();

            viewModel = new AboutViewModel();
            BindingContext = viewModel;
            DatabaseName.Text = viewModel.GetStoreName();
            if (string.IsNullOrWhiteSpace(DatabaseName.Text))
            {
                PassXYZLib.User user = viewModel.GetUser();
                string username = user == null ? "" : user.Username;
                DatabaseName.Text = username + "*";
            }

            DateTime localTime = viewModel.GetStoreModifiedTime().ToLocalTime();
            LastModifiedDate.Text = localTime.ToLongDateString();
            LastModifiedTime.Text = localTime.ToLongTimeString();

            var version = AppResources.Version + " " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
#if DEBUG
            version = version + " (Debug)";
#endif
            AppVersion.Text = version;
            Debug.WriteLine($"Version: {version}");
        }
        void OnProblemReportSendClicked(object sender, EventArgs e)
        {
        }
    }
}