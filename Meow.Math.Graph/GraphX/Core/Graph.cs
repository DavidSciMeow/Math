using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace GraphX.Core
{
    /// <summary>
    /// ��Ȩͼʵ�֣�����ڲ� `GraphStorage` �ṩ�ڽ������ͼ������<br/>Weighted graph implementation that composes a GraphStorage for adjacency and basic graph operations.
    /// </summary>
    /// <typeparam name="NodeType">�ڵ����� / node type</typeparam>
    /// <typeparam name="TWeight">Ȩ������ / weight type</typeparam>
    public class Graph<NodeType, TWeight> : IGraph<NodeType, TWeight> 
        where NodeType : IEquatable<NodeType> 
        where TWeight : IComparable<TWeight>
    {
        private readonly GraphStorage<NodeType> _storage = new();
        private readonly Dictionary<NodeType, WeightedNode<NodeType, TWeight>> _wnodes = new();
        private readonly IWeightOperator<TWeight> _op;

        /// <summary>
        /// ʹ�ø�����Ȩ�ز����������Ȩͼʵ����<br/>Construct a weighted graph instance using the provided weight operator.
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
        /// �����Ȩ�ߵı�����أ�ʹ�ò������� One ��ΪĬ��Ȩ�أ���<br/>Convenience overload for adding an unweighted edge (uses operator.One as default weight).
        /// </summary>
        public bool AddEdge(NodeType u, NodeType v, bool directed = false)
        {
            // ������ת������Ȩ�ص� AddEdge��ʹ��Ĭ�� One ��ΪȨ�أ��� TWeight ��ת��ʱ����/ forward to weighted AddEdge using default One weight when TWeight can be cast
            if (typeof(TWeight) == typeof(long))
            {
                var w = (TWeight)(object)Convert.ToInt64(((IWeightOperator<long>)(object)_op).One);
                return AddEdge(u, v, w, directed);
            }
            // ����ͨ��������ʹ��Ĭ�� One�������ã���/ best-effort: attempt to use default 'One' through operator if available
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
                // ���˵��洢���ھӣ���ʹ��Ĭ��Ȩ�� / fall back to storage neighbors with default weight
                return _storage.GetNeighbors(u).Select(x => new KeyValuePair<NodeType, TWeight>(x, _op.One));
            }
            return _wnodes[u].Neighbors.ToList();
        }

        /// <summary>
        /// ��ݵ���Ȩ�ھӷ��ʣ�ֻ���ؽڵ��ʶ����<br/>Convenience method returning neighbor node ids only (no weights).
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

        // ���� IGraph ����ʽʵ�֣���Ȩ��ͼ�� / Explicit implementation for generic IGraph Edges (non-weighted view)
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


        // ���׵��� mermaid ��������֧�������и�ʽ / Very small mermaid-like parser: supports lines like
        // a--b��������Ȩ�� / a--b (undirected unweighted)
        // a-->b��������Ȩ�� / a-->b (directed unweighted)
        // a-->b:2��������Ȩ�� / a-->b:2 (directed weighted)
        // a--b:3��������Ȩ�� / a--b:3 (undirected weighted)
        // �ڵ�Ϊ�����հ׵��ַ������� # ��ͷ����Ϊע�͡� / Nodes are strings without whitespace. Lines starting with # are comments.
        public static Graph<string, long> Parse(string text, out string inferredType)
        {
            // Delegate to the generic parser specialization (NodeType = string, TWeight = long)
            var op = new LongWeightOperator();
            return Graph<string, long>.Parse(text, op, out inferredType, nodeParser: null, weightParser: null);
        }

        /// <summary>
        /// ͨ�� Parse�����ı�����Ϊ��ǰ���� Graph&lt;NodeType,TWeight&gt;�������û��ṩ��
        /// - nodeParser: ��δ��ַ������� NodeType����Ϊ null����� NodeType==string ��ֱ��ת���������Գ���ת�����ԣ�
        /// - weightParser: ��δ��ַ������� TWeight����Ϊ null����Գ������ͳ��� Parse / TypeConverter��
        /// - op: �����Ȩ�ز�����������Ĭ��Ȩ��ֵ��op.One��
        /// ����ͼ��ͨ�� out ���������ƶϵ����ͣ�DirectedWeighted / ...��
        /// </summary>
        public static Graph<NodeType, TWeight> Parse(string text,
            IWeightOperator<TWeight> op,
            out string inferredType,
            Func<string, NodeType>? nodeParser = null,
            Func<string, TWeight>? weightParser = null)
        {
            // helper: convert node token -> NodeType
            NodeType ParseNode(string token)
            {
                if (nodeParser != null) return nodeParser(token);
                // common fallback: NodeType == string
                if (typeof(NodeType) == typeof(string))
                {
                    return (NodeType)(object)token;
                }
                // try static Parse(string)
                var parseMi = typeof(NodeType).GetMethod("Parse", new[] { typeof(string) });
                if (parseMi != null && parseMi.IsStatic)
                {
                    var r = parseMi.Invoke(null, new object[] { token });
                    return (NodeType)r!;
                }
                // try TryParse(string, out NodeType)
                var tryParseMi = typeof(NodeType).GetMethod("TryParse", new[] { typeof(string), typeof(NodeType).MakeByRefType() });
                if (tryParseMi != null && tryParseMi.IsStatic)
                {
                    var args = new object[] { token, Activator.CreateInstance(typeof(NodeType))! };
                    var ok = (bool)tryParseMi.Invoke(null, args)!;
                    if (ok) return (NodeType)args[1];
                }
                // try TypeConverter
                var conv = TypeDescriptor.GetConverter(typeof(NodeType));
                if (conv != null && conv.CanConvertFrom(typeof(string)))
                {
                    var v = conv.ConvertFromInvariantString(token);
                    return (NodeType)v!;
                }
                throw new InvalidOperationException($"No nodeParser provided and cannot convert '{token}' to {typeof(NodeType).Name}. Provide a nodeParser.");
            }

            // helper: convert weight token -> TWeight
            TWeight ParseWeight(string token)
            {
                if (weightParser != null) return weightParser(token);
                // common fallback for long
                if (typeof(TWeight) == typeof(long))
                {
                    if (long.TryParse(token, out var lv)) return (TWeight)(object)lv;
                    throw new InvalidOperationException($"Cannot parse weight token '{token}' to {typeof(TWeight).Name}");
                }
                // try static Parse(string)
                var parseMi = typeof(TWeight).GetMethod("Parse", new[] { typeof(string) });
                if (parseMi != null && parseMi.IsStatic)
                {
                    var r = parseMi.Invoke(null, new object[] { token });
                    return (TWeight)r!;
                }
                // try TryParse
                var tryParseMi = typeof(TWeight).GetMethod("TryParse", new[] { typeof(string), typeof(TWeight).MakeByRefType() });
                if (tryParseMi != null && tryParseMi.IsStatic)
                {
                    var args = new object[] { token, Activator.CreateInstance(typeof(TWeight))! };
                    var ok = (bool)tryParseMi.Invoke(null, args)!;
                    if (ok) return (TWeight)args[1];
                }
                // TypeConverter
                var conv = TypeDescriptor.GetConverter(typeof(TWeight));
                if (conv != null && conv.CanConvertFrom(typeof(string)))
                {
                    var v = conv.ConvertFromInvariantString(token);
                    return (TWeight)v!;
                }
                throw new InvalidOperationException($"No weightParser provided and cannot convert '{token}' to {typeof(TWeight).Name}. Provide a weightParser.");
            }

            var g = new Graph<NodeType, TWeight>(op);
            bool anyDirected = false;
            bool anyWeight = false;

            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (line.Length == 0) continue;
                if (line.StartsWith("#")) continue;

                // detect pattern: find arrow or dash
                var directed = false;
                string[] parts = null;
                var weight = op.One;

                if (line.Contains("--"))
                {
                    parts = line.Split(new[] { "--" }, StringSplitOptions.None);
                    directed = false;
                }
                else if (line.Contains("->"))
                {
                    parts = line.Split(new[] { "->" }, StringSplitOptions.None);
                    directed = true;
                }
                else
                {
                    parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                }

                if (parts == null || parts.Length < 2) continue;
                string leftToken = parts[0].Trim();
                string rightToken = parts[1].Trim();

                // handle right having :weight suffix
                if (rightToken.Contains(":"))
                {
                    var idx = rightToken.IndexOf(":");
                    var nodePart = rightToken.Substring(0, idx);
                    var wstr = rightToken.Substring(idx + 1);
                    rightToken = nodePart;
                    // parse weight token into TWeight
                    try
                    {
                        weight = ParseWeight(wstr);
                        anyWeight = true;
                    }
                    catch
                    {
                        // couldn't parse weight: leave as op.One and don't mark anyWeight
                        weight = op.One;
                    }
                }

                // convert tokens to NodeType values
                var leftNode = ParseNode(leftToken);
                var rightNode = ParseNode(rightToken);

                if (!g.Exist(leftNode)) g.AddNode(leftNode);
                if (!g.Exist(rightNode)) g.AddNode(rightNode);

                if (directed) anyDirected = true;
                g.AddEdge(leftNode, rightNode, weight, directed);
            }

            // infer graph type
            if (anyDirected && anyWeight) inferredType = "DirectedWeighted";
            else if (anyDirected && !anyWeight) inferredType = "DirectedUnweighted";
            else if (!anyDirected && anyWeight) inferredType = "UndirectedWeighted";
            else inferredType = "UndirectedUnweighted";

            return g;
        }
    }
}
