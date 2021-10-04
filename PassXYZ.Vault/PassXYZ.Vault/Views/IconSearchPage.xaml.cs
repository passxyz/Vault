using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using KeePassLib;
using PassXYZLib;
using PassXYZ.Vault.ViewModels;

namespace PassXYZ.Vault.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class IconSearchPage : ContentPage
    {
        private string searchText = null;
        private readonly IconSearchViewModel viewModel;

        public IconSearchPage()
        {
            InitializeComponent();
            BindingContext = viewModel = new IconSearchViewModel();
        }

        public IconSearchPage(Item item)
        {
            InitializeComponent();
            BindingContext = viewModel = new IconSearchViewModel(item);
        }

        private void OnPxIconSelected(object sender, SelectedItemChangedEventArgs args)
        {
            var icon = args.SelectedItem as PxIcon;
            if (icon == null)
            {
                Debug.WriteLine("OnPxIconSelected: icon is null.");
                return;
            }
            else { viewModel.SelectedIcon = icon; }
        }

        private async void OnMenuDeleteAsync(object sender, EventArgs e)
        {
            var mi = (MenuItem)sender;

            if (mi.BindingContext is PxIcon icon)
            {
                _ = await viewModel.DeleteCustomIconAsync(icon);
                Debug.WriteLine("IconSearchPage: OnMenuDeleteAsync clicked");
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        private void OnSearchBarButtonPressed(object sender, EventArgs args)
        {
            // Get the search text.
            SearchBar searchBar = (SearchBar)sender;
            searchText = searchBar.Text;

            viewModel.Search(searchText);
        }
    }
}