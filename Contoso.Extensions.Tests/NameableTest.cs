using System;
using Xunit;

namespace Contoso.Extensions
{
    public class NameableTest
    {
        [Fact]
        public void NameableAssignments()
        {
            // Act
            var value1 = new Nameable<int>(1);
            var value2 = new Nameable<int>(2, "Name");
            var value3 = (Nameable<int>)3;
            var value4 = (Nameable<int>)4;
            var value5 = value4.AsName("Name");

            // Assert
            Assert.Equal(1, actual: value1.Value); Assert.Null(value1.Name);
            Assert.Equal(2, actual: value2.Value); Assert.NotNull(value2.Name);
            Assert.Equal(3, actual: value3.Value); Assert.Null(value3.Name);
            Assert.Equal(4, actual: (int)value4); Assert.Null(value4.Name);
            Assert.Equal(4, actual: (int)value5); Assert.NotNull(value5.Name);
        }

        [Fact]
        public void NameableStatics()
        {
            // Arrange
            var value1 = new Nameable<int>(1);
            var value2 = new Nameable<int>(2, "Name");

            // Act
            var compare1 = Nameable.Compare(value1, value1);
            var compare2 = Nameable.Compare(value1, value2);
            var equals1 = Nameable.Equals(value1, value1);
            var equals2 = Nameable.Equals(value1, value2);
            var type1 = Nameable.GetUnderlyingType(value1.GetType());

            // Assert
            Assert.Equal(0, actual: compare1); Assert.Equal(-1, actual: compare2);
            Assert.True(equals1); Assert.False(equals2);
            Assert.Equal(typeof(int), type1);
        }
    }
}
