using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
        public static bool IsConnected { get; } = false;
        virtual public bool IsModified { get; } = false;
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
                    if (IsConnected)
                    {
                        return Preferences.Get(nameof(PxLocalFileStatus) + nameof(LastWriteTime), lastWriteTime);
                    }
                    else
                    {
                        return lastWriteTime;
                    }
                }
                else
                {
                    return default;
                }
            }
            set
            {
                if (User.IsUserExist && IsConnected)
                {
                    Preferences.Set(nameof(PxLocalFileStatus) + nameof(LastWriteTime), value);
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
                    if (IsConnected)
                    {
                        return Preferences.Get(nameof(PxLocalFileStatus) + nameof(Length), fileInfo.Length);
                    }
                    else
                    {
                        return fileInfo.Length;
                    }
                }
                else
                {
                    return 0;
                }
            }

            set
            {
                if (User.IsUserExist && IsConnected)
                {
                    Preferences.Set(nameof(PxLocalFileStatus) + nameof(Length), value);
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
                if (User.IsUserExist && IsConnected)
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
                    if (LastWriteTime == User.LocalFileStatus.LastWriteTime &&
                    Length == User.LocalFileStatus.Length)
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

#if PASSXYZ_CLOUD_SERVICE

        #region PxUserFileStatus
        public PxFileStatus RemoteFileStatus;
        public PxFileStatus LocalFileStatus;
        public PxFileStatus CurrentFileStatus;
        public PxCloudSyncStatus SyncStatus = PxCloudSyncStatus.PxLocal;

        public PxUser()
        {
            RemoteFileStatus = new PxRemoteFileStatus(this);
            LocalFileStatus = new PxLocalFileStatus(this);
            CurrentFileStatus = new PxCurrentFileStatus(this);
        }
        #endregion

#endif // PASSXYZ_CLOUD_SERVICE

    }
}
