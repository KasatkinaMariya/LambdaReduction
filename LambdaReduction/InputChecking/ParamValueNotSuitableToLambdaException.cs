using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LambdaReduction
{
    public class ParamValueNotSuitableToLambdaException : Exception
    {
        public string ProblemParamName { get; private set; }
        public IEnumerable<ParameterExpression> DesiredParams { get; private set; }
        public IEnumerable<object> ActualValues { get; private set; }

        public ParamValueNotSuitableToLambdaException(string problemParamName,
                                                      IEnumerable<ParameterExpression> desiredParams,
                                                      IEnumerable<object> actualValues)
            : this (problemParamName, desiredParams, actualValues, null)
        {
        }

        public ParamValueNotSuitableToLambdaException(string problemParamName,
                                                      IEnumerable<ParameterExpression> desiredParams,
                                                      IEnumerable<object> actualValues,
                                                      string message)
            : base (message)
        {
            ProblemParamName = problemParamName;
            DesiredParams = desiredParams;
            ActualValues = actualValues;
        }
    }
}
