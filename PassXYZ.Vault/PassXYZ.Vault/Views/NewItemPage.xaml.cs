using KeePassLib;
using PassXYZ.Vault.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using PassXYZLib;

namespace PassXYZ.Vault.Views
{
    public partial class NewItemPage : ContentPage
    {
        NewItemViewModel _viewModel;
        public Item Item { get; set; }

        public NewItemPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new NewItemViewModel();
        }

        public NewItemPage(ItemSubType type) : this()
        {
            _viewModel.Type = type;
        }
    }
}