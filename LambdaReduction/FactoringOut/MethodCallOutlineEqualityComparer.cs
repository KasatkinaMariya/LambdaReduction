using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LambdaReduction
{
    public class MethodCallOutlineEqualityComparer : IEqualityComparer<MethodCallExpression>
    {
        public bool Equals(MethodCallExpression x, MethodCallExpression y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;

            if (x.NodeType != y.NodeType || x.Type != y.Type)
                return false;

            bool areNotationsConsilient = x.ToString().Equals(y.ToString());
            return areNotationsConsilient;
        }

        public int GetHashCode(MethodCallExpression call)
        {
            var code = new StringBuilder();

            code.Append(call.Method.Name);
            foreach (var curArg in call.Arguments)
                code.Append(curArg.NodeType);

            return code.ToString().GetHashCode();
        }
    }
}
