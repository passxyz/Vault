using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

using PassXYZLib;
using PassXYZ.Vault.Views;


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

        public UsersViewModel()
        {
            Users = new ObservableCollection<PxUser>();
            LoadUsersCommand = new Command(() => ExecuteLoadUsersCommand());
            AddUserCommand = new Command(OnAddUser);
            ExecuteLoadUsersCommand();
        }

        public void OnUserSelected(User user)
        {
            if (LoginViewModel.CurrentUser != null)
            {
                LoginViewModel.CurrentUser.Username = user.Username;
            }
            Debug.Write($"UsersViewModel: OnUserSelected {user.Username}");
        }

        /// <summary>
        /// Delete a user.
        /// </summary>
        /// <param name="user">an instance of User</param>
        public void Delete(User user)
        {
            user.Delete();
            Users.Remove((PxUser)user);
            Debug.Write($"UsersViewModel: Delete {user.Username}");
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
