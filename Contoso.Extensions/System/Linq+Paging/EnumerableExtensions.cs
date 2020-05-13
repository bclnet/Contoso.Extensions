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
using System.Collections.Generic;

namespace System.Linq
{
    /// <summary>
    /// EnumerableExtensions
    /// </summary>
    public static partial class EnumerableExtensions
    {
        /// <summary>
        /// To the paged array.
        /// </summary>
        /// <typeparam name="TSource">The type of the t source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="metadata">The metadata.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <returns>TSource[].</returns>
        public static TSource[] ToPagedArray<TSource>(this IEnumerable<TSource> source, int pageIndex, out IPagedMetadata metadata, int pageSize = 20) => ToPagedArray(source, new LinqPagedCriteria { Index = pageIndex, PageSize = pageSize }, out metadata);
        /// <summary>
        /// To the paged array.
        /// </summary>
        /// <typeparam name="TSource">The type of the t source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="criteria">The criteria.</param>
        /// <param name="metadata">The metadata.</param>
        /// <returns>TSource[].</returns>
        /// <exception cref="System.ArgumentNullException">
        /// source
        /// or
        /// criteria
        /// </exception>
        public static TSource[] ToPagedArray<TSource>(this IEnumerable<TSource> source, LinqPagedCriteria criteria, out IPagedMetadata metadata)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (criteria == null)
                throw new ArgumentNullException(nameof(criteria));
            metadata = LinqPagedMetadataProviders.Current.GetMetadata(source, criteria);
            var pageSize = criteria.PageSize;
            var index = metadata.Index;
            return metadata.TotalItems > 0
                ? new Buffer<TSource>(index == 0 ? source.Take(pageSize) : source.Skip(index * pageSize).Take(pageSize)).ToArray()
                : new TSource[] { };
        }

        /// <summary>
        /// To the paged list.
        /// </summary>
        /// <typeparam name="TSource">The type of the t source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <returns>IPagedList&lt;TSource&gt;.</returns>
        public static IPagedList<TSource> ToPagedList<TSource>(this IEnumerable<TSource> source, int pageIndex, int pageSize = 20) => ToPagedList(source, new LinqPagedCriteria { Index = pageIndex, PageSize = pageSize });
        /// <summary>
        /// To the paged list.
        /// </summary>
        /// <typeparam name="TSource">The type of the t source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="criteria">The criteria.</param>
        /// <returns>IPagedList&lt;TSource&gt;.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// source
        /// or
        /// criteria
        /// </exception>
        public static IPagedList<TSource> ToPagedList<TSource>(this IEnumerable<TSource> source, LinqPagedCriteria criteria)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (criteria == null)
                throw new ArgumentNullException("criteria");
            var metadata = LinqPagedMetadataProviders.Current.GetMetadata(source, criteria);
            var pageSize = criteria.PageSize;
            var index = metadata.Index;
            return metadata.TotalItems > 0
                ? new PagedList<TSource>(index == 0 ? source.Take(pageSize) : source.Skip(index * pageSize).Take(pageSize), metadata)
                : new PagedList<TSource>(metadata);
        }
        /// <summary>
        /// To the paged list.
        /// </summary>
        /// <typeparam name="TSource">The type of the t source.</typeparam>
        /// <typeparam name="TPagedSource">The type of the t paged source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="metadata">The metadata.</param>
        /// <returns>IPagedList&lt;TSource&gt;.</returns>
        /// <exception cref="System.ArgumentNullException">metadata - Not from PagedList<TPagedSource></exception>
        public static IPagedList<TSource> ToPagedList<TSource, TPagedSource>(this IEnumerable<TSource> source, IEnumerable<TPagedSource> metadata)
        {
            if (!(metadata is PagedList<TPagedSource> pagedList))
                throw new ArgumentNullException(nameof(metadata), "Not from PagedList<TPagedSource>");
            return new PagedList<TSource>(source, pagedList._metadata);
        }
    }
}
