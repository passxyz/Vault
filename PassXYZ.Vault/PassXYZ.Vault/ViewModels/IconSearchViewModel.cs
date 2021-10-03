using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

using Xamarin.Forms;

using KeePassLib;
using PassXYZLib;
using System.Threading.Tasks;

namespace PassXYZ.Vault.ViewModels
{
    public class IconSearchViewModel : BaseViewModel
    {
        private readonly Item selectedItem = null;
        public ObservableCollection<PxIcon> PxIcons { get; set; }
        public Command AddIconCommand { get; }
        public Command SaveChangeCommand { get; }
        public Command CancelChangeCommand { get; }
        public Command SearchItemsCommand { get; set; }
        public Command LoadIconsCommand { get; set; }
        public PxIcon SelectedIcon = null;

        public IconSearchViewModel()
        {
            PxIcons = new ObservableCollection<PxIcon>();
            SearchItemsCommand = new Command<string>((string strSearch) => Search(strSearch));
            LoadIconsCommand = new Command(() => ExecuteLoadIconsCommand());
            AddIconCommand = new Command(OnAddIcon);
            SaveChangeCommand = new Command(OnSave);
            CancelChangeCommand = new Command(OnCancel);
            ExecuteLoadIconsCommand();
        }

        public IconSearchViewModel(Item item) : this()
        {
            selectedItem = item;
            OnAddIcon();
        }

        private void ExecuteLoadIconsCommand()
        {
            try
            {
                PxIcons.Clear();
                List<PwCustomIcon> customIconList = DataStore.GetCustomIcons();
                foreach (PwCustomIcon pwci in customIconList)
                {
                    PxIcon icon = new PxIcon
                    {
                        IconType = PxIconType.PxEmbeddedIcon,
                        Uuid = pwci.Uuid,
                        Name = pwci.Name,
                        ImgSource = DataStore.GetBuiltInImage(pwci.Uuid),
                    };
                    PxIcons.Add(icon);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task<bool> DeleteCustomIconAsync(PxIcon icon)
        {
            var result = await DataStore.DeleteCustomIconAsync(icon.Uuid);
            if (result)
            {
                _ = PxIcons.Remove(icon);
            }
            return result;
        }

        public void Search(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText)) { return; }

            List<PwCustomIcon> customIconList = DataStore.GetCustomIcons();
            PxIcons.Clear();
            foreach (PwCustomIcon pwci in customIconList)
            {
                if (pwci.Name.Contains(searchText))
                {
                    PxIcon icon = new PxIcon
                    {
                        IconType = PxIconType.PxEmbeddedIcon,
                        Uuid = pwci.Uuid,
                        Name = pwci.Name,
                        ImgSource = DataStore.GetBuiltInImage(pwci.Uuid),
                    };
                    PxIcons.Add(icon);
                }
            }
        }

        private async void OnAddIcon()
        {
            if (selectedItem != null && !selectedItem.IsGroup)
            {
                await Task.Run(async () => {
                    PxIcon icon = selectedItem.AddNewIcon();
                    if (icon != null)
                    {
                        icon.ImgSource = DataStore.GetBuiltInImage(icon.Uuid);
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            // interact with UI elements
                            PxIcons.Add(icon);
                        });
                        await DataStore.SaveAsync();
                        Debug.WriteLine($"IconSearchViewModel: OnAddIcon({icon.Name})");
                    }
                });
            }
        }

        private async void OnSave(object obj)
        {
            if (SelectedIcon != null && selectedItem != null)
            {
                selectedItem.CustomIconUuid = SelectedIcon.Uuid;
                await DataStore.SaveAsync();
            }
            _ = await Shell.Current.Navigation.PopModalAsync();
        }

        private async void OnCancel(object obj)
        {
            _ = await Shell.Current.Navigation.PopModalAsync();
        }
    }
}
