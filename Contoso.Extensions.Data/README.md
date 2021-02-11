# Contoso.Extensions.Data
Configuration and data services


# Configuration

.NET Core brought changes to configuration files utilizating `appsettings.json` instead of `web.config`.

`Contoso.Extensions.Data` includes dependecies on the newer `Microsoft.Extensions.Configuration` NuGet packages.

`ConfigBase.Configuration` becomes a singleton property to access the current configuration.

Set `ConfigBase.Configuration` directly, or use the `ConfigBase.Configure()` polyfill to set the `ConfigBase.Configuration` property in .Net Framework projects.


## ConfigBase
*Abstract config base class.*

A project `Config` class should implement `ConfigBase` and either set the `Configuration` property or invoke the `Configure()` method.

Method      | Type | Description
---         | ---  | ---
Configuration | IConfiguration | Gets or sets the configuration.
Configure   | void | Sets the configuration using a polyfill.

### Example for .Net Core 
```C#
internal class Config : ConfigBase
{
    public static string MyConnectionString => Configuration.GetConnectionString("MyConn");
    public static string MyValue => Configuration.GetValue<string>("MyValue");
}

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Config.Configuration = configuration;
        ...
    }
    ...
}
```

### Example for .Net Framework
```C#
internal class Config : ConfigBase
{
    static Config() => Configure();
    public static string MyConnectionString => Configuration.GetConnectionString("MyConn");
    public static string MyValue => Configuration.GetValue<string>("MyValue");
}
```



# Data
The following are data classes:

* `EmailConnection` - Email connection based on connection string.

## EmailConnection
*Email connection based on connection string.*

Method          | Type   | Description             | Parameter
---             | ---    | ---                     | ---
FromEmail       | string | Gets the from email.    | FromEmail
ToEmail         | string | Get the to email.       | ToEmail
Subject         | string | Tries to read the specified target. | Subject
Server          | string | Gets the server. | Server, Data Source
PickupDirectory | string | Gets the pickup directory. | Pickup Directory
Credential      | NetworkCredential | Gets the credential. | Credential, User Id, Password, Uid, Pwd
BuildMessageForException | string | Builds the message for an exception.
SendEmail       | void   | Sends the email. | Host, Ssl, UseDefaultCredentials


### Example

```
Server=mail.server.com;ToEmail=x@x.com;FromEmail=info@x.com;Subject=Alpha:{0}
```

```C#
var email = EmailConnection(Config.MyConnectionString);
email.SendEmail("Subject", "Message");
```


# Services
The following are data services:

* `DbService` - Database connection which checks for azure and generates token when needed. 
* `EmailService` - Email connection that looks up connection string. 

## DbService
*Database connection which checks for azure and generates token when needed.*

Method              | Type   | Description                  | Value
---                 | ---    | ---                          | ---
CommandTimeout      | int    | Default command timeout.     | 60
LongCommandTimeout  | int    | A long command timeout.      | 360
VeryLongCommandTimeout | int | A very long command timeout. | 3600
GetConnection       | IDbConnection | Gets the connection, adding an Access Token if DataSource is Azure.
GetConnectionString | string | Gets the connection string, adding an Access Token if DataSource is Azure.

### Example
```C#
static readonly IDbService _dbService = new DbService();

using (var db = _dbService.GetConnection())
    db.Execute("SQL");
```


## EmailService
*Email connection that looks up connection string.*

Method              | Type   | Description                  | Value
---                 | ---    | ---                          | ---
GetConnection       | IEmailConnection | Gets the connection.

### Example
```C#
static readonly IEmailService _emailService = new EmailService();

_emailService.GetConnection()
    .SendEmail("Subject", "Message");
```
