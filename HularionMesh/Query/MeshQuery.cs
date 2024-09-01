#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion


using HularionCore.Logic;
using HularionCore.Pattern.Topology;
using HularionMesh.Domain;
using HularionMesh.DomainAggregate;
using HularionMesh.DomainValue;
using HularionMesh.Query.ExpressionHandler;
using HularionMesh.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace HularionMesh.Query
{
    /// <summary>
    /// An abstract mesh query containing shared query information.
    /// </summary>
    public abstract class MeshQuery
    {

        /// <summary>
        /// The name given to the query.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The domain of the query item.
        /// </summary>
        public MeshDomain Domain { get; protected set; }

        /// <summary>
        /// The query that created this query.
        /// </summary>
        public MeshQuery Creator { get; protected set; }

        /// <summary>
        /// The type of the query domain.
        /// </summary>
        public Type QueryDomainType { get; set; }

        /// <summary>
        /// The where expression node applied to the domain node of the query each time it is visited.
        /// </summary>
        public WhereExpressionNode DomainWhere { get; set; } = WhereExpressionNode.ReadAll;

        /// <summary>
        /// The where expression applied to the domain node the first time it is visited on a branch.
        /// </summary>
        public IDictionary<int, WhereExpressionNode> RecurrenceWhere { get; protected set; } = new Dictionary<int, WhereExpressionNode>();

        /// <summary>
        /// The items that will be read in the query.
        /// </summary>
        public DomainReadRequest Reads { get; protected set; } = DomainReadRequest.ReadAll;

        /// <summary>
        /// The query nodes that are linked to this query.
        /// </summary>
        public IDictionary<string, MeshQuery> Links { get; protected set; } = new Dictionary<string, MeshQuery>();

        /// <summary>
        /// The where expression that will be applied ( AND(ed) ) to a query node linked to this node.
        /// </summary>
        public IDictionary<string, WhereExpressionNode> LinkedWhere { get; protected set; } = new Dictionary<string, WhereExpressionNode>();

        protected MeshRepository repository;

        protected ExpressionProcessor ExpressionProcessor = new ExpressionProcessor();


        /// <summary>
        /// This query imposes on each query in Impositions.Keys for the number of records specified. (i.e. there must be x records in order to allow the ancestor.)
        /// </summary>
        public HashSet<MeshQuery> Impositions { get; set; } = new HashSet<MeshQuery>();

        /// <summary>
        /// Creates the aggregate query item given this query.
        /// </summary>
        /// <returns>The aggregate query item.</returns>
        public abstract AggregateQueryItem CreateAggregateQueryItem();
    }

    /// <summary>
    /// Contains all of the details for performing a query from the caller's perspective.
    /// </summary>
    public class MeshQuery<DomainType> : MeshQuery
        where DomainType : class
    {

        public MeshQuery(MeshRepository repository)
        {
            this.repository = repository;
            QueryDomainType = typeof(DomainType);
            Domain = repository.GetDomainFromType<DomainType>();
        }


        #region Select

        /// <summary>
        /// Selects the domain values to read.
        /// </summary>
        /// <param name="reads">The domain values to read.</param>
        /// <returns>This query.</returns>
        public MeshQuery<DomainType> Select(DomainReadRequest reads)
        {
            this.Reads = reads;
            return this;
        }

        /// <summary>
        /// Selects all domain values.
        /// </summary>
        /// <returns>This query.</returns>
        public MeshQuery<DomainType> SelectAll()
        {
            this.Reads = DomainReadRequest.ReadAll;
            return this;
        }

        /// <summary>
        /// Selects just the domain keys to read.
        /// </summary>
        /// <returns>This query.</returns>
        public MeshQuery<DomainType> SelectJustKeys()
        {
            this.Reads = DomainReadRequest.ReadKeys;
            return this;
        }

        /// <summary>
        /// Adds all the domain meta values to the read request.
        /// </summary>
        /// <returns>This query.</returns>
        public MeshQuery<DomainType> SelectAllMeta()
        {
            Reads.Meta = MeshDomain.MetaProperties.Select(x => x.Name).ToList();
            return this;
        }

        /// <summary>
        /// Adds all the domain property values to the read request.
        /// </summary>
        /// <returns>This query.</returns>
        public MeshQuery<DomainType> SelectAllValues()
        {
            var proxies = new HashSet<string>(Domain.Properties.Select(x=>x.Name));
            Reads.Values = Domain.Properties.Where(x=>proxies.Contains(x.Name)).Select(x => x.Name).ToList();
            return this;
        }


        /// <summary>
        /// Selects the member of DomainType to retrieve (e.g. x=> x.Prop1). Returning antything else will cause an exception to be thrown (e.g. x=> 123.456).
        /// </summary>
        /// <param name="select">The expression used to derive the read.</param>
        /// <returns>This query.</returns>
        public MeshQuery<DomainType> Select(Expression<Func<DomainType, object>> select)
        {
            var member = ExpressionProcessor.GetMember(select);
            return Select(member.Name);
        }

        /// <summary>
        /// Selects the member with the specified name.
        /// </summary>
        /// <param name="memberName">The name of the member to select.</param>
        /// <returns>This query.</returns>
        public MeshQuery<DomainType> Select(string memberName)
        {
            var type = typeof(DomainType);
            var proxyType = typeof(DomainPropertyAttribute);
            var property = type.GetProperties().Where(x => x.Name == memberName).FirstOrDefault();
            if (property != null)
            {
                var proxy = property.GetCustomAttributes(false).Where(x => x.GetType() == proxyType).FirstOrDefault();
                if (proxy == null) { Reads.Values.Add(memberName); }
                else if (((DomainPropertyAttribute)proxy).Selector != DomainObjectPropertySelector.Key)
                {
                    Reads.Meta.Add(((DomainPropertyAttribute)proxy).Selector.ToString());
                }
                return this;
            }
            var field = type.GetFields().Where(x => x.Name == memberName).FirstOrDefault();
            if (field != null)
            {
                var proxy = field.GetCustomAttributes(false).Where(x => x.GetType() == proxyType).FirstOrDefault();
                if (proxy == null) { Reads.Values.Add(memberName); }
                else if (((DomainPropertyAttribute)proxy).Selector != DomainObjectPropertySelector.Key)
                {
                    Reads.Meta.Add(((DomainPropertyAttribute)proxy).Selector.ToString());
                }
                return this;
            }
            Reads.Values.Add(memberName);
            return this;
        }

        #endregion


        #region Where

        /// <summary>
        /// Sets the where clause for a domain node.
        /// </summary>
        /// <param name="where">The where expression node to apply to the domain query node.</param>
        /// <remarks>The applies to domain nodes at each recursion. This node is AND(ed) with the recursion where if there is one.</remarks>
        /// <returns>This query.</returns>
        public MeshQuery<DomainType> Where(WhereExpressionNode where)
        {
            this.DomainWhere = where;
            return this;
        }


        /// <summary>
        /// Sets the where clause for a domain node using a lambda expression.
        /// </summary>
        /// <param name="where">The where clause to apply.</param>
        /// /// <param name="whereMode">Optional. Specifies how the where will be set.</param>
        /// <remarks>The applies to domain nodes at each recursion. This node is AND(ed) with the recursion where if there is one.</remarks>
        /// <returns>This query.</returns>
        public MeshQuery<DomainType> Where(Expression<Func<DomainType, bool>> where, QueryWhereMode whereMode = QueryWhereMode.Inferred)
        {
            var derived = DeriveWhere(where);
            if (whereMode == QueryWhereMode.Inferred)
            {
                if(Creator == null) { DomainWhere = derived; }
                else { Creator.LinkedWhere[Name] = derived; }
            }
            if (whereMode == QueryWhereMode.Domain) { Creator.LinkedWhere[Name] = derived; }
            if (whereMode == QueryWhereMode.Linked && Creator != null) { Creator.LinkedWhere[Name] = Creator.LinkedWhere[Name] = derived; }
            return this;
        }

        private WhereExpressionNode DeriveWhere(Expression<Func<DomainType, bool>> where)
        {
            bool hasComparison = false;
            var traverser = new TreeTraverser<ExpressionNode>();
            var root = new ExpressionNode() { Expression = ExpressionProcessor.GetComparison(where) };
            var getNextBinary = new Action<ExpressionNode>((node) =>
            {
                hasComparison = true;
                node.Type = ExpressionNode.NodeType.BinaryOperator;
                var expression = (BinaryExpression)node.Expression;
                node.Next = new ExpressionNode[]
                {
                    new ExpressionNode(){ Expression = expression.Left },
                    new ExpressionNode(){ Expression = expression.Right }
                };
            });


            var plan = traverser.CreateEvaluationPlan(TreeTraversalOrder.ParentLeftRight, root, node =>
            {
                switch (node.Expression.NodeType)
                {
                    case ExpressionType.AndAlso:
                        node.Where = WhereExpressionNode.CreateBinaryOperatorNode(BinaryOperator.AND);
                        getNextBinary(node);
                        return node.Next;
                    case ExpressionType.OrElse:
                        node.Where = WhereExpressionNode.CreateBinaryOperatorNode(BinaryOperator.OR);
                        getNextBinary(node);
                        return node.Next;
                    case ExpressionType.ExclusiveOr:
                        node.Where = WhereExpressionNode.CreateBinaryOperatorNode(BinaryOperator.XOR);
                        getNextBinary(node);
                        return node.Next;
                    case ExpressionType.LessThan:
                        node.Where = WhereExpressionNode.CreateComparisonNode(MeshType.DataTypeComparison.LessThan);
                        getNextBinary(node);
                        return node.Next;
                    case ExpressionType.LessThanOrEqual:
                        node.Where = WhereExpressionNode.CreateComparisonNode(MeshType.DataTypeComparison.LessThanOrEqualTo);
                        getNextBinary(node);
                        return node.Next;
                    case ExpressionType.GreaterThan:
                        node.Where = WhereExpressionNode.CreateComparisonNode(MeshType.DataTypeComparison.GreaterThan);
                        getNextBinary(node);
                        return node.Next;
                    case ExpressionType.GreaterThanOrEqual:
                        node.Where = WhereExpressionNode.CreateComparisonNode(MeshType.DataTypeComparison.GreaterThanOrEqualTo);
                        getNextBinary(node);
                        return node.Next;
                    case ExpressionType.Equal:
                        node.Where = WhereExpressionNode.CreateComparisonNode(MeshType.DataTypeComparison.Equal);
                        getNextBinary(node);
                        return node.Next;
                    case ExpressionType.NotEqual:
                        node.Where = WhereExpressionNode.CreateComparisonNode(MeshType.DataTypeComparison.NotEqual);
                        getNextBinary(node);
                        return node.Next;
                    case ExpressionType.Not:
                        node.Where = WhereExpressionNode.CreateOperatorNotNode();
                        node.Type = ExpressionNode.NodeType.UnaryOperator;
                        node.Next = new ExpressionNode[] { new ExpressionNode() { Expression = ((UnaryExpression)node.Expression).Operand } };
                        return node.Next;
                    case ExpressionType.MemberAccess:
                        if (!hasComparison) { throw new ArgumentException(String.Format("The provided expression must contain a comparison to be a valid where clause. (e.g. &&, ||, ^, ==, != <, <=, >, >=). [w58Jmnn9fk6L2RZH7UhrlA]")); }
                        node.Type = ExpressionNode.NodeType.Member;
                        return node.Next;
                    case ExpressionType.Constant:
                        if (!hasComparison) { throw new ArgumentException(String.Format("The provided expression must contain a comparison to be a valid where clause. (e.g. &&, ||, ^, ==, != <, <=, >, >=). [w58Jmnn9fk6L2RZH7UhrlA]")); }
                        node.Type = ExpressionNode.NodeType.Constant;
                        return node.Next;
                    case ExpressionType.Convert:
                        node.Type = ExpressionNode.NodeType.Convert;
                        var expression = node.Expression;
                        while(expression.NodeType == ExpressionType.Convert) { expression = ((UnaryExpression)expression).Operand; }
                        node.Expression = expression;
                        if (expression.NodeType == ExpressionType.MemberAccess)
                        {
                            node.Type = ExpressionNode.NodeType.Member;
                        }
                        if(expression.NodeType == ExpressionType.Constant)
                        {
                            node.Type = ExpressionNode.NodeType.Constant;
                        }
                        //node.Next = new ExpressionNode[] { new ExpressionNode() { Expression = expression } };
                        return node.Next;
                    default:
                        throw new ArgumentException(String.Format("The provided expression contains an unhandled operation. {0} {1}. [ce36hUYe9UKcqCk2Fakz3w]", node.Expression.NodeType, node.ToString()));
                }
            }, true);

            for (var i = 0; i < plan.Length; i++)
            {
                var node = plan[i];
                if (node.Expression == null) { continue; }
                if (node.Type == ExpressionNode.NodeType.BinaryOperator)
                {
                    var next0 = node.Next[0];
                    if (next0.Type == ExpressionNode.NodeType.UnaryOperator || next0.Type == ExpressionNode.NodeType.BinaryOperator)
                    {
                        node.Where.Nodes[0] = next0.Where;
                    }
                    if (next0.Type == ExpressionNode.NodeType.Member)
                    {
                        node.Where.Property = ((MemberExpression)next0.Expression).Member.Name;
                        node.Where.Nodes = new WhereExpressionNode[] { };
                    }
                    if (next0.Type == ExpressionNode.NodeType.Constant)
                    {
                        node.Where.Value = ((ConstantExpression)next0.Expression).Value;
                        node.Where.Nodes = new WhereExpressionNode[] { };
                    }

                    var next1 = node.Next[1];
                    if (next1.Type == ExpressionNode.NodeType.UnaryOperator || next1.Type == ExpressionNode.NodeType.BinaryOperator)
                    {
                        node.Where.Nodes[1] = next1.Where;
                    }
                    if (next1.Type == ExpressionNode.NodeType.Member)
                    {
                        node.Where.Property = ((MemberExpression)next1.Expression).Member.Name;
                        node.Where.Nodes = new WhereExpressionNode[] { };
                    }
                    if (next1.Type == ExpressionNode.NodeType.Constant)
                    {
                        node.Where.Value = ((ConstantExpression)next1.Expression).Value;
                        node.Where.Nodes = new WhereExpressionNode[] { };
                    }
                }
                if (node.Type == ExpressionNode.NodeType.UnaryOperator)
                {
                    var next0 = node.Next[0];
                    if (next0.Type == ExpressionNode.NodeType.UnaryOperator || next0.Type == ExpressionNode.NodeType.BinaryOperator)
                    {
                        node.Where.Nodes[0] = next0.Where;
                    }
                    if (next0.Type == ExpressionNode.NodeType.Member)
                    {
                        node.Where.Property = ((MemberExpression)next0.Expression).Member.Name;
                        node.Where.Nodes = new WhereExpressionNode[] { };
                    }
                    if (next0.Type == ExpressionNode.NodeType.Constant)
                    {
                        node.Where.Value = ((ConstantExpression)next0.Expression).Value;
                        node.Where.Nodes = new WhereExpressionNode[] { };
                    }
                }
                if (node.Type == ExpressionNode.NodeType.Convert)
                {

                }

            }
            return plan[0].Where;
        }

        /// <summary>
        /// Creates a domain where clause specifying that the member's value be included among the provided values.
        /// </summary>
        /// <param name="memberSelector">Selects the member of DomainType to retrieve (e.g. x=> x.Prop1). Returning antything else will cause an exception to be thrown (e.g. x=> 123.456)</param>
        /// <param name="whereMode">Optional. Specifies how the where will be set.</param>
        /// <param name="values">The set of values that can be matched.</param>
        /// <returns>This query.</returns>
        public MeshQuery<DomainType> WhereIn(Expression<Func<DomainType, object>> memberSelector, QueryWhereMode whereMode = QueryWhereMode.Inferred, params object[] values)
        {
            var member = ExpressionProcessor.GetMember<DomainType>(memberSelector);
            WhereIn(memberSelector, values, whereMode: whereMode);
            var where = WhereExpressionNode.CreateMemberIn(member.Name, values);
            if (whereMode == QueryWhereMode.Inferred)
            {
                if (Creator == null) { DomainWhere = where; }
                else { Creator.LinkedWhere[Name] = where; }
            }
            if (whereMode == QueryWhereMode.Domain) { Creator.LinkedWhere[Name] = where; }
            if (whereMode == QueryWhereMode.Linked && Creator != null) { Creator.LinkedWhere[Name] = Creator.LinkedWhere[Name] = where; }
            return this;
        }

        /// <summary>
        /// Creates a domain where clause specifying that the member's value be included among the provided values.
        /// </summary>
        /// <param name="memberSelector">Selects the member of DomainType to retrieve (e.g. x=> x.Prop1). Returning antything else will cause an exception to be thrown (e.g. x=> 123.456)</param>
        /// <param name="values">The set of values that can be matched.</param>
        /// /// <param name="whereMode">Optional. Specifies how the where will be set.</param>
        /// <returns>This query.</returns>
        public MeshQuery<DomainType> WhereIn(Expression<Func<DomainType, object>> memberSelector, IEnumerable<object> values, QueryWhereMode whereMode = QueryWhereMode.Inferred)
        {
            var member = ExpressionProcessor.GetMember<DomainType>(memberSelector);
            var where = WhereExpressionNode.CreateMemberIn(member.Name, values);
            if (whereMode == QueryWhereMode.Inferred)
            {
                if (Creator == null) { DomainWhere = where; }
                else { Creator.LinkedWhere[Name] = where; }
            }
            if (whereMode == QueryWhereMode.Domain) { Creator.LinkedWhere[Name] = where; }
            if (whereMode == QueryWhereMode.Linked && Creator != null) { Creator.LinkedWhere[Name] = Creator.LinkedWhere[Name] = where; }
            return this;
        }

        /// <summary>
        /// Creates a domain where clause specifying that the member's value be included among the provided values for the specified recursion index.
        /// </summary>
        /// <param name="recursionIndex">The zero-based nth recursion of the node for a query branch.</param>
        /// <param name="memberSelector">Selects the member of DomainType to retrieve (e.g. x=> x.Prop1). Returning antything else will cause an exception to be thrown (e.g. x=> 123.456)</param>
        /// <param name="values">The set of values that can be matched.</param>
        /// <returns>This query.</returns>
        public MeshQuery<DomainType> WhereIn(int recursionIndex, Expression<Func<DomainType, object>> memberSelector, params object[] values)
        {
            var member = ExpressionProcessor.GetMember<DomainType>(memberSelector);
            RecurrenceWhere[recursionIndex] = WhereExpressionNode.CreateMemberIn(member.Name, values);
            return this;
        }

        /// <summary>
        /// Creates a domain where clause specifying that the member's value be included among the provided values for the specified recursion index.
        /// </summary>
        /// <param name="recursionIndex">The zero-based nth recursion of the node for a query branch.</param>
        /// <param name="memberSelector">Selects the member of DomainType to retrieve (e.g. x=> x.Prop1). Returning antything else will cause an exception to be thrown (e.g. x=> 123.456)</param>
        /// <param name="values">The set of values that can be matched.</param>
        /// <returns>This query.</returns>
        public MeshQuery<DomainType> WhereIn(int recursionIndex, Expression<Func<DomainType, object>> memberSelector, IEnumerable<object> values)
        {
            var member = ExpressionProcessor.GetMember<DomainType>(memberSelector);
            RecurrenceWhere[recursionIndex] = WhereExpressionNode.CreateMemberIn(member.Name, values);
            return this;
        }

        #endregion


        #region Link

        /// <summary>
        /// Creates a query link to the selected member.
        /// </summary>
        /// <typeparam name="LinkedType">The type of the member to link.</typeparam>
        /// <param name="select">The expression to select the member. (e.g. x => x.Prop1)</param>
        /// <param name="impose">If true, this query will only return objects for which there is at least one linked object.</param>
        /// <returns></returns>
        public MeshQuery<LinkedType> Link<LinkedType>(Expression<Func<DomainType, LinkedType>> select, ImposeOnMode impose = ImposeOnMode.None)
            where LinkedType : class
        {
            var member = ExpressionProcessor.GetMember(select);
            Select(member.Name);
            var link = repository.CreateQuery<LinkedType>();
            Links[member.Name] = link;
            if(impose == ImposeOnMode.Strict) { link.Impositions.Add(this); }
            link.Creator = this;
            link.Name = member.Name;
            return link;
        }

        #endregion


        #region Join

        //Add these later.

        #endregion


        #region Run


        /// <summary>
        /// Creates an AggregateQueryItem from this query, including links.
        /// </summary>
        /// <returns>An AggregateQueryItem that can be used to execute the query.</returns>
        public override AggregateQueryItem CreateAggregateQueryItem()
        {

            var traverser = new TreeTraverser<MeshQuery>();
            var plan = traverser.CreateEvaluationPlan(TreeTraversalOrder.ParentLeftRight, this, node => node.Links.Values.ToArray(), true);
            var map = plan.ToDictionary(x=>x, x=>new AggregateQueryItem()
            {
                Reads = x.Reads,
                Domain = x.Domain,
                DomainWhere = x.DomainWhere
            });
            foreach(var query in plan)
            {
                var item = map[query];
                item.Impositions = new HashSet<AggregateQueryItem>(query.Impositions.Select(x=> map[x]));
                foreach (var link in query.Links)
                {
                    var linkedQuery = map[link.Value];
                    var linkQuery = new AggregateQueryItem() { Alias = link.Key, Mode = AggregateDomainMode.Link };
                    repository.SetLinkMember(query.Domain, linkedQuery.Domain, link.Key, linkQuery);
                    if (query.LinkedWhere.ContainsKey(link.Key))
                    {
                        linkedQuery.DomainWhere = query.LinkedWhere[link.Key];
                    }
                    linkQuery.Links.Add(linkedQuery);
                    item.Links.Add(linkQuery);
                }
            }

            return map[this];
        }

        /// <summary>
        /// Runs this query and returns the result.
        /// </summary>
        /// <returns>The requested objects.</returns>
        public RepositoryQueryResponse<DomainType> Render()
        {
            var query = CreateAggregateQueryItem();
            var result = repository.Query<DomainType>(query);
            return result;
        }


        #endregion


        public override string ToString()
        {
            return String.Format("{0} - {1} nodes", Name, Links.Count());
        }

        /// <summary>
        /// Utility class for deriving a WhereExpressionNode tree from an Expression tree.
        /// </summary>
        internal class ExpressionNode
        {

            public Expression Expression { get; set; }

            public WhereExpressionNode Where { get; set; }

            public NodeType Type { get; set; }

            public ExpressionNode[] Next { get; set; } = new ExpressionNode[] { };

            public enum NodeType
            {
                UnaryOperator,
                BinaryOperator,
                Comparison,
                Member,
                Constant,
                Convert
            }

        }


        #region AddLater

        /// <summary>
        /// Selects the member of DomainType to retrieve and assigns it to the member of TargetType (e.g. (d,t)=>t.PropA = d.PropX). Any other form will cause an exception to be thrown (e.g. x=>x.PropA).
        /// </summary>
        /// <typeparam name="TargetType">The type to which the output will be mapped.</typeparam>
        /// <param name="select"></param>
        /// <returns>This query.</returns>
        //public MeshQuery<DomainType> Select<TargetType>(Expression<Func<DomainType, TargetType, object>> select)
        //{

        //    return this;
        //}


        /// <summary>
        /// Joins the result of this domain query with the results of another domain query.
        /// </summary>
        /// <param name="join">The other query to join to this one.</param>
        /// <param name="where">The where clause to match any two domain objects.</param>
        /// <returns>This query.</returns>
        //public MeshQuery<DomainType> Join<JoinType>(MeshQuery<JoinType> join, Expression<Func<DomainType, JoinType, bool>> where)
        //    where JoinType : class
        //{
        //    return this;
        //}
        //public MeshQuery<DomainType> LeftJoin<JoinType>(MeshQuery<JoinType> join, Expression<Func<DomainType, JoinType, bool>> where)
        //    where JoinType : class
        //{
        //    return this;
        //}
        //public MeshQuery<DomainType> RightJoin<JoinType>(MeshQuery<JoinType> join, Expression<Func<DomainType, JoinType, bool>> where)
        //    where JoinType : class
        //{
        //    return this;
        //}


        /// <summary>
        /// Runs this query and applies the result to objects of the provided type.
        /// </summary>
        /// <typeparam name="ResultType"></typeparam>
        /// <returns></returns>
        //public RepositoryQueryResponse<ResultType> Run<ResultType>()
        //    where ResultType : class
        //{
        //}


        /// <summary>
        /// Imposes the results of this node onto the indicated ancestor. All ancestors to the target ancestor must eventually link to an object from this node in order to return a result.
        /// </summary>
        /// <typeparam name="AncestorType"></typeparam>
        /// <param name="ancestor"></param>
        /// <returns></returns>
        //public MeshQuery<DomainType> ImposeOn<AncestorType>(MeshQuery<AncestorType> ancestor)
        //    where AncestorType : class
        //{
        //    return this;
        //}


        /// <summary>
        /// Sets the where clause for a domain node that is involved in a recursion loop.
        /// </summary>
        /// <param name="recursionIndex">The one-based nth recursion of the node for a query branch. This applies when the node is linked from its parent. The index for the root query is always set to zero.</param>
        /// <param name="where">The where clause to apply.</param>
        /// <remarks>The applies only to the specified recursion index along a branch. This node is AND(ed) with the domain where if there is one.</remarks>
        /// <returns>This query.</returns>
        //public MeshQuery Where(int recursionIndex, WhereExpressionNode where)
        //{
        //    this.RecurrenceWhere[recursionIndex] = where;
        //    return this;
        //}

        /// <summary>
        /// Sets the where clause for a domain node using a lambda expression that is involved in a recursion loop.
        /// </summary>
        /// <param name="recursionIndex">The one-based nth recursion of the node for a query branch.</param>
        /// <param name="where">The where clause to apply.</param>
        /// <remarks>The applies only to the specified recursion index along a branch. This node is AND(ed) with the domain where if there is one.</remarks>
        /// <returns>The query.</returns>
        //public MeshQuery<DomainType> Where(int recursionIndex, Expression<Func<DomainType, bool>> where)
        //{
        //    RecurrenceWhere[recursionIndex] = DeriveWhere(where);
        //    return this;
        //}

        #endregion
    }

    /// <summary>
    /// The first node of a query .
    /// </summary>
    /// <typeparam name="DomainType">The type of domain used in the query.</typeparam>
    public class MeshRootQuery<DomainType> : MeshQuery<DomainType>
        where DomainType : class
    {

        /// <summary>
        /// The where expression for the first time the DomainType domain is visited.
        /// </summary>
        /// <remarks>
        /// When a query is run, if there are nodes that reference back to the root node, the DomainWhere will constantly filter those links.
        /// To prevent that filtering, use RootRecurrenceWhere to apply the where only the first time the node is visited.
        /// </remarks>
        public WhereExpressionNode RootRecurrenceWhere { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="repository">The repository used in this query.</param>
        public MeshRootQuery(MeshRepository repository) 
            :base(repository)
        {
        }

    }
}

