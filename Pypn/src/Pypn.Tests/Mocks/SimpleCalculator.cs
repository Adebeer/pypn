using System;

namespace Pypn.Tests.Mocks
{
    public class SimpleCalculator
    {
        public decimal Add(decimal a, decimal b)
        {
            return a + b;
        }

        public decimal Multiply(decimal a, decimal b)
        {
            return a * b;
        }

        public decimal Abs(decimal value)
        {
            return Math.Abs(value);
        }
    }
}
