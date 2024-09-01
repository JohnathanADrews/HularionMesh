#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionCore.General;
using HularionCore.Logic;
using HularionCore.Pattern.Functional;
using HularionCore.Pattern.Identifier;
using HularionCore.Pattern.Topology;
using HularionMesh.Domain;
using HularionMesh.MeshType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HularionMesh
{
    /// <summary>
    /// Represents a node in a where expression.
    /// </summary>
    public class WhereExpressionNode
    {
        /// <summary>
        /// (Leaf node only.) The name of the property that is checked against the value and operator.
        /// </summary>
        public string Property { get; set; }
        /// <summary>
        /// (Leaf node only.) The value used to check against the indicated property.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Allows serializers to deserialize to an array.
        /// </summary>
        /// <remarks>This should be removed once a serializer is created to deserialize an array to Value as object[].</remarks>
        public object[] Values
        {
            get
            {
                if(Value == null) { return null; }
                if (Value.GetType().IsArray) { return (object[])Value; }
                if (typeof(IEnumerable<object>).IsAssignableFrom(Value.GetType())) { return ((IEnumerable<object>)Value).ToArray(); }
                return new object[] { Value };
            }
            set
            {
                if (value == null) { return; }
                var type = value.GetType();
                if (type.IsArray && ((Array)value).Length == 1) { Value = ((Array)value).GetValue(0); return; }
                //if (typeof(IEnumerable<>).IsAssignableFrom(type) && ((IEnumerable<>)value).) { }
                Value = value;
            }
        }

        /// <summary>
        /// (Leaf node only.) The comparison that will be used to check the value against the indicated property.
        /// </summary>
        public DataTypeComparison Comparison { get; set; }

        /// <summary>
        /// The type of the node.
        /// </summary>
        public DataType Type { get; set; }
        /// <summary>
        /// Provides the value of this node.
        /// Overrides the value of this node at evaluation.
        /// Use when the source is not known at node creation, e.g. batch processing.
        /// </summary>
        public IProvider<object> ValueProvider { get; set; }
        /// <summary>
        /// Inidcates that the ValueProvider should be used to provide the value of this node.
        /// </summary>
        public bool IsProvided { get; set; }
        /// <summary>
        /// The mode used to compare the node's value.  Defaults to WhereExpressionNodeValueMode.Value.
        /// </summary>
        public WhereExpressionNodeValueMode Mode { get; set; }


        /// <summary>
        /// (Parent node only.) The child nodes of this node.
        /// </summary>
        public WhereExpressionNode[] Nodes { get; set; }
        /// <summary>
        /// (Parent node only.) One of the sixteen boolean operators that will compare the results of the child node.
        /// </summary>
        /// <remarks>
        /// There must be at least two nodes. Nodes are processed in order, and only BLOCK, AND, XOR, OR, XNOR, and PASS are commutative and associative, and therefore order independent.
        /// </remarks>
        public BinaryOperator Operator { get; set; }


        /// <summary>
        /// True iff the negation of this node should be taken.
        /// </summary>
        public bool Negated { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public WhereExpressionNode()
        {
            this.Nodes = new WhereExpressionNode[]{ };
            this.Negated = false;
            this.Mode = WhereExpressionNodeValueMode.Value;
        }

        private static MemberMapper mapper = new MemberMapper();

        static WhereExpressionNode()
        {
            mapper.CreateMap<WhereExpressionNode, WhereExpressionNode>(includeNulls: true);
        }

        /// <summary>
        /// Creates a clone of this node.
        /// </summary>
        /// <returns>The cloned node.</returns>
        public WhereExpressionNode Clone()
        {
            var clone = new WhereExpressionNode();
            mapper.Map(this, clone);
            return clone;
        }

        /// <summary>
        /// Copies the provided node to this node.
        /// </summary>
        /// <param name="node">The node to copy.</param>
        public void Copy(WhereExpressionNode node)
        {
            mapper.Map(node, this);
        }

        /// <summary>
        /// Clones each node in the tree and re-constructs the tree. If the original tree had the same node appearing more than once, the new tree has a new instance for each appearance.
        /// </summary>
        /// <returns>The cloned tree.</returns>
        public WhereExpressionNode DeepClone()
        {
            var traverser = new TreeTraverser<WhereProxy>();
            var proxyRoot = new WhereProxy() { Node = this };
            var nodes = new HashSet<WhereExpressionNode>();
            var plan = traverser.CreateEvaluationPlan(TreeTraversalOrder.ParentLeftRight, proxyRoot, node =>
            {
                for(var i = 0; i < node.Node.Nodes.Length; i++)
                {
                    node.Nodes.Add(new WhereProxy() { Node = node.Node.Nodes[i] });
                }
                return node.Nodes.ToArray();
            }, true);

            foreach (var proxy in plan) { proxy.Clone = proxy.Node.Clone(); }
            foreach (var proxy in plan)
            {
                proxy.Clone.Nodes = proxy.Nodes.Select(x => x.Clone).ToArray();
            }
            return proxyRoot.Clone;
        }

        /// <summary>
        /// Creates a new WhereExpressionNode for the provided operator using this expression and the provided expression as operands.
        /// </summary>
        /// <param name="other">The other expression to append.</param>
        /// <param name="binaryOperator">The operator to use to combine the nodes.</param>
        /// <param name="thisOnLeft">If true, this expression will be at index 0 (left). Otherwise, it will be at index 1 (right).</param>
        /// <param name="negated">Negates the new expression result if true.</param>
        /// <returns>The new combined expression.</returns>
        public WhereExpressionNode CombineWithOperator(WhereExpressionNode other, BinaryOperator binaryOperator, bool thisOnLeft = true, bool negated = false)
        {
            var result = WhereExpressionNode.CreateBinaryOperatorNode(binaryOperator, negated: negated);
            if (thisOnLeft)
            {
                result.Nodes[0] = this;
                result.Nodes[1] = other;
            }
            else
            {
                result.Nodes[0] = other;
                result.Nodes[1] = this;
            }
            return result;
        }

        private class WhereProxy
        {
            public WhereExpressionNode Node { get; set; }

            public WhereExpressionNode Clone { get; set; }

            public List<WhereProxy> Nodes { get; set; } = new List<WhereProxy>();
        }

        /// <summary>
        /// Converts this WhereExpressionNode (tree) to one that uses only AND and OR operators.
        /// </summary>
        /// <returns></returns>
        public WhereExpressionNode ToAndOr()
        {
            var root = this.DeepClone();
            var traverser = new TreeTraverser<WhereExpressionNode>();
            var plan = traverser.CreateEvaluationPlan(TreeTraversalOrder.ParentLeftRight, root, node => node.Nodes, true);
            foreach (var node in plan)
            {
                if (node.Operator == BinaryOperator.BLOCK) 
                {
                    node.Nodes = new WhereExpressionNode[] { };
                    node.Comparison = DataTypeComparison.Equal;
                    node.Mode = WhereExpressionNodeValueMode.Constant;
                    node.Value = false;
                }
                if (node.Operator == BinaryOperator.AND) { }
                if (node.Operator == BinaryOperator.ANB) 
                {
                    node.Operator = BinaryOperator.AND;
                    node.Nodes[1].Negated ^= true;
                }
                if (node.Operator == BinaryOperator.EA) 
                {
                    node.Operator = BinaryOperator.AND;
                    node.Nodes = new WhereExpressionNode[] { node.Nodes[0], WhereExpressionNode.CreateWhereTrue() };
                }

                if (node.Operator == BinaryOperator.NAB)
                {
                    node.Operator = BinaryOperator.AND;
                    node.Nodes[0].Negated ^= true;
                }
                if (node.Operator == BinaryOperator.EB)
                {
                    node.Operator = BinaryOperator.AND;
                    node.Nodes = new WhereExpressionNode[] { WhereExpressionNode.CreateWhereTrue(), node.Nodes[1] };
                }
                if (node.Operator == BinaryOperator.XOR)
                {
                    var left = new WhereExpressionNode() { Operator = BinaryOperator.AND, Nodes = new WhereExpressionNode[] { node.Nodes[0], node.Nodes[1].Clone() } };
                    left.Nodes[1].Negated ^= true;
                    var right = new WhereExpressionNode() { Operator = BinaryOperator.AND, Nodes = new WhereExpressionNode[] { node.Nodes[0].Clone(), node.Nodes[1] } };
                    right.Nodes[0].Negated ^= true;
                    node.Operator = BinaryOperator.OR;
                    node.Nodes = new WhereExpressionNode[] { left, right };
                }
                if (node.Operator == BinaryOperator.OR) { }

                if (node.Operator == BinaryOperator.NOR) 
                {
                    node.Operator = BinaryOperator.OR;
                    node.Negated ^= true;
                }
                if (node.Operator == BinaryOperator.XNOR)
                {
                    var left = new WhereExpressionNode() { Operator = BinaryOperator.AND, Nodes = new WhereExpressionNode[] { node.Nodes[0], node.Nodes[1] } };
                    var right = new WhereExpressionNode() { Operator = BinaryOperator.AND, Nodes = new WhereExpressionNode[] { node.Nodes[0].Clone(), node.Nodes[1].Clone() } };
                    right.Nodes[0].Negated ^= true;
                    right.Nodes[1].Negated ^= true;
                    node.Operator = BinaryOperator.OR;
                    node.Nodes = new WhereExpressionNode[] { left, right };
                }
                if (node.Operator == BinaryOperator.NB)
                {
                    node.Operator = BinaryOperator.AND;
                    node.Nodes = new WhereExpressionNode[] { WhereExpressionNode.CreateWhereTrue(), node.Nodes[1] };
                    node.Nodes[1].Negated ^= true;
                }
                if (node.Operator == BinaryOperator.AORNB)
                {
                    node.Operator = BinaryOperator.OR;
                    node.Nodes[1].Negated ^= true;
                }

                if (node.Operator == BinaryOperator.NA)
                {
                    node.Operator = BinaryOperator.AND;
                    node.Nodes = new WhereExpressionNode[] { node.Nodes[0], WhereExpressionNode.CreateWhereTrue() };
                    node.Nodes[0].Negated ^= true;
                }
                if (node.Operator == BinaryOperator.NAORB)
                {
                    node.Operator = BinaryOperator.OR;
                    node.Nodes[0].Negated ^= true;
                }
                if (node.Operator == BinaryOperator.NAND)
                {
                    node.Operator = BinaryOperator.AND;
                    node.Negated ^= true;
                }
                if (node.Operator == BinaryOperator.PASS)
                {
                    node.Nodes = new WhereExpressionNode[] { };
                    node.Comparison = DataTypeComparison.Equal;
                    node.Mode = WhereExpressionNodeValueMode.Constant;
                    node.Value = true;
                }
            }
            //replace double-referenced nodes with clones.
            return root.DeepClone();
        }

        /// <summary>
        /// Creates a where expression for the common key matching case.
        /// </summary>
        /// <param name="keys">The keys to match.</param>
        /// <returns>A where expression for the common key matching case.</returns>
        public static WhereExpressionNode CreateKeysIn(params object[] keys)
        {
            var node = new WhereExpressionNode()
            {
                Property = MeshKeyword.Key.Alias,
                Comparison = DataTypeComparison.In,
                Value = keys,
                Mode = WhereExpressionNodeValueMode.Key
            };
            if (keys.Length > 0)
            {
                if (keys[0].GetType() == typeof(string))
                {
                    node.Type = DataType.Text8;
                }
            }
            return node;
        }

        /// <summary>
        /// Creates a where expression for the common key matching case.
        /// </summary>
        /// <param name="keys">The keys to match.</param>
        /// <returns>A where expression for the common key matching case.</returns>
        public static WhereExpressionNode CreateKeysIn(IEnumerable<object> keys)
        {
            return CreateKeysIn(keys.ToArray());
        }

        /// <summary>
        /// Creates a where expression for matching a property value with the provided values.
        /// </summary>
        /// <param name="values">The values to match.</param>
        /// <returns>A where expression for matching a property value with the provided values.</returns>
        public static WhereExpressionNode CreateMemberIn(string propertyName, IEnumerable<object> values)
        {
            return CreateMemberIn(WhereExpressionNodeValueMode.Value, values.ToArray(), propertyName);
        }


        /// <summary>
        /// Creates a where expression for matching a property value with the provided values.
        /// </summary>
        /// <param name="values">The values to match.</param>
        /// <returns>A where expression for matching a property value with the provided values.</returns>
        private static WhereExpressionNode CreateMemberIn(WhereExpressionNodeValueMode mode, object[] values, string propertyName = null)
        {
            if(mode == WhereExpressionNodeValueMode.Key) { propertyName = MeshKeyword.Key.Alias; }
            var valueType = typeof(object);
            if (values.Length > 0) { valueType = values[0].GetType(); }
            var node = new WhereExpressionNode()
            {
                Property = propertyName,
                Comparison = DataTypeComparison.In,
                Value = values,
                Mode = mode,
                Type = DataType.FromCSharpType(valueType)
            };
            if(node.Type == DataType.UnknownCSType)
            {
                if (typeof(IMeshKey).IsAssignableFrom(valueType)) { node.Type = DataType.Text8; }
            }
            return node;
        }

        /// <summary>
        /// Creates a node that evaluates to true.
        /// </summary>
        /// <returns>A node that evaluates to true.</returns>
        public static WhereExpressionNode CreateWhereTrue()
        {
            return new WhereExpressionNode() { Mode = WhereExpressionNodeValueMode.Constant, Comparison = DataTypeComparison.Equal, Value = true, Type = DataType.Truth };
        }

        /// <summary>
        /// Creates a node that evaluates to false.
        /// </summary>
        /// <returns>A node that evaluates to false.</returns>
        public static WhereExpressionNode CreateWhereFalse()
        {
            return new WhereExpressionNode() { Mode = WhereExpressionNodeValueMode.Constant, Comparison = DataTypeComparison.Equal, Value = false, Type = DataType.Truth };
        }

        /// <summary>
        /// Creates a node using the operator.
        /// </summary>
        /// <returns>A node using the operator.</returns>
        public static WhereExpressionNode CreateBinaryOperatorNode(BinaryOperator binaryOperator, bool negated = false, bool createNodes = false)
        {
            if (createNodes)
            {
                var where = new WhereExpressionNode() { Operator = binaryOperator, Negated = negated, Nodes = new WhereExpressionNode[2] };
                where.Nodes[0] = new WhereExpressionNode();
                where.Nodes[1] = new WhereExpressionNode();
                return where;
            }
            return new WhereExpressionNode() { Operator = binaryOperator, Negated = negated, Nodes = new WhereExpressionNode[2] };
        }

        /// <summary>
        /// Creates a node using the operator.
        /// </summary>
        /// <returns>A node using the operator.</returns>
        public static WhereExpressionNode CreateOperatorNotNode(bool negated = false)
        {
            return new WhereExpressionNode() { Negated = true ^ negated, Comparison = DataTypeComparison.Not, Nodes = new WhereExpressionNode[1] };
        }

        /// <summary>
        /// Creates a node using the comparison.
        /// </summary>
        /// <returns>A node using the comparison.</returns>
        public static WhereExpressionNode CreateComparisonNode(DataTypeComparison comparison, string property = null, object value = null, bool negated = false)
        {
            var result = new WhereExpressionNode() { Comparison = comparison, Negated = negated, Property = property, Value = value };            
            return result;
        }

        /// <summary>
        /// A read all where clause.
        /// </summary>
        /// <returns>A where clause matching all values.</returns>
        public static WhereExpressionNode ReadAll { get { return new WhereExpressionNode() { Value = true, Mode = WhereExpressionNodeValueMode.Constant, Type = DataType.Truth }; } }

        /// <summary>
        /// A read nonde where clause.
        /// </summary>
        /// <returns>A where clause matching no values.</returns>
        public static WhereExpressionNode ReadNone { get { return new WhereExpressionNode() { Value = false, Mode = WhereExpressionNodeValueMode.Constant, Type = DataType.Truth }; } }

        /// <summary>
        /// Creates a node that evaluates to true.
        /// </summary>
        /// <returns>A node that evaluates to true.</returns>
        public static WhereExpressionNode WhereTrue { get { return new WhereExpressionNode() { Mode = WhereExpressionNodeValueMode.Constant, Comparison = DataTypeComparison.Equal, Value = true, Type = DataType.Truth }; } }

        /// <summary>
        /// Creates a node that evaluates to false.
        /// </summary>
        /// <returns>A node that evaluates to false.</returns>
        public static WhereExpressionNode WhereFalse { get { return new WhereExpressionNode() { Mode = WhereExpressionNodeValueMode.Constant, Comparison = DataTypeComparison.Equal, Value = false, Type = DataType.Truth }; } }

    }

    /// <summary>
    /// Indicates the value mode.
    /// </summary>
    public enum WhereExpressionNodeValueMode
    {
        /// <summary>
        /// The where expression node compares the key of the object to the node value.
        /// </summary>
        Key,
        /// <summary>
        /// The where expression node compares the value to the object's property with the indicated name.
        /// </summary>
        Value,
        /// <summary>
        /// The where expression node compares the value to the object's metadata with the indicated name.
        /// </summary>
        Meta,
        /// <summary>
        /// The where expression node is a constant boolean value.
        /// </summary>
        Constant
    }

}
