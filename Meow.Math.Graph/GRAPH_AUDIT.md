Graph ��������޸��ƻ�

����

���ļ��Ƕ� Meow.Math.Graph ��Ŀ�Ĵ�����Ʊ��棬�г����ֵ����⣨���� bug����ȷ�ԡ����ܡ�API ������̰߳�ȫ���棩���������ȼ��ṩ��ϸ���޸��ƻ����ĵ���벿�ָ�������Ĳ����ƻ�����Ҫ�޸ĵ��ļ����޸��������޸�˳��

�ֿ�������

- ��Ŀ: Meow.Math.Graph (netstandard2.0)
- ��Ҫ����ļ�: Struct/Graph.cs, Util.cs, Comparer/EdgeCircleEqualityComparer.cs

��Ҫ���֣��߲�Σ�

1. ��ȷ��
- EdgeCircleEqualityComparer.Equals �ڱȽ�����ʱδ��鳤�ȣ����ܵ��� IndexOutOfRangeException��
- Graph.Link / Graph.LinkTo ��ÿ����ӱ�ʱֱ��д�� UnDirected �� UnWeighted ����ֵ���ᱻ�������޸ģ�����ȫ��״̬����ȷ��
- FloydWarshall_Map δ���Խ�����Ϊ 0������ i->i ·������Ϊ���ɴ��������
- Bellman�CFord ��ǰ��洢ʹ�� tuple �����ظ��������ҵ������Ͽ��á�

2. ����
- SimplePriorityQueue �� Dequeue �� O(n)�������ڽ�ѧ���ڴ�ͼ�����ܲ
- DFS/TreePrint �ڲ���λ��ʹ�� ElementAt/HashSet.Last ���� O(n^2) ��Ϊ��HashSet ��������Ҳ���������ȷ����
- Kruskal ʹ�� GetHashCode ƴ���������ȥ�أ����ڹ�ϣ��ͻ���պ�ʵ�ִ�����
- GetAllPaths* ��û�����Ƶ�����»�ָ����ը�����ܺľ��ڴ�/CPU��

3. �̰߳�ȫ
- ���ֹ�������ʹ�� lockobj������������ NodeTable �ķ���û���������ڲ����޸ĳ����´��ھ�̬���ա�

4. API / ������
- ·���������Ͳ�ͳһ��ȱ��ͳһ�� Path ������ͣ������ڵ�������ɱ��ȣ���
- ȱ�ٶԳ�ʱ����������ȡ��/��ʱ֧�֣�CancellationToken����

��ϸ���⡢�������뽨���޸�

A. EdgeCircleEqualityComparer.cs
- ������: �ߣ����ܱ�����
- ����: Equals �������鳤����ͬ�ҷǿգ�GetHashCode ʹ�� XOR �׳�ͻ��
- �޸�: ���ӿ�ֵ�볤�ȼ�飬���ø��Ƚ��Ĺ�ϣ��Ϸ�����HashCode ��δ����˷�����

B. Graph.Link / Graph.LinkTo ��־����
- ������: �ߣ������� bug��Ӱ���㷨��
- ����: ÿ�� Link/LinkTo �����һ����ӵı߾��� UnWeighted/UnDirected ״̬��
- �޸�: ���Ӽ����ֶΣ�_directedEdgeCount��_undirectedEdgeCount��_weightedEdgeCount�������ݼ����������ԣ�UnDirected = _undirectedEdgeCount > 0 && _directedEdgeCount == 0; UnWeighted = _weightedEdgeCount == 0��

C. FloydWarshall_Map �Խ���
- ������: �ߣ����·���������
- ����: δ�� i==j ������Ϊ 0��
- �޸�: ��ʼ��ʱ���Խ�������Ϊ 0��������ҪʱΪ mPr ָ��Ĭ��ֵ��

D. SimplePriorityQueue �� Dijkstra
- ������: ��
- ����: Dequeue O(n)��dist ʹ�� int �Ҵ� long ǿת�����������
- �޸�: ���������͸�Ϊ long��Ϊ net6 �ṩ System.PriorityQueue ʵ�ֻ�Ϊ netstandard2.0 �ṩ�����ʵ�֡�

E. GetAllPathsFromStartToEnd
- ������: �У�����/�ܾ�����
- ����: û�б߽����ʽ�����
- �޸�: �ṩ�� maxDepth �� maxPaths �����أ����Ϊ���� IEnumerable<NodeType[]> ��ʹ�� yield return��

F. TreePrint / Util.ReadMap
- ������: ��/��
- ����: ʹ�� ElementAt/HashSet.Last�����²�ȷ��˳�����������⡣
- �޸�: ʹ�� List ��ά��ȷ��˳�򣬱����ظ� ElementAt��

G. Kruskal �����ȥ���߼�
- ������: ��/��
- ����: ʹ�� GetHashCode ƴ�ӹ���Ψһ�����ܳ�ͻ��
- �޸�: ʹ�ýڵ㰴��� Tuple<NodeType,NodeType> ��ʹ�ú��ʵıȽ�������ȥ�ء�

H. �̰߳�ȫ
- ������: ��/�У�ȡ���÷���
- ����: �ֲ�������������δ������
- �޸�: ��ȷ�̰߳�ȫ��Լ����ʹ�� ReaderWriterLockSlim ���� NodeTable �Ķ�д������

�޸����裨����

1. �ڲֿ��д�������ĵ�������ɣ���
2. �޸� EdgeCircleEqualityComparer�������޸�����
3. �޸� Graph.cs �� Link/LinkTo �ı�־����ʹ�ü������������ԡ�
4. �޸� FloydWarshall_Map �Խ��߳�ʼ����
5. �� Dijkstra ��С���Ľ���ʹ�� long ��¼���룬��Ϊ .NET6 ʹ�� System.PriorityQueue����ѡ����
6. �Ľ� Kruskal �������ȥ���߼���ʹ������Ԫ�飩��
7. Ϊ GetAllPathsFromStartToEnd �ṩ���������ػ��Ϊ IEnumerable��
8. �����������滻 Util.TreePrint �ж� ElementAt/Last �ĵ����Ա��� O(n^2)��

�����޸�Ŀ���ļ�

- Meow.Math.Graph/Comparer/EdgeCircleEqualityComparer.cs�����޸���
- Meow.Math.Graph/Struct/Graph.cs�������ƻ��޸� Link/LinkTo��FloydWarshall_Map��Dijkstra��
- Meow.Math.Graph/Util.cs��TreePrint �Ż����Ժ��޸ģ�

ʵ��˵���� API ������

- �޸����������������ݡ������ڲ���Ӽ����ֶΣ������� API ���ֲ��䣬�����޸�����ı����塣
- ���ֲ��������ģ�ع��������������� Path ����������ȡ�� token ֧�֡�

��һ������

�ҽ���˳���ڴ�����Ӧ�������޸���
1) EdgeCircleEqualityComparer������ɣ�
2) Graph.cs������ Link/LinkTo ���������Լ��㣻�޸� FloydWarshall_Map �Խ��ߣ��� Dijkstra �о������͸�Ϊ long

ÿ���޸ĺ��һ����й������޸������ı������Ȼ�������һ������ҽ�����ִ�� Graph.cs �ĵ�һ���޸ģ�Link/LinkTo ��Ϊ������FloydWarshall �Խ����޸����Լ� Dijkstra �������͸�Ϊ long��
