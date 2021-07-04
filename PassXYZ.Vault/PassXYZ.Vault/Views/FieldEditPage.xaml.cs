using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using PassXYZ.Vault.ViewModels;

namespace PassXYZ.Vault.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FieldEditPage : ContentPage
    {
        private FieldEditViewModel _viewModel;
        public FieldEditPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new FieldEditViewModel();
            Title = KeyField.Text = _viewModel.Key;
            ValueField.Text = _viewModel.Value;
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            _ = await Navigation.PopModalAsync();
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            _ = await Navigation.PopModalAsync();
        }
    }
}