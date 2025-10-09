using System;
using System.Collections.Generic;

namespace GraphX.Util
{
    /// <summary>
    /// 简单的二叉最小堆实现（教学用）；支持重复优先级；不提供 decrease-key。<br/>Simple binary min-heap implementation for teaching. Supports duplicate priorities. No decrease-key.
    /// </summary>
    public class BinaryHeap<TPriority, TItem>
    {
        private readonly List<(TPriority priority, TItem item)> _data;
        private readonly Comparison<TPriority> _cmp;

        public BinaryHeap(Comparison<TPriority> comparer)
        {
            if (comparer == null) throw new ArgumentNullException("comparer");
            _cmp = comparer;
            _data = new List<(TPriority, TItem)>();
        }

        public int Count { get { return _data.Count; } }

        public void Enqueue(TItem item, TPriority priority)
        {
            _data.Add((priority, item));
            SiftUp(_data.Count - 1);
        }

        public (TPriority priority, TItem item) DequeueMin()
        {
            if (_data.Count == 0) throw new InvalidOperationException("Heap is empty");
            var root = _data[0];
            var last = _data[_data.Count - 1];
            _data.RemoveAt(_data.Count - 1);
            if (_data.Count > 0)
            {
                _data[0] = last;
                SiftDown(0);
            }
            return root;
        }

        private void SiftUp(int i)
        {
            while (i > 0)
            {
                int parent = (i - 1) / 2;
                if (_cmp(_data[i].priority, _data[parent].priority) < 0)
                {
                    var tmp = _data[i]; _data[i] = _data[parent]; _data[parent] = tmp;
                    i = parent;
                }
                else break;
            }
        }

        private void SiftDown(int i)
        {
            int n = _data.Count;
            while (true)
            {
                int left = i * 2 + 1;
                int right = i * 2 + 2;
                int smallest = i;
                if (left < n && _cmp(_data[left].priority, _data[smallest].priority) < 0) smallest = left;
                if (right < n && _cmp(_data[right].priority, _data[smallest].priority) < 0) smallest = right;
                if (smallest == i) break;
                var tmp = _data[i]; _data[i] = _data[smallest]; _data[smallest] = tmp;
                i = smallest;
            }
        }
    }
}
