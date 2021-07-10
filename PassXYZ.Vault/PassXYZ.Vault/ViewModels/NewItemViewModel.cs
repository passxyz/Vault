using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

using KeePassLib;
using PassXYZLib;

namespace PassXYZ.Vault.ViewModels
{
    public class NewItemViewModel : BaseViewModel
    {
        private string text;
        private string description;
        private ItemSubType type = ItemSubType.Group;

        public NewItemViewModel()
        {
            SaveCommand = new Command(OnSave, ValidateSave);
            CancelCommand = new Command(OnCancel);
            this.PropertyChanged +=
                (_, __) => SaveCommand.ChangeCanExecute();
            Title = "New " + type.ToString();
        }

        public NewItemViewModel(ItemSubType type):this()
        {
            this.type = type;
            Title = "New " + type.ToString();
        }

        private bool ValidateSave()
        {
            return !String.IsNullOrWhiteSpace(text)
                && !String.IsNullOrWhiteSpace(description);
        }

        public ItemSubType Type
        {
            get => type;
            set
            {
                _ = SetProperty(ref type, value);
                Title = "New " + type.ToString();
            }
        }

        public string Text
        {
            get => text;
            set => SetProperty(ref text, value);
        }

        public string Description
        {
            get => description;
            set => SetProperty(ref description, value);
        }

        public Command SaveCommand { get; }
        public Command CancelCommand { get; }

        private async void OnCancel()
        {
            // This will pop the current page off the navigation stack
            //await Shell.Current.GoToAsync("..");
            _ = await Shell.Current.Navigation.PopModalAsync();
        }

        private async void OnSave()
        {
            Item newItem = null;

            if (Type == ItemSubType.Group)
            {
                newItem = new PwGroup(true, true)
                {
                    Name = Text,
                    Notes = Description
                };
            }
            else if (Type != ItemSubType.None)
            {
                PwEntry entry = new PwEntry(true, true)
                {
                    Name = Text,
                    Notes = Description
                };
                entry.SetType(Type);
                newItem = entry;
            }

            if (newItem != null)
            {
                _ = await DataStore.AddItemAsync(newItem);
            }

            // This will pop the current page off the navigation stack
            //await Shell.Current.GoToAsync("..");
            _ = await Shell.Current.Navigation.PopModalAsync();
        }
    }
}
