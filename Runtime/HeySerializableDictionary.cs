using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace JahnStarGames.Attributes
{
    [Serializable]
    public class HeySerializableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<SerializableKeyValuePair> list = new();
        
        [SerializeField, HideInInspector]
        private Dictionary<TKey, int> indexByKey = new();
        
        [SerializeField, HideInInspector]
        private Dictionary<TKey, TValue> dict = new();

        #pragma warning disable 0414
        [SerializeField, HideInInspector]
        private bool keyCollision;
        #pragma warning restore 0414

        [Serializable]
        public class SerializableKeyValuePair
        {
            public TKey Key;
            public TValue Value;

            public SerializableKeyValuePair(TKey key, TValue value)
            {
                Key = key;
                Value = value;
            }
        }

        public void OnBeforeSerialize()
        {
            // Update dictionary values from list before serialization
            for (int i = 0; i < list.Count; i++)
            {
                var key = list[i].Key;
                if (key != null && dict.ContainsKey(key)) dict[key] = list[i].Value;
            }
        }

        public void OnAfterDeserialize()
        {
            dict.Clear();
            indexByKey.Clear();
            keyCollision = false;

            for (int i = 0; i < list.Count; i++)
            {
                var key = list[i].Key;
                if (key != null && !dict.ContainsKey(key))
                {
                    dict.Add(key, list[i].Value);
                    indexByKey.Add(key, i);
                }
                else keyCollision = true;
            }
        }

        public void SyncListToDictionary(int index)
        {
            if (index >= 0 && index < list.Count)
            {
                var kvp = list[index];
                if (kvp.Key != null) dict[kvp.Key] = kvp.Value;
            }
        }

        public TValue this[TKey key]
        {
            get => dict[key];
            set
            {
                dict[key] = value;
                if (indexByKey.ContainsKey(key))
                {
                    var index = indexByKey[key];
                    list[index] = new SerializableKeyValuePair(key, value);
                }
                else
                {
                    list.Add(new SerializableKeyValuePair(key, value));
                    indexByKey.Add(key, list.Count - 1);
                }
            }
        }

        public ICollection<TKey> Keys => dict.Keys;
        public ICollection<TValue> Values => dict.Values;
        public int Count => dict.Count;
        public bool IsReadOnly { get; set; }

        public void Add(TKey key, TValue value)
        {
            dict.Add(key, value);
            list.Add(new SerializableKeyValuePair(key, value));
            indexByKey.Add(key, list.Count - 1);
        }

        public bool ContainsKey(TKey key) => dict.ContainsKey(key);

        public bool Remove(TKey key)
        {
            if (dict.Remove(key))
            {
                var index = indexByKey[key];
                list.RemoveAt(index);
                UpdateIndexLookup(index);
                indexByKey.Remove(key);
                return true;
            }
            return false;
        }

        private void UpdateIndexLookup(int removedIndex)
        {
            for (int i = removedIndex; i < list.Count; i++)
            {
                var key = list[i].Key;
                indexByKey[key] = i;
            }
        }

        public bool TryGetValue(TKey key, out TValue value) => dict.TryGetValue(key, out value);

        public void Clear()
        {
            dict.Clear();
            list.Clear();
            indexByKey.Clear();
        }

        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

        public bool Contains(KeyValuePair<TKey, TValue> item)
        => dict.TryGetValue(item.Key, out TValue value) ? EqualityComparer<TValue>.Default.Equals(value, item.Value) : false;

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentException("The array cannot be null.");
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException("The starting array index cannot be negative.");
            if (array.Length - arrayIndex < dict.Count) throw new ArgumentException("The destination array has fewer elements than the collection.");

            foreach (var pair in dict)
            {
                array[arrayIndex] = pair;
                arrayIndex++;
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (dict.TryGetValue(item.Key, out TValue value))
            {
                bool valueMatch = EqualityComparer<TValue>.Default.Equals(value, item.Value);
                if (valueMatch) return Remove(item.Key);
            }
            return false;
        }

        public HeySerializableDictionary<string, string> Clone()
        {
            HeySerializableDictionary<string, string> clone = new();
            foreach (var kvp in dict) clone.Add(kvp.Key.ToString(), kvp.Value.ToString());
            return clone;
        }

        public Dictionary<TKey, TValue> ToDictionary()
        {
            Dictionary<TKey, TValue> newDict = new();
            foreach (var kvp in dict) newDict.Add(kvp.Key, kvp.Value);
            return newDict;
        }

        public void ForEach(Action<KeyValuePair<TKey, TValue>> action)
        {
            foreach (var kvp in dict) action(kvp);
        }
        
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => dict.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => dict.GetEnumerator();

    }
}