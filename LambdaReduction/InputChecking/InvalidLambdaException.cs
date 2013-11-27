using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LambdaReduction
{
    class InvalidLambdaException : Exception
    {
        public LambdaExpression ProblemLambda { get; private set; }

        public InvalidLambdaException(LambdaExpression problemLambda)
            : this (problemLambda, null, null)
        {
        }

        public InvalidLambdaException(LambdaExpression problemLambda,
                                        string message)
            : this(problemLambda, message, null)
        {
        }

        public InvalidLambdaException(LambdaExpression problemLambda,
                                        string message,
                                        Exception inner)
            : base (message, inner)
        {
            ProblemLambda = problemLambda;
        }
    }
}
