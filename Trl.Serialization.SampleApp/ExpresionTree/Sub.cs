namespace Trl.Serialization.SampleApp.ExpresionTree
{
    public class Sub : BinaryOperator, IExpression
    {
        public Sub(double lhs, double rhs) : base(lhs, rhs)
        {
        }

        public Sub(double lhs, IExpression rhs) : base(lhs, rhs)
        {
        }

        public Sub(IExpression lhs, double rhs) : base(lhs, rhs)
        {
        }

        public Sub(IExpression lhs, IExpression rhs) : base(lhs, rhs)
        {
        }

        public double Calculate()
        {
            double lhs = exprLhs != null ? exprLhs.Calculate() : numLhs;
            double rhs = exprRhs != null ? exprRhs.Calculate() : numRhs;
            return lhs - rhs;
        }
    }
}
