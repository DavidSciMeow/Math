Meow.Math.Graph — 实现情况

本文件将特性清单（算法、图属性、API 建议）映射到当前仓库的实现状态。每一项标注为：已实现 / 部分实现 / 未实现。对于已实现项包含文件引用；对于未实现项给出简短说明与建议优先级。

说明
- 已实现：仓库中存在相应实现（可能包含已知问题，详见 GRAPH_AUDIT.md）。
- 部分实现：存在接口或部分代码，但完整实现或稳健 API 缺失。
- 未实现：仓库中不存在对应实现。

当前（2025-10-02）已实现/状态一览

1) 路径搜索 / 最短路 算法

- BFS（无权最短路）
  - 状态：已实现
  - 位置：Meow.Math.Graph/Struct/Graph.cs :: BFS
  - 说明：返回遍历顺序，可作为无权最短路径基线。

- DFS（遍历 / 路径存在性）
  - 状态：已实现
  - 位置：Meow.Math.Graph/Struct/Graph.cs :: DFS
  - 说明：基于栈的迭代实现，工作正常。

- Dijkstra（非负权）
  - 状态：已实现（教学版本）
  - 位置：Meow.Math.Graph/Struct/Graph.cs :: Dijkstra, Dijkstra_Old
  - 说明：已将距离类型调整为 long 以避免溢出；仍使用教学用 SimplePriorityQueue（O(n) dequeue）。后续建议为 .NET6 使用 System.PriorityQueue 或提供二叉堆实现以提升性能。

- A*（启发式）
  - 状态：未实现（仅接口存在）
  - 位置：接口 Meow.Math.Graph/Interface/IMatrixPathfinder.cs 声明了 AStar
  - 说明：需要实现并提供启发函数接口（Func<Node,double>）及可选 CancellationToken。

- Bellman–Ford（含负权 / 负环检测）
  - 状态：已实现
  - 位置：Meow.Math.Graph/Struct/Graph.cs :: BellmanFord, BellmanFord_Map, BellmanFord_NWCDetector
  - 说明：功能完整，可检测负权环。路径前序存储结构可进一步简化。

- Floyd–Warshall（任意两点最短路）
  - 状态：已实现（已修复对角线初始化问题）
  - 位置：Meow.Math.Graph/Struct/Graph.cs :: FloydWarshall_Map, FloydWarshall
  - 说明：已将对角线初始化为 0，路径矩阵构造可进一步优化以减少 KeyValuePair 的重复创建。

- Johnson（重权重 + Dijkstra）
  - 状态：未实现

- SPFA（队列版 Bellman-Ford）
  - 状态：未实现

- K-shortest paths（Yen / Eppstein）
  - 状态：未实现

- DAG 上最短路（拓扑 DP）
  - 状态：未实现

- 动态 / 增量最短路
  - 状态：未实现

2) 流 / 切 / 匹配 / MST

- 最大流 / 最小割（Edmonds–Karp, Dinic, Push–Relabel）
  - 状态：未实现

- 最小生成树（Kruskal, Prim）
  - 状态：已实现（Prim, Kruskal）
  - 位置：Meow.Math.Graph/Struct/Graph.cs :: MST_Prim, MST_Kruskal
  - 说明：Kruskal 的无向边去重逻辑已改进（使用有序元组判断），仍可进一步用自定义比较器加固。

- 最大匹配（Hopcroft–Karp / Edmonds Blossom）
  - 状态：未实现

- Stoer–Wagner（无向图全局最小割）
  - 状态：未实现

3) 图属性 / 分量 / 结构

- 连通性（连通分量、强连通分量）
  - 状态：部分实现（接口存在）
  - 位置：Meow.Math.Graph/Interface/ICommunityDetection.cs
  - 说明：需要实现 Kosaraju 或 Tarjan 等算法以得到 SCC 与连通分量。

- 桥与关节点检测（Tarjan 变体）
  - 状态：未实现

- 双连通分量
  - 状态：未实现

- 欧拉路径/回路（Hierholzer）
  - 状态：未实现

- 哈密顿路径
  - 状态：未实现（NP 难，可提供回溯/启发式）

- 直径 / 半径 / 离心率 / 平均距离
  - 状态：未实现

