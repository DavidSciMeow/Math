# GraphX �� ������ʹ��

## �ֿ���֯�����£�

- `GraphX/` - ��Դ�루Ŀ�꣺.NET Standard 2.0��
  - `Core/IGraph.cs`��ͼ�ӿڣ��ڵ�/��/Ȩ��/������������
  - `Core/Graph.cs`��ͼ�ĺ���ʵ���븨�����롣
  - `Graph/`��ͼ�ľ���ʵ�֣������Ȩͼ�ȣ������ڣ���
  - `Algorithms/GraphMethods.cs`���㷨���ϣ����·����С������������������˵ȵ�ʵ������չ��������
  - `Algorithms/GraphMethods.Parser.cs`���ı��������������ı���������Ϊͼ����
  - `Parser/GraphTextParserBase.cs`���������������󣨿�ʵ���Զ��������ʽ��������
  - `Parser/DefaultTextParser.cs`��Ĭ�ϵ��ı�����ʵ�֣�֧�� a--b / a->b / a->b:W �﷨����
  - `Error/`���Զ����쳣���ͣ��� `GraphMethodNotApplicableException`��`BFANWCDetectedException`��`InvalidArgumentException` �ȣ���
  - `Helpers/LongWeightOperator.cs`��Ȩ����������� `long` ��ʵ��ʾ����
  - `Util/`���������ݽṹ��`BinaryHeap`��`UnionFind` �ȣ���
  - `Structs/PathResult.cs`��·�������������ڵ����С����ۡ��ɴ��ԣ���

- ������Ŀ
  - `GraphX.Tests/` - ��Ԫ/���ɲ��ԣ�Ŀ�꣺.NET 6����
  - `Bit/`��`Meow.Math.Fraction/` ��Ϊ�������е�������ѧ���߿���Ŀ��

> ע�⣺����·�����ڵ�ǰ���������֣�ʵ���ļ�������Ŀ¼������ʵ�ֵ�����

## ��������ƣ�������

- �ṩ�˿���չ�Ľ��������� `Parser/GraphTextParserBase.cs`�����ڽ��������루�ı���CSV������ JSON��XML Ƭ�εȣ�ӳ��Ϊͼ������������

- Ĭ��ʵ�� `Parser/DefaultTextParser.cs` ֧�ּ����﷨��
  - `a--b`��������Ȩ��
  - `a->b`��������Ȩ��
  - `a->b:W`��������Ȩ��
  - `a--b:W`��������Ȩ��

- ʹ�÷�ʽʾ����

```csharp
// ʹ��Ĭ�Ͻ�����
var parser = new GraphX.Parser.DefaultTextParser<string,long>();
var g = parser.Parse("s->a:1\na->b:2\n", new LongWeightOperator(), out var inferred);

// �Զ����������ʵ�� GraphTextParserBase ������ JSON ��������ʽ
var myParser = new MyCustomParser<MyIdType,double>();
var g2 = myParser.Parse(text, myOp, out var type);
```

## ����ʾ������ν��� JSON / XML / CSV

����������ֳ��������ʽ����Сʵ��˼·��������ǰ��Ӣ�� �ں󣩣�

1) JSON�������ı�Ϊ JSON ���飩<br/>
- ˼·��ʹ�� `System.Text.Json.JsonSerializer` ���ı������л�Ϊ DTO ���飨ÿ����� `u, v, w, d`����Ȼ����� DTO ����ͼ��<br/>
- Idea: use `System.Text.Json.JsonSerializer` to deserialize to DTOs (fields `u, v, w, d`) and build the graph.

```csharp
public class EdgeDto { public string u { get; set; } public string v { get; set; } public long w { get; set; } public bool d { get; set; } }
var list = System.Text.Json.JsonSerializer.Deserialize<List<EdgeDto>>(jsonText);
var g = new Graph<string,long>(new LongWeightOperator());
foreach (var e in list) { if (!g.Exist(e.u)) g.AddNode(e.u); if (!g.Exist(e.v)) g.AddNode(e.v); g.AddEdge(e.u, e.v, e.w, e.d); }
```

2) ���� JSON��ÿ����һ��С����<br/>
- ˼·�����ж�ȡ�����е��� `JsonSerializer.Deserialize<EdgeDto>(line)`����ʵ�� `GraphTextParserBase` ������ȥ����ÿ�в����� token��<br/>
- Idea: parse line-by-line with `JsonSerializer` or subclass `GraphTextParserBase` to extract tokens per line.

