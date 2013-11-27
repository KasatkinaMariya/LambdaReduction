using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using LambdaReduction;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class ParametersCollectorTest
    {
        private readonly ParametersCollectorDecorator _collector
                                        = new ParametersCollectorDecorator();

        [TestMethod]
        public void CollectNoParamsTest()
        {
            Func<int> methodWithoutParams = () => 1;
            var callExpression = Expression.Call(methodWithoutParams.Method,
                                                 new List<Expression>());
            IEnumerable<ParameterExpression> actualCollected
                        = ParametersCollectorDecorator.CollectParameters(callExpression);
            
            Assert.IsNotNull(actualCollected);
            Assert.AreEqual(0,actualCollected.Count());
        }

        [TestMethod]
        public void CollectSeveralParamsTest()
        {
            Func<int, DirectoryInfo, long?, int> methodWithParams = (value, reference, nullable) => 5;
            var callExpression = Expression.Call(methodWithParams.Method,
                                                 new List<Expression>()
                                                 {
                                                    Expression.Parameter(typeof(int),"x"),
                                                    Expression.Parameter(typeof(DirectoryInfo), "dir"),
                                                    Expression.Parameter(typeof(long?),"y"),
                                                 });

            IEnumerable<ParameterExpression> actualCollected
                        = ParametersCollectorDecorator.CollectParameters(callExpression);
            
            Assert.IsNotNull(actualCollected);
            Assert.AreEqual(3, actualCollected.Count());

            Action<int, Type, string> checkOneParam =
                (position, type, name) =>
                    {
                        var param = actualCollected.ElementAt(position);
                        Assert.IsNotNull(param);
                        Assert.AreEqual(type, param.Type);
                        Assert.AreEqual(name, param.Name);
                    };
            checkOneParam(0, typeof (int), "x");
            checkOneParam(1, typeof (DirectoryInfo), "dir");
            checkOneParam(2, typeof (long?), "y");
        }
    }
}
