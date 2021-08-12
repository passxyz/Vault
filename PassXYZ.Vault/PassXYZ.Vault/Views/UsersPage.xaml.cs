using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using PassXYZLib;
using PassXYZ.Vault.ViewModels;

namespace PassXYZ.Vault.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UsersPage : ContentPage
    {
        private readonly UsersViewModel _viewModel;
        public UsersPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new UsersViewModel();
        }

        private void OnMenuDeleteAsync(object sender, EventArgs e)
        {
            var mi = (MenuItem)sender;

            if (mi.BindingContext is User user)
            {
                _viewModel.Delete(user);
            }
        }

        private void OnUserTap(object sender, ItemTappedEventArgs args)
        {
            var user = args.Item as User;
            if (user == null)
            {
                return;
            }
            _viewModel.OnUserSelected(user);
        }

    }
}