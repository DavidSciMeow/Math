# GraphX �� ������ʹ��

## �ֿ���֯����Ҫ��

- `GraphX/` - ��Դ�루Ŀ�� .NET Standard 2.0��
  - `Core/IGraph.cs`��ͳһ��ͼ�ӿڣ��ڵ�/��/Ȩ��/������������
  - `Graph/`��ͼʵ�֣��� `WeightedGraph.cs`���������캯������
  - `Algorithms/GraphMethods.cs`����̬��չ�㷨���ϣ�Dijkstra��Bellman-Ford��A*, Floyd-Warshall��Johnson��Kruskal��Prim��Edmonds�CKarp��Dinic��SCC��Bridges��Articulation �ȣ���
  - `Algorithms/GraphMethods.Parser.cs`���ı��������������ı���������Ϊͼ����
  - `Error/`���Զ����쳣���ͣ�`GraphMethodNotApplicableException`��`BFANWCDetectedException`��`InvalidArgumentException` �ȣ���
  - `Helpers/LongWeightOperator.cs`��Ȩ���������ʵ�֣�ʾ������ long����
  - `Util/`���������ݽṹ��`BinaryHeap`��`UnionFind` �ȣ���
  - `Structs/PathResult.cs`��·�������������ڵ����С����ۡ��ɴ��ԣ���

- `GraphX.Tests/` - ��Ԫ/���ɲ�����ʾ����`.json` ���������������� + �㷨���ɲ��ԣ���

## �����÷���ʾ����

1. ��������һ��ͼ

```csharp
// ʹ�ÿ��ڵ��ı���������֧�� a--b / a->b / ��Ȩ�ص� a->b:W��
var baseG = GraphX.Algorithms.GraphTextParser.Parse("s->a:1\na->b:2\n", out _);
IGraph<string,long> ig = baseG;
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

- ��ͼ������ĳ���㷨��ǰ������������ Dijkstra Ҫ��Ǹ�Ȩ��Kruskal/Prim Ҫ��������������Ҫ������ȣ������׳� `GraphX.Error.GraphMethodNotApplicableException`���쳣���� `MethodName`��`Reason`��`GraphSummary`��������ϡ�
- ����⵽��Ȩ��·��������·�㷨��Bellman-Ford / SPFA / Johnson ���׶Σ����׳� `BFANWCDetectedException`��

## ע������

- �����㷨Ϊ��չ���������� `IGraph<NodeType,TWeight>` ���ɵ��á�
- Ȩ������ͨ�� `IWeightOperator<T>` ���󣬳���ʵ�� `LongWeightOperator` ��ֱ��ʹ�á�
- �ڲ��������г����£�����㷨֧�� `CancellationToken` ��������ȡ����

����ϸ����ο� `GraphX/Algorithms/GraphMethods.cs` �е�ÿ������ע�͡