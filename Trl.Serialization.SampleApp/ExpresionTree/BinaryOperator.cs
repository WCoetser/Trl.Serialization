namespace Trl.Serialization.SampleApp.ExpresionTree
{
    public abstract class BinaryOperator
    {
        protected double numLhs;
        protected double numRhs;
        protected IExpression exprLhs;
        protected IExpression exprRhs;

        public BinaryOperator(double lhs, double rhs) { numLhs = lhs; numRhs = rhs;  }
        public BinaryOperator(double lhs, IExpression rhs) { numLhs = lhs; exprRhs = rhs; }
        public BinaryOperator(IExpression lhs, double rhs) { exprLhs = lhs; numRhs = rhs; }
        public BinaryOperator(IExpression lhs, IExpression rhs) { exprLhs = lhs; exprRhs = rhs; }
    }
}
