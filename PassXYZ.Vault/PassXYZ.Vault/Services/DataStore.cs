using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Keys;
using KeePassLib.Security;
using KeePassLib.Serialization;
using PassXYZLib;

using PassXYZ.Vault.Resx;

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
        private User _user;

        public DataStore()
        {
            db = PasswordDb.Instance;
        }

        public bool IsOpen => db != null && db.IsOpen;

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

        public User CurrentUser
        {
            get => _user;
        }

        public void SetCurrentToParent()
        {
            if (!CurrentGroup.Uuid.Equals(RootGroup.Uuid))
            {
                CurrentGroup = db.CurrentGroup.ParentGroup;
            }
        }

        private async Task SaveAsync()
        {
            var logger = new KPCLibLogger();
            db.DescriptionChanged = DateTime.UtcNow;
            await Task.Run(() => db.Save(logger));
        }

        public async Task AddItemAsync(Item item)
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
            await SaveAsync();
            Debug.WriteLine($"DataStore: AddItemAsync({item.Name}), saved");
        }

        public async Task UpdateItemAsync(Item item)
        {
            //var oldItem = items.Where((Item arg) => arg.Uuid == item.Uuid).FirstOrDefault();
            //items.Remove(oldItem);
            //items.Add(item);
            item.LastModificationTime = DateTime.UtcNow;
            await SaveAsync();
            Debug.WriteLine($"DataStore: UpdateItemAsync({item.Name}), saved");
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
                await SaveAsync();
                Debug.WriteLine($"DataStore: DeleteItemAsync({oldItem.Name}), saved");
                return await Task.FromResult(true);
            }
            else
            {
                return await Task.FromResult(false);
            }
        }

        public async Task<Item> GetItemAsync(string id)
        {
            return await Task.FromResult(items.FirstOrDefault(s => s.Id == id));
        }

        public async Task<PwEntry> FindEntryByIdAsync(string id)
        {
            return await Task.Run(() => { return db.FindEntryById(id); });
        }

        public PwEntry FindEntryById(string id)
        {
            return db.FindEntryById(id);
        }

        public async Task<IEnumerable<Item>> GetItemsAsync(bool forceRefresh = false)
        {
            return await Task.FromResult(items);
        }

        /// <summary>
        /// Search entries with a keyword
        /// </summary>
        /// <param name="strSearch">keyword to be searched</param>
        /// <param name="itemGroup">If it is not null, this group is searched</param>
        /// <returns>a list of entries</returns>
        public async Task<IEnumerable<Item>> SearchEntriesAsync(string strSearch, Item itemGroup = null)
        {
            return await Task.Run(() => { return db.SearchEntries(strSearch, itemGroup); });
        }

        public async Task<IEnumerable<PwEntry>> GetOtpEntryListAsync()
        {
            return await Task.Run(() => { return db.GetOtpEntryList(); });
        }

        public async Task<bool> LoginAsync(PassXYZLib.User user)
        {
            if (user == null) { Debug.Assert(false); throw new ArgumentNullException("user"); }
            _user = user;

            db.Open(user);
            if (db.IsOpen)
            {
                items = db.RootGroup.GetItems();
            }

            return await Task.FromResult(db.IsOpen);
        }

        public async Task SignUpAsync(PassXYZLib.User user)
        {
            if (user == null) { Debug.Assert(false); throw new ArgumentNullException("user"); }
            
            var logger = new KPCLibLogger();
            await Task.Run(() => {
                db.New(user);
                // Create a PassXYZ Usage note entry
                PwEntry pe = new PwEntry(true, true);
                pe.Strings.Set(PxDefs.TitleField, new ProtectedString(false, AppResources.entry_id_passxyz_usage));
                pe.Strings.Set(PxDefs.NotesField, new ProtectedString(false, AppResources.about_passxyz_usage));
                //pe.CustomData.Set(Item.TemplateType, ItemSubType.Notes.ToString());
                //pe.CustomData.Set(Item.PxIconName, "ic_entry_passxyz.png");
                pe.SetType(ItemSubType.Notes);
                db.RootGroup.AddEntry(pe, true);

                try
                {
                    logger.StartLogging("Saving database ...", true);
                    db.DescriptionChanged = DateTime.UtcNow;
                    db.Save(logger);
                    logger.EndLogging();
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Failed to save database." + e.Message);
                }
            });

        }

        public void Logout()
        {
            if (db.IsOpen) { db.Close(); }
        }

        public string GetStoreName()
        {
            return db.Name;
        }

        public DateTime GetStoreModifiedTime()
        {
            return db.DescriptionChanged;
        }

        public async Task<bool> ChangeMasterPassword(string newPassword)
        {
            bool result = db.ChangeMasterPassword(newPassword, _user);
            if (result)
            {
                db.MasterKeyChanged = DateTime.UtcNow;
                // Save the database to take effect
                await SaveAsync();
            }
            return result;
        }

        public string GetMasterPassword()
        {
            var userKey = db.MasterKey.GetUserKey(typeof(KcpPassword)) as KcpPassword;
            return userKey.Password.ReadString();
        }

        public string GetDeviceLockData()
        {
            return db.GetDeviceLockData(_user);
        }

        /// <summary>
        /// Recreate a key file from a PxKeyData
        /// </summary>
        /// <param name="data">PxKeyData source</param>
        /// <param name="username">username inside PxKeyData source</param>
        /// <returns>true - created key file, false - failed to create key file.</returns>
        public bool CreateKeyFile(string data, string username)
        {
            return db.CreateKeyFile(data, username);
        }
    }
}