using PassXYZ.Vault.ViewModels;
using PassXYZ.Vault.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xamarin.Forms;

namespace PassXYZ.Vault
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public static AppShell CurrentAppShell = null;
        public string CurrentRoute = string.Empty;
        public string TargetRoute = string.Empty;
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
            Routing.RegisterRoute(nameof(NotesPage), typeof(NotesPage));
            Routing.RegisterRoute(nameof(NewItemPage), typeof(NewItemPage));
            Routing.RegisterRoute(nameof(SignUpPage), typeof(SignUpPage));
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(SearchPage), typeof(SearchPage));
            Routing.RegisterRoute("group", typeof(ItemsPage));
            CurrentAppShell = this;
        }

        private async void OnMenuItemClicked(object sender, EventArgs e)
        {
            await Current.GoToAsync("//LoginPage");
        }

        protected override void OnNavigating(ShellNavigatingEventArgs args)
        {
            base.OnNavigating(args);

            if(args.Current != null) 
            {
                //Debug.WriteLine($"AppShell: source={args.Current.Location}, target={args.Target.Location}");
                CurrentRoute = args.Current.Location.ToString();
                TargetRoute = args.Target.Location.ToString();
            }
        }

        public void SetRootPageTitle(string name)
        {
            //RootPage.Route = name;
            RootItem.Title = name;
            RootItem.FlyoutItemIsVisible = true;
        }
    }
}
