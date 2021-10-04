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
        private readonly Dictionary<string, MenuItem> showActions;
        private const int CONTEXT_ACTIONS_NUM = 4;
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new ItemDetailViewModel();
            showActions = new Dictionary<string, MenuItem>();
        }

        private void OnMenuShow(object sender, EventArgs e)
        {
            var mi = (MenuItem)sender;

            if (mi.CommandParameter is Field field)
            {
                if (field.IsHide)
                {
                    field.ShowPassword();
                    showActions[field.Key].Text = AppResources.action_id_hide;
                }
                else
                {
                    field.HidePassword();
                    showActions[field.Key].Text = AppResources.action_id_show;
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
                _viewModel.Deleted(field);
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
                if (theViewCell.ContextActions.Count < CONTEXT_ACTIONS_NUM && field.IsProtected)
                {
                    if (!showActions.ContainsKey(field.Key))
                    {
                        showActions[field.Key] = new MenuItem
                        {
                            Text = AppResources.action_id_show
                        };
                        showActions[field.Key].SetBinding(MenuItem.CommandParameterProperty, new Binding("."));
                        showActions[field.Key].Clicked += OnMenuShow;
                    }
                    theViewCell.ContextActions.Add(showActions[field.Key]);
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