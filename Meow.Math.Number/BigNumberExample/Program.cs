using MathX.Number;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace BigNumberExample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            _ = args;

            // GrandInt 示例
            GrandInt bn = ulong.MaxValue;
            for (int i = 0; i < 2; i++) bn *= bn;
            Console.WriteLine("GrandInt 示例:");
            Console.WriteLine($"{bn}\n{bn.GetHeapAllocatedSize()} Byte on Heap\n");

            // BigFraction 示例
            Console.WriteLine("BigFraction 示例:");
            var a = new BigFraction(new GrandInt(1), new GrandInt(2)); //1/2
            var b = new BigFraction(new GrandInt(1), new GrandInt(3)); //1/3
            var sum = a + b; //1/2 +1/3 =5/6
            Console.WriteLine($"{a} + {b} = {sum}");

            // 从 double 创建并化简
            var f = BigFraction.FromDouble(0.75); // 应为3/4
            Console.WriteLine($"FromDouble(0.75) = {f}");

            // 使用 larger numbers
            var bigNum = new GrandInt(1234567890123456789L);
            var frac = new BigFraction(bigNum, new GrandInt(7));
            Console.WriteLine($"大分数示例: {frac} -> double: {frac.ToDouble()}");

            var pi = new BigFraction(102928, 32763);
            Console.WriteLine($"102928/32763 >> 100 = {pi >> 100}");
        }
    }
}
