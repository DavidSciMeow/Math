using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GraphX.Core
{
    /// <summary>
    /// 线程安全的图存储组件（从 GraphBase 提取）。<br/>Thread-safe graph storage component extracted from GraphBase.
    /// 提供邻接关系和边管理的基础能力。<br/>Provides adjacency and edge management primitives.
    /// </summary>
    /// <typeparam name="NodeType">节点类型 / node type</typeparam>
    public class GraphStorage<NodeType> where NodeType : IEquatable<NodeType>
    {
        private readonly Dictionary<NodeType, HashSet<NodeType>> _adj = new();
        private readonly List<(NodeType U, NodeType V)> _edges = new();
        private readonly ReaderWriterLockSlim _rw = new(LockRecursionPolicy.NoRecursion);

        /// <summary>
        /// 指示图是否包含有向边。<br/>Whether the graph contains directed edges.
        /// </summary>
        public bool IsDirected { get; private set; } = false;

        /// <summary>
        /// 节点数量。<br/>Number of nodes.
        /// </summary>
        public int NodeCount { get { _rw.EnterReadLock(); try { return _adj.Count; } finally { _rw.ExitReadLock(); } } }

        /// <summary>
        /// 边数量（按存储）。<br/>Number of edges (as stored).
        /// </summary>
        public int EdgeCount { get { _rw.EnterReadLock(); try { return _edges.Count; } finally { _rw.ExitReadLock(); } } }

        /// <summary>
        /// 枚举所有节点。<br/>Enumerate all nodes.
        /// </summary>
        public IEnumerable<NodeType> Nodes { get { _rw.EnterReadLock(); try { return _adj.Keys.ToList(); } finally { _rw.ExitReadLock(); } } }

        /// <summary>
        /// 枚举所有（存储的）边对。<br/>Enumerate stored edge pairs.
        /// </summary>
        public IEnumerable<(NodeType U, NodeType V)> Edges { get { _rw.EnterReadLock(); try { return _edges.ToList(); } finally { _rw.ExitReadLock(); } } }

        /// <summary>
        /// 添加节点。<br/>Add a node.
        /// </summary>
        public bool AddNode(NodeType id)
        {
            _rw.EnterWriteLock();
            try
            {
                if (_adj.ContainsKey(id)) return false;
                _adj[id] = new HashSet<NodeType>();
                return true;
            }
            finally { _rw.ExitWriteLock(); }
        }

        /// <summary>
        /// 删除节点并清理相关边。<br/>Remove a node and clean up incident edges.
        /// </summary>
        public bool RemoveNode(NodeType id)
        {
            _rw.EnterWriteLock();
            try
            {
                if (!_adj.ContainsKey(id)) return false;
                _adj.Remove(id);
                foreach (var kv in _adj)
                {
                    kv.Value.Remove(id);
                }
                _edges.RemoveAll(e => EqualityComparer<NodeType>.Default.Equals(e.U, id) || EqualityComparer<NodeType>.Default.Equals(e.V, id));
                return true;
            }
            finally { _rw.ExitWriteLock(); }
        }

        /// <summary>
        /// 检查节点是否存在。<br/>Check if a node exists.
        /// </summary>
        public bool Exist(NodeType id)
        {
            _rw.EnterReadLock();
            try { return _adj.ContainsKey(id); }
            finally { _rw.ExitReadLock(); }
        }

        /// <summary>
        /// 添加边（有向或无向）。<br/>Add an edge (directed or undirected).
        /// </summary>
        public bool AddEdge(NodeType u, NodeType v, bool directed = false)
        {
            _rw.EnterWriteLock();
            try
            {
                if (!_adj.ContainsKey(u) || !_adj.ContainsKey(v)) throw new ArgumentException("One or both nodes do not exist");
                if (directed)
                {
                    _adj[u].Add(v);
                    _edges.Add((u, v));
                    IsDirected = IsDirected || true; // 一旦出现有向边，标志保持为 true / once any directed edge exists, flag stays true
                    return true;
                }
                else
                {
                    _adj[u].Add(v);
                    _adj[v].Add(u);
                    var key = NormalizeEdge(u, v);
                    if (!_edges.Contains(key)) _edges.Add(key);
                    return true;
                }
            }
            finally { _rw.ExitWriteLock(); }
        }

        /// <summary>
        /// 删除边（有向或无向）。<br/>Remove an edge (directed or undirected).
        /// </summary>
        public bool RemoveEdge(NodeType u, NodeType v, bool directed = false)
        {
            _rw.EnterWriteLock();
            try
            {
                if (directed)
                {
                    var removed = _adj.ContainsKey(u) && _adj[u].Remove(v);
                    if (removed) _edges.RemoveAll(e => EqualityComparer<NodeType>.Default.Equals(e.U, u) && EqualityComparer<NodeType>.Default.Equals(e.V, v));
                    return removed;
                }
                else
                {
                    var removed1 = _adj.ContainsKey(u) && _adj[u].Remove(v);
                    var removed2 = _adj.ContainsKey(v) && _adj[v].Remove(u);
                    var key = NormalizeEdge(u, v);
                    _edges.RemoveAll(e => EqualityComparer<NodeType>.Default.Equals(e.U, key.U) && EqualityComparer<NodeType>.Default.Equals(e.V, key.V));
                    return removed1 || removed2;
                }
            }
            finally { _rw.ExitWriteLock(); }
        }

        /// <summary>
        /// 获取节点的邻居集合。<br/>Get neighbors of a given node.
        /// </summary>
        public IEnumerable<NodeType> GetNeighbors(NodeType u)
        {
            _rw.EnterReadLock();
            try
            {
                if (!_adj.ContainsKey(u)) throw new ArgumentException("Node not found");
                return _adj[u].ToList();
            }
            finally { _rw.ExitReadLock(); }
        }

        /// <summary>
        /// 规范化无向边的键（保证顺序一致性）。<br/>Normalize undirected edge key to a canonical ordering.
        /// </summary>
        protected (NodeType U, NodeType V) NormalizeEdge(NodeType a, NodeType b)
        {
            var ha = EqualityComparer<NodeType>.Default.GetHashCode(a);
            var hb = EqualityComparer<NodeType>.Default.GetHashCode(b);
            if (ha < hb) return (a, b);
            if (ha > hb) return (b, a);
            var sa = a?.ToString() ?? string.Empty;
            var sb = b?.ToString() ?? string.Empty;
            return String.CompareOrdinal(sa, sb) <= 0 ? (a, b) : (b, a);
        }

        /// <summary>
        /// 节点的度。<br/>Degree of a node.
        /// </summary>
        public int Degree(NodeType node)
        {
            _rw.EnterReadLock();
            try
            {
                if (!_adj.ContainsKey(node)) throw new ArgumentException("Node not found");
                if (!IsDirected) return _adj[node].Count;

                // 在持有读锁的情况下计算出度与入度，避免递归加锁 / compute out-degree and in-degree while holding the read lock to avoid recursive lock acquisition
                int outDeg = _adj[node].Count;
                int inDeg = 0;
                foreach (var kv in _adj)
                {
                    if (kv.Value.Contains(node)) inDeg++;
                }
                return inDeg + outDeg;
            }
            finally { _rw.ExitReadLock(); }
        }

        /// <summary>
        /// 节点的出度。<br/>Out-degree of a node.
        /// </summary>
        public int OutDegree(NodeType node)
        {
            _rw.EnterReadLock();
            try
            {
                if (!_adj.ContainsKey(node)) throw new ArgumentException("Node not found");
                return _adj[node].Count;
            }
            finally { _rw.ExitReadLock(); }
        }

        /// <summary>
        /// 节点的入度。<br/>In-degree of a node.
        /// </summary>
        public int InDegree(NodeType node)
        {
            _rw.EnterReadLock();
            try
            {
                if (!_adj.ContainsKey(node)) throw new ArgumentException("Node not found");
                int cnt = 0;
                foreach (var kv in _adj)
                {
                    if (kv.Value.Contains(node)) cnt++;
                }
                return cnt;
            }
            finally { _rw.ExitReadLock(); }
        }

        /// <summary>
        /// 检查图是否连通（弱连通性检查）。<br/>Check whether the graph is connected (weak connectivity check).
        /// </summary>
        public bool IsConnected()
        {
            _rw.EnterReadLock();
            try
            {
                if (_adj.Count == 0) return true;
                var start = _adj.Keys.First();
                var visited = new HashSet<NodeType>();
                var q = new Queue<NodeType>();
                visited.Add(start);
                q.Enqueue(start);
                while (q.Count > 0)
                {
                    var u = q.Dequeue();
                    foreach (var v in _adj[u])
                    {
                        if (visited.Add(v)) q.Enqueue(v);
                    }
                    foreach (var kv in _adj)
                    {
                        if (kv.Value.Contains(u) && visited.Add(kv.Key)) q.Enqueue(kv.Key);
                    }
                }
                return visited.Count == _adj.Count;
            }
            finally { _rw.ExitReadLock(); }
        }
    }
}