4) 中心性 / 社区 / 谱分析

- 度中心性 / 接近中心性 / 谐波中心性
  - 状态：接口存在（ICentralityDetection），实现缺失

- 介数中心性（Brandes 算法）
  - 状态：未实现

- PageRank / 随机游走
  - 状态：部分实现（接口存在），无具体实现

- 社区检测（Louvain / Label Propagation）
  - 状态：未实现

- 谱方法（拉普拉斯矩阵、特征值）
  - 状态：未实现（若实现需引入线性代数库）

5) 匹配 / 同构 / 传递闭包

- 图着色（DSATUR / 贪心）
  - 状态：未实现

- 最大匹配（Hopcroft–Karp / Edmonds Blossom）
  - 状态：未实现

- 传递闭包 / 可达性（Warshall / DFS）
  - 状态：部分实现（FloydWarshall_Map、BFS/DFS）

- 图同构 / 子图同构（VF2 / Ullmann）
  - 状态：未实现

6) 实用工具 / API / 设计

- IGraph 接口
  - 状态：已实现（接口文件存在）

- 统一的路径结果类型（Path Result）
  - 状态：部分实现（存在 NodePath 类，但多数方法仍返回 NodeType[]）

- 为 A* 提供启发函数接口
  - 状态：未实现（需要 API 及实现）

- CancellationToken / 超时支持
  - 状态：未实现

- 抽象优先队列 / 可替换 PQ
  - 状态：未实现（当前使用 SimplePriorityQueue）；建议为 .NET6 使用 System.PriorityQueue 或实现堆

- 线程安全策略
  - 状态：部分实现（Add/Exist/Remove 等有锁，遍历方法无锁）。建议明确线程安全契约或使用 ReaderWriterLockSlim

- 序列化 / 随机图生成器
  - 状态：部分实现（Util.ReadMap、Util.TreePrint），可扩展为导出/导入与图生成工具

7) 其它 / 高级算法

- Tarjan（SCC / 桥 / 关节点）
  - 状态：未实现

- Dinic / Push-Relabel（最大流）
  - 状态：未实现

- Yen / Eppstein（K-shortest）
  - 状态：未实现

- LCA（二进制提升）
  - 状态：未实现

- 布局 / 可视化算法（力导向、分层布局）
  - 状态：未实现

- 基准测试（BenchmarkDotNet）
  - 状态：未实现（建议在优化前后进行对比）

近期已完成的修复（已在代码中提交）
- Comparer/EdgeCircleEqualityComparer.cs：已修复空值与长度检查，改进哈希实现。
- Struct/Graph.cs：
  - 使用内部计数维护 UnDirected/UnWeighted 状态（修复以前被后续边覆盖的问题）。
  - Dijkstra 中将距离类型改为 long，并做兼容性修正（移除 GetValueOrDefault，改用 TryGetValue）。
  - FloydWarshall_Map 已将对角线初始化为 0。

构建状态
- 当前分支构建成功（build successful）。

优先级建议（短期清单）
1. 修复 GRAPH_AUDIT.md 中列出的正确性 bug（已开始并完成若干项）。
2. 用堆替换或补充 SimplePriorityQueue（优先级队列性能提升，对 Dijkstra 效果显著）。
3. 实现 A* 并暴露启发函数 API（高优先级）。
4. 实现 Tarjan/Kosaraju（强连通分量与桥/关节点）。
5. 加固 Kruskal 去重逻辑并完善并查集（按秩合并）。
6. 实现 PageRank 与 Brandes（介数中心性）。
7. 实现 Dinic（最大流）与 Hopcroft–Karp（二分图匹配）。
8. 将 GetAllPaths 改为流式 IEnumerable 或提供最大深度/条数限制。
9. 添加单元测试与基准。

持续更新策略
- 我将把 GRAPH_AUDIT.md 与本文件保持同步：每次对代码做修改后，会更新这两个文档以反映最新修复与状态。若你希望我每次修改后自动提交文档更新，请确认。我会在每次变动后先提交代码改动再更新这两个文档。

如果你同意，我将按照既定顺序继续修复下一项（建议先改进 Kruskal 去重或优化优先队列）。请回复你希望优先修复的序号或说“按计划继续”。