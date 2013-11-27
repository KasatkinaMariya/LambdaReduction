using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using LambdaReduction;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class MethodCallsComparerTest
    {
        private readonly IEqualityComparer<MethodCallExpression> _comparer
                                        = new MethodCallOutlineEqualityComparer();

        [TestMethod]
        public void OutlineEqualityBase()
        {
            Func<int> simpleCall = () => 1;
            Func<int> anotherSimpleCall = () => 2;

            var simpleCallExp = Expression.Call(simpleCall.Method, new List<Expression>());
            var anotherSimpleCallExp = Expression.Call(anotherSimpleCall.Method, new List<Expression>());

            // рефлексивность
            Assert.IsTrue(_comparer.Equals(simpleCallExp, simpleCallExp));

            // симметричность
            Assert.AreEqual(_comparer.Equals(simpleCallExp, anotherSimpleCallExp),
                            _comparer.Equals(anotherSimpleCallExp, simpleCallExp));

            // поведение при null
            Assert.IsFalse(_comparer.Equals(simpleCallExp, null));
            Assert.IsFalse(_comparer.Equals(null, simpleCallExp));
            Assert.IsTrue(_comparer.Equals(null, null));
        }

        [TestMethod]
        public void OutlineEqualityWithParams()
        {
            Func<int, int> call1 = (int x) => x + 1;
            Func<int, int> call2 = (int y) => y + 1;
            Func<int, int, int> call3 = (int x, int z) => x + 1;
            Func<int, int> call4 = (int x) => x + 2;
            Func<int, int> call5 = (int x) => x + 1;

            var callExp1 = Expression.Call(call1.Method, new List<Expression>() { Expression.Constant(1)});
            var callExp2 = Expression.Call(call2.Method, new List<Expression>() { Expression.Constant(1) });
            var callExp3 = Expression.Call(call3.Method, new List<Expression>() { Expression.Constant(1), Expression.Constant(2) });
            var callExp4 = Expression.Call(call4.Method, new List<Expression>() { Expression.Constant(1) });
            var callExp5 = Expression.Call(call1.Method, new List<Expression>() { Expression.Constant(1) });

            // отличие в имени параметра дает дает другое выражение
            Assert.IsFalse(_comparer.Equals(callExp1,callExp2));

            // отличие в количестве параметров дает другое выражение
            Assert.IsFalse(_comparer.Equals(callExp1, callExp3));

            // отличие в теле дает другое выражение
            Assert.IsFalse(_comparer.Equals(callExp1, callExp4));

            // при совпадении списка параметров и тела выражения идентичны
            Assert.IsTrue(_comparer.Equals(callExp1, callExp5));
        }

        /// <summary>
        /// 
        /// Вход компаратора:
        /// вызовы методов, у которых отличается только тип входного параметра.
        /// 
        /// Выход компаратора:
        /// false (в общем случае даже "родственные" типы (int и byte, например)
        /// могут привести к вызову разных перегрузок метода). 
        /// 
        /// </summary>
        [TestMethod]
        [Ignore]
        public void OutlineEqualityWithDifferentInputTypes()
        {
            Func<int, int> call1 = (int x) => x + 1;
            Func<byte, int> call2 = (byte x) => x + 1;

            var callExp1 = Expression.Call(call1.Method, new List<Expression>() { Expression.Constant(1) });
            var callExp2 = Expression.Call(call2.Method, new List<Expression>() { Expression.Constant(1, typeof(byte)) });

            Assert.IsFalse(_comparer.Equals(callExp1, callExp2));
        }
    }
}
