# Contoso.Extensions
extensions to the System namespace

# Linq Extensions

The following are class extensions to the `System.Linq` namespace:

* `source.GroupAt(size)` groups the `source` at every `size` items.
* `source.ToPagedList(criteria)` and `source.ToPagedArray(criteria, out metadata)` paginate the `source` by `criteria`.

## Linq - GroupAt (using System.Linq)
*Groups at interval.*

### Example
```C#
foreach (var batch in Enumerable.Range(1, 100).GroupAt(10))
	batch.Dump();
```

## Linq - Paging (using System.Linq)
*creates pagable set at interval.*

### IPagedList
*Interface and concrete returned from ToPagedList()*

```C#
public interface IPagedList<T> : IList<T>, IPagedMetadata {}

public class PagedList<T> : List<T>, IPagedList<T> {}
```

Method      | Type | Description
---         | ---  | ---
Set         | IEnumerable<T> | Gets the set.
Pages       | int  | Gets the pages.
TotalItems  | int  | Gets the total items.
Items       | int  | Gets the items.
Index       | int  | Gets the index.
HasPrevious | bool | Gets a value indicating whether this instance can previous.
HasNext     | bool | Gets a value indicating whether this instance can next.
IsFirst     | bool | Gets a value indicating whether this instance is first.
IsLast      | bool | Gets a value indicating whether this instance is last.

### IPagedMetadata
*IPagedMetadata*

Method      | Type | Description
---         | ---  | ---
Pages       | int  | Gets the pages.
TotalItems  | int  | Gets the total items.
Items       | int  | Gets the items.
Index       | int  | Gets the index.
HasPrevious | bool | Gets a value indicating whether this instance can previous.
HasNext     | bool | Gets a value indicating whether this instance can next.
IsFirst     | bool | Gets a value indicating whether this instance is first.
IsLast      | bool | Gets a value indicating whether this instance is last.

### LinqPagedCriteria
*LinqPagedCriteria*

Method      | Type | Description
---         | ---  | ---
TotalItemsAccessor | Func<int> | Gets or sets the total items accessor.
PageSize    | int  | Gets or sets the size of the page.
Index       | int  | Gets or sets the index of the page.
ShowAll     | bool | Gets or sets a value indicating whether [show all].
PageSetSize | int  | Gets or sets the size of the page set.

### EnumerableExtensions
*Enumerable extensions*

```C#
.ToPagedArray(int pageIndex, out IPagedMetadata metadata, int pageSize = 20) -> TSource[]
.ToPagedArray(LinqPagedCriteria criteria, out IPagedMetadata metadata) -> TSource[]

.ToPagedList(int pageIndex, int pageSize = 20) -> IPagedList<TSource>
.ToPagedList(LinqPagedCriteria criteria) -> IPagedList<TSource>
```

### QueryableExtensions
*Queryable extensions*

```C#
.ToPagedArray(int pageIndex, out IPagedMetadata metadata, int pageSize = 20) -> TSource[]
.ToPagedArray(LinqPagedCriteria criteria, out IPagedMetadata metadata) -> TSource[]

.ToPagedList(int pageIndex, int pageSize = 20) -> IPagedList<TSource>
.ToPagedList(LinqPagedCriteria criteria) -> IPagedList<TSource>
```

### Example
```C#
foreach (var batch in Enumerable.Range(1, 100).ToPagedList(0))
	batch.Dump();
```



# Security

Credentials contains sensitive information and should never be commited to source control. Connection strings may contain such credentials.

Some common solutions are:

* Encrypt part or all of the configuration file, and store the encrypted version.
* Use Environment Variables to store the sensitive information (default).
* Use an external file or keystore, outside of source control, and retrieve values at runtime.

On Microsoft Windows, `Credential Manager` is a built-in keystore which stores credentals per user. You can find it in your control panel, or by using the following command:
```
rundll32.exe keymgr.dll, KRShowKeyMgr
```

The following are security classes:

* `CredentialManager` read, write, or deletes these values.
* `ParsedConnectionString` parses a connection string, and can lookup credentials in credental manager.


## CredentialManager (using System.Security)
*CredentialManager*

Method      | Type | Description
---         | ---  | ---
Delete      | int  | Deletes the specified target.
Query       | int  | Queries the specified filter.
TryRead     | int  | Tries to read the specified target.
Write       | int  | Writes the specified user credential.
ReadGeneric | NetworkCredential | Reads a generic credential (default).

### Example
```C#
if (CredentialManager.TryRead("MyCredential", CredentialManager.CredentialType.GENERIC, out var cred) != 0)
    throw new InvalidOperationException("Unable to read credential store");
Console.WriteLine(cred.UserName);
```


## ParsedConnectionString (using System.Security)
*ParsedConnectionString*

Method      | Type | Description
---         | ---  | ---
Server      | string | Gets the server.
Credential  | NetworkCredential | Gets the credential.
Params      | Dictionary<string, string> | Gets the parameters.

### Example
```C#
var connString = new ParsedConnectionString("Server=Database;User Id=User;Password=Password");
Console.WriteLine($"{connString.Server}, {connString.Credential.UserName}");

var connString = new ParsedConnectionString("Data Source=Database;Uid=User;Pwd=Password;Extra=Anything");
Console.WriteLine($"{connString.Server}, {connString.Credential.UserName}");

var connString = new ParsedConnectionString("Server=Database;Credential=LookupName");
Console.WriteLine($"{connString.Server}, {connString.Credential.UserName}");
```



# General

The following are general classes:

* `AppContextSwitch` internal AppContext switches.
* `Grammar` provides basic grammatological methods.
* `Nameable` represents a type that can be assigned a name.

## AppContextSwitch (using System)
*Toggles internal AppContext switches.*

Method      | Description
---         | ---
EnableUnsafeBinaryFormatterSerialization | Gets or sets a value indicating whether to enable unsafe binary formatter serialization.

### Example
```C#
AppContextSwitch.EnableUnsafeBinaryFormatterSerialization = true;
```


## Grammar (using System.Globalization)
*Provides basic grammatological methods.*

Method      | Description
---         | ---
Vowels      | The vowels
WasWere     | Adds was/were based on `number` to string `stringToAppend`
Pluralize   | Makes a string plural by adding "s" to it if `number` is greater than one
Possesive   | Makes a string possesive by adding "'s" to it if the string does not end with "s"
HeShe       | Returns he/she or they based on `gender`
HeSheHas    | Returns he/she or they have based on `gender`
HimHer      | Returns him/her or them based on `gender`
HisHers     | Returns his/hers or theirs based on `gender`
PluralizePhrase | Returns the number and makes string `stringToAppend` based on `number`
PluralizePhraseWithArticles | Converts the `stringToAppend` to plural
Nth         | Returns to its nth form based on `number`

### Example
```C#
var gender = "Male";
Console.WriteLine($"{Grammar.HeShe(gender)} has won {Grammar.HisHers(gender)} {Grammar.Nth(2)} game.");

// he has won his 2nd game.
```

## Nameable (using System)
*Represents a type that can be assigned a name.*

### Example
```C#
class Sample
{
	public Nameable<int> UserOne { get; set; }
	public Nameable<int> UserTwo { get; set; }
}

var sample = new Sample
{
    UserOne = 123,
    UserTwo = new Nameable<int>(456, "John Doe")
};
```



# Other

The following are unclassified classes:

* `MultipartRequestHelper` used for multipart processing.

## MultipartRequestHelper (using System.Net)
*MultipartRequestHelper*