```csharp
var parser = new JsonLineParser(); // ʾ�����̳� GraphTextParserBase ��ʵ�� TryParseTokens
var g = parser.Parse(text, new LongWeightOperator(), out var type);
```

3) XML��ȫ��Ϊһ�� XML �ĵ�����Ϊ����Ҳ�ɣ�<br/>
- ˼·��ʹ�� `System.Xml.Linq.XDocument` �� `XmlDocument`���ҵ�ÿ�� `<edge u="A" v="B" w="3" d="true" />` �ڵ㣬��ȡ���Բ�����ͼ��<br/>
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

4) CSV���򵥶��ŷָ�����Ϊ u,v,w,d��<br/>
- ˼·������ split('\n')���ٶ�ÿ�� `line.Split(',')`�������в�����ڵ�/�ߡ��Ժ�����/ת��� CSV ��ʹ��ר�ÿ⣨�� `CsvHelper`����<br/>
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

5) ʹ�� `GraphTextParserBase` ��չ�Զ����ʽ���Ƽ���<br/>
- ˼·���̳� `GraphTextParserBase<NodeType,TWeight>` ��ʵ�� `TryParseTokens`���÷������һ�л�һ���ı�����ȡ `leftToken, rightToken, weightToken, directed`��Ȼ����û��� `Parse` ���ת����ͼ���졣<br/>
- Idea: implement `TryParseTokens` in a subclass; base class handles conversion and graph construction.

```csharp
// α���룺ʵ�� TryParseTokens �� JSON / XML / CSV ��һ����¼ӳ��Ϊ tokens
class MyParser : GraphTextParserBase<string,long> {
  protected override bool TryParseTokens(string line, out string left, out string right, out string? w, out bool d) { ... }
}
var my = new MyParser(); var g = my.Parse(text, new LongWeightOperator(), out var type);
```

## ʹ��ʾ��

1. ��������һ��ͼ

```csharp
// ���ݾɰ���ã����� Graph<string,long>��
var baseG = GraphX.Core.Graph.Parse("s->a:1\na->b:2\n", new LongWeightOperator(), out _);
IGraph<string,long> ig = baseG;

// ��ʹ�� Parser ����
var parser = new GraphX.Parser.DefaultTextParser<string,long>();
var g = parser.Parse("s->a:1\na->b:2\n", new LongWeightOperator(), out _);
IGraph<string,long> ig2 = g;
```

2. �����㷨����Ϊ `IGraph` ����չ������

```csharp
// ���· - Dijkstra
var result = ig.Dijkstra<string,long>("s", "b", new LongWeightOperator(), includeNodeWeight:false);
if (result.Reachable) Console.WriteLine($"cost={result.Cost}");

// Bellman-Ford����⸺����
var bf = ig.BellmanFord<string,long>("s", new LongWeightOperator());
var distMap = bf.Item1;

// ��С������ - Kruskal / Prim������������ͼ��
var mst = ig.Kruskal<string,long>();
var prim = ig.Prim<string,long>(ig.Nodes.First());

// �������򣨽���������޻�ͼ��
var order = ig.TopologicalSort<string,long>();

// �������Edmonds-Karp / Dinic������������ͼ��
long mf = ig.EdmondsKarpMaxFlow<string>("s","t");
long mf2 = ig.DinicMaxFlow<string>("s","t");
```

3. �쳣��ǰ������

- ��ͼ������ĳ���㷨��ǰ������������ Dijkstra Ҫ��Ǹ�Ȩ��Kruskal/Prim Ҫ��������������Ҫ���޻��ȣ�����
- �׳� `GraphX.Error.GraphMethodNotApplicableException`���쳣���� `MethodName`��`Reason`��`GraphSummary`��������ϡ�
- ����⵽��Ȩ��·��������·�㷨��Bellman-Ford / Johnson �ļ��׶Σ����׳� `BFANWCDetectedException`��

## ����

- �����˽�������ز��ԣ�`GraphX.Tests/ParserTests/CustomParserTests.cs`�������� `DefaultTextParser` �Ļ�����������Լ�һ��ʾ���Զ������� JSON �������Ĳ��ԡ�

����ϸ����ʾ������鿴 `GraphX` ��ĿԴ���� `GraphX.Tests` �еĲ���������