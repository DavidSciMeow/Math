using System;
using System.Numerics;

namespace MathX.Number
{
    /// <summary>
    /// 有理数（分数）实现，使用 `GrandInt`作为分子与分母。<br/>
    /// Represents a rational number as a fraction (numerator/denominator) backed by `GrandInt`.<br/>
    /// </summary>
    public readonly struct BigFraction : IEquatable<BigFraction>, IComparable<BigFraction>, IComparable, IConvertible, IFormattable
    {
        /// <summary>
        /// 分子。<br/>
        /// Numerator.<br/>
        /// </summary>
        public GrandInt Numerator { get; }
        /// <summary>
        /// 分母（始终为正）。<br/>
        /// Denominator (always positive).<br/>
        /// </summary>
        public GrandInt Denominator { get; }

        /// <summary>
        /// 构造一个分数（会进行约分并保证分母为正）。<br/>
        /// Construct a fraction (will be reduced and denominator normalized to positive).<br/>
        /// </summary>
        /// <param name="num">分子 / numerator</param>
        /// <param name="den">分母 / denominator (must be non-zero)</param>
        public BigFraction(GrandInt num, GrandInt den)
        {
            if (den.IsZero()) throw new DivideByZeroException("Denominator cannot be zero.");

            // Convert to BigInteger for GCD and normalization
            var biNum = ToBigInteger(num);
            var biDen = ToBigInteger(den);

            // normalize sign so denominator positive
            if (biDen.Sign < 0)
            {
                biNum = BigInteger.Negate(biNum);
                biDen = BigInteger.Negate(biDen);
            }

            if (biNum.IsZero)
            {
                // zero normalized to0/1
                Numerator = new GrandInt(0);
                Denominator = new GrandInt(1);
                return;
            }

            // compute GCD
            BigInteger a = BigInteger.Abs(biNum);
            BigInteger b = BigInteger.Abs(biDen);
            while (b != BigInteger.Zero)
            {
                BigInteger r = a % b;
                a = b;
                b = r;
            }
            BigInteger gcd = a;
            biNum /= gcd;
            biDen /= gcd;

            Numerator = FromBigInteger(biNum);
            Denominator = FromBigInteger(biDen);
        }

        /// <summary>
        /// 从整数创建分数（分母 =1）。<br/>
        /// Create from integer value (denominator =1).<br/>
        /// </summary>
        public BigFraction(GrandInt integer) : this(integer, new GrandInt(1)) { }

        /// <summary>
        /// 是否为零。<br/>
        /// Whether the fraction equals zero.<br/>
        /// </summary>
        public bool IsZero => Numerator.IsZero();

        /// <summary>
        /// 是否为整数（分母为1）。<br/>
        /// Whether the fraction is an integer (denominator ==1).<br/>
        /// </summary>
        public bool IsInteger => Denominator.Length == 1 && Denominator.GetMagnitudeBytes()[0] == 1;

        /// <summary>
        /// 映射为 double（可能丢失精度）。<br/>
        /// Map to double (may lose precision).<br/>
        /// </summary>
        public double ToDouble()
        {
            // Use GrandInt -> double then divide
            double n = Numerator.ToDouble(null);
            double d = Denominator.ToDouble(null);
            return n / d;
        }

        /// <summary>
        /// 映射为 float（可能丢失精度）。<br/>
        /// Map to float (may lose precision).<br/>
        /// </summary>
        public float ToSingle()
        {
            return (float)ToDouble();
        }

        /// <summary>
        /// 转为字符串表示。若为整数则只输出整数，否则输出 `numerator/denominator`。<br/>
        /// Convert to string. If integer, output integer only; otherwise output `numerator/denominator`.<br/>
        /// </summary>
        public override string ToString()
        {
            if (Denominator.Length == 1 && Denominator.GetMagnitudeBytes().Length == 1 && Denominator.GetMagnitudeBytes()[0] == 1)
                return Numerator.ToString();
            return Numerator.ToString() + "/" + Denominator.ToString();
        }

        /// <summary>
        /// 从 double 创建精确分数（基于 IEEE754 表示）。<br/>
        /// Create an exact fraction from a double (based on IEEE754 representation).<br/>
        /// </summary>
        public static BigFraction FromDouble(double d)
        {
            if (double.IsNaN(d) || double.IsInfinity(d)) throw new ArgumentException("Cannot convert NaN or Infinity to fraction.");
            if (d == 0.0) return new BigFraction(new GrandInt(0));

            long bits = BitConverter.DoubleToInt64Bits(d);
            bool neg = (bits >> 63) != 0;
            int exp = (int)((bits >> 52) & 0x7FFL);
            long frac = bits & 0xFFFFFFFFFFFFFL; //52 bits

            // For normalized and subnormal, compute significand and exponent correctly using BigInteger
            int unbiasedExp;
            BigInteger significand;
            if (exp == 0)
            {
                // subnormal
                unbiasedExp = 1 - 1023 - 52; // exponent for significand when frac has no implicit1
                significand = new BigInteger(frac);
            }
            else
            {
                unbiasedExp = exp - 1023 - 52; // exponent to apply after considering52 fraction bits
                significand = (BigInteger.One << 52) | new BigInteger(frac);
            }

            BigInteger numeratorBi = significand;
            BigInteger denominatorBi = BigInteger.One;
            if (unbiasedExp >= 0)
            {
                numeratorBi <<= unbiasedExp;
            }
            else
            {
                denominatorBi <<= -unbiasedExp;
            }

            if (neg) numeratorBi = BigInteger.Negate(numeratorBi);

            var num = FromBigInteger(numeratorBi);
            var den = FromBigInteger(denominatorBi);
            return new BigFraction(num, den);
        }

        // -------------------------
        // Arithmetic operators implemented via BigInteger and converted back to BigFraction
        // -------------------------

        /// <summary>
        ///计算两个 BigInteger 的最大公约数（非负）。<br/>
        /// Compute the greatest common divisor (GCD) of two BigInteger values (non-negative).<br/>
        /// </summary>
        private static BigInteger Gcd(BigInteger x, BigInteger y)
        {
            x = BigInteger.Abs(x);
            y = BigInteger.Abs(y);
            while (y != BigInteger.Zero)
            {
                BigInteger r = x % y;
                x = y;
                y = r;
            }
            return x;
        }

        /// <summary>
        /// 两个分数相加并返回新分数。<br/>
        /// Adds two BigFraction values and returns their sum as a new BigFraction.<br/>
        /// </summary>
        public static BigFraction operator +(BigFraction a, BigFraction b)
        {
            var aNum = ToBigInteger(a.Numerator);
            var aDen = ToBigInteger(a.Denominator);
            var bNum = ToBigInteger(b.Numerator);
            var bDen = ToBigInteger(b.Denominator);
            // compute gcd of denominators to reduce intermediate size
            var g = Gcd(aDen, bDen);
            var aDenDivG = aDen / g;
            var bDenDivG = bDen / g;
            // numerator = aNum*(bDen/g) + bNum*(aDen/g)
            var num = aNum * bDenDivG + bNum * aDenDivG;
            // denominator = lcm = (aDen/g) * bDen
            var den = aDenDivG * bDen;
            return new BigFraction(FromBigInteger(num), FromBigInteger(den));
        }

        /// <summary>
        /// 两个分数相减并返回新分数。<br/>
        /// Subtracts one BigFraction from another and returns the result as a new BigFraction.<br/>
        /// </summary>
        public static BigFraction operator -(BigFraction a, BigFraction b)
        {
            var aNum = ToBigInteger(a.Numerator);
            var aDen = ToBigInteger(a.Denominator);
            var bNum = ToBigInteger(b.Numerator);
            var bDen = ToBigInteger(b.Denominator);
            var g = Gcd(aDen, bDen);
            var aDenDivG = aDen / g;
            var bDenDivG = bDen / g;
            var num = aNum * bDenDivG - bNum * aDenDivG;
            var den = aDenDivG * bDen;
            return new BigFraction(FromBigInteger(num), FromBigInteger(den));
        }

        /// <summary>
        /// 两个分数相乘并返回新分数。<br/>
        /// Multiplies two BigFraction values and returns the result as a new BigFraction.<br/>
        /// </summary>
        public static BigFraction operator *(BigFraction a, BigFraction b)
        {
            var aNum = ToBigInteger(a.Numerator);
            var aDen = ToBigInteger(a.Denominator);
            var bNum = ToBigInteger(b.Numerator);
            var bDen = ToBigInteger(b.Denominator);
            // cross-cancellation to reduce intermediate sizes
            var g1 = Gcd(aNum, bDen);
            if (g1 > BigInteger.One)
            {
                aNum /= g1;
                bDen /= g1;
            }
            var g2 = Gcd(bNum, aDen);
            if (g2 > BigInteger.One)
            {
                bNum /= g2;
                aDen /= g2;
            }
            var num = aNum * bNum;
            var den = aDen * bDen;
            return new BigFraction(FromBigInteger(num), FromBigInteger(den));
        }

        /// <summary>
        /// 两个分数相除并返回新分数。<br/>
        /// Divides one BigFraction value by another and returns the result as a new BigFraction.<br/>
        /// </summary>
        public static BigFraction operator /(BigFraction a, BigFraction b)
        {
            var aNum = ToBigInteger(a.Numerator);
            var aDen = ToBigInteger(a.Denominator);
            var bNum = ToBigInteger(b.Numerator);
            var bDen = ToBigInteger(b.Denominator);
            if (bNum.IsZero) throw new DivideByZeroException();
            // cross-cancellation before multiplication: cancel gcd(aNum,bNum) and gcd(aDen,bDen)
            var g1 = Gcd(aNum, bNum);
            if (g1 > BigInteger.One)
            {
                aNum /= g1;
                bNum /= g1;
            }
            var g2 = Gcd(aDen, bDen);
            if (g2 > BigInteger.One)
            {
                aDen /= g2;
                bDen /= g2;
            }
            var num = aNum * bDen;
            var den = aDen * bNum;
            if (den.Sign < 0) { den = BigInteger.Negate(den); num = BigInteger.Negate(num); }
            return new BigFraction(FromBigInteger(num), FromBigInteger(den));
        }

        /// <summary>
        /// 判断相等（值相等），适用于 == 运算符。<br/>
        /// Determines whether two BigFraction instances represent the same numeric value.<br/>
        /// </summary>
        public static bool operator ==(BigFraction a, BigFraction b) => a.Equals(b);
        /// <summary>
        /// 判断不等。<br/>
        /// Determines whether two BigFraction instances are not equal.<br/>
        /// </summary>
        public static bool operator !=(BigFraction a, BigFraction b) => !a.Equals(b);

        /// <summary>
        /// 判断与另一个分数是否相等（已规范化后直接比较分子与分母）。<br/>
        /// Determines whether the current BigFraction instance is equal to the specified BigFraction value.<br/>
        /// </summary>
        public bool Equals(BigFraction other)
        {
            // since normalized, compare numerators and denominators directly
            return Numerator.Equals(other.Numerator) && Denominator.Equals(other.Denominator);
        }

        /// <summary>
        /// 覆盖 Object.Equals。<br/>
        /// Overrides Object.Equals.<br/>
        /// </summary>
        public override bool Equals(object obj) => obj is BigFraction bf && Equals(bf);
        /// <summary>
        /// 返回哈希码，基于分子与分母。<br/>
        /// Serves as the default hash function for the object (based on numerator and denominator).<br/>
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                int h = 17;
                h = h * 31 + Numerator.GetHashCode();
                h = h * 31 + Denominator.GetHashCode();
                return h;
            }
        }

        // Conversions helpers between GrandInt and BigInteger
        /// <summary>
        /// 将 GrandInt 转换为 System.Numerics.BigInteger（互操作帮助）。<br/>
        /// Convert a `GrandInt` to `System.Numerics.BigInteger` to assist interop operations.<br/>
        /// </summary>
        private static BigInteger ToBigInteger(GrandInt g)
        {
            // prefer GrandInt.ToBigInteger if available
            return g.ToBigInteger();
        }

        /// <summary>
        /// 从 BigInteger 创建 GrandInt 的工厂方法。<br/>
        /// Create a `GrandInt` from a `BigInteger` value (factory helper).<br/>
        /// </summary>
        private static GrandInt FromBigInteger(BigInteger bi)
        {
            return GrandInt.FromBigInteger(bi);
        }

        // -------------------------
        // IComparable and IFormattable
        // -------------------------
        /// <summary>
        /// 将当前分数与另一个分数比较，返回小于0、等于0或大于0的值。<br/>
        /// Compare this fraction with another; returns less than0,0, or greater than0.
        /// </summary>
        public int CompareTo(BigFraction other)
        {
            // compare Numerator/Denominator with other.Numerator/other.Denominator
            var aNum = ToBigInteger(Numerator);
            var aDen = ToBigInteger(Denominator);
            var bNum = ToBigInteger(other.Numerator);
            var bDen = ToBigInteger(other.Denominator);
            // compare aNum * bDen with bNum * aDen
            var left = aNum * bDen;
            var right = bNum * aDen;
            return left.CompareTo(right);
        }

        int IComparable.CompareTo(object obj)
        {
            if (obj == null) return 1;
            if (!(obj is BigFraction)) throw new ArgumentException("Object must be BigFraction");
            return CompareTo((BigFraction)obj);
        }

        /// <summary>
        /// 使用格式化字符串进行格式化。<br/>
        /// Format using IFormattable contract.<br/>
        /// </summary>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format) || string.Equals(format, "G", StringComparison.OrdinalIgnoreCase) || string.Equals(format, "R", StringComparison.OrdinalIgnoreCase))
            {
                return ToString();
            }
            // If format looks like numeric format, fallback to double formatting
            try
            {
                double d = ToDouble();
                return d.ToString(format, formatProvider);
            }
            catch
            {
                return ToString();
            }
        }

        // -------------------------
        // IConvertible
        // -------------------------
        /// <summary>
        /// 获取 TypeCode。<br/>
        /// Get the TypeCode.
        /// </summary>
        public TypeCode GetTypeCode() => TypeCode.Object;

        /// <inheritdoc/>
        public bool ToBoolean(IFormatProvider provider) => !IsZero;

        /// <inheritdoc/>
        public char ToChar(IFormatProvider provider) => Convert.ToChar(ToInt32(provider));

        /// <inheritdoc/>
        public sbyte ToSByte(IFormatProvider provider) => Convert.ToSByte(ToInt32(provider));

        /// <inheritdoc/>
        public byte ToByte(IFormatProvider provider) => Convert.ToByte(ToDouble(provider));

        /// <inheritdoc/>
        public short ToInt16(IFormatProvider provider) => Convert.ToInt16(ToDouble(provider));

        /// <inheritdoc/>
        public ushort ToUInt16(IFormatProvider provider) => Convert.ToUInt16(ToDouble(provider));

        /// <inheritdoc/>
        public int ToInt32(IFormatProvider provider) => Convert.ToInt32(ToDouble(provider));

        /// <inheritdoc/>
        public uint ToUInt32(IFormatProvider provider) => Convert.ToUInt32(ToDouble(provider));

        /// <inheritdoc/>
        public long ToInt64(IFormatProvider provider) => Convert.ToInt64(ToDouble(provider));

        /// <inheritdoc/>
        public ulong ToUInt64(IFormatProvider provider) => Convert.ToUInt64(ToDouble(provider));

        /// <inheritdoc/>
        public float ToSingle(IFormatProvider provider) => (float)ToDouble(provider);

        /// <inheritdoc/>
        public double ToDouble(IFormatProvider provider) => ToDouble();

        /// <inheritdoc/>
        public decimal ToDecimal(IFormatProvider provider) => Convert.ToDecimal(ToDouble(provider));

        /// <inheritdoc/>
        public DateTime ToDateTime(IFormatProvider provider) => throw new InvalidCastException();

        /// <inheritdoc/>
        public string ToString(IFormatProvider provider) => ToString();

        /// <inheritdoc/>
        public object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == typeof(string)) return ToString(provider);
            if (conversionType == typeof(double)) return ToDouble(provider);
            if (conversionType == typeof(float)) return ToSingle(provider);
            if (conversionType == typeof(decimal)) return ToDecimal(provider);
            // fallback: attempt change via double
            var d = ToDouble(provider);
            return Convert.ChangeType(d, conversionType, provider);
        }

        // -------------------------
        // New display operators: >> (decimal digits), ~ (int part + remainder), ! (fraction string)
        // -------------------------
        /// <summary>
        ///计算并返回整数部分与指定位数的小数（作为字符串，保留前导零）。<br/>
        /// Compute and return integer part and the requested number of decimal digits (string, preserves leading zeros).<br/>
        ///例: (52163/16604) >>20 -> (3, "14159238737653577451")
        /// </summary>
        public static (GrandInt, string) operator >>(BigFraction a, int digits)
        {
            if (digits < 0) throw new ArgumentOutOfRangeException(nameof(digits));
            var num = ToBigInteger(a.Numerator);
            var den = ToBigInteger(a.Denominator);
            if (den.IsZero) throw new DivideByZeroException();

            BigInteger q = BigInteger.DivRem(num, den, out BigInteger rem);
            // bool negative = q.Sign < 0 || (q.IsZero && num.Sign < 0);
            // For decimal expansion, work with absolute remainder
            rem = BigInteger.Abs(rem);
            den = BigInteger.Abs(den);

            var sb = new System.Text.StringBuilder(digits);
            for (int i = 0; i < digits; i++)
            {
                rem *= 10;
                BigInteger digit = BigInteger.DivRem(rem, den, out BigInteger newRem);
                sb.Append((char)('0' + (int)digit));
                rem = newRem;
                if (rem.IsZero)
                {
                    // fill remaining with zeros
                    for (int j = i + 1; j < digits; j++) sb.Append('0');
                    break;
                }
            }

            var intPart = FromBigInteger(q);
            // ensure intPart sign matches negative if q==0
            if (q.IsZero && num.Sign < 0) intPart = FromBigInteger(BigInteger.Zero) ;

            return (intPart, sb.ToString());
        }

        /// <summary>
        /// 返回 (整数部分,余数分数) 的表示，余数为不大于1的分数。<br/>
        /// Return (integer part, remainder fraction) where remainder less than 1.
        /// 例: ~ (52163/16604) -> (3, "2351 /16604")
        /// </summary>
        public static (GrandInt, string) operator ~(BigFraction a)
        {
            var num = ToBigInteger(a.Numerator);
            var den = ToBigInteger(a.Denominator);
            BigInteger q = BigInteger.DivRem(num, den, out BigInteger rem);
            // normalize remainder to positive
            if (rem.Sign < 0) { rem = BigInteger.Negate(rem); }
            var intPart = FromBigInteger(q);
            var remainderNum = FromBigInteger(rem);
            var remainderDen = FromBigInteger(den);
            string remStr = remainderNum.ToString() + " / " + remainderDen.ToString();
            return (intPart, remStr);
        }

        /// <summary>
        /// 返回分式自身的 "numerator / denominator" 字符串表示。<br/>
        /// Return the fraction as a "numerator / denominator" string.
        /// </summary>
        public static string operator !(BigFraction a)
        {
            return a.Numerator.ToString() + " / " + a.Denominator.ToString();
        }
    }
}
