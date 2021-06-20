using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using KeePassLib;

namespace PassXYZ.Vault.Services
{
    public interface IDataStore<T>
    {
        Task<bool> AddItemAsync(T item);
        Task<bool> UpdateItemAsync(T item);
        Task<bool> DeleteItemAsync(string id);
        Task<T> GetItemAsync(string id);
        Task<IEnumerable<T>> GetItemsAsync(bool forceRefresh = false);
        Item CurrentGroup { get; set; }
        string CurrentPath { get; }
        void SetCurrentToParent();
        Item RootGroup { get; }
        Task<bool> LoginAsync(string path, string key);
        void Logout();
    }
}
