using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Hake.Extension.ValueRecord
{
    [DebuggerDisplay("Count={Count}")]
    public sealed class ListRecord : RecordBase, IList<RecordBase>, IReadOnlyList<RecordBase>
    {
        private IList<RecordBase> records;

        #region interfaces implementations
        public RecordBase this[int index]
        {
            get { return records[index]; }
            set { records[index] = value; }
        }

        public int Count { get { return records.Count; } }

        public bool IsReadOnly { get { return false; } }

        public void Add(RecordBase item)
        {
            records.Add(item);
        }

        public void Clear()
        {
            records.Clear();
        }

        public bool Contains(RecordBase item)
        {
            return records.Contains(item);
        }

        public void CopyTo(RecordBase[] array, int arrayIndex)
        {
            records.CopyTo(array, arrayIndex);
        }

        public IEnumerator<RecordBase> GetEnumerator()
        {
            return records.GetEnumerator();
        }

        public int IndexOf(RecordBase item)
        {
            return records.IndexOf(item);
        }

        public void Insert(int index, RecordBase item)
        {
            records.Insert(index, item);
        }

        public bool Remove(RecordBase item)
        {
            return records.Remove(item);
        }

        public void RemoveAt(int index)
        {
            records.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return records.GetEnumerator();
        }
        #endregion interfaces implementations

        public ListRecord() : base(RecordType.List)
        {
            records = new List<RecordBase>();
        }
        public ListRecord(IEnumerable<RecordBase> records) : base(RecordType.List)
        {
            this.records = new List<RecordBase>(records);
        }
        public ListRecord(int capcity) : base(RecordType.List)
        {
            records = new List<RecordBase>(capcity);
        }
    }
}
