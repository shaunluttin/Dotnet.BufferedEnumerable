using System;
using System.Collections.Generic;
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
        public void Enumeration_Always_YieldsAllItemsInOrder(int itemCount)
        {
            // Arrange
            var sequence = Enumerable.Range(0, itemCount);

            // Act
            var unitUnderTest = new BufferedEnumerable<int>(sequence);
            var results = unitUnderTest.ToList();

            // Assert
            var doAllItemsMatchInOrder = sequence.Zip(results).All(item => item.First == item.Second);
            Assert.True(doAllItemsMatchInOrder);
        }

        [Theory]
        [InlineData(50, 10)] // 500 ms to buffer.
        [InlineData(100, 10)]  // 1000 ms to buffer.
        public void Enumeration_AfterGivenTimeToBufferAll_YieldsAllItemsVeryQuickly(
            int itemCount,
            int latencyPerItemMs
        )
        {
            // 10 ms to process after full buffering.
            const int expectedMaxDurationAfterBufferingMs = 10;

            // Arrange
            var sequence = Enumerable.Range(0, itemCount).Select(item => 
            {
                Thread.Sleep(latencyPerItemMs);
                return item;
            });

            // Act
            var unitUnderTest = new BufferedEnumerable<int>(sequence).StartBuffering();

            // Give time to buffer all the items.
            Thread.Sleep(itemCount * latencyPerItemMs);

            var stopwatch = Stopwatch.StartNew();

            var results = unitUnderTest.ToList();

            stopwatch.Stop();

            // Assert
            Assert.InRange(stopwatch.ElapsedMilliseconds, 0, expectedMaxDurationAfterBufferingMs);
        }

        [Theory]
        [InlineData(500, 10, 50)]
        public void Enumeration_WhenGivenMaxBufferSize_StaysWithinBufferSize(
            int itemCount,
            int latencyPerItemMs,
            int maxBufferSizeInItems
        )
        {
            var itemsInBuffer = new HashSet<int>();

            // Arrange
            var sequence = Enumerable.Range(0, itemCount).Select(item => 
            {
                Console.WriteLine(item);
                itemsInBuffer.Add(item);
                return item;
            });

            // Act
            var unitUnderTest = new BufferedEnumerable<int>(sequence, maxBufferSizeInItems).StartBuffering();

            Thread.Sleep(itemCount * latencyPerItemMs); // give it time to buffer completely.

            Assert.InRange(itemsInBuffer.Count, 0, maxBufferSizeInItems);
        }
    }
}
