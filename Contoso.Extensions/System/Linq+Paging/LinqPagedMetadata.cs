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
    /// Class LinqPagedMetadata.
    /// </summary>
    /// <typeparam name="TSource">The type of the t source.</typeparam>
    /// <seealso cref="IPagedMetadata" />
    [Serializable]
    public class LinqPagedMetadata<TSource> : IPagedMetadata
    {
        /// <summary>
        /// Gets the total items.
        /// </summary>
        /// <value>The total items.</value>
        public int TotalItems { get; }
        /// <summary>
        /// Gets the pages.
        /// </summary>
        /// <value>The pages.</value>
        public int Pages { get; }
        /// <summary>
        /// Gets the items.
        /// </summary>
        /// <value>The items.</value>
        public int Items { get; }
        /// <summary>
        /// Gets the index.
        /// </summary>
        /// <value>The index.</value>
        public int Index { get; private set; }
        /// <summary>
        /// Gets the criteria.
        /// </summary>
        /// <value>The criteria.</value>
        public LinqPagedCriteria Criteria { get; }
        /// <summary>
        /// Gets a value indicating whether this instance has overflowed show all.
        /// </summary>
        /// <value><c>true</c> if this instance has overflowed show all; otherwise, <c>false</c>.</value>
        public bool HasOverflowedShowAll { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinqPagedMetadata{TSource}"/> class.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="criteria">The criteria.</param>
        /// <exception cref="System.ArgumentNullException">
        /// items
        /// or
        /// criteria
        /// </exception>
        public LinqPagedMetadata(IEnumerable<TSource> items, LinqPagedCriteria criteria)
        {
            Items = (items ?? throw new ArgumentNullException(nameof(items))).Count();
            Criteria = criteria ?? throw new ArgumentNullException(nameof(criteria));
            TotalItems = criteria.TotalItemsAccessor?.Invoke() ?? Items;
            Index = criteria.Index;
            Pages = TotalItems > 0 ? (int)Math.Ceiling(TotalItems / (decimal)Criteria.PageSize) : 1;
            HasOverflowedShowAll = Criteria.ShowAll && Items < TotalItems;
            EnsureVisiblity();
        }

        /// <summary>
        /// Ensures the visiblity.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool EnsureVisiblity()
        {
            if (Index > Pages)
            {
                Index = Pages;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gets a value indicating whether this instance has previous page.
        /// </summary>
        /// <value><c>true</c> if this instance has previous page; otherwise, <c>false</c>.</value>
        public bool HasPrevious => Index > 0;

        /// <summary>
        /// Gets a value indicating whether this instance has next page.
        /// </summary>
        /// <value><c>true</c> if this instance has next page; otherwise, <c>false</c>.</value>
        public bool HasNext => Index < (Pages - 1);

        /// <summary>
        /// Gets a value indicating whether this instance is first page.
        /// </summary>
        /// <value><c>true</c> if this instance is first page; otherwise, <c>false</c>.</value>
        public bool IsFirst => Index <= 0;

        /// <summary>
        /// Gets a value indicating whether this instance is last page.
        /// </summary>
        /// <value><c>true</c> if this instance is last page; otherwise, <c>false</c>.</value>
        public bool IsLast => Index >= (Pages - 1);
    }
}
