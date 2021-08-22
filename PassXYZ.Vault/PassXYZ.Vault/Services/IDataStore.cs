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
        Task<PwEntry> FindEntryById(string id);
        Task<IEnumerable<T>> GetItemsAsync(bool forceRefresh = false);
        Task<IEnumerable<PwEntry>> GetOtpEntryListAsync();
        Item CurrentGroup { get; set; }
        string CurrentPath { get; }
        void SetCurrentToParent();
        Item RootGroup { get; }
        Task<bool> LoginAsync(PassXYZLib.User user);
        void Logout();
        string GetStoreName();
        DateTime GetStoreModifiedTime();
        User CurrentUser { get; }
        Task SignUpAsync(PassXYZLib.User user);
    }
}
