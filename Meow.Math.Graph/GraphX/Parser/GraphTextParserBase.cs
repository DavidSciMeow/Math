using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using GraphX.Core;

namespace GraphX.Parser
{
    /// <summary>
    /// ��������������ÿ����������ȡ���ƣ���ڵ㡢�ҽڵ㡢��ѡȨ�����ơ������־����<br/>
    /// Subclasses ʵ�� <see cref="TryParseTokens"/> ��֧�������ʽ���� mermaid-like��CSV������ JSON��XML Ƭ�εȣ���<br/>
    /// ���ฺ������ת��Ϊ NodeType / TWeight ������ͼ��<br/>
    /// Base parser that extracts tokens (left node, right node, optional weight token, directed flag)<br/>
    /// from each input line. Subclasses implement <see cref="TryParseTokens"/> to support arbitrary<br/>
    /// formats (mermaid-like, CSV, JSON-per-line, XML fragment, etc.).<br/>
    /// The base class handles token��NodeType / token��TWeight conversion and graph construction.
    /// </summary>
    public abstract class GraphTextParserBase<NodeType, TWeight>
        where NodeType : IEquatable<NodeType>
        where TWeight : IComparable<TWeight>
    {
        /// <summary>
        /// ���Խ����зǿ��������Ϊ���ơ�<br/>
        /// - ���� true ʱ��leftToken �� rightToken ����Ϊ�ǿա�<br/>
        /// - weightToken ��Ϊ null����ʾʹ�� op.One����<br/>
        /// - directed ָʾ�ñ��Ƿ�Ϊ����<br/>
        /// �����а�����Ч����ʱ���� true�����򷵻� false�����Ը��У���<br/>
        /// Attempt to parse a single non-empty input line into tokens.<br/>
        /// - leftToken and rightToken must be non-null when returning true.<br/>
        /// - weightToken may be null (means use op.One).<br/>
        /// - directed indicates whether the edge is directed.<br/>
        /// Return true when the line contains a valid connection; false to ignore the line.
        /// </summary>
        protected abstract bool TryParseTokens(string line, out string leftToken, out string rightToken, out string? weightToken, out bool directed);

        /// <summary>
        /// �߼� Parse��ͨ����ÿһ�е��� TryParseTokens ���������ͻ��� Graph��<br/>
        /// ���ṩ��ѡ�Ľڵ�/Ȩ������ת��������δ�ṩ���Գ����Ļ��˲��ԡ�<br/>
        /// High-level Parse: builds a typed Graph from text by calling TryParseTokens for each line.<br/>
        /// Provide optional node/weight token converters; when omitted the method attempts common fallbacks.
        /// </summary>
        public virtual Graph<NodeType, TWeight> Parse(string text,
            IWeightOperator<TWeight> op,
            out string inferredType,
            Func<string, NodeType>? nodeParser = null,
            Func<string, TWeight>? weightParser = null)
        {
            if (op == null) throw new ArgumentNullException(nameof(op));
            inferredType = "Unknown";

            // token -> NodeType conversion (fallbacks similar to existing code)
            NodeType ConvertNode(string token)
            {
                if (nodeParser != null) return nodeParser(token);
                if (typeof(NodeType) == typeof(string)) return (NodeType)(object)token;

                var parseMi = typeof(NodeType).GetMethod("Parse", new[] { typeof(string) });
                if (parseMi != null && parseMi.IsStatic) return (NodeType)parseMi.Invoke(null, new object[] { token })!;

                var tryParseMi = typeof(NodeType).GetMethod("TryParse", new[] { typeof(string), typeof(NodeType).MakeByRefType() });
                if (tryParseMi != null && tryParseMi.IsStatic)
                {
                    var args = new object[] { token, Activator.CreateInstance(typeof(NodeType))! };
                    var ok = (bool)tryParseMi.Invoke(null, args)!;
                    if (ok) return (NodeType)args[1];
                }

                var conv = TypeDescriptor.GetConverter(typeof(NodeType));
                if (conv != null && conv.CanConvertFrom(typeof(string))) return (NodeType)conv.ConvertFromInvariantString(token)!;

                throw new InvalidOperationException($"Cannot convert node token '{token}' to {typeof(NodeType).Name}. Provide nodeParser.");
            }

            // token -> TWeight conversion
            TWeight ConvertWeight(string token)
            {
                if (weightParser != null) return weightParser(token);
                if (typeof(TWeight) == typeof(long))
                {
                    if (long.TryParse(token, out var lv)) return (TWeight)(object)lv;
                    throw new InvalidOperationException($"Cannot parse weight token '{token}' to {typeof(TWeight).Name}");
                }

                var parseMi = typeof(TWeight).GetMethod("Parse", new[] { typeof(string) });
                if (parseMi != null && parseMi.IsStatic) return (TWeight)parseMi.Invoke(null, new object[] { token })!;

                var tryParseMi = typeof(TWeight).GetMethod("TryParse", new[] { typeof(string), typeof(TWeight).MakeByRefType() });
                if (tryParseMi != null && tryParseMi.IsStatic)
                {
                    var args = new object[] { token, Activator.CreateInstance(typeof(TWeight))! };
                    var ok = (bool)tryParseMi.Invoke(null, args)!;
                    if (ok) return (TWeight)args[1];
                }

                var conv = TypeDescriptor.GetConverter(typeof(TWeight));
                if (conv != null && conv.CanConvertFrom(typeof(string))) return (TWeight)conv.ConvertFromInvariantString(token)!;

                throw new InvalidOperationException($"Cannot convert weight token '{token}' to {typeof(TWeight).Name}. Provide weightParser.");
            }

            var g = new Graph<NodeType, TWeight>(op);
            bool anyDirected = false;
            bool anyWeight = false;

            var lines = (text ?? string.Empty).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (line.Length == 0) continue;
                if (line.StartsWith("#")) continue;

                if (!TryParseTokens(line, out var leftTok, out var rightTok, out var wTok, out var directed))
                    continue;

                // convert tokens
                NodeType leftNode = ConvertNode(leftTok);
                NodeType rightNode = ConvertNode(rightTok);

                TWeight weight = op.One;
                if (!string.IsNullOrEmpty(wTok))
                {
                    try
                    {
                        weight = ConvertWeight(wTok);
                        anyWeight = true;
                    }
                    catch
                    {
                        // leave weight as op.One when conversion fails
                        weight = op.One;
                    }
                }

                if (!g.Exist(leftNode)) g.AddNode(leftNode);
                if (!g.Exist(rightNode)) g.AddNode(rightNode);

                if (directed) anyDirected = true;
                g.AddEdge(leftNode, rightNode, weight, directed);
            }

            if (anyDirected && anyWeight) inferredType = "DirectedWeighted";
            else if (anyDirected && !anyWeight) inferredType = "DirectedUnweighted";
            else if (!anyDirected && anyWeight) inferredType = "UndirectedWeighted";
            else inferredType = "UndirectedUnweighted";

            return g;
        }
    }
}