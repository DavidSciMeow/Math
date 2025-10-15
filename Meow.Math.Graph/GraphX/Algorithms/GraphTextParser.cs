using System;
using GraphX.Core;
using GraphX.Parser;

namespace GraphX.Algorithms
{
    /// <summary>
    /// ���� shim�������ɵ� GraphTextParser.Parse API��ί�е� Parser/DefaultTextParser��<br/>
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
