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
    class PxSFtp : ICloudServices<PxUser>
    {
        private readonly PxUser _user = null;

        public PxSFtp(PxUser user)
        {
            _user = user;
        }

        private bool _isConnected = false;
        public bool IsConnected()
        {
            return _isConnected;
        }

        public async Task LoginAsync()
        {
            if (_user != null)
            {
                await Task.Run(() =>
                {
                    using (var sftp = new SftpClient(_user.CloudConfig.Hostname, _user.CloudConfig.Username, _user.CloudConfig.Password))
                    {
                        try
                        {
                            Debug.WriteLine($"SFTP: Trying to connect to {_user.CloudConfig.Hostname}.");
                            sftp.Connect();

                            if (sftp.IsConnected)
                            {
                                sftp.ChangeDirectory(_user.RemoteFilePath);
                                var fileAttribute = sftp.GetAttributes(_user.FileName);
                                _user.RemoteFileStatus.LastWriteTime = fileAttribute.LastWriteTime;
                                _user.RemoteFileStatus.Length = fileAttribute.Size;
                                _isConnected = true;
                            }
                            else
                            {
                                Debug.WriteLine($"SFTP: LoginAsync connection error.");
                            }
                        }
                        catch (SshAuthenticationException ex)
                        {
                            Debug.WriteLine($"SFTP: LoginAsync {ex}");
                        }
                        catch (SftpPathNotFoundException ex)
                        {
                            // Connection is good, but the file cannot be found.
                            _user.RemoteFileStatus.LastWriteTime = default;
                            _user.RemoteFileStatus.Length = 0;
                            Debug.WriteLine($"SFTP: LoginAsync {ex}");
                        }
                        catch (SshConnectionException ex)
                        {
                            Debug.WriteLine($"SFTP: LoginAsync {ex}");
                        }
                        catch (System.Net.Sockets.SocketException ex) 
                        {
                            Debug.WriteLine($"SFTP: LoginAsync {ex}");
                        }
                    }
                });
            }
        }

        public void Logout()
        {
            _isConnected = false;
            Debug.WriteLine("SFTP: Logout");
        }
    }
}
