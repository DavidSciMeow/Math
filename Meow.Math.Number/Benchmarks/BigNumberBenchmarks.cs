using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using MathX.Number;
using System;

[MemoryDiagnoser]
public class GrandIntBench
{
    [Params(8, 32, 128)]
    public int SizeBytes;

    private GrandInt a;
    private GrandInt b;
    private string decimalString;

    [GlobalSetup]
    public void Setup()
    {
        var rnd = new Random(42);
        var magA = new byte[SizeBytes];
        var magB = new byte[SizeBytes];
        rnd.NextBytes(magA);
        rnd.NextBytes(magB);
        // ensure highest byte not zero to avoid accidental small magnitudes
        if (magA[SizeBytes - 1] == 0) magA[SizeBytes - 1] = 1;
        if (magB[SizeBytes - 1] == 0) magB[SizeBytes - 1] = 2;

        a = GrandInt.FromMagnitude(magA, false);
        b = GrandInt.FromMagnitude(magB, false);
        decimalString = a.ToString();
    }

    [Benchmark]
    public GrandInt Add() => a + b;

    [Benchmark]
    public GrandInt Multiply() => a * b;

    [Benchmark]
    public string ToDecimalString() => a.ToString();

    [Benchmark]
    public GrandInt ParseDecimal() => GrandInt.Parse(decimalString);

    [Benchmark]
    public byte[] Serialize() => a.ToSerialized();

    [Benchmark]
    public int HeapAllocated() => a.GetHeapAllocatedSize();
}

public class Program
{
    public static void Main() => BenchmarkRunner.Run<GrandIntBench>();
}
