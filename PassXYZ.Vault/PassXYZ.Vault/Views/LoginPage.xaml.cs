using PassXYZ.Vault.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;

using FontAwesome.Solid;
using ZXing.Net.Mobile.Forms;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;

using PassXYZLib;
using PassXYZ.Vault.Resx;

namespace PassXYZ.Vault.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        private readonly LoginViewModel _viewModel;
        FingerprintAvailability availability = FingerprintAvailability.NoFingerprint;
        private CancellationTokenSource _cancel;
        private bool _initialized;
        string authenticationType;
        private static bool isFingerprintCancelled = false;

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
                passwordEntry.Completed += (object sender, EventArgs e) => 
                {
                    _viewModel.OnLoginClicked();
                };
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();

            if (LoginUser.IsPrivacyNoticeAccepted)
            {
                if (!string.IsNullOrEmpty(LoginViewModel.CurrentUser.Username))
                {
                    usernameEntry.Text = LoginViewModel.CurrentUser.Username;
                }

                InitFingerPrintButton();
            }
            else
            {
                bool result = await DisplayAlert("", AppResources.privacy_notice, AppResources.alert_id_yes, AppResources.alert_id_no);
                if (result)
                {
                    LoginUser.IsPrivacyNoticeAccepted = true;
                }
                else
                {
                    messageLabel.Text = AppResources.error_message_privacy_notice;
                    Debug.WriteLine($"LoginPage: {messageLabel.Text}");
                    // PassXYZ.Vault.App.Current.Quit();
                }
            }

        }

        private async void InitFingerPrintButton()
        {
            if (LoginViewModel.CurrentUser.IsDeviceLockEnabled && !LoginViewModel.CurrentUser.IsKeyFileExist)
            {
                Debug.WriteLine("LoginPage: SetupQRCode");

                isFingerprintCancelled = true;
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

                if (availability == FingerprintAvailability.NoFingerprint)
                {
                    GetAvailabilityAsync();
                }

                bool isFingerprintEnabled = await LoginViewModel.CurrentUser.IsFingerprintEnabledAsync();
                if (availability == FingerprintAvailability.Available && isFingerprintEnabled)
                {
                    fpButton.IsVisible = true;
                    if (!App.InBackgroup && !isFingerprintCancelled) { await FingerprintLogin(); }
                }
            }
        }

        private async void ScanKeyFileQRCode()
        {
            var scanPage = new ZXingScannerPage();
            var info = AppResources.settings_security_DLK_Created_success;

            scanPage.OnScanResult += (result) =>
            {
                // Stop scanning
                scanPage.IsScanning = false;
                bool updateUI = false;

                if (result.Text.StartsWith(PxDefs.PxKeyFile))
                {
                    if (_viewModel.CreateKeyFile(result.Text)) 
                    {
                        updateUI = true;
                    }
                    else
                    {
                        info = AppResources.settings_security_DLK_Created_failure;
                    }

                }
                else { info = AppResources.settings_security_DLK_Wrong_Format; }


                // Pop the page and show the result
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Navigation.PopAsync();
                    if (updateUI)
                    {
                        fpButton.Source = new FontAwesome.Solid.IconSource
                        {
                            Icon = FontAwesome.Solid.Icon.Fingerprint
                        };
                        fpButton.IsVisible = false;
                        passwordEntry.IsEnabled = true;
                        messageLabel.Text = " ";
                    }

                    await DisplayAlert(AppResources.settings_security_scan_result, info, AppResources.alert_id_ok);
                });
            };

            // Navigate to our scanner page
            await Navigation.PushAsync(scanPage);
        }

        private async void GetAvailabilityAsync()
        {
            availability = await CrossFingerprint.Current.GetAvailabilityAsync();
        }

        private async void OnSwitchUsersClicked(object sender, EventArgs e)
        {
            var users = User.GetUsersList();
            var username = await DisplayActionSheet(AppResources.pt_id_switchusers, AppResources.action_id_cancel, null, users.ToArray());
            if (username != AppResources.action_id_cancel)
            {
                messageLabel.Text = "";
                LoginViewModel.CurrentUser.Username = usernameEntry.Text = username;
                LoginViewModel.CurrentUser.Password = passwordEntry.Text = "";
                InitFingerPrintButton();
            }
            Debug.WriteLine($"LoginPage: OnSwitchUsersClicked(Username: {LoginViewModel.CurrentUser.Username})");
        }

        private async void OnFingerprintClicked(object sender, EventArgs e)
        {
            if (LoginViewModel.CurrentUser.IsDeviceLockEnabled && !LoginViewModel.CurrentUser.IsKeyFileExist)
            {
                // Import key file or scan QR code
                string[] templates = {
                    AppResources.import_keyfile,
                    AppResources.import_keyfile_scan
                };
                var template = await Shell.Current.DisplayActionSheet(AppResources.import_message1, AppResources.action_id_cancel, null, templates);
                if (template == AppResources.import_keyfile)
                {
                    _viewModel.ImportKeyFile();
                }
                else if (template == AppResources.import_keyfile_scan)
                {
                    ScanKeyFileQRCode();
                }

                Device.BeginInvokeOnMainThread(() =>
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
                });
            }
            else
            {
                isFingerprintCancelled = false;
                await FingerprintLogin();
                Debug.WriteLine("LoginPage: OnFingerprintClicked");
            }

        }

        private async Task SetResultAsync(FingerprintAuthenticationResult result)
        {
            if (result.Authenticated)
            {
                try
                {
                    LoginViewModel.CurrentUser.Password = await LoginViewModel.CurrentUser.GetSecurityAsync();
                    if(LoginViewModel.CurrentUser.Password != null)
                    {
                        _viewModel.OnLoginClicked();
                    }
                    else
                    {
                        messageLabel.Text = AppResources.LoginErrorMessage;
                        passwordEntry.Text = string.Empty;
                    }

                }
                catch (Exception ex)
                {
                    // Possible that device doesn't support secure storage on device.
                    messageLabel.Text = AppResources.LoginErrorMessage;
                    passwordEntry.Text = string.Empty;
                    Debug.WriteLine($"SettingsPage: in SetResultAsync, {ex}");
                }

                Debug.WriteLine("SetResultAsync: authentication is passed successfully.");
            }
            else
            {
                messageLabel.Text = $"{result.Status}: {result.ErrorMessage}";
                Debug.WriteLine($"SetResultAsync: {result.Status}: {result.ErrorMessage}.");
                if (result.Status == FingerprintAuthenticationResultStatus.Canceled)
                {
                    isFingerprintCancelled = true;
                }
            }
        }

        private async Task AuthenticateAsync(string reason, string cancel = null, string fallback = null, string tooFast = null)
        {
            // _cancel = swAutoCancel.IsToggled ? new CancellationTokenSource(TimeSpan.FromSeconds(10)) : new CancellationTokenSource();
            _cancel = new CancellationTokenSource();

            var dialogConfig = new AuthenticationRequestConfiguration("Verify your fingerprint", reason)
            { // all optional
                CancelTitle = cancel,
                FallbackTitle = fallback,
                AllowAlternativeAuthentication = false
            };

            // optional
            dialogConfig.HelpTexts.MovedTooFast = tooFast;

            var result = await Plugin.Fingerprint.CrossFingerprint.Current.AuthenticateAsync(dialogConfig, _cancel.Token);

            await SetResultAsync(result);
        }

        private async Task FingerprintLogin()
        {
            try
            {
                string data = string.Empty;

                if (!string.IsNullOrEmpty(_viewModel.Username))
                {
                    data = await LoginViewModel.CurrentUser.GetSecurityAsync();
                }

                if ((availability == FingerprintAvailability.Available) && (!string.IsNullOrWhiteSpace(data)))
                {
                    if (!_initialized)
                    {
                        _initialized = true;
                        authenticationType = "Auth Type: " + await CrossFingerprint.Current.GetAuthenticationTypeAsync();
                    }
                    fpButton.IsVisible = true;
                    Debug.WriteLine($"Fingerprint is {availability}, {authenticationType}.");
                    await AuthenticateAsync(_viewModel.Username + ": " + AppResources.fingerprint_login_message);
                }
                else
                {
                    fpButton.IsVisible = false;
                }
            }
            catch (Exception ex)
            {
                Debug.Write($"{ex}");
            }
        }

    }
}