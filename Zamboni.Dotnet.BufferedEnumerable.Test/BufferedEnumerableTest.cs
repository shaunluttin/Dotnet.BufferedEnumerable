using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Xunit;

namespace Zamboni.Dotnet.BufferedEnumerable.Test
{
    public class BufferedEnumerableTest
    {
        [Theory]
        [InlineData(100)]
        public void Enumeration_Always_YieldsAllItemsInOrder(int sequenceLength)
        {
            // Arrange
            var sequence = Enumerable.Range(0, sequenceLength);

            // Act
            var unitUnderTest = new BufferedEnumerable<int>(sequence);
            var results = unitUnderTest.ToList();

            // Assert
            var doAllItemsMatchInOrder = sequence.Zip(results).All(item => item.First == item.Second);
            Assert.True(doAllItemsMatchInOrder);
        }

        [Theory]
        [InlineData(500, 10, 10)] // 5,000 ms to buffer.
        [InlineData(1000, 10, 10)] // 10,000 ms to buffer.
        public void Enumeration_AfterGivenTimeToBufferAll_YieldsAllItemsVeryQuickly(
            int sequenceLength,
            int latencyPerItemMs,
            int expectedMaxDurationAfterBufferingMs
        )
        {
            // Arrange
            var sequence = Enumerable.Range(0, sequenceLength).Select(item => 
            {
                Thread.Sleep(latencyPerItemMs);
                return item;
            });

            // Act
            var unitUnderTest = new BufferedEnumerable<int>(sequence).StaffBuffering();

            // Give time to buffer all the items.
            Thread.Sleep(sequenceLength * latencyPerItemMs);

            var stopwatch = Stopwatch.StartNew();

            var results = unitUnderTest.ToList();

            stopwatch.Stop();

            // Assert
            Assert.InRange(stopwatch.ElapsedMilliseconds, 0, expectedMaxDurationAfterBufferingMs);
        }
    }
}
