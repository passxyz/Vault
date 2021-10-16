using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Xamarin.Forms;
using Xamarin.Essentials;

using PassXYZLib;

using PassXYZ.Vault.Resx;
using PassXYZ.Vault.ViewModels;
using System.Threading.Tasks;

namespace PassXYZ.Vault.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        private ItemDetailViewModel _viewModel;
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new ItemDetailViewModel();
        }

        private void OnMenuShow(object sender, EventArgs e)
        {
            var mi = (MenuItem)sender;

            if (mi.CommandParameter is Field field)
            {
                if (field.IsProtected)
                {
                    if (field.ShowContextAction != null) 
                    {
                        MenuItem menuItem = (MenuItem)field.ShowContextAction;
                        if (field.IsHide)
                        {
                            field.ShowPassword();
                            menuItem.Text = AppResources.action_id_hide;
                        }
                        else
                        {
                            field.HidePassword();
                            menuItem.Text = AppResources.action_id_show;
                        }
                    }
                }
            }
        }

        private async void OnMenuCopyAsync(object sender, EventArgs e)
        {
            var mi = (MenuItem)sender;

            if (mi.CommandParameter is Field field)
            {
                await Clipboard.SetTextAsync(field.Value);
            }
        }

        private void OnMenuEdit(object sender, EventArgs e)
        {
            var mi = (MenuItem)sender;

            if (mi.CommandParameter is Field field)
            {
                _viewModel.Update(field);
            }
        }

        private void OnMenuDeleteAsync(object sender, EventArgs e)
        {
            var mi = (MenuItem)sender;

            if (mi.CommandParameter is Field field)
            {
                _viewModel.DeletedAsync(field);
            }
        }

        private void OnBindingContextChanged(object sender, EventArgs e)
        {
            base.OnBindingContextChanged();
            if (BindingContext == null)
            {
                return;
            }

            ViewCell theViewCell = (ViewCell)sender;
            if (theViewCell.BindingContext is Field field)
            {
                // We need to check CONTEXT_ACTIONS_NUM to prevent showAction will be added multiple times.
                MenuItem menuItem = theViewCell.ContextActions[3];
                if (field.IsProtected)
                {
                    // Keep ContextAction of show / hide password
                    field.ShowContextAction = theViewCell.ContextActions[3];
                    menuItem.IsEnabled = true;
                }
                else 
                {
                    menuItem.IsEnabled = false;
                }
            }
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }
    }
}