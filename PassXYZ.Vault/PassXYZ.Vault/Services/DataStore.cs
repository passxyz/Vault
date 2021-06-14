using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using KeePassLib;
using PassXYZLib;

using Xamarin.Forms;

namespace PassXYZ.Vault.Services
{
    public class DataStore : IDataStore<Item>
    {
        readonly List<Item> items;
        PasswordDb db = null;

        public DataStore()
        {
            const string TEST_DB = "pass_d_E8f4pEk.xyz";
            const string TEST_DB_KEY = "12345";

            db = PasswordDb.Instance;
            db.Open(TEST_DB, TEST_DB_KEY);
            items = db.CurrentGroup.GetItems();
        }

        public async Task<bool> AddItemAsync(Item item)
        {
            items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateItemAsync(Item item)
        {
            var oldItem = items.Where((Item arg) => arg.Uuid == item.Uuid).FirstOrDefault();
            items.Remove(oldItem);
            items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteItemAsync(string id)
        {
            var oldItem = items.Where((Item arg) => arg.Id == id).FirstOrDefault();
            items.Remove(oldItem);

            return await Task.FromResult(true);
        }

        public async Task<Item> GetItemAsync(string id)
        {
            return await Task.FromResult(items.FirstOrDefault(s => s.Id == id));
        }

        public async Task<IEnumerable<Item>> GetItemsAsync(bool forceRefresh = false)
        {
            return await Task.FromResult(items);
        }
    }
}