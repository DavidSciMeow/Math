using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GraphX.Core;
using GraphX.Util;

namespace GraphX.Algorithms
{
    /// <summary>
    /// 静态图算法集（以 `IGraph` 的扩展方法形式提供）。<br/>Static graph algorithms provided as extension methods for IGraph.
    /// </summary>
    public static class GraphMethods
    {
        /// <summary>
        /// Dijkstra 最短路径（扩展方法），返回 `PathResult`。支持 `CancellationToken` 以取消长时间搜索。<br/>Dijkstra shortest path as an extension method. Returns PathResult. Supports CancellationToken to cancel long-running searches.
        /// </summary>
        public static PathResult<NodeType, TWeight> Dijkstra<NodeType, TWeight>(this IGraph<NodeType, TWeight> graph, NodeType start, NodeType end, IWeightOperator<TWeight> op, bool includeNodeWeight, CancellationToken ct = default)
            where NodeType : IEquatable<NodeType>
            where TWeight : IComparable<TWeight>
        {
            // 先决条件：Dijkstra 要求边权非负 / Precondition: Dijkstra requires non-negative edge weights
            bool _dijkstraHasNegative = false;
            foreach (var _e in graph.WeightedEdges)
            {
                if (op.Compare(_e.W, op.Zero) < 0) { _dijkstraHasNegative = true; break; }
            }
            if (_dijkstraHasNegative)
            {
                throw new GraphX.Error.GraphMethodNotApplicableException("Dijkstra", "Graph contains negative edge weights; Dijkstra requires non-negative weights.", $"directed={graph.IsDirected}, negativeEdges=true");
            }

            var result = new PathResult<NodeType, TWeight>();

            var dist = new Dictionary<NodeType, TWeight>();
            var prev = new Dictionary<NodeType, NodeType>();

            var heap = new BinaryHeap<TWeight, NodeType>(delegate (TWeight a, TWeight b) { return op.Compare(a, b); });

            // 从边快照初始化节点集合 / initialize nodes from edges
            var edges = graph.WeightedEdges.ToList();
            var nodes = new HashSet<NodeType>();
            foreach (var e in edges) { nodes.Add(e.U); nodes.Add(e.V); }

            foreach (var n in nodes) dist[n] = op.Infinity;
            if (!dist.ContainsKey(start)) dist[start] = op.Zero;
            dist[start] = includeNodeWeight ? graph.GetNodeWeight(start) : op.Zero;
            heap.Enqueue(start, dist[start]);

            while (heap.Count > 0)
            {
                ct.ThrowIfCancellationRequested();

                var pr = heap.DequeueMin();
                var u = pr.item;
                if (op.Compare(pr.priority, dist[u]) > 0) continue;
                if (u.Equals(end)) break;

                foreach (var nb in graph.GetNeighbors(u))
                {
                    ct.ThrowIfCancellationRequested();

                    var v = nb.Key;
                    var w = nb.Value;
                    var vNodeWeight = includeNodeWeight ? graph.GetNodeWeight(v) : op.Zero;
                    var nd = op.Add(op.Add(dist[u], w), vNodeWeight);
                    if (op.Compare(nd, dist[v]) < 0)
                    {
                        dist[v] = nd;
                        prev[v] = u;
                        heap.Enqueue(v, nd);
                    }
                }
            }

            if (!prev.ContainsKey(end) && !start.Equals(end)) { result.Reachable = false; return result; }

            var path = new List<NodeType>();
            var cur = end; path.Add(cur);
            while (!cur.Equals(start)) { cur = prev[cur]; path.Add(cur); }
            path.Reverse();
            result.Nodes = path;
            result.Cost = dist[end];
            result.Reachable = true;
            return result;
        }

        /// <summary>
        /// Bellman-Ford 扩展方法，返回 (dist, prev)。若检测到负环则抛出 `BFANWCDetectedException`。支持取消。<br/>Bellman-Ford extension method. Returns (dist, prev). Throws BFANWCDetectedException on negative cycles. Supports cancellation.
        /// </summary>
        public static Tuple<Dictionary<NodeType, TWeight>, Dictionary<NodeType, NodeType>> BellmanFord<NodeType, TWeight>(this IGraph<NodeType, TWeight> graph, NodeType start, IWeightOperator<TWeight> op, CancellationToken ct = default)
            where NodeType : IEquatable<NodeType>
            where TWeight : IComparable<TWeight>
        {
            // 从邻接构造节点和有向边快照，确保在无向图中也能正确松弛 / Build nodes and directed-edge snapshot to ensure proper relaxation for undirected graphs
            var nodes = new HashSet<NodeType>(graph.Nodes);
            var edges = new List<(NodeType U, NodeType V, TWeight W)>();
            foreach (var u in nodes)
            {
                ct.ThrowIfCancellationRequested();
                foreach (var nb in graph.GetNeighbors(u))
                {
                    edges.Add((u, nb.Key, nb.Value));
                }
            }

            var dist = new Dictionary<NodeType, TWeight>();
            var prev = new Dictionary<NodeType, NodeType>();

            foreach (var n in nodes) dist[n] = op.Infinity;
            if (!dist.ContainsKey(start)) dist[start] = op.Zero; else dist[start] = op.Zero;

            int ncount = nodes.Count;
            for (int i = 0; i < ncount - 1; i++)
            {
                ct.ThrowIfCancellationRequested();

                bool updated = false;
                foreach (var e in edges)
                {
                    ct.ThrowIfCancellationRequested();

                    var u = e.U; var v = e.V; var w = e.W;
                    if (op.Compare(dist[u], op.Infinity) == 0) continue;
                    var nd = op.Add(dist[u], w);
                    if (op.Compare(nd, dist[v]) < 0)
                    {
                        dist[v] = nd;
                        prev[v] = u;
                        updated = true;
                    }
                }
                if (!updated) break;
            }

            // 负环检测 / negative cycle detection
            var neg = new List<Tuple<NodeType, NodeType, TWeight>>();
            foreach (var e in edges)
            { 
                ct.ThrowIfCancellationRequested();

                var u = e.U; var v = e.V; var w = e.W;
                if (op.Compare(dist[u], op.Infinity) == 0) continue;
                var nd = op.Add(dist[u], w);
                if (op.Compare(nd, dist[v]) < 0) neg.Add(Tuple.Create(u, v, w));
            }
            if (neg.Count > 0) throw new GraphX.Error.BFANWCDetectedException();
            return Tuple.Create(dist, prev);
        }

