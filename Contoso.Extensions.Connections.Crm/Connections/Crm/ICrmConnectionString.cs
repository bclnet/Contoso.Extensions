namespace CRM
{
    /// <summary>
    /// ICrmConnectionString
    /// </summary>
    public interface ICrmConnectionString
    {
        string this[string name] { get; }
    }
}
