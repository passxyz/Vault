using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using KeePassLib;
using PassXYZLib;

using Xamarin.Forms;

namespace PassXYZ.Vault.Services
{
    public static class TEST_DB
    {
        public static string PATH 
        { 
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "pass_d_E8f4pEk.xyz"); } 
        }

        public static string KEY = "12345";

        public static string RES_PATH = "PassXYZ.Vault.data.pass_d_E8f4pEk.xyz";
    }

    public class DataStore : IDataStore<Item>
    {
        List<Item> items;
        private readonly PasswordDb db = null;

        public DataStore()
        {
            db = PasswordDb.Instance;
            db.Open(TEST_DB.PATH, TEST_DB.KEY);
            items = db.RootGroup.GetItems();
        }

        public Item RootGroup
        {
            get => db.RootGroup;
        }

        public Item CurrentGroup
        {
            get => db.CurrentGroup;
            set
            {
                db.CurrentGroup = (PwGroup)value;
                items = db.CurrentGroup.GetItems();
            }
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