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
        public void Enumeration_AfterGivenTimeToBufferCompletely_YieldsAllItemsVeryQuickly(
            int itemCount,
            int latencyPerItemMs
        )
        {
            // 10 ms to process after full buffering.
            const int expectedMaxDurationAfterBufferingMs = 10;

            // Arrange
            var sequence = SequenceWithLatencyPerItem(itemCount, latencyPerItemMs);

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
        public void StartBuffering_WhenGivenTimeToBufferCompletely_ReachesMaxBufferSize(
            int itemCount,
            int latencyPerItemMs,
            int maxBufferSizeInItems
        )
        {
            var itemsInBuffer = new HashSet<int>();

            // Arrange
            var sequence = SequenceWithLatencyPerItem(itemCount, latencyPerItemMs).Select(item => 
            {
                itemsInBuffer.Add(item);
                return item;
            });

            // Act
            var unitUnderTest = new BufferedEnumerable<int>(sequence, maxBufferSizeInItems).StartBuffering();
            Thread.Sleep(itemCount * latencyPerItemMs); // Give it time to buffer completely.

            // Assert
            Assert.Equal(itemsInBuffer.Count, maxBufferSizeInItems);
        }

        [Theory]
        [InlineData(500, 10, 50)]
        public void StartBuffering_WhenGivenTimeToBufferCompletely_StaysWithinMaBufferSize(
            int itemCount,
            int latencyPerItemMs,
            int maxBufferSizeInItems
        )
        {
            var itemsInBuffer = new HashSet<int>();

            // Arrange
            var sequence = SequenceWithLatencyPerItem(itemCount, latencyPerItemMs).Select(item => 
            {
                itemsInBuffer.Add(item);
                return item;
            });

            // Act
            var unitUnderTest = new BufferedEnumerable<int>(sequence, maxBufferSizeInItems).StartBuffering();
            Thread.Sleep(itemCount * latencyPerItemMs); // Give it time to buffer completely.

            // Assert
            Assert.InRange(itemsInBuffer.Count, 0, maxBufferSizeInItems);
        }

        [Theory]
        [InlineData(500, 10, 50)]
        [InlineData(500, 10, 25)]
        public void ForEach_WhenGivenTimeToBufferOnEach_BuffersExpectedItemCount(
            int itemCount,
            int latencyPerItemMs,
            int itemsToBufferEachIteration
        )
        {
            var itemsInBuffer = new HashSet<int>();

            // Arrange
            var sequence = SequenceWithLatencyPerItem(itemCount, latencyPerItemMs).Select(item => 
            {
                itemsInBuffer.Add(item);
                return item;
            });

            // Act
            var unitUnderTest = new BufferedEnumerable<int>(sequence).StartBuffering();

            // Assert
            foreach (var item in unitUnderTest) 
            {
                itemsInBuffer.Remove(item);

                Thread.Sleep(itemsToBufferEachIteration * latencyPerItemMs); // Give it time to buffer (n) items.

                Assert.Equal(itemsInBuffer.Count, itemsToBufferEachIteration);
            }
        }

        private IEnumerable<int> SequenceWithLatencyPerItem(int itemCount, int itemLatencyMilliseconds)
        {
            for (var i = 0; i < itemCount; ++i)
            {
                Thread.Sleep(itemLatencyMilliseconds);
                yield return i;
            }
        }
    }
}
