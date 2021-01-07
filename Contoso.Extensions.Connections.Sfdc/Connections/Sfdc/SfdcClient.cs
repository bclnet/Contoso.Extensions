using System;
using System.Collections.Generic;

namespace Contoso.Extensions.Connections.Sfdc
{
    /// <summary>
    /// SfdcClient
    /// </summary>
    public class SfdcClient : IDisposable
    {
        public SoapClient Client;
        public SessionHeader Header;

        public void Dispose() { Client?.Close(); Client = null; }

        /// <summary>
        /// Fieldses to null.
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <returns>System.String[].</returns>
        public static string[] FieldsToNull(params object[] fields)
        {
            var list = new List<string>();
            for (var i = 0; i < fields.Length; i += 2)
                if (fields[i + 1] == null)
                    list.Add((string)fields[i]);
            return list.Count == 0 ? null : list.ToArray();
        }
    }
}
