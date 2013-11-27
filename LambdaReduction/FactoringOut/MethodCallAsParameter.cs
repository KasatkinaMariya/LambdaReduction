using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using LambdaReduction.Properties;

namespace LambdaReduction
{
    class MethodCallAsParameter
    {
        public ParameterExpression Parameter { get; private set; }
        public LambdaExpression CallPackedToLambda { get; private set; }

        public MethodCallAsParameter(MethodCallExpression call, string parameterName = null)
        {
            Parameter = Expression.Parameter(call.Type, parameterName);
            CallPackedToLambda = Expression.Lambda (call,
                                                    Settings.Default.UseTailOptimization,
                                                    ParametersCollectorDecorator.CollectParameters(call));
        }

        public string FormOutputString()
        {
            var builder = new StringBuilder();

            // "var p1"
            builder.Append("var ")
                   .Append(Parameter.Name);

            // "var p1 = (int x, int y)"
            builder.Append(" = (");
            foreach (var curCallArgument in CallPackedToLambda.Parameters)
                builder.Append(curCallArgument.Type.Name)
                       .Append(" ")
                       .Append(curCallArgument.Name)
                       .Append(", ");
            builder.Remove(builder.Length - 2, 2).Append(")");

            // "var p1 = (int x, int y) => F(x,2*y)"
            builder.Append(" => ")
                   .Append(CallPackedToLambda.Body);

            return builder.ToString();
        }
    }
}