# GraphX — 概览与使用

## 仓库组织（更新）

- `GraphX/` - 库源码（目标：.NET Standard 2.0）
  - `Core/IGraph.cs`：图接口（节点/边/权重/基本操作）。
  - `Core/Graph.cs`：图的核心实现与辅助代码。
  - `Graph/`：图的具体实现（例如加权图等，若存在）。
  - `Algorithms/GraphMethods.cs`：算法集合（最短路、最小生成树、最大流、拓扑等的实现与扩展方法）。
  - `Algorithms/GraphMethods.Parser.cs`：文本解析器（将简单文本描述解析为图）。
  - `Parser/GraphTextParserBase.cs`：解析器基础抽象（可实现自定义输入格式解析）。
  - `Parser/DefaultTextParser.cs`：默认的文本解析实现（支持 a--b / a->b / a->b:W 语法）。
  - `Error/`：自定义异常类型（如 `GraphMethodNotApplicableException`、`BFANWCDetectedException`、`InvalidArgumentException` 等）。
  - `Helpers/LongWeightOperator.cs`：权重运算抽象与 `long` 的实现示例。
  - `Util/`：工具数据结构（`BinaryHeap`、`UnionFind` 等）。
  - `Structs/PathResult.cs`：路径返回容器（节点序列、代价、可达性）。

- 其他项目
  - `GraphX.Tests/` - 单元/集成测试（目标：.NET 6）。
  - `Bit/`、`Meow.Math.Fraction/` 等为工作区中的其他数学工具库项目。

> 注意：以上路径基于当前工作区布局，实际文件名或子目录可能随实现调整。

## 解析器设计（新增）

- 提供了可扩展的解析器抽象 `Parser/GraphTextParserBase.cs`，用于将任意输入（文本、CSV、逐行 JSON、XML 片段等）映射为图的连接描述。

- 默认实现 `Parser/DefaultTextParser.cs` 支持简明语法：
  - `a--b`（无向、无权）
  - `a->b`（有向、无权）
  - `a->b:W`（有向、有权）
  - `a--b:W`（无向、有权）

- 使用方式示例：

```csharp
// 使用默认解析器
var parser = new GraphX.Parser.DefaultTextParser<string,long>();
var g = parser.Parse("s->a:1\na->b:2\n", new LongWeightOperator(), out var inferred);

// 自定义解析器：实现 GraphTextParserBase 来解析 JSON 或其它格式
var myParser = new MyCustomParser<MyIdType,double>();
var g2 = myParser.Parse(text, myOp, out var type);
```

## 快速示例：如何解析 JSON / XML / CSV

下面给出几种常见输入格式的最小实现思路（中文在前，英文 在后）：

1) JSON（整个文本为 JSON 数组）<br/>
- 思路：使用 `System.Text.Json.JsonSerializer` 将文本反序列化为 DTO 数组（每项包含 `u, v, w, d`），然后遍历 DTO 构造图。<br/>
- Idea: use `System.Text.Json.JsonSerializer` to deserialize to DTOs (fields `u, v, w, d`) and build the graph.

```csharp
public class EdgeDto { public string u { get; set; } public string v { get; set; } public long w { get; set; } public bool d { get; set; } }
var list = System.Text.Json.JsonSerializer.Deserialize<List<EdgeDto>>(jsonText);
var g = new Graph<string,long>(new LongWeightOperator());
foreach (var e in list) { if (!g.Exist(e.u)) g.AddNode(e.u); if (!g.Exist(e.v)) g.AddNode(e.v); g.AddEdge(e.u, e.v, e.w, e.d); }
```

2) 逐行 JSON（每行是一个小对象）<br/>
- 思路：逐行读取，逐行调用 `JsonSerializer.Deserialize<EdgeDto>(line)`，或实现 `GraphTextParserBase` 的子类去解析每行并返回 token。<br/>
- Idea: parse line-by-line with `JsonSerializer` or subclass `GraphTextParserBase` to extract tokens per line.

```csharp
var parser = new JsonLineParser(); // 示例：继承 GraphTextParserBase 并实现 TryParseTokens
var g = parser.Parse(text, new LongWeightOperator(), out var type);
```

