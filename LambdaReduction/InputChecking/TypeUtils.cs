using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LambdaReduction
{
    static class TypeUtils
    {
        public static void CheckExpressionGenericIsDelegate<T>(Expression<T> lambda)
        {
            bool genericParamIsDelegate = typeof(T).IsSubclassOf(typeof(Delegate));
            if (!genericParamIsDelegate)
            {
                var message = string.Format("Generic type in Expression<T> is non-delegate '{0}'",
                                            typeof(T).Name);
                throw new InvalidOperationException(message);
            }
        }

        public static void CheckParamValuesAreSuitableToLambda(LambdaExpression lambda, params object[] paramValues)
        {
            int actuallySpecifiedParamsCount = Math.Min(lambda.Parameters.Count, paramValues.Count());

            for (int i = 0; i < actuallySpecifiedParamsCount; i++)
            {
                Type curDesiredType = lambda.Parameters[i].Type;
                object curValue = paramValues[i];

                bool curValueIsCompatible = curDesiredType.IsAssignableFrom(curValue.GetType());
                if (!curValueIsCompatible)
                {
                    var message = string.Format("Lambda parameter '{0}' should be compatible with type '{1}',"
                                                + " but actual value '{2}' may not be compatible with it",
                                                lambda.Parameters[i].Name, curDesiredType, curValue);
                    throw new ParamValueNotSuitableToLambdaException
                        (lambda.Parameters[i].Name,lambda.Parameters,paramValues,message);
                }
            }

            for (int i = actuallySpecifiedParamsCount; i < lambda.Parameters.Count; i++)
            {
                bool curLambdaParamMayBeNull = IsNullable(lambda.Parameters[i].Type);
                if (!curLambdaParamMayBeNull)
                {
                    var message = string.Format("Argument's list doesn't contain value for parameter '{0}' of non-nullable type '{1}'",
                                                lambda.Parameters[i].Name, lambda.Parameters[i].Type);
                    throw new ParamValueNotSuitableToLambdaException
                        (lambda.Parameters[i].Name, lambda.Parameters, paramValues, message);
                }
            }
        }

        private static bool IsNullable(Type typeToTest)
        {
            return !typeToTest.IsValueType // ссылочный тип
                   || Nullable.GetUnderlyingType(typeToTest) != null; // или тип значения является nullable
        }
    }
}
