Meow.Math.Graph �� ʵ�����

���ļ��������嵥���㷨��ͼ���ԡ�API ���飩ӳ�䵽��ǰ�ֿ��ʵ��״̬��ÿһ���עΪ����ʵ�� / ����ʵ�� / δʵ�֡�������ʵ��������ļ����ã�����δʵ����������˵���뽨�����ȼ���

˵��
- ��ʵ�֣��ֿ��д�����Ӧʵ�֣����ܰ�����֪���⣬��� GRAPH_AUDIT.md����
- ����ʵ�֣����ڽӿڻ򲿷ִ��룬������ʵ�ֻ��Ƚ� API ȱʧ��
- δʵ�֣��ֿ��в����ڶ�Ӧʵ�֡�

��ǰ��2025-10-02����ʵ��/״̬һ��

1) ·������ / ���· �㷨

- BFS����Ȩ���·��
  - ״̬����ʵ��
  - λ�ã�Meow.Math.Graph/Struct/Graph.cs :: BFS
  - ˵�������ر���˳�򣬿���Ϊ��Ȩ���·�����ߡ�

- DFS������ / ·�������ԣ�
  - ״̬����ʵ��
  - λ�ã�Meow.Math.Graph/Struct/Graph.cs :: DFS
  - ˵��������ջ�ĵ���ʵ�֣�����������

- Dijkstra���Ǹ�Ȩ��
  - ״̬����ʵ�֣���ѧ�汾��
  - λ�ã�Meow.Math.Graph/Struct/Graph.cs :: Dijkstra, Dijkstra_Old
  - ˵�����ѽ��������͵���Ϊ long �Ա����������ʹ�ý�ѧ�� SimplePriorityQueue��O(n) dequeue������������Ϊ .NET6 ʹ�� System.PriorityQueue ���ṩ�����ʵ�����������ܡ�

- A*������ʽ��
  - ״̬��δʵ�֣����ӿڴ��ڣ�
  - λ�ã��ӿ� Meow.Math.Graph/Interface/IMatrixPathfinder.cs ������ AStar
  - ˵������Ҫʵ�ֲ��ṩ���������ӿڣ�Func<Node,double>������ѡ CancellationToken��

- Bellman�CFord������Ȩ / ������⣩
  - ״̬����ʵ��
  - λ�ã�Meow.Math.Graph/Struct/Graph.cs :: BellmanFord, BellmanFord_Map, BellmanFord_NWCDetector
  - ˵���������������ɼ�⸺Ȩ����·��ǰ��洢�ṹ�ɽ�һ���򻯡�

- Floyd�CWarshall�������������·��
  - ״̬����ʵ�֣����޸��Խ��߳�ʼ�����⣩
  - λ�ã�Meow.Math.Graph/Struct/Graph.cs :: FloydWarshall_Map, FloydWarshall
  - ˵�����ѽ��Խ��߳�ʼ��Ϊ 0��·��������ɽ�һ���Ż��Լ��� KeyValuePair ���ظ�������

- Johnson����Ȩ�� + Dijkstra��
  - ״̬��δʵ��

- SPFA�����а� Bellman-Ford��
  - ״̬��δʵ��

- K-shortest paths��Yen / Eppstein��
  - ״̬��δʵ��

- DAG �����·������ DP��
  - ״̬��δʵ��

- ��̬ / �������·
  - ״̬��δʵ��

2) �� / �� / ƥ�� / MST

- ����� / ��С�Edmonds�CKarp, Dinic, Push�CRelabel��
  - ״̬��δʵ��

- ��С��������Kruskal, Prim��
  - ״̬����ʵ�֣�Prim, Kruskal��
  - λ�ã�Meow.Math.Graph/Struct/Graph.cs :: MST_Prim, MST_Kruskal
  - ˵����Kruskal �������ȥ���߼��ѸĽ���ʹ������Ԫ���жϣ����Կɽ�һ�����Զ���Ƚ����ӹ̡�

- ���ƥ�䣨Hopcroft�CKarp / Edmonds Blossom��
  - ״̬��δʵ��

- Stoer�CWagner������ͼȫ����С�
  - ״̬��δʵ��

3) ͼ���� / ���� / �ṹ

- ��ͨ�ԣ���ͨ������ǿ��ͨ������
  - ״̬������ʵ�֣��ӿڴ��ڣ�
  - λ�ã�Meow.Math.Graph/Interface/ICommunityDetection.cs
  - ˵������Ҫʵ�� Kosaraju �� Tarjan ���㷨�Եõ� SCC ����ͨ������

- ����ؽڵ��⣨Tarjan ���壩
  - ״̬��δʵ��

- ˫��ͨ����
  - ״̬��δʵ��

- ŷ��·��/��·��Hierholzer��
  - ״̬��δʵ��

- ���ܶ�·��
  - ״̬��δʵ�֣�NP �ѣ����ṩ����/����ʽ��

- ֱ�� / �뾶 / ������ / ƽ������
  - ״̬��δʵ��

