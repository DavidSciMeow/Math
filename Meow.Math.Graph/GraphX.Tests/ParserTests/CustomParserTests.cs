using System;
using GraphX.Core;
using GraphX.Parser;
using Xunit;

namespace GraphX.Tests.ParserTests
{
    public class CustomParserTests
    {
        private static LongWeightOperator Op => new();

        [Fact]
        public void DefaultTextParser_BasicParse_Works()
        {
            var text = "# comment\nA->B:3\nB--C:2\nD E\n";
            var parser = new DefaultTextParser<string, long>();
            var g = parser.Parse(text, Op, out var t);

            Assert.Equal("DirectedWeighted", t); // contains directed and weighted lines
            Assert.True(g.Exist("A"));
            Assert.True(g.Exist("B"));
            Assert.True(g.Exist("C"));
            Assert.True(g.Exist("D"));
            Assert.True(g.Exist("E"));

            // check edges and weights
            var edges = g.WeightedEdges;
            Assert.Contains(edges, e => e.U == "A" && e.V == "B" && e.W == 3);
            Assert.Contains(edges, e => (e.U == "B" && e.V == "C" || e.U == "C" && e.V == "B") && e.W == 2);
        }

        [Fact]
        public void CustomLineParser_ParseJsonLikeLines()
        {
            // simple JSON-per-line format: {"u":"X","v":"Y","w":5,"d":true}
            var lines = new[] {
                "{\"u\":\"x\",\"v\":\"y\",\"w\":10,\"d\":true}",
                "{\"u\":\"y\",\"v\":\"z\",\"w\":4,\"d\":false}"
            };
            var txt = string.Join("\n", lines);

            var parser = new JsonLineParser<string, long>();
            var g = parser.Parse(txt, Op, out var t);
            Assert.Equal("DirectedWeighted", t);
            Assert.True(g.Exist("x"));
            Assert.True(g.Exist("y"));
            Assert.True(g.Exist("z"));
            Assert.Contains(g.WeightedEdges, e => e.U == "x" && e.V == "y" && e.W == 10);
            Assert.Contains(g.WeightedEdges, e => (e.U == "y" && e.V == "z" || e.U == "z" && e.V == "y") && e.W == 4);
        }
    }

    // simple demonstration parser for JSON-like per-line objects
    internal class JsonLineParser<NodeType, TWeight> : GraphTextParserBase<NodeType, TWeight>
        where NodeType : IEquatable<NodeType>
        where TWeight : IComparable<TWeight>
    {
        protected override bool TryParseTokens(string line, out string leftToken, out string rightToken, out string? weightToken, out bool directed)
        {
            leftToken = string.Empty; rightToken = string.Empty; weightToken = null; directed = false;
            if (string.IsNullOrWhiteSpace(line)) return false;
            line = line.Trim();
            // very small JSON-ish parser (not robust) just for tests
            try
            {
                // expects keys: u, v, w, d
                var uIdx = line.IndexOf("\"u\"");
                var vIdx = line.IndexOf("\"v\"");
                if (uIdx < 0 || vIdx < 0) return false;
                string ReadVal(int idx)
                {
                    var colon = line.IndexOf(':', idx);
                    var comma = line.IndexOf(',', colon);
                    if (comma < 0) comma = line.IndexOf('}', colon);
                    var raw = line.Substring(colon + 1, comma - colon - 1).Trim().Trim('"');
                    return raw;
                }
                leftToken = ReadVal(uIdx);
                rightToken = ReadVal(vIdx);
                var wIdx = line.IndexOf("\"w\"");
                if (wIdx >= 0) weightToken = ReadVal(wIdx);
                var dIdx = line.IndexOf("\"d\"");
                if (dIdx >= 0)
                {
                    var dv = ReadVal(dIdx);
                    directed = dv.Equals("true", StringComparison.OrdinalIgnoreCase);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
