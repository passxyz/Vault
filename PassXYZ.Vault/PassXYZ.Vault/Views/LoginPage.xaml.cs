using PassXYZ.Vault.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using PassXYZLib;
using PassXYZ.Vault.Resx;

namespace PassXYZ.Vault.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        LoginViewModel _viewModel;
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
        }

        private void OnLoginButtonClicked(object sender, EventArgs e)
        {
            _viewModel.OnLoginClicked();
            Debug.WriteLine("LoginPage: OnLoginButtonClicked");
        }

        private async void OnSwitchUsersClicked(object sender, EventArgs e)
        {
            var users = User.GetUsersList();
            var username = await DisplayActionSheet(AppResources.pt_id_switchusers, AppResources.action_id_cancel, null, users.ToArray());
            if (username != AppResources.action_id_cancel)
            {
                messageLabel.Text = "";
                LoginViewModel.CurrentUser.Username = usernameEntry.Text = username;

                if (!LoginViewModel.CurrentUser.IsKeyFileExist)
                {
                    //SetupQRCode();
                    Debug.WriteLine("LoginPage: SetupQRCode");
                }
                else
                {
                    passwordEntry.IsEnabled = true;
                    fpButton.IsVisible = false;
                    fpButton.Image = "ic_passxyz_fingerprint.png";
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
            Debug.WriteLine($"LoginPage: OnSwitchUsersClicked(Username: {LoginViewModel.CurrentUser.Username})");
        }

        async void OnFingerprintClicked(object sender, EventArgs e)
        {
            Debug.WriteLine("LoginPage: OnFingerprintClicked");
        }

    }
}