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
using System.Text;

namespace HularionMesh.Query.ExpressionHandler
{
    /// <summary>
    /// Operates on an expression, providing a desired node or navigating to that node.
    /// </summary>
    public class ExpressionOperator
    {
        /// <summary>
        /// The type of expression node.
        /// </summary>
        public ExpressionType NodeType { get; set; }

        /// <summary>
        /// The type of path operation to perform.
        /// </summary>
        public OperationNodeType OperationType { get; set; } = OperationNodeType.Locator;

        /// <summary>
        /// Gets the next expression node given the current one.
        /// </summary>
        public Func<Expression, Expression> Next { get; set; }

        /// <summary>
        /// Provides the desired item if this is a Retriever type.
        /// </summary>
        public Func<Expression, object> Retriever { get; set; }

        /// <summary>
        /// Indicates what mode the path item is in.
        /// </summary>
        public enum OperationNodeType
        {
            /// <summary>
            /// Navigates to the next expression node.
            /// </summary>
            Locator,
            /// <summary>
            /// Provides the desired item from the given node.
            /// </summary>
            Retriever
        }
    }
}
