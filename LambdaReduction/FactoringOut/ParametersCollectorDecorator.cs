using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace LambdaReduction
{
    public class ParametersCollectorDecorator
    {
        private static readonly ParametersCollectorVisitor _st_parametersVisitor;

        public static IEnumerable<ParameterExpression> CollectParameters(Expression expressionToExplore)
        {
            return _st_parametersVisitor.CollectParameters(expressionToExplore);
        }

        static ParametersCollectorDecorator()
        {
            _st_parametersVisitor = new ParametersCollectorVisitor();
        }

        private class ParametersCollectorVisitor : ExpressionVisitor
        {
            [ThreadStatic]
            private static ISet<ParameterExpression> _paramsCollectedInCurThread;

            public IEnumerable<ParameterExpression> CollectParameters(Expression expressionToExplore)
            {
                _paramsCollectedInCurThread = new HashSet<ParameterExpression>();
                Visit(expressionToExplore);
                return _paramsCollectedInCurThread;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                _paramsCollectedInCurThread.Add(node);
                return base.VisitParameter(node);
            }
        }
    }
}