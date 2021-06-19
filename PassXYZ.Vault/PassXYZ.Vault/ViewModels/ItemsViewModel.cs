using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

using KeePassLib;
using PassXYZLib;
using PassXYZ.Vault.Views;

namespace PassXYZ.Vault.ViewModels
{
    public class ItemsViewModel : BaseViewModel
    {
        private Item _selectedItem;

        public ObservableCollection<Item> Items { get; }
        public Command LoadItemsCommand { get; }
        public Command AddItemCommand { get; }
        public Command<Item> ItemTapped { get; }

        public ItemsViewModel()
        {
            Items = new ObservableCollection<Item>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

            ItemTapped = new Command<Item>(OnItemSelected);

            AddItemCommand = new Command(OnAddItem);
        }

        async Task ExecuteLoadItemsCommand()
        {
            IsBusy = true;

            Debug.WriteLine($"ItemsViewModel: {Shell.Current.CurrentState.Location}");
            Title = DataStore.CurrentGroup.Name;
            try
            {
                Items.Clear();
                var items = await DataStore.GetItemsAsync(true);
                foreach (var item in items)
                {
                    ImageSource imgSource = (ImageSource)item.ImgSource;
                    if(item.ImgSource == null)
                    {
                        item.SetIcon();
                    }
                    // Debug.WriteLine($"{item.Name}-{imgSource}");
                    Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void OnAppearing()
        {
            IsBusy = true;
            SelectedItem = null;
        }

        public Item SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                OnItemSelected(value);
            }
        }

        private async void OnAddItem(object obj)
        {
            await Shell.Current.GoToAsync(nameof(NewItemPage));
        }

        private async void OnItemSelected(Item item)
        {
            if (item == null)
            {
                return;
            }

            if (item.IsGroup)
            {
                Routing.RegisterRoute($"//{DataStore.CurrentGroup.Name}/{item.Name}", typeof(ItemsPage));
                DataStore.CurrentGroup = item;
                await ExecuteLoadItemsCommand();
                Debug.WriteLine($"ItemsViewModel: go to page {item.Name}");
                await Shell.Current.GoToAsync($"{item.Name}");
            }
            else
            {
                // This will push the ItemDetailPage onto the navigation stack
                await Shell.Current.GoToAsync($"{nameof(ItemDetailPage)}?{nameof(ItemDetailViewModel.ItemId)}={item.Id}");
            }
        }
    }
}