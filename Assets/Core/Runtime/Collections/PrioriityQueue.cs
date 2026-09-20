using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CrystalEngine.Collections
{
    public class PriorityQueue<T> : IEnumerable<T>, IReadOnlyCollection<T>, ICollection
    {
        private Tuple<T, float>[] _array;
        private int _head;
        private int _tail;
        private int _size;
        private int _version;

        [NonSerialized]
        private object _syncRoot;
        private const int _MinimumGrow = 4;
        private const float _ShrinkThreshold = 0.9f;
        private const long _GrowFactor = 200L;
        private const int _DefaultCapacity = 4;
        private static Tuple<T, float>[] _emptyArray = new Tuple<T, float>[0];
        public int Count
        {
            get
            {
                return _size;
            }
        }

        public PriorityQueue()
        {
            _array = _emptyArray;
        }

        public PriorityQueue(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            _array = new Tuple<T, float>[capacity];
            _head = 0;
            _tail = 0;
            _size = 0;
        }

        public PriorityQueue(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException();
            }
            _array = new Tuple<T, float>[_DefaultCapacity];
            _size = 0;
            _version = 0;
            foreach (T item in collection)
            {
                Enqueue(item, 0);
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public object SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                {
                    Interlocked.CompareExchange<object>(ref _syncRoot, new object(), (object)null);
                }
                return _syncRoot;
            }
        }

        public void Clear()
        {
            if (_head < _tail)
            {
                Array.Clear(_array, _head, _size);
            }
            else
            {
                Array.Clear(_array, _head, _array.Length - _head);
                Array.Clear(_array, 0, _tail);
            }
            _head = 0;
            _tail = 0;
            _size = 0;
            _version++;
        }

        public void CopyTo(Tuple<T, float>[] targetArray, int arrayIndex)
        {
            if (targetArray == null)
            {
                throw new ArgumentNullException();
            }
            if (arrayIndex < 0 || arrayIndex > targetArray.Length)
            {
                throw new ArgumentOutOfRangeException();
            }
            int targetLength = targetArray.Length;
            if (targetLength - arrayIndex < _size)
            {
                throw new ArgumentException();
            }
            int totalCount = ((targetLength - arrayIndex < _size) ? (targetLength - arrayIndex) : _size);
            if (totalCount != 0)
            {
                int beginCount = ((_array.Length - _head < totalCount) ? (_array.Length - _head) : totalCount);
                Array.Copy(_array, _head, targetArray, arrayIndex, beginCount);
                totalCount -= beginCount;
                if (totalCount > 0)
                {
                    Array.Copy(_array, 0, targetArray, arrayIndex + _array.Length - _head, totalCount);
                }
            }
        }

        public void CopyTo(Array targetArray, int arrayIndex)
        {
            if (targetArray == null)
            {
                throw new ArgumentNullException();
            }
            if (targetArray.Rank != 1)
            {
                throw new ArgumentException();
            }
            int targetLength = targetArray.Length;
            if (arrayIndex < 0 || arrayIndex > targetLength)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (targetLength - arrayIndex < _size)
            {
                throw new ArgumentException();
            }
            int totalCount = ((targetLength - arrayIndex < _size) ? (targetLength - arrayIndex) : _size);
            if (totalCount == 0)
            {
                return;
            }
            try
            {
                int beginCount = ((_array.Length - _head < totalCount) ? (_array.Length - _head) : totalCount);
                Array.Copy(_array, _head, targetArray, arrayIndex, beginCount);
                totalCount -= beginCount;
                if (totalCount > 0)
                {
                    Array.Copy(_array, 0, targetArray, arrayIndex + _array.Length - _head, totalCount);
                }
            }
            catch (ArrayTypeMismatchException)
            {
                throw new ArgumentException();
            }
        }
        public void Enqueue(T item, float priority)
        {
            Tuple<T, float> element = Tuple.Create(item, priority);
            if (_size == 0)
            {
                _array = new Tuple<T, float>[_DefaultCapacity];
            }
            if (_size == _array.Length)
            {
                int newCapacity = (int)(_array.Length * _GrowFactor / 100);
                if (newCapacity < _array.Length + _MinimumGrow)
                {
                    newCapacity = _array.Length + _MinimumGrow;
                }
                SetCapacity(newCapacity);
            }
            _array[_tail] = element;
            _tail = (_tail + 1) % _array.Length;
            _size++;
            _version++;
            int index = _tail > 0 ? _tail - 1 : _array.Length - 1;
            if (_size <= 1)
            {
                return;
            }
            while (index != _head)
            {
                int parentIndex = index > 0 ? index - 1 : _array.Length - 1;
                if (_array[index].Item2 < _array[parentIndex].Item2)
                {
                    Swap(index, parentIndex);
                }
                index = parentIndex;
            }
        }

        public T Dequeue()
        {
            if (_size == 0)
            {
                throw new InvalidOperationException();
            }
            T result = _array[_head].Item1;
            _array[_head] = default;
            _head = (_head + 1) % _array.Length;
            _size--;
            _version++;
            return result;
        }

        public T Peek()
        {
            if (_size == 0)
            {
                throw new InvalidOperationException();
            }
            return _array[_head].Item1;
        }

        public bool Contains(Tuple<T, float> item)
        {
            int index = _head;
            int size = _size;
            EqualityComparer<Tuple<T, float>> @default = EqualityComparer<Tuple<T, float>>.Default;
            while (size-- > 0)
            {
                if (item == null)
                {
                    if (_array[index] == null)
                    {
                        return true;
                    }
                }
                else if (_array[index] != null && @default.Equals(_array[index], item))
                {
                    return true;
                }
                index = (index + 1) % _array.Length;
            }
            return false;
        }

        public Tuple<T, float>[] ToArray()
        {
            Tuple<T, float>[] array = new Tuple<T, float>[_size];
            if (_size == 0)
            {
                return array;
            }
            if (_head < _tail)
            {
                Array.Copy(_array, _head, array, 0, _size);
            }
            else
            {
                Array.Copy(_array, _head, array, 0, _array.Length - _head);
                Array.Copy(_array, 0, array, _array.Length - _head, _tail);
            }
            return array;
        }

        internal T GetElement(int index)
        {
            return _array[(_head + index) % _array.Length].Item1;
        }

        private void SetCapacity(int capacity)
        {
            Tuple<T, float>[] array = new Tuple<T, float>[capacity];
            if (_size > 0)
            {
                if (_head < _tail)
                {
                    Array.Copy(_array, _head, array, 0, _size);
                }
                else
                {
                    Array.Copy(_array, _head, array, 0, _array.Length - _head);
                    Array.Copy(_array, 0, array, _array.Length - _head, _tail);
                }
            }
            _array = array;
            _head = 0;
            _tail = ((_size != capacity) ? _size : 0);
            _version++;
        }

        public void TrimExcess()
        {
            int thresholdValue = (int)((double)_array.Length * _ShrinkThreshold);
            if (_size < thresholdValue)
            {
                SetCapacity(_size);
            }
        }
        private void Swap(int i, int j)
        {
            Tuple<T, float> temp = _array[i];
            _array[i] = _array[j];
            _array[j] = temp;
        }

        public override string ToString()
        {
            if (_size == 0)
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder();
            foreach (var item in _array)
            {
                builder.Append(item?.ToString());
            }
            return builder.ToString();
        }

        public string ToString(bool withSeparation = true)
        {
            if (withSeparation)
            {
                if (_size == 0)
                {
                    return string.Empty;
                }
                string[] values = new string[_array.Length];
                for (int index = 0; index < _size; index++)
                {
                    values[index] = _array[index]?.ToString();
                }
                string result = string.Join(Environment.NewLine, values);
                return result;
            }
            return ToString();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        private struct Enumerator : IEnumerator<T>
        {
            private PriorityQueue<T> _parent;
            private int _position;
            private int _version;
            private T _current;

            internal Enumerator(PriorityQueue<T> parent)
            {
                _parent = parent;
                _version = _parent._version;
                _position = -1;
                _current = default;
            }
            public T Current
            {
                get
                {
                    if (_position < 0)
                    {
                        throw new InvalidOperationException();
                    }
                    return _current;
                }
            }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                _position = -2;
                _current = default;
            }

            public bool MoveNext()
            {
                if (_version != _parent._version)
                {
                    throw new InvalidOperationException();
                }
                if (_position == -2)
                {
                    return false;
                }
                _position++;
                if (_position == _parent._size)
                {
                    _position = -2;
                    _current = default;
                    return false;
                }
                _current = _parent.GetElement(_position);
                return true;
            }

            public void Reset()
            {
                if (_version != _parent._version)
                {
                    throw new InvalidOperationException();
                }
                _position = -1;
                _current = default;
            }
        }
    }
}