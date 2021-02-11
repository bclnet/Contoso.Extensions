# Contoso.Extensions.Ssh
An extension to [`SSH.NET`](https://nuget.org/packages/SSH.NET) for connecting via a connection string.

This library depends on [`Contoso.Extensions.Data`](https://nuget.org/packages/Contoso.Extensions.Data) pre-configured for use with `ConfigBase`.

# ConnectionString
*Parameters in the connection string*

Name       | Description
---        | ---
Server     | The server location
Credential | The credential to lookup.
User Id / Uid | The user id instead of looking up credential
Password / Pwd | The password instead of looking up credential
FilePath (optional) | The folder for the ssh key. (defaults to ~/.ssh/id_rsa)

### Example
```Json
{
  "ConnectionStrings": {
    "Store": "Server=myserver;Uid=myuser;Pwd=mypassword"
  }
}
```
```C#
static readonly ISshService _ssh = new SshService();

using (var ssh = _ssh.GetConnection("Store")) {
	ssh.Connect();
	ssh.Upload(new FileInfo("MyPath"), "RemotePath");
}
```