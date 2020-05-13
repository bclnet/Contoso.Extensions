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

namespace System.Linq
{
    /// <summary>
    /// LinqPagedCriteria
    /// </summary>
    [Serializable]
    public class LinqPagedCriteria
    {
        /// <summary>
        /// Gets or sets the total items accessor.
        /// </summary>
        /// <value>The total items accessor.</value>
        public Func<int> TotalItemsAccessor { get; set; }
        /// <summary>
        /// Gets or sets the size of the page.
        /// </summary>
        /// <value>The size of the page.</value>
        public int PageSize { get; set; }
        /// <summary>
        /// Gets or sets the index of the page.
        /// </summary>
        /// <value>The index of the page.</value>
        public int Index { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [show all].
        /// </summary>
        /// <value><c>true</c> if [show all]; otherwise, <c>false</c>.</value>
        public bool ShowAll { get; set; }
        /// <summary>
        /// Gets or sets the size of the page set.
        /// </summary>
        /// <value>The size of the page set.</value>
        public int PageSetSize { get; set; }
    }
}
