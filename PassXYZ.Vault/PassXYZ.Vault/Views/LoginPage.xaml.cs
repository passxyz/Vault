using PassXYZ.Vault.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using FontAwesome.Solid;

using PassXYZLib;
using PassXYZ.Vault.Resx;

namespace PassXYZ.Vault.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        private readonly LoginViewModel _viewModel;

        public LoginPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new LoginViewModel();

            if(User.GetUsersList().Count > 1)
            {
                switchUsersButton.IsVisible = true;
            }

            if (Device.RuntimePlatform == Device.iOS)
            {
                passwordEntry.ReturnType = ReturnType.Next;
            }
            else
            {
                passwordEntry.ReturnType = ReturnType.Done;
                passwordEntry.Completed += OnLoginButtonClicked;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
            if (!string.IsNullOrEmpty(LoginViewModel.CurrentUser.Username))
            {
                usernameEntry.Text = LoginViewModel.CurrentUser.Username;
            }
            Debug.WriteLine($"LoginPage: OnAppearing => CurrentUser: {LoginViewModel.CurrentUser.Username}, Username: {_viewModel.Username}");

            InitFingerPrintButton();
        }

        private void OnLoginButtonClicked(object sender, EventArgs e)
        {
            _viewModel.OnLoginClicked();
            Debug.WriteLine("LoginPage: OnLoginButtonClicked");
        }

        private void InitFingerPrintButton() 
        {
            if (LoginViewModel.CurrentUser.IsDeviceLockEnabled && !LoginViewModel.CurrentUser.IsKeyFileExist)
            {
                Debug.WriteLine("LoginPage: SetupQRCode");

                // isFingerprintCancelled = true;
                fpButton.Source = new IconSource
                {
                    Icon = FontAwesome.Solid.Icon.Qrcode,
                    Color = (Color)Application.Current.Resources["Primary"],
                    Size = 32
                };
                // fpButton.Clicked += OnScanQRCodeClicked;
                fpButton.IsVisible = true;
                passwordEntry.IsEnabled = false;
                messageLabel.Text = AppResources.settings_security_DLK_message1 + LoginViewModel.CurrentUser.Username + ".";
            }
            else
            {
                fpButton.IsVisible = false;
                passwordEntry.IsEnabled = true;
                messageLabel.Text = "";
                fpButton.Source = new IconSource
                {
                    Icon = FontAwesome.Solid.Icon.Fingerprint,
                    Color = (Color)Application.Current.Resources["Primary"],
                    Size = 32
                };
                //if (availability == FingerprintAvailability.NoFingerprint)
                //{
                //    GetAvailabilityAsync();
                //}
                //Debug.WriteLine($"Change to user: {username}, fingerprint: {availability}");

                //if (availability == FingerprintAvailability.Available)
                //{
                //    fpButton.IsVisible = true;
                //    await FingerprintLogin();
                //}
            }
        }

        private async void OnSwitchUsersClicked(object sender, EventArgs e)
        {
            var users = User.GetUsersList();
            var username = await DisplayActionSheet(AppResources.pt_id_switchusers, AppResources.action_id_cancel, null, users.ToArray());
            if (username != AppResources.action_id_cancel)
            {
                messageLabel.Text = "";
                LoginViewModel.CurrentUser.Username = usernameEntry.Text = username;
                InitFingerPrintButton();
            }
            Debug.WriteLine($"LoginPage: OnSwitchUsersClicked(Username: {LoginViewModel.CurrentUser.Username})");
        }

        private async void OnFingerprintClicked(object sender, EventArgs e)
        {
            Debug.WriteLine("LoginPage: OnFingerprintClicked");
        }


    }
}