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

        public DataStore()
        {
            ImageSource imgSource1 = "icon_about.png";
            ImageSource imgSource2 = "icon_feed.png";
            // ImageSource imgSource3 = ItemExtensions.GetImageByUrl("http://www.cmbchina.com")

            items = new List<Item>()
            {
                new PwGroup(true, true)
                {
                    Name = "New PwGroup01",
                    Notes = "The first Group"
                },
                new PwEntry(true, true)
                {
                    Name = "New PwEntry01",
                    //ImgSource = imgSource2
                },
                new PwGroup(true, true)
                {
                    Name = "New PwGroup02",
                    Notes = "The first Group",
                    ImgSource = imgSource1
                }
            };
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