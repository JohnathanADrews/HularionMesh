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
using HularionMesh;
using HularionMesh.MeshType;
using  HularionMesh.Translator.SqlBase.ORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HularionCore.Pattern.Topology;
using HularionCore.Pattern.Functional;
using HularionCore.Pattern.Identifier;

namespace  HularionMesh.Translator.SqlBase.SqlGenerator
{
    /// <summary>
    /// A SQL where clause derived from a mesh request.
    /// </summary>
    public class SqlMeshWhere
    {
        /// <summary>
        /// The string containing the where logic.
        /// </summary>
        public string Where { get; private set; }

        /// <summary>
        /// true iff "where " appears at the beginning of Where.
        /// </summary>
        public bool WhereKeywordIncluded { get; private set; }

        /// <summary>
        /// The parameters used in the where string.
        /// </summary>
        public ParameterCreator ParameterCreator { get; private set; } = new ParameterCreator();

        /// <summary>
        /// The SqlMeshRepository.
        /// </summary>
        public SqlMeshRepository Repository { get; private set; }

        /// <summary>
        /// The domain translator.
        /// </summary>
        public SqlDomainTranslator SqlDomain { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sqlDomain">The domain translator.</param>
        /// <param name="where">The root node from which the where string will be created.</param>
        /// <param name="repository">The SqlMeshRepository.</param>
        /// <param name="includeWhereKeyword">iff true, the Where string will start with "where ".</param>
        /// <param name="parameterCreator">Creates and maintains the parameters.</param>
        public SqlMeshWhere(SqlDomainTranslator sqlDomain, WhereExpressionNode where, SqlMeshRepository repository, bool includeWhereKeyword = false, ParameterCreator parameterCreator = null)
        {
            Repository = repository;
            SqlDomain = sqlDomain;
            if (parameterCreator != null) { ParameterCreator = parameterCreator; }
            SqlDomain.Properties.ToDictionary(x => x.MeshProperty.Name, x=>x.ColumnNames);
            var plan = SetupWhere(where);
            var request = new WhereTransformRequest() { Root = plan[0] };
            request.WhereInformationProvider = ParameterizedProvider.FromSingle<WhereExpressionNode, WhereNodeInformation>(node =>
            {
                var info = new WhereNodeInformation() { Where = node };
                if(node.Property == null) { return info; }
                var property = SqlDomain.Properties.Where(x => x.MeshProperty.Name == node.Property).FirstOrDefault();
                info.PrimaryProperty = new SqlProperty()
                {
                    SqlType = property.SqlType,
                    TableName = SqlDomain.TableName,
                    ColumnNames = property.ColumnNames
                };
                return info;
            });
            var result = repository.SqlRepository.WhereTransformer.Transform(request);
            SetFromWhereExpressionNode(result.Root, includeWhereKeyword);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">The model type to use when creating the Where string.</param>
        /// <param name="where">The root node from which the where string will be created.</param>
        /// <param name="repository">The SqlMeshRepository.</param>
        /// <param name="includeWhereKeyword">iff true, the Where string will start with "where ".</param>
        /// <param name="parameterCreator">Creates and maintains the parameters.</param>
        public SqlMeshWhere(Type type, WhereExpressionNode where, SqlMeshRepository repository, bool includeWhereKeyword = false, ParameterCreator parameterCreator = null)
        {
            Repository = repository;
            if (parameterCreator != null) { ParameterCreator = parameterCreator; }
            var tableInfo = repository.GetModelInfo(type);
            var plan = SetupWhere(where);
            var request = new WhereTransformRequest() { Root = plan[0] };
            request.WhereInformationProvider = ParameterizedProvider.FromSingle<WhereExpressionNode, WhereNodeInformation>(node =>
            {
                var info = new WhereNodeInformation() { Where = node };
                if(node.Property != null && tableInfo.Members.ContainsKey(node.Property))
                {
                    var property = tableInfo.Members[node.Property];
                    info.PrimaryProperty = new SqlProperty() 
                    { 
                        SqlType = property.SqlType,
                        TableName = tableInfo.TableName,
                        ColumnNames = property.CreateColumnSpecifications.Select(x=>x.Name).ToArray()
                    };
                }
                return info;
            });
            var result = repository.SqlRepository.WhereTransformer.Transform(request);
            SetFromWhereExpressionNode(result.Root, includeWhereKeyword);
        }

        private WhereExpressionNode[] SetupWhere(WhereExpressionNode where)
        {
            where = where.DeepClone();
            var traverser = new TreeTraverser<WhereExpressionNode>();
            var plan = traverser.CreateEvaluationPlan(TreeTraversalOrder.ParentLeftRight, where, node => node.Nodes, true);
            return plan;
        }

        private void SetFromWhereExpressionNode(WhereExpressionNode where, bool includeWhereKeyword = false)
        {
            where = where.ToAndOr();
            var whereClause = new StringBuilder();
            WhereKeywordIncluded = includeWhereKeyword;
            if (includeWhereKeyword) { whereClause.Append("where "); }
            var traverser = new TreeTraverser<WhereExpressionNode>();

            traverser.WeaveExecute(TreeWeaveOrder.FromLeft, where, node => node.Nodes,
                entryAction: state =>
                {
                    var node = state.Subject;
                    var property = node.Property;
                    if (node.Negated) { whereClause.Append("not ("); }
                    else { whereClause.Append("("); }
                    if (state.IsLeaf)
                    {
                        switch (node.Comparison)
                        {
                            case DataTypeComparison.In:
                                {
                                    if(node.Values.Length == 0)
                                    {
                                        whereClause.Append("(0 <> 0)");
                                        break;
                                    }
                                    whereClause.Append(String.Format("{0} in (", property));
                                    var first = true;
                                    foreach (var value in node.Values)
                                    {
                                        if (!first) { whereClause.Append(", "); }
                                        var parameter = ParameterCreator.Create(value);
                                        whereClause.Append(parameter.Name);
                                        first = false;
                                    }
                                    whereClause.Append(")");
                                }
                                break;
                            case DataTypeComparison.Equal:
                                {
                                    if (node.Mode == WhereExpressionNodeValueMode.Constant)
                                    {
                                        whereClause.Append(String.Format("0 = {0}", (bool)node.Value ? "0" : "1"));
                                        break;
                                    }
                                    var parameter = ParameterCreator.Create(node.Value);
                                    whereClause.Append(String.Format("{0} = {1}", property, parameter.Name));
                                }
                                break;
                            case DataTypeComparison.NotEqual:
                                {
                                    var parameter = ParameterCreator.Create(node.Value);
                                    whereClause.Append(String.Format("{0} <> {1}", property, parameter.Name));
                                }
                                break;
                            case DataTypeComparison.GreaterThan:
                                {
                                    var parameter = ParameterCreator.Create(node.Value);
                                    whereClause.Append(String.Format("{0} > {1}", property, parameter.Name));
                                }
                                break;
                            case DataTypeComparison.GreaterThanOrEqualTo:
                                {
                                    var parameter = ParameterCreator.Create(node.Value);
                                    whereClause.Append(String.Format("{0} >= {1}", property, parameter.Name));
                                }
                                break;
                            case DataTypeComparison.LessThan:
                                {
                                    var parameter = ParameterCreator.Create(node.Value);
                                    whereClause.Append(String.Format("{0} < {1}", property, parameter.Name));
                                }
                                break;
                            case DataTypeComparison.LessThanOrEqualTo:
                                {
                                    var parameter = ParameterCreator.Create(node.Value);
                                    whereClause.Append(String.Format("{0} <= {1}", property, parameter.Name));
                                }
                                break;
                            case DataTypeComparison.Like:
                                break;
                        }
                        whereClause.Append(")");
                    }
                },
                exitAction: state =>
                {
                    if (!state.IsLeaf) { whereClause.Append(")"); }
                    if (state.IsRoot || state.IsRowLast) { return; }
                    if (state.Parent.Operator == BinaryOperator.AND)
                    {
                        whereClause.Append(" and ");
                    }
                    else if (state.Parent.Operator == BinaryOperator.OR)
                    {
                        whereClause.Append(" or ");
                    }
                },
                lastAction: state => { },
                upAction: state => { });

            Where = whereClause.ToString();
        }


    }
}
