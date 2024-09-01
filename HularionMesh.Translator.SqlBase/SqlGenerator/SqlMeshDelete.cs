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
using  HularionMesh.Translator.SqlBase.ORM;
using System;
using System.Collections.Generic;
using System.Text;

namespace  HularionMesh.Translator.SqlBase.SqlGenerator
{
    /// <summary>
    /// A SQL delete statement derived from a mesh request.
    /// </summary>
    public class SqlMeshDelete
    {
        /// <summary>
        /// The sql statement to delete the indicated records.
        /// </summary>
        public string Delete { get; private set; }

        /// <summary>
        /// The parameters in the where clause.
        /// </summary>
        public ParameterCreator ParameterCreator { get; private set; } 

        /// <summary>
        /// The domain translator.
        /// </summary>
        public SqlDomainTranslator SqlDomain { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sqlDomain">The domain translator.</param>
        /// <param name="where">Determines which values to delete.</param>
        /// <param name="repository">The SqlMeshRepository.</param>
        public SqlMeshDelete(SqlDomainTranslator sqlDomain, WhereExpressionNode where, SqlMeshRepository repository)
        {
            SqlDomain = sqlDomain;
            var sqlWhere = new SqlMeshWhere(SqlDomain, where, repository);
            ParameterCreator = sqlWhere.ParameterCreator;
            Delete = String.Format("delete from {0} where {1};", SqlDomain.TableName, sqlWhere.Where);
        }

    }
}
