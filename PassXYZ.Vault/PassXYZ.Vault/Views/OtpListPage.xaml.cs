using System;
using System.Diagnostics;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using KeePassLib;
using PassXYZ.Vault.ViewModels;

namespace PassXYZ.Vault.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OtpListPage : ContentPage
    {
        private readonly OtpListViewModel viewModel;

        public OtpListPage ()
        {
            InitializeComponent();
            BindingContext = viewModel = new OtpListViewModel();
            // _ = viewModel.ExecuteGetOtpListCommand();
        }

        private void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            if (!(args.SelectedItem is Item item))
            {
                Debug.WriteLine("OtpListPage: item is null in OnItemSelected().");
                return;
            }

            viewModel.OnItemSelected(item);

            // Manually deselect item.
            OtpListView.SelectedItem = null;
            //viewModel.UpdateTokenDone = true;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = viewModel.ExecuteGetOtpListCommand();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Stop the timer
            viewModel.UpdateTokenDone = false;
            Debug.WriteLine($"OtpListPage:OnDisappearing and UpdateTokenDone={viewModel.UpdateTokenDone}.");
        }

    }
}