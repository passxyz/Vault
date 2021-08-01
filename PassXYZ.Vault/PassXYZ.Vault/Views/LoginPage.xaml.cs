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
        }

        void OnLoginButtonClicked(object sender, EventArgs e)
        {
            _viewModel.OnLoginClicked();
            Debug.WriteLine("LoginPage: OnLoginButtonClicked");
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