using System.Linq;
using Xunit;

namespace Contoso.Extensions
{
    public class LinqPagingTest
    {
        [Fact]
        public void ToPagedList()
        {
            var list = Enumerable.Range(1, 50).ToPagedList(0);
            Assert.Equal(50, actual: list.TotalItems);
            Assert.Equal(50, actual: list.Items);
            Assert.Equal(20, actual: list.Count);
        }

        [Fact]
        public void ToPagedArray()
        {
            var array = Enumerable.Range(1, 50).ToPagedArray(0, out var metadata);
            Assert.Equal(50, actual: metadata.TotalItems);
            Assert.Equal(50, actual: metadata.Items);
            Assert.Equal(20, actual: array.Length);
        }
    }
}
