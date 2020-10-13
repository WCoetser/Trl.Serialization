namespace Trl.Serialization.SampleApp.ExpresionTree
{
    public class Add : BinaryOperator, IExpression
    {
        public Add(double lhs, double rhs) : base(lhs, rhs)
        {
        }

        public Add(double lhs, IExpression rhs) : base(lhs, rhs)
        {
        }

        public Add(IExpression lhs, double rhs) : base(lhs, rhs)
        {
        }

        public Add(IExpression lhs, IExpression rhs) : base(lhs, rhs)
        {
        }

        public double Calculate()
        {
            double lhs = exprLhs != null ? exprLhs.Calculate() : numLhs;
            double rhs = exprRhs != null ? exprRhs.Calculate() : numRhs;
            return lhs + rhs;
        }
    }
}
