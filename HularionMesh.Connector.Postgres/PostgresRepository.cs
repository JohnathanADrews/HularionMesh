﻿#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion


using HularionMesh;
using HularionMesh.Domain;
using HularionMesh.DomainLink;
using HularionMesh.MeshType;
using HularionMesh.Standard;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using HularionCore.Pattern.Functional;
using HularionMesh.Translator.SqlBase;
using HularionMesh.Translator.SqlBase.ORM;
using HularionCore.Logic;

namespace HularionMesh.Connector.Postgres
{
    /// <summary>
    /// A repository that connects to a Postgres database.
    /// </summary>
    public class PostgresRepository : ISqlRepository
    {
        /// <summary>
        /// Provides an ISqlType given the mesh data type.
        /// </summary>
        public IParameterizedProvider<DataType, ISqlType> SqlTypeProvider { get { return SqlType.SqlTypeProvider; } }

        /// <summary>
        /// Provides all the database system types.
        /// </summary>
        public IProvider<IEnumerable<ISqlType>> SqlTypesProvider { get; set; } = new ProviderFunction<IEnumerable<ISqlType>>(() => SqlType.GetTypes());

        /// <summary>
        /// Creates domain object keys given the provided domain.
        /// </summary>
        /// <remarks>StandardDomainForm.DomainValueKeyCreator  is the standard creator.</remarks>
        public IParameterizedCreator<MeshDomain, IMeshKey> DomainValueKeyCreator { get { return StandardDomainForm.DomainValueKeyCreator; } }

        /// <summary>
        /// Provides the link form specification for linked domains.
        /// </summary>
        /// <remarks>StandardLinkForm.LinkKeyFormProvider is the standard specification.</remarks>
        public IParameterizedProvider<LinkedDomains, DomainLinkForm> LinkKeyFormProvider { get { return StandardLinkForm.LinkKeyFormProvider; } }

        /// <summary>
        /// Creates a language-specific name given the SqlObject.
        /// </summary>
        public IParameterizedCreator<SqlObject, string> ObjectNameCreator { get; set; } = ParameterizedCreator.FromSingle<SqlObject, string>(x => String.Format("\"{0}\"", x.Name));

        /// <summary>
        /// The string containing the connection details to connect to a Postgres database.
        /// </summary>
        public string ConnectionString { get; private set; }

        /// <summary>
        /// Transforms a WhereExpressionNode to conform to an implemented connector.
        /// </summary>
        public ITransform<WhereTransformRequest, WhereTransformResult> WhereTransformer { get; set; } = new TransformFunction<WhereTransformRequest, WhereTransformResult>(request => TransformWhere(request));

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="connectionString">The string containing the connection details to connect to a Postgres database.</param>
        public PostgresRepository(string connectionString)
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Creates the table using the provided table specification.
        /// </summary>
        /// <param name="table">The specification to create the table.</param>
        public void CreateTable(CreateTableSpecification table)
        {
            var command = new StringBuilder();
            command.Append(String.Format("create table if not exists {0} (", table.Name));

            var keys = table.Columns.Where(x => x.IsPrimaryKey).ToList();
            if (keys.Count() > 0)
            {
                foreach (var key in keys)
                {
                    if (key != keys.Last()) { command.Append(", "); }
                    command.Append(String.Format("{0} {1} primary key", key.Name, key.Type));
                }
            }
            command.Append(");\n");

            var columns = table.Columns.Where(x => !x.IsPrimaryKey).ToList();
            foreach(var column in columns)
            {
                command.Append(String.Format("alter table {0} add column if not exists {1} {2};\n",
                    table.Name,
                    column.Name,
                    column.Type));
            }
            var commandString = command.ToString();
            ExecuteCommand(commandString);
        }

        /// <summary>
        /// Adds the table columns to the table with name tableName.
        /// </summary>
        /// <param name="tableName">The name of the table to add the columns.</param>
        /// <param name="columns">The columns to add to the table.</param>
        public void AddTableColumns(string tableName, CreateColumnSpecification[] columns)
        {
            var command = new StringBuilder();
            foreach (var column in columns)
            {
                command.Append(String.Format("alter table {0} add column if not exists {1} {2};\n",
                    tableName,
                    column.Name,
                    column.Type));
            }
            var commandString = command.ToString();
            ExecuteCommand(commandString);
        }

