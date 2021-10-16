using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Essentials;

using FontAwesome.Solid;

namespace PassXYZLib
{
#if PASSXYZ_CLOUD_SERVICE
    public abstract class PxFileStatus
    {
        private readonly PxUser _user;
        public PxUser User { get => _user; }
        virtual public bool IsModified { get; set; } = false;
        virtual public DateTime LastWriteTime { get; set; }
        virtual public long Length { get; set; }
        public PxFileStatus(PxUser user)
        {
            _user = user;
        }
    }

    public class PxRemoteFileStatus : PxFileStatus
    {
        public override DateTime LastWriteTime { get; set; }
        public override long Length { get; set; }
        public PxRemoteFileStatus(PxUser user) : base(user)
        {
        }
    }

    public class PxLocalFileStatus : PxFileStatus
    {
        public override DateTime LastWriteTime
        {
            get
            {
                if (User.IsUserExist)
                {
                    var lastWriteTime = File.GetLastWriteTime(User.Path);
                    return Preferences.Get(User.FileName + nameof(LastWriteTime), lastWriteTime);
                }
                else
                {
                    return default;
                }
            }
            set
            {
                if (User.IsUserExist) 
                {
                    Preferences.Set(User.FileName + nameof(LastWriteTime), value);
                }
            }
        }

        public override long Length
        {
            get
            {
                if (User.IsUserExist)
                {
                    var fileInfo = new FileInfo(User.Path);
                    return Preferences.Get(User.FileName + nameof(Length), fileInfo.Length);
                }
                else
                {
                    return 0;
                }
            }

            set
            {
                if (User.IsUserExist)
                {
                    Preferences.Set(User.FileName + nameof(Length), value);
                }
            }
        }

        /// <summary>
        /// Has the remote file been changed?
        /// true  - remote file has been change,
        /// false - remote file is the same as the local one.
        /// </summary>

