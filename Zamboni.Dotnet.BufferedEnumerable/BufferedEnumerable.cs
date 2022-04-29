using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zamboni.Dotnet.BufferedEnumerable
{
    public class BufferedEnumerable<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> _sequence;

        private readonly ConcurrentQueue<T> _buffer = new ConcurrentQueue<T>();

        public BufferedEnumerable(IEnumerable<T> sequence)
        {
            _sequence = sequence;
        }

        public IEnumerator<T> GetEnumerator()
        {
            while(_buffer.TryDequeue(out var item))
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public BufferedEnumerable<T> StaffBuffering()
        {
            Task.Run(() =>
            {
                foreach(var item in _sequence)
                {
                    _buffer.Enqueue(item);
                }
            });

            return this;
        }
    }
}
