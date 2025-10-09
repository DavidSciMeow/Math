using System;
using GraphX.Algorithms;
using GraphX.Error;
using GraphX.Core;
using GraphX.Graph;
using Xunit;

namespace GraphX.Tests
{
    public class GraphMethodPreconditionTests
    {
        private static LongWeightOperator Op => new();

        [Fact(DisplayName = "Dijkstra throws on negative edges")]
        public void Dijkstra_NegativeEdges_Throws()
        {
            var g = GraphTextParser.Parse("s->a:1\na->b:-3\n", out _);
            IGraph<string, long> ig = g;
            var ex = Assert.Throws<GraphMethodNotApplicableException>(() => ig.Dijkstra<string, long>("s", "b", Op, includeNodeWeight: false));
            Assert.Equal("Dijkstra", ex.MethodName);
            Assert.Contains("negative", ex.Reason, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("negativeEdges=true", ex.GraphSummary);
        }

        [Fact(DisplayName = "Kruskal throws on directed graph")]
        public void Kruskal_DirectedGraph_Throws()
        {
            var g = GraphTextParser.Parse("1->2:1\n2->3:1\n", out _);
            IGraph<string, long> ig = g;
            var ex = Assert.Throws<GraphMethodNotApplicableException>(() => ig.Kruskal<string, long>());
            Assert.Equal("Kruskal", ex.MethodName);
            Assert.Contains("directed", ex.GraphSummary);
        }

        [Fact(DisplayName = "Prim throws on directed graph")]
        public void Prim_DirectedGraph_Throws()
        {
            var g = GraphTextParser.Parse("1->2:1\n2->3:1\n", out _);
            IGraph<string, long> ig = g;
            var ex = Assert.Throws<GraphMethodNotApplicableException>(() => ig.Prim<string, long>("1"));
            Assert.Equal("Prim", ex.MethodName);
            Assert.Contains("directed", ex.GraphSummary);
        }

        [Fact(DisplayName = "TopologicalSort throws on undirected graph")]
        public void Topological_UndirectedGraph_Throws()
        {
            var g = GraphTextParser.Parse("a--b\nb--c\n", out _);
            IGraph<string, long> ig = g;
            var ex = Assert.Throws<GraphMethodNotApplicableException>(() => ig.TopologicalSort<string, long>());
            Assert.Equal("TopologicalSort", ex.MethodName);
            Assert.Contains("directed", ex.GraphSummary);
        }

        [Fact(DisplayName = "EdmondsKarp throws on undirected graph")]
        public void EdmondsKarp_UndirectedGraph_Throws()
        {
            var g = GraphTextParser.Parse("s--t:5\n", out _);
            IGraph<string, long> ig = g;
            var ex = Assert.Throws<GraphMethodNotApplicableException>(() => ig.EdmondsKarpMaxFlow<string>("s", "t"));
            Assert.Equal("EdmondsKarpMaxFlow", ex.MethodName);
            Assert.Contains("directed", ex.GraphSummary);
        }

        [Fact(DisplayName = "Dinic throws on undirected graph")]
        public void Dinic_UndirectedGraph_Throws()
        {
            var g = GraphTextParser.Parse("s--t:5\n", out _);
            IGraph<string, long> ig = g;
            var ex = Assert.Throws<GraphMethodNotApplicableException>(() => ig.DinicMaxFlow<string>("s", "t"));
            Assert.Equal("DinicMaxFlow", ex.MethodName);
            Assert.Contains("directed", ex.GraphSummary);
        }
    }
}
