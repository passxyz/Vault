using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace PassXYZLib
{
#if PASSXYZ_CLOUD_SERVICE
    class PxSFtp : ICloudServices<PxUser>
    {
        private readonly PxCloudConfig cloudConfig = null;

        public PxSFtp(PxCloudConfig config)
        {
            cloudConfig = config;
        }

        private bool _isConnected = false;
        public bool IsConnected()
        {
            return _isConnected;
        }

        private async Task<bool> ConnectAsync(Action<SftpClient> updateAction)
        {
            await Task.Run(() =>
            {
                _isConnected = false;
                using (var sftp = new SftpClient(cloudConfig.Hostname, cloudConfig.Username, cloudConfig.Password))
                {
                    try
                    {
                        Debug.WriteLine($"SFTP: Trying to connect to {cloudConfig.Hostname}.");
                        sftp.Connect();

                        if (sftp.IsConnected)
                        {
                            _isConnected = true;
                            updateAction?.Invoke(sftp);
                        }
                        else
                        {
                            Debug.WriteLine($"SFTP: connection error.");
                        }
                    }
                    catch (SshAuthenticationException ex)
                    {
                        _isConnected = false;
                        Debug.WriteLine($"SFTP: {ex}");
                    }
                    catch (SftpPathNotFoundException ex)
                    {
                        _isConnected = false;
                        Debug.WriteLine($"SFTP: {ex}");
                    }
                    catch (SshConnectionException ex)
                    {
                        _isConnected = false;
                        Debug.WriteLine($"SFTP: {ex}");
                    }
                    catch (System.Net.Sockets.SocketException ex)
                    {
                        _isConnected = false;
                        Debug.WriteLine($"SFTP: {ex}");
                    }
                }
            });
            return true;
        }

        public async Task LoginAsync()
        {
            if (cloudConfig != null)
            {
                await ConnectAsync((SftpClient sftp) => {
                    Debug.WriteLine($"SFTP: LoginAsync {sftp.IsConnected}");
                });
            }
        }

        public async Task<string> DownloadFileAsync(string filename, bool isMerge = true)
        {
            if (cloudConfig != null)
            {
                var path = string.Empty;
                if (isMerge)
                {
                    path = Path.Combine(PxDataFile.TmpFilePath, filename);
                }
                else
                {
                    path = Path.Combine(PxDataFile.DataFilePath, filename);
                }

                var remotepath = cloudConfig.RemoteHomePath + filename;
                await ConnectAsync((SftpClient sftp) => {
                    using (var fileStr = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None,
                                bufferSize: 4096, useAsync: true)) 
                    {
                        if (fileStr != null)
                        {
                            sftp.DownloadFile(remotepath, fileStr);
                        }
                    }
                });
                return path;
            }
            return string.Empty;
        }

        public async Task UploadFileAsync(string filename)
        {
            if (cloudConfig != null)
            {
                var localPath = Path.Combine(PxDataFile.DataFilePath, filename);
                string remotePath = cloudConfig.RemoteHomePath + filename;
                await ConnectAsync((SftpClient sftp) => {
                    using (var uploadedFileStream = new FileStream(localPath, FileMode.Open)) 
                    {
                        sftp.UploadFile(uploadedFileStream, remotePath, true, null);
                        Debug.WriteLine($"SFTP: UploadFileAsync {sftp.IsConnected}");
                    }
                });
            }
        }

        public async Task<bool> DeleteFileAsync(string filename)
        {
            if (cloudConfig != null)
            {
                await ConnectAsync((SftpClient sftp) => {
                    string remotePath = cloudConfig.RemoteHomePath + filename;
                    var file = sftp.Get(remotePath);
                    file.Delete();
                    Debug.WriteLine($"SFTP: DeleteFileAsync {sftp.IsConnected}");
                });

                if (_isConnected)
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<IEnumerable<PxUser>> GetCloudUsersListAsync()
        {
            if (cloudConfig != null)
            {
                List<PxUser> Users = new List<PxUser>();

                await ConnectAsync((SftpClient sftp) => {
                    Users.Clear();
                    foreach (SftpFile remoteFile in sftp.ListDirectory(cloudConfig.RemoteHomePath))
                    {
                        if (Path.GetExtension(remoteFile.FullName) == ".xyz")
                        {
                            // If it is a PassXYZ data file, then we add it to the list.
                            string userName = PxDataFile.GetUserName(Path.GetFileName(remoteFile.FullName));
                            if (!string.IsNullOrWhiteSpace(userName))
                            {
                                Users.Add(
                                    new PxUser()
                                    {
                                        Username = userName
                                    });
                            }
                        }
                    }
                });
                return Users;
            }
            return null;
        }

        public void Logout()
        {
            _isConnected = false;
            Debug.WriteLine("SFTP: Logout");
        }
    }
#endif // PASSXYZ_CLOUD_SERVICE
}
