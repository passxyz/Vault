using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Xamarin.Forms;

using KeePassLib;
using PassXYZLib;

using PassXYZ.Vault.Views;

namespace PassXYZ.Vault.ViewModels
{
    internal class OtpListViewModel : BaseViewModel
    {
        public ObservableCollection<PwEntry> Entries { get; set; }
        public Command GetOtpListCommand { get; set; }
        public Command UpdateTokenCommand { get; set; }
        public bool UpdateTokenDone = true;

        public OtpListViewModel()
        {
            Entries = new ObservableCollection<PwEntry>();
            GetOtpListCommand = new Command(async () => await ExecuteGetOtpListCommand());
            UpdateTokenCommand = new Command(() => ExecuteUpdateToken());
        }

        public void ExecuteUpdateToken()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            foreach (PwEntry entry in Entries)
            {
                entry.UpdateToken();
                // Debug.WriteLine($"OtpListViewModel: ExecuteUpdateToken({entry.Name} {entry.Progress})");
            }

            IsBusy = false;
        }

        public async Task ExecuteGetOtpListCommand()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                Entries.Clear();
                IEnumerable<PwEntry> items = await DataStore.GetOtpEntryListAsync();

                foreach (PwEntry entry in items)
                {
                    Entries.Add(entry);
                }

                UpdateTokenDone = true;
                Device.StartTimer(new TimeSpan(0, 0, PwEntry.TimerStep), () =>
                {
                    ExecuteUpdateToken();
                    return UpdateTokenDone; // True = Repeat again, False = Stop the timer
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OtpListViewModel: GetOtpListCommand: {ex}");
                IsBusy = false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async void OnItemSelected(Item item)
        {
            if (item == null)
            {
                return;
            }

            PwEntry entry = (PwEntry)item;
            if (entry.IsNotes())
            {
                await Shell.Current.GoToAsync($"{nameof(NotesPage)}?{nameof(ItemDetailViewModel.ItemId)}={item.Id}");
            }
            else
            {
                await Shell.Current.GoToAsync($"{nameof(ItemDetailPage)}?{nameof(ItemDetailViewModel.ItemId)}={item.Id}");
            }
        }

    }
}
