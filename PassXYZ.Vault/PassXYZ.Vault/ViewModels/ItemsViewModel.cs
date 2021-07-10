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
        public bool IsRootGroup => DataStore.CurrentGroup.Uuid.Equals(DataStore.RootGroup.Uuid);
        public bool IsItemGroupSelected { get; set; }
        private bool _isBackButtonClicked = false;
        public bool IsBackButtonClicked { get => _isBackButtonClicked; set { _isBackButtonClicked = value; } }

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

            try
            {
                if (DataStore.RootGroup != null)
                {
                    //if (AppShell.CurrentAppShell != null)
                    //{
                    //    Debug.WriteLine($"ItemsViewModel: {Shell.Current.CurrentState.Location}");
                    //    Debug.WriteLine($"ItemsViewModel: C:{AppShell.CurrentAppShell.CurrentRoute}, T:{AppShell.CurrentAppShell.TargetRoute}");
                    //    Debug.WriteLine($"ItemsViewModel: {DataStore.CurrentPath}");
                    //}

                    if (AppShell.CurrentAppShell.TargetRoute.Equals("..") || IsBackButtonClicked)
                    {
                        if(!IsItemGroupSelected) 
                        {
                            if(!IsRootGroup && AppShell.CurrentAppShell.CurrentRoute.EndsWith("group")) 
                            {
                                DataStore.SetCurrentToParent();
                                Debug.WriteLine($"ItemsViewModel: back to group {DataStore.CurrentGroup.Name}");
                            }
                            else
                            {
                                Debug.WriteLine($"ItemsViewModel: back from route {AppShell.CurrentAppShell.CurrentRoute}");
                            }
                        }
                    }

                    Title = DataStore.CurrentGroup.Name;
                    Items.Clear();
                    var items = await DataStore.GetItemsAsync(true);
                    foreach (var item in items)
                    {
                        ImageSource imgSource = (ImageSource)item.ImgSource;
                        if (item.ImgSource == null)
                        {
                            item.SetIcon();
                        }
                        // Debug.WriteLine($"{item.Name}-{imgSource}");
                        Items.Add(item);
                    }
                    IsItemGroupSelected = false;
                    IsBackButtonClicked = false;
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
                DataStore.CurrentGroup = item;
                if (!IsRootGroup)
                {
                    await Shell.Current.GoToAsync($"group");
                    IsItemGroupSelected = true;
                    Debug.WriteLine($"ItemsViewModel: selected group {item.Name}.");
                }
                await ExecuteLoadItemsCommand();
            }
            else
            {
                var pwEntry = (PwEntry)item;
                if(pwEntry.IsNotes()) 
                {
                    await Shell.Current.GoToAsync($"{nameof(NotesPage)}?{nameof(ItemDetailViewModel.ItemId)}={item.Id}");
                }
                else 
                {
                    await Shell.Current.GoToAsync($"{nameof(ItemDetailPage)}?{nameof(ItemDetailViewModel.ItemId)}={item.Id}");
                }
            }
        }

        /// <summary>
        /// Update an item. The item can be a group or an entry.
        /// </summary>
        /// <param name="item">an instance of Item</param>
        public async void Update(Item item)
        {
            if (item == null)
            {
                return;
            }

            await Shell.Current.Navigation.PushModalAsync(new NavigationPage(new FieldEditPage((string k, string v, bool isProtected) => {
                item.Notes = v;
                //Debug.WriteLine($"ItemsViewModel: Update item {item.Name}.");
            }, item.Name, item.Notes)));
        }


    }
}