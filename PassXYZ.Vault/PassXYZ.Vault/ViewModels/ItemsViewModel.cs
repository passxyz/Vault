using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

using KeePassLib;
using PassXYZLib;
using PassXYZ.Vault.Views;
using PassXYZ.Vault.Resx;

namespace PassXYZ.Vault.ViewModels
{
    [QueryProperty(nameof(ItemId), nameof(ItemId))]
    public class ItemsViewModel : BaseViewModel
    {
        private Item _selectedItem;
        private bool _reloadCurrentGroup = false;

        public ObservableCollection<Item> Items { get; }
        public Command LoadItemsCommand { get; }
        public Command AddItemCommand { get; }
        public Command LoadSearchItemsCommand { get; set; }
        public Command SearchCommand { get; set; }
        public Command<Item> ItemTapped { get; }
        public bool IsRootGroup => DataStore.CurrentGroup.Uuid.Equals(DataStore.RootGroup.Uuid);
        public bool IsItemGroupSelected { get; set; }
        private bool _isBackButtonClicked = false;
        public bool IsBackButtonClicked { get => _isBackButtonClicked; set { _isBackButtonClicked = value; } }

        public Item SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                OnItemSelected(value);
            }
        }
        /// <summary>
        /// The item Id of the selected group
        /// </summary>
        public string ItemId
        {
            get
            {
                if (_selectedItem == null) { return string.Empty; }
                return _selectedItem.Id;
            }
            set
            {
                _selectedItem = DataStore.FindGroup(value);
                IsItemGroupSelected = true;
            }
        }

        public ItemsViewModel()
        {
            Items = new ObservableCollection<Item>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
            LoadSearchItemsCommand = new Command<string>(async (string strSearch) => await ExecuteSearchCommand(strSearch, null));
            SearchCommand = new Command(OnSearchItem);

            ItemTapped = new Command<Item>(OnItemSelected);

            AddItemCommand = new Command(OnAddItem);
        }

        public async Task ExecuteSearchCommand(string strSearch, Item item)
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                Items.Clear();
                var items = await DataStore.SearchEntriesAsync(strSearch, item);
                foreach (Item entry in items)
                {
                    ImageSource imgSource = (ImageSource)entry.ImgSource;
                    if (entry.ImgSource == null)
                    {
                        entry.SetIcon();
                    }
                    Items.Add(entry);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ItemsViewModel: SearchEntriesAsync, {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Load items from database. There are three cases we need to handle:
        /// 1. Forward - When a item is selected
        /// 2. Backward - Click backward key
        /// 3. Reload the current group - After editing, deleting or adding an entry or a group
        /// </summary>
        private async Task ExecuteLoadItemsCommand()
        {
            // IsBusy = true;

            try
            {
                if (DataStore.RootGroup != null)
                {
                    //if (AppShell.CurrentAppShell != null)
                    //{
                    //    Debug.WriteLine($"ItemsViewModel: ELTC=>{Shell.Current.CurrentState.Location}");
                    //    Debug.WriteLine($"ItemsViewModel: ELTC=>C:{AppShell.CurrentAppShell.CurrentRoute}, T:{AppShell.CurrentAppShell.TargetRoute}");
                    //    Debug.WriteLine($"ItemsViewModel: ELTC=>{DataStore.CurrentPath}");
                    //}

                    if (AppShell.CurrentAppShell.TargetRoute.Equals("..") || IsBackButtonClicked)
                    {
                        if (_reloadCurrentGroup)
                        {
                            // We come back from a ItemDetailPage
                            _reloadCurrentGroup = false;
                        }
                        else
                        {
                            DataStore.SetCurrentToParent();
                        }
                        if (!IsRootGroup && AppShell.CurrentAppShell.CurrentRoute.EndsWith("group"))
                        {
                            Debug.WriteLine($"ItemsViewModel: ELTC <= back to group {DataStore.CurrentGroup.Name}");
                        }
                        else
                        {
                            Debug.WriteLine($"ItemsViewModel: ELTC <= back from route {AppShell.CurrentAppShell.CurrentRoute} and current group {DataStore.CurrentGroup.Name}");
                        }
                    }
                    else
                    {
                        if (IsItemGroupSelected)
                        {
                            Debug.WriteLine($"ItemsViewModel: ELTC => Loading items from {DataStore.CurrentGroup.Name}");
                        }
                        else 
                        {
                            Debug.WriteLine($"ItemsViewModel: ELTC = Reloading the current group {DataStore.CurrentGroup.Name}");
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
            // SelectedItem = null;
        }

        private async void OnAddItem(object obj)
        {
            string[] templates = {
                AppResources.item_subtype_group,
                AppResources.item_subtype_entry,
                AppResources.item_subtype_notes,
                AppResources.item_subtype_pxentry
            };

            var template = await Shell.Current.DisplayActionSheet(AppResources.pt_id_choosetemplate, AppResources.action_id_cancel, null, templates);
            ItemSubType type;
            if (template == AppResources.item_subtype_entry)
            {
                type = ItemSubType.Entry;
            }
            else if (template == AppResources.item_subtype_pxentry)
            {
                type = ItemSubType.PxEntry;
            }
            else if (template == AppResources.item_subtype_group)
            {
                type = ItemSubType.Group;
            }
            else if (template == AppResources.item_subtype_notes)
            {
                type = ItemSubType.Notes;
            }
            else if (template == AppResources.action_id_cancel)
            {
                type = ItemSubType.None;
                Debug.WriteLine("Canceled the Template selection.");
            }
            else
            {
                type = ItemSubType.None;
                Debug.WriteLine("Canceled the Template selection.");
            }

            if (type != ItemSubType.None)
            {
                //await Shell.Current.GoToAsync(nameof(NewItemPage));
                await Shell.Current.Navigation.PushModalAsync(new NavigationPage(new NewItemPage(type)));
            }
        }

        public async void OnItemSelected(Item item)
        {
            if (item == null)
            {
                return;
            }

            Debug.WriteLine($"ItemsViewModel: OnItemSelected, item={item.Name}, IsBusy={IsBusy}");

            if (item.IsGroup)
            {
                DataStore.CurrentGroup = item;
                if (!IsRootGroup)
                {
                    await Shell.Current.GoToAsync($"group?{nameof(ItemsViewModel.ItemId)}={item.Id}");
                }
                // await ExecuteLoadItemsCommand();
            }
            else
            {
                _reloadCurrentGroup = true;
                var pwEntry = (PwEntry)item;
                if (pwEntry.IsNotes())
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

            await Shell.Current.Navigation.PushModalAsync(new NavigationPage(new FieldEditPage(async (string k, string v, bool isProtected) => {
                item.Name = k;
                item.Notes = v;
                await DataStore.UpdateItemAsync(item);
            }, item.Name, item.Notes, true)));
        }

        /// <summary>
        /// Delete an item.
        /// </summary>
        /// <param name="item">an instance of Item</param>
        public async Task DeletedAsync(Item item)
        {
            if (item == null)
            {
                return;
            }

            if (Items.Remove(item))
            {
                _ = await DataStore.DeleteItemAsync(item.Id);
            }
            else
            {
                return;
            }

        }

        private async void OnSearchItem(object obj)
        {
            await Shell.Current.GoToAsync($"{nameof(SearchPage)}");
            Debug.WriteLine("ItemsViewModel: SearchCommand clicked");
        }

    }
}