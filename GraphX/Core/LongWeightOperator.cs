using System;

namespace GraphX.Core
{
    /// <summary>
    /// long 类型权重操作器：提供加法、比较与常量（Zero/One/Infinity）等语义，适用于容量或权重运算。<br/>Long weight operator providing arithmetic and comparison semantics for long capacities/weights.
    /// </summary>
    public sealed class LongWeightOperator : IWeightOperator<long>
    {
        /// <inheritdoc/>
        public static readonly LongWeightOperator Instance = new();
        /// <inheritdoc/>
        public long Zero => 0L;
        /// <inheritdoc/>
        public long One => 1L;
        /// <inheritdoc/>
        public long Infinity => long.MaxValue / 4;
        /// <inheritdoc/>
        public long Add(long a, long b)
        {
            if (a >= Infinity || b >= Infinity) return Infinity;
            try
            {
                checked
                {
                    var r = a + b;
                    // 仅在超过 Infinity 时钳制 / only clamp on positive overflow beyond Infinity
                    return r > Infinity ? Infinity : r;
                }
            }
            catch (OverflowException)
            {
                return Infinity;
            }
        }

        /// <inheritdoc/>
        public int Compare(long a, long b) => a.CompareTo(b);
    }

    public sealed class DoubleWeightOperator : IWeightOperator<double>
    {
        /// <inheritdoc/>
        public static readonly DoubleWeightOperator Instance = new();
        /// <inheritdoc/>
        public double Zero => 0.0;
        /// <inheritdoc/>
        public double One => 1.0;
        /// <inheritdoc/>
        public double Infinity => double.PositiveInfinity;
        /// <inheritdoc/>
        public double Add(double a, double b)
        {
            if (a >= Infinity || b >= Infinity) return Infinity;
            try
            {
                checked
                {
                    var r = a + b;
                    // 仅在超过 Infinity 时钳制 / only clamp on positive overflow beyond Infinity
                    return r > Infinity ? Infinity : r;
                }
            }
            catch (OverflowException)
            {
                return Infinity;
            }
        }
        /// <inheritdoc/>
        public int Compare(double a, double b) => a.CompareTo(b);
    }
 }
