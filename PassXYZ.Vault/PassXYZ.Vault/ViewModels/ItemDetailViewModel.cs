using KeePassLib;
using PassXYZLib;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

using PassXYZ.Vault.Views;

namespace PassXYZ.Vault.ViewModels
{
    [QueryProperty(nameof(ItemId), nameof(ItemId))]
    public class ItemDetailViewModel : BaseViewModel
    {
        private string itemId;
        private string text;
        private string description;
        private PwEntry dataEntry = null;

        public ObservableCollection<Field> Fields { get; set; }
        public Command LoadFieldsCommand { get; }
        public Command AddFieldCommand { get; }
        public Command<Field> FieldTapped { get; }

        public string Id { get; set; }

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

        public string ItemId
        {
            get
            {
                return itemId;
            }
            set
            {
                itemId = value;
                LoadItemId(value);
            }
        }

        public ItemDetailViewModel()
        {
            Fields = new ObservableCollection<Field>();
            LoadFieldsCommand = new Command(() => ExecuteLoadFieldsCommand());
            FieldTapped = new Command<Field>(OnFieldSelected);
            AddFieldCommand = new Command(OnAddField);
        }

        private void ExecuteLoadFieldsCommand()
        {
            if (dataEntry != null)
            {
                var fields = dataEntry.GetFields();
                foreach (var field in fields)
                {
                    Fields.Add(field);
                }
            }
        }

        public async void LoadItemId(string itemId)
        {
            try
            {
                var item = await DataStore.GetItemAsync(itemId);
                Id = item.Id;
                Text = item.Name;
                Title = Text;
                dataEntry = (PwEntry)item;

                Description = Markdig.Markdown.ToHtml(dataEntry.GetNotes());
                // Description = dataEntry.GetNotes();
                ExecuteLoadFieldsCommand();
            }
            catch (Exception)
            {
                Debug.WriteLine("Failed to Load Item");
            }
        }

        /// <summary>
        /// Update a field.
        /// </summary>
        /// <param name="field">an instance of Field</param>
        public async void Update(Field field)
        {
            if (field == null)
            {
                return;
            }

            await Shell.Current.Navigation.PushModalAsync(new NavigationPage(new FieldEditPage((string k, string v, bool isProtected) => {
                string key = field.IsEncoded ? field.EncodedKey : field.Key;
                if (dataEntry.Strings.Exists(key))
                {
                    field.Value = v;
                    // We cannot set to field.Value, since it will return a masked string for protected data
                    dataEntry.Strings.Set(key, new KeePassLib.Security.ProtectedString(field.IsProtected, v));
                    Debug.WriteLine($"ItemDetailViewModel: Update field {field.Key}={field.Value}.");
                }
                else
                {
                    Debug.WriteLine($"ItemDetailViewModel: Cannot update field {field.Key}.");
                }
            }, field.Key, field.Value)));
        }

        /// <summary>
        /// Delete a field.
        /// </summary>
        /// <param name="field">an instance of Field</param>
        public void Deleted(Field field)
        {
            if (field == null)
            {
                return;
            }

            string key;
            if (Fields.Remove(field))
            {
                key = field.IsEncoded ? field.EncodedKey : field.Key;
            }
            else
            {
                return;
            }

            if (dataEntry.Strings.Exists(key))
            {
                if (dataEntry.Strings.Remove(key))
                {

                    Debug.WriteLine($"ItemDetailViewModel: Field {field.Key} deleted.");
                }
                else
                {
                    Debug.WriteLine($"ItemDetailViewModel: Cannot delete field {field.Key}.");
                }
            }
        }

        private async void OnAddField(object obj)
        {
            await Shell.Current.Navigation.PushModalAsync(new NavigationPage(new FieldEditPage((string k, string v, bool isProtected) => {
                Field field;
                string key = k;
                if (dataEntry.IsPxEntry())
                {
                    key = dataEntry.EncodeKey(k);
                    field = new Field(k, v, isProtected, key);
                }
                else
                {
                    field = new Field(k, v, isProtected);
                }

                Fields.Add(field);
                dataEntry.Strings.Set(key, new KeePassLib.Security.ProtectedString(field.IsProtected, v));
            })));
            Debug.WriteLine($"ItemDetailViewModel: Add field");
        }

        private void OnFieldSelected(Field field)
        {
            if (field == null)
            {
                return;
            }
            Debug.WriteLine($"Field {field.Key} selected");
        }
    }
}
