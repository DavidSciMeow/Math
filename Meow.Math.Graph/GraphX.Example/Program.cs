using GraphX.Core;
using GraphX.Algorithms;

namespace GraphX.Example
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var parser = new Parser.DefaultTextParser<string, long>();
            var lwo = new LongWeightOperator();
            var g = parser.Parse(File.ReadAllText("test.txt"), lwo, out var inferred);


            {
                var stt = DateTime.Now;
                var path = g.Dijkstra("青岛站", "山东大学", lwo, false);
                var ett = DateTime.Now;
                Console.WriteLine(ett - stt);
                Console.WriteLine(path);
            }


            {
                var stt = DateTime.Now;
                var (dpath,dpk) = g.BellmanFord("青岛站", lwo);
                var ett = DateTime.Now;
                Console.WriteLine(ett - stt);
                Console.WriteLine(dpath["山东大学"]);
                Console.WriteLine("---dpk-shortestlink---");
                foreach (var i in dpk)
                {
                    Console.WriteLine(i);
                }
            }



        }
    }
}
