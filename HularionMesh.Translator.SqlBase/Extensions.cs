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
using HularionMesh;
using HularionMesh.Domain;
using HularionMesh.DomainValue;
using HularionMesh.MeshType;
using HularionMesh.SystemDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace  HularionMesh.Translator.SqlBase
{
    /// <summary>
    /// Extensions on Mesh objects.
    /// </summary>
    public static class Extensions
    {

        /// <summary>
        /// Creates a table name using the ObjectNameCreator given to the repository.
        /// </summary>
        /// <param name="repository">The repository that is extended.</param>
        /// <param name="name">The base table name.</param>
        /// <returns>The name of the table formatted for the appropriate data source.</returns>
        public static string CreateTableName(this ISqlRepository repository, object name)
        {
            return repository.ObjectNameCreator.Create(new SqlObject() { Name = name.ToString(), ObjectType = SqlObjectType.Table });
        }

        /// <summary>
        /// Creates a column name using the ObjectNameCreator given to the repository.
        /// </summary>
        /// <param name="repository">The repository that is extended.</param>
        /// <param name="name">The base column name.</param>
        /// <returns>The name of the column formatted for the appropriate data source.</returns>
        public static string CreateColumnName(this ISqlRepository repository, object name)
        {
            return repository.ObjectNameCreator.Create(new SqlObject() { Name = name.ToString(), ObjectType = SqlObjectType.Column });
        }


    }
}
