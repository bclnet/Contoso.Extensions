using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;

namespace Contoso.Extensions.Connections.Sfdc
{
    /// <summary>
    ///  SfdcExtensions
    /// </summary>
    public static class SfdcExtensions
    {
        /// <summary>
        /// Executes the specified function.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="func">The function.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>System.Object.</returns>
        public static object Execute(this SfdcClient source, Func<SoapClient, SessionHeader, object[], object> func, params object[] args)
        {
            using (var scope = new OperationContextScope(source.Client.InnerChannel))
                return func(source.Client, source.Header, args);
        }

        /// <summary>
        /// Creates the specified objects.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="objects">The objects.</param>
        /// <param name="throwOnError">if set to <c>true</c> [throw on error].</param>
        /// <returns>System.String.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static string Create(this SfdcClient source, sObject[] objects, bool throwOnError = true)
        {
            using (var scope = new OperationContextScope(source.Client.InnerChannel))
            {
                source.Client.create(source.Header, null, null, null, null, null, null, null, null, null, null, null, objects, out var limits, out var rs);
                var errors = rs.Where(x => !x.success).SelectMany(x => x.errors).Select(x => x.message).ToArray();
                if (throwOnError && errors.Length != 0)
                    throw new InvalidOperationException(string.Join(", ", errors));
                return rs.First().id;
            }
        }

        /// <summary>
        /// Updates the specified objects.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="objects">The objects.</param>
        /// <param name="throwOnError">if set to <c>true</c> [throw on error].</param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void Update(this SfdcClient source, sObject[] objects, bool throwOnError = true)
        {
            using (var scope = new OperationContextScope(source.Client.InnerChannel))
            {
                source.Client.update(source.Header, null, null, null, null, null, null, null, null, null, null, null, null, objects, out var limits, out var rs);
                var errors = rs.Where(x => !x.success).SelectMany(x => x.errors).Select(x => x.message).ToArray();
                if (throwOnError && errors.Length != 0)
                    throw new InvalidOperationException(string.Join(", ", errors));
            }
        }

        /// <summary>
        /// Queries the specified query.
        /// </summary>
        /// <typeparam name="TResult">The type of the t result.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="query">The query.</param>
        /// <param name="batchSize">Size of the batch.</param>
        /// <returns>IEnumerable&lt;TResult&gt;.</returns>
        public static IEnumerable<TResult> Query<TResult>(this SfdcClient source, string query, int? batchSize = null)
        {
            var queryOptions = batchSize == null ? null : new QueryOptions { batchSize = batchSize.Value, batchSizeSpecified = true };
            using (var scope = new OperationContextScope(source.Client.InnerChannel))
            {
                source.Client.query(source.Header, queryOptions, null, null, query, out var r);
                if (r.records == null) yield break;
                foreach (var x in r.records.Cast<TResult>()) yield return x;
                while (!r.done)
                {
                    source.Client.queryMore(source.Header, queryOptions, r.queryLocator, out r);
                    if (r.records == null) yield break;
                    foreach (var x in r.records.Cast<TResult>()) yield return x;
                }
            }
        }

        /// <summary>
        /// Deletes the specified ids.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="ids">The ids.</param>
        public static void Delete(this SfdcClient source, string[] ids)
        {
            using (var scope = new OperationContextScope(source.Client.InnerChannel))
                source.Client.delete(source.Header, null, null, null, null, null, null, null, null, null, null, ids, out var limits, out var r);
        }

        /// <summary>
        /// Uploads the file.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="documentId">The document identifier.</param>
        /// <param name="data">The data.</param>
        /// <param name="path">The path.</param>
        /// <param name="title">The title.</param>
        /// <param name="description">The description.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public static string UploadFile(this SfdcClient source, string locationId, string documentId, Stream data, string path, string title = null, string description = null, Action<ContentVersion> action = null)
        {
            using (var s = new MemoryStream())
            {
                data.CopyTo(s);
                return source.UploadFile(locationId, documentId, s.ToArray(), path, title, description, action);
            }
        }

        /// <summary>
        /// Uploads the file.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="documentId">The document identifier.</param>
        /// <param name="data">The data.</param>
        /// <param name="path">The path.</param>
        /// <param name="title">The title.</param>
        /// <param name="description">The description.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public static string UploadFile(this SfdcClient source, string locationId, string documentId, byte[] data, string path, string title = null, string description = null, Action<ContentVersion> action = null)
        {
            var content = new ContentVersion
            {
                FirstPublishLocationId = documentId != null ? null : locationId,
                ContentDocumentId = documentId,
                PathOnClient = path,
                Title = title,
                Description = description,
                VersionData = data
            };
            action?.Invoke(content);
            return source.Create(new[] { content });
        }

        /// <summary>
        /// Downloads the file.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="folder">The folder.</param>
        /// <param name="documentId">The document identifier.</param>
        /// <param name="documentName">Name of the document.</param>
        /// <param name="documentDate">The document date.</param>
        /// <returns></returns>
        public static bool DownloadFile(this SfdcClient source, string folder, string documentId, out string documentName, out DateTime documentDate)
        {
            var d = source.Query<ContentDocument>($"SELECT Title, FileExtension, LatestPublishedVersionId, LastModifiedDate FROM ContentDocument WHERE Id = '{documentId}'").SingleOrDefault();
            if (d == null)
            {
                documentName = default;
                documentDate = default;
                return default;
            }
            var versionId = d.LatestPublishedVersionId;
            documentName = $"{d.Title}.{d.FileExtension}";
            documentDate = d.LastModifiedDate.Value;

            // download
            var path = Path.Combine(folder, documentId, documentName);
            if (!Directory.Exists(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            var address = source.Client.Endpoint.Address.Uri;
            var endpoint = $"{address.Scheme}://{address.Host}";
            using (var wc = new WebClient())
            {
                wc.Headers.Add("Authorization", $"Bearer {source.Header.sessionId}");
                wc.DownloadFile($"{endpoint}/services/data/v39.0/sobjects/ContentVersion/{versionId}/VersionData", path);
            }
            return true;
        }

        /// <summary>
        /// Gets the local file path.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <param name="documentId">The document identifier.</param>
        /// <param name="documentName">Name of the document.</param>
        /// <returns></returns>
        public static string GetLocalFilePath(string folder, string documentId, string documentName) => Path.Combine(folder, documentId, documentName);
    }
}
