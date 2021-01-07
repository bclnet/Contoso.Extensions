# Contoso.Extensions.Connection.Sfdc
a connection to Salesforce CX

---

## Generate Connection.cs

First login to SFDC and download the enterprise wsdl, replacing wsdl.xml

Second generate the connection file

Run: Generate

```
Generate
```

This should update the Connection.cs file

## Register with project

```C#
internal class Config : ConfigBase, ISfdcConnectionString
{
    static Config() => Configure();

    string ISfdcConnectionString.this[string name] => Configuration.GetConnectionString("Sfdc");
}
```

```C#
public static void ConfigureServices(IServiceCollection services)
{
    var config = new Config();
    services.AddSfdcContext(config);
}
```

## Using SfdcContext

```C#
public MyMethod(ISfdcContext sfdcCtx)
{
    var s = ...
    using (var sfdc = sfdcCtx.Connect()) {

        // QUERY
        {
            var query = sfdc.Query<Account>("SELECT Id, Name FROM Account");
        }
        
        // ADD
        {
            sfdc.Create(new[]{ new Account
            {
                fieldsToNull = SfdcClient.FieldsToNull(
                    "Name", s.Name),
                Name = s.Name,
            }});
        }

        // UPDATE
        {
            sfdc.Update(new[]{ new Account
            {
                fieldsToNull = SfdcClient.FieldsToNull(
                    "Name", s.Name)
                Id = s.Id,
                Name = s.Name
            }});
        }

        // DELETE
        {
            sfdc.Delete(s.Id);
        }
    }
}
```


