using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphX.Core
{
    public static class GraphTextParser
    {
        // 简易的类 mermaid 解析器：支持如下行格式 / Very small mermaid-like parser: supports lines like
        // a--b（无向无权） / a--b (undirected unweighted)
        // a-->b（有向无权） / a-->b (directed unweighted)
        // a-->b:2（有向有权） / a-->b:2 (directed weighted)
        // a--b:3（无向有权） / a--b:3 (undirected weighted)
        // 节点为不含空白的字符串。以 # 开头的行为注释。 / Nodes are strings without whitespace. Lines starting with # are comments.
        public static Graph<string, long> Parse(string text, out string inferredType)
        {
            var op = new LongWeightOperator();
            var g = new Graph<string, long>(op);
            bool anyDirected = false;
            bool anyWeight = false;

            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (line.Length == 0) continue;
                if (line.StartsWith("#")) continue;

                // 检测模式：查找箭头或连字符 / detect pattern: find arrow or dash
                var directed = false;
                string[] parts = null;
                long weight = op.One;

                if (line.Contains("--"))
                {
                    // 可能是 "a--b" 或 "a--b:3" / could be "a--b" or "a--b:3"
                    parts = line.Split(new[] { "--" }, StringSplitOptions.None);
                    directed = false;
                }
                else if (line.Contains("->"))
                {
                    // 将 -> 视为有向 / treat -> as directed
                    parts = line.Split(new[] { "->" }, StringSplitOptions.None);
                    directed = true;
                }
                else
                {
                    // 回退为空白分割 / fallback whitespace split
                    parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                }

                if (parts == null || parts.Length < 2) continue;
                // parts[1] 可能包含 ":weight" 后缀，如 "b:3" / parts[1] may contain ":weight" suffix like "b:3"
                string left = parts[0].Trim();
                string right = parts[1].Trim();
                // 处理右侧的 :weight / handle right having :weight
                if (right.Contains(":"))
                {
                    var idx = right.IndexOf(":");
                    var node = right.Substring(0, idx);
                    var wstr = right.Substring(idx + 1);
                    right = node;
                    if (long.TryParse(wstr, out var wv)) { weight = wv; anyWeight = true; }
                }
                // 确保节点存在 / ensure nodes exist
                if (!g.Exist(left)) g.AddNode(left);
                if (!g.Exist(right)) g.AddNode(right);

                if (directed) anyDirected = true;
                g.AddEdge(left, right, weight, directed);
            }

            // 推断图类型 / infer graph type
            if (anyDirected && anyWeight) inferredType = "DirectedWeighted";
            else if (anyDirected && !anyWeight) inferredType = "DirectedUnweighted";
            else if (!anyDirected && anyWeight) inferredType = "UndirectedWeighted";
            else inferredType = "UndirectedUnweighted";

            return g;
        }

        // 解析并返回便捷类型实例（如 UndirectedUnweightedGraph, DirectedWeightedGraph, ...）/ Parse and return a convenience typed instance (UndirectedUnweightedGraph, DirectedWeightedGraph, ...)
        public static (object Graph, string InferredType) ParseToConvenience(string text)
        {
            var baseG = Parse(text, out var inferred);
            // 创建对应的便捷图并复制节点与边 / create appropriate convenience instance and copy nodes/edges
            if (inferred == "DirectedWeighted")
            {
                var dg = new Graph.DirectedWeightedGraph();
                foreach (var v in baseG.Nodes) dg.AddNode(v);
                foreach (var e in baseG.WeightedEdges) dg.AddEdge(e.U, e.V, e.W, true);
                return (dg, inferred);
            }
            else if (inferred == "DirectedUnweighted")
            {
                var dg = new Graph.DirectedUnweightedGraph();
                foreach (var v in baseG.Nodes) dg.AddNode(v);
                foreach (var e in baseG.WeightedEdges) dg.AddEdge(e.U, e.V); // 有向无权 / unweighted directed
                return (dg, inferred);
            }
            else if (inferred == "UndirectedWeighted")
            {
                var ug = new Graph.UndirectedWeightedGraph();
                foreach (var v in baseG.Nodes) ug.AddNode(v);
                foreach (var e in baseG.WeightedEdges) ug.AddEdge(e.U, e.V, e.W, false);
                return (ug, inferred);
            }
            else // UndirectedUnweighted
            {
                var ug = new Graph.UndirectedUnweightedGraph();
                foreach (var v in baseG.Nodes) ug.AddNode(v);
                foreach (var e in baseG.WeightedEdges) ug.AddEdge(e.U, e.V, false);
                return (ug, inferred);
            }
        }
    }
}
