using System;
using MathX.Number;
using Xunit;

namespace BigNumberTest
{
    public class BigFractionTests
    {
        [Fact]
        public void AdditionAndReduction_Works()
        {
            var a = new BigFraction(new GrandInt(1), new GrandInt(2));
            var b = new BigFraction(new GrandInt(1), new GrandInt(3));
            var sum = a + b;
            Assert.Equal(new BigFraction(new GrandInt(5), new GrandInt(6)), sum);
        }

        [Fact]
        public void FromDouble_CreatesExactFraction()
        {
            var f = BigFraction.FromDouble(0.75);
            Assert.Equal(new BigFraction(new GrandInt(3), new GrandInt(4)), f);
        }

        [Fact]
        public void ToDouble_RoundTripsApproximately()
        {
            var f = new BigFraction(new GrandInt(22), new GrandInt(7));
            double d = f.ToDouble();
            double expected = 22.0 / 7.0;
            Assert.Equal(expected, d, 12);
        }

        [Fact]
        public void Zero_Normalized()
        {
            var z = new BigFraction(new GrandInt(0), new GrandInt(123));
            Assert.True(z.IsZero);
            Assert.Equal(new BigFraction(new GrandInt(0), new GrandInt(1)), z);
        }
    }
}
