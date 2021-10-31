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
    public class PxEntryFixture : IDisposable
    {
        const string TEST_DB = "pass_d_E8f4pEk.xyz";
        const string TEST_DB_KEY = "12345";

        public PxEntryFixture()
        {
            PxDb = new PxDatabase();
            PxDb.Open(TEST_DB, TEST_DB_KEY);
        }

        public void Dispose()
        {
            PxDb.Close();
        }

        public PxDatabase PxDb { get; private set; }
        public KPCLibLogger Logger { get; private set; }

        public string Username
        {
            get { return PxDefs.GetUserNameFromDataFile(TEST_DB); }
        }
    }

    [CollectionDefinition("PxEntry collection")]
    public class PxEntryCollection : ICollectionFixture<PxEntryFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }

    [Collection("PxEntry collection")]
    public class PxEntryTests
    {
        PxEntryFixture passxyz;

        public PxEntryTests(PxEntryFixture fixture)
        {
            this.passxyz = fixture;
        }

        [Fact]
        public void PxEntryInitTests() 
        {
            var entry = new PxEntry();
            Assert.NotNull(entry);
        }

        [Theory]
        [InlineData("pxdat://{'IsPxEntry':true,'Strings':{'000UserName':{'Value':'','IsProtected':false},'001Password':{'Value':'','IsProtected':true},'002Email':{'Value':'test@webdev.com','IsProtected':false},'003WebDAV URL':{'Value':'http://www.bing.com','IsProtected':false},'004QQ':{'Value':'123456789','IsProtected':false},'005WeChat':{'Value':'webdev','IsProtected':false},'006002Email':{'Value':'test@webdev.com','IsProtected':false},'Notes':{'Value':'This is a WebDAV entry.','IsProtected':false},'Title':{'Value':'WebDAV','IsProtected':false}}}")]
        public void CreatePxEntryFromJsonTest(string str)
        {
            PxEntry pxEntry = new PxEntry(str);
            Assert.True(pxEntry.IsPxEntry());
            Debug.WriteLine($"{pxEntry}");
        }

        [Theory]
        [InlineData("pxdat://{'IsPxEntry':false,'Strings':{'Email':{'Value':'','IsProtected':false},'Mobile':{'Value':'','IsProtected':false},'Notes':{'Value':'','IsProtected':false},'Password':{'Value':'','IsProtected':true},'Title':{'Value':'KeePass Entry','IsProtected':false},'URL':{'Value':'','IsProtected':false},'UserName':{'Value':'test1','IsProtected':false}}}")]
        public void CreatePwEntryFromJsonTest(string str)
        {
            PxEntry pxEntry = new PxEntry(str);
            Assert.False(pxEntry.IsPxEntry());
            Debug.WriteLine($"{pxEntry}");
        }

        [Fact]
        public void PrintPxEntryTest() 
        {
            Debug.WriteLine("*** PrintPxEntryTest ***");
            foreach (PwEntry entry in passxyz.PxDb.GetAllEntries())
            {
                if (entry.IsPxEntry())
                {
                    var plainFields = new PxPlainFields(entry);
                    Debug.WriteLine($"{plainFields}");

                    Debug.WriteLine($"Title: {entry.Name}");
                    Debug.WriteLine($"{entry.EncodeKey("TestKey")}");
                    var fields = entry.GetFields();
                    foreach (var field in fields) { Debug.WriteLine($"{field.EncodedKey}={field.Value}"); }
                    Debug.WriteLine($"Notes: {entry.GetNotes()}");
                    Assert.True(true);
                    return;
                }
            }
            Debug.WriteLine("Cannot find PxEntry.");
            Assert.True(false);
        }

        [Fact]
        public void PrintPwEntryTest()
        {
            Debug.WriteLine("*** PrintPwEntryTest ***");
            foreach (PwEntry entry in passxyz.PxDb.GetAllEntries())
            {
                if(!entry.IsNotes()) 
                {
                    if (!entry.IsPxEntry())
                    {
                        var plainFields = new PxPlainFields(entry);
                        Debug.WriteLine($"{plainFields}");
                        Debug.WriteLine($"Title: {entry.Name}");
                        var fields = entry.GetFields();
                        foreach (var field in fields) { Debug.WriteLine($"{field.Key}={field.Value}"); }
                        Debug.WriteLine($"Notes: {entry.GetNotes()}");
                        Assert.True(true);
                        return;
                    }
                }
            }
            Debug.WriteLine("Cannot find PwEntry.");
            Assert.True(false);
        }

        [Fact]
        public void PrintNotesTest()
        {
            Debug.WriteLine("*** PrintNotesTest ***");
            foreach (PwEntry entry in passxyz.PxDb.GetAllEntries())
            {
                if (entry.IsNotes())
                {
                    Debug.WriteLine($"Title: {entry.Name}");
                    Debug.WriteLine($"Notes: {entry.GetNotes()}");
                    Assert.True(true);
                    return;
                }
            }
            Debug.WriteLine("Cannot find Notes.");
            Assert.True(false);
        }
    }
}
