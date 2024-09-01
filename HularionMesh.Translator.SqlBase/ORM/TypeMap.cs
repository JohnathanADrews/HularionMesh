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
using HularionMesh.MeshType;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace  HularionMesh.Translator.SqlBase.ORM
{
    /// <summary>
    /// Maps rows in a DataTable to an array of values with a specified type.
    /// </summary>
    public class TypeMap
    {

        private Dictionary<Type, MapInfo> maps = new Dictionary<Type, MapInfo>();

        /// <summary>
        /// The repository.
        /// </summary>
        public SqlMeshRepository Repository { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="repository">The SQL mesh repository.</param>
        public TypeMap(SqlMeshRepository repository)
        {
            Repository = repository;
        }

        /// <summary>
        /// Generates a set of values given the data table.
        /// </summary>
        /// <typeparam name="T">The type of values to generate.</typeparam>
        /// <param name="table">The table containing the values.</param>
        /// <returns>A set of values given the data table.</returns>
        public T[] GetValues<T>(DataTable table)
        {
            return GetValues(typeof(T), table).Select(x => (T)x).ToArray();
        }

        /// <summary>
        /// Generates a set of values given the data table.
        /// </summary>
        /// <param name="table">The table containing the values.</param>
        /// <param name="type">The type of values to generate.<</param>
        /// <returns>A set of values given the data table.</returns>
        public object[] GetValues(Type type, DataTable table)
        {
            var map = CheckAddType(type);
            var result = map.FromDataTable(table);
            return result;
        }

        private MapInfo CheckAddType(Type type)
        {
            if (!maps.ContainsKey(type))
            {
                lock (maps)
                {
                    if (!maps.ContainsKey(type)) { maps.Add(type, new MapInfo(Repository, type)); }
                }
            }
            return maps[type];
        }

       

    }
}
