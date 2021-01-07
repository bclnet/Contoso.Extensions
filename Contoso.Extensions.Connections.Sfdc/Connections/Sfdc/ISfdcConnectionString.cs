namespace Contoso.Extensions.Connections.Sfdc
{
    /// <summary>
    /// ISfdcConnectionString
    /// </summary>
    public interface ISfdcConnectionString
    {
        string this[string name] { get; }
    }
}