        /// <summary>
        /// Kruskal 最小生成树（扩展方法），返回 MST 的边列表。仅适用于无向图。<br/>Kruskal minimum spanning tree as an extension method; returns list of edges in the MST. Only for undirected graphs.
        /// </summary>
        public static List<Tuple<NodeType, NodeType, TWeight>> Kruskal<NodeType, TWeight>(this IGraph<NodeType, TWeight> graph, IComparer<TWeight> weightComparer = null)
            where NodeType : IEquatable<NodeType>
            where TWeight : IComparable<TWeight>
        {
            // Kruskal 仅适用于无向图；在有向图上该概念不明确 / Kruskal is defined for undirected graphs; for directed graphs the notion is ambiguous
            if (graph.IsDirected) throw new GraphX.Error.GraphMethodNotApplicableException("Kruskal", "Kruskal requires an undirected graph.", $"directed={graph.IsDirected}");
             // 获取边的快照 / Take snapshot of edges
             var edges = graph.WeightedEdges.ToList();
             // 对无向图边可能已按规范形式存储 / For undirected graphs edges may be canonical
             // 按权重排序 / Sort edges by weight
             if (weightComparer == null)
             {
                 weightComparer = Comparer<TWeight>.Default;
             }

             edges.Sort((a, b) => weightComparer.Compare(a.W, b.W));

             var uf = new UnionFind<NodeType>();
             var nodes = new HashSet<NodeType>();
             foreach (var e in edges) { nodes.Add(e.U); nodes.Add(e.V); }
             foreach (var n in nodes) uf.MakeSet(n);

             var mst = new List<Tuple<NodeType, NodeType, TWeight>>();
             foreach (var e in edges)
             {
                 var u = e.U; var v = e.V; var w = e.W;
                 if (!EqualityComparer<NodeType>.Default.Equals(uf.Find(u), uf.Find(v)))
                 {
                     uf.Union(u, v);
                     mst.Add(Tuple.Create(u, v, w));
                 }
             }

             return mst;
        }

        /// <summary>
        /// Prim 算法构造最小生成树（从指定起点）。若图不连通，仅返回该连通分量的 MST。<br/>Prim's algorithm to build a minimum spanning tree starting from a given node. If graph is disconnected returns MST for the connected component.
        /// </summary>
        public static List<Tuple<NodeType, NodeType, TWeight>> Prim<NodeType, TWeight>(this IGraph<NodeType, TWeight> graph, NodeType start, IComparer<TWeight> weightComparer = null, CancellationToken ct = default)
            where NodeType : IEquatable<NodeType>
            where TWeight : IComparable<TWeight>
        {
            // Prim 仅适用于无向图 / Prim is defined for undirected graphs
            if (graph.IsDirected) throw new GraphX.Error.GraphMethodNotApplicableException("Prim", "Prim requires an undirected graph.", $"directed={graph.IsDirected}");
             if (weightComparer == null) weightComparer = Comparer<TWeight>.Default;
             var result = new List<Tuple<NodeType, NodeType, TWeight>>();

             var visited = new HashSet<NodeType>();
             var heap = new BinaryHeap<TWeight, Tuple<NodeType, NodeType>>(delegate (TWeight a, TWeight b) { return weightComparer.Compare(a, b); });

             visited.Add(start);
             // 从起点压入所有相邻边 / push all edges from start
             foreach (var nb in graph.GetNeighbors(start))
             {
                 ct.ThrowIfCancellationRequested();
                 heap.Enqueue(Tuple.Create(start, nb.Key), nb.Value);
             }

             while (heap.Count > 0)
             {
                 ct.ThrowIfCancellationRequested();
                 var entry = heap.DequeueMin();
                 var w = entry.priority;
                 var uv = entry.item; // (u,v)
                 var u = uv.Item1; var v = uv.Item2;
                 if (visited.Contains(v)) continue;
                 // 选择边 u-v / select edge u-v
                 result.Add(Tuple.Create(u, v, w));
                 visited.Add(v);
                 // 从 v 扩展边 / add edges from v
                 foreach (var nb in graph.GetNeighbors(v))
                 {
                     ct.ThrowIfCancellationRequested();
                     if (!visited.Contains(nb.Key)) heap.Enqueue(Tuple.Create(v, nb.Key), nb.Value);
                 }
             }

             return result;
        }

        /// <summary>
        /// A* 搜索算法（启发式），返回最短路径结果。启发函数估计从任意节点到目标的代价。<br/>A* search algorithm (heuristic), returns a shortest path result. The heuristic estimates cost from a node to the goal.
        /// </summary>
        public static PathResult<NodeType, TWeight> AStar<NodeType, TWeight>(this IGraph<NodeType, TWeight> graph, NodeType start, NodeType goal, IWeightOperator<TWeight> op, Func<NodeType, NodeType, TWeight> heuristic, bool includeNodeWeight = false, CancellationToken ct = default)
            where NodeType : IEquatable<NodeType>
            where TWeight : IComparable<TWeight>
        {
            // 先决条件：A* 要求边权非负 / Precondition: A* requires non-negative edge weights
            bool _astarHasNegative = false;
            foreach (var _e in graph.WeightedEdges)
            {
                if (op.Compare(_e.W, op.Zero) < 0) { _astarHasNegative = true; break; }
            }
            if (_astarHasNegative)
            {
                throw new GraphX.Error.GraphMethodNotApplicableException("A*", "Graph contains negative edge weights; A* requires non-negative weights.", $"directed={graph.IsDirected}, negativeEdges=true");
            }

            var result = new PathResult<NodeType, TWeight>();
            var dist = new Dictionary<NodeType, TWeight>(); // g 代价 / g score
            var prev = new Dictionary<NodeType, NodeType>();

            var heap = new BinaryHeap<TWeight, NodeType>(delegate (TWeight a, TWeight b) { return op.Compare(a, b); });

            // 从边快照初始化节点集合 / initialize nodes from edges
            var edges = graph.WeightedEdges.ToList();
            var nodes = new HashSet<NodeType>();
            foreach (var e in edges) { nodes.Add(e.U); nodes.Add(e.V); }

            foreach (var n in nodes) dist[n] = op.Infinity;
            if (!dist.ContainsKey(start)) dist[start] = op.Zero;

            dist[start] = includeNodeWeight ? graph.GetNodeWeight(start) : op.Zero;
            var fstart = op.Add(dist[start], heuristic(start, goal));
            heap.Enqueue(start, fstart);

            while (heap.Count > 0)
            {
                ct.ThrowIfCancellationRequested();
                var cur = heap.DequeueMin();
                var u = cur.item;
                if (op.Compare(cur.priority, op.Add(dist[u], heuristic(u, goal))) > 0) continue; // 过期条目 / stale
                if (u.Equals(goal)) break;

                foreach (var nb in graph.GetNeighbors(u))
                {
                    ct.ThrowIfCancellationRequested();
                    var v = nb.Key; var w = nb.Value;
                    var vNodeWeight = includeNodeWeight ? graph.GetNodeWeight(v) : op.Zero;
                    var tentative = op.Add(op.Add(dist[u], w), vNodeWeight);
                    if (op.Compare(tentative, dist[v]) < 0)
                    {
                        dist[v] = tentative;
                        prev[v] = u;
                        var f = op.Add(tentative, heuristic(v, goal));
                        heap.Enqueue(v, f);
                    }
                }
            }

            if (!prev.ContainsKey(goal) && !start.Equals(goal)) { result.Reachable = false; return result; }

            var path = new List<NodeType>();
            var curNode = goal; path.Add(curNode);
            while (!curNode.Equals(start)) { curNode = prev[curNode]; path.Add(curNode); }
            path.Reverse();
            result.Nodes = path;
            result.Cost = dist[goal];
            result.Reachable = true;
            return result;
        }

