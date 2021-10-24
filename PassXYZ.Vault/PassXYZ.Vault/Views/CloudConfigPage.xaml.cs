using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using PassXYZLib;
using PassXYZ.Vault.ViewModels;

namespace PassXYZ.Vault.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CloudConfigPage : ContentPage
    {
        private readonly UsersViewModel _viewModel;

        public CloudConfigPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new UsersViewModel(false);
            messageLabel.BindingContext = _viewModel;
        }
    }
}