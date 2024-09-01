#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionCore.Pattern.Functional;
using HularionCore.Pattern.Identifier;
using HularionCore.Pattern.Topology;
using System;
using System.Collections.Generic;
using System.Text;

namespace HularionMesh.Translator.SqlBase
{
    /// <summary>
    /// The details for configuring a WhereExpressionNode. 
    /// </summary>
    /// <remarks>The details needed to adjust the Where node and apply it to multiple columns.</remarks>
    public class WhereTransformRequest
    {
        /// <summary>
        /// A where node with column comparison.
        /// </summary>
        public WhereExpressionNode Root { get; set; }

        /// <summary>
        /// Provides property information for the corresponding where node.
        /// </summary>
        public IParameterizedProvider<WhereExpressionNode, WhereNodeInformation> WhereInformationProvider { get; set; }

        /// <summary>
        /// Traverses the where node tree and returns an array of WhereExpressionNode according to the specified order.
        /// </summary>
        /// <param name="traverseOrder">The order in which to traverse the tree,</param>
        /// <returns>An array of WhereExpressionNode according to the specified order.</returns>
        public WhereExpressionNode[] GetWhereEvaluationPlan(TreeTraversalOrder traverseOrder = TreeTraversalOrder.ParentLeftRight)
        {
            var traverser = new TreeTraverser<WhereExpressionNode>();
            var plan = traverser.CreateEvaluationPlan(traverseOrder, Root, node => node.Nodes, true);
            return plan;
        }

    }
}
