using Meow.Math.Graph.Struct;
using Meow.Math.Graph.Util;
using System.Buffers;
using System.Runtime.InteropServices;

//DateTime start = DateTime.Now;
//double usedMemory1 = Process.GetCurrentProcess().WorkingSet64 / 1024.0 / 1024.0;
//{
string s = "" +
        "n1-n6:3\n" +
        "n6-n5:6\n" +
        "n5-n4:11\n" +
        "n6-n7:4\n" +
        "n5-n7:12\n" +
        "n2-n1:6\n" +
        "n1-n8:7\n" +
        "n2-n8:2\n" +
        "n3-n8:4\n" +
        "n4-n8:7\n" +
        "n7-n8:1\n" +
        "";

Graph<string> g = Util.ReadMap(s.Split("\n"));
var i = g.MST_Prim("n1");

Console.WriteLine(Util.PrintTree(i.Root,i.Table));
//}
//double usedMemory2 = Process.GetCurrentProcess().WorkingSet64 / 1024.0 / 1024.0;
//DateTime end = DateTime.Now;
//Console.WriteLine("----------");
//Console.WriteLine($"{(end - start).TotalMilliseconds} ms / {usedMemory2 - usedMemory1} MB");
