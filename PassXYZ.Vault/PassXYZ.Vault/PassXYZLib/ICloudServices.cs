using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PassXYZLib
{
#if PASSXYZ_CLOUD_SERVICE
    public enum PxCloudType
    {
        OneDrive,
        WebDav,
        SFTP,
        FTP,
        SMB
    }

    public class PxCloudConfig
    {
        public PxCloudType CurrentServiceType { get => PxCloudType.SFTP; }

        public string Username { get; set; }
        public string Password { get; set; }
        public string Hostname { get; set; }
        public string RemoteHomePath { get; set; }

        public PxCloudConfig()
        {
        }

        public ICloudServices<PxUser> GetCloudServices()
        {
            var service = new PxSFtp(this);
            return service;
        }
    }

    public interface ICloudServices<T>
    {
        Task LoginAsync();
        bool IsConnected();
        Task<string> DownloadFileAsync(string filename, bool isMerge = false);
        Task UploadFileAsync(string filename);
        Task<bool> DeleteFileAsync(string filename);
        Task<IEnumerable<T>> GetCloudUsersListAsync();
        void Logout();
    }
#endif // PASSXYZ_CLOUD_SERVICE
}
