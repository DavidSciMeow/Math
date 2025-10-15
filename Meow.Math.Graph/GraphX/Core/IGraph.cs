using System;
using System.Collections.Generic;

namespace GraphX.Core
{
    /// <summary>
    /// 统一的图接口：描述包含权重的常见图操作与属性。<br/>Unified graph interface describing common graph operations and properties including weights.
    /// </summary>
    /// <typeparam name="NodeType">节点类型 / node type</typeparam>
    /// <typeparam name="TWeight">权重类型 / weight type</typeparam>
    public interface IGraph<NodeType, TWeight> 
        where NodeType : IEquatable<NodeType>
        where TWeight : IComparable<TWeight>
    {
        /// <summary>
        /// 是否为有向图。<br/>Whether the graph is directed.
        /// </summary>
        bool IsDirected { get; }

        /// <summary>
        /// 图中节点数量。<br/>Number of nodes in the graph.
        /// </summary>
        int NodeCount { get; }

        /// <summary>
        /// 图中边数量（按存储方式计数）。<br/>Number of edges in the graph (as stored).
        /// </summary>
        int EdgeCount { get; }

        /// <summary>
        /// 枚举所有节点。<br/>Enumerate all nodes.
        /// </summary>
        IEnumerable<NodeType> Nodes { get; }

        /// <summary>
        /// 主邻居访问器：返回带权邻居对 (neighbor, weight)。对无权图权重可能为默认的 "one"。<br/>Primary neighbor accessor: returns weighted neighbor pairs (neighbor, weight). For unweighted graphs weight may be the default "one".
        /// </summary>
        IEnumerable<KeyValuePair<NodeType, TWeight>> GetNeighbors(NodeType u);

        /// <summary>
        /// 带权边枚举（优先形式，按存储方向）。<br/>Enumerate weighted edges (primary form, directed as stored).
        /// </summary>
        IEnumerable<(NodeType U, NodeType V, TWeight W)> WeightedEdges { get; }

        /// <summary>
        /// 兼容性：不带权重的边列表。<br/>Compatibility: edge list without weights.
        /// </summary>
        IEnumerable<(NodeType U, NodeType V)> Edges { get; }

        /// <summary>
        /// 添加节点（若存在则返回 false）。<br/>Add a node (returns false when already present).
        /// </summary>
        bool AddNode(NodeType id);

        /// <summary>
        /// 删除节点（若不存在则返回 false）。<br/>Remove a node (returns false when not present).
        /// </summary>
        bool RemoveNode(NodeType id);

        /// <summary>
        /// 判断节点是否存在。<br/>Check whether a node exists in the graph.
        /// </summary>
        bool Exist(NodeType id);

        /// <summary>
        /// 添加带权边。对于不带权的实现，可以使用默认权重（例如 1）。<br/>Add an edge with weight. Unweighted implementations may use a default weight (e.g. 1).
        /// </summary>
        bool AddEdge(NodeType u, NodeType v, TWeight w, bool directed = false);

        /// <summary>
        /// 删除边。<br/>Remove an edge.
        /// </summary>
        bool RemoveEdge(NodeType u, NodeType v, bool directed = false);

        /// <summary>
        /// 获取节点的节点权重（可选，若图支持节点权重）。<br/>Get node's node-weight (optional for graphs that support node weights).
        /// </summary>
        TWeight GetNodeWeight(NodeType id);

        /// <summary>
        /// 设置节点的节点权重（可选）。<br/>Set node's node-weight (optional).
        /// </summary>
        void SetNodeWeight(NodeType id, TWeight weight);

        /// <summary>
        /// 节点的度（总度）。<br/>Degree of a node (total degree).
        /// </summary>
        int Degree(NodeType node);

        /// <summary>
        /// 节点的出度（定向图）。<br/>Out-degree of a node (for directed graphs).
        /// </summary>
        int OutDegree(NodeType node);

        /// <summary>
        /// 节点的入度（定向图）。<br/>In-degree of a node (for directed graphs).
        /// </summary>
        int InDegree(NodeType node);

        /// <summary>
        /// 检查图是否连通（语义由实现决定）。<br/>Check whether the graph is connected (semantics depend on implementation).
        /// </summary>
        bool IsConnected();
    }
}
