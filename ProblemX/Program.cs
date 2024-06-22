using System.Collections;
using System.Diagnostics;
using System.Text;
using SkiaSharp;

namespace ProblemX
{
    internal class Program
    {
        static void Main(string[] args) {
            List<Task> tasks = new List<Task>();
            for (int i = 6; i < 10; i++)
            {
                tasks.Add(DoTask(100, i));
            }
            while (true)
            {
                var i = Task.WaitAny(tasks.ToArray());
                
            }
            Console.WriteLine("AllCompletes");
        }


        static Task DoTask(int MAX_PT, int SIZE)
        {
            return Task.Factory.StartNew(() =>
            {
                long startMemory = Process.GetCurrentProcess().WorkingSet64;
                var dts = DateTime.Now;
                Dictionary<NodeX, List<NodeX>> NodeTable = new Dictionary<NodeX, List<NodeX>>();
                for (int i = 0; i < MAX_PT; i++) NodeTable.Add(new NodeX(i, SIZE), new List<NodeX>()); 
                foreach (var i in NodeTable)
                {
                    foreach (var j in NodeTable)
                    {
                        if (i.Key.Rear == j.Key.Head)
                        {
                            NodeTable[i.Key].Add(j.Key);
                        }
                    }
                }
                var _构造时间 = DateTime.Now - dts; dts = DateTime.Now;
                PaintToFile(NodeTable, MAX_PT, $"./{MAX_PT}_{SIZE}.png"); // 画图
                var _画图时间 = DateTime.Now - dts; dts = DateTime.Now;
                var pth = DistPath(GetAllPaths(NodeTable));
                var _寻路时间 = DateTime.Now - dts; dts = DateTime.Now;
                long endMemory = Process.GetCurrentProcess().WorkingSet64;

                Dictionary<int, int> LengthTable = new Dictionary<int, int>();
                foreach (var j in pth)
                {
                    if (LengthTable.ContainsKey(j.Length))
                    {
                        LengthTable[j.Length]++;
                    }
                    else
                    {
                        LengthTable.Add(j.Length, 1);
                    }
                }
                Console.WriteLine("-----------------------");
                Console.WriteLine($"构造时间:{_构造时间}\n画图时间:{_画图时间}\n寻路时间:{_寻路时间}\n");
                Console.WriteLine("Memory used: " + ((endMemory - startMemory) / 1024.0 / 1024.0) + " M Bytes");
                Console.WriteLine("-----------------------");
                Console.WriteLine($"TOTAL: {pth.Count}");
                foreach (var i in LengthTable.OrderBy(k => k.Key))
                {
                    Console.WriteLine($"LENGTH: {i.Key} COUNT: {i.Value}");
                }
            });
        }
        static void PaintToFile(IDictionary<NodeX, List<NodeX>> NodeTable, int maxpt, string name = "./output.jpg")
        {
            int width = 1000;
            int _colcount = 10;
            SKImageInfo info = new SKImageInfo(width, width);
            using SKBitmap bitmap = new SKBitmap(info);
            using SKCanvas canvas = new SKCanvas(bitmap);
            SKPaint paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.White,
                StrokeWidth = 2,
                TextSize = 20
            };
            SKPaint paintr = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Red,
                StrokeWidth = 1
            };
            Dictionary<NodeX, KeyValuePair<int, int>> pointpos = new();
            int x = width / 10;
            int y = width / 10;
            int count = 0;
            int stepx = (width - 2 * (x)) / _colcount;
            int stepy = (width - 2 * (x)) / (maxpt / _colcount);
            foreach (var i in NodeTable)
            {
                pointpos.Add(i.Key, new KeyValuePair<int, int>(x, y));
                canvas.DrawPoint(x, y, paint);
                canvas.DrawText($"{i.Key.Data}", x, y, paint);
                x += stepx;
                count++;
                if (count % _colcount == 0)
                {
                    x = width / 10;
                    y += stepy;
                }
            }
            foreach (var i in NodeTable)
            {
                foreach (var j in i.Value)
                {
                    var _i = pointpos[i.Key];
                    var _j = pointpos[j];
                    canvas.DrawLine(_i.Key, _i.Value, _j.Key, _j.Value, paintr);
                }
            }
            using SKImage image = SKImage.FromBitmap(bitmap);
            SKData data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
            using var stream = File.OpenWrite(name);
            data.SaveTo(stream);
            Console.WriteLine("GRAPH OK");
        }
        static Task<List<NodePath<NodeX>>> FindAllPaths(IDictionary<NodeX, List<NodeX>> nodeTable, NodeX startNode, NodeX endNode)
        {
            return Task.Factory.StartNew(() =>
            {
                var paths = new List<NodePath<NodeX>>();
                var stack = new Stack<List<NodeX>>();
                var initialPath = new List<NodeX> { startNode };

                stack.Push(initialPath);

                while (stack.Count > 0)
                {
                    var path = stack.Pop();
                    var lastNode = path.Last();
                    if (lastNode == endNode)
                    {
                        paths.Add(new NodePath<NodeX>(path.ToArray()));
                    }
                    else
                    {
                        foreach (var neighbor in nodeTable[lastNode])
                        {
                            if (!path.Contains(neighbor))
                            {
                                var newPath = new List<NodeX>(path) { neighbor };
                                stack.Push(newPath);
                            }
                        }
                    }
                }

                return paths;
            });
        }
        static List<NodePath<NodeX>> GetAllPaths(IDictionary<NodeX, List<NodeX>> nodeTable)
        {
            List<NodePath<NodeX>> list = new();
            List<Task<List<NodePath<NodeX>>>> ltsk = new();
            foreach (var i in nodeTable.Keys)
            {
                foreach (var j in nodeTable.Keys)
                {
                    var tsk = FindAllPaths(nodeTable, i, j);
                    ltsk.Add(tsk);
                    list.AddRange(tsk.GetAwaiter().GetResult());
                }
            }
            Task.WaitAll(ltsk.ToArray());
            return list;
        }
        static List<NodePath<NodeX>> DistPath(List<NodePath<NodeX>> sequences)
        {
            List<NodePath<NodeX>> result = new List<NodePath<NodeX>>();

            for (int i = 0; i < sequences.Count; i++)
            {
                bool isSubsequence = false;
                for (int j = 0; j < sequences.Count; j++)
                {
                    if (i != j && NodePath<NodeX>.IsSubsequence(sequences[i], sequences[j]))
                    {
                        isSubsequence = true;
                        break;
                    }
                }
                if (!isSubsequence)
                {
                    result.Add(sequences[i]);
                }
            }

            return result;
        }
    }



    class NodeX : IEquatable<NodeX>
    {
        public int Head;
        public int Rear;
        public int Data;
        public int Size;
        public bool Equals(NodeX? other) => other is NodeX s && s.Data == Data;
        public NodeX(int d, int size)
        {
            Head = GetRandomSeed();
            Rear = GetRandomSeed();
            while (Head == Rear) Rear = GetRandomSeed();
            Data = d;
            Size = size;
        }
        int GetRandomSeed()
        {
            return Random.Shared.Next(0, (int)Math.Pow(2, Size));
        }
        public override string ToString()
        {
            return Data.ToString();
        }

    }
    class NodePath<NodeType> : IEnumerable<NodeType> where NodeType : IEquatable<NodeType>
    {
        public List<NodeType> Path { get; } = new();
        public int Length => Path.Count;

        public NodePath(NodeType[] array) => Path = array.ToList();
        public NodeType this[int k] => Path[k];
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var i in Path)
            {
                sb.Append($" [{i}] ");
            }
            return sb.ToString();
        }
        public bool HaveSubSet(NodePath<NodeType> subset) => IsSubsequence(subset, this);
        public static bool IsSubsequence(NodePath<NodeType> subset, NodePath<NodeType> superset)
        {
            int i = 0;
            for (int j = 0; j < superset.Length; j++)
            {
                if (subset[i].Equals(superset[j]))
                {
                    i++;
                    if (i == subset.Length)
                        return true;
                }
            }
            return false;
        }
        public IEnumerator<NodeType> GetEnumerator() => ((IEnumerable<NodeType>)Path).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Path).GetEnumerator();
    }
}
