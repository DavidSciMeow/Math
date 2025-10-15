using System;

namespace GraphX.Parser
{
    /// <summary>
    /// Ĭ��ʵ�֣����������﷨��<br/>
    /// - a--b       ������Ȩ<br/>
    /// - a-->b      ������Ȩ<br/>
    /// - a-->b:2    ������Ȩ<br/>
    /// - a--b:3     ������Ȩ<br/>
    /// ����չ������֧�� CSV / JSON / XML �ȸ�ʽ��<br/>
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

            // ����ʹ�ü�ͷ/���ַ���ǣ���Ϊ����ǰ��ͬ / prefer arrow/dash markers, mirror previous behavior
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

            // �����Ҳ�Ȩ�غ�׺: node:weight / handle weight suffix in right token: node:weight
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