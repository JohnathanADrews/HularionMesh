#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace HularionMesh.Query.ExpressionHandler
{
    /// <summary>
    /// Processes expressions, retrieving desired expression information.
    /// </summary>
    public class ExpressionProcessor
    {

        private static  Dictionary<ExpressionOperatorType, ExpressionOperator> operators = new Dictionary<ExpressionOperatorType, ExpressionOperator>();
        private static Dictionary<RetrieveType, ExpressionLocatorPath> paths = new Dictionary<RetrieveType, ExpressionLocatorPath>();
        private static Type MemberExpressionType = typeof(MemberExpression);

        /// <summary>
        /// Constructor.
        /// </summary>
        public ExpressionProcessor()
        {

        }

        static ExpressionProcessor()
        {
            #region Operators
            operators.Add(ExpressionOperatorType.GetBody, new ExpressionOperator() { NodeType = ExpressionType.Lambda, Next = e => ((LambdaExpression)e).Body, OperationType = ExpressionOperator.OperationNodeType.Locator });
            operators.Add(ExpressionOperatorType.GetOperand, new ExpressionOperator() { NodeType = ExpressionType.MemberAccess, Retriever = e => ((MemberExpression)e).Member, OperationType = ExpressionOperator.OperationNodeType.Locator });
            operators.Add(ExpressionOperatorType.GetConvert, new ExpressionOperator() { NodeType = ExpressionType.Convert, Next = e => ((UnaryExpression)e).Operand, OperationType = ExpressionOperator.OperationNodeType.Locator });
            operators.Add(ExpressionOperatorType.GetLambda, new ExpressionOperator() { NodeType = ExpressionType.Lambda, Retriever = e => ((LambdaExpression)e).Body, OperationType = ExpressionOperator.OperationNodeType.Retriever });

            var getMemberOperator = new ExpressionOperator() { NodeType = ExpressionType.Lambda, OperationType = ExpressionOperator.OperationNodeType.Retriever };
            getMemberOperator.Retriever = e =>
            {
                if (!MemberExpressionType.IsAssignableFrom(e.GetType()))
                {
                    throw new ArgumentException(String.Format("The provided expression must return a member. (e.g. x=> x.Key) [Tp5cgYJSL0eUmzKp7oQPxA]"));
                }
                return ((MemberExpression)e).Member;
            };
            operators.Add(ExpressionOperatorType.GetMember, getMemberOperator);

            var getComparison = new ExpressionOperator() { NodeType = ExpressionType.Lambda, OperationType = ExpressionOperator.OperationNodeType.Retriever };
            getComparison.Retriever = e =>
            {
                return e;
                //((MemberExpression)e).Member;
            };
            operators.Add(ExpressionOperatorType.GetComparison, getComparison);

            #endregion

            #region Paths

            paths.Add(RetrieveType.Member, new ExpressionLocatorPath(new ExpressionOperator[]
            {
                operators[ExpressionOperatorType.GetBody],
                operators[ExpressionOperatorType.GetMember]
            }));

            paths.Add(RetrieveType.MemberFromConvert, new ExpressionLocatorPath(new ExpressionOperator[]
            {
                operators[ExpressionOperatorType.GetBody],
                operators[ExpressionOperatorType.GetConvert],
                operators[ExpressionOperatorType.GetMember]
            }));

            paths.Add(RetrieveType.Comparison, new ExpressionLocatorPath(new ExpressionOperator[]
            {
                operators[ExpressionOperatorType.GetLambda]
            }));

            #endregion

        }

        /// <summary>
        /// Gets the member info of the member indicated in the expression.
        /// </summary>
        /// <typeparam name="T">The type from which to get the member.</typeparam>
        /// <param name="expression">The expression containing the member access.</param>
        /// <returns>The member info of the accessed member.</returns>
        public MemberInfo GetMember<T>(Expression<Func<T, object>> expression)
        {
            ExpressionLocatorPath path = null;
            if (expression.Body.NodeType == ExpressionType.MemberAccess) { path = paths[RetrieveType.Member]; }
            if (expression.Body.NodeType == ExpressionType.Convert) { path = paths[RetrieveType.MemberFromConvert]; }
            var value = ProcessExpression(expression, path.Operators);
            return (MemberInfo)value;
        }

        /// <summary>
        /// Gets the member info of the member indicated in the expression.
        /// </summary>
        /// <typeparam name="T">The type from which to get the member.</typeparam>
        /// <typeparam name="U">The type of member being retrieved.</typeparam>
        /// <param name="expression">The expression containing the member access.</param>
        /// <returns>The member info of the accessed member.</returns>
        public MemberInfo GetMember<T, U>(Expression<Func<T, U>> expression)
        {
            ExpressionLocatorPath path = null;
            if (expression.Body.NodeType == ExpressionType.MemberAccess) { path = paths[RetrieveType.Member]; }
            if (expression.Body.NodeType == ExpressionType.Convert) { path = paths[RetrieveType.MemberFromConvert]; }
            var value = ProcessExpression(expression, path.Operators);
            return (MemberInfo)value;
        }

        /// <summary>
        /// Gets the comparison expression from the provided expression.
        /// </summary>
        /// <typeparam name="T">The type of the object containing members to be compared.</typeparam>
        /// <param name="expression">The expression to inspect/</param>
        /// <returns>The expression containing the comparison.</returns>
        public Expression GetComparison<T>(Expression<Func<T, bool>> expression)
        {
            var path = paths[RetrieveType.Comparison];
            var value = ProcessExpression(expression, path.Operators);
            return (Expression)value;
        }

        /// <summary>
        /// Gets the comparison expression from the provided expression.
        /// </summary>
        /// <typeparam name="T">A type of the object containing members to be compared.</typeparam>
        /// <typeparam name="U">A type of the object containing members to be compared.</typeparam>
        /// <param name="expression">The expression to inspect/</param>
        /// <returns>The expression containing the comparison.</returns>
        public Expression GetComparison<T, U>(Expression<Func<T, U, bool>> expression)
        {
            var path = paths[RetrieveType.Comparison];
            var value = ProcessExpression(expression, path.Operators);
            return (Expression)value;
        }

        private object ProcessExpression(Expression expression, ExpressionOperator[] processors)
        {
            object result = null;
            ExpressionOperator processor = null;
            for (var i = 0; i < processors.Length; i++)
            {
                processor = processors[i];
                if (processor.OperationType == ExpressionOperator.OperationNodeType.Locator)
                {
                    expression = processor.Next(expression);
                    continue;
                }
                if (processor.OperationType == ExpressionOperator.OperationNodeType.Retriever)
                {
                    break;
                }
            }
            if (processor != null && processor.OperationType == ExpressionOperator.OperationNodeType.Retriever)
            {
                result = processor.Retriever(expression);
            }
            return result;
        }

        private enum ExpressionOperatorType 
        {
            GetBody,
            GetOperand,
            GetMember,
            GetComparison,
            GetLambda,
            GetConvert
        }

        private enum RetrieveType
        {
            Member,
            Comparison,
            MemberFromConvert
        }

    }
}
