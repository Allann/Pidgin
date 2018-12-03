using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Pidgin
{
    /// <summary>
    /// A mutable struct! Careful!
    /// </summary>
    internal struct PooledList<T>
    {
        private const int InitialCapacity = 16;
        private T[] _items;
        private int _size;

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return _size;
            }
        }

        public PooledList(int initialCapacity)
        {
            _items = ArrayPool<T>.Shared.Rent(initialCapacity);
            _size = 0;
        }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                void ThrowArgumentOutOfRangeException()
                {
                    throw new ArgumentOutOfRangeException("index");
                }
                if (index >= _size)
                {
                    ThrowArgumentOutOfRangeException();
                }
                return _items[index];
            }
        }

        public U Aggregate<U>(U seed, Func<U, T, U> func)
        {
            var z = seed;
            for (var i = 0; i < _size; i++)
            {
                z = func(z, _items[i]);
            }
            return z;
        }
        public U AggregateR<U>(U seed, Func<T, U, U> func)
        {
            var z = seed;
            for (var i = _size - 1; i >= 0; i--)
            {
                z = func(_items[i], z);
            }
            return z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            GrowIfNecessary();
            _items[_size] = item;
            _size++;
        }

        public void Clear()
        {
            ArrayPool<T>.Shared.Return(_items);
            _items = null;
            _size = 0;
        }

        private void GrowIfNecessary()
        {
            if (_items == null)
            {
                _items = ArrayPool<T>.Shared.Rent(InitialCapacity);
            }
            else if (_size == _items.Length)
            {
                var newBuffer = ArrayPool<T>.Shared.Rent(_items.Length * 2);
                Array.Copy(_items, newBuffer, _items.Length);
                ArrayPool<T>.Shared.Return(_items);
                _items = newBuffer;
            }
        }
    }
}