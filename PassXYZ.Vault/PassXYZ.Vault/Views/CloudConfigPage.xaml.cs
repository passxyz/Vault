using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using PassXYZLib;
using PassXYZ.Vault.Resx;
using PassXYZ.Vault.ViewModels;

namespace PassXYZ.Vault.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CloudConfigPage : ContentPage
    {
        private readonly UsersViewModel _viewModel;

        public CloudConfigPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new UsersViewModel(false);
            messageLabel.BindingContext = _viewModel;
        }

        private async void OnSwitchToggled(object sender, ToggledEventArgs e)
        {
            // Perform an action after examining e.Value
            if (e.Value)
            {
                isSyncing.IsRunning = true;
                PxCloudConfig.IsEnabled = true;
                messageLabel.Text = AppResources.user_id_syncing;
                await LoginViewModel.SynchronizeUsersAsync();
                isSyncing.IsRunning = false;
                messageLabel.Text = AppResources.message_id_sync_success;
            }
            else
            {
                PxCloudConfig.IsEnabled = false;
            }
            Debug.WriteLine($"CloudConfigPage: PxCloudConfig={PxCloudConfig.IsEnabled}");
        }
    }
}