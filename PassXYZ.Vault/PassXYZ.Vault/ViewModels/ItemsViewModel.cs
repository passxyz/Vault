using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

using ZXing.Net.Mobile.Forms;

using KeePassLib;
using PassXYZLib;
using PassXYZ.Vault.Views;
using PassXYZ.Vault.Resx;

namespace PassXYZ.Vault.ViewModels
{
    [QueryProperty(nameof(ItemId), nameof(ItemId))]
    public class ItemsViewModel : BaseViewModel
    {
        private Item _selectedItem = null;
        // private bool _reloadCurrentGroup = false;

        public ObservableCollection<Item> Items { get; }
        public Command LoadItemsCommand { get; }
        public Command AddItemCommand { get; }
        public Command LoadSearchItemsCommand { get; set; }
        public Command SearchCommand { get; set; }
        public Command<Item> ItemTapped { get; }
        public bool IsRootGroup => DataStore.CurrentGroup.Uuid.Equals(DataStore.RootGroup.Uuid);
        // public bool IsItemGroupSelected { get; set; }
        // private bool _isBackButtonClicked = false;
        // public bool IsBackButtonClicked { get => _isBackButtonClicked; set { _isBackButtonClicked = value; } }

        public Item SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }
        /// <summary>
        /// The item Id of the selected group
        /// </summary>
        public string ItemId
        {
            get => _selectedItem == null ? string.Empty : _selectedItem.Id;
            set
            {
                _selectedItem = DataStore.FindGroup(value);
                // IsItemGroupSelected = true;
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

                    //if (AppShell.CurrentAppShell.TargetRoute.Equals("..") || IsBackButtonClicked)
                    //{
                    //    if (_reloadCurrentGroup)
                    //    {
                    //        // We come back from a ItemDetailPage
                    //        _reloadCurrentGroup = false;
                    //    }
                    //    else
                    //    {
                    //        DataStore.SetCurrentToParent();
                    //    }
                    //    if (!IsRootGroup && AppShell.CurrentAppShell.CurrentRoute.EndsWith("group"))
                    //    {
                    //        Debug.WriteLine($"ItemsViewModel: ELTC <= back to group {DataStore.CurrentGroup.Name}");
                    //    }
                    //    else
                    //    {
                    //        Debug.WriteLine($"ItemsViewModel: ELTC <= back from route {AppShell.CurrentAppShell.CurrentRoute} and current group {DataStore.CurrentGroup.Name}");
                    //    }
                    //}
                    //else
                    //{
                    //    if (IsItemGroupSelected)
                    //    {
                    //        Debug.WriteLine($"ItemsViewModel: ELTC => Loading items from {DataStore.CurrentGroup.Name}");
                    //    }
                    //    else 
                    //    {
                    //        Debug.WriteLine($"ItemsViewModel: ELTC = Reloading the current group {DataStore.CurrentGroup.Name}");
                    //    }
                    //}

                    if (SelectedItem != null)
                    {
                        // The SelectedItem is the current group in this ItemsPage
                        if(IsRootGroup)
                        {
                            SelectedItem = DataStore.CurrentGroup;
                        }
                        else
                        {
                            DataStore.CurrentGroup = SelectedItem;
                        }
                    }
                    else
                    {
                        // This is the case for root group.
                        SelectedItem = DataStore.CurrentGroup;
                    }

                    Debug.WriteLine($"ItemsViewModel: ELTC = Loading the current group {DataStore.CurrentGroup.Name}");

                    // To do the real work here
                    Title = DataStore.CurrentGroup.Name;
                    Items.Clear();
                    var items = await DataStore.GetItemsAsync(true);
                    foreach (Item item in items)
                    {
                        ImageSource imgSource = (ImageSource)item.ImgSource;
                        if (item.ImgSource == null)
                        {
                            item.SetIcon();
                        }
                        Items.Add(item);
                    }
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
                AppResources.item_subtype_pxentry,
                AppResources.action_id_scan
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
            else if (template == AppResources.action_id_scan)
            {
                type = ItemSubType.None;
                ScanQRCode();
                Debug.WriteLine("ItemsViewModel: Scan QR code");
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

        private async void CreatePxEntryfromJsonData(string data, string passwd = null)
        {
            PxEntry pxEntry = new PxEntry(data, passwd);
            if (pxEntry != null && pxEntry.Strings.UCount != 0)
            {
                await DataStore.AddItemAsync(pxEntry);
                IsBusy = true;
            }
        }

        private async void HandleJsonData(string data)
        {
            if (data.StartsWith(PxDefs.PxJsonTemplate))
            {
                PxPlainFields plainFields = new PxPlainFields(data);

                if (plainFields.IsGroup)
                {
                    PxGroup pxGroup = new PxGroup(data);
                    if (pxGroup != null)
                    {
                        await DataStore.AddItemAsync(pxGroup);
                        IsBusy = true;
                    }
                }
                else
                {
                    CreatePxEntryfromJsonData(data);
                }

            }
            else if (data.StartsWith(PxDefs.PxJsonData))
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    string passwd = await Shell.Current.DisplayPromptAsync("", AppResources.ph_id_password, keyboard: Keyboard.Default);
                    if (!string.IsNullOrEmpty(passwd))
                    {
                        await Task.Run(() => {
                            CreatePxEntryfromJsonData(data, passwd);
                        });
                    }
                    else
                    {
                        Debug.WriteLine("HandleJsonData: password is empty, error!");
                        return;
                    }
                });
            }
            else
            {
                Debug.WriteLine("HandleJsonData: wrong JSON string, error!");
                return;
            }

        }

        private void ScanQRCode()
        {
            if (IsBusy) { return; }

            var scanPage = new ZXingScannerPage();
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Shell.Current.Navigation.PushAsync(scanPage);
            });

            scanPage.OnScanResult += async (result) =>
            {
                // Stop scanning
                scanPage.IsScanning = false;

                // Pop the page and show the result
                Device.BeginInvokeOnMainThread(async () =>
                {
                    _ = await Shell.Current.Navigation.PopAsync();
                });

                if (result.Text != null)
                {
                    // Debug.WriteLine($"ItemsViewModel: IsScanning={scanPage.IsScanning}, {result.Text}");
                    await Task.Run(() => {
                        HandleJsonData(result.Text);
                    });

                }
            };

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
                    await Shell.Current.GoToAsync($"group?{nameof(ItemId)}={item.Id}");
                }
            }
            else
            {
                // _reloadCurrentGroup = true;
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