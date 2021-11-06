using KeePassLib;
using PassXYZ.Vault.ViewModels;
using PassXYZ.Vault.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using ZXing.Net.Mobile.Forms;

using PassXYZLib;
using PassXYZ.Vault.Resx;

namespace PassXYZ.Vault.Views
{
    public class QrCodePage : ContentPage
    {
        public QrCodePage(string msg, string name)
        {
            var layout = new StackLayout
            {
                Spacing = 10,
                Padding = new Thickness(10, 20, 0, 0)
            };
            ZXingBarcodeImageView barcode = new ZXingBarcodeImageView
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                HeightRequest = 300,
                WidthRequest = 300,
            };
            barcode.BarcodeFormat = ZXing.BarcodeFormat.QR_CODE;
            barcode.BarcodeOptions.Width = 300;
            barcode.BarcodeOptions.Height = 300;
            barcode.BarcodeValue = msg;

            var qrcodeTitle = new Label()
            {
                Text = AppResources.action_id_generateqrcode + ": " + name,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                FontAttributes = FontAttributes.Bold
            };

            var button = new Button
            {
                Text = AppResources.alert_id_ok,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };
            button.Clicked += async (sender1, e1) => { await Navigation.PopModalAsync(); };

            layout.Children.Add(qrcodeTitle);
            layout.Children.Add(barcode);
            layout.Children.Add(button);
            Content = layout;
        }
    }

    public partial class ItemsPage : ContentPage
    {
        private readonly ItemsViewModel _viewModel;
        private static bool isChild = false;

        public ItemsPage()
        {
            InitializeComponent();

            BindingContext = _viewModel = new ItemsViewModel();

            if(isChild)
            {
                // This is needed for iOS build. It seems iOS build back button can
                // not work well without this.
                Shell.SetBackButtonBehavior(this, new BackButtonBehavior()
                {
                    Command = new Command(async () => {
                        //_viewModel.IsBackButtonClicked = true;
                        await Navigation.PopAsync();

                    })
                });
                Debug.Write($"ItemsPage: child page SetBackButtonBehavior");
            }
            else 
            {
                isChild = true;
                Debug.Write($"ItemsPage: root page");
            }
        }

        private void OnMenuEdit(object sender, EventArgs e)
        {
            var mi = (MenuItem)sender;

            if (mi.CommandParameter is Item item)
            {
                _viewModel.Update(item);
            }
        }

        private async void OnMenuShare(object sender, EventArgs e)
        {
            var mi = (MenuItem)sender;

            if (mi.CommandParameter is Item item)
            {
                PxPlainFields plainFields = item.GetPlainFields();
                string data = plainFields.ToString();
                if (data.Length < PxDefs.QR_CODE_MAX_LEN)
                {
                    Debug.WriteLine($"ItemsPage: sharing {data}");
                    QrCodePage qrCodePage = new QrCodePage(data, item.Name);
                    await Navigation.PushModalAsync(new NavigationPage(qrCodePage));
                }
                else
                {
                    Debug.WriteLine($"ItemsPage: cannot sharing {item.Name}, it is too large");
                }
            }
        }

        private async void OnMenuChangeIconAsync(object sender, EventArgs e)
        {
            var mi = (MenuItem)sender;
            Item item = mi.CommandParameter as Item;
            ContentPage page = new IconSearchPage(item);
            await Navigation.PushModalAsync(new NavigationPage(page));
        }

        private async void OnMenuDeleteAsync(object sender, EventArgs e)
        {
            var mi = (MenuItem)sender;

            if (mi.CommandParameter is Item item)
            {
                await _viewModel.DeletedAsync(item);
                Debug.WriteLine("ItemsPage: OnMenuDeleteAsync clicked");
            }
        }

        void OnTap(object sender, ItemTappedEventArgs args)
        {
            var item = args.Item as Item;
            if (item == null)
            {
                return;
            }
            _viewModel.OnItemSelected(item);
        }

        private void OnBindingContextChanged(object sender, EventArgs e)
        {
            base.OnBindingContextChanged();
            if (BindingContext == null)
            {
                return;
            }

            ViewCell theViewCell = (ViewCell)sender;
            if (theViewCell.BindingContext is Item item)
            {
                // disable Share for group and notes
                MenuItem menuItem = theViewCell.ContextActions[1];
                if (item.IsGroup || item.IsNotes())
                {
                    // disable Share for group
                    menuItem.IsEnabled = false;
                    Debug.WriteLine($"ItemsPage: disable sharing of {item.Name}");
                }
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }
    }
}