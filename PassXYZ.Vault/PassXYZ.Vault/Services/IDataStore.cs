using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Xamarin.Forms;

using KeePassLib;
using PassXYZLib;

namespace PassXYZ.Vault.Services
{
    public interface IDataStore<T>
    {
        Task AddItemAsync(T item);
        Task UpdateItemAsync(T item);
        Task<bool> DeleteItemAsync(string id);
        Task<T> GetItemFromCurrentGroupAsync(string id);
        Item GetItemFromCurrentGroup(string id);
        PwGroup FindGroup(string id);
        Task<PwEntry> FindEntryByIdAsync(string id);
        PwEntry FindEntryById(string id);
        Task<IEnumerable<T>> GetItemsAsync(bool forceRefresh = false);
        Task<IEnumerable<Item>> SearchEntriesAsync(string strSearch, Item itemGroup = null);
        Task<IEnumerable<PwEntry>> GetOtpEntryListAsync();
        Item CurrentGroup { get; set; }
        string CurrentPath { get; }
        void SetCurrentToParent();
        Task SaveAsync();
        Item RootGroup { get; }
        bool IsOpen { get; }
        Task<bool> LoginAsync(PxUser user);
        void Logout();
        string GetStoreName();
        DateTime GetStoreModifiedTime();
        User CurrentUser { get; }
        Task SignUpAsync(PassXYZLib.User user);
        Task<bool> ChangeMasterPassword(string newPassword);
        string GetMasterPassword();
        string GetDeviceLockData();
        List<PwCustomIcon> GetCustomIcons();
        Task<bool> DeleteCustomIconAsync(PwUuid uuidIcon);
        ImageSource GetBuiltInImage(PwUuid uuid);
        bool CreateKeyFile(string data, string username);
        Task<bool> MergeAsync(string path, PwMergeMethod mm);
    }
}
