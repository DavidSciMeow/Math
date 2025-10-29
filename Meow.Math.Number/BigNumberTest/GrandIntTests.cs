using System;
using System.Numerics;
using MathX.Number;
using Xunit;

namespace BigNumberTest
{
    public class GrandIntTests
    {
        [Fact]
        public void ConstructAndToString_IntAndLong()
        {
            GrandInt a = 123456789;
            GrandInt b = (long)9876543210;
            Assert.Equal("123456789", a.ToString());
            Assert.Equal("9876543210", b.ToString());
        }

        [Fact]
        public void AddAndMultiply_ProduceCorrectValues()
        {
            var a = new GrandInt(1234567890123456789UL);
            var b = new GrandInt(9876543210UL);
            var sum = a + b;
            var prod = a * b;

            var ai = BigInteger.Parse(a.ToString());
            var bi = BigInteger.Parse(b.ToString());
            Assert.Equal((ai + bi).ToString(), sum.ToString());
            Assert.Equal((ai * bi).ToString(), prod.ToString());
        }

        [Fact]
        public void Parse_TryParse_HexBinaryAndUnderscore()
        {
            Assert.True(GrandInt.TryParse("123_456_789", out var d1));
            Assert.Equal("123456789", d1.ToString());

            Assert.True(GrandInt.TryParse("0xFF_EE_DD", out var h));
            Assert.Equal("0xFFEEDD".Length > 0, true); // sanity
            Assert.Equal("16772829", h.ToString());

            Assert.True(GrandInt.TryParse("0b1010_1111", out var b));
            Assert.Equal("175", b.ToString());
        }

        [Fact]
        public void Serialize_Roundtrip()
        {
            var x = GrandInt.Parse("123456789012345678901234567890");
            var data = x.ToSerialized();
            var y = GrandInt.FromSerialized(data);
            Assert.Equal(x.ToString(), y.ToString());
        }

        [Fact]
        public void ToDouble_SmallValues()
        {
            var x = new GrandInt(42);
            double d = x.ToDouble(null);
            Assert.Equal(42.0, d);
        }
    }
}
