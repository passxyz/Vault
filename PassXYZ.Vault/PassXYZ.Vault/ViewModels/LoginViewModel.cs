using PassXYZ.Vault.Views;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace PassXYZ.Vault.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public Command LoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new Command(OnLoginClicked);
        }

        public void OnAppearing()
        {
            if (DataStore.RootGroup != null) { DataStore.Logout(); }
        }

        private async void OnLoginClicked(object obj)
        {
            // Prefixing with `//` switches to a different navigation stack instead of pushing to the active one
            bool status = await DataStore.LoginAsync("", "");
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
    }
}
