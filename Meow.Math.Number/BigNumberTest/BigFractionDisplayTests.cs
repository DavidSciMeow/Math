using MathX.Number;
using Xunit;

namespace BigNumberTest
{
    public class BigFractionDisplayTests
    {
        [Fact]
        public void ShiftRightDecimal_20Digits()
        {
            var a = new BigFraction(new GrandInt(52163), new GrandInt(16604));
            var (intPart, dec) = a >> 20;
            Assert.Equal("3", intPart.ToString());
            Assert.Equal("14159238737653577451", dec);
        }

        [Fact]
        public void ShiftRightDecimal_50Digits()
        {
            var a = new BigFraction(new GrandInt(52163), new GrandInt(16604));
            var (intPart, dec) = a >> 50;
            Assert.Equal("3", intPart.ToString());
            Assert.Equal("14159238737653577451216574319441098530474584437484", dec);
        }

        [Fact]
        public void TildeOperator_ReturnsIntegerAndRemainder()
        {
            var a = new BigFraction(new GrandInt(52163), new GrandInt(16604));
            var (intPart, rem) = ~a;
            Assert.Equal("3", intPart.ToString());
            Assert.Equal("2351 / 16604", rem);
        }

        [Fact]
        public void ExclamationOperator_ReturnsFractionString()
        {
            var a = new BigFraction(new GrandInt(52163), new GrandInt(16604));
            var s = !a;
            Assert.Equal("52163 / 16604", s);
        }
    }
}