4) ������ / ���� / �׷���

- �������� / �ӽ������� / г��������
  - ״̬���ӿڴ��ڣ�ICentralityDetection����ʵ��ȱʧ

- ���������ԣ�Brandes �㷨��
  - ״̬��δʵ��

- PageRank / �������
  - ״̬������ʵ�֣��ӿڴ��ڣ����޾���ʵ��

- ������⣨Louvain / Label Propagation��
  - ״̬��δʵ��

- �׷�����������˹��������ֵ��
  - ״̬��δʵ�֣���ʵ�����������Դ����⣩

5) ƥ�� / ͬ�� / ���ݱհ�

- ͼ��ɫ��DSATUR / ̰�ģ�
  - ״̬��δʵ��

- ���ƥ�䣨Hopcroft�CKarp / Edmonds Blossom��
  - ״̬��δʵ��

- ���ݱհ� / �ɴ��ԣ�Warshall / DFS��
  - ״̬������ʵ�֣�FloydWarshall_Map��BFS/DFS��

- ͼͬ�� / ��ͼͬ����VF2 / Ullmann��
  - ״̬��δʵ��

6) ʵ�ù��� / API / ���

- IGraph �ӿ�
  - ״̬����ʵ�֣��ӿ��ļ����ڣ�

- ͳһ��·��������ͣ�Path Result��
  - ״̬������ʵ�֣����� NodePath �࣬�����������Է��� NodeType[]��

- Ϊ A* �ṩ���������ӿ�
  - ״̬��δʵ�֣���Ҫ API ��ʵ�֣�

- CancellationToken / ��ʱ֧��
  - ״̬��δʵ��

- �������ȶ��� / ���滻 PQ
  - ״̬��δʵ�֣���ǰʹ�� SimplePriorityQueue��������Ϊ .NET6 ʹ�� System.PriorityQueue ��ʵ�ֶ�

- �̰߳�ȫ����
  - ״̬������ʵ�֣�Add/Exist/Remove ������������������������������ȷ�̰߳�ȫ��Լ��ʹ�� ReaderWriterLockSlim

- ���л� / ���ͼ������
  - ״̬������ʵ�֣�Util.ReadMap��Util.TreePrint��������չΪ����/������ͼ���ɹ���

7) ���� / �߼��㷨

- Tarjan��SCC / �� / �ؽڵ㣩
  - ״̬��δʵ��

- Dinic / Push-Relabel���������
  - ״̬��δʵ��

- Yen / Eppstein��K-shortest��
  - ״̬��δʵ��

- LCA��������������
  - ״̬��δʵ��

- ���� / ���ӻ��㷨�������򡢷ֲ㲼�֣�
  - ״̬��δʵ��

- ��׼���ԣ�BenchmarkDotNet��
  - ״̬��δʵ�֣��������Ż�ǰ����жԱȣ�

��������ɵ��޸������ڴ������ύ��
- Comparer/EdgeCircleEqualityComparer.cs�����޸���ֵ�볤�ȼ�飬�Ľ���ϣʵ�֡�
- Struct/Graph.cs��
  - ʹ���ڲ�����ά�� UnDirected/UnWeighted ״̬���޸���ǰ�������߸��ǵ����⣩��
  - Dijkstra �н��������͸�Ϊ long�������������������Ƴ� GetValueOrDefault������ TryGetValue����
  - FloydWarshall_Map �ѽ��Խ��߳�ʼ��Ϊ 0��

����״̬
- ��ǰ��֧�����ɹ���build successful����

���ȼ����飨�����嵥��
1. �޸� GRAPH_AUDIT.md ���г�����ȷ�� bug���ѿ�ʼ������������
2. �ö��滻�򲹳� SimplePriorityQueue�����ȼ����������������� Dijkstra Ч����������
3. ʵ�� A* ����¶�������� API�������ȼ�����
4. ʵ�� Tarjan/Kosaraju��ǿ��ͨ��������/�ؽڵ㣩��
5. �ӹ� Kruskal ȥ���߼������Ʋ��鼯�����Ⱥϲ�����
6. ʵ�� PageRank �� Brandes�����������ԣ���
7. ʵ�� Dinic����������� Hopcroft�CKarp������ͼƥ�䣩��
8. �� GetAllPaths ��Ϊ��ʽ IEnumerable ���ṩ������/�������ơ�
9. ��ӵ�Ԫ�������׼��

�������²���
- �ҽ��� GRAPH_AUDIT.md �뱾�ļ�����ͬ����ÿ�ζԴ������޸ĺ󣬻�����������ĵ��Է�ӳ�����޸���״̬������ϣ����ÿ���޸ĺ��Զ��ύ�ĵ����£���ȷ�ϡ��һ���ÿ�α䶯�����ύ����Ķ��ٸ����������ĵ���

�����ͬ�⣬�ҽ����ռȶ�˳������޸���һ������ȸĽ� Kruskal ȥ�ػ��Ż����ȶ��У�����ظ���ϣ�������޸�����Ż�˵�����ƻ���������