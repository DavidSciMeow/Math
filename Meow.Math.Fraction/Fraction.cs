using System;
using System.Data.SqlTypes;
using System.Numerics;

namespace Meow.Util.Math
{
    /// <summary>
    /// Fraction struct
    /// </summary>
    public readonly struct Fraction : IEquatable<Fraction>, IComparable<Fraction>
    {
        private readonly BigInteger num;
        private readonly BigInteger den;

        /// <summary>
        /// is Fraction Init with a simple form (Default No)
        /// </summary>
        public static bool FractionCreateWithSimp { get; set; } = false;
        public readonly static BigInteger Perc = BigInteger.Pow(new BigInteger(10000000000000000000), 1000); //perc

        public Fraction(double numerator, double denominator)
        {
            if (denominator == 0) throw new ArgumentException("Denominator cannot be zero.", nameof(denominator));
            double t = FractionCreateWithSimp ? GCD(numerator, denominator) : 1;
            num = (BigInteger)(numerator / t);
            den = (BigInteger)(denominator / t);
        }
        public Fraction(long numerator, long denominator)
        {
            if (denominator == 0) throw new ArgumentException("Denominator cannot be zero.", nameof(denominator));
            double t = FractionCreateWithSimp ? GCD(numerator, denominator) : 1;
            num = (BigInteger)(numerator / t);
            den = (BigInteger)(denominator / t);
        }
        public Fraction(BigInteger numerator, BigInteger denominator)
        {
            if (denominator == 0) throw new ArgumentException("Denominator cannot be zero.", nameof(denominator));
            BigInteger t = FractionCreateWithSimp ? BigInteger.GreatestCommonDivisor(numerator, denominator) : 1;
            num = numerator / t;
            den = denominator / t;
        }

        public Fraction(long numerator, long denominator, double gcd)
        {
            num = (BigInteger)(numerator / gcd);
            den = (BigInteger)(denominator / gcd);
        }
        public Fraction(double numerator, double denominator, double gcd)
        {
            num = (BigInteger)(numerator / gcd);
            den = (BigInteger)(denominator / gcd);
        }
        public Fraction(BigInteger numerator, BigInteger denominator, BigInteger gcd)
        {
            num = numerator / gcd;
            den = denominator / gcd;
        }

        /// <summary>
        /// convert double number into Fraction of Most Simplify form
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static Fraction DoSimpDouble(double d)
        {
            double denominator = 1;
            for (int i = 0; i < 128; i++)
            {
                if (d != (int)d)
                {
                    denominator *= 10;
                    d *= 10;
                }
            }
            double t = GCD(denominator, d);
            return new Fraction(d, denominator, t);
        }
        public static Fraction DoSimpFraction(Fraction d) => new(d.num, d.den, BigInteger.GreatestCommonDivisor(d.num, d.den));
        public static Fraction DoSimpInt(int d) => new(d, 1, 1);
        public static Fraction DoSimpLong(long d) => new(d, 1, 1);


        public static (long, Fraction)GetMixedFraction(Fraction b)
        {
            long dev = (long)BigInteger.DivRem(b.num, b.den, out var rem);
            double devr = (double)rem / (double)b.den;
            return (dev, devr);
        }
        public static (BigInteger, double) GetMixedFractionDoubleForm(Fraction b) => (BigInteger.DivRem(b.num, b.den, out var rem), (double)rem / (double)b.den);
        public static (BigInteger, BigInteger) GetMixedFractionAllpercForm(Fraction b) => (BigInteger.DivRem(b.num, b.den, out BigInteger rem), BigInteger.Divide(rem * Perc, b.den));
        public static double GetDoubleForm(Fraction b)
        {
            long dev = (long)BigInteger.DivRem(b.num, b.den, out var rem);
            double devr = (double)rem / (double)b.den;
            return dev + devr;
        }

        
        public static double GCD(double a, double b)
        {
            if (a % b == 0) return b;
            return GCD(b, a % b);
        }
        public static decimal GCD(decimal a, decimal b)
        {
            if (a % b == 0) return b;
            return GCD(b, a % b);
        }
        public static double GCD(long a, long b) => (long)BigInteger.GreatestCommonDivisor(a, b);

        public static implicit operator Fraction(double d) => DoSimpDouble(d);
        public static implicit operator Fraction(float d) => DoSimpDouble(d);
        public static implicit operator Fraction(long d) => DoSimpLong(d);
        public static implicit operator Fraction(int b) => DoSimpInt(b);

        public static explicit operator double(Fraction b) => GetDoubleForm(b);
        public static explicit operator (BigInteger intg, double remdg)(Fraction b) => GetMixedFractionDoubleForm(b);
        public static explicit operator (BigInteger intg, BigInteger remdg)(Fraction b) => GetMixedFractionAllpercForm(b);
        public static explicit operator (long intg, Fraction remdg)(Fraction b) => GetMixedFraction(b);

        public static Fraction operator +(Fraction a) => a;
        public static Fraction operator -(Fraction a) => new(-a.num, a.den);
        public static Fraction operator +(Fraction a, Fraction b) => new(a.num * b.den + b.num * a.den, a.den * b.den);
        public static Fraction operator -(Fraction a, Fraction b) => a + (-b);
        public static Fraction operator *(Fraction a, Fraction b) => new(a.num * b.num, a.den * b.den);
        public static Fraction operator /(Fraction a, Fraction b) => (b.num == 0) ? throw new DivideByZeroException() : new Fraction(a.num * b.den, a.den * b.num);
        public static Fraction operator !(Fraction a) => DoSimpFraction(a);
        public static (BigInteger intg, BigInteger remdg) operator ~(Fraction a) => GetMixedFractionAllpercForm(a);

        public static bool operator ==(Fraction left, Fraction right) => left.Equals(right);
        public static bool operator !=(Fraction left, Fraction right) => !(left == right);
        public override string ToString()
        {
            if(den == num)
            {
                return "1";
            }
            else if(den == 0)
            {
                return "/0 type, Fraction err";
            }
            else if(den == 1)
            {
                return $"{num}";
            }
            else if(num == 0)
            {
                return "0";
            }
            else
            {
                return $"{num}/{den}";
            }
        }
        public bool Equals(Fraction other) => num == other.num && den == other.den;
        public override bool Equals(object? obj) => obj is Fraction f && Equals(f);
        public override int GetHashCode() => num.GetHashCode() ^ den.GetHashCode();
        public int CompareTo(Fraction obj) => (double)(this - obj) == 0 ? 0 : (double)(this - obj) < 0 ? -1 : 1;
    }

    class FractionFormatter : IFormatProvider, ICustomFormatter
    {
        public string Format(string? format, object? arg, IFormatProvider? formatProvider)
        {
            if(arg is Fraction a)
            {
                return format switch
                {
                    "Mixed" => $"{((BigInteger, double))a}",
                    _ => $"{a}",
                };
            }
            else
            {
                throw new ArgumentException(null, nameof(arg));
            }
        }

        public object? GetFormat(Type? formatType) => (formatType == typeof(ICustomFormatter)) ? this : null;
    }
}