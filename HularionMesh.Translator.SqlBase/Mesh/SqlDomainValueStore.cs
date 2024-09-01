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
using HularionMesh.DomainValue;
using HularionMesh.MeshType;
using HularionMesh.Structure;
using  HularionMesh.Translator.SqlBase.SqlGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HularionCore.Pattern.Functional;

namespace  HularionMesh.Translator.SqlBase.Mesh
{

    /// <summary>
    /// Implements IDomainValueStore by storing data to a SQL database.
    /// </summary>
    public class SqlDomainValueStore : IDomainValueStore
    {
        /// <summary>
        /// The domain of the objects managed by this store.
        /// </summary>
        public MeshDomain Domain { get; private set; }

        /// <summary>
        /// The SqlMeshRepository.
        /// </summary>
        public SqlMeshRepository Repository { get; private set; }

        /// <summary>
        /// The mesh domain sql translator.
        /// </summary>
        public SqlDomainTranslator SqlDomain { get; private set; }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="connectionString">The connection details.</param>
        /// <param name="sqlDomain">The mesh domain sql translator.</param>
        public SqlDomainValueStore(SqlMeshRepository repository, SqlDomainTranslator sqlDomain)
        {
            Repository = repository;
            SqlDomain = sqlDomain;
            Domain = sqlDomain.Domain;
            Repository.CreateDomainOnce(SqlDomain);
        }

        /// <summary>
        /// Queries the values of this store.
        /// </summary>
        /// <param name="userKey">The key of the user making the request.</param>
        /// <param name="where">The root node of a where expression tree.</param>
        /// <param name="readRequest">The values to read in the query.</param>
        /// <returns>The objects matching the provided query parameters.</returns>
        public DomainObject[] QueryValues(IMeshKey userKey, WhereExpressionNode where, DomainReadRequest readRequest)
        {
            var select = new SqlMeshSelect(readRequest, Repository, SqlDomain);
            var sqlWhere = new SqlMeshWhere(SqlDomain, where, Repository);
            var query = new SqlMeshQuery(SqlDomain, select, sqlWhere, Repository);
            var result = Repository.QueryDomainObject(SqlDomain, query.Query, sqlWhere.ParameterCreator.Parameters);
            return result;
        }

        /// <summary>
        /// Queries the number of records matching where.
        /// </summary>
        /// <param name="userKey">The key of the user making the request.</param>
        /// <param name="where">The root node of a where expression tree.</param>
        /// <returns>The number of records matching where.</returns>
        public long QueryCount(IMeshKey userKey, WhereExpressionNode where)
        {
            var result = Repository.GetDomainWhereCount(SqlDomain, where);
            return result.Result;

        }

        /// <summary>
        /// Inserts the provided values to the store and sets the key on each object.
        /// </summary>
        /// <param name="userKey">The key of the user making the request.</param>
        /// <param name="values">The values to add.</param>
        public void InsertValues(IMeshKey userKey, params DomainObject[] values)
        {
            if (SqlDomain.Domain.IsGeneric) { Repository.PrepareDomainGenerics(SqlDomain.Domain, values); }
            foreach(var value in values)
            {
                if (!value.Meta.ContainsKey(MeshKeyword.Generics.Alias)) { value.Meta.Add(MeshKeyword.Generics.Alias, string.Empty); }
            }
            var insert = new SqlMeshInsert(userKey, SqlDomain, Repository, values);
            Repository.ExecuteMeshCommand(insert.Insert, insert.ParameterCreator.Parameters);
        }

        /// <summary>
        /// Updates the values specified by the updater and then reads the requested values back.
        /// </summary>
        /// <param name="userKey">The key of the user making the request.</param>
        /// <param name="updaters">Specifies which objects and which object members to update.</param>
        public void UpdateValues(IMeshKey userKey, params DomainObjectUpdater[] updaters)
        {
            var update = new SqlMeshUpdate(userKey, SqlDomain, Repository, updaters);
            Repository.ExecuteMeshCommand(update.Update, update.ParameterCreator.Parameters);
        }

        /// <summary>
        /// Deletes the specified objects.
        /// </summary>
        /// <param name="userKey">The key of the user making the request.</param>
        /// <param name="where">The root where node which details how to query the objects to delete.</param>
        public void DeleteValues(IMeshKey userKey, WhereExpressionNode where)
        {
            var deleteCommand = new SqlMeshDelete(SqlDomain, where, Repository);
            Repository.ExecuteMeshCommand(deleteCommand.Delete, deleteCommand.ParameterCreator.Parameters);
        }
    }
}
