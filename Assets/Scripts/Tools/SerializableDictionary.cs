using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A dictionary that Unity can serialise. Entries are fixed after construction —
/// Inspector cannot add/remove items or modify keys, only edit values.
/// </summary>
[Serializable]
public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver,
    IEnumerable<KeyValuePair<TKey, TValue>>
{
    [Serializable]
    private struct Entry
    {
        public TKey key;
        public TValue value;
    }

    [SerializeField] private List<Entry> _entries = new();
    [NonSerialized] private Dictionary<TKey, TValue> _dict = new();

    /* ---------- Runtime API (mirrors Dictionary<TKey,TValue>) ---------- */

    public TValue this[TKey key]
    {
        get => _dict[key];
        set => _dict[key] = value;
    }

    public int Count => _dict.Count;
    public bool ContainsKey(TKey key) => _dict.ContainsKey(key);
    public void Add(TKey key, TValue value) => _dict.Add(key, value);
    public Dictionary<TKey, TValue>.KeyCollection Keys => _dict.Keys;
    public Dictionary<TKey, TValue>.ValueCollection Values => _dict.Values;

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dict.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /* -------------- Unity serialisation round-trip -------------- */

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        _entries.Clear();
        foreach (var kvp in _dict)
            _entries.Add(new Entry { key = kvp.Key, value = kvp.Value });
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        _dict.Clear();
        foreach (var entry in _entries)
            _dict[entry.key] = entry.value;
    }
}

// Concrete types Unity's serialisation + PropertyDrawer can bind to
[Serializable]
public class FloatDictionary : SerializableDictionary<string, float> { }

[Serializable]
public class VectorDictionary : SerializableDictionary<string, Vector4> { }