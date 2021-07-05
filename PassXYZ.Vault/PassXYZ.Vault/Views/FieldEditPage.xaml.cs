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
        private Action<string, string> _updateAction;

        public FieldEditPage(string key, string value, Action<string, string> updateAction)
        {
            InitializeComponent();

            Title = KeyField.Text = key;
            KeyField.IsVisible = false;
            ValueField.Text = value;
            _updateAction = updateAction;
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            _updateAction?.Invoke(KeyField.Text, ValueField.Text);
            _ = await Navigation.PopModalAsync();
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            _ = await Navigation.PopModalAsync();
        }
    }
}