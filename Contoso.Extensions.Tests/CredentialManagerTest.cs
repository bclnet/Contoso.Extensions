using System;
using System.Security;
using Xunit;

namespace Contoso.Extensions
{
    public class CredentialManagerTest
    {
        [Fact(Skip = "Requires CredentialManager to be set")]
        public void TryRead()
        {
            if (CredentialManager.TryRead("MyCredential", CredentialManager.CredentialType.GENERIC, out var cred) != 0)
                throw new InvalidOperationException("Unable to read credential store");
            Assert.Equal("UserName", actual: cred.UserName);
        }
    }
}
