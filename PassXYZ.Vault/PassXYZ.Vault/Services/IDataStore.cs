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
        Task<IEnumerable<T>> GetItemsAsync(bool forceRefresh = false);
        Item CurrentGroup { get; set; }
        string CurrentPath { get; }
        void SetCurrentToParent();
        Item RootGroup { get; }
        Task<bool> LoginAsync(PassXYZLib.User user);
        void Logout();
        string GetStoreName();
        DateTime GetStoreModifiedTime();
        User CurrentUser { get; }
    }
}
