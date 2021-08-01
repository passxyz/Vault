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
        public Command LoginCommand { get; }
        public Command SignUpCommand { get; }
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }
        public PassXYZLib.User CurrentUser { get; set; }

        public LoginViewModel()
        {
            LoginCommand = new Command(OnLoginClicked, ValidateLogin);
            SignUpCommand = new Command(OnSignUpClicked);
            this.PropertyChanged +=
                (_, __) => LoginCommand.ChangeCanExecute();

            CurrentUser = new PassXYZLib.User();
        }

        private bool ValidateLogin()
        {
            return !string.IsNullOrWhiteSpace(_username)
                && !string.IsNullOrWhiteSpace(_password);
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
                CurrentUser.Username = Username;
                CurrentUser.Password = Password;

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
        private async void OnSignUpClicked(object obj)
        {
            Debug.WriteLine("LoginViewModel: OnSignUpClicked");
        }
    }
}
