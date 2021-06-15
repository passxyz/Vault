using System;
using System.IO;
using System.Reflection;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using PassXYZ.Vault.Services;
using PassXYZ.Vault.Views;

namespace PassXYZ.Vault
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            DependencyService.Register<DataStore>();
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
            InitTestDb();
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private void InitTestDb()
        {
            string fileName = TEST_DB.PATH;

            if (!File.Exists(fileName))
            {
                var assembly = this.GetType().GetTypeInfo().Assembly;
                using (var stream = assembly.GetManifestResourceStream(TEST_DB.RES_PATH))
                using (var fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                {
                    stream.CopyTo(fileStream);
                }
            }
        }
    }
}
