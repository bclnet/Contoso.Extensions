using Microsoft.Net.Http.Headers;
using System.IO;

namespace System.Net
{
    /// <summary>
    /// Class MultipartRequestHelper.
    /// </summary>
    public static class MultipartRequestHelper
    {
        // Content-Type: multipart/form-data; boundary="----WebKitFormBoundarymx2fSWqWSd0OxQqq"
        // The spec says 70 characters is a reasonable limit.
        public static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
        {
            var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary);
            if (!boundary.HasValue)
                throw new InvalidDataException("Missing content-type boundary.");
            if (boundary.Length > lengthLimit)
                throw new InvalidDataException($"Multipart boundary length limit {lengthLimit} exceeded.");
            return boundary.Value;
        }

        public static bool IsMultipartContentType(string contentType) => !string.IsNullOrEmpty(contentType) && contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0;

        // Content-Disposition: form-data; name="key";
        public static bool HasFormDataContentDisposition(ContentDispositionHeaderValue contentDisposition) => contentDisposition != null && contentDisposition.DispositionType.Equals("form-data") && string.IsNullOrEmpty(contentDisposition.FileName.Value) && string.IsNullOrEmpty(contentDisposition.FileNameStar.Value);

        // Content-Disposition: form-data; name="myfile1"; filename="Misc 002.jpg"
        public static bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition) => contentDisposition != null && contentDisposition.DispositionType.Equals("form-data") && (!string.IsNullOrEmpty(contentDisposition.FileName.Value) || !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));
    }
}
