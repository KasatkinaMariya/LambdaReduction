using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LambdaReduction.Properties;

namespace LambdaReduction
{
    static class FactoringOutAndCalculation
    {
        private static readonly IEqualityComparer<MethodCallExpression> _st_methodCallsComparer = new MethodCallOutlineEqualityComparer();

        public static object OptimizedCalculation(Delegate functionF,
                                                   LambdaExpression lambda,
                                                   params object[] lambdaParams)
        {
            CheckInputCorrectness(lambda,lambdaParams);

            var optimalityInfo = OptimizeLambda(lambda, functionF);
            if (Settings.Default.OutputOptimizingExpressions)
                optimalityInfo.Output();

            var result = CalculateLambda(optimalityInfo, lambdaParams);
            if (Settings.Default.OutputCalculatedResult)
                Console.WriteLine(result);

            return result;
        }

        private static void CheckInputCorrectness(LambdaExpression lambda,
                                                  params object[] lambdaParams)
        {
            try
            {
                lambda.Compile();
            }
            catch (Exception originalException)
            {
                var message = string.Format("Lambda '{0}' is not compilable", lambda);
                throw new InvalidLambdaException(lambda,message,originalException);
            }

            TypeUtils.CheckParamValuesAreSuitableToLambda(lambda, lambdaParams);
        }

        private static OptimizingLambdaInfo OptimizeLambda(LambdaExpression lambda,
                                                           Delegate functionF)
        {
            var factoringOutDecorator = new MethodCallFactoringOutDecorator
                                                (lambda, functionF, _st_methodCallsComparer);

            return new OptimizingLambdaInfo()
            {
                OriginalLambda = lambda,
                OptimizedLambda = factoringOutDecorator.NewLambda,
                FactoredOutCalls = factoringOutDecorator.FactoredOutCalls,
                NotFactoredOutParams = factoringOutDecorator.NotFactoredOutParams
            };
        }

        private static object CalculateLambda(OptimizingLambdaInfo optimalityInfo,
                                              params object[] originalLambdaParams)
        {
            var originalParamNameToValue = CalculationUtils.BuildParamNameToValueSavingOrder
                                             (optimalityInfo.OriginalLambda.Parameters, originalLambdaParams);

            var factoredOutParamNameToValue = CalculationUtils.CalculateFactoredOutCalls
                                                (optimalityInfo.FactoredOutCalls, originalParamNameToValue);
            var notFactoredOutParamNameToValue = optimalityInfo.NotFactoredOutParams
                                                 .Join(originalParamNameToValue,
                                                       notFactored => notFactored.Name,
                                                       original => original.Key,
                                                       (notFactored, original) => new KeyValuePair<string, object>
                                                                                    (original.Key, original.Value));

            var allRequiredParamNameToValue = factoredOutParamNameToValue
                                              .Union(notFactoredOutParamNameToValue)
                                              .ToDictionary(p => p.Key, p => p.Value);

            return CalculationUtils.DoCalculateLambda(optimalityInfo.OptimizedLambda,
                                                      allRequiredParamNameToValue);
        }

        private class OptimizingLambdaInfo
        {
            public LambdaExpression OriginalLambda { get; set; }
            public LambdaExpression OptimizedLambda { get; set; }

            public IEnumerable<MethodCallAsParameter> FactoredOutCalls { get; set; }
            public IEnumerable<ParameterExpression> NotFactoredOutParams { get; set; }

            public void Output()
            {
                foreach (var callAsParam in FactoredOutCalls)
                    Console.WriteLine(callAsParam.FormOutputString());

                Console.WriteLine(OptimizedLambda);
            }
        }
    }
}
