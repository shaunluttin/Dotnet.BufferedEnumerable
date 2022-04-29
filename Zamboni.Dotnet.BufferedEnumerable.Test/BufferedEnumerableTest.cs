using System;
using System.Linq;
using Xunit;

namespace Zamboni.Dotnet.BufferedEnumerable.Test
{
    public class BufferedEnumerableTest
    {
        [Fact]
        public void EnumeratesAllItems()
        {
            // Arrange
            var sequence = Enumerable.Range(0, 100);

            // Act
            var unitUnderTest = new BufferedEnumerable<int>(sequence);
            var results = unitUnderTest.ToList();

            // Assert
            var doAllItemsMatchInOrder = sequence.Zip(results).All(item => item.First == item.Second);
            Assert.True(doAllItemsMatchInOrder);
        }
    }
}
