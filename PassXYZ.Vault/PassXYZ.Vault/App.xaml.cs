using System;
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

namespace PassXYZ.Vault
{
    public partial class App : Application
    {
        public static bool InBackgroup = false;
        public App()
        {
            InitializeComponent();

            DependencyService.Register<DataStore>();
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
            InBackgroup = false;
            InitTestDb();
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
                    _ = Task.Factory.StartNew(async () =>
                      {
                          await Shell.Current.GoToAsync("//LoginPage");
                      });
                    return false;
                }
                else
                {
                    Debug.WriteLine("PassXYZ.App: OnSleep, running in foreground.");
                    return true;
                }
            });
        }

        protected override void OnResume()
        {
            InBackgroup = false;
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
