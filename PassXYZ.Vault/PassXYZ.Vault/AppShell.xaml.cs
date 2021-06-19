using PassXYZ.Vault.ViewModels;
using PassXYZ.Vault.Views;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

using KeePassLib;
using PassXYZLib;
using PassXYZ.Vault.Services;

namespace PassXYZ.Vault
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        IDataStore<Item> DataStore => DependencyService.Get<IDataStore<Item>>();
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
            Routing.RegisterRoute(nameof(NewItemPage), typeof(NewItemPage));
            RootPage.Route = DataStore.RootGroup.Name;
            RootItem.Title = DataStore.RootGroup.Name;
        }

        private async void OnMenuItemClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
