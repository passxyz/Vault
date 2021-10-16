using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using PassXYZLib;
using PassXYZ.Vault.Services;
using PassXYZ.Vault.Views;
using PassXYZ.Vault.ViewModels;

namespace PassXYZ.Vault
{
    public partial class App : Application
    {
        public static bool InBackgroup = false;
        private static bool _isLogout = false;
        public static ObservableCollection<PxUser> Users { get; set; }
        public static bool IsBusyToLoadUsers = false;
        public App()
        {
            InitializeComponent();

            DependencyService.Register<DataStore>();
            Users = new ObservableCollection<PxUser>();
            MainPage = new AppShell();
        }

        protected override async void OnStart()
        {
            InBackgroup = false;
            InitTestDb();
#if PASSXYZ_CLOUD_SERVICE
            await LoginViewModel.SynchronizeUsersAsync();
#endif // PASSXYZ_CLOUD_SERVICE
            Debug.WriteLine($"PassXYZ: OnStart, InBackgroup={InBackgroup}");
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
            InBackgroup = true;
            Debug.WriteLine($"PassXYZ: OnSleep, InBackgroup={InBackgroup}");

            // Lock screen after timeout
            Device.StartTimer(TimeSpan.FromSeconds(PxUser.AppTimeout), () =>
            {
                if (InBackgroup)
                {
                    //_ = Task.Factory.StartNew(async () =>
                    //  {
                    //      await Shell.Current.GoToAsync("//LoginPage");
                    //  });
                    _isLogout = true;
                    Debug.WriteLine("PassXYZ: Timer, force logout.");
                    return false;
                }
                else
                {
                    Debug.WriteLine("PassXYZ: Timer, running in foreground.");
                    return false;
                }
            });
        }

        protected override void OnResume()
        {
            InBackgroup = false;
            if (_isLogout)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Shell.Current.GoToAsync("//LoginPage");
                });
                _isLogout = false;
                Debug.WriteLine("PassXYZ: OnResume, force logout");
            }

            Debug.WriteLine($"PassXYZ: OnResume, InBackgroup={InBackgroup}");
        }

    [System.Diagnostics.Conditional("DEBUG")]
        private void InitTestDb()
        {
            foreach(EmbeddedDatabase eDb in TEST_DB.DataFiles)
            {
                if (!File.Exists(eDb.Path))
                {
                    var assembly = this.GetType().GetTypeInfo().Assembly;
                    using (var stream = assembly.GetManifestResourceStream(eDb.ResourcePath))
                    using (var fileStream = new FileStream(eDb.Path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                    {
                        stream.CopyTo(fileStream);
                    }
                }
            }
        }
    }
}
