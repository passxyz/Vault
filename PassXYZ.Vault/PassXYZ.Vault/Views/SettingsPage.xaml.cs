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

using ZXing.Net.Mobile.Forms;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;

using KeePassLib;
using KeePassLib.Utility;
using PassXYZLib;

using PassXYZ.Vault.Resx;
using PassXYZ.Vault.ViewModels;

namespace PassXYZ.Vault.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        private readonly LoginViewModel _viewModel;
        FingerprintAvailability availability = FingerprintAvailability.NoFingerprint;
        private CancellationTokenSource _cancel;
        private bool _initialized;
        private string authenticationType;

        public SettingsPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new LoginViewModel(true);
            GetAvailabilityAsync();
            timerField.Text = AppResources.settings_timer_title + " " + PxUser.AppTimeout.ToString() + " " + AppResources.settings_timer_unit_seconds;
        }

        private async void GetAvailabilityAsync()
        {
            availability = await CrossFingerprint.Current.GetAvailabilityAsync();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            // Refresh username and device lock status
            _ = _viewModel.Username;
            if (availability == FingerprintAvailability.Available)
            {
                FingerPrintSwitcher.IsEnabled = true;
                FingerPrintSwitcher.IsVisible = true;
                if (!_initialized)
                {
                    _initialized = true;
                    authenticationType = "Auth Type: " + await CrossFingerprint.Current.GetAuthenticationTypeAsync();
                }

                try
                {
                    string data = await LoginViewModel.CurrentUser.GetSecurityAsync();
                    FingerPrintSwitcher.IsToggled = data != null;
                }
                catch (Exception ex)
                {
                    // Possible that device doesn't support secure storage on device.
                    FingerPrintSwitcher.IsEnabled = false;
                    FingerPrintSwitcher.IsToggled = false;
                    FingerprintStatus.Text = AppResources.settings_fingerprint_disabled;
                    Debug.WriteLine($"{ex}");
                }
            }
            else
            {
                FingerPrintSwitcher.IsEnabled = false;
                FingerPrintSwitcher.IsVisible = false;
                FingerprintStatus.Text = AppResources.settings_fingerprint_disabled;
            }
            Debug.WriteLine($"Fingerprint is {availability}, {authenticationType}.");

            // Initialze Key File status
            keyFileField.Text = _viewModel.IsDeviceLockEnabled ? AppResources.settings_keyFileField_Value1 : AppResources.settings_keyFileField_Value2;
        }

        private async void OnTimerTappedAsync(object sender, System.EventArgs e)
        {
            List<string> timerlist = new List<string>();
            var timer_30seconds = "30 " + AppResources.settings_timer_unit_seconds;
            timerlist.Add(timer_30seconds);
            var timer_2minutes = "2 " + AppResources.settings_timer_unit_minutes;
            timerlist.Add(timer_2minutes);
            var timer_5minutes = "5 " + AppResources.settings_timer_unit_minutes;
            timerlist.Add(timer_5minutes);
            var timer_10minutes = "10 " + AppResources.settings_timer_unit_minutes;
            timerlist.Add(timer_10minutes);
            var timer_30minutes = "30 " + AppResources.settings_timer_unit_minutes;
            timerlist.Add(timer_30minutes);
            var timer_1hour = "1 " + AppResources.settings_timer_unit_hour;
            timerlist.Add(timer_1hour);

            var timerValue = await DisplayActionSheet(AppResources.settings_timer_title, AppResources.action_id_cancel, null, timerlist.ToArray());
            if (timerValue == timer_30seconds) { PxUser.AppTimeout = 30; }
            else if (timerValue == timer_2minutes) { PxUser.AppTimeout = 120; }
            else if (timerValue == timer_5minutes) { PxUser.AppTimeout = 300; }
            else if (timerValue == timer_10minutes) { PxUser.AppTimeout = 600; }
            else if (timerValue == timer_30minutes) { PxUser.AppTimeout = 1800; }
            else if (timerValue == timer_1hour) { PxUser.AppTimeout = 3600; }

            timerField.Text = AppResources.settings_timer_title + " " + PxUser.AppTimeout.ToString() + " " + AppResources.settings_timer_unit_seconds;
        }

        private async void OnSecuritySettingsTappedAsync(object sender, System.EventArgs e)
        {
            string msg = _viewModel.GetDeviceLockData();
            if (!string.IsNullOrEmpty(msg))
            {
                var qrcodePage = new ContentPage();
                var layout = new StackLayout
                {
                    Spacing = 10,
                    Padding = new Thickness(10, 20, 0, 0)
                };
                ZXingBarcodeImageView barcode = new ZXingBarcodeImageView
                {
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    HeightRequest = 300,
                    WidthRequest = 300,
                };
                barcode.BarcodeFormat = ZXing.BarcodeFormat.QR_CODE;
                barcode.BarcodeOptions.Width = 300;
                barcode.BarcodeOptions.Height = 300;
                barcode.BarcodeValue = PxDefs.PxKeyFile + msg;

                var qrcodeTitle = new Label()
                {
                    Text = AppResources.field_id_username + ": " + _viewModel.Username,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    FontAttributes = FontAttributes.Bold
                };

                var button = new Button
                {
                    Text = AppResources.alert_id_ok,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center
                };
                button.Clicked += async (sender1, e1) => { await Navigation.PopModalAsync(); };

                layout.Children.Add(qrcodeTitle);
                layout.Children.Add(barcode);
                layout.Children.Add(button);
                qrcodePage.Content = layout;

                await Navigation.PushModalAsync(new NavigationPage(qrcodePage));
            }

        }

        async Task AuthenticateAsync(string reason, string cancel = null, string fallback = null, string tooFast = null)
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

            var result = await CrossFingerprint.Current.AuthenticateAsync(dialogConfig, _cancel.Token);

            SetResultAsync(result);
        }

        private async void SetResultAsync(FingerprintAuthenticationResult result)
        {
            if (result.Authenticated)
            {
                try
                {
                    await LoginViewModel.CurrentUser.SetSecurityAsync();
                }
                catch (Exception ex)
                {
                    // Possible that device doesn't support secure storage on device.
                    Debug.WriteLine($"SettingsPage: in SetResultAsync, {ex}");
                }
            }
            else
            {
                FingerprintStatus.Text = $"{result.Status}: {result.ErrorMessage}";
            }
        }

        private async void OnSwitcherToggledAsync(object sender, ToggledEventArgs e)
        {
            if(availability == FingerprintAvailability.NoFingerprint) { return; }

            if (e.Value)
            {
                try
                {
                    string data = await LoginViewModel.CurrentUser.GetSecurityAsync();
                    if (data == null)
                    {
                        if (_initialized)
                        {
                            await AuthenticateAsync(AppResources.fingerprint_login_message);
                            Debug.WriteLine($"OnSwitcherToggled: e.Value={e.Value}, turn on fingerprint.");
                        }
                        else
                        {
                            Debug.WriteLine($"OnSwitcherToggled: e.Value={e.Value}, fingerprint is not initialized.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Possible that device doesn't support secure storage on device.
                    Debug.WriteLine($"{ex}");
                }
            }
            else
            {
                // Turn off fingerprint
                _ = await LoginViewModel.CurrentUser.DisableSecurityAsync();
            }
        }
    }
}