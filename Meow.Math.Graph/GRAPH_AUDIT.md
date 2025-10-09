Graph 库审计与修复计划

概览

本文件是对 Meow.Math.Graph 项目的代码审计报告，列出发现的问题（包括 bug、正确性、性能、API 设计与线程安全方面），并按优先级提供详细的修复计划。文档后半部分给出具体的补丁计划：需要修改的文件、修改内容与修改顺序。

仓库上下文

- 项目: Meow.Math.Graph (netstandard2.0)
- 主要检查文件: Struct/Graph.cs, Util.cs, Comparer/EdgeCircleEqualityComparer.cs

主要发现（高层次）

1. 正确性
- EdgeCircleEqualityComparer.Equals 在比较数组时未检查长度，可能导致 IndexOutOfRangeException。
- Graph.Link / Graph.LinkTo 在每次添加边时直接写入 UnDirected 与 UnWeighted 布尔值，会被后续边修改，导致全局状态不正确。
- FloydWarshall_Map 未将对角线设为 0，导致 i->i 路径被视为不可达或计算错误。
- Bellman–Ford 的前序存储使用 tuple 包含重复项，表意混乱但功能上可用。

2. 性能
- SimplePriorityQueue 的 Dequeue 是 O(n)，适用于教学但在大图上性能差。
- DFS/TreePrint 在部分位置使用 ElementAt/HashSet.Last 导致 O(n^2) 行为，HashSet 的无序性也导致输出不确定。
- Kruskal 使用 GetHashCode 拼接做无向边去重，存在哈希冲突风险和实现脆弱。
- GetAllPaths* 在没有限制的情况下会指数爆炸，可能耗尽内存/CPU。

3. 线程安全
- 部分公开方法使用 lockobj，但其他遍历 NodeTable 的方法没有上锁。在并发修改场景下存在竞态风险。

4. API / 可用性
- 路径返回类型不统一；缺少统一的 Path 结果类型（包含节点序列与成本等）。
- 缺少对长时运行搜索的取消/超时支持（CancellationToken）。

详细问题、严重性与建议修复

A. EdgeCircleEqualityComparer.cs
- 严重性: 高（可能崩溃）
- 问题: Equals 假设数组长度相同且非空；GetHashCode 使用 XOR 易冲突。
- 修复: 增加空值与长度检查，改用更稳健的哈希组合方法（HashCode 或未溢出乘法）。

B. Graph.Link / Graph.LinkTo 标志管理
- 严重性: 高（语义性 bug，影响算法）
- 问题: 每次 Link/LinkTo 以最后一次添加的边决定 UnWeighted/UnDirected 状态。
- 修复: 增加计数字段（_directedEdgeCount、_undirectedEdgeCount、_weightedEdgeCount），根据计数计算属性：UnDirected = _undirectedEdgeCount > 0 && _directedEdgeCount == 0; UnWeighted = _weightedEdgeCount == 0。

C. FloydWarshall_Map 对角线
- 严重性: 高（最短路径结果错误）
- 问题: 未将 i==j 的项设为 0。
- 修复: 初始化时将对角线设置为 0，并在需要时为 mPr 指定默认值。

D. SimplePriorityQueue 与 Dijkstra
- 严重性: 中
- 问题: Dequeue O(n)；dist 使用 int 且从 long 强转，可能溢出。
- 修复: 将距离类型改为 long；为 net6 提供 System.PriorityQueue 实现或为 netstandard2.0 提供二叉堆实现。

E. GetAllPathsFromStartToEnd
- 严重性: 中（性能/拒绝服务）
- 问题: 没有边界或流式输出。
- 修复: 提供带 maxDepth 或 maxPaths 的重载，或改为返回 IEnumerable<NodeType[]> 并使用 yield return。

F. TreePrint / Util.ReadMap
- 严重性: 低/中
- 问题: 使用 ElementAt/HashSet.Last，导致不确定顺序与性能问题。
- 修复: 使用 List 来维护确定顺序，避免重复 ElementAt。

G. Kruskal 无向边去重逻辑
- 严重性: 低/中
- 问题: 使用 GetHashCode 拼接构造唯一键可能冲突。
- 修复: 使用节点按序的 Tuple<NodeType,NodeType> 并使用合适的比较器进行去重。

H. 线程安全
- 严重性: 低/中（取决用法）
- 问题: 局部加锁，但遍历未上锁。
- 修复: 明确线程安全契约，或使用 ReaderWriterLockSlim 包裹 NodeTable 的读写操作。

修复步骤（有序）

1. 在仓库中创建审计文档（已完成）。
2. 修复 EdgeCircleEqualityComparer（快速修复）。
3. 修改 Graph.cs 中 Link/LinkTo 的标志管理：使用计数并计算属性。
4. 修复 FloydWarshall_Map 对角线初始化。
5. 对 Dijkstra 做小幅改进：使用 long 记录距离，并为 .NET6 使用 System.PriorityQueue（可选）。
6. 改进 Kruskal 的无向边去重逻辑（使用有序元组）。
7. 为 GetAllPathsFromStartToEnd 提供最大深度重载或改为 IEnumerable。
8. 用索引集合替换 Util.TreePrint 中对 ElementAt/Last 的调用以避免 O(n^2)。

首轮修改目标文件

- Meow.Math.Graph/Comparer/EdgeCircleEqualityComparer.cs（已修复）
- Meow.Math.Graph/Struct/Graph.cs（将按计划修改 Link/LinkTo、FloydWarshall_Map、Dijkstra）
- Meow.Math.Graph/Util.cs（TreePrint 优化，稍后修改）

实现说明与 API 兼容性

- 修复将尽量保持向后兼容。会在内部添加计数字段，但公共 API 保持不变，除非修复必须改变语义。
- 首轮不会做大规模重构；后续可以引入 Path 结果类型与可取消 token 支持。

下一步动作

我将按顺序在代码中应用首轮修复：
1) EdgeCircleEqualityComparer（已完成）
2) Graph.cs：更新 Link/LinkTo 计数和属性计算；修复 FloydWarshall_Map 对角线；将 Dijkstra 中距离类型改为 long

每次修改后我会运行构建并修复产生的编译错误，然后继续下一项。现在我将继续执行 Graph.cs 的第一批修改：Link/LinkTo 行为修正，FloydWarshall 对角线修复，以及 Dijkstra 距离类型改为 long。
