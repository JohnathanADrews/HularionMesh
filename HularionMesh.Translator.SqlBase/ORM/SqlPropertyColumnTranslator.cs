#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionMesh.MeshType;
using System;
using System.Collections.Generic;
using System.Text;

namespace HularionMesh.Translator.SqlBase.ORM
{
    /// <summary>
    /// Contains translation information for a Mesh property to SQL column translation.
    /// </summary>
    public class SqlPropertyColumnTranslator
    {
        /// <summary>
        /// The SQL to Mesh DataType translator.
        /// </summary>
        public ISqlType SqlType { get; private set; }

        /// <summary>
        /// The names of the SQL columns generated for the property.
        /// </summary>
        public string[] ColumnNames { get; private set; }

        /// <summary>
        /// The SqlMeshRepository.
        /// </summary>
        public SqlMeshRepository Repository { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="repository">The SqlMeshRepository.</param>
        /// <param name="type">The type being translated.</param>
        /// <param name="propertyName">The name of the Mesh proeprty.</param>
        public SqlPropertyColumnTranslator(SqlMeshRepository repository, Type type, string propertyName)
        {
            Repository = repository;
            var dataType = DataType.FromCSharpType(type);
            SqlType = repository.SqlRepository.SqlTypeProvider.Provide(dataType);

            ColumnNames = new string[SqlType.SqlTypeCount];
            for (var i = 0; i < SqlType.SqlTypeCount; i++)
            {
                ColumnNames[i] = String.Format("{0}_{1}", propertyName, i);
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="repository">The SqlMeshRepository.</param>
        /// <param name="sqlType">The SQL to Mesh DataType translator.</param>
        /// <param name="propertyName">The name of the Mesh proeprty.</param>
        public SqlPropertyColumnTranslator(SqlMeshRepository repository, ISqlType sqlType, string propertyName)
        {
            Repository = repository;
            SqlType = sqlType;

            ColumnNames = new string[SqlType.SqlTypeCount];
            for (var i = 0; i < SqlType.SqlTypeCount; i++)
            {
                ColumnNames[i] = String.Format("{0}_{1}", propertyName, i);
            }
        }


    }
}