        /// <summary>
        /// 计算弱连通分量（将边视为无向），返回每个连通分量的节点列表。支持取消。<br/>Compute weakly connected components (treat edges as undirected) and return each component as a list of nodes. Supports cancellation.
        /// </summary>
        public static List<List<NodeType>> ConnectedComponents<NodeType, TWeight>(this IGraph<NodeType, TWeight> graph, CancellationToken ct = default)
            where NodeType : IEquatable<NodeType>
            where TWeight : IComparable<TWeight>
        {
            // 节点和边的快照 / snapshot nodes and edges
            var nodes = new HashSet<NodeType>(graph.Nodes);
            var edges = graph.WeightedEdges.ToList();

            // 构建无向邻接的快照 / build undirected adjacency snapshot
            var adj = new Dictionary<NodeType, List<NodeType>>();
            foreach (var n in nodes) adj[n] = new List<NodeType>();
            foreach (var e in edges)
            {
                ct.ThrowIfCancellationRequested();
                // 两个方向都加入（对有向/无向存储均适用）/ add both directions (works for directed or undirected stored edges)
                if (nodes.Contains(e.U) && nodes.Contains(e.V))
                {
                    adj[e.U].Add(e.V);
                    adj[e.V].Add(e.U);
                }
            }

            var visited = new HashSet<NodeType>();
            var result = new List<List<NodeType>>();

            foreach (var n in nodes)
            {
                ct.ThrowIfCancellationRequested();
                if (visited.Contains(n)) continue;
                // 从 n 进行 BFS/DFS / BFS/DFS from n
                var comp = new List<NodeType>();
                var q = new Queue<NodeType>();
                visited.Add(n);
                q.Enqueue(n);
                while (q.Count > 0)
                {
                    ct.ThrowIfCancellationRequested();
                    var u = q.Dequeue();
                    comp.Add(u);
                    foreach (var v in adj[u])
                    {
                        if (visited.Add(v)) q.Enqueue(v);
                    }
                }
                result.Add(comp);
            }

            return result;
        }

        /// <summary>
        /// 返回弱连通分量数量。<br/>Return number of weakly connected components.
        /// </summary>
        public static int ConnectedComponentsCount<NodeType, TWeight>(this IGraph<NodeType, TWeight> graph, CancellationToken ct = default)
            where NodeType : IEquatable<NodeType>
            where TWeight : IComparable<TWeight>
        {
            return graph.ConnectedComponents<NodeType, TWeight>(ct).Count;
        }

        /// <summary>
        /// 计算强连通分量（Tarjan 算法），支持取消。<br/>Compute strongly connected components using Tarjan's algorithm. Supports cancellation.
        /// </summary>
        public static List<List<NodeType>> StronglyConnectedComponents<NodeType, TWeight>(this IGraph<NodeType, TWeight> graph, CancellationToken ct = default)
            where NodeType : IEquatable<NodeType>
            where TWeight : IComparable<TWeight>
        {
            // 取有向边快照 / snapshot nodes and edges (directed)
            var nodes = graph.Nodes.ToList();
            var edges = graph.WeightedEdges.ToList();

            var adj = new Dictionary<NodeType, List<NodeType>>();
            foreach (var n in nodes) adj[n] = new List<NodeType>();
            foreach (var e in edges)
            {
                ct.ThrowIfCancellationRequested();
                if (adj.ContainsKey(e.U) && adj.ContainsKey(e.V)) adj[e.U].Add(e.V);
            }

            var index = 0;
            var indices = new Dictionary<NodeType, int>();
            var lowlink = new Dictionary<NodeType, int>();
            var onstack = new HashSet<NodeType>();
            var stack = new Stack<NodeType>();
            var result = new List<List<NodeType>>();

            void StrongConnect(NodeType v)
            {
                ct.ThrowIfCancellationRequested();
                indices[v] = index;
                lowlink[v] = index;
                index++;
                stack.Push(v);
                onstack.Add(v);

                foreach (var w in adj[v])
                {
                    ct.ThrowIfCancellationRequested();
                    if (!indices.ContainsKey(w))
                    {
                        StrongConnect(w);
                        lowlink[v] = Math.Min(lowlink[v], lowlink[w]);
                    }
                    else if (onstack.Contains(w))
                    {
                        lowlink[v] = Math.Min(lowlink[v], indices[w]);
                    }
                }

                // 若 v 为根，则弹栈并形成一个强连通分量 / If v is a root node, pop the stack and generate an SCC
                if (lowlink[v] == indices[v])
                {
                    var comp = new List<NodeType>();
                    NodeType w;
                    do
                    {
                        w = stack.Pop();
                        onstack.Remove(w);
                        comp.Add(w);
                    } while (!EqualityComparer<NodeType>.Default.Equals(w, v));
                    result.Add(comp);
                }
            }

            foreach (var v in nodes)
            {
                ct.ThrowIfCancellationRequested();
                if (!indices.ContainsKey(v)) StrongConnect(v);
            }

            return result;
        }

        /// <summary>
        /// 顶点拓扑排序（Kahn 算法）。若图含有环则抛出异常。支持取消。<br/>Topological sort using Kahn's algorithm. Throws InvalidOperationException if the graph has a cycle. Supports cancellation.
        /// </summary>
        public static List<NodeType> TopologicalSort<NodeType, TWeight>(this IGraph<NodeType, TWeight> graph, CancellationToken ct = default)
            where NodeType : IEquatable<NodeType>
            where TWeight : IComparable<TWeight>
        {
            // 先决条件：需要有向无环图 / Precondition: requires a directed acyclic graph
            if (!graph.IsDirected) throw new GraphX.Error.GraphMethodNotApplicableException("TopologicalSort", "TopologicalSort requires a directed graph.", $"directed={graph.IsDirected}");
            // 节点与边的快照 / snapshot nodes and edges
            var nodes = graph.Nodes.ToList();
            var edges = graph.WeightedEdges.ToList();

            // 构建邻接与入度 / build adjacency and in-degree
            var adj = new Dictionary<NodeType, List<NodeType>>();
            var indeg = new Dictionary<NodeType, int>();
            foreach (var n in nodes)
            {
                adj[n] = new List<NodeType>();
                indeg[n] = 0;
            }
            foreach (var e in edges)
            {
                ct.ThrowIfCancellationRequested();
                if (!adj.ContainsKey(e.U) || !adj.ContainsKey(e.V)) continue;
                adj[e.U].Add(e.V);
                indeg[e.V] = indeg.ContainsKey(e.V) ? indeg[e.V] + 1 : 1;
            }

            var q = new Queue<NodeType>();
            foreach (var kv in indeg)
            {
                if (kv.Value == 0) q.Enqueue(kv.Key);
            }

            var res = new List<NodeType>();
            while (q.Count > 0)
            {
                ct.ThrowIfCancellationRequested();
                var u = q.Dequeue();
                res.Add(u);
                foreach (var v in adj[u])
                {
                    ct.ThrowIfCancellationRequested();
                    indeg[v]--;
                    if (indeg[v] == 0) q.Enqueue(v);
                }
            }

            if (res.Count != nodes.Count)
            {
                throw new InvalidOperationException("Graph has at least one cycle; topological sort not possible.");
            }

            return res;
         }

