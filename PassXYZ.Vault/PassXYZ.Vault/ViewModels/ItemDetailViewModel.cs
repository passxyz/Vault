using KeePassLib;
using PassXYZLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Essentials;

using Markdig;
using KeePassLib.Security;
using PassXYZ.Vault.Resx;
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
        public Command AddBinaryCommand { get; }
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
            AddBinaryCommand = new Command(OnAddBinary);
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
                Item item = await DataStore.GetItemFromCurrentGroupAsync(itemId);
                if (item == null)
                {
                    // This may be the case that we navigate to ItemDetailPage from OtpListPage
                    item = DataStore.FindEntryById(itemId);
                }

                if (item == null) { throw new ArgumentNullException("itemId"); }

                Id = item.Id;
                Text = item.Name;
                Title = Text;
                dataEntry = (PwEntry)item;

                // Configure the pipeline with all advanced extensions active
                var pipeline = new Markdig.MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
                Description = Markdig.Markdown.ToHtml(dataEntry.GetNotes(), pipeline);
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

            if (!field.IsBinaries)
            {
                await Shell.Current.Navigation.PushModalAsync(new NavigationPage(new FieldEditPage(async (string k, string v, bool isProtected) => {
                    string key = field.IsEncoded ? field.EncodedKey : field.Key;
                    if (dataEntry.Strings.Exists(key))
                    {
                        field.Value = v;
                        // We cannot set to field.Value, since it will return a masked string for protected data
                        dataEntry.Strings.Set(key, new KeePassLib.Security.ProtectedString(field.IsProtected, v));
                        Debug.WriteLine($"ItemDetailViewModel: Update field {field.Key}={field.Value}.");
                        await DataStore.UpdateItemAsync(dataEntry);
                    }
                    else
                    {
                        Debug.WriteLine($"ItemDetailViewModel: Cannot update field {field.Key}.");
                    }
                }, field.Key, field.Value)));
            }
            else
            {
                await Shell.Current.DisplayAlert(AppResources.label_id_attachment, AppResources.message_id_edit_binary, AppResources.alert_id_ok);
            }
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

            if (field.IsBinaries)
            {
                if (dataEntry.Binaries.Exists(key))
                {
                    if (dataEntry.Binaries.Remove(key))
                    {

                        Debug.WriteLine($"ItemDetailViewModel: Attachment {field.Key} deleted.");
                    }
                    else
                    {
                        Debug.WriteLine($"ItemDetailViewModel: Cannot delete Attachment {field.Key}.");
                    }
                }
            }
            else
            {
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
        }

        private async void OnAddField(object obj)
        {
            await Shell.Current.Navigation.PushModalAsync(new NavigationPage(new FieldEditPage(async (string k, string v, bool isProtected) => {
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
                if (key.EndsWith(PwDefs.UrlField))
                {
                    // If this is a URL field, we can try to find a custom icon.
                    await dataEntry.SetCustomIconByUrl(v);
                }
                await DataStore.UpdateItemAsync(dataEntry);
            })));
        }

        private async Task LoadPhotoAsync(FileResult photo)
        {
            // canceled
            if (photo == null)
            {
                return;
            }
            // save the file into local storage
            var newFile = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
            using (var stream = await photo.OpenReadAsync())
            using (var newStream = File.OpenWrite(newFile))
                await stream.CopyToAsync(newStream);
            AddBinary(newFile, photo.FileName);
        }

        private async void AddBinary(string tempFilePath, string fileName)
        {
            var vBytes = File.ReadAllBytes(tempFilePath);
            if (vBytes != null)
            {
                ProtectedBinary pb = new ProtectedBinary(false, vBytes);
                dataEntry.Binaries.Set(fileName, pb);
            }

            Fields.Add(new Field(fileName, $"{AppResources.label_id_attachment} {dataEntry.Binaries.UCount}", false)
            {
                IsBinaries = true,
                Binary = dataEntry.Binaries.Get(fileName),
                ImgSource = new FontAwesome.Solid.IconSource
                {
                    Icon = FontAwesome.Solid.Icon.Paperclip
                }
            });
            await DataStore.UpdateItemAsync(dataEntry);
        }

        private async void OnAddBinary(object obj)
        {
            List<string> inputTypeList = new List<string>()
            {
                AppResources.field_id_file,
                AppResources.field_id_camera,
                AppResources.field_id_gallery
            };


            var typeValue = await Shell.Current.DisplayActionSheet(AppResources.message_id_attachment_options, AppResources.action_id_cancel, null, inputTypeList.ToArray());
            if (typeValue == AppResources.field_id_gallery)
            {
                try
                {
                    var photo = await MediaPicker.PickPhotoAsync();
                    await LoadPhotoAsync(photo);
                }
                catch (FeatureNotSupportedException fnsEx)
                {
                    // Feature is not supported on the device
                    Debug.WriteLine($"ItemDetailViewModel: PickPhotoAsync => {fnsEx.Message}");
                }
                catch (PermissionException pEx)
                {
                    // Permissions not granted
                    Debug.WriteLine($"ItemDetailViewModel: PickPhotoAsync => {pEx.Message}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ItemDetailViewModel: PickPhotoAsync => {ex.Message}");
                }
                Debug.WriteLine($"ItemDetailViewModel: Add an attachment from Gallery");
            }
            else if (typeValue == AppResources.field_id_file) 
            {
                try
                {
                    var result = await FilePicker.PickAsync();
                    if (result != null)
                    {
                        var tempFilePath = Path.GetTempFileName();
                        var stream = await result.OpenReadAsync();
                        var fileStream = File.Create(tempFilePath);
                        stream.Seek(0, SeekOrigin.Begin);
                        stream.CopyTo(fileStream);
                        fileStream.Close();
                        AddBinary(tempFilePath, result.FileName);
                        File.Delete(tempFilePath);
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert(AppResources.message_id_attachment_options, AppResources.import_error_msg, AppResources.alert_id_ok);
                    }
                }
                catch (Exception ex)
                {
                    // The user canceled or something went wrong
                    Debug.WriteLine($"LoginViewModel: Import attachment, {ex}");
                }
                Debug.WriteLine($"ItemDetailViewModel: Add an attachment from local storage");
            }
            else if (typeValue == AppResources.field_id_camera)
            {
                try
                {
                    var photo = await MediaPicker.CapturePhotoAsync();
                    await LoadPhotoAsync(photo);
                }
                catch (FeatureNotSupportedException fnsEx)
                {
                    // Feature is not supported on the device
                    Debug.WriteLine($"ItemDetailViewModel: CapturePhotoAsync => {fnsEx.Message}");
                }
                catch (PermissionException pEx)
                {
                    // Permissions not granted
                    Debug.WriteLine($"ItemDetailViewModel: CapturePhotoAsync => {pEx.Message}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ItemDetailViewModel: CapturePhotoAsync => {ex.Message}");
                }
                Debug.WriteLine($"ItemDetailViewModel: Add an attachment using Camera");
            }
        }

        private async void OnFieldSelected(Field field)
        {
            if (field == null)
            {
                return;
            }

            if (field.IsBinaries)
            {
                var bdc = BinaryDataClassifier.ClassifyUrl(field.Key);
                if ((bdc == BinaryDataClass.Image) && (field.Binary != null))
                {
                    await Shell.Current.Navigation.PushModalAsync(new NavigationPage(new ImagePreviewPage(field.Binary)));
                }
                Debug.WriteLine($"ItemDetailViewModel: Attachment {field.Key} selected");
            }
        }
    }
}
