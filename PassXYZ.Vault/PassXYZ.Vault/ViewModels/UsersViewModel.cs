using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
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
        private bool isBusy = false;
        public bool IsBusy
        {
            get => isBusy;
            set => _ = SetProperty(ref isBusy, value);
        }

        public ObservableCollection<PxUser> Users { get; set; }
        public Command LoadUsersCommand { get; }
        public Command AddUserCommand { get; }
        public Command ImportUserCommand { get; }
        public Command ExportUserCommand { get; }

        public UsersViewModel()
        {
            Users = new ObservableCollection<PxUser>();
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
        public void Delete(User user)
        {
            user.Delete();
            Users.Remove((PxUser)user);
            Debug.WriteLine($"UsersViewModel: Delete {user.Username}");
        }

        private async void AddImportedUser(string userName, FileResult result)
        {
            PxUser newUser = new PxUser()
            {
                Username = userName
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
            Users.Add(newUser);
        }

        private async void ImportUser() 
        {
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

            try
            {
                var result = await FilePicker.PickAsync(options);
                if (result != null)
                {
                    if (result.FileName.EndsWith(".kdbx", StringComparison.OrdinalIgnoreCase))
                    {
                        string fileName = System.IO.Path.GetFileNameWithoutExtension(result.FileName);
                        AddImportedUser(fileName, result);
                    }
                    else if (result.FileName.EndsWith(".xyz", StringComparison.OrdinalIgnoreCase)) 
                    {
                        string userName = PxDataFile.GetUserName(result.FileName);
                        AddImportedUser(userName, result);
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

        private void ExecuteLoadUsersCommand()
        {
            var dataFiles = Directory.EnumerateFiles(PxDataFile.DataFilePath, PxDefs.all_xyz);
            foreach (string currentFile in dataFiles)
            {
                string fileName = currentFile.Substring(PxDataFile.DataFilePath.Length + 1);
                string userName = PxDataFile.GetUserName(fileName);
                if (userName != string.Empty && !string.IsNullOrWhiteSpace(userName))
                {
                    Users.Add(
                        new PxUser()
                        {
                            Username = userName
                        });
                }
            }
        }

        private async void OnAddUser(object obj)
        {
            await Shell.Current.Navigation.PushModalAsync(new NavigationPage(new SignUpPage((string username) =>
            {
                Users.Add(
                    new PxUser()
                    {
                        Username = username
                    });
            })));
        }

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

        #region INotifyPropertyChanged
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
