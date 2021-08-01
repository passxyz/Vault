using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using PassXYZ.Vault.ViewModels;

namespace PassXYZ.Vault.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FieldEditPage : ContentPage
    {
        private Action<string, string, bool> _updateAction;
        private bool _isNewField = true;

        public FieldEditPage(Action<string, string, bool> updateAction, string key = "", string value = "")
        {
            InitializeComponent();

            Title = keyField.Text = key;
            if(!string.IsNullOrEmpty(key))
            {
                keyField.IsVisible = false;
                checkBox.IsVisible = false;
                _isNewField = false;
            }
            
            valueField.Text = value;

            _updateAction = updateAction;
        }

        public FieldEditPage(Action<string, string, bool> updateAction, string key, string value, bool isKeyVisible = false):this(updateAction, key, value)
        {
            // This is the same as the another constructor, except this part
            if (isKeyVisible)
            {
                keyField.IsVisible = true;
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            bool isProtected = checkBox.IsChecked;
            if(_isNewField)
            {
                _updateAction?.Invoke(keyField.Text, valueField.Text, isProtected);
            }
            else
            {
                _updateAction?.Invoke(keyField.Text, valueField.Text, false);
            }
            _ = await Navigation.PopModalAsync();
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            _ = await Navigation.PopModalAsync();
        }
    }
}