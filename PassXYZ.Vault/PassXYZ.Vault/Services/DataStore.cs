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
    public class EmbeddedDatabase
    {
        public string Path;
        public string Key;
        public string ResourcePath;

        public EmbeddedDatabase(string path, string key, string rpath)
        {
            Path = path;
            Key = key;
            ResourcePath = rpath;
        }
    }

    public static class TEST_DB
    {
        public static EmbeddedDatabase[] DataFiles = new EmbeddedDatabase[]
        {
            new EmbeddedDatabase(Path.Combine(PxDataFile.DataFilePath, "pass_d_E8f4pEk.xyz"), "12345", "PassXYZ.Vault.data.pass_d_E8f4pEk.xyz"),
            new EmbeddedDatabase(Path.Combine(PxDataFile.DataFilePath, "pass_e_JyHzpRxcopt.xyz"), "123123", "PassXYZ.Vault.data.pass_e_JyHzpRxcopt.xyz"),
            new EmbeddedDatabase(Path.Combine(PxDataFile.KeyFilePath, "pass_k_JyHzpRxcopt.k4xyz"), "", "PassXYZ.Vault.data.pass_k_JyHzpRxcopt.k4xyz")
        };
    }

    public class DataStore : IDataStore<Item>
    {
        private List<Item> items;
        private readonly PasswordDb db = null;

        public DataStore()
        {
            db = PasswordDb.Instance;
        }

        public Item RootGroup
        {
            get => db.RootGroup;
        }

        public string CurrentPath => db.CurrentPath;

        public Item CurrentGroup
        {
            get => db.CurrentGroup;
            set
            {
                db.CurrentGroup = (PwGroup)value;
                items = db.CurrentGroup.GetItems();
            }
        }

        public void SetCurrentToParent()
        {
            if (!CurrentGroup.Uuid.Equals(RootGroup.Uuid))
            {
                CurrentGroup = db.CurrentGroup.ParentGroup;
            }
        }

        public async Task<bool> AddItemAsync(Item item)
        {
            items.Add(item);
            if(item.IsGroup)
            {
                db.CurrentGroup.AddGroup(item as PwGroup, true);
            }
            else
            {
                db.CurrentGroup.AddEntry(item as PwEntry, true);
            }

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
            Item oldItem = items.FirstOrDefault((Item arg) => arg.Id == id);
            if (items.Remove(oldItem))
            {
                if (oldItem.IsGroup)
                {
                    db.DeleteGroup(oldItem as PwGroup);
                }
                else
                {
                    db.DeleteEntry(oldItem as PwEntry);
                }
            }

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

        public async Task<bool> LoginAsync(PassXYZLib.User user)
        {
            db.Open(user);
            if (db.IsOpen)
            {
                items = db.RootGroup.GetItems();
            }

            return await Task.FromResult(db.IsOpen);
        }

        public void Logout()
        {
            if (db.IsOpen) { db.Close(); }
        }
    }
}