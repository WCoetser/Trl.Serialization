using System;
using System.Collections.Generic;
using System.Text;

namespace Trl.Serialization.SampleApp.ExpresionTree
{
    public class Div : BinaryOperator, IExpression
    {
        public Div(double lhs, double rhs) : base(lhs, rhs)
        {
        }

        public Div(double lhs, IExpression rhs) : base(lhs, rhs)
        {
        }

        public Div(IExpression lhs, double rhs) : base(lhs, rhs)
        {
        }

        public Div(IExpression lhs, IExpression rhs) : base(lhs, rhs)
        {
        }

        public double Calculate()
        {
            double lhs = exprLhs != null ? exprLhs.Calculate() : numLhs;
            double rhs = exprRhs != null ? exprRhs.Calculate() : numRhs;
            return lhs / rhs;
        }
    }
}
