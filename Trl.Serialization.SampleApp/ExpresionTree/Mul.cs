using System;
using System.Collections.Generic;
using System.Text;

namespace Trl.Serialization.SampleApp.ExpresionTree
{
    public class Mul : BinaryOperator, IExpression
    {
        public Mul(double lhs, double rhs) : base(lhs, rhs)
        {
        }

        public Mul(double lhs, IExpression rhs) : base(lhs, rhs)
        {
        }

        public Mul(IExpression lhs, double rhs) : base(lhs, rhs)
        {
        }

        public Mul(IExpression lhs, IExpression rhs) : base(lhs, rhs)
        {
        }

        public double Calculate()
        {
            double lhs = exprLhs != null ? exprLhs.Calculate() : numLhs;
            double rhs = exprRhs != null ? exprRhs.Calculate() : numRhs;
            return lhs * rhs;
        }
    }
}
