#region License
/*
The MIT License

Copyright (c) 2008 Sky Morey

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
#endregion

using Newtonsoft.Json;

namespace System.Collections.Generic
{
    /// <summary>
    /// Interface IPagedList
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="System.Collections.Generic.IList{T}" />
    /// <seealso cref="System.Collections.Generic.IPagedMetadata" />
    public interface IPagedList<T> : IList<T>, IPagedMetadata { }

    /// <summary>
    /// Class PagedList.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="System.Collections.Generic.List{T}" />
    /// <seealso cref="System.Collections.Generic.IPagedList{T}" />
    [Serializable, JsonObject(MemberSerialization.OptIn)]
    public class PagedList<T> : List<T>, IPagedList<T>
    {
        internal readonly IPagedMetadata _metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="PagedList{T}"/> class.
        /// </summary>
        public PagedList() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="PagedList{T}"/> class.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        public PagedList(IPagedMetadata metadata) => _metadata = metadata;
        /// <summary>
        /// Initializes a new instance of the <see cref="PagedList{T}"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="metadata">The metadata.</param>
        public PagedList(IEnumerable<T> collection, IPagedMetadata metadata) : base(collection) => _metadata = metadata;

        /// <summary>
        /// Gets the set.
        /// </summary>
        /// <value>The set.</value>
        [JsonProperty] IEnumerable<T> Set => ToArray();

        /// <summary>
        /// Gets the pages.
        /// </summary>
        /// <value>The pages.</value>
        [JsonProperty] public int Pages => _metadata.Pages;

        /// <summary>
        /// Gets the total items.
        /// </summary>
        /// <value>The total items.</value>
        [JsonProperty] public int TotalItems => _metadata.TotalItems;

        /// <summary>
        /// Gets the items.
        /// </summary>
        /// <value>The items.</value>
        [JsonProperty] public int Items => _metadata.Items;

        /// <summary>
        /// Gets the index.
        /// </summary>
        /// <value>The index.</value>
        [JsonProperty] public int Index => _metadata.Index;

        /// <summary>
        /// Gets a value indicating whether this instance can previous.
        /// </summary>
        /// <value><c>true</c> if this instance can previous; otherwise, <c>false</c>.</value>
        [JsonProperty] public bool HasPrevious => _metadata.HasPrevious;

        /// <summary>
        /// Gets a value indicating whether this instance can next.
        /// </summary>
        /// <value><c>true</c> if this instance can next; otherwise, <c>false</c>.</value>
        [JsonProperty] public bool HasNext => _metadata.HasNext;

        /// <summary>
        /// Gets a value indicating whether this instance is first.
        /// </summary>
        /// <value><c>true</c> if this instance is first; otherwise, <c>false</c>.</value>
        [JsonProperty] public bool IsFirst => _metadata.IsFirst;

        /// <summary>
        /// Gets a value indicating whether this instance is last.
        /// </summary>
        /// <value><c>true</c> if this instance is last; otherwise, <c>false</c>.</value>
        [JsonProperty] public bool IsLast => _metadata.IsLast;
    }
}