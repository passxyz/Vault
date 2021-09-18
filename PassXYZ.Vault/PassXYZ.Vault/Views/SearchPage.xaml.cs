using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using KeePassLib;
using PassXYZ.Vault.ViewModels;

namespace PassXYZ.Vault.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SearchPage : ContentPage
    {
        private readonly Item currentGroup = null;
        private string searchText = null;
        private readonly ItemsViewModel viewModel;

        public SearchPage ()
        {
            InitializeComponent();
            BindingContext = viewModel = new ItemsViewModel();
            _ = viewModel.ExecuteSearchCommand(null, null);
        }

        public SearchPage(Item itemGroup) : this()
        {
            currentGroup = itemGroup;
        }

        /// <summary>
        /// Context action "Edit".
        /// Edit an entry or a group
        /// </summary>
        /// <param name="sender">The Id of the item.</param>
        /// <param name="e">The Id of the item.</param>
        private void OnMenuEdit(object sender, EventArgs e)
        {
            var mi = (MenuItem)sender;

            if (mi.CommandParameter is Item item)
            {
                viewModel.Update(item);
            }
        }

        private void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            var item = args.SelectedItem as Item;
            if (item == null)
            {
                Debug.WriteLine("OnItemSelected: item is null.");
                return;
            }

            viewModel.OnItemSelected(item);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        private async void OnSearchBarButtonPressed(object sender, EventArgs args)
        {
            // Get the search text.
            SearchBar searchBar = (SearchBar)sender;
            searchText = searchBar.Text;

            await viewModel.ExecuteSearchCommand(searchText, null);
        }
    }
}