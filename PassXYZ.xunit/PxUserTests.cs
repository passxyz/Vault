using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

using KeePassLib;
using PassXYZLib;

namespace PassXYZ.xunit
{
    public class PxUserFixture : IDisposable
    {
        const string TEST_DB = "pass_d_E8f4pEk.xyz";
        const string TEST_DB_KEY = "12345";
        const string TEST_DB_USER = "test1";
        public PxUser user;

        public PxUserFixture()
        {
            user = new PxUser
            {
                Username = TEST_DB_USER,
                Password = TEST_DB_KEY
            };
            PxCloudConfig.Username = "tester";
            PxCloudConfig.Password = "12345";
            PxCloudConfig.Hostname = "172.28.67.181";
            PxCloudConfig.RemoteHomePath = "/home/tester/pxvault/";

            PxDb = new PxDatabase();
            if (user.IsUserExist) 
            {
                PxDb.Open(user);
            }
        }

        public void Dispose()
        {
            PxDb.Close();
        }

        public PxDatabase PxDb { get; private set; }
        public KPCLibLogger Logger { get; private set; }
    }

    [CollectionDefinition("PxUser collection")]
    public class PxUserCollection : ICollectionFixture<PxUserFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }

    [Collection("PxUser collection")]
    [TestCaseOrderer("PassXYZ.xunit.PxAlphabeticalOrderer", "PassXYZ.xunit")]
    public class PxUserTests
    {
        private readonly PxUserFixture userFixture;

        public PxUserTests(PxUserFixture fixture)
        {
            userFixture = fixture;
        }

        [Fact]
        public async void Test1LoginWithPassword()
        {
            PxUser user = userFixture.user;
            ICloudServices<PxUser> sftp = PxCloudConfig.GetCloudServices();

            try
            {
                if (PxCloudConfig.IsConfigured) 
                {
                    await sftp.LoginAsync();
                    Debug.WriteLine("Login ...");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Login status: {ex}");
            }

            if (sftp.IsConnected())
            {
                var rfStatus = user.RemoteFileStatus;
                Debug.WriteLine($"LastWriteTime={rfStatus.LastWriteTime}, Length={rfStatus.Length}");
            }
            Assert.True(sftp.IsConnected());
            Debug.WriteLine("Done");
        }

        [Fact]
        public async void Test2UploadFile()
        {
            PxUser user = userFixture.user;
            ICloudServices<PxUser> sftp = PxCloudConfig.GetCloudServices();
            await sftp.UploadFileAsync(user.FileName);
            var user1 = new PxUser
            {
                Username = "kpclibpy",
                Password = "123123",
                IsDeviceLockEnabled = true
            };

            await sftp.UploadFileAsync(user1.FileName);
            Assert.True(sftp.IsConnected());
            Debug.WriteLine($"UploadFileTests: {user.FileName}, {user1.FileName}");
        }

        [Fact]
        public async void Test3DownloadFile()
        {
            PxUser user = userFixture.user;
            ICloudServices<PxUser> sftp = PxCloudConfig.GetCloudServices();
            var user1 = new PxUser
            {
                Username = "kpclibpy",
                Password = "123123",
                IsDeviceLockEnabled = true
            };

            var path = await sftp.DownloadFileAsync(user1.FileName, false);
            Assert.True(sftp.IsConnected());
            Debug.WriteLine($"DownloadTests: {path}");
        }

        [Fact]
        public async void Test4GetLocalUsers() 
        {
            IEnumerable<PxUser> localUsers = await PxUser.LoadLocalUsersAsync();
            foreach (PxUser localUser in localUsers)
            {
                Debug.WriteLine($"Test4GetLocalUsers: Username={localUser.Username}, FileName={localUser.FileName}");
            }
        }

        [Fact]
        public async void Test5GetRemoteUsers()
        {
            PxUser user = userFixture.user;
            ICloudServices<PxUser> sftp = PxCloudConfig.GetCloudServices();

            try
            {
                IEnumerable<PxUser> remoteUsers = await sftp.LoadRemoteUsersAsync();
                foreach (PxUser remoteUser in remoteUsers)
                {
                    Debug.WriteLine($"Test5GetRemoteUsers: Username={remoteUser.Username}, FileName={remoteUser.FileName}");
                }
                Assert.True(sftp.IsConnected());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetUsersTests: {ex}");
                Assert.False(sftp.IsConnected());
            }
        }

        [Fact]
        public async void Test6DeleteFile()
        {
            PxUser user = userFixture.user;
            ICloudServices<PxUser> sftp = PxCloudConfig.GetCloudServices();

            await sftp.UploadFileAsync(user.FileName);
            bool result = await sftp.DeleteFileAsync(user.FileName);
            Assert.True(result);
            Debug.WriteLine($"DeleteFileTest: {user.FileName}");
        }

        [Fact]
        public async void Test7DeleteFileFailed() 
        {
            PxUser user = userFixture.user;
            user.Username = "B@d U$er";
            ICloudServices<PxUser> sftp = PxCloudConfig.GetCloudServices();

            bool result = await sftp.DeleteFileAsync(user.FileName);
            Assert.False(result);
            Debug.WriteLine($"DeleteFileTest: {user.FileName}");
        }

        [Fact]
        /// <summary>
        /// Get users only exist locally.
        /// </summary>
        public async void Test8GetLocalUsersOnly()
        {
            ICloudServices<PxUser> sftp = PxCloudConfig.GetCloudServices();

            try
            {
                IEnumerable<PxUser> localUsers = await PxUser.LoadLocalUsersAsync();
                IEnumerable<PxUser> remoteUsers = await sftp.LoadRemoteUsersAsync();
                IEnumerable<PxUser> localOnyUsers = localUsers.Except(remoteUsers, new PxUserComparer()).ToList();
                foreach (PxUser localOnlyUser in localOnyUsers)
                {
                    Debug.WriteLine($"Test8GetLocalUsersOnly: Username={localOnlyUser.Username}, FileName={localOnlyUser.FileName}");
                }
                Assert.True(sftp.IsConnected());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Test8GetLocalUsersOnly: {ex}");
                Assert.False(sftp.IsConnected());
            }
        }
    

        [Fact]
        /// <summary>
        /// Get users only exist remotely.
        /// </summary>
        public async void Test9GetRemoteUsersOnly()
        {
            ICloudServices<PxUser> sftp = PxCloudConfig.GetCloudServices();

            try
            {
                IEnumerable<PxUser> localUsers = await PxUser.LoadLocalUsersAsync();
                IEnumerable<PxUser> remoteUsers = await sftp.LoadRemoteUsersAsync();
                IEnumerable<PxUser> remoteOnyUsers = remoteUsers.Except(localUsers, new PxUserComparer()).ToList();
                foreach (PxUser remoteOnlyUser in remoteOnyUsers)
                {
                    Debug.WriteLine($"Test9GetRemoteUsersOnly: Username={remoteOnlyUser.Username}, FileName={remoteOnlyUser.FileName}");
                }
                Assert.True(sftp.IsConnected());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Test9GetRemoteUsersOnly: {ex}");
                Assert.False(sftp.IsConnected());
            }
        }

        [Fact]
        /// <summary>
        /// Get users exist both locally and remotely.
        /// </summary>
        public async void Test10GetSyncedUsers()
        {
            ICloudServices<PxUser> sftp = PxCloudConfig.GetCloudServices();

            try
            {
                IEnumerable<PxUser> localUsers = await PxUser.LoadLocalUsersAsync();
                IEnumerable<PxUser> remoteUsers = await sftp.LoadRemoteUsersAsync();
                IEnumerable<PxUser> syncedUsers = remoteUsers.Intersect(localUsers, new PxUserComparer());
                foreach (PxUser syncedUser in syncedUsers)
                {
                    Debug.WriteLine($"Test10GetSyncedUsers: Username={syncedUser.Username}, FileName={syncedUser.FileName}");
                }
                Assert.True(sftp.IsConnected());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Test10GetSyncedUsers: {ex}");
                Assert.False(sftp.IsConnected());
            }
        }
    }
}
