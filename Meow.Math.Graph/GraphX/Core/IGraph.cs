using System;
using System.Collections.Generic;

namespace GraphX.Core
{
    /// <summary>
    /// ͳһ��ͼ�ӿڣ���������Ȩ�صĳ���ͼ���������ԡ�<br/>Unified graph interface describing common graph operations and properties including weights.
    /// </summary>
    /// <typeparam name="NodeType">�ڵ����� / node type</typeparam>
    /// <typeparam name="TWeight">Ȩ������ / weight type</typeparam>
    public interface IGraph<NodeType, TWeight> 
        where NodeType : IEquatable<NodeType>
        where TWeight : IComparable<TWeight>
    {
        /// <summary>
        /// �Ƿ�Ϊ����ͼ��<br/>Whether the graph is directed.
        /// </summary>
        bool IsDirected { get; }

        /// <summary>
        /// ͼ�нڵ�������<br/>Number of nodes in the graph.
        /// </summary>
        int NodeCount { get; }

        /// <summary>
        /// ͼ�б����������洢��ʽ��������<br/>Number of edges in the graph (as stored).
        /// </summary>
        int EdgeCount { get; }

        /// <summary>
        /// ö�����нڵ㡣<br/>Enumerate all nodes.
        /// </summary>
        IEnumerable<NodeType> Nodes { get; }

        /// <summary>
        /// ���ھӷ����������ش�Ȩ�ھӶ� (neighbor, weight)������ȨͼȨ�ؿ���ΪĬ�ϵ� "one"��<br/>Primary neighbor accessor: returns weighted neighbor pairs (neighbor, weight). For unweighted graphs weight may be the default "one".
        /// </summary>
        IEnumerable<KeyValuePair<NodeType, TWeight>> GetNeighbors(NodeType u);

        /// <summary>
        /// ��Ȩ��ö�٣�������ʽ�����洢���򣩡�<br/>Enumerate weighted edges (primary form, directed as stored).
        /// </summary>
        IEnumerable<(NodeType U, NodeType V, TWeight W)> WeightedEdges { get; }

        /// <summary>
        /// �����ԣ�����Ȩ�صı��б�<br/>Compatibility: edge list without weights.
        /// </summary>
        IEnumerable<(NodeType U, NodeType V)> Edges { get; }

        /// <summary>
        /// ��ӽڵ㣨�������򷵻� false����<br/>Add a node (returns false when already present).
        /// </summary>
        bool AddNode(NodeType id);

        /// <summary>
        /// ɾ���ڵ㣨���������򷵻� false����<br/>Remove a node (returns false when not present).
        /// </summary>
        bool RemoveNode(NodeType id);

        /// <summary>
        /// �жϽڵ��Ƿ���ڡ�<br/>Check whether a node exists in the graph.
        /// </summary>
        bool Exist(NodeType id);

        /// <summary>
        /// ��Ӵ�Ȩ�ߡ����ڲ���Ȩ��ʵ�֣�����ʹ��Ĭ��Ȩ�أ����� 1����<br/>Add an edge with weight. Unweighted implementations may use a default weight (e.g. 1).
        /// </summary>
        bool AddEdge(NodeType u, NodeType v, TWeight w, bool directed = false);

        /// <summary>
        /// ɾ���ߡ�<br/>Remove an edge.
        /// </summary>
        bool RemoveEdge(NodeType u, NodeType v, bool directed = false);

        /// <summary>
        /// ��ȡ�ڵ�Ľڵ�Ȩ�أ���ѡ����ͼ֧�ֽڵ�Ȩ�أ���<br/>Get node's node-weight (optional for graphs that support node weights).
        /// </summary>
        TWeight GetNodeWeight(NodeType id);

        /// <summary>
        /// ���ýڵ�Ľڵ�Ȩ�أ���ѡ����<br/>Set node's node-weight (optional).
        /// </summary>
        void SetNodeWeight(NodeType id, TWeight weight);

        /// <summary>
        /// �ڵ�Ķȣ��ܶȣ���<br/>Degree of a node (total degree).
        /// </summary>
        int Degree(NodeType node);

        /// <summary>
        /// �ڵ�ĳ��ȣ�����ͼ����<br/>Out-degree of a node (for directed graphs).
        /// </summary>
        int OutDegree(NodeType node);

        /// <summary>
        /// �ڵ����ȣ�����ͼ����<br/>In-degree of a node (for directed graphs).
        /// </summary>
        int InDegree(NodeType node);

        /// <summary>
        /// ���ͼ�Ƿ���ͨ��������ʵ�־�������<br/>Check whether the graph is connected (semantics depend on implementation).
        /// </summary>
        bool IsConnected();
    }
}
