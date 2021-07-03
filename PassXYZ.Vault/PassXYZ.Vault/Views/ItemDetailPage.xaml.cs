using PassXYZ.Vault.ViewModels;
using System;
using System.ComponentModel;
using System.Diagnostics;
using Xamarin.Forms;

using KeePassLib;
using PassXYZLib;

namespace PassXYZ.Vault.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        ItemDetailViewModel _viewModel;
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new ItemDetailViewModel();
        }

        private void OnMenuShow(object sender, EventArgs e)
        {
            var mi = ((MenuItem)sender);
            Field field = mi.CommandParameter as Field;
            Debug.WriteLine($"ItemDetailPage: Show Context Action clicked: {field.Key}");
            if(field != null) { field.ShowPassword(); }
            // CrossClipboard.Current.SetText(Value);
        }

        private void OnMenuEdit(object sender, EventArgs e)
        {
            var mi = (MenuItem)sender;
            Field field = mi.CommandParameter as Field;
            Debug.WriteLine($"ItemDetailPage: Edit Context Action clicked: {field.Key}");
        }

        private void OnMenuDeleteAsync(object sender, EventArgs e)
        {
            var mi = ((MenuItem)sender);
            Field field = mi.CommandParameter as Field;
            Debug.WriteLine($"ItemDetailPage: Delete Context Action clicked: {field.Key}");
        }

        private void OnBindingContextChanged(object sender, EventArgs e)
        {
            base.OnBindingContextChanged();
            if (BindingContext == null)
            {
                return;
            }

            ViewCell theViewCell = (ViewCell)sender;
            var field = theViewCell.BindingContext as Field;
            if (field != null)
            {
                // Debug.WriteLine($"ItemDetailPage: OnBindingContextChanged: {field.Key} {field.IsProtected}");
                if (field.IsProtected)
                {
                    var showAction = new MenuItem
                    {
                        Text = "Show"
                    };
                    showAction.SetBinding(MenuItem.CommandParameterProperty, new Binding("."));
                    showAction.Clicked += OnMenuShow;
                    theViewCell.ContextActions.Add(showAction);
                }
            }
        }
    }
}