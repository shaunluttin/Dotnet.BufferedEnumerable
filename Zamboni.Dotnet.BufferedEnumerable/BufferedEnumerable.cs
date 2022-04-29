using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Zamboni.Dotnet.BufferedEnumerable
{
    public class BufferedEnumerable<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> _sequence;
        private readonly int _maxBufferSizeInItems;
        private readonly ConcurrentQueue<T> _buffer = new ConcurrentQueue<T>();

        private TaskCompletionSource _delayUntilRoomInBuffer = new TaskCompletionSource();

        public BufferedEnumerable(IEnumerable<T> sequence, int maxBufferSizeInItems = -1)
        {
            _sequence = sequence;
            _maxBufferSizeInItems = maxBufferSizeInItems;
        }

        public IEnumerator<T> GetEnumerator()
        {
            while(_buffer.TryDequeue(out var item))
            {
                yield return item;

                // After we dequeue an item,
                // by definition there must be room in the buffer.
                _delayUntilRoomInBuffer.TrySetResult();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public BufferedEnumerable<T> StartBuffering()
        {
            Task.Run(async () =>
            {
                foreach(var item in _sequence)
                {
                    _buffer.Enqueue(item);

                    if (_buffer.Count >= _maxBufferSizeInItems)
                    {
                        await _delayUntilRoomInBuffer.Task;
                    }
                }
            });

            return this;
        }
    }
}
