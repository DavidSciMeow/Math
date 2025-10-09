using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphX.Core
{
    /// <summary>
    /// 加权图实现：组合内部 `GraphStorage` 提供邻接与基本图操作。<br/>Weighted graph implementation that composes a GraphStorage for adjacency and basic graph operations.
    /// </summary>
    /// <typeparam name="NodeType">节点类型 / node type</typeparam>
    /// <typeparam name="TWeight">权重类型 / weight type</typeparam>
    public class Graph<NodeType, TWeight> : IGraph<NodeType, TWeight> 
        where NodeType : IEquatable<NodeType> 
        where TWeight : IComparer<TWeight>
    {
        private readonly GraphStorage<NodeType> _storage = new();
        private readonly Dictionary<NodeType, WeightedNode<NodeType, TWeight>> _wnodes = new();
        private readonly IWeightOperator<TWeight> _op;

        /// <summary>
        /// 使用给定的权重操作器构造加权图实例。<br/>Construct a weighted graph instance using the provided weight operator.
        /// </summary>
        public Graph(IWeightOperator<TWeight> op)
        {
            _op = op;
        }

        /// <inheritdoc/>
        public bool AddNode(NodeType id)
        {
            var added = _storage.AddNode(id);
            if (added)
            {
                _wnodes[id] = new WeightedNode<NodeType, TWeight>(id);
            }
            return added;
        }

        /// <inheritdoc/>
        public bool RemoveNode(NodeType id)
        {
            var removed = _storage.RemoveNode(id);
            if (removed)
            {
                if (_wnodes.ContainsKey(id)) _wnodes.Remove(id);
            }
            return removed;
        }

        /// <inheritdoc/>
        public bool Exist(NodeType id) => _storage.Exist(id);

        /// <inheritdoc/>
        public bool AddEdge(NodeType u, NodeType v, TWeight w, bool directed = false)
        {
            var added = _storage.AddEdge(u, v, directed);
            if (added)
            {
                if (!_wnodes.ContainsKey(u) || (!_wnodes.ContainsKey(v) && !directed))
                {
                    throw new Exception("Node not found");
                }

                _wnodes[u].AddNeighbor(v, w);
                if (!directed) _wnodes[v].AddNeighbor(u, w);
            }
            return added;
        }

        /// <summary>
        /// 添加无权边的便捷重载（使用操作器的 One 作为默认权重）。<br/>Convenience overload for adding an unweighted edge (uses operator.One as default weight).
        /// </summary>
        public bool AddEdge(NodeType u, NodeType v, bool directed = false)
        {
            // 将调用转发到带权重的 AddEdge，使用默认 One 作为权重（当 TWeight 可转换时）。/ forward to weighted AddEdge using default One weight when TWeight can be cast
            if (typeof(TWeight) == typeof(long))
            {
                var w = (TWeight)(object)Convert.ToInt64(((IWeightOperator<long>)(object)_op).One);
                return AddEdge(u, v, w, directed);
            }
            // 尽力通过操作器使用默认 One（若可用）。/ best-effort: attempt to use default 'One' through operator if available
            try
            {
                var one = _op.One;
                return AddEdge(u, v, one, directed);
            }
            catch
            {
                throw new InvalidOperationException("Cannot add unweighted edge because TWeight does not support default 'One' conversion.");
            }
        }

        /// <inheritdoc/>
        public bool RemoveEdge(NodeType u, NodeType v, bool directed = false)
        {
            var removed = _storage.RemoveEdge(u, v, directed);
            if (removed)
            {
                if (_wnodes.ContainsKey(u)) _wnodes[u].RemoveNeighbor(v);
                if (!directed && _wnodes.ContainsKey(v)) _wnodes[v].RemoveNeighbor(u);
            }
            return removed;
        }

        /// <inheritdoc/>
        public IEnumerable<KeyValuePair<NodeType, TWeight>> GetNeighbors(NodeType u)
        {
            if (!_wnodes.ContainsKey(u))
            {
                // 回退到存储层邻居，并使用默认权重 / fall back to storage neighbors with default weight
                return _storage.GetNeighbors(u).Select(x => new KeyValuePair<NodeType, TWeight>(x, _op.One));
            }
            return _wnodes[u].Neighbors.ToList();
        }

        /// <summary>
        /// 便捷的无权邻居访问（只返回节点标识）。<br/>Convenience method returning neighbor node ids only (no weights).
        /// </summary>
        public IEnumerable<NodeType> GetNeighborNodes(NodeType u)
        {
            return _storage.GetNeighbors(u);
        }

        /// <inheritdoc/>
        public IEnumerable<(NodeType U, NodeType V, TWeight W)> WeightedEdges
        {
            get
            {
                var list = new List<(NodeType U, NodeType V, TWeight W)>();
                foreach (var e in _storage.Edges)
                {
                    if (_wnodes.TryGetValue(e.U, out var nu) && nu.TryGetWeight(e.V, out var w))
                    {
                        list.Add((e.U, e.V, w));
                    }
                    else
                    {
                        list.Add((e.U, e.V, _op.One));
                    }
                }
                return list;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<(NodeType U, NodeType V)> Edges => _storage.Edges;

        // 泛型 IGraph 的显式实现（无权视图） / Explicit implementation for generic IGraph Edges (non-weighted view)
        /// <inheritdoc/>
        IEnumerable<(NodeType U, NodeType V)> IGraph<NodeType, TWeight>.Edges => _storage.Edges;

        /// <inheritdoc/>
        public TWeight GetNodeWeight(NodeType id)
        {
            if (!_wnodes.ContainsKey(id)) throw new Exception("Node not found");
            return _wnodes[id].NodeWeight;
        }

        /// <inheritdoc/>
        public void SetNodeWeight(NodeType id, TWeight weight)
        {
            if (!_wnodes.ContainsKey(id)) throw new Exception("Node not found");
            _wnodes[id].NodeWeight = weight;
        }

        /// <inheritdoc/>
        public bool IsDirected => _storage.IsDirected;

        /// <inheritdoc/>
        public int NodeCount => _storage.NodeCount;

        /// <inheritdoc/>
        public int EdgeCount => _storage.EdgeCount;

        /// <inheritdoc/>
        public IEnumerable<NodeType> Nodes => _storage.Nodes;

        IEnumerable<(NodeType U, NodeType V, TWeight W)> IGraph<NodeType, TWeight>.WeightedEdges => WeightedEdges;

        /// <inheritdoc/>
        public int Degree(NodeType node) => _storage.Degree(node);

        /// <inheritdoc/>
        public int OutDegree(NodeType node) => _storage.OutDegree(node);

        /// <inheritdoc/>
        public int InDegree(NodeType node) => _storage.InDegree(node);

        /// <inheritdoc/>
        public bool IsConnected() => _storage.IsConnected();

    }
}
