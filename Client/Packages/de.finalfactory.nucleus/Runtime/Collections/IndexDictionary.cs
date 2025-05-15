using System.Collections.Generic;

namespace FinalFactory.Collections
{
    public class IndexDictionary<TKey, TValue>
    {
        private readonly DualDictionary<TKey, int> _keyToIndex = new();
        private readonly List<TValue> _values = new();

        public int Count => _values.Count;

        public IReadOnlyList<TValue> Values => _values;
        
        public bool ContainsKey(TKey key) => _keyToIndex.ContainsKey(key);
        
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (_keyToIndex.TryGetValue(key, out var index))
            {
                value = _values[index];
                return true;
            }
            value = default;
            return false;
        }

        public void Add(TKey key, TValue value)
        {
            if (_keyToIndex.ContainsKey(key))
            {
                throw new System.ArgumentException($"An element with the same key already exists. Key: {key}");
            }
            _keyToIndex.Add(key, _values.Count);
            _values.Add(value);
        }

        public bool Remove(TKey key)
        {
            if (_keyToIndex.TryGetValue(key, out var index))
            {
                // If the key is the last element, just remove it
                if (index == _values.Count - 1)
                {
                    _keyToIndex.Remove(key);
                    _values.RemoveAt(index);
                    return true;
                }
                // Otherwise, move the last element to the index of the key
                if (_keyToIndex.TryGetKey(_values.Count - 1, out var lastKey))
                {
                    _keyToIndex.Remove(key);
                
                    _values[index] = _values[^1];
                    _keyToIndex[lastKey] = index;

                    // Remove the last element
                    _values.RemoveAt(_values.Count - 1);
                    return true;
                }
            }
            return false;
        }

        public TValue this[TKey key]
        {
            get
            {
                var index = _keyToIndex[key];
                return _values[index];
            }
            set
            {
                if (_keyToIndex.TryGetValue(key, out var index))
                {
                    _values[index] = value;
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        public TValue this[int index]
        {
            get => _values[index];
            set => _values[index] = value;
        }
        
        public void ChangeKey(TKey oldKey, TKey newKey) => _keyToIndex.ChangeKey(oldKey, newKey);

        public void Clear()
        {
            _values.Clear();
            _keyToIndex.Clear();
        }
    }
}