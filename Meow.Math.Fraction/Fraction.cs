using NeinMath;

namespace Meow.Util.Math
{
    /// <summary>
    /// Fraction struct
    /// </summary>
    public readonly struct Fraction : IEquatable<Fraction>, IComparable<Fraction>
    {
        private readonly Integer num;
        private readonly Integer den;

        public Fraction(double numerator, double denominator)
        {
            if (denominator == 0) throw new ArgumentException("Denominator cannot be zero.", nameof(denominator));
            double t = denominator == 1 ? GCD(numerator, denominator) : 1;
            num = (Integer)(numerator / t);
            den = (Integer)(denominator / t);
        }
        public Fraction(long numerator, long denominator)
        {
            if (denominator == 0) throw new ArgumentException("Denominator cannot be zero.", nameof(denominator));
            double t = denominator == 1 ? GCD(numerator, denominator) : 1;
            num = (Integer)(numerator / t);
            den = (Integer)(denominator / t);
        }
        public Fraction(Integer numerator, Integer denominator)
        {
            if (denominator == 0) throw new ArgumentException("Denominator cannot be zero.", nameof(denominator));
            Integer t = denominator == 1 ? numerator.Gcd(denominator) : 1;
            num = numerator / t;
            den = denominator / t;
        }
        public Fraction(long numerator, long denominator, double gcd)
        {
            num = (Integer)(numerator / gcd);
            den = (Integer)(denominator / gcd);
        }
        public Fraction(double numerator, double denominator, double gcd)
        {
            num = (Integer)(numerator / gcd);
            den = (Integer)(denominator / gcd);
        }
        public Fraction(Integer numerator, Integer denominator, Integer gcd)
        {
            num = numerator / gcd;
            den = denominator / gcd;
        }

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
        public static Fraction DoSimpFraction(Fraction d) => new(d.num, d.den, d.num.Gcd(d.den));
        public static Fraction DoSimpInt(int d) => new(d, 1, 1);
        public static Fraction DoSimpLong(long d) => new(d, 1, 1);

        public static (long, Fraction) GetMixedFraction(Fraction b) => ((long)b.num.DivRem(b.den, out var rem), (Fraction)((double)rem / (double)b.den));
        public static (Integer, double) GetMixedFractionDoubleForm(Fraction b) => (b.num.DivRem(b.den, out Integer rem), (double)rem / (double)b.den);
        public static (Integer, Integer) GetMixedFractionForm(Fraction b, int k) => (b.num.DivRem(b.den, out Integer rem), rem.Multiply(Integer.Parse("10").Pow(k)).Divide(b.den));
        public static double GetDoubleForm(Fraction b) => ((long)b.num.DivRem(b.den, out Integer rem)) + ((double)rem / (double)b.den);

        public static double GCD(double a, double b)
        {
            if (a % b == 0) return b;
            return GCD(b, a % b);
        }
        public static double GCD(long a, long b)
        {
            if (a % b == 0) return b;
            return GCD(b, a % b);
        }
        public static decimal GCD(decimal a, decimal b)
        {
            if (a % b == 0) return b;
            return GCD(b, a % b);
        }

        public static Fraction operator !(Fraction a) => DoSimpFraction(a);
        public static (Integer intg, Integer remdg) operator >>(Fraction a, int k) => GetMixedFractionForm(a, k);
        public static (Integer intg, Integer remdg) operator ~(Fraction a) => GetMixedFractionForm(a, 10);

        public static implicit operator Fraction(long d) => DoSimpLong(d);
        public static implicit operator Fraction(int b) => DoSimpInt(b);

        public static explicit operator Fraction(double d) => DoSimpDouble(d);
        public static explicit operator Fraction(float d) => DoSimpDouble(d);
        public static explicit operator double(Fraction b) => GetDoubleForm(b);

        public static Fraction operator +(Fraction a) => a;
        public static Fraction operator ++(Fraction a) => a + 1;
        public static Fraction operator --(Fraction a) => a - 1;
        public static Fraction operator -(Fraction a) => new(-a.num, a.den);
        public static Fraction operator +(Fraction a, Fraction b) => new(a.num * b.den + b.num * a.den, a.den * b.den);
        public static Fraction operator -(Fraction a, Fraction b) => a + (-b);
        public static Fraction operator *(Fraction a, Fraction b) => new(a.num * b.num, a.den * b.den);
        public static Fraction operator /(Fraction a, Fraction b) => (b.num == 0) ? throw new DivideByZeroException() : new Fraction(a.num * b.den, a.den * b.num);
        


        public static bool operator ==(Fraction left, Fraction right) => left.Equals(right);
        public static bool operator !=(Fraction left, Fraction right) => !(left == right);
        public override string ToString()
        {
            if(den == num)
            {
                return "1";
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
        public override int GetHashCode() => HashCode.Combine(num, den);
        public int CompareTo(Fraction obj) => (double)(this - obj) == 0 ? 0 : (double)(this - obj) < 0 ? -1 : 1;

        public static bool operator <(Fraction left, Fraction right) => left.CompareTo(right) < 0;
        public static bool operator <=(Fraction left, Fraction right) => left.CompareTo(right) <= 0;
        public static bool operator >(Fraction left, Fraction right) => left.CompareTo(right) > 0;
        public static bool operator >=(Fraction left, Fraction right) => left.CompareTo(right) >= 0;
    }
}