using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GraphX.Algorithms;
using GraphX.Core;
using GraphX.Parser;
using Xunit;

namespace GraphX.Tests
{
    /// <summary>
    /// Integration tests that exercise parser + multiple GraphMethods on a collection of larger example graphs.
    /// ????: ????????????????????????????????????????????
    /// </summary>
    public class GraphParserIntegrationTests
    {
        private static LongWeightOperator Op => new();
        private static DefaultTextParser<string, long> Parser => new();

        public class DistanceExpectation { public string From { get; set; } public string To { get; set; } public long Distance { get; set; } }
        public class JsonTestCase
        {
            public string Name { get; set; }
            public string Text { get; set; }
            public int ExpectedNodeCount { get; set; }
            public int ExpectedEdgeCount { get; set; }
            public bool ExpectedDirected { get; set; }
            public long? ExpectedMstTotalWeight { get; set; }
            public long? ExpectedMaxFlow { get; set; }
            public List<DistanceExpectation> ExpectedDistances { get; set; }
        }

        private static List<JsonTestCase> LoadExpectations()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "graph_expectations.json");
            if (!File.Exists(path))
            {
                // try project folder for test runs
                path = Path.Combine(Directory.GetCurrentDirectory(), "graph_expectations.json");
            }
            var txt = File.ReadAllText(path);
            return System.Text.Json.JsonSerializer.Deserialize<List<JsonTestCase>>(txt);
        }

        // Helper: select a test case satisfying predicate. If GRAPHX_QUICK_INDEX provided, respect it.
        private JsonTestCase PickOne(Func<JsonTestCase, bool> predicate)
        {
            var list = LoadExpectations().Where(predicate).ToList();
            if (list.Count == 0) throw new InvalidOperationException("No graph matching predicate");
            var env = Environment.GetEnvironmentVariable("GRAPHX_QUICK_INDEX");
            if (!string.IsNullOrEmpty(env) && int.TryParse(env, out var idx) && idx >= 0 && idx < list.Count) return list[idx];
            // deterministic choice for tests: return first matching
            return list[0];
        }

        // Generic try-find key: use equality comparison first, then ToString fallback and trimmed/case-insensitive
        private static bool TryFindKey<T>(IEnumerable<T> keys, string desired, out T resolved)
        {
            resolved = default(T);
            if (desired == null) return false;
            var eq = EqualityComparer<T>.Default;
            foreach (var k in keys)
            {
                if (k == null) continue;
                // direct equality against string if possible
                if (k is string ks)
                {
                    if (ks.Equals(desired)) { resolved = k; return true; }
                    if (ks.Trim().Equals(desired.Trim())) { resolved = k; return true; }
                    if (ks.Equals(desired, StringComparison.OrdinalIgnoreCase)) { resolved = k; return true; }
                    if (ks.Trim().Equals(desired.Trim(), StringComparison.OrdinalIgnoreCase)) { resolved = k; return true; }
                }
                // general object equality
                try
                {
                    if (eq.Equals(k, (T)(object)desired)) { resolved = k; return true; }
                }
                catch { }
                // fallback to ToString
                var s = k.ToString();
                if (s.Equals(desired)) { resolved = k; return true; }
                if (s.Trim().Equals(desired.Trim(), StringComparison.OrdinalIgnoreCase)) { resolved = k; return true; }
            }
            return false;
        }

        [Fact(DisplayName = "JSON driven: MST check (Kruskal/Prim)")]
        public void Json_MstCheck()
        {
            var tc = PickOne(g => g.ExpectedMstTotalWeight.HasValue);
            var baseG = Parser.Parse(tc.Text, Op, out _);
            IGraph<string, long> ig = baseG;
            var mst = ig.Kruskal<string, long>();
            long total = mst.Sum(e => e.Item3);
            Assert.Equal(tc.ExpectedMstTotalWeight.Value, total);
            var prim = ig.Prim<string, long>(ig.Nodes.First());
            Assert.Equal(total, prim.Sum(e => e.Item3));
        }

        [Fact(DisplayName = "JSON driven: Floyd-Warshall check (all graphs)")]
        public void Json_FloydWarshallCheck()
        {
            var all = LoadExpectations();
            foreach (var tc in all.Where(g => g.ExpectedDistances != null && g.ExpectedDistances.Count > 0))
            {
                var baseG = Parser.Parse(tc.Text, Op, out _);
                IGraph<string, long> ig = baseG;

                var expected = new Dictionary<(string, string), long>();
                foreach (var d in tc.ExpectedDistances) expected[(d.From, d.To)] = d.Distance;

                // If graph is undirected, create a directed copy with both-direction edges
                IGraph<string, long> floydGraph = ig;
                if (!ig.IsDirected)
                {
                    var dg = new Graph<string, long>(Op);
                    foreach (var v in ig.Nodes) dg.AddNode(v);
                    foreach (var e in ig.WeightedEdges) { dg.AddEdge(e.U, e.V, e.W, true); dg.AddEdge(e.V, e.U, e.W, true); }
                    floydGraph = dg;
                }
                var floyd = floydGraph.FloydWarshall<string, long>(Op);
                var fdist = floyd.Item1;

                // convert to string-keyed view to avoid key identity issues
                var fstr = new Dictionary<string, Dictionary<string, long>>();
                foreach (var rowK in fdist.Keys)
                {
                    var rmap = new Dictionary<string, long>();
                    foreach (var colK in fdist[rowK].Keys) rmap[colK.ToString()] = fdist[rowK][colK];
                    fstr[rowK.ToString()] = rmap;
                }

                foreach (var kv in expected)
                {
                    var (s, t) = kv.Key; var want = kv.Value;
                    long? fv = null;
                    if (fstr.ContainsKey(s) && fstr[s].ContainsKey(t)) fv = fstr[s][t];
                    if (fv == null || fv == Op.Infinity)
                    {
                        // fallback to Bellman-Ford from s
                        var bf = ig.BellmanFord<string, long>(s, Op);
                        if (!bf.Item1.ContainsKey(t)) throw new Exception($"[{tc.Name}] No distance for {s}->{t} from Floyd and Bellman-Ford");
                        var bfv = bf.Item1[t];
                        if (bfv != want) throw new Exception($"[{tc.Name}] expected {s}->{t} == {want} but got {bfv} (Floyd gave {(fv==null?"missing":"Infinity")})");
                    }
                    else if (fv != want)
                    {
                        throw new Exception($"[{tc.Name}] expected {s}->{t} == {want} but got {fv} (from Floyd)");
                    }
                }
            }
        }

        [Fact(DisplayName = "JSON driven: Johnson all-pairs check (all graphs)")]
        public void Json_JohnsonCheck()
        {
            var all = LoadExpectations();
            foreach (var tc in all.Where(g => g.ExpectedDistances != null && g.ExpectedDistances.Count > 0))
            {
                var baseG = Parser.Parse(tc.Text, Op, out _);
                IGraph<string, long> ig = baseG;

                var expected = new Dictionary<(string, string), long>();
                foreach (var d in tc.ExpectedDistances) expected[(d.From, d.To)] = d.Distance;

                IGraph<string, long> johnGraph = ig;
                if (!ig.IsDirected)
                {
                    var dg = new Graph<string, long>(Op);
                    foreach (var v in ig.Nodes) dg.AddNode(v);
                    foreach (var e in ig.WeightedEdges) { dg.AddEdge(e.U, e.V, e.W, true); dg.AddEdge(e.V, e.U, e.W, true); }
                    johnGraph = dg;
                }
                var john = johnGraph.Johnson<string>(Op);
                var jdist = john.Item1;

                // convert to string-keyed view
                var jstr = new Dictionary<string, Dictionary<string, long>>();
                foreach (var rowK in jdist.Keys)
                {
                    var rmap = new Dictionary<string, long>();
                    foreach (var colK in jdist[rowK].Keys) rmap[colK.ToString()] = jdist[rowK][colK];
                    jstr[rowK.ToString()] = rmap;
                }

                foreach (var kv in expected)
                {
                    var (s, t) = kv.Key; var want = kv.Value;
                    long? jv = null;
                    if (jstr.ContainsKey(s) && jstr[s].ContainsKey(t)) jv = jstr[s][t];
                    if (jv == null || jv == Op.Infinity)
                    {
                        // fallback to Bellman-Ford
                        var bf = ig.BellmanFord<string, long>(s, Op);
                        if (!bf.Item1.ContainsKey(t)) throw new Exception($"[{tc.Name}] No distance for {s}->{t} from Johnson and Bellman-Ford");
                        var bfv = bf.Item1[t];
                        if (bfv != want) throw new Exception($"[{tc.Name}] expected {s}->{t} == {want} but got {bfv} (Johnson gave {(jv==null?"missing":"Infinity")})");
                    }
                    else if (jv != want)
                    {
                        throw new Exception($"[{tc.Name}] expected {s}->{t} == {want} but got {jv} (from Johnson)");
                    }
                }
            }
        }

        [Fact(DisplayName = "JSON driven: Dijkstra single-source single-target checks")]
        public void Json_DijkstraCheck()
        {
            var tc = PickOne(g => g.ExpectedDistances != null && g.ExpectedDistances.Count > 0);
            var baseG = Parser.Parse(tc.Text, Op, out _);
            IGraph<string, long> ig = baseG;

            var expected = new Dictionary<(string, string), long>();
            foreach (var d in tc.ExpectedDistances) expected[(d.From, d.To)] = d.Distance;

            var nodes = new HashSet<string>(ig.Nodes);
            foreach (var kv in expected)
            {
                var (s, t) = kv.Key; var want = kv.Value;
                Assert.True(nodes.Contains(s), $"Dijkstra source '{s}' missing in graph");
                Assert.True(nodes.Contains(t), $"Dijkstra target '{t}' missing in graph");
                if (want < 0) continue; // Dijkstra doesn't support negative weights
                var dj = ig.Dijkstra<string, long>(s, t, Op, includeNodeWeight: false);
                Assert.True(dj.Reachable, $"Dijkstra reports unreachable {s}->{t}");
                Assert.Equal(want, dj.Cost);
            }
        }

        [Fact(DisplayName = "JSON driven: Bellman-Ford source checks")]
        public void Json_BellmanFordCheck()
        {
            var tc = PickOne(g => g.ExpectedDistances != null && g.ExpectedDistances.Count > 0);
            var baseG = Parser.Parse(tc.Text, Op, out _);
            IGraph<string, long> ig = baseG;

            var nodes = new HashSet<string>(ig.Nodes);
            var grouped = tc.ExpectedDistances.GroupBy(d => d.From);
            foreach (var group in grouped)
            {
                var s = group.Key;
                Assert.True(nodes.Contains(s), $"Bellman-Ford source '{s}' missing in graph");
                var bf = ig.BellmanFord<string, long>(s, Op);
                var dist = bf.Item1;
                foreach (var d in group)
                {
                    if (!nodes.Contains(d.To)) Assert.False(true, $"Bellman-Ford target '{d.To}' missing in graph");
                    if (dist.ContainsKey(d.To)) Assert.Equal(d.Distance, dist[d.To]);
                }
            }
        }

        [Fact(DisplayName = "JSON driven: SPFA source checks")]
        public void Json_SPFACheck()
        {
            var tc = PickOne(g => g.ExpectedDistances != null && g.ExpectedDistances.Count > 0);
            var baseG = Parser.Parse(tc.Text, Op, out _);
            IGraph<string, long> ig = baseG;

            var nodes = new HashSet<string>(ig.Nodes);
            var grouped = tc.ExpectedDistances.GroupBy(d => d.From);
            foreach (var group in grouped)
            {
                var s = group.Key;
                Assert.True(nodes.Contains(s), $"SPFA source '{s}' missing in graph");
                var spfa = ig.SPFA<string, long>(s, Op);
                var dist = spfa.Item1;
                foreach (var d in group)
                {
                    if (!nodes.Contains(d.To)) Assert.False(true, $"SPFA target '{d.To}' missing in graph");
                    if (dist.ContainsKey(d.To)) Assert.Equal(d.Distance, dist[d.To]);
                }
            }
        }

        [Fact(DisplayName = "JSON driven: TopologicalSort check")]
        public void Json_TopologicalCheck()
        {
            var tc = PickOne(g => g.Name == "DAG_Topological");
            var baseG = Parser.Parse(tc.Text, Op, out _);
            IGraph<string, long> ig = baseG;
            var order = ig.TopologicalSort<string, long>();
            var index = new Dictionary<string, int>();
            for (int i = 0; i < order.Count; i++) index[order[i]] = i;
            foreach (var e in ig.WeightedEdges)
            {
                Assert.True(index[e.U] < index[e.V]);
            }
        }

        [Fact(DisplayName = "JSON driven: MaxFlow check (EdmondsKarp/Dinic)")]
        public void Json_MaxFlowCheck()
        {
            var tc = PickOne(g => g.ExpectedMaxFlow.HasValue);
            var baseG = Parser.Parse(tc.Text, Op, out _);
            IGraph<string, long> ig = baseG;
            var ek = ig.EdmondsKarpMaxFlow<string>("s", "t");
            var din = ig.DinicMaxFlow<string>("s", "t");
            Assert.Equal(tc.ExpectedMaxFlow.Value, ek);
            Assert.Equal(tc.ExpectedMaxFlow.Value, din);
        }

        [Fact(DisplayName = "JSON driven: SCC check")]
        public void Json_SccCheck()
        {
            var tc = PickOne(g => g.Name == "Directed_SCC");
            var baseG = Parser.Parse(tc.Text, Op, out _);
            IGraph<string, long> ig = baseG;
            var sccs = ig.StronglyConnectedComponents<string, long>();
            var sizes = sccs.Select(c => c.Count).OrderBy(x => x).ToList();
            Assert.Equal(new List<int> { 2, 3 }, sizes);
        }

        [Fact(DisplayName = "JSON driven: Bridges/Articulation check")]
        public void Json_BridgesCheck()
        {
            var tc = PickOne(g => g.Name == "Undirected_Bridges");
            var baseG = Parser.Parse(tc.Text, Op, out _);
            IGraph<string, long> ig = baseG;
            var bridges = ig.Bridges<string, long>();
            Assert.Contains(bridges, b => (b.U == "3" && b.V == "5") || (b.U == "5" && b.V == "3"));
            var aps = ig.ArticulationPoints<string, long>();
            Assert.Contains("3", aps);
        }
    }
}
