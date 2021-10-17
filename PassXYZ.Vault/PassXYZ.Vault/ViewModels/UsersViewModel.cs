using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using Xamarin.Essentials;

using PassXYZLib;
using PassXYZ.Vault.Views;
using PassXYZ.Vault.Resx;

namespace PassXYZ.Vault.ViewModels
{
    public class UsersViewModel : INotifyPropertyChanged
    {
        public bool IsBusy
        {
            get => App.IsBusyToLoadUsers;
            set => _ = SetProperty(ref App.IsBusyToLoadUsers, value);
        }

        public ObservableCollection<PxUser> Users { get; }
        public Command LoadUsersCommand { get; }
        public Command AddUserCommand { get; }
        public Command ImportUserCommand { get; }
        public Command ExportUserCommand { get; }

        public UsersViewModel()
        {
            Users = App.Users;
            LoadUsersCommand = new Command(() => ExecuteLoadUsersCommand());
            AddUserCommand = new Command(OnAddUser);
            ImportUserCommand = new Command(() => ImportUser());
            ExportUserCommand = new Command(() => { ExportUserAsync(); });
            ExecuteLoadUsersCommand();
        }

        public void OnUserSelected(User user)
        {
            if (LoginViewModel.CurrentUser != null)
            {
                LoginViewModel.CurrentUser.Username = user.Username;
            }
            Debug.WriteLine($"UsersViewModel: OnUserSelected {user.Username}");
        }

        /// <summary>
        /// Delete a user.
        /// </summary>
        /// <param name="user">an instance of User</param>
        public async Task DeleteAsync(User user)
        {
            if (IsBusy)
            {
                Debug.WriteLine($"UsersViewModel: is busy and cannot delete {user.Username}");
            }

            IsBusy = true;
            PxUser pxUser = (PxUser)user;
#if PASSXYZ_CLOUD_SERVICE
            await pxUser.DeleteAsync();
#else
            user.Delete();
#endif // PASSXYZ_CLOUD_SERVICE
            Users.Remove((PxUser)user);

            IsBusy = false;
            Debug.WriteLine($"UsersViewModel: Delete {user.Username}");
        }

#if PASSXYZ_CLOUD_SERVICE
        public bool IsSynchronized
        {
            get
            {
                ICloudServices<PxUser> sftp = PxCloudConfig.GetCloudServices();
                return sftp.IsSynchronized;
            }
        }

        public async Task EnableSyncAsync(PxUser user)
        {
            // Upload local
            await user.EnableSyncAsync();
            Debug.WriteLine($"UsersViewModel: EnableSyncAsync for {user.Username}");
        }

        public async Task DisableSyncAsync(PxUser user)
        {
            // Delete remote
            await user.DisableSyncAsync();
            Debug.WriteLine($"UsersViewModel: DisableSyncAsync for {user.Username}");
        }
#endif // PASSXYZ_CLOUD_SERVICE

        private async void AddImportedUser(string userName, FileResult result, bool isDeviceLockEnabled = false)
        {
            PxUser newUser = new PxUser()
            {
                Username = userName,
                IsDeviceLockEnabled = isDeviceLockEnabled
            };

            if (System.IO.File.Exists(newUser.Path))
            {
                await Shell.Current.DisplayAlert(AppResources.action_id_import + $" {userName}", AppResources.import_error_user_exits, AppResources.alert_id_ok);
                return;
            }

            var stream = await result.OpenReadAsync();
            var fileStream = File.Create(newUser.Path);
            stream.Seek(0, SeekOrigin.Begin);
            stream.CopyTo(fileStream);
            fileStream.Close();

            if (IsBusy)
            {
                Debug.WriteLine($"UsersViewModel: is busy and cannot import {newUser.Username}");
                return;
            }

            IsBusy = true;
            Users.Add(newUser);
            IsBusy = false;
        }

        private async void ImportUser()
        {
            string[] templates = {
                AppResources.import_data_file,
                AppResources.import_data_file_ex
            };

            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, new[] { "public.my.comic.extension" } }, // or general UTType values
                    { DevicePlatform.Android, new[] { "application/comics" } },
                    { DevicePlatform.UWP, new[] { ".cbr", ".cbz" } },
                    { DevicePlatform.macOS, new[] { "cbr", "cbz" } }, // or general UTType values
                });

            var options = new PickOptions
            {
                PickerTitle = AppResources.import_message1,
                //FileTypes = customFileType,
            };

            bool isDeviceLockEnabled = false;

            var template = await Shell.Current.DisplayActionSheet(AppResources.import_message1, AppResources.action_id_cancel, null, templates);
            if (template == AppResources.import_data_file_ex)
            {
                isDeviceLockEnabled = true;
            }

            try
            {
                var result = await FilePicker.PickAsync(options);
                if (result != null)
                {
                    if (result.FileName.EndsWith(".kdbx", StringComparison.OrdinalIgnoreCase))
                    {
                        string fileName = System.IO.Path.GetFileNameWithoutExtension(result.FileName);
                        AddImportedUser(fileName, result, isDeviceLockEnabled);
                    }
                    else if (result.FileName.EndsWith(".xyz", StringComparison.OrdinalIgnoreCase)) 
                    {
                        string userName = PxDataFile.GetUserName(result.FileName);
                        AddImportedUser(userName, result, isDeviceLockEnabled);
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert(AppResources.action_id_import, AppResources.message_id_import_failure + $" {result.FileName}.", AppResources.alert_id_ok);
                    }
                }
            }
            catch (Exception ex)
            {
                // The user canceled or something went wrong
                Debug.WriteLine($"UsersViewModel: ImportUser, {ex}");
            }
        }

        private async void ExportUserAsync()
        {
            if (LoginViewModel.CurrentUser.Path == null) 
            {
                await Shell.Current.DisplayAlert(AppResources.settings_export_title, AppResources.export_error1, AppResources.alert_id_ok);
                return;
            }

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = AppResources.settings_export_title + $": {LoginViewModel.CurrentUser.Username}",
                File = new ShareFile(LoginViewModel.CurrentUser.Path)
            });
        }

        private async void ExecuteLoadUsersCommand()
        {
            if (IsBusy)
            {
                Debug.WriteLine("UsersViewModel: is busy and cannot load users");
                return;
            }

            IsBusy = true;

            Users.Clear();
            var users = await PxUser.LoadLocalUsersAsync();
            foreach (PxUser user in users)
            {
                Users.Add(user);
            }

            IsBusy = false;
        }

        private async void OnAddUser(object obj)
        {
            await Shell.Current.Navigation.PushModalAsync(new NavigationPage(new SignUpPage((string username) =>
            {
                if (IsBusy)
                {
                    Debug.WriteLine($"UsersViewModel: is busy and cannot add {username}");
                    return;
                }

                IsBusy = true;
                Users.Add(
                    new PxUser()
                    {
                        Username = username
                    });
                IsBusy = false;
            })));
        }

        #region INotifyPropertyChanged
        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName] string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
