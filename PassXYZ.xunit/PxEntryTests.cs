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
        [InlineData("Hello World 11", "12345678901")]
        [InlineData("Hello World 12", "123456789012")]
        [InlineData("Hello World 13", "1234567890123")]
        [InlineData("Hello World 14", "12345678901234")]
        [InlineData("Hello World 15", "123456789012345")]
        public void EncryptStringTest(string secretMessage, string password)
        {
            string encryptedMessage = PxEncryption.EncryptWithPassword(secretMessage, password);
            string decryptedMessage = PxEncryption.DecryptWithPassword(encryptedMessage, password);
            Debug.WriteLine($"EncryptStringTest: PasswdLen={password.Length}, TextLen={encryptedMessage.Length}, {decryptedMessage}");
        }

        [Theory]
        [InlineData("pxtem://{'IsPxEntry':true,'Strings':{'000UserName':{'Value':'','IsProtected':false},'001Password':{'Value':'','IsProtected':true},'002Email':{'Value':'','IsProtected':false},'003URL':{'Value':'','IsProtected':false},'004QQ':{'Value':'','IsProtected':false},'005WeChat':{'Value':'','IsProtected':false},'Notes':{'Value':'','IsProtected':false},'Title':{'Value':'PxEntry 1','IsProtected':false}}}")]
        [InlineData("pxtem://{'IsPxEntry':true,'Strings':{'000UserName':{'Value':'user2','IsProtected':false},'001Password':{'Value':'12345','IsProtected':true},'002Email':{'Value':'user2@passxyz.com','IsProtected':false},'003URL':{'Value':'https://passxyz.github.io','IsProtected':false},'004QQ':{'Value':'1234567890','IsProtected':false},'005WeChat':{'Value':'passxyz','IsProtected':false},'Notes':{'Value':'This is a PxEntry.','IsProtected':false},'Title':{'Value':'PxEntry 2','IsProtected':false}}}")]
        [InlineData("pxtem://{'IsPxEntry':true,'Strings':{'000UserName':{'Value':'','IsProtected':false},'002Email':{'Value':'','IsProtected':false},'003URL':{'Value':'','IsProtected':false},'004QQ':{'Value':'','IsProtected':false},'005WeChat':{'Value':'','IsProtected':false},'Notes':{'Value':'','IsProtected':false},'Title':{'Value':'PxEntry 3','IsProtected':false}}}")]
        public void CreatePxEntryFromJsonTest(string str)
        {
            PxEntry pxEntry = new PxEntry(str);
            Assert.True(pxEntry.IsPxEntry());
            Debug.WriteLine($"{pxEntry}");
        }

        [Theory]
        [InlineData("pxtem://{'IsPxEntry':false,'Strings':{'Password':{'Value':'','IsProtected':true},'Mobile':{'Value':'','IsProtected':false},'Notes':{'Value':'','IsProtected':false},'PIN':{'Value':'','IsProtected':true},'Title':{'Value':'KeePass Entry 1','IsProtected':false},'URL':{'Value':'','IsProtected':false},'UserName':{'Value':'test1','IsProtected':false}}}")]
        [InlineData("pxtem://{'IsPxEntry':false,'Strings':{'Password':{'Value':'12345','IsProtected':true},'Mobile':{'Value':'13678909876','IsProtected':false},'Notes':{'Value':'','IsProtected':false},'PIN':{'Value':'123','IsProtected':true},'Title':{'Value':'KeePass Entry 2','IsProtected':false},'URL':{'Value':'https://github.com','IsProtected':false},'UserName':{'Value':'test2','IsProtected':false}}}")]
        public void CreatePwEntryFromJsonTest(string str)
        {
            PxEntry pxEntry = new PxEntry(str);
            Assert.False(pxEntry.IsPxEntry());
            Debug.WriteLine($"{pxEntry}");
        }

        [Theory]
        [InlineData("12345", @"pxdat://6Dw+zRK+X0dQ5XAv71HBxs0sBZnFFt63KJGAqJTrNMBfqGU9AuWRjuCUhogft5MI4xCsGoo4S/XT75Gg4tPZ1gGGBxX6GxGpYj50X4O131kZ6vh146VPdqCP4+QRysPSg56wEaTLcp4L6fDXXfXQ7Wsv849p7luFQ6cFOFnYPpahvzhzOJbw8Z1bdcU5jrTRHVeWiMpcd33JfnFVsNr2AkkP3UW7KVZV3OOERA4CntBxVbXSpB9jJbu4q/3j6roKPO5e5TIzKPc/dtCp2Kg0xGu/la+o1MMA/N34XgqnyLLWA0xcy5PHDwDs4frnxiqOR7nqamNm6wjQXJ4JvD159mqmHDWfGB9uJosJYJBDyuCWkqMfUBJTZcG95+P9PwTrQ7E3rZwMJGyu2r0WWzgzvDq/aenZZgXkE1AzU6NN/FhrPQKoD1umMwQSLrwu5mKKq3XEQRiOWcnPkeQ1DaRZiDWq6tBjGU+h1wccsa3vxa74+2K0yxVAcumPpRbzREhd40Zay9EutYkwhwMjlrlmcVSvgWMJ8+y8pjjHYYWi1a/96w+B6ODQ/RyzUjmyQaXoVgYGBkneXxHfu8S4TRalBQ==")]
        public void CreatePwEntryFromEncrytedJsonTest(string password, string str)
        {
            PxEntry pxEntry = new PxEntry(str, password);
            Assert.False(pxEntry.IsPxEntry());
            Debug.WriteLine($"{pxEntry}");
        }

        [Theory]
        [InlineData("12345", @"pxdat://Did3addyfdEtHv10kR1Iu7tfdgaIekB0+KQ+u3aIaKazjMStxkMSt74gg0n89ytFaM9F1JfQBigY6lQgqoJuYZpZC7tpavKHdhv4XWafe3ofqsWy2HnwwsHgkfMhre1Z1QDGBjyujef9vBWWsHP6NYmAzWecJXyoq2NqOTotIefRyjfMoRWo/mHKzJfh7j4ZTkA7Sj7C/lxWsLeoC+R2IThp8MtCCX3FtT9xU0Hen+eRcxG8P2MimoETFde6ddi/Vi/Eof+z1WML86z4xui5a/mkbnq6jYIXYroo0Np60WuFKQ3rj4P4b6I6YBGsSc2Yj7SNdjWaDFL17aOwTzzCbYZC6SyMymzwHUp9Ww3lgFnsFa8tBrjrLTIDZP5j+uu6T0CkRHEqtT70Seee3z7uR91THjhrU6LAye62gfCS4peHwI88KuYUgzukQ3NJNjXRtAUZTc9hricFeOjiiHjENUbokAYd8nLXKnFKH5OEw2DOiUgB5d+ot/oL0RUwDSL9QyfehOo1VC+tW9fUKf+ySDpOf4EsA//ST/lfj9kt7O50lreclTbzrWHkYI06eWAFaLwLFWVUtwWK/eSuNVwVESfbanmjpngNELdssUDJ58hBLN0owPkU70AsA6vgEoiTh6wQM1JC4JUAO8Jb+8AZ0KpsqIcnw9umH0JHhp8Q7PfUFY3daoXc1dKi3hUAIYvUkxS31Tp24tcvxcxx7Se3og==")]
        public void CreatePxEntryFromEncrytedJsonTest(string password, string str)
        {
            PxEntry pxEntry = new PxEntry(str, password);
            Assert.True(pxEntry.IsPxEntry());
            Debug.WriteLine($"{pxEntry}");
        }

        [Theory]
        [InlineData("http://passxyz.com")]
        [InlineData("pxdat://passxyz.com")]
        [InlineData("pxtem://passxyz.com")]
        public void CreatePwEntryFailureTest(string str) 
        {
            PxEntry pxEntry = new PxEntry(str);
            Assert.Empty(pxEntry.Name);
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
