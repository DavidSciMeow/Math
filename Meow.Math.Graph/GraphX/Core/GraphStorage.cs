using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GraphX.Core
{
    /// <summary>
    /// �̰߳�ȫ��ͼ�洢������� GraphBase ��ȡ����<br/>Thread-safe graph storage component extracted from GraphBase.
    /// �ṩ�ڽӹ�ϵ�ͱ߹���Ļ���������<br/>Provides adjacency and edge management primitives.
    /// </summary>
    /// <typeparam name="NodeType">�ڵ����� / node type</typeparam>
    public class GraphStorage<NodeType> where NodeType : IEquatable<NodeType>
    {
        private readonly Dictionary<NodeType, HashSet<NodeType>> _adj = new();
        private readonly List<(NodeType U, NodeType V)> _edges = new();
        private readonly ReaderWriterLockSlim _rw = new(LockRecursionPolicy.NoRecursion);

        /// <summary>
        /// ָʾͼ�Ƿ��������ߡ�<br/>Whether the graph contains directed edges.
        /// </summary>
        public bool IsDirected { get; private set; } = false;

        /// <summary>
        /// �ڵ�������<br/>Number of nodes.
        /// </summary>
        public int NodeCount { get { _rw.EnterReadLock(); try { return _adj.Count; } finally { _rw.ExitReadLock(); } } }

        /// <summary>
        /// �����������洢����<br/>Number of edges (as stored).
        /// </summary>
        public int EdgeCount { get { _rw.EnterReadLock(); try { return _edges.Count; } finally { _rw.ExitReadLock(); } } }

        /// <summary>
        /// ö�����нڵ㡣<br/>Enumerate all nodes.
        /// </summary>
        public IEnumerable<NodeType> Nodes { get { _rw.EnterReadLock(); try { return _adj.Keys.ToList(); } finally { _rw.ExitReadLock(); } } }

        /// <summary>
        /// ö�����У��洢�ģ��߶ԡ�<br/>Enumerate stored edge pairs.
        /// </summary>
        public IEnumerable<(NodeType U, NodeType V)> Edges { get { _rw.EnterReadLock(); try { return _edges.ToList(); } finally { _rw.ExitReadLock(); } } }

        /// <summary>
        /// ��ӽڵ㡣<br/>Add a node.
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
        /// ɾ���ڵ㲢������رߡ�<br/>Remove a node and clean up incident edges.
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
        /// ���ڵ��Ƿ���ڡ�<br/>Check if a node exists.
        /// </summary>
        public bool Exist(NodeType id)
        {
            _rw.EnterReadLock();
            try { return _adj.ContainsKey(id); }
            finally { _rw.ExitReadLock(); }
        }

        /// <summary>
        /// ��ӱߣ���������򣩡�<br/>Add an edge (directed or undirected).
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
                    IsDirected = IsDirected || true; // һ����������ߣ���־����Ϊ true / once any directed edge exists, flag stays true
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
        /// ɾ���ߣ���������򣩡�<br/>Remove an edge (directed or undirected).
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
        /// ��ȡ�ڵ���ھӼ��ϡ�<br/>Get neighbors of a given node.
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
        /// �淶������ߵļ�����֤˳��һ���ԣ���<br/>Normalize undirected edge key to a canonical ordering.
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
        /// �ڵ�Ķȡ�<br/>Degree of a node.
        /// </summary>
        public int Degree(NodeType node)
        {
            _rw.EnterReadLock();
            try
            {
                if (!_adj.ContainsKey(node)) throw new ArgumentException("Node not found");
                if (!IsDirected) return _adj[node].Count;

                // �ڳ��ж���������¼����������ȣ�����ݹ���� / compute out-degree and in-degree while holding the read lock to avoid recursive lock acquisition
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
        /// �ڵ�ĳ��ȡ�<br/>Out-degree of a node.
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
        /// �ڵ����ȡ�<br/>In-degree of a node.
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
        /// ���ͼ�Ƿ���ͨ������ͨ�Լ�飩��<br/>Check whether the graph is connected (weak connectivity check).
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
