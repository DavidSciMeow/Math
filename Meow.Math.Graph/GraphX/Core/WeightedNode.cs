using System;
using System.Collections.Generic;

namespace GraphX.Core
{
    /// <summary>
    /// ��Ȩ�ڵ㣺ά���ھӼ�����Ȩ�أ��̰߳�ȫ����<br/>Weighted node: maintains neighbors and node's own weight (thread-safe).
    /// </summary>
    /// <typeparam name="NodeType">�ڵ����� / node type</typeparam>
    public class WeightedNode<NodeType, TWeight> where NodeType : IEquatable<NodeType>
    {
        private readonly object _lock = new();
        private readonly Dictionary<NodeType, TWeight> _neighbors = new();
        private TWeight _nodeWeight = default!; // �ڵ�Ȩ�ص��ڲ��洢 / internal storage for node weight

        /// <summary>
        /// �ڵ��ʶ��<br/>Node identifier.
        /// </summary>
        public NodeType Id { get; }

        /// <summary>
        /// ��ʼ����Ȩ�ڵ㡣<br/>Init a weighted node.
        /// </summary>
        public WeightedNode(NodeType id)
        {
            Id = id;
            _nodeWeight = default!;
        }

        /// <summary>
        /// �̰߳�ȫ�ػ�ȡ�����ýڵ�Ȩ�ء�Ĭ��ֵΪ TWeight ��Ĭ��ֵ��<br/>Get or set the node weight in a thread-safe manner. Default is default(TWeight).
        /// </summary>
        public TWeight NodeWeight
        {
            get { lock (_lock) { return _nodeWeight; } }
            set { lock (_lock) { _nodeWeight = value; } }
        }

        /// <summary>
        /// ���Ի�ȡ�ھӽڵ�ı�Ȩ��<br/>Try get the neighbor weight.
        /// </summary>
        public bool TryGetWeight(NodeType nid, out TWeight weight)
        {
            lock (_lock) { return _neighbors.TryGetValue(nid, out weight); }
        }

        /// <summary>
        /// ����ھӲ����ñ�Ȩ��<br/>Add neighbor with edge weight.
        /// </summary>
        public bool AddNeighbor(NodeType nid, TWeight weight)
        {
            lock (_lock)
            {
                if (_neighbors.ContainsKey(nid)) return false;
                _neighbors.Add(nid, weight);
                return true;
            }
        }

        /// <summary>
        /// �Ƴ��ھӡ�<br/>Remove neighbor.
        /// </summary>
        public bool RemoveNeighbor(NodeType nid)
        {
            lock (_lock)
            {
                return _neighbors.Remove(nid);
            }
        }

        /// <summary>
        /// ԭ�ӵ�����������ھӡ�<br/>Add or update neighbor atomically.
        /// </summary>
        public void AddOrUpdateNeighbor(NodeType nid, TWeight weight)
        {
            lock (_lock)
            {
                _neighbors[nid] = weight;
            }
        }

        /// <summary>
        /// ��ȡ�ھӼ��ϵĿ����Ա㰲ȫö�٣����ؿ�������<br/>Get a snapshot of neighbors for safe enumeration (returns a copy).
        /// </summary>
        public IEnumerable<KeyValuePair<NodeType, TWeight>> Neighbors
        {
            get { lock (_lock) { return new List<KeyValuePair<NodeType, TWeight>>(_neighbors); } }
        }
    }
}