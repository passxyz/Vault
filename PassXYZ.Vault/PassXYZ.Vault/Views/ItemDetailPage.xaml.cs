using System;
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
        private MenuItem showAction;
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }

        private void OnMenuShow(object sender, EventArgs e)
        {
            var mi = (MenuItem)sender;

            if (mi.CommandParameter is Field field)
            {
                if (field.IsHide)
                {
                    field.ShowPassword();
                    showAction.Text = AppResources.action_id_hide;
                }
                else
                {
                    field.HidePassword();
                    showAction.Text = AppResources.action_id_show;
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

            if (mi.CommandParameter is Field field) { }
        }

        private void OnMenuDeleteAsync(object sender, EventArgs e)
        {
            var mi = (MenuItem)sender;

            if (mi.CommandParameter is Field field) { }
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
                if (field.IsProtected)
                {
                    showAction = new MenuItem
                    {
                        Text = AppResources.action_id_show
                    };
                    showAction.SetBinding(MenuItem.CommandParameterProperty, new Binding("."));
                    showAction.Clicked += OnMenuShow;
                    theViewCell.ContextActions.Add(showAction);
                }
            }
        }
    }
}