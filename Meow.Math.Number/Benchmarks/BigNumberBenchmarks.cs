using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using MathX.Number;
using System;
using System.IO;

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

public class FractionBench
{
    private BigFraction bf;
    private double d;

    [GlobalSetup]
    public void Setup()
    {
        bf = new BigFraction(new GrandInt(52163), new GrandInt(16604));
        d = 52163.0 / 16604.0;
    }

    [Benchmark]
    public string FractionToString() => bf.ToString();

    [Benchmark]
    public double FractionToDouble() => bf.ToDouble();

    [Benchmark]
    public string DoubleToString() => d.ToString();

    [Benchmark]
    public double DoubleValue() => d;
}

public class Program
{
    public static void Main()
    {
        // Ensure a consistent artifacts folder at repository root: create _benchmark in current project directory
        var projectRoot = Directory.GetCurrentDirectory();
        var artifacts = Path.Combine(projectRoot, "_benchmark");
        Directory.CreateDirectory(artifacts);

        // Create a config that writes artifacts into the desired folder
        var config = ManualConfig.Create(DefaultConfig.Instance).WithArtifactsPath(artifacts);

        BenchmarkRunner.Run(new Type[] { typeof(GrandIntBench), typeof(FractionBench) }, config);
    }
}
