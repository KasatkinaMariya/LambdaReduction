using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LambdaReduction.Properties;

namespace LambdaReduction
{
    static class CalculationUtils
    {
        public static Dictionary<string, object> BuildParamNameToValueSavingOrder(IList<ParameterExpression> parameters,
                                                                                  IList<object> values)
        {
            var nameToValueDictionary = new Dictionary<string, object>();

            for (int i = 0; i < parameters.Count; i++)
            {
                object curParameterValue;

                try
                {
                    curParameterValue = values[i];
                }
                catch (IndexOutOfRangeException)
                {
                    curParameterValue = null;
                }

                nameToValueDictionary.Add(parameters[i].Name, curParameterValue);
            }

            return nameToValueDictionary;
        }

        public static object DoCalculateLambda(LambdaExpression lambda,
                                               IDictionary<string, object> paramNameToValue)
        {
            IEnumerable<object> requiredParamValues;
            if (lambda.Parameters.Count == paramNameToValue.Count)
                requiredParamValues = paramNameToValue.Values;
            else
                requiredParamValues = from requiredParam in lambda.Parameters
                                      join allParam in paramNameToValue
                                        on requiredParam.Name equals allParam.Key
                                      select allParam.Value;
            
            return lambda.Compile().DynamicInvoke(requiredParamValues.ToArray());
        }

        public static Dictionary<string, object> CalculateFactoredOutCalls(IEnumerable<MethodCallAsParameter> calls,
                                                                           Dictionary<string,object> originalParamNameToValue)
        {
            var callValues = Settings.Default.UseConcurrencyForFCalculation
                             ? calls
                                .AsParallel().AsOrdered()
                                .Select(p => DoCalculateLambda(p.CallPackedToLambda,
                                                               originalParamNameToValue))
                             : calls
                                .Select(p => DoCalculateLambda(p.CallPackedToLambda,
                                                               originalParamNameToValue));

            return BuildParamNameToValueSavingOrder (calls.Select(p=>p.Parameter).ToList(),
                                                     callValues.ToList());
        }
    }
}
