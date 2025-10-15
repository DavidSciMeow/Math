using System;
using GraphX.Core;
using GraphX.Parser;

namespace GraphX.Algorithms
{
    /// <summary>
    /// 兼容 shim：保留旧的 GraphTextParser.Parse API，委托到 Parser/DefaultTextParser。<br/>
    /// Compatibility shim: keep the old GraphTextParser.Parse API and delegate to Parser/DefaultTextParser.
    /// </summary>
    public static class GraphTextParser
    {
        public static Graph<string, long> Parse(string text, out string inferredType)
        {
            var parser = new DefaultTextParser<string, long>();
            return parser.Parse(text, new LongWeightOperator(), out inferredType);
        }
    }
}
