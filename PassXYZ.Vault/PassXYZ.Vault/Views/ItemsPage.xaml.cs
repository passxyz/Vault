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
        private readonly ItemsViewModel _viewModel;
        private static bool isChild = false;

        public ItemsPage()
        {
            InitializeComponent();

            BindingContext = _viewModel = new ItemsViewModel();

            if(isChild)
            {
                // This is needed for iOS build. It seems iOS build back button can
                // not work well without this.
                Shell.SetBackButtonBehavior(this, new BackButtonBehavior()
                {
                    Command = new Command(async () => {
                        _viewModel.IsBackButtonClicked = true;
                        await Navigation.PopAsync();

                    })
                });
                Debug.Write($"ItemsPage: child page SetBackButtonBehavior");
            }
            else 
            {
                isChild = true;
                Debug.Write($"ItemsPage: root page");
            }
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

        void OnTap(object sender, ItemTappedEventArgs args)
        {
            var item = args.Item as Item;
            if (item == null)
            {
                return;
            }
            _viewModel.OnItemSelected(item);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }
    }
}