        /// <summary>
        /// Executes the provided command.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="parameters">The parameters to use in the command.</param>
        public void ExecuteCommand(string command, IEnumerable<SqlMeshParameter> parameters = null)
        {
            using (NpgsqlConnection sqlConnection = new NpgsqlConnection(ConnectionString))
            using (NpgsqlCommand sqlCommand = new NpgsqlCommand(command, sqlConnection))
            {
                AddParameters(sqlCommand, parameters);
                sqlConnection.Open();
                sqlCommand.ExecuteNonQuery();
                sqlConnection.Close();
            }
        }

        /// <summary>
        /// Executes the query using the provided parameters and returns a DataTable with the result.
        /// </summary>
        /// <param name="query">The query to retrieve the values.</param>
        /// <param name="parameters">The parameters to the query.</param>
        /// <returns>A DataTable with the result of the query.</returns>
        public DataTable ExecuteQuery(string query, IEnumerable<SqlMeshParameter> parameters = null)
        {
            var table = new DataTable();
            using (NpgsqlConnection sqlConnection = new NpgsqlConnection(ConnectionString))
            using (NpgsqlCommand sqlCommand = new NpgsqlCommand(query, sqlConnection))
            {
                AddParameters(sqlCommand, parameters);
                sqlConnection.Open();
                table.Load(sqlCommand.ExecuteReader());
                sqlConnection.Close();
            }
            return table;
        }

        private IEnumerable<NpgsqlParameter> ConvertParameters(IEnumerable<SqlMeshParameter> parameters)
        {
            if(parameters == null) { return new List<NpgsqlParameter>(); }
            return parameters.Select(x => new NpgsqlParameter() { ParameterName = x.Name, Value = x.Value == null ? DBNull.Value : x.Value }).ToList();
        }

        private void AddParameters(NpgsqlCommand command, IEnumerable<SqlMeshParameter> parameters)
        {
            var sqlParameters = ConvertParameters(parameters);
            foreach(var parameter in sqlParameters) { command.Parameters.Add(parameter); }
        }

        /// <summary>
        /// Creates the database using the provided name and connection string.
        /// </summary>
        /// <param name="connectionString">The connection details.</param>
        /// <param name="databaseName">The name of the database to create.</param>
        public static void CreateDatabase(string connectionString, string databaseName)
        {
            try
            {
                var command = String.Format("create database {0};", databaseName);
                using (NpgsqlConnection sqlConnection = new NpgsqlConnection(connectionString))
                using (NpgsqlCommand sqlCommand = new NpgsqlCommand(command, sqlConnection))
                {
                    sqlConnection.Open();
                    sqlCommand.ExecuteNonQuery();
                    sqlConnection.Close();
                }
            }
            catch (PostgresException e)
            {
                if (e.SqlState == "42P04")
                {
                    //the database already exists. Keep going.
                }
                else
                {
                    throw (e);
                }
            }
        }

