#region License
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
using  HularionMesh.Translator.SqlBase.ORM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using HularionCore.Pattern.Functional;

namespace  HularionMesh.Translator.SqlBase
{
    /// <summary>
    /// An interface with the necessary SQL functionality to implement a mesh connector.
    /// </summary>
    public interface ISqlRepository
    {

        /// <summary>
        /// Creates the table using the provided table specification.
        /// </summary>
        /// <param name="table">The specification to create the table.</param>
        void CreateTable(CreateTableSpecification table);

        /// <summary>
        /// Adds the columns to the specified table.
        /// </summary>
        /// <param name="tableName">The name of the table to which to add the columns./param>
        /// <param name="columns">The columns to add.</param>
        void AddTableColumns(string tableName, CreateColumnSpecification[] columns);

        /// <summary>
        /// Executes the provided command.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="parameters">The parameters to use in the command.</param>
        void ExecuteCommand(string command, IEnumerable<SqlMeshParameter> parameters = null);

        /// <summary>
        /// Executes the query using the provided parameters and returns a DataTable with the result.
        /// </summary>
        /// <param name="query">The query to retrieve the values.</param>
        /// <param name="parameters">The parameters to the query.</param>
        /// <returns>A DataTable with the result of the query.</returns>
        DataTable ExecuteQuery(string query, IEnumerable<SqlMeshParameter> parameters = null);

        /// <summary>
        /// Provides an ISqlType given the mesh data type.
        /// </summary>
        IParameterizedProvider<DataType, ISqlType> SqlTypeProvider { get; }

        /// <summary>
        /// Provides all the database system types.
        /// </summary>
        IProvider<IEnumerable<ISqlType>> SqlTypesProvider { get; }

        /// <summary>
        /// Creates domain object keys given the provided domain.
        /// </summary>
        /// <remarks>StandardDomainForm.DomainValueKeyCreator  is the standard creator.</remarks>
        IParameterizedCreator<MeshDomain, IMeshKey> DomainValueKeyCreator { get; }

        /// <summary>
        /// Provides the link form specification for linked domains.
        /// </summary>
        /// <remarks>StandardLinkForm.LinkKeyFormProvider is the standard specification.</remarks>
        IParameterizedProvider<LinkedDomains, DomainLinkForm> LinkKeyFormProvider { get; }

        /// <summary>
        /// Creates a language-specific name given the SqlObject. e.g. Adds [ and ] to the ends of the name.
        /// </summary>
        IParameterizedCreator<SqlObject, string> ObjectNameCreator { get; }

        /// <summary>
        /// Transforms a WhereExpressionNode to conform to an implemented connector.
        /// </summary>
        ITransform<WhereTransformRequest, WhereTransformResult> WhereTransformer { get; }

    }
}
