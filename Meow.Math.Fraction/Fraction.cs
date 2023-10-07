
using System.Numerics;

namespace Meow.Math
{
    /// <summary>
    /// 分数结构(实现分数类) <br/>
    /// Math Fraction struct
    /// </summary>
    public readonly struct Fraction : IEquatable<Fraction>
    {
        /// <summary>
        /// 分子<br/>numerator
        /// </summary>
        private readonly BigInteger num;
        /// <summary>
        /// 分母<br/>denominator
        /// </summary>
        private readonly BigInteger den;

        /// <summary>
        /// 初始化一个分数 (使用<b><see langword="大"/></b>整数分子分母)<br/>Init a Struct of Fraction. by Input <b><see langword="Large (Arbitrarily)"/></b> BigInteger 
        /// </summary>
        /// <param name="numerator">分子<br/>numerator</param>
        /// <param name="denominator">分母<br/>denominator</param>
        /// <exception cref="ArgumentException"></exception>
        public Fraction(BigInteger numerator, BigInteger denominator)
        {
            if (denominator == 0) throw new DivideByZeroException();
            BigInteger t = denominator == 1 ? BigInteger.GreatestCommonDivisor(numerator, denominator) : 1;
            num = numerator / t;
            den = denominator / t;
        }
        /// <summary>
        /// 初始化一个分数 (使用<b><see langword="大"/></b>整数分子分母)<br/>Init a Struct of Fraction. by Input <b><see langword="Large (Arbitrarily)"/></b> BigInteger 
        /// </summary>
        /// <param name="numerator">分子<br/>numerator</param>
        /// <param name="denominator">分母<br/>denominator</param>
        /// <param name="Gcd">最大公约数<br/>GreatestCommonDivisor</param>
        /// <exception cref="ArgumentException"></exception>
        public Fraction(BigInteger numerator, BigInteger denominator, BigInteger Gcd)
        {
            if (denominator == 0) throw new DivideByZeroException();
            num = numerator / Gcd;
            den = denominator / Gcd;
        }

        /// <summary>
        /// 化简分数<br/>Simplify fraction
        /// </summary>
        /// <param name="d">分数<br/>fraction</param>
        /// <returns>
        /// 已经<b><see langword="化简"/></b>后的分数<br/>
        /// Returns a <b><see langword="reduced"/></b> fraction.
        /// </returns>
        public static Fraction operator !(Fraction d) => new(d.num, d.den);
        /// <summary>
        /// 尝试将分数精确至 <paramref name="k"/> 个小数位数<br/>Try to make the fraction accurate to <paramref name="k"/> decimal places
        /// </summary>
        /// <param name="f">分数<br/>fraction</param>
        /// <param name="k">要精确的小数位数<br/>The number of decimal places you want to be precise</param>
        /// <returns>
        /// 返回一个 <b><see langword="(整数位, 小数位)"/></b> 表示的元组<br/>
        /// Returns a tuple represented by <b><see langword="(BigInteger, decimal part)"/></b>.
        /// </returns>
        public static (BigInteger intg, BigInteger remdg) operator >>(Fraction f, int k) => (BigInteger.DivRem(f.num, f.den, out var rem), BigInteger.Divide(BigInteger.Multiply(rem, BigInteger.Pow(10, k)), f.den));
        /// <summary>
        /// 尝试将分数变成复分数 <br/> Try turning the fraction into a complex fraction
        /// </summary>
        /// <param name="f">分数<br/>fraction</param>
        /// <returns>
        /// 返回一个 <b><see langword="(整数位, 复分数)"/></b> 表示的元组<br/>
        /// Returns a tuple represented by <b><see langword="(BigInteger part, fraction part)"/></b>.
        /// </returns>
        public static (BigInteger comp, Fraction sofrac) operator ~(Fraction f) => (BigInteger.DivRem(f.num,f.den, out var rem), new Fraction(rem, f.den));

        /// <inheritdoc/>
        public static implicit operator Fraction(long d) => new(d, 1);
        /// <inheritdoc/>
        public static explicit operator double(Fraction d)
        {
            var (intg, remdg) = d >> 20;
            if(intg > (BigInteger)double.MaxValue) throw new OverflowException();
            return (double)intg + (double)remdg / (double)BigInteger.Pow(10, 20);
        }
        /// <inheritdoc/>
        public static implicit operator Fraction(double d)
        {
            var num = (long)(d * System.Math.Pow(10, 16));
            var den = BigInteger.Pow(10, 16);
            return new(num, den);
        }
        /// <inheritdoc/>
        public static Fraction operator +(Fraction a) => a;
        /// <inheritdoc/>
        public static Fraction operator -(Fraction a) => new(-a.num, a.den);
        /// <inheritdoc/>
        public static Fraction operator ++(Fraction a) => a + 1;
        /// <inheritdoc/>
        public static Fraction operator --(Fraction a) => a - 1;
        /// <inheritdoc/>
        public static Fraction operator +(Fraction a, Fraction b) => new(a.num * b.den + b.num * a.den, a.den * b.den);
        /// <inheritdoc/>
        public static Fraction operator -(Fraction a, Fraction b) => a + (-b);
        /// <inheritdoc/>
        public static Fraction operator *(Fraction a, Fraction b) => new(a.num * b.num, a.den * b.den);
        /// <inheritdoc/>
        public static Fraction operator /(Fraction a, Fraction b) => (b.num == 0) ? throw new DivideByZeroException() : new Fraction(a.num * b.den, a.den * b.num, BigInteger.GreatestCommonDivisor(a.num * b.den, a.den * b.num));
        /// <inheritdoc/>
        public bool Equals(Fraction other) => num == other.num && den == other.den;
        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is Fraction f && Equals(f);
        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(num, den);
        /// <inheritdoc/>
        public static bool operator ==(Fraction left, Fraction right) => left.Equals(right);
        /// <inheritdoc/>
        public static bool operator !=(Fraction left, Fraction right) => !(left == right);
        /// <inheritdoc/>
        public override string ToString()
        {
            if (den == num)
            {
                return "1";
            }
            else if (den == 1)
            {
                return $"{num}";
            }
            else if (num == 0)
            {
                return "0";
            }
            else
            {
                return $"{num} / {den}";
            }
        }

    }
}