using System;
using System.Diagnostics;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using PassXYZLib;
using PassXYZ.Vault.ViewModels;
using PassXYZ.Vault.Resx;

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

        private async void OnMenuDeleteAsync(object sender, EventArgs e)
        {
            var mi = (MenuItem)sender;

            if (mi.BindingContext is User user)
            {
                await _viewModel.DeleteAsync(user);
            }
        }

        private async void OnMenuChangeStatus(object sender, EventArgs e)
        {
            var mi = (MenuItem)sender;

#if PASSXYZ_CLOUD_SERVICE
            if (mi.BindingContext is PxUser user)
            {
                if (user.SyncStatus == PxCloudSyncStatus.PxSynced)
                {
                    await _viewModel.DisableSyncAsync(user);
                }
                else if (user.SyncStatus == PxCloudSyncStatus.PxLocal)
                {
                    await _viewModel.EnableSyncAsync(user);
                }
                else 
                {
                    Debug.WriteLine($"UsersViewModel: error status for {user.Username}");
                }
            }
#endif // PASSXYZ_CLOUD_SERVICE
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

        private void OnBindingContextChanged(object sender, EventArgs e)
        {
            base.OnBindingContextChanged();
            if (BindingContext == null)
            {
                return;
            }

            ViewCell theViewCell = (ViewCell)sender;
            if (theViewCell.BindingContext is PxUser user)
            {
                // We need to check CONTEXT_ACTIONS_NUM to prevent showAction will be added multiple times.
                MenuItem menuItem = theViewCell.ContextActions[1];
#if PASSXYZ_CLOUD_SERVICE
                if (user.SyncStatus == PxCloudSyncStatus.PxLocal && PxCloudConfig.IsConfigured && _viewModel.IsSynchronized)
                {
                    // Keep ContextAction of show / hide password
                    menuItem.IsEnabled = true;
                    menuItem.Text = AppResources.action_id_enable_sync;
                }
                else if (user.SyncStatus == PxCloudSyncStatus.PxSynced)
                {
                    menuItem.IsEnabled = true;
                    menuItem.Text = AppResources.action_id_disable_sync;
                }
                else
                {
                    menuItem.IsEnabled = false;
                }
#else
                menuItem.IsEnabled = false;
#endif // PASSXYZ_CLOUD_SERVICE
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Debug.WriteLine($"LoginPage: OnAppearing => CurrentUser: {LoginViewModel.CurrentUser.Username}");
        }
    }
}