3) XML（全部为一个 XML 文档，若为逐条也可）<br/>
- 思路：使用 `System.Xml.Linq.XDocument` 或 `XmlDocument`，找到每个 `<edge u="A" v="B" w="3" d="true" />` 节点，读取属性并加入图。<br/>
- Idea: use `System.Xml.Linq.XDocument` to select edge elements and read attributes.

```csharp
var doc = System.Xml.Linq.XDocument.Parse(xmlText);
var g = new Graph<string,long>(new LongWeightOperator());
foreach (var xe in doc.Descendants("edge")) {
  var u = (string)xe.Attribute("u"); var v = (string)xe.Attribute("v");
  var w = (long?)xe.Attribute("w") ?? 1; var d = (bool?)xe.Attribute("d") ?? false;
  if (!g.Exist(u)) g.AddNode(u); if (!g.Exist(v)) g.AddNode(v);
  g.AddEdge(u, v, w, d);
}
```

4) CSV（简单逗号分隔，列为 u,v,w,d）<br/>
- 思路：按行 split('\n')，再对每行 `line.Split(',')`，索引列并构造节点/边。对含引号/转义的 CSV 可使用专用库（如 `CsvHelper`）。<br/>
- Idea: split lines and columns; for robust CSV use `CsvHelper` or similar.

```csharp
var g = new Graph<string,long>(new LongWeightOperator());
foreach (var line in csvText.Split(new[]{'\r','\n'}, StringSplitOptions.RemoveEmptyEntries)) {
  var cols = line.Split(','); var u = cols[0].Trim('"'); var v = cols[1].Trim('"');
  var w = cols.Length>2 ? long.Parse(cols[2]) : 1; var d = cols.Length>3 ? bool.Parse(cols[3]) : false;
  if (!g.Exist(u)) g.AddNode(u); if (!g.Exist(v)) g.AddNode(v);
  g.AddEdge(u, v, w, d);
}
```

5) 使用 `GraphTextParserBase` 扩展自定义格式（推荐）<br/>
- 思路：继承 `GraphTextParserBase<NodeType,TWeight>` 并实现 `TryParseTokens`，该方法需从一行或一段文本中提取 `leftToken, rightToken, weightToken, directed`，然后调用基类 `Parse` 完成转换与图构造。<br/>
- Idea: implement `TryParseTokens` in a subclass; base class handles conversion and graph construction.

```csharp
// 伪代码：实现 TryParseTokens 将 JSON / XML / CSV 的一条记录映射为 tokens
class MyParser : GraphTextParserBase<string,long> {
  protected override bool TryParseTokens(string line, out string left, out string right, out string? w, out bool d) { ... }
}
var my = new MyParser(); var g = my.Parse(text, new LongWeightOperator(), out var type);
```

## 使用示例

1. 构造或解析一个图

```csharp
// 兼容旧版调用（返回 Graph<string,long>）
var baseG = GraphX.Core.Graph.Parse("s->a:1\na->b:2\n", new LongWeightOperator(), out _);
IGraph<string,long> ig = baseG;

// 或使用 Parser 抽象
var parser = new GraphX.Parser.DefaultTextParser<string,long>();
var g = parser.Parse("s->a:1\na->b:2\n", new LongWeightOperator(), out _);
IGraph<string,long> ig2 = g;
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

- 若图不满足某个算法的前置条件（例如 Dijkstra 要求非负权、Kruskal/Prim 要求无向、拓扑排序要求无环等），将
- 抛出 `GraphX.Error.GraphMethodNotApplicableException`，异常包含 `MethodName`、`Reason`、`GraphSummary`，便于诊断。
- 若检测到负权回路，相关最短路算法（Bellman-Ford / Johnson 的检测阶段）会抛出 `BFANWCDetectedException`。

## 测试

- 新增了解析器相关测试：`GraphX.Tests/ParserTests/CustomParserTests.cs`，包含对 `DefaultTextParser` 的基本解析检查以及一个示例自定义逐行 JSON 解析器的测试。

更多细节与示例，请查看 `GraphX` 项目源码与 `GraphX.Tests` 中的测试用例。