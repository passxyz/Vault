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
    public partial class NotesPage : ContentPage
    {
        private readonly ItemDetailViewModel _viewModel;
        public NotesPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new ItemDetailViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            //webView.Text = _viewModel.Description;
            var htmlSource = new HtmlWebViewSource
            {
                Html = _viewModel.Description
            };
            webView.Source = htmlSource;
        }
    }
}