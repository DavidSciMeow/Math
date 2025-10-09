using System;
using System.Collections.Generic;

namespace GraphX.Core
{
    /// <summary>
    /// 加权节点：维护邻居及自身权重（线程安全）。<br/>Weighted node: maintains neighbors and node's own weight (thread-safe).
    /// </summary>
    /// <typeparam name="NodeType">节点类型 / node type</typeparam>
    public class WeightedNode<NodeType, TWeight> where NodeType : IEquatable<NodeType>
    {
        private readonly object _lock = new();
        private readonly Dictionary<NodeType, TWeight> _neighbors = new();
        private TWeight _nodeWeight = default!; // 节点权重的内部存储 / internal storage for node weight

        /// <summary>
        /// 节点标识。<br/>Node identifier.
        /// </summary>
        public NodeType Id { get; }

        /// <summary>
        /// 初始化加权节点。<br/>Init a weighted node.
        /// </summary>
        public WeightedNode(NodeType id)
        {
            Id = id;
            _nodeWeight = default!;
        }

        /// <summary>
        /// 线程安全地获取或设置节点权重。默认值为 TWeight 的默认值。<br/>Get or set the node weight in a thread-safe manner. Default is default(TWeight).
        /// </summary>
        public TWeight NodeWeight
        {
            get { lock (_lock) { return _nodeWeight; } }
            set { lock (_lock) { _nodeWeight = value; } }
        }

        /// <summary>
        /// 尝试获取邻居节点的边权。<br/>Try get the neighbor weight.
        /// </summary>
        public bool TryGetWeight(NodeType nid, out TWeight weight)
        {
            lock (_lock) { return _neighbors.TryGetValue(nid, out weight); }
        }

        /// <summary>
        /// 添加邻居并设置边权。<br/>Add neighbor with edge weight.
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
        /// 移除邻居。<br/>Remove neighbor.
        /// </summary>
        public bool RemoveNeighbor(NodeType nid)
        {
            lock (_lock)
            {
                return _neighbors.Remove(nid);
            }
        }

        /// <summary>
        /// 原子地新增或更新邻居。<br/>Add or update neighbor atomically.
        /// </summary>
        public void AddOrUpdateNeighbor(NodeType nid, TWeight weight)
        {
            lock (_lock)
            {
                _neighbors[nid] = weight;
            }
        }

        /// <summary>
        /// 获取邻居集合的快照以便安全枚举（返回拷贝）。<br/>Get a snapshot of neighbors for safe enumeration (returns a copy).
        /// </summary>
        public IEnumerable<KeyValuePair<NodeType, TWeight>> Neighbors
        {
            get { lock (_lock) { return new List<KeyValuePair<NodeType, TWeight>>(_neighbors); } }
        }
    }
}