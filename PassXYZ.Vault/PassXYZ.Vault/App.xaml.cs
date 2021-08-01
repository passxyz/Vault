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
