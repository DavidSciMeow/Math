using System;

namespace GraphX.Parser
{
    /// <summary>
    /// 默认实现：理解简单连接语法：<br/>
    /// - a--b       无向，无权<br/>
    /// - a-->b      有向，无权<br/>
    /// - a-->b:2    有向，有权<br/>
    /// - a--b:3     无向，有权<br/>
    /// 可扩展子类以支持 CSV / JSON / XML 等格式。<br/>
    /// Default implementation that understands simple connection syntax:<br/>
    /// - a--b       undirected, unweighted<br/>
    /// - a-->b      directed, unweighted<br/>
    /// - a-->b:2    directed, weighted<br/>
    /// - a--b:3     undirected, weighted<br/>
    /// Subclasses can be created for CSV / JSON / XML etc.
    /// </summary>
    public class DefaultTextParser<NodeType, TWeight> : GraphTextParserBase<NodeType, TWeight>
        where NodeType : IEquatable<NodeType>
        where TWeight : IComparable<TWeight>
    {
        protected override bool TryParseTokens(string line, out string leftToken, out string rightToken, out string? weightToken, out bool directed)
        {
            leftToken = string.Empty;
            rightToken = string.Empty;
            weightToken = null;
            directed = false;

            if (string.IsNullOrWhiteSpace(line)) return false;

            // 优先使用箭头/连字符标记，行为与以前相同 / prefer arrow/dash markers, mirror previous behavior
            if (line.Contains("--"))
            {
                var parts = line.Split(new[] { "--" }, StringSplitOptions.None);
                if (parts.Length < 2) return false;
                leftToken = parts[0].Trim();
                rightToken = parts[1].Trim();
                directed = false;
            }
            else if (line.Contains("->"))
            {
                var parts = line.Split(new[] { "->" }, StringSplitOptions.None);
                if (parts.Length < 2) return false;
                leftToken = parts[0].Trim();
                rightToken = parts[1].Trim();
                directed = true;
            }
            else
            {
                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) return false;
                leftToken = parts[0].Trim();
                rightToken = parts[1].Trim();
                directed = false;
            }

            // 处理右侧权重后缀: node:weight / handle weight suffix in right token: node:weight
            if (rightToken.Contains(":"))
            {
                var idx = rightToken.IndexOf(":");
                weightToken = rightToken.Substring(idx + 1).Trim();
                rightToken = rightToken.Substring(0, idx).Trim();
            }

            return !string.IsNullOrEmpty(leftToken) && !string.IsNullOrEmpty(rightToken);
        }
    }
}