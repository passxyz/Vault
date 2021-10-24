using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Markdig;

using PassXYZLib;
using PassXYZ.Vault.ViewModels;

namespace PassXYZ.Vault.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FileInfoPage : ContentPage
    {
        private readonly FileInfoModel _viewModel;
        private readonly PxUser pxUser;

        public FileInfoPage(PxUser user)
        {
            InitializeComponent();
            pxUser = user;
            BindingContext = _viewModel = new FileInfoModel(user);
        }

        private async void OnButtonCloseClicked(object sender, EventArgs args)
        {
            _ = await Shell.Current.Navigation.PopModalAsync();
        }

        private async void OnMenuUpdateAsync(object sender, EventArgs e)
        {
            var mi = (MenuItem)sender;

            if (mi.BindingContext is PxFileInfo fileInfo)
            {
                await _viewModel.RecoverAsync(fileInfo);
            }
        }

        protected override void OnAppearing()
        {
            var pipeline = new Markdig.MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            _footer.Text = Markdig.Markdown.ToHtml(Resx.AppResources.message_id_recover_datafile, pipeline);
            _title.Text = Resx.AppResources.menu_id_data_recovery + " - " + pxUser.Username;
            _viewModel.LoadFileAttributes();
        }
    }

    public class FileInfoModel : BaseViewModel
    {
        private readonly PxUser pxUser;
        public ObservableCollection<PxFileInfo> FileAttributes { get; }
        public Command LoadFileAttributesCommand { get; }
        public FileInfoModel(PxUser user)
        {
            pxUser = user;
            FileAttributes = new ObservableCollection<PxFileInfo>();
            LoadFileAttributesCommand = new Command(() => LoadFileAttributes());
        }

        public void LoadFileAttributes()
        {
            if (pxUser != null)
            {
                if (IsBusy)
                {
                    Debug.WriteLine("FileInfoModel: is busy and cannot load FileAttributes");
                    return;
                }

                IsBusy = true;

                FileAttributes.Clear();
                FileAttributes.Add(new PxFileInfo()
                {
                    FileType = PxSyncFileType.Local,
                    FileTypeComments = Resx.AppResources.field_id_local_data,
                    LastWriteTime = pxUser.CurrentFileStatus.LastWriteTime,
                    Length = pxUser.CurrentFileStatus.Length,
                    IconPath = System.IO.Path.Combine(PxDataFile.IconFilePath, "ic_passxyz_local.png")
                });

                FileAttributes.Add(new PxFileInfo()
                {
                    FileType = PxSyncFileType.Backup,
                    FileTypeComments = Resx.AppResources.field_id_backup_data,
                    LastWriteTime = pxUser.BackupFileStatus.LastWriteTime,
                    Length = pxUser.BackupFileStatus.Length,
                    IconPath = System.IO.Path.Combine(PxDataFile.IconFilePath, "ic_passxyz_local.png")
                });

                FileAttributes.Add(new PxFileInfo()
                {
                    FileType = PxSyncFileType.Remote,
                    FileTypeComments = Resx.AppResources.field_id_remote_data,
                    LastWriteTime = pxUser.RemoteFileStatus.LastWriteTime,
                    Length = pxUser.RemoteFileStatus.Length,
                    IconPath = System.IO.Path.Combine(PxDataFile.IconFilePath, "ic_passxyz_cloud.png")
                });

                IsBusy = false;
            }
        }

        public async Task RecoverAsync(PxFileInfo fileInfo)
        {
            if (pxUser == null) { return; }

            if (fileInfo.FileType == PxSyncFileType.Backup)
            {
                string backupPath = Path.Combine(PxDataFile.BakFilePath, pxUser.FileName);
                if (File.Exists(backupPath))
                {
                    File.Copy(backupPath, pxUser.Path, true);
                }
                Debug.WriteLine("FileInfoModel: recover from backup file");
            }
            else if (fileInfo.FileType == PxSyncFileType.Remote)
            {
                ICloudServices<PxUser> sftp = PxCloudConfig.GetCloudServices();

                _ = await sftp.DownloadFileAsync(pxUser.FileName, false);
                Debug.WriteLine("FileInfoModel: recover from remote file");
            }
            else if (fileInfo.FileType == PxSyncFileType.Local)
            {
                ICloudServices<PxUser> sftp = PxCloudConfig.GetCloudServices();
                await sftp.UploadFileAsync(pxUser.FileName);
                Debug.WriteLine("FileInfoModel: recover cloud file using local file");
            }
            else
            {
                Debug.WriteLine($"FileInfoModel: error file type {fileInfo.FileType}");
            }

            if (PxCloudConfig.IsConfigured && PxCloudConfig.IsEnabled)
            {
                await LoginViewModel.SynchronizeUsersAsync();
            }
            LoadFileAttributes();
        }
    }
}