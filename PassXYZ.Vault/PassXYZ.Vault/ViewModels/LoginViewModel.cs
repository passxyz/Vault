using PassXYZ.Vault.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xamarin.Forms;

using PassXYZ.Vault.Resx;

namespace PassXYZ.Vault.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _username;
        private string _password;
        private string _password2;
        private bool _isDeviceLockEnabled;
        private Action<string> _signUpAction;
        public Command LoginCommand { get; }
        public Command SignUpCommand { get; }
        public Command CancelCommand { get; }
        public string Username
        {
            get => _username;

            set
            {
                _ = SetProperty(ref _username, value);
                CurrentUser.Username = value;
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _ = SetProperty(ref _password, value);
                CurrentUser.Password = value;
            }
        }
        public string Password2
        {
            get => _password2;
            set => SetProperty(ref _password2, value);
        }
        public bool IsDeviceLockEnabled
        {
            get => _isDeviceLockEnabled;
            set {
                _ = SetProperty(ref _isDeviceLockEnabled, value);
                CurrentUser.IsDeviceLockEnabled = value;
            }
        }
        public static PassXYZLib.User CurrentUser { get; set; }

        public LoginViewModel()
        {
            LoginCommand = new Command(OnLoginClicked, ValidateLogin);
            SignUpCommand = new Command(OnSignUpClicked, ValidateSignUp);
            CancelCommand = new Command(OnCancelClicked);
            this.PropertyChanged +=
                (_, __) => LoginCommand.ChangeCanExecute();

            this.PropertyChanged +=
                (_, __) => SignUpCommand.ChangeCanExecute();

            CurrentUser = new PassXYZLib.User();
        }

        public LoginViewModel(Action<string> signUpAction) : this()
        {
            _signUpAction = signUpAction;
        }

        private bool ValidateLogin()
        {
            return !string.IsNullOrWhiteSpace(_username)
                && !string.IsNullOrWhiteSpace(_password);
        }

        private bool ValidateSignUp()
        {
            return !string.IsNullOrWhiteSpace(_username)
                && !string.IsNullOrWhiteSpace(_password)
                && !string.IsNullOrWhiteSpace(_password2)
                && _password.Equals(_password2);
        }

        public void OnAppearing()
        {
            if (DataStore.RootGroup != null) { DataStore.Logout(); }
        }

        public async void OnLoginClicked()
        {
            // Prefixing with `//` switches to a different navigation stack instead of pushing to the active one
            try
            {
                bool status = await DataStore.LoginAsync(CurrentUser);

                if (status)
                {
                    if (AppShell.CurrentAppShell != null)
                    {
                        AppShell.CurrentAppShell.SetRootPageTitle(DataStore.RootGroup.Name);
                        await Shell.Current.GoToAsync($"//{nameof(ItemsPage)}");
                    }
                    else
                    {
                        await Shell.Current.GoToAsync($"//{nameof(AboutPage)}");
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert(AppResources.LoginErrorMessage, ex.Message, AppResources.alert_id_ok);
            }
        }
        private async void OnSignUpClicked()
        {
            if(CurrentUser.IsUserExist)
            {
                await Shell.Current.DisplayAlert(AppResources.SignUpPageTitle, AppResources.SignUpErrorMessage1, AppResources.alert_id_ok);
                return;
            }
            try
            {
                await DataStore.SignUpAsync(CurrentUser);
                _signUpAction?.Invoke(CurrentUser.Username);
                _ = await Shell.Current.Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert(AppResources.SignUpPageTitle, ex.Message, AppResources.alert_id_ok);
            }
            Debug.WriteLine($"LoginViewModel: OnSignUpClicked {Username}, DeviceLock: {IsDeviceLockEnabled}");
        }

        private async void OnCancelClicked()
        {
            _ = await Shell.Current.Navigation.PopModalAsync();
        }
    }
}
