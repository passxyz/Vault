using System;
using System.IO;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using KeePassLib.Security;

namespace PassXYZ.Vault.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ImagePreviewPage : ContentPage
    {
        private readonly ProtectedBinary binaryImage;

        public ImagePreviewPage(ProtectedBinary binary)
        {
            InitializeComponent();

            if(binary != null)
            {
                binaryImage = binary;

                imageView.Source = ImageSource.FromStream(() => new MemoryStream(binaryImage.ReadData()));
            }
        }

        public ImagePreviewPage ()
        {
            InitializeComponent();
        }

        private async void OnCloseClickedAsync(object sender, EventArgs e)
        {
            _ = await Navigation.PopModalAsync();
        }

    }
}