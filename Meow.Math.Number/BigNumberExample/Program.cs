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
            GrandInt bn = ulong.MaxValue;
            for (int i = 0; i < 2; i++) bn *= bn;
            Console.WriteLine($"{bn}\n{bn.GetHeapAllocatedSize()} Byte on Heap");
        }
    }
}
