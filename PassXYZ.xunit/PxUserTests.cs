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
    public class PxUserTests
    {
        private readonly PxUserFixture userFixture;

        public PxUserTests(PxUserFixture fixture)
        {
            userFixture = fixture;
        }

        [Fact]
        public async void LoginWithPasswordTests()
        {
            PxUser user = userFixture.user;
            Debug.WriteLine($"Current: LastWriteTime={user.CurrentFileStatus.LastWriteTime}, Length={user.CurrentFileStatus.Length}");
            Debug.WriteLine($"Local: LastWriteTime={user.LocalFileStatus.LastWriteTime}, Length={user.LocalFileStatus.Length}");
            user.CloudConfig.Username = "tester";
            user.CloudConfig.Password = "12345";
            user.CloudConfig.Hostname = "127.0.0.1";
            user.CloudConfig.RemoteHomePath = "/home/tester/pxvault";
            ICloudServices<PxUser> sftp = user.CloudConfig.GetCloudServices();

            try 
            {
                await sftp.LoginAsync();
                Debug.WriteLine("Login ...");
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
            Debug.WriteLine("Done");
        }
    }
}
