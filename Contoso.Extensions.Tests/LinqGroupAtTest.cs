using System.Linq;
using Xunit;

namespace Contoso.Extensions
{
    public class LinqGroupAtTest
    {
        [Fact]
        public void GroupAt()
        {
            var groups = Enumerable.Range(1, 50).GroupAt(10);
            Assert.Equal(10, actual: groups.First().Count());
            Assert.Equal(5, actual: groups.Count());
        }
    }
}
