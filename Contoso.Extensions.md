# Contoso.Extensions
extensions to the System namespace


# Nameable (using System)
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


# Grammar (using System.Globalization)
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


# Linq GroupAt (using System.Linq)
*Groups at interval.*

### Example
```C#
foreach (var batch in Enumerable.Range(1, 100).GroupAt(10))
	batch.Dump();
```


# Linq Paging (using System.Linq)
*creates pagable set at interval.*

### IPagedList
*Interface and concrete returned from ToPagedList()*

```C#
public interface IPagedList<T> : IList<T>, IPagedMetadata {}

public class PagedList<T> : List<T>, IPagedList<T> {}
```

Method      | Type | Description
---         | ---  | ---
Set | IEnumerable<T> | Gets the set.
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


# MultipartRequestHelper (using System.Net)
*MultipartRequestHelper*


# CredentialManager (using System.Security)
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


# ParsedConnectionString (using System.Security)
*ParsedConnectionString*

Method      | Type | Description
---         | ---  | ---
Server      | string | Gets the server.
Credential  | NetworkCredential | Gets the credential.
Params | Dictionary<string, string> | Gets the parameters.

### Example
```C#
var connString = new ParsedConnectionString("server=Database;UserId=User;Pwd=Password");
Console.WriteLine($"{connString.Server}, {connString.Credential.UserName}");

var connString = new ParsedConnectionString("server=Database;Credential=LookupName");
Console.WriteLine($"{connString.Server}, {connString.Credential.UserName}");
```

