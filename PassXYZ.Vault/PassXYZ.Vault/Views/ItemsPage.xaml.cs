using KeePassLib;
using PassXYZ.Vault.ViewModels;
using PassXYZ.Vault.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PassXYZ.Vault.Views
{
    public partial class ItemsPage : ContentPage
    {
        ItemsViewModel _viewModel;

        public ItemsPage()
        {
            InitializeComponent();

            BindingContext = _viewModel = new ItemsViewModel();
            // This is needed for iOS build. It seems iOS build back button can
            // not work well without this.
            Shell.SetBackButtonBehavior(this, new BackButtonBehavior()
            {
                Command = new Command(async () => {
                    _viewModel.IsBackButtonClicked = true;
                    await Navigation.PopAsync();

                })
            });
        }

        private void OnMenuEdit(object sender, EventArgs e)
        {
            var mi = (MenuItem)sender;

            if (mi.CommandParameter is Item item)
            {
                _viewModel.Update(item);
            }
        }

        private async void OnMenuDeleteAsync(object sender, EventArgs e)
        {
            var mi = (MenuItem)sender;

            if (mi.CommandParameter is Item item)
            {
                await _viewModel.DeletedAsync(item);
                Debug.WriteLine("ItemsPage: OnMenuDeleteAsync clicked");
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }
    }
}