        /// <summary>
        /// 从起点进行广度优先遍历，返回按 BFS 顺序访问的节点列表。支持取消。<br/>Breadth-First Search from a start node. Returns visited nodes in BFS order. Supports cancellation.
        /// </summary>
        public static List<NodeType> BFS<NodeType, TWeight>(this IGraph<NodeType, TWeight> graph, NodeType start, CancellationToken ct = default)
            where NodeType : IEquatable<NodeType>
            where TWeight : IComparable<TWeight>
        {
            var result = new List<NodeType>();
            var visited = new HashSet<NodeType>();
            var q = new Queue<NodeType>();

            var nodes = new HashSet<NodeType>(graph.Nodes);
            if (!nodes.Contains(start)) return result;

            visited.Add(start);
            q.Enqueue(start);

            while (q.Count > 0)
            {
                ct.ThrowIfCancellationRequested();
                var u = q.Dequeue();
                result.Add(u);
                foreach (var nb in graph.GetNeighbors(u))
                {
                    ct.ThrowIfCancellationRequested();
                    var v = nb.Key;
                    if (visited.Add(v)) q.Enqueue(v);
                }
            }

            return result;
        }

        /// <summary>
        /// 从起点进行深度优先遍历（迭代实现），返回按前序访问的节点列表。支持取消。<br/>Depth-First Search from a start node (iterative). Returns visited nodes in DFS pre-order. Supports cancellation.
        /// </summary>
        public static List<NodeType> DFS<NodeType, TWeight>(this IGraph<NodeType, TWeight> graph, NodeType start, CancellationToken ct = default)
            where NodeType : IEquatable<NodeType>
            where TWeight : IComparable<TWeight>
        {
            var result = new List<NodeType>();
            var visited = new HashSet<NodeType>();
            var stack = new Stack<NodeType>();

            var nodes = new HashSet<NodeType>(graph.Nodes);
            if (!nodes.Contains(start)) return result;

            stack.Push(start);
            while (stack.Count > 0)
            {
                ct.ThrowIfCancellationRequested();
                var u = stack.Pop();
                if (!visited.Add(u)) continue;
                result.Add(u);
                // 反向压栈以近似递归 DFS 的遍历顺序（若顺序可稳定）/ push neighbors in reverse order to approximate recursive DFS order if stable ordering available
                var neighbors = graph.GetNeighbors(u).Select(kv => kv.Key).ToList();
                for (int i = neighbors.Count - 1; i >= 0; i--)
                {
                    ct.ThrowIfCancellationRequested();
                    var v = neighbors[i];
                    if (!visited.Contains(v)) stack.Push(v);
                }
            }

            return result;
        }

        /// <summary>
        /// 找出所有“桥”边（删除后会增加连通分量数的边），将图视为无向。支持取消。<br/>Find all bridge edges in the graph (edges whose removal increases number of connected components), treating the graph as undirected. Supports cancellation.
        /// </summary>
        public static List<(NodeType U, NodeType V)> Bridges<NodeType, TWeight>(this IGraph<NodeType, TWeight> graph, CancellationToken ct = default)
            where NodeType : IEquatable<NodeType>
            where TWeight : IComparable<TWeight>
        {
            var nodes = graph.Nodes.ToList();
            var adj = new Dictionary<NodeType, List<NodeType>>();
            foreach (var n in nodes) adj[n] = new List<NodeType>();
            foreach (var u in nodes)
            {
                ct.ThrowIfCancellationRequested();
                foreach (var nb in graph.GetNeighbors(u))
                {
                    if (!adj.ContainsKey(nb.Key)) continue;
                    // 添加无向邻接 / add undirected adjacency
                    adj[u].Add(nb.Key);
                }
            }

            var visited = new Dictionary<NodeType, bool>();
            var disc = new Dictionary<NodeType, int>();
            var low = new Dictionary<NodeType, int>();
            var parent = new Dictionary<NodeType, NodeType>();
            var time = 0;
            var bridges = new List<(NodeType U, NodeType V)>();

            foreach (var v in nodes) { visited[v] = false; parent[v] = default(NodeType); }

            void Dfs(NodeType u)
            {
                ct.ThrowIfCancellationRequested();
                visited[u] = true;
                disc[u] = low[u] = ++time;

                foreach (var v in adj[u])
                {
                    ct.ThrowIfCancellationRequested();
                    if (!visited[v])
                    {
                        parent[v] = u;
                        Dfs(v);
                        low[u] = Math.Min(low[u], low[v]);
                        // 若 low[v] > disc[u] 则 u-v 为桥 / if low[v] > disc[u] then edge u-v is a bridge
                        if (low[v] > disc[u]) bridges.Add((u, v));
                    }
                    else if (!EqualityComparer<NodeType>.Default.Equals(parent[u], v))
                    {
                        // 返祖边 / back edge
                        low[u] = Math.Min(low[u], disc[v]);
                    }
                }
            }

            foreach (var u in nodes)
            {
                if (!visited[u]) Dfs(u);
            }

            return bridges;
        }

        /// <summary>
        /// 找出图中的所有割点（关节点），将图视为无向。支持取消。<br/>Find articulation points (cut vertices) treating the graph as undirected. Supports cancellation.
        /// </summary>
        public static List<NodeType> ArticulationPoints<NodeType, TWeight>(this IGraph<NodeType, TWeight> graph, CancellationToken ct = default)
            where NodeType : IEquatable<NodeType>
            where TWeight : IComparable<TWeight>
        {
            var nodes = graph.Nodes.ToList();
            var adj = new Dictionary<NodeType, List<NodeType>>();
            foreach (var n in nodes) adj[n] = new List<NodeType>();
            foreach (var u in nodes)
            {
                ct.ThrowIfCancellationRequested();
                foreach (var nb in graph.GetNeighbors(u))
                {
                    if (!adj.ContainsKey(nb.Key)) continue;
                    adj[u].Add(nb.Key);
                }
            }

            var visited = new Dictionary<NodeType, bool>();
            var disc = new Dictionary<NodeType, int>();
            var low = new Dictionary<NodeType, int>();
            var parent = new Dictionary<NodeType, NodeType>();
            var ap = new Dictionary<NodeType, bool>();
            var time = 0;

            foreach (var v in nodes)
            {
                visited[v] = false;
                parent[v] = default(NodeType);
                ap[v] = false;
            }

            void Dfs(NodeType u)
            {
                ct.ThrowIfCancellationRequested();
                visited[u] = true;
                disc[u] = low[u] = ++time;
                int children = 0;

                foreach (var v in adj[u])
                {
                    ct.ThrowIfCancellationRequested();
                    if (!visited[v])
                    {
                        children++;
                        parent[v] = u;
                        Dfs(v);
                        low[u] = Math.Min(low[u], low[v]);
                        // 若 u 为根且子树数 >= 2 / if u is root and has two or more children
                        if (EqualityComparer<NodeType>.Default.Equals(parent[u], default(NodeType)) && children > 1)
                            ap[u] = true;
                        // 若 u 非根且满足 low[v] >= disc[u] / if u is not root and low[v] >= disc[u]
                        if (!EqualityComparer<NodeType>.Default.Equals(parent[u], default(NodeType)) && low[v] >= disc[u])
                            ap[u] = true;
                    }
                    else if (!EqualityComparer<NodeType>.Default.Equals(parent[u], v))
                    {
                        low[u] = Math.Min(low[u], disc[v]);
                    }
                }
            }

            foreach (var u in nodes)
            {
                if (!visited[u]) Dfs(u);
            }

            return ap.Where(kv => kv.Value).Select(kv => kv.Key).ToList();
        }


        /// <summary>
        /// Floyd-Warshall 全源最短路，返回 (dist, next) 字典。<br/>Floyd-Warshall all-pairs shortest paths. Returns (dist, next) dictionaries.
        /// </summary>
        public static Tuple<Dictionary<NodeType, Dictionary<NodeType, TWeight>>, Dictionary<NodeType, Dictionary<NodeType, NodeType>>> FloydWarshall<NodeType, TWeight>(this IGraph<NodeType, TWeight> graph, IWeightOperator<TWeight> op, CancellationToken ct = default)
            where NodeType : IEquatable<NodeType>
            where TWeight : IComparable<TWeight>
        {
            var nodes = graph.Nodes.ToList();
            var dist = new Dictionary<NodeType, Dictionary<NodeType, TWeight>>();
            var next = new Dictionary<NodeType, Dictionary<NodeType, NodeType>>();

            // 初始化 / init
            foreach (var u in nodes)
            {
                dist[u] = new Dictionary<NodeType, TWeight>();
                next[u] = new Dictionary<NodeType, NodeType>();
                foreach (var v in nodes) dist[u][v] = op.Infinity;
                dist[u][u] = op.Zero;
            }

            foreach (var e in graph.WeightedEdges)
            {
                ct.ThrowIfCancellationRequested();
                dist[e.U][e.V] = e.W;
                next[e.U][e.V] = e.V;
            }

            foreach (var k in nodes)
            {
                ct.ThrowIfCancellationRequested();
                foreach (var i in nodes)
                {
                    foreach (var j in nodes)
                    {
                        var ij = dist[i][j];
                        var ik = dist[i][k];
                        var kj = dist[k][j];
                        if (op.Compare(ik, op.Infinity) == 0 || op.Compare(kj, op.Infinity) == 0) continue;
                        var alt = op.Add(ik, kj);
                        if (op.Compare(alt, ij) < 0)
                        {
                            dist[i][j] = alt;
                            next[i][j] = next[i].ContainsKey(k) ? next[i][k] : k;
                        }
                    }
                }
            }

            return Tuple.Create(dist, next);
        }

        /// <summary>
        /// 基于 Floyd-Warshall 的 next 表重建路径。<br/>Reconstruct path from Floyd-Warshall next table.
        /// </summary>
        public static List<NodeType> ReconstructPathFromFloydNext<NodeType, TWeight>(Dictionary<NodeType, Dictionary<NodeType, NodeType>> next, NodeType u, NodeType v)
            where NodeType : IEquatable<NodeType>
            where TWeight : IComparable<TWeight>
        {
            var path = new List<NodeType>();
            if (!next.ContainsKey(u) || !next[u].ContainsKey(v)) return path;
            var at = u;
            path.Add(at);
            while (!EqualityComparer<NodeType>.Default.Equals(at, v))
            {
                at = next[at][v];
                path.Add(at);
            }
            return path;
        }

        /// <summary>
        /// SPFA（Shortest Path Faster Algorithm），使用队列并在出现负环时抛出异常。<br/>SPFA (Shortest Path Faster Algorithm) - uses queue and detects negative cycles.
        /// </summary>
        public static Tuple<Dictionary<NodeType, TWeight>, Dictionary<NodeType, NodeType>> SPFA<NodeType, TWeight>(this IGraph<NodeType, TWeight> graph, NodeType start, IWeightOperator<TWeight> op, CancellationToken ct = default)
            where NodeType : IEquatable<NodeType>
            where TWeight : IComparable<TWeight>
        {
            var nodes = graph.Nodes.ToList();
            var dist = new Dictionary<NodeType, TWeight>();
            var prev = new Dictionary<NodeType, NodeType>();
            var inq = new Dictionary<NodeType, bool>();
            var count = new Dictionary<NodeType, int>();

            foreach (var n in nodes)
            {
                dist[n] = op.Infinity;
                inq[n] = false;
                count[n] = 0;
            }
            if (!dist.ContainsKey(start)) dist[start] = op.Zero;
            dist[start] = op.Zero;

            var q = new Queue<NodeType>();
            q.Enqueue(start); inq[start] = true; count[start] = 1;

            while (q.Count > 0)
            {
                ct.ThrowIfCancellationRequested();
                var u = q.Dequeue(); inq[u] = false;
                foreach (var nb in graph.GetNeighbors(u))
                {
                    ct.ThrowIfCancellationRequested();
                    var v = nb.Key; var w = nb.Value;
                    if (op.Compare(dist[u], op.Infinity) == 0) continue;
                    var nd = op.Add(dist[u], w);
                    if (op.Compare(nd, dist[v]) < 0)
                    {
                        dist[v] = nd; prev[v] = u;
                        if (!inq[v]) { q.Enqueue(v); inq[v] = true; count[v]++; if (count[v] > nodes.Count) throw new GraphX.Error.BFANWCDetectedException(); }
                    }
                }
            }

            return Tuple.Create(dist, prev);
        }

        /// <summary>
        /// 最小生成森林：对每个连通分量生成各自的 MST（基于 Kruskal 结果）。<br/>MinimumSpanningForest: produce MST per connected component using Kruskal results.
        /// </summary>
        public static List<List<Tuple<NodeType, NodeType, TWeight>>> MinimumSpanningForest<NodeType, TWeight>(this IGraph<NodeType, TWeight> graph, IComparer<TWeight> weightComparer = null)
            where NodeType : IEquatable<NodeType>
            where TWeight : IComparable<TWeight>
        {
            var mstEdges = graph.Kruskal<NodeType, TWeight>(weightComparer);
            var uf = new UnionFind<NodeType>();
            var nodes = new HashSet<NodeType>(graph.Nodes);
            foreach (var n in nodes) uf.MakeSet(n);
            foreach (var e in mstEdges) uf.Union(e.Item1, e.Item2);

            var groups = new Dictionary<NodeType, List<Tuple<NodeType, NodeType, TWeight>>>();
            foreach (var e in mstEdges)
            {
                var root = uf.Find(e.Item1);
                if (!groups.ContainsKey(root)) groups[root] = new List<Tuple<NodeType, NodeType, TWeight>>();
                groups[root].Add(e);
            }

            return groups.Values.ToList();
        }

        /// <summary>
        /// Johnson 算法：适用于可能存在负权但无负环的图，计算全源最短路；返回 (dist, next)。<br/>Johnson's algorithm for all-pairs shortest paths for graphs with potentially negative weights but no negative cycles. Returns (dist, next).
        /// </summary>
        public static Tuple<Dictionary<NodeType, Dictionary<NodeType, long>>, Dictionary<NodeType, Dictionary<NodeType, NodeType>>> Johnson<NodeType>(this IGraph<NodeType, long> graph, IWeightOperator<long> op, CancellationToken ct = default)
            where NodeType : IEquatable<NodeType>
        {
            // 添加虚拟源并运行 Bellman-Ford 以获得势能 h / Add a super-source and run Bellman-Ford to get potentials h
            var nodes = graph.Nodes.ToList();
            var h = new Dictionary<NodeType, long>();
            foreach (var n in nodes) h[n] = op.Infinity;
            // 通过松弛所有边 |V|-1 次来模拟从虚拟源出发 / relax all edges |V|-1 times from virtual source
            foreach (var n in nodes) h[n] = 0;
            var edges = graph.WeightedEdges.ToList();
            int ncount = nodes.Count;
            for (int i = 0; i < ncount - 1; i++)
            {
                ct.ThrowIfCancellationRequested();
                bool updated = false;
                foreach (var e in edges)
                {
                    var u = e.U; var v = e.V; var w = e.W;
                    if (h[u] == op.Infinity) continue;
                    var nd = h[u] + w;
                    if (nd < h[v]) { h[v] = nd; updated = true; }
                }
                if (!updated) break;
            }
            // 负环检查 / check negative cycles
            foreach (var e in edges)
            {
                if (h[e.U] == op.Infinity) continue;
                if (h[e.U] + e.W < h[e.V]) throw new GraphX.Error.BFANWCDetectedException();
            }

            // 重权：w' = w + h[u] - h[v] / reweight edges: w' = w + h[u] - h[v]
            var adj = new Dictionary<NodeType, List<KeyValuePair<NodeType, long>>>();
            foreach (var u in nodes) adj[u] = new List<KeyValuePair<NodeType, long>>();
            foreach (var e in edges)
            {
                var wprime = e.W + h[e.U] - h[e.V];
                adj[e.U].Add(new KeyValuePair<NodeType, long>(e.V, wprime));
            }

            // 对每个源运行 Dijkstra（非负权）/ For each source run Dijkstra on non-negative weights
            var allDist = new Dictionary<NodeType, Dictionary<NodeType, long>>();
            var next = new Dictionary<NodeType, Dictionary<NodeType, NodeType>>();
            foreach (var s in nodes)
            {
                ct.ThrowIfCancellationRequested();
                var dist = new Dictionary<NodeType, long>();
                foreach (var n in nodes) dist[n] = op.Infinity;
                dist[s] = 0;
                var pq = new BinaryHeap<long, NodeType>((a, b) => a.CompareTo(b));
                pq.Enqueue(s, 0);
                while (pq.Count > 0)
                {
                    var e = pq.DequeueMin();
                    var u = e.item; var du = e.priority;
                    if (du != dist[u]) continue;
                    foreach (var kv in adj[u])
                    {
                        var v = kv.Key; var w = kv.Value;
                        var nd = du + w;
                        if (nd < dist[v]) { dist[v] = nd; pq.Enqueue(v, nd); if (!next.ContainsKey(s)) next[s] = new Dictionary<NodeType, NodeType>(); next[s][v] = EqualityComparer<NodeType>.Default.Equals(u, s) ? v : (next[s].ContainsKey(u) ? next[s][u] : v); }
                    }
                }
                // 转回原权：d[u][v] = dist'[v] + h[v] - h[u] / convert back to original weights
                var realDist = new Dictionary<NodeType, long>();
                foreach (var v in nodes)
                {
                    if (dist[v] == op.Infinity) realDist[v] = op.Infinity; else realDist[v] = dist[v] + h[v] - h[s];
                }
                allDist[s] = realDist;
            }

            return Tuple.Create(allDist, next);
        }

        /// <summary>
        /// Edmonds-Karp 最大流，适用于 long 容量；返回最大流值。优化为在 BFS 时仅遍历邻接表。<br/>Edmonds-Karp max flow for graphs with long capacities. Returns max flow value. Optimized to use adjacency lists for BFS.
        /// </summary>
        public static long EdmondsKarpMaxFlow<NodeType>(this IGraph<NodeType, long> graph, NodeType source, NodeType sink, CancellationToken ct = default)
            where NodeType : IEquatable<NodeType>
        {
            // 先决条件：最大流算法要求有向图 / Precondition: Max flow algorithms expect a directed graph
            if (!graph.IsDirected) throw new GraphX.Error.GraphMethodNotApplicableException("EdmondsKarpMaxFlow", "Edmonds-Karp max flow requires a directed graph.", $"directed={graph.IsDirected}");
            // 构建邻接表和残量容量字典 / Build adjacency list and residual capacity dictionary
            var adj = new Dictionary<NodeType, List<NodeType>>();
            var residual = new Dictionary<(NodeType U, NodeType V), long>();

            // 确保所有节点都在邻接表中 / ensure all nodes present in adj
            foreach (var n in graph.Nodes)
            {
                if (!adj.ContainsKey(n)) adj[n] = new List<NodeType>();
            }

            foreach (var e in graph.WeightedEdges)
            {
                var u = e.U; var v = e.V; var w = e.W;
                if (!adj.ContainsKey(u)) adj[u] = new List<NodeType>();
                if (!adj.ContainsKey(v)) adj[v] = new List<NodeType>();
                if (!adj[u].Contains(v)) adj[u].Add(v);
                if (!adj[v].Contains(u)) adj[v].Add(u); // 为残量遍历加入反向边 / add reverse for residual traversal

                var key = (e.U, e.V);
                if (residual.ContainsKey(key)) residual[key] = Math.Max(residual[key], w);
                else residual[key] = w;
                var rev = (e.V, e.U);
                if (!residual.ContainsKey(rev)) residual[rev] = 0L;
            }

            long maxflow = 0L;
            while (true)
            {
                ct.ThrowIfCancellationRequested();

                // 在残量网络中用 BFS 找增广路 / BFS to find augmenting path in residual graph
                var q = new Queue<NodeType>();
                var parent = new Dictionary<NodeType, NodeType>();
                var edgeFrom = new Dictionary<NodeType, (NodeType U, NodeType V)>();
                var vis = new HashSet<NodeType>();

                q.Enqueue(source);
                vis.Add(source);
                bool found = false;

                while (q.Count > 0 && !found)
                {
                    var u = q.Dequeue();
                    if (!adj.ContainsKey(u)) continue;
                    foreach (var v in adj[u])
                    {
                        ct.ThrowIfCancellationRequested();
                        var cap = residual.ContainsKey((u, v)) ? residual[(u, v)] : 0L;
                        if (cap <= 0) continue;
                        if (vis.Add(v))
                        {
                            parent[v] = u;
                            edgeFrom[v] = (u, v);
                            q.Enqueue(v);
                            if (EqualityComparer<NodeType>.Default.Equals(v, sink)) { found = true; break; }
                        }
                    }
                }

                if (!found) break;

                // 计算瓶颈 / find bottleneck
                var cur = sink; long bottleneck = long.MaxValue;
                while (!EqualityComparer<NodeType>.Default.Equals(cur, source))
                {
                    var ekey = edgeFrom[cur]; var cap = residual[ekey]; bottleneck = Math.Min(bottleneck, cap); cur = parent[cur];
                }

                // 增广 / augment
                cur = sink;
                while (!EqualityComparer<NodeType>.Default.Equals(cur, source))
                {
                    var ekey = edgeFrom[cur]; var rev = (ekey.V, ekey.U);
                    residual[ekey] -= bottleneck;
                    residual[rev] = (residual.ContainsKey(rev) ? residual[rev] : 0L) + bottleneck;
                    cur = parent[cur];
                }

                maxflow = checked(maxflow + bottleneck);
            }

            return maxflow;
        }

        /// <summary>
        /// Dinic 最大流实现（long 容量），返回最大流值。<br/>Dinic max flow implementation for long capacities. Returns max flow value.
        /// </summary>
        public static long DinicMaxFlow<NodeType>(this IGraph<NodeType, long> graph, NodeType source, NodeType sink, CancellationToken ct = default)
            where NodeType : IEquatable<NodeType>
        {
            // 先决条件：Dinic 需要有向图 / Precondition: Dinic expects a directed graph
            if (!graph.IsDirected) throw new GraphX.Error.GraphMethodNotApplicableException("DinicMaxFlow", "Dinic max flow requires a directed graph.", $"directed={graph.IsDirected}");
            // 构建邻接和残量图 / build adjacency residual graph
            var adj = new Dictionary<NodeType, List<NodeType>>();
            var cap = new Dictionary<(NodeType, NodeType), long>();
            foreach (var n in graph.Nodes) adj[n] = new List<NodeType>();
            foreach (var e in graph.WeightedEdges)
            {
                var key = (e.U, e.V);
                if (!adj.ContainsKey(e.U)) adj[e.U] = new List<NodeType>();
                if (!adj.ContainsKey(e.V)) adj[e.V] = new List<NodeType>();
                if (!adj[e.U].Contains(e.V)) adj[e.U].Add(e.V);
                if (!adj[e.V].Contains(e.U)) adj[e.V].Add(e.U);
                cap[key] = (cap.ContainsKey(key) ? cap[key] : 0L) + e.W;
                var rev = (e.V, e.U);
                if (!cap.ContainsKey(rev)) cap[rev] = 0L;
            }

            long flow = 0;
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                // BFS 分层图 / BFS level graph
                var level = new Dictionary<NodeType, int>();
                var q = new Queue<NodeType>();
                q.Enqueue(source); level[source] = 0;
                while (q.Count > 0)
                {
                    var u = q.Dequeue();
                    foreach (var v in adj[u])
                    {
                        if (level.ContainsKey(v)) continue;
                        var capuvCheck = cap.ContainsKey((u, v)) ? cap[(u, v)] : 0L;
                        if (capuvCheck > 0)
                        {
                            level[v] = level[u] + 1;
                            q.Enqueue(v);
                        }
                    }
                }
                if (!level.ContainsKey(sink)) break;

                var iters = new Dictionary<NodeType, int>();
                foreach (var n in adj.Keys) iters[n] = 0;

                long Dfs(NodeType u, long f)
                {
                    if (EqualityComparer<NodeType>.Default.Equals(u, sink)) return f;
                    for (int i = iters[u]; i < adj[u].Count; i++)
                    {
                        iters[u] = i;
                        var v = adj[u][i];
                        if (!level.ContainsKey(v) || level[v] != level[u] + 1) continue;
                        var capuv = cap.ContainsKey((u, v)) ? cap[(u, v)] : 0L;
                        if (capuv <= 0) continue;
                        var pushed = Dfs(v, Math.Min(f, capuv));
                        if (pushed > 0)
                        {
                            cap[(u, v)] = capuv - pushed;
                            cap[(v, u)] = (cap.ContainsKey((v, u)) ? cap[(v, u)] : 0L) + pushed;
                            return pushed;
                        }
                    }
                    return 0L;
                }

                while (true)
                {
                    var pushed = Dfs(source, long.MaxValue);
                    if (pushed == 0) break;
                    flow = checked(flow + pushed);
                }
            }

            // 返回总流量 / return total flow
            return flow;
        }

        /// <summary>
        /// Stoer-Wagner 全局最小割，返回 (minCutWeight, oneSideNodes)。通过提供的 `IWeightOperator` 实现通用权重运算与比较。<br/>Stoer-Wagner global minimum cut. Returns tuple (minCutWeight, oneSideNodes). Implements generic TWeight using provided IWeightOperator for arithmetic and comparison.
        /// </summary>
        public static Tuple<TWeight, List<NodeType>> StoerWagnerMinCut<NodeType, TWeight>(this IGraph<NodeType, TWeight> graph, IWeightOperator<TWeight> op, bool preferSmallerSide = true, CancellationToken ct = default)
            where NodeType : IEquatable<NodeType>
            where TWeight : struct, IComparable<TWeight>
        {
            var partition = StoerWagnerMinCutPartition<NodeType, TWeight>(graph, op, preferSmallerSide, ct);
            return Tuple.Create(partition.Item1, partition.Item2);
        }

        /// <summary>
        /// Stoer-Wagner 的分割辅助：返回 (cutWeight, sideA, sideB)。<br/>Stoer-Wagner partition helper returning (cutWeight, sideA, sideB).
        /// </summary>
        public static Tuple<TWeight, List<NodeType>, List<NodeType>> StoerWagnerMinCutPartition<NodeType, TWeight>(this IGraph<NodeType, TWeight> graph, IWeightOperator<TWeight> op, bool preferSmallerSide = true, CancellationToken ct = default)
            where NodeType : IEquatable<NodeType>
            where TWeight : struct, IComparable<TWeight>
        {
            // 构建邻接权重矩阵 / Build adjacency weight map
            var nodes = graph.Nodes.ToList();
            int n = nodes.Count;
            if (n == 0) return Tuple.Create(op.Zero, new List<NodeType>(), new List<NodeType>());
            if (n == 1) return Tuple.Create(op.Zero, new List<NodeType> { nodes[0] }, new List<NodeType>());

            // 节点到索引映射 / map node to index
            var idx = new Dictionary<NodeType, int>();
            for (int i = 0; i < nodes.Count; i++) idx[nodes[i]] = i;

            // 邻接矩阵（TWeight）/ adjacency matrix using TWeight
            var adj = new TWeight[n, n];
            for (int i = 0; i < n; i++) for (int j = 0; j < n; j++) adj[i, j] = op.Zero;

            foreach (var e in graph.WeightedEdges)
            {
                if (!idx.ContainsKey(e.U) || !idx.ContainsKey(e.V)) continue;
                var iu = idx[e.U];
                var iv = idx[e.V];
                adj[iu, iv] = op.Add(adj[iu, iv], e.W);
                adj[iv, iu] = op.Add(adj[iv, iu], e.W);
            }

            var vertices = Enumerable.Range(0, n).ToList();
            TWeight bestWeight = op.Infinity;
            List<int> bestCut = null;

            while (vertices.Count > 1)
            {
                ct.ThrowIfCancellationRequested();

                var added = new bool[n];
                var weights = new TWeight[n];
                for (int i = 0; i < n; i++) weights[i] = op.Zero;

                int prev = -1;
                int last = -1;
                var order = new List<int>();

                for (int iter = 0; iter < vertices.Count; iter++)
                {
                    // 选择当前权重最大的未加入顶点 / select next vertex with max weight among remaining
                    int sel = -1;
                    for (int i = 0; i < vertices.Count; i++)
                    {
                        int v = vertices[i];
                        if (added[v]) continue;
                        if (sel == -1 || op.Compare(weights[v], weights[sel]) > 0) sel = v;
                    }

                    added[sel] = true;
                    order.Add(sel);
                    prev = last;
                    last = sel;

                    // 更新权重 / update weights
                    foreach (var v in vertices)
                    {
                        if (added[v]) continue;
                        weights[v] = op.Add(weights[v], adj[sel, v]);
                    }
                }

                // last 为 t，prev 为 s；此阶段割重为 weights[last] / last is t, prev is s; cut weight is weights[last]
                var cutWeight = weights[last];
                if (op.Compare(cutWeight, bestWeight) < 0)
                {
                    bestWeight = cutWeight;
                    var sideA = order.Take(order.Count - 1).ToList();
                    bestCut = new List<int>(sideA);
                }
                else if (bestCut != null && op.Compare(cutWeight, bestWeight) == 0)
                {
                    // 平手时根据参数选择较小或较大一侧 / tie-breaker
                    var sideA = order.Take(order.Count - 1).ToList();
                    int sideASize = sideA.Count;
                    int currentBestSize = bestCut.Count;
                    if (preferSmallerSide)
                    {
                        if (sideASize < currentBestSize) bestCut = new List<int>(sideA);
                    }
                    else
                    {
                        if (sideASize > currentBestSize) bestCut = new List<int>(sideA);
                    }
                }

                // 合并顶点：将 last 合并到 prev / merge last into prev
                if (prev == -1) break; // 无需合并 / nothing to merge
                foreach (var v in vertices)
                {
                    if (v == prev || v == last) continue;
                    adj[prev, v] = op.Add(adj[prev, v], adj[last, v]);
                    adj[v, prev] = op.Add(adj[v, prev], adj[v, last]);
                }

                // 从集合中移除 last / remove last from vertices
                vertices.Remove(last);
            }

            // 将索引集转换为节点集 / convert bestCut indices to nodes
            List<NodeType> sideANodes = new();
            List<NodeType> sideBNodes = new();
            if (bestCut == null)
            {
                // 平凡划分 / trivial partition
                sideANodes.Add(nodes[0]);
                sideBNodes = nodes.Skip(1).ToList();
                return Tuple.Create(bestWeight, sideANodes, sideBNodes);
            }

            var inSideA = new HashSet<int>(bestCut);
            for (int i = 0; i < n; i++)
            {
                if (inSideA.Contains(i)) sideANodes.Add(nodes[i]); else sideBNodes.Add(nodes[i]);
            }

            // 根据偏好确保一侧大小 / prefer smaller or larger side per flag
            bool wantSmaller = preferSmallerSide;
            if (wantSmaller)
            {
                if (sideANodes.Count > sideBNodes.Count)
                {
                    var tmp = sideANodes; sideANodes = sideBNodes; sideBNodes = tmp;
                }
            }
            else
            {
                if (sideANodes.Count < sideBNodes.Count)
                {
                    var tmp = sideANodes; sideANodes = sideBNodes; sideBNodes = tmp;
                }
            }

            return Tuple.Create(bestWeight, sideANodes, sideBNodes);
        }

        /// <summary>
        /// Dinic 最小割分割：运行 Dinic 后返回 (maxflow, S, V\S)，其中 S 为残量图中从源仍可达的节点集合。仅支持 long 容量。<br/>Dinic min-cut partition: run Dinic and return (maxflow, S, V\S) where S is set of nodes reachable from source in residual graph. Only supports long capacities.
        /// </summary>
        public static Tuple<long, List<NodeType>, List<NodeType>> DinicMinCutPartition<NodeType>(this IGraph<NodeType, long> graph, NodeType source, NodeType sink, CancellationToken ct = default)
            where NodeType : IEquatable<NodeType>
        {
            // 构建邻接残量图 / Build adjacency residual graph
            var adj = new Dictionary<NodeType, List<NodeType>>();
            var cap = new Dictionary<(NodeType, NodeType), long>();
            foreach (var n in graph.Nodes) adj[n] = new List<NodeType>();
            foreach (var e in graph.WeightedEdges)
            {
                var key = (e.U, e.V);
                if (!adj.ContainsKey(e.U)) adj[e.U] = new List<NodeType>();
                if (!adj.ContainsKey(e.V)) adj[e.V] = new List<NodeType>();
                if (!adj[e.U].Contains(e.V)) adj[e.U].Add(e.V);
                if (!adj[e.V].Contains(e.U)) adj[e.V].Add(e.U);
                cap[key] = (cap.ContainsKey(key) ? cap[key] : 0L) + e.W;
                var rev = (e.V, e.U);
                if (!cap.ContainsKey(rev)) cap[rev] = 0L;
            }

            long flow = 0;
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                // BFS 分层图 / BFS level graph
                var level = new Dictionary<NodeType, int>();
                var q = new Queue<NodeType>();
                q.Enqueue(source); level[source] = 0;
                while (q.Count > 0)
                {
                    var u = q.Dequeue();
                    if (!adj.ContainsKey(u)) continue;
                    foreach (var v in adj[u])
                    {
                        if (level.ContainsKey(v)) continue;
                        var capuv = cap.ContainsKey((u, v)) ? cap[(u, v)] : 0L;
                        if (capuv > 0)
                        {
                            level[v] = level[u] + 1;
                            q.Enqueue(v);
                        }
                    }
                }
                if (!level.ContainsKey(sink)) break;

                var iters = new Dictionary<NodeType, int>();
                foreach (var n in adj.Keys) iters[n] = 0;

                long Dfs(NodeType u, long f)
                {
                    if (EqualityComparer<NodeType>.Default.Equals(u, sink)) return f;
                    if (!adj.ContainsKey(u)) return 0L;
                    for (int i = iters[u]; i < adj[u].Count; i++)
                    {
                        iters[u] = i;
                        var v = adj[u][i];
                        if (!level.ContainsKey(v) || level[v] != level[u] + 1) continue;
                        var capuv = cap.ContainsKey((u, v)) ? cap[(u, v)] : 0L;
                        if (capuv <= 0) continue;
                        var pushed = Dfs(v, Math.Min(f, capuv));
                        if (pushed > 0)
                        {
                            cap[(u, v)] = capuv - pushed;
                            var rev = (v, u);
                            cap[rev] = (cap.ContainsKey(rev) ? cap[rev] : 0L) + pushed;
                            return pushed;
                        }
                    }
                    return 0L;
                }

                while (true)
                {
                    var pushed = Dfs(source, long.MaxValue);
                    if (pushed == 0) break;
                    flow = checked(flow + pushed);
                }
            }

            // 计算残量图中从源可达的集合 S / compute reachable set S
            var visited = new HashSet<NodeType>();
            var q2 = new Queue<NodeType>();
            visited.Add(source); q2.Enqueue(source);
            while (q2.Count > 0)
            {
                var u = q2.Dequeue();
                if (!adj.ContainsKey(u)) continue;
                foreach (var v in adj[u])
                {
                    var c = cap.ContainsKey((u, v)) ? cap[(u, v)] : 0L;
                    if (c > 0 && visited.Add(v)) q2.Enqueue(v);
                }
            }

            var S = visited.ToList();
            var V = graph.Nodes.ToList();
            var complement = V.Where(x => !visited.Contains(x)).ToList();
            return Tuple.Create(flow, S, complement);
        }


    }
}