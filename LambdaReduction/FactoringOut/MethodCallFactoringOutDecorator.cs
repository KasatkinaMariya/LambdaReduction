using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using LambdaReduction.Properties;

namespace LambdaReduction
{
    class MethodCallFactoringOutDecorator
    {
        public LambdaExpression NewLambda { get; private set; }
        public IEnumerable<MethodCallAsParameter> FactoredOutCalls { get; private set; }
        public IEnumerable<ParameterExpression> NotFactoredOutParams { get; private set; }

        public MethodCallFactoringOutDecorator(LambdaExpression lambdaToExplore,
                                               Delegate methodToFactorOut,
                                               IEqualityComparer<MethodCallExpression> callsComparer = null)
        {
            var factoringOutVisitor = new MethodCallFactoringOutModifier(methodToFactorOut, callsComparer);
            var lambdaWithNewBody = factoringOutVisitor.Visit(lambdaToExplore) as LambdaExpression;
            
            FactoredOutCalls = factoringOutVisitor.FactoredOutCalls;
            NotFactoredOutParams = factoringOutVisitor.NotFactoredOutParams;

            CheckUniquenessOfParamNames(lambdaToExplore.Parameters,
                                        FactoredOutCalls.Select(f => f.Parameter));
            
            var allRequiredParams = NotFactoredOutParams
                                    .Union(FactoredOutCalls
                                    .Select(p => p.Parameter));
            NewLambda = Expression.Lambda(lambdaWithNewBody.Body,
                                          Settings.Default.UseTailOptimization,
                                          allRequiredParams);
        }

        private void CheckUniquenessOfParamNames(IEnumerable<ParameterExpression> originalParams,
                                                 IEnumerable<ParameterExpression> newFactoredParams)
        {
            var paramNamesIntersection = newFactoredParams.Select(factored => factored.Name)
                                         .Intersect(originalParams.Select(original => original.Name));
                                                     
            if (paramNamesIntersection.Any())
            {
                var message = string.Format("Original param's names intersect with new factored out: '{0}'."
                                            + " Choose value for 'NewParamsNamePattern' in Settings more carefully",
                                            paramNamesIntersection.Aggregate((s1, s2) => s1 + " " + s2));
                throw new ArgumentException(message);
            }
        }

        private class MethodCallFactoringOutModifier : ExpressionVisitor
        {
            private readonly Dictionary<MethodCallExpression, MethodCallAsParameter> _factoredCallToRepresentingParam;
            public IEnumerable<MethodCallAsParameter> FactoredOutCalls
            {
                get
                {
                    return _factoredCallToRepresentingParam
                           .Select(call => call.Value);
                }
            }

            private readonly Dictionary<ParameterExpression, int> _paramToNotFactoredOutOccurences;
            public IEnumerable<ParameterExpression> NotFactoredOutParams
            {
                get
                {
                    return _paramToNotFactoredOutOccurences
                           .Where(p => p.Value > 1)
                           .Select(p => p.Key);
                }
            }

            private readonly Delegate _methodToFactorOut;
            private static int _st_modifiedCallsNumber;

            public MethodCallFactoringOutModifier (Delegate methodToFactorOut,
                                                   IEqualityComparer<MethodCallExpression> callsComparer = null)
            {
                _methodToFactorOut = methodToFactorOut;
                _factoredCallToRepresentingParam = new Dictionary<MethodCallExpression, MethodCallAsParameter>(callsComparer);
                _paramToNotFactoredOutOccurences = new Dictionary<ParameterExpression, int>();
            }

            protected override Expression VisitMethodCall(MethodCallExpression call)
            {
                if (call.Method == _methodToFactorOut.Method)
                {
                    MethodCallAsParameter callAsParameter;

                    if (!_factoredCallToRepresentingParam.TryGetValue(call, out callAsParameter))
                    {
                        callAsParameter = new MethodCallAsParameter(call, FormParamName());
                        _factoredCallToRepresentingParam.Add(call, callAsParameter);
                    }

                    return callAsParameter.Parameter;
                }

                return base.VisitMethodCall(call);
            }

            protected override Expression VisitParameter(ParameterExpression param)
            {
                int occurencesNumber;
                if (_paramToNotFactoredOutOccurences.TryGetValue(param, out occurencesNumber))
                    _paramToNotFactoredOutOccurences[param]++;
                else
                    _paramToNotFactoredOutOccurences.Add(param,1);

                return base.VisitParameter(param);
            }

            private string FormParamName()
            {
                return string.Format("{0}{1}", Settings.Default.NewParamsNamePattern,
                                               ++_st_modifiedCallsNumber);
            }
        }
    }
}