        private static WhereTransformResult TransformWhere(WhereTransformRequest request)
        {
            var result = new WhereTransformResult();
            result.Root = request.Root;
            var plan = request.GetWhereEvaluationPlan();
            foreach(var node in plan)
            {
                var info = request.WhereInformationProvider.Provide(node);
                if(info.PrimaryProperty != null)
                {
                    if(info.PrimaryProperty.SqlType.SqlTypeCount == 1)
                    {
                        node.Property = info.PrimaryProperty.ColumnNames[0];
                        continue;
                    }
                    if (info.PrimaryProperty.SqlType == SqlType.UnsignedInteger64)
                    {
                        var saved = node.Clone();

                        if (saved.Comparison == DataTypeComparison.Equal
                            || saved.Comparison == DataTypeComparison.NotEqual
                            || saved.Comparison == DataTypeComparison.GreaterThan
                            || saved.Comparison == DataTypeComparison.GreaterThanOrEqualTo
                            || saved.Comparison == DataTypeComparison.LessThan
                            || saved.Comparison == DataTypeComparison.LessThanOrEqualTo)
                        {
                            var values = info.PrimaryProperty.SqlType.ToSqlTransform.Transform(saved.Value);
                            var newNode = MakeULongEquals(info.PrimaryProperty.ColumnNames, values);
                            switch (saved.Comparison)
                            {
                                case DataTypeComparison.Equal:
                                    newNode.Nodes[0].Comparison = DataTypeComparison.Equal;
                                    newNode.Nodes[1].Comparison = DataTypeComparison.Equal;
                                    break;
                                case DataTypeComparison.NotEqual:
                                    newNode.Negated = true;
                                    newNode.Nodes[0].Comparison = DataTypeComparison.Equal;
                                    newNode.Nodes[1].Comparison = DataTypeComparison.Equal;
                                    break;
                                case DataTypeComparison.GreaterThan:
                                    MakeULongGreaterThan(newNode, info.PrimaryProperty.ColumnNames, values);
                                    break;
                                case DataTypeComparison.GreaterThanOrEqualTo:
                                    MakeULongGreaterThanOrEqualTo(newNode, info.PrimaryProperty.ColumnNames, values);
                                    break;
                                case DataTypeComparison.LessThan:
                                    MakeULongGreaterThanOrEqualTo(newNode, info.PrimaryProperty.ColumnNames, values);
                                    newNode.Negated = true;
                                    break;
                                case DataTypeComparison.LessThanOrEqualTo:
                                    MakeULongGreaterThan(newNode, info.PrimaryProperty.ColumnNames, values);
                                    newNode.Negated = true;
                                    break;
                            }
                            newNode.Negated ^= node.Negated;
                            node.Copy(newNode);
                        }

                        if(saved.Comparison == DataTypeComparison.In)
                        {
                            var values = new HashSet<object>(saved.Values).ToArray();
                            if(values.Length == 0) { continue; }

                            var zeros = new List<object>();
                            var ones = new List<object>();
                            foreach (var value in values)
                            {
                                var parts = info.PrimaryProperty.SqlType.ToSqlTransform.Transform(value);
                                if ((short)parts[0] == 0) { zeros.Add(parts[1]); }
                                if ((short)parts[0] == 1) { ones.Add(parts[1]); }
                            }

                            var newWhere = WhereExpressionNode.CreateBinaryOperatorNode(BinaryOperator.OR);
                            newWhere.Nodes[0] = MakeULongEquals(info.PrimaryProperty.ColumnNames, new object[] { (short)0, null });
                            newWhere.Nodes[1] = MakeULongEquals(info.PrimaryProperty.ColumnNames, new object[] { (short)1, null });
                            newWhere.Nodes[0].Nodes[1] = WhereExpressionNode.CreateMemberIn(info.PrimaryProperty.ColumnNames[1], zeros);
                            newWhere.Nodes[1].Nodes[1] = WhereExpressionNode.CreateMemberIn(info.PrimaryProperty.ColumnNames[1], ones);
                            node.Copy(newWhere);
                        }
                    }
                }
            }
            return result;
        }

        private static void MakeULongGreaterThan(WhereExpressionNode where, string[] columns, object[] values)
        {
            where.Operator = BinaryOperator.OR;
            where.Nodes[0].Comparison = DataTypeComparison.GreaterThan;
            var equalsNode = WhereExpressionNode.CreateBinaryOperatorNode(BinaryOperator.AND, createNodes: true);
            where.Nodes[1] = equalsNode;
            equalsNode.Nodes[0].Comparison = DataTypeComparison.Equal;
            equalsNode.Nodes[0].Property = columns[0];
            equalsNode.Nodes[0].Value = values[0];
            equalsNode.Nodes[1].Comparison = DataTypeComparison.GreaterThan;
            equalsNode.Nodes[1].Property = columns[1];
            equalsNode.Nodes[1].Value = values[1];
        }

        private static void MakeULongGreaterThanOrEqualTo(WhereExpressionNode where, string[] columns, object[] values)
        {
            MakeULongGreaterThan(where, columns, values);
            where.Nodes[1].Nodes[1].Comparison = DataTypeComparison.GreaterThanOrEqualTo;
        }

        private static WhereExpressionNode MakeULongEquals(string[] columns, object[] values)
        {
            var result = WhereExpressionNode.CreateBinaryOperatorNode(BinaryOperator.AND, createNodes: true);
            result.Nodes[0].Property = columns[0];
            result.Nodes[0].Value = values[0];
            result.Nodes[1].Property = columns[1];
            result.Nodes[1].Value = values[1];
            return result;
        }

    }
}