        public override bool IsModified
        {
            get
            {
                if (User.IsUserExist)
                {
                    if (LastWriteTime == User.RemoteFileStatus.LastWriteTime &&
                        Length == User.RemoteFileStatus.Length)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        public PxLocalFileStatus(PxUser user) : base(user)
        {
        }
    }

    public class PxCurrentFileStatus : PxFileStatus
    {
        public override DateTime LastWriteTime
        {
            get
            {
                if (User.IsUserExist)
                {
                    return File.GetLastWriteTime(User.Path);
                }
                else
                {
                    return default;
                }
            }
        }

        public override long Length
        {
            get
            {
                if (User.IsUserExist)
                {
                    var fileInfo = new FileInfo(User.Path);
                    return fileInfo.Length;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Has the local file changed?
        /// true  - the file is changed locally,
        /// false - the file is not touched
        /// </summary>
        public override bool IsModified
        {
            get
            {
                if (User.IsUserExist)
                {
                    return Preferences.Get(User.FileName, LastWriteTime != User.LocalFileStatus.LastWriteTime || Length != User.LocalFileStatus.Length);
                    //return LastWriteTime != User.LocalFileStatus.LastWriteTime ||Length != User.LocalFileStatus.Length;
                }
                else
                {
                    return false;
                }
            }

            set
            {
                if (User.IsUserExist)
                {
                    Preferences.Set(User.FileName, value);
                }
            }
        }

        public PxCurrentFileStatus(PxUser user) : base(user)
        {
        }
    }
#endif // PASSXYZ_CLOUD_SERVICE

    /// <summary>
    /// This is a class extended PassXYZLib.User. It has a dependency on Xamarin.Forms.
    /// </summary>
    public class PxUser : User
    {
        /// <summary>
        /// The DefaultTimeout is set to 120 seconds.
        /// If this is too short, there is problem to apply sync package.
        /// </summary>
        public static int DefaultTimeout = 120;

        /// <summary>
        /// The Timeout value to close the database.
        /// </summary>
        public static int AppTimeout
        {
            get => Preferences.Get(nameof(AppTimeout), DefaultTimeout);

            set => Preferences.Set(nameof(AppTimeout), value);
        }

        /// <summary>
        /// This is the icon for the user. We will use an icon to differentiate the user with Device Lock.
        /// </summary>
        public ImageSource ImgSource => new IconSource
        {
            Icon = IsDeviceLockEnabled ? Icon.UserLock : Icon.User
        };

        /// <summary>
        /// Load local users
        /// </summary>
        public static async Task<IEnumerable<PxUser>> LoadLocalUsersAsync()
        {
            List<PxUser> localUsers = new List<PxUser>();

            return await Task.Run(() => {
                var dataFiles = Directory.EnumerateFiles(PxDataFile.DataFilePath, PxDefs.all_xyz);
                foreach (string currentFile in dataFiles)
                {
                    string fileName = System.IO.Path.GetFileName(currentFile);
                    PxUser pxUser = new PxUser(fileName);
                    if (!string.IsNullOrWhiteSpace(pxUser.Username))
                    {
                        localUsers.Add(pxUser);
                    }
                }
                Debug.WriteLine($"PxUser: LoadLocalUsersAsync {localUsers.Count}");
                return localUsers;
            });
        }

        /// <summary>
        /// Remove all temporary files
        /// </summary>
        public static async Task RemoveTempFilesAsync()
        {
            await Task.Run(() => {
                var dataFiles = Directory.EnumerateFiles(PxDataFile.TmpFilePath, PxDefs.all_xyz);
                foreach (string currentFile in dataFiles)
                {
                    File.Delete(currentFile);
                    Debug.WriteLine($"PxUser: RemoveTempFiles {currentFile}");
                }
            });
        }

        /// <summary>
        /// Create an instance from filename
        /// </summary>
        /// <param name="fileName">File name used to decode username</param>
        public PxUser(string fileName) : this()
        {
            string trimedName;

            if (fileName.StartsWith(PxDefs.head_xyz) || fileName.StartsWith(PxDefs.head_data))
            {
                trimedName = fileName.Substring(PxDefs.head_xyz.Length);
                trimedName = trimedName.Substring(0, trimedName.LastIndexOf(PxDefs.xyz));
                try
                {
                    if (trimedName != null)
                    {
                        trimedName = Base58CheckEncoding.GetString(trimedName);
                        Username = trimedName;
                    }

                    if(fileName.StartsWith(PxDefs.head_data))
                    {
                        IsDeviceLockEnabled = true;
                    }
                }
                catch (FormatException e)
                {
                    Debug.WriteLine($"PxUser: {e.Message}");
                }
            }
            else
            {
                Debug.WriteLine($"PxUser: {fileName} is not PassXYZ data file.");
            }
        }

        public virtual void Logout() { }

#if PASSXYZ_CLOUD_SERVICE

        #region PxUserFileStatus
        public PxFileStatus RemoteFileStatus;
        public PxFileStatus LocalFileStatus;
        public PxFileStatus CurrentFileStatus;
        private PxCloudSyncStatus _syncStatus = PxCloudSyncStatus.PxLocal;
        public PxCloudSyncStatus SyncStatus
        {
            get => _syncStatus;
            set
            {
                _syncStatus = value;
                if (_syncStatus == PxCloudSyncStatus.PxSynced || _syncStatus == PxCloudSyncStatus.PxSyncing)
                {
                    LocalFileStatus.LastWriteTime = RemoteFileStatus.LastWriteTime;
                    LocalFileStatus.Length = RemoteFileStatus.Length;
                    CurrentFileStatus.IsModified = false;
                }
            }
        }

        public PxUser()
        {
            RemoteFileStatus = new PxRemoteFileStatus(this);
            LocalFileStatus = new PxLocalFileStatus(this);
            CurrentFileStatus = new PxCurrentFileStatus(this);
        }
        #endregion
#else
        public PxUser()
        { 
        }
#endif // PASSXYZ_CLOUD_SERVICE

    }

    public class PxUserComparer : IEqualityComparer<PxUser>
    {
        bool IEqualityComparer<PxUser>.Equals(PxUser x, PxUser y)
        {
            return (x.Username.Equals(y.Username));
        }

        int IEqualityComparer<PxUser>.GetHashCode(PxUser obj)
        {
            if (obj is null)
                return 0;

            return obj.ToString().GetHashCode();
        }
    }
}
