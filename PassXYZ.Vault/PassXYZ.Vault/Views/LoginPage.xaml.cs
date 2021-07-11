using PassXYZ.Vault.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

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
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }

        void OnUsernameFilled(object sender, EventArgs e)
        {
            _viewModel.Username = usernameEntry.Text;
            //Debug.WriteLine($"LoginPage: Username is {usernameEntry.Text}.");
        }

        void OnPasswordFilled(object sender, EventArgs e)
        {
            _viewModel.Password = passwordEntry.Text;
            //Debug.WriteLine($"LoginPage: Username is {passwordEntry.Text}.");
        }

        async void OnSwitchUsersClicked(object sender, EventArgs e)
        {
            Debug.WriteLine("LoginPage: OnSwitchUsersClicked");
        }

        async void OnFingerprintClicked(object sender, EventArgs e) 
        {
            Debug.WriteLine("LoginPage: OnFingerprintClicked");
        }

    }
}