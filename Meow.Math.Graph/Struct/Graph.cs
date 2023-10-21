using Meow.Math.Graph.ErrorList;
using Meow.Math.Graph.Interface;
using Meow.Math.Graph.Struct.Comparer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Meow.Math.Graph.Struct
{
    /// <summary>
    /// 一般图<br/>General Graph
    /// </summary>
    /// <typeparam name="NodeType">节点类型<br/>NodeType</typeparam>
    public class Graph<NodeType> : IGraph<NodeType>, IGPathfinder<NodeType>, IMST<NodeType> where NodeType : IEquatable<NodeType>
    {
        private readonly object lockobj = new object();
        private IDictionary<NodeType, Node<NodeType>> NodeTable { get; } = new Dictionary<NodeType, Node<NodeType>>();
        public bool UnDirected { get; private set; } = false;
        public bool UnWeighted { get; private set; } = false;

        public bool Add(NodeType id)
        {
            lock (lockobj)
            {
                if (NodeTable.ContainsKey(id)) return false;
                NodeTable.Add(id, new Node<NodeType>(id));
                return true;
            }
        }
        public bool Exist(NodeType id) => NodeTable.ContainsKey(id);
        public bool Remove(NodeType id)
        {
            lock (lockobj)
            {
                if (!NodeTable.ContainsKey(id)) return false;
                return NodeTable.Remove(id);
            }
        }
        public Node<NodeType> this[NodeType id] => NodeTable.ContainsKey(id) ? NodeTable[id] : throw new NodeNotExistException();
        public bool Link(NodeType node1, NodeType node2, int weight = 1)
        {
            if (weight != 1) UnWeighted = true;
            return this[node1][node2, weight] && this[node2][node1, weight];
        }
        public bool LinkTo(NodeType node1, NodeType node2, int weight = 1)
        {
            if (weight != 1) UnWeighted = true;
            UnDirected = true;
            return this[node1][node2, weight];
        }
        public int PathWeight(NodeType[] nodelist)
        {
            int totalWeight = 0;
            NodeType temp = default;
            foreach (var node in nodelist)
            {
                if (temp is NodeType && NodeTable.ContainsKey(temp))
                {
                    if (this[temp].Exist(node))
                    {
                        totalWeight += this[temp][node];
                    }
                    else
                    {
                        throw new NodeNotExistException();
                    }
                }
                temp = node;
            }
            return totalWeight;
        }

        public NodeType[] BFS(NodeType start)
        {
            if (!(start is NodeType) || !NodeTable.ContainsKey(start)) throw new NodeNotExistException();
            HashSet<NodeType> visited = new HashSet<NodeType>() { start };//标记头节点
            Queue<NodeType> queue = new Queue<NodeType>();
            List<NodeType> path = new List<NodeType>();
            queue.Enqueue(start);//头节点入队
            while (queue.Count > 0) //σ(n) 任意队列不空
            {
                var s = queue.Peek();//获取队列头元素
                path.Add(s);//添加路径
                queue.Dequeue();//末尾的元素出队
                foreach (var i in NodeTable[s]) //σ(k) 获取节点的邻接节点
                {
                    if (visited.Add(i.Key)) queue.Enqueue(i.Key);//没添加过的元素入队
                }
            }
            return path.ToArray();
        }
        public NodeType[] DFS(NodeType start)
        {
            if (!(start is NodeType) || !NodeTable.ContainsKey(start)) throw new NodeNotExistException();
            HashSet<NodeType> visited = new HashSet<NodeType>() { start };//首元素访问标记
            List<NodeType> path = new List<NodeType>() { start };//头元素入搜索表
            Stack<NodeType> ss = new Stack<NodeType>();
            ss.Push(start);//搜索元素入栈
            while (ss.Any())//σ(n) 若栈不空
            {
                bool _isEdgeVisited = true;//标记边访问
                foreach (var i in NodeTable[ss.Peek()])//σ(n-k) 获得下一节点
                {
                    if (visited.Add(i.Key))//未访问节点则标记访问
                    {
                        ss.Push(i.Key);//入栈
                        path.Add(i.Key);//添加访问表
                        _isEdgeVisited = false;//取消边访问
                        break;
                    }
                }

                if (_isEdgeVisited) ss.Pop();//边访问, 元素出栈
            }
            return path.ToArray();
        }
        public NodeType[] Dijkstra(NodeType start, NodeType end)
        {
            if (!NodeTable.ContainsKey(start) || !NodeTable.ContainsKey(end)) throw new NodeNotExistException();//不存在这两个节点
            Queue<NodeType> queue = new Queue<NodeType>();//生成队列
            queue.Enqueue(start);//头节点入队列
            var edges = new Dictionary<NodeType, NodeType>();//经过的边
            var pathcost = new Dictionary<NodeType, int> { { start, 0 } };//经过的每个节点的数据
            while (queue.Any())//σ(n)队列不空, 进入搜索循环
            {
                var current = queue.Dequeue();//取队列节点
                if (current.Equals(end)) break; //已经搜索到, 退出搜索循环
                foreach (var i in this[current])//任意邻接节点
                {
                    var nowcost = pathcost[current] + i.Value;//计算节点当前权重
                    if (!pathcost.ContainsKey(i.Key) || nowcost < pathcost[i.Key])//若节点不存在记录过的权重, 或者新权重更小则松弛节点
                    {
                        pathcost[i.Key] = nowcost;//权重更新 + (HeuristicFunc?.Invoke(end, i.Key)
                        queue.Enqueue(i.Key);//新节点入队列
                        edges[i.Key] = current;//原节点链接
                    }
                }
            }
            if (edges.ContainsKey(end))
            {
                Stack<NodeType> path = new Stack<NodeType>();//形成路径
                path.Push(end);//末尾位置入栈
                var node = edges[end];//取末尾位置链接点 σ(1)
                while (!node.Equals(start))//前序寻找 最大次数σ(k)
                {
                    path.Push(node);//节点入栈
                    node = edges[node];//前序节点更新
                }
                path.Push(start);//已完成寻找,添加头节点

                return path.ToArray();//返回路径
            }
            else
            {
                throw new NodeUnreachableException();//节点全遍历后不可达
            }
        }
        public (Dictionary<NodeType, int> Dist, Dictionary<NodeType, NodeType> AdjacencyTable) BellmanFord_Map(NodeType start)
        {
            HashSet<Tuple<NodeType, NodeType, int, bool>> Set = new HashSet<Tuple<NodeType, NodeType, int, bool>>(new GeneralEdgeEqualityComparer<NodeType>());
            foreach (var i in NodeTable) //O(n) 全部节点遍历
            {
                foreach (var j in i.Value) //O(j) 每个边
                {
                    Set.Add(new Tuple<NodeType, NodeType, int, bool>(i.Key, j.Key, j.Value, !this[j.Key].Exist(i.Key))); //O(1)
                }
            }

            Dictionary<NodeType, int> dist = new Dictionary<NodeType, int>(); //源点最短路径表
            Dictionary<NodeType, NodeType> path = new Dictionary<NodeType, NodeType>();//前序节点表
            foreach (var i in NodeTable) dist.Add(i.Key, int.MaxValue); //初始化节点表 O(n)
            dist[start] = 0;//设置起始点为0
            for (int i = 1; i <= NodeTable.Count - 1; i++) // 松弛所有边 |V| - 1 次 (并记录任意边的前序) O(n-1)
            {
                foreach (var (u, v, w, _) in Set) //对任意组成边 O(n)
                {
                    if (dist[u] != int.MaxValue && dist[u] + w < dist[v])
                    {
                        if (path.ContainsKey(v))
                        {
                            path[v] = u;
                        }
                        else
                        {
                            path.Add(v, u);
                        }
                        dist[v] = dist[u] + w;
                    }
                }
            }
            return (dist, path);
        }
        public Tuple<NodeType, NodeType, int>[] BellmanFord_NWCDetector(Dictionary<NodeType, int> Dist)
        {
            HashSet<Tuple<NodeType, NodeType, int, bool>> Set = new HashSet<Tuple<NodeType, NodeType, int, bool>>(new GeneralEdgeEqualityComparer<NodeType>());
            foreach (var i in NodeTable) //O(n) 全部节点遍历
            {
                foreach (var j in i.Value) //O(j) 每个边
                {
                    Set.Add(new Tuple<NodeType, NodeType, int, bool>(i.Key, j.Key, j.Value, !this[j.Key].Exist(i.Key))); //O(1)
                }
            }
            List<Tuple<NodeType, NodeType, int>> collection = new List<Tuple<NodeType, NodeType, int>>();
            foreach (var (u, v, w, _) in Set) if (Dist[u] != int.MaxValue && Dist[u] + w < Dist[v]) collection.Add(new Tuple<NodeType, NodeType, int>(u, v, w));
            return collection.ToArray();
        }
        public NodeType[] BellmanFord(NodeType start, NodeType end)
        {
            HashSet<Tuple<NodeType, NodeType, int, bool>> Set = new HashSet<Tuple<NodeType, NodeType, int, bool>>(new GeneralEdgeEqualityComparer<NodeType>());
            foreach (var i in NodeTable) //O(n) 全部节点遍历
            {
                foreach (var j in i.Value) //O(j) 每个边
                {
                    Set.Add(new Tuple<NodeType, NodeType, int, bool>(i.Key, j.Key, j.Value, !this[j.Key].Exist(i.Key))); //O(1)
                }
            }

            Dictionary<NodeType, int> dist = new Dictionary<NodeType, int>(); //源点最短路径表
            Dictionary<NodeType, (NodeType, NodeType, int)> ppath = new Dictionary<NodeType, (NodeType, NodeType, int)>();//前序节点表

            foreach (var i in NodeTable) dist.Add(i.Key, int.MaxValue); //初始化节点表 O(n)
            dist[start] = 0;//设置起始点为0

            for (int i = 1; i <= NodeTable.Count - 1; i++) // 松弛所有边 |V| - 1 次 (并记录任意边的前序) O(n-1)
            {
                foreach (var (u, v, w, _) in Set) //对任意组成边 O(n)
                {
                    if (dist[u] != int.MaxValue && dist[u] + w < dist[v])
                    {
                        ppath.Add(v, (v, u, w));
                        dist[v] = dist[u] + w;
                    }
                }
            }
            

            foreach (var (u, v, w, _) in Set) //负权环检测 O(n)
            {
                if (dist[u] != int.MaxValue && dist[u] + w < dist[v])
                {
                    throw new BFANWCDetectedException();
                }
            }

            Stack<NodeType> path = new Stack<NodeType>(); //逆查所有节点
            while (true)
            {
                var (v, u, _) = ppath[end]; //取前节点边
                path.Push(v); //压倒序节点栈
                end = u;//前移节点
                if (end.Equals(start))
                {
                    path.Push(u);
                    break;
                }
            }
            return path.ToArray();
        }
        public NodeType[] FloydWarshall(NodeType start, NodeType end)
        {
            Dictionary<KeyValuePair<NodeType, NodeType>, int> d = new Dictionary<KeyValuePair<NodeType, NodeType>, int>();
            foreach(var k in NodeTable)
            {
                foreach (var i in NodeTable)
                {
                    foreach (var j in NodeTable)
                    {

                    }
                }
            }
            throw new NotImplementedException();
        }
        public (Dictionary<NodeType, HashSet<NodeType>> Table, NodeType Root) MST_BellmanFord(NodeType start)
        {
            var (_, at) = BellmanFord_Map(start);
            Dictionary<NodeType, HashSet<NodeType>> tree = new Dictionary<NodeType, HashSet<NodeType>>();
            foreach (var i in at)
            {
                if (tree.ContainsKey(i.Value))
                {
                    tree[i.Value].Add(i.Key);
                }
                else
                {
                    tree.Add(i.Value, new HashSet<NodeType>() { i.Key });
                }
            }
            return (tree, start);
        }
        public (Dictionary<NodeType, HashSet<NodeType>> Table, NodeType Root) MST_Prim(NodeType start)
        {
            throw new NotImplementedException();
        }
        public (Dictionary<NodeType, HashSet<NodeType>> Table, NodeType Root) MST_Kruskal(NodeType start)
        {
            throw new NotImplementedException();
        }
    }


    /// <summary>
    /// 最小生成树接口<br/>Minimum Spanning Tree Algorithm
    /// </summary>
    /// <typeparam name="NodeType">节点类型<br/>NodeType</typeparam>
    public interface IMST<NodeType>
    {
        (Dictionary<NodeType, HashSet<NodeType>> Table, NodeType Root) MST_Prim(NodeType start);
        (Dictionary<NodeType, HashSet<NodeType>> Table, NodeType Root) MST_Kruskal(NodeType start);
        /// <summary>
        /// 以 <b>贝尔曼福德算法</b> 为最短路径基准生成最小生成树 <br/> Use <i><b>Bellman-Ford Algorithm</b></i> to create a Minimum Spanning Tree
        /// <para>
        /// 时间复杂度(Time Complexity) :: <i><b><see langword="σ(3n^2-n) ~ O(3n*j*(n-1))" /></b></i><br/>
        /// </para>
        /// </summary>
        /// <param name="start">起始节点(根节点)<br/>Start Node In Graph (Root Node of tree)</param>
        /// <returns>一个最小生成树<br/>Minimum Spanning Tree</returns>
        (Dictionary<NodeType, HashSet<NodeType>> Table, NodeType Root) MST_BellmanFord(NodeType start);
    }
    public interface IMatrixPathfinder<NodeType> : IGPathfinder<NodeType>
    {
        NodeType[] AStar(NodeType start, NodeType end);
    }
    public interface IRandomWalk<NodeType> : IGPathfinder<NodeType>
    {
        NodeType[] RandomWalk(NodeType start);
    }
    public interface ICentralityDetection
    {
        void DegreeCentrality();
        void DegreeAverage();
        void DegreeDistribution();
        void DegreeWeightedCentrality();
        void ClosenessCentrality();
        void HarmonicCentrality();
        void BetweennessCentrality();
        void PageRank();
    }
    public interface ICommunityDetection
    {
        void Measuring_TriangleCount();
        void Measuring_ClusteringCoefficient();
        void Component_StronglyConnect();
        void Component_Connect();
    }
}
