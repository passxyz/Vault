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
        private readonly PxUser _user = null;
        public PxCloudType CurrentServiceType { get => PxCloudType.SFTP; }

        public string Username { get; set; }
        public string Password { get; set; }
        public string Hostname { get; set; }
        public string RemoteHomePath { get; set; }

        public PxCloudConfig(PxUser user)
        {
            _user = user;
        }

        public ICloudServices<PxUser> GetCloudServices()
        {
            var service = new PxSFtp(_user);
            return service;
        }
    }

    public interface ICloudServices<T>
    {
        Task LoginAsync();
        bool IsConnected();
        void Logout();
    }
#endif // PASSXYZ_CLOUD_SERVICE
}
