using System.Security;
using Xunit;

namespace Contoso.Extensions
{
    public class ParsedConnectionStringTest
    {
        [Fact]
        public void Parse()
        {
            var connString = new ParsedConnectionString("Server=Database;Uid=User;Pwd=Password;Extra=Anything");
            Assert.Equal("Database", actual: connString.Server);
            Assert.Equal("User", actual: connString.Credential.UserName);
            Assert.Equal("Password", actual: connString.Credential.Password);
            Assert.Equal("Anything", actual: connString.Params["extra"]);
        }

        [Fact]
        public void ParseAlternative()
        {
            var connString = new ParsedConnectionString("Data Source=Database;User Id=User;Password=Password;Extra=Anything");
            Assert.Equal("Database", actual: connString.Server);
            Assert.Equal("User", actual: connString.Credential.UserName);
            Assert.Equal("Password", actual: connString.Credential.Password);
            Assert.Equal("Anything", actual: connString.Params["extra"]);
        }

        [Fact(Skip = "Requires CredentialManager to be set")]
        public void ParseWithCredential()
        {
            var connString = new ParsedConnectionString("Server=Database;Credential=LookupName");
            Assert.Equal("Database", actual: connString.Server);
            Assert.Equal("User", actual: connString.Credential.UserName);
            Assert.Equal("Password", actual: connString.Credential.Password);
        }
    }
}
