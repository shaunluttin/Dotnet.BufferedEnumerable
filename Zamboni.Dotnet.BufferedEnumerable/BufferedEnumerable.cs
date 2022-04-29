using System;
using System.Collections;
using System.Collections.Generic;

namespace Zamboni.Dotnet.BufferedEnumerable
{
    public class BufferedEnumerable<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> _sequence;

        public BufferedEnumerable(IEnumerable<T> sequence)
        {
            _sequence = sequence;
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach(var item in _sequence)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
