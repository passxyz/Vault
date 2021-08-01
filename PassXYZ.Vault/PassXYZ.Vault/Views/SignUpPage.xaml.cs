using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using PassXYZ.Vault.ViewModels;

namespace PassXYZ.Vault.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SignUpPage : ContentPage
    {
        LoginViewModel _viewModel;

        public SignUpPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new LoginViewModel();
        }

        void OnKeyFileSwitcherToggled(object sender, ToggledEventArgs e)
        {
            Debug.WriteLine("SignUpPage: OnKeyFileSwitcherToggled");
        }
    }
}