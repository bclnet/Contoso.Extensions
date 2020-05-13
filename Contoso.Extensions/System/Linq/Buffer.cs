#region Foreign-License
// From microsoft
#endregion
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace System.Linq
{
    /// <summary>
    /// Buffer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct Buffer<TElement>
    {
        internal TElement[] _items;
        internal int _count;

        internal Buffer(IEnumerable<TElement> source)
        {
            TElement[] array = null;
            var length = 0;
            if (source is ICollection<TElement> sourceAsCollection)
            {
                length = sourceAsCollection.Count;
                if (length > 0)
                {
                    array = new TElement[length];
                    sourceAsCollection.CopyTo(array, 0);
                }
            }
            else
                foreach (TElement local in source)
                {
                    if (array == null)
                        array = new TElement[4];
                    else if (array.Length == length)
                    {
                        var destinationArray = new TElement[length * 2];
                        Array.Copy(array, 0, destinationArray, 0, length);
                        array = destinationArray;
                    }
                    array[length] = local;
                    length++;
                }
            _items = array;
            _count = length;
        }

        internal TElement[] ToArray()
        {
            if (_count == 0)
                return new TElement[0];
            if (_items.Length == _count)
                return _items;
            var destinationArray = new TElement[_count];
            Array.Copy(_items, 0, destinationArray, 0, _count);
            return destinationArray;
        }
    }
}
