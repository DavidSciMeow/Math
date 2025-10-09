# GraphX — 概览与使用

## 仓库组织（简要）

- `GraphX/` - 库源码（目标 .NET Standard 2.0）
  - `Core/IGraph.cs`：统一的图接口（节点/边/权重/基本操作）。
  - `Graph/`：图实现（如 `WeightedGraph.cs`、便利构造函数）。
  - `Algorithms/GraphMethods.cs`：静态扩展算法集合（Dijkstra、Bellman-Ford、A*, Floyd-Warshall、Johnson、Kruskal、Prim、Edmonds–Karp、Dinic、SCC、Bridges、Articulation 等）。
  - `Algorithms/GraphMethods.Parser.cs`：文本解析器（将简单文本描述解析为图）。
  - `Error/`：自定义异常类型（`GraphMethodNotApplicableException`、`BFANWCDetectedException`、`InvalidArgumentException` 等）。
  - `Helpers/LongWeightOperator.cs`：权重运算抽象实现（示例用于 long）。
  - `Util/`：工具数据结构（`BinaryHeap`、`UnionFind` 等）。
  - `Structs/PathResult.cs`：路径返回容器（节点序列、代价、可达性）。

- `GraphX.Tests/` - 单元/集成测试与示例（`.json` 测试用例、解析器 + 算法集成测试）。

## 基本用法（示例）

1. 构造或解析一个图

```csharp
// 使用库内的文本解析器（支持 a--b / a->b / 带权重的 a->b:W）
var baseG = GraphX.Algorithms.GraphTextParser.Parse("s->a:1\na->b:2\n", out _);
IGraph<string,long> ig = baseG;
```

2. 调用算法（作为 `IGraph` 的扩展方法）

```csharp
// 最短路 - Dijkstra
var result = ig.Dijkstra<string,long>("s", "b", new LongWeightOperator(), includeNodeWeight:false);
if (result.Reachable) Console.WriteLine($"cost={result.Cost}");

// Bellman-Ford（检测负环）
var bf = ig.BellmanFord<string,long>("s", new LongWeightOperator());
var distMap = bf.Item1;

// 最小生成树 - Kruskal / Prim（适用于无向图）
var mst = ig.Kruskal<string,long>();
var prim = ig.Prim<string,long>(ig.Nodes.First());

// 拓扑排序（仅针对有向无环图）
var order = ig.TopologicalSort<string,long>();

// 最大流（Edmonds-Karp / Dinic，适用于有向图）
long mf = ig.EdmondsKarpMaxFlow<string>("s","t");
long mf2 = ig.DinicMaxFlow<string>("s","t");
```

3. 异常与前置条件

- 若图不满足某个算法的前置条件（例如 Dijkstra 要求非负权、Kruskal/Prim 要求无向、拓扑排序要求有向等），将抛出 `GraphX.Error.GraphMethodNotApplicableException`，异常包含 `MethodName`、`Reason`、`GraphSummary`，用于诊断。
- 若检测到负权回路，相关最短路算法（Bellman-Ford / SPFA / Johnson 检测阶段）会抛出 `BFANWCDetectedException`。

## 注意事项

- 所有算法为扩展方法，传入 `IGraph<NodeType,TWeight>` 即可调用。
- 权重运算通过 `IWeightOperator<T>` 抽象，常用实现 `LongWeightOperator` 可直接使用。
- 在并发或长运行场景下，许多算法支持 `CancellationToken` 参数用于取消。

更多细节请参考 `GraphX/Algorithms/GraphMethods.cs` 中的每个方法注释。