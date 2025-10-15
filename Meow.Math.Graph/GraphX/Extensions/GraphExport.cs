using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GraphX.Core;

namespace GraphX.Extensions
{
    /// <summary>
    /// ���� / ��ӡ GraphX ͼ����չ������֧�� mermaid ����������̨��ӡ��д���ļ���
    /// - Ĭ�Ϸ���: LR
    /// - Ĭ����ʾȨ��: true
    /// - ֧�ָ���ָ���ڵ��ߣ�Ĭ�Ͽ����ã�
    /// </summary>
    public static class GraphExport
    {
        private static string NodeId<T>(T node, int index)
        {
            // ���ɼ����ȶ��� id�����������ַ���ͻ
            return "n" + index.ToString();
        }

        /// <summary>
        /// ��ͼ����Ϊ mermaid �ı���
        /// </summary>
        public static string ToMermaid<TNode, TWeight>(this IGraph<TNode, TWeight> g,
            bool includeWeights = true,
            string direction = "LR",
            IEnumerable<TNode>? highlightNodes = null,
            IEnumerable<(TNode U, TNode V)>? highlightEdges = null)
            where TNode : IEquatable<TNode>
            where TWeight : IComparable<TWeight>
        {
            var sb = new StringBuilder();
            sb.AppendLine($"graph {direction}");

            // map nodes to simple ids
            var nodeList = g.Nodes.ToList();
            var nodeToId = new Dictionary<TNode, string>();
            for (int i = 0; i < nodeList.Count; i++) nodeToId[nodeList[i]] = NodeId(nodeList[i], i);

            // render nodes with labels
            foreach (var kv in nodeToId)
            {
                var label = kv.Key?.ToString()?.Replace("\"", "\\\"") ?? string.Empty;
                sb.AppendLine($"{kv.Value}[\"{label}\"]");
            }

            // prepare highlights
            var highlightNodeIds = new HashSet<string>();
            if (highlightNodes != null)
            {
                foreach (var n in highlightNodes)
                {
                    if (nodeToId.TryGetValue(n, out var id)) highlightNodeIds.Add(id);
                }
            }

            var highlightEdgePairs = new HashSet<string>();
            if (highlightEdges != null)
            {
                foreach (var e in highlightEdges)
                {
                    if (nodeToId.TryGetValue(e.U, out var a) && nodeToId.TryGetValue(e.V, out var b))
                    {
                        highlightEdgePairs.Add(a + "->" + b);
                    }
                }
            }

            // render edges and collect indices for linkStyle
            var edges = g.WeightedEdges.ToList();
            int eidx = 0;
            var highlightedEdgeIndices = new List<int>();
            foreach (var e in edges)
            {
                if (!nodeToId.TryGetValue(e.U, out var a) || !nodeToId.TryGetValue(e.V, out var b)) { eidx++; continue; }
                var arrow = g.IsDirected ? "-->" : "---";
                var label = includeWeights ? $"|{e.W}|" : string.Empty;
                sb.AppendLine($"{a} {arrow}{label} {b}");
                if (highlightEdgePairs.Contains(a + "->" + b)) highlightedEdgeIndices.Add(eidx);
                eidx++;
            }

            // node highlight styles
            if (highlightNodeIds.Count > 0)
            {
                sb.AppendLine("%% node highlight styles");
                foreach (var nid in highlightNodeIds)
                {
                    sb.AppendLine($"style {nid} fill:#ffefef,stroke:#cc3333,stroke-width:2px");
                }
            }

            // edge highlight styles using linkStyle indices
            if (highlightedEdgeIndices.Count > 0)
            {
                sb.AppendLine("%% edge highlight styles");
                foreach (var idx in highlightedEdgeIndices)
                {
                    sb.AppendLine($"linkStyle {idx} stroke:#ff3333,stroke-width:4px");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// �� PathResult ����Ϊ mermaid �ı���������·���ϵĽڵ���˳��ߣ�������ע���а����ɴ����������Ϣ��
        /// </summary>
        public static string ToMermaid<TNode, TWeight>(this PathResult<TNode, TWeight> path,
            bool includeCost = true,
            string direction = "LR")
            where TNode : IEquatable<TNode>
            where TWeight : IComparable<TWeight>
        {
            var sb = new StringBuilder();
            sb.AppendLine($"graph {direction}");
            var nodes = path.Nodes ?? new List<TNode>();
            for (int i = 0; i < nodes.Count; i++)
            {
                var label = nodes[i]?.ToString()?.Replace("\"", "\\\"") ?? string.Empty;
                sb.AppendLine($"n{i}[\"{label}\"]");
            }
            for (int i = 0; i + 1 < nodes.Count; i++)
            {
                sb.AppendLine($"n{i} --> n{i + 1}");
            }
            if (includeCost)
            {
                sb.AppendLine($"%% Path reachable: {path.Reachable}, Cost: {path.Cost}");
            }
            else
            {
                sb.AppendLine($"%% Path reachable: {path.Reachable}");
            }
            return sb.ToString();
        }

        /// <summary>
        /// ��ӡ mermaid �ı�������̨����ݷ�������
        /// </summary>
        public static void PrintMermaidToConsole<TNode, TWeight>(this IGraph<TNode, TWeight> g,
            bool includeWeights = true,
            string direction = "LR",
            IEnumerable<TNode>? highlightNodes = null,
            IEnumerable<(TNode U, TNode V)>? highlightEdges = null)
            where TNode : IEquatable<TNode>
            where TWeight : IComparable<TWeight>
        {
            var txt = g.ToMermaid(includeWeights, direction, highlightNodes, highlightEdges);
            Console.WriteLine(txt);
        }

        /// <summary>
        /// �� mermaid �ı�д���ļ���
        /// </summary>
        public static void WriteMermaidToFile<TNode, TWeight>(this IGraph<TNode, TWeight> g,
            string filePath,
            bool includeWeights = true,
            string direction = "LR",
            IEnumerable<TNode>? highlightNodes = null,
            IEnumerable<(TNode U, TNode V)>? highlightEdges = null)
            where TNode : IEquatable<TNode>
            where TWeight : IComparable<TWeight>
        {
            var txt = g.ToMermaid(includeWeights, direction, highlightNodes, highlightEdges);
            File.WriteAllText(filePath, txt, Encoding.UTF8);
        }

        /// <summary>
        /// ��ӡ PathResult �� mermaid ������̨��
        /// </summary>
        public static void PrintMermaidToConsole<TNode, TWeight>(this PathResult<TNode, TWeight> path,
            bool includeCost = true,
            string direction = "LR")
            where TNode : IEquatable<TNode>
            where TWeight : IComparable<TWeight>
        {
            Console.WriteLine(path.ToMermaid(includeCost, direction));
        }

        /// <summary>
        /// �� PathResult �� mermaid д���ļ���
        /// </summary>
        public static void WriteMermaidToFile<TNode, TWeight>(this PathResult<TNode, TWeight> path,
            string filePath,
            bool includeCost = true,
            string direction = "LR")
            where TNode : IEquatable<TNode>
            where TWeight : IComparable<TWeight>
        {
            var txt = path.ToMermaid(includeCost, direction);
            File.WriteAllText(filePath, txt, Encoding.UTF8);
        }
    }
}
