using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using KeePassLib;
using PassXYZLib;

namespace PassXYZ.Vault.Services
{
    public interface IDataStore<T>
    {
        Task AddItemAsync(T item);
        Task UpdateItemAsync(T item);
        Task<bool> DeleteItemAsync(string id);
        Task<T> GetItemAsync(string id);
        Task<PwEntry> FindEntryByIdAsync(string id);
        PwEntry FindEntryById(string id);
        Task<IEnumerable<T>> GetItemsAsync(bool forceRefresh = false);
        Task<IEnumerable<Item>> SearchEntriesAsync(string strSearch, Item itemGroup = null);
        Task<IEnumerable<PwEntry>> GetOtpEntryListAsync();
        Item CurrentGroup { get; set; }
        string CurrentPath { get; }
        void SetCurrentToParent();
        Item RootGroup { get; }
        bool IsOpen { get; }
        Task<bool> LoginAsync(PassXYZLib.User user);
        void Logout();
        string GetStoreName();
        DateTime GetStoreModifiedTime();
        User CurrentUser { get; }
        Task SignUpAsync(PassXYZLib.User user);
        Task<bool> ChangeMasterPassword(string newPassword);
        string GetMasterPassword();
        string GetDeviceLockData();
        bool CreateKeyFile(string data, string username);
    }
}
