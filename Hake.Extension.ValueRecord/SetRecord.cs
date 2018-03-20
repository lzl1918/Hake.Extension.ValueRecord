using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Hake.Extension.ValueRecord
{
    [DebuggerDisplay("Count={Count}")]
    public sealed class SetRecord : RecordBase, IDictionary<string, RecordBase>
    {
        private IDictionary<string, RecordBase> records;

        #region interfaces implementations
        public RecordBase this[string key]
        {
            get { return records[key]; }
            set { records[key] = value; }
        }

        public ICollection<string> Keys { get { return records.Keys; } }

        public ICollection<RecordBase> Values { get { return records.Values; } }

        public int Count { get { return records.Count; } }

        public bool IsReadOnly { get { return records.IsReadOnly; } }

        public void Add(string key, RecordBase value)
        {
            records.Add(key, value);
        }

        public void Add(KeyValuePair<string, RecordBase> item)
        {
            records.Add(item);
        }

        public void Clear()
        {
            records.Clear();
        }

        public bool Contains(KeyValuePair<string, RecordBase> item)
        {
            return records.Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return records.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, RecordBase>[] array, int arrayIndex)
        {
            records.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, RecordBase>> GetEnumerator()
        {
            return records.GetEnumerator();
        }

        public bool Remove(string key)
        {
            return records.Remove(key);
        }

        public bool Remove(KeyValuePair<string, RecordBase> item)
        {
            return records.Remove(item);
        }

        public bool TryGetValue(string key, out RecordBase value)
        {
            return records.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return records.GetEnumerator();
        }
        #endregion interfaces implementations

        public SetRecord(bool ignoreKeyCase = false) : base(RecordType.Set)
        {
            if (ignoreKeyCase)
                records = new Dictionary<string, RecordBase>(StringComparer.OrdinalIgnoreCase);
            else
                records = new Dictionary<string, RecordBase>();
        }
        public SetRecord(IDictionary<string, RecordBase> records, bool ignoreKeyCase = false) : base(RecordType.Set)
        {
            if (ignoreKeyCase)
                this.records = new Dictionary<string, RecordBase>(records, StringComparer.OrdinalIgnoreCase);
            else
                this.records = new Dictionary<string, RecordBase>(records);
        }
        public SetRecord(int capacity, bool ignoreKeyCase = false) : base(RecordType.Set)
        {
            if (ignoreKeyCase)
                records = new Dictionary<string, RecordBase>(capacity, StringComparer.OrdinalIgnoreCase);
            else
                records = new Dictionary<string, RecordBase>(capacity);
        }
    }
}
