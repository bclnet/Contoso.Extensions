# Contoso.Extensions.Connection.Crm
a connection to Microsoft CRM

---

## Generate Connection.cs

First generate the connection file

Run: Generate Domain, Username, Password

```
Generate my.domain.com MyUsername, MyPassword
```

This should update the Connection.cs file

## Register with project

```C#
internal class Config : ConfigBase, ICrmConnectionString
{
    static Config() => Configure();

    string ICrmConnectionString.this[string name] => Configuration.GetConnectionString("Crm");
}
```

```C#
public static void ConfigureServices(IServiceCollection services)
{
    var config = new Config();
    services.AddCrmContext(config);
}
```

## Using CrmContext

```C#
public MyMethod(ICrmContext crmCtx)
{
    var s = ...
    using (var crm = crmCtx.Connect()) {

        // QUERY
        {
            var query = crm.AccountSet.Select(x => x).ToList();
        }

        // ADD
        {
            crm.AddObject(new Account
            {
                Name = s.Name
            });
            crm.SaveChanges();
        }

        // UPDATE
        {
            var item = crm.Account.SingleOrDefault(x => x.Id == s.Id);
            if (item == null)
                return;
            item.Account = s.Name;
            crm.UpdateObject(item);
            crm.SaveChanges();
        }

        // DELETE
        {
            var item = crm.Account.SingleOrDefault(x => x.Id == s.Id);
            if (item == null)
                return;
            crm.DeleteObject(item);
            crm.SaveChanges();
        }
    }
}
```