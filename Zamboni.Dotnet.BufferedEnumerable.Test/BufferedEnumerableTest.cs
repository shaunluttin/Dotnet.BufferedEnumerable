using System.Linq;
using Xunit;

namespace Zamboni.Dotnet.BufferedEnumerable.Test
{
    public class BufferedEnumerableTest
    {
        [Fact]
        public void Enumeration_Always_YieldsAllItemsInOrder()
        {
            // Arrange
            int sequenceLength = 100;
            var sequence = Enumerable.Range(0, sequenceLength);

            // Act
            var unitUnderTest = new BufferedEnumerable<int>(sequence);
            var results = unitUnderTest.ToList();

            // Assert
            var doAllItemsMatchInOrder = sequence.Zip(results).All(item => item.First == item.Second);
            Assert.True(doAllItemsMatchInOrder);
        }

        [Fact]
        public void Enumeration_WhenGivenTimeToBuffer_YieldsBufferedItemsImmediately()
        {
        }
    }
}
