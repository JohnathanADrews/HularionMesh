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
using System.Linq;
using System.Text;
using System.Reflection;
using HularionCore.Pattern.Functional;
using System.Data;

namespace HularionMesh.Translator.SqlBase.ORM
{
    /// <summary>
    /// Contains the mapping information for a type to a table.
    /// </summary>
    public class MapInfo
    {
        /// <summary>
        /// The type being mapped.
        /// </summary>
        public Type Type { get; set; }

        private List<MemberMap> maps = new List<MemberMap>();

        private static Type columnAttributeType = typeof(ColumnAttribute);


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="repository">The SQL repository.</param>
        /// <param name="type">The map type.</param>
        public MapInfo(SqlMeshRepository repository, Type type)
        {
            Type = type;

            Dictionary<string, PropertyInfo> propertyColumns = new Dictionary<string, PropertyInfo>();
            foreach (var property in type.GetProperties())
            {
                var column = property.GetCustomAttributes<ColumnAttribute>().FirstOrDefault();
                if(column == null) { continue; }
                propertyColumns.Add(column.Column, property);
            }

            Dictionary<string, FieldInfo> fieldColumns = new Dictionary<string, FieldInfo>();
            foreach (var field in type.GetFields())
            {
                var column = field.GetCustomAttributes<ColumnAttribute>().FirstOrDefault();
                if (column == null) { continue; }
                fieldColumns.Add(column.Column, field);
            }

            foreach (var property in propertyColumns)
            {
                var columnTranslator = new SqlPropertyColumnTranslator(repository, property.Value.PropertyType, property.Key);
                maps.Add(new MemberMap()
                {
                    MemberName = property.Key,
                    Property = property.Value,
                    ColumnNames = columnTranslator.ColumnNames,
                    Assigner = new Action<object, object[]>((instance, values) =>
                    {
                        if (values == null) { return; }
                        foreach (var value in values) { if (value == null || value == DBNull.Value) { return; } }
                        property.Value.SetValue(instance, columnTranslator.SqlType.ToCSTransform.Transform(values));
                    })
                });
            }
            foreach (var field in fieldColumns)
            {
                var columnTranslator = new SqlPropertyColumnTranslator(repository, field.Value.FieldType, field.Key);
                maps.Add(new MemberMap()
                {
                    MemberName = field.Key,
                    Field = field.Value,
                    ColumnNames = columnTranslator.ColumnNames,
                    Assigner = new Action<object, object[]>((instance, values) =>
                    {
                        if (values == null) { return; }
                        foreach (var value in values) { if (value == null || value == DBNull.Value) { return; } }
                        field.Value.SetValue(instance, columnTranslator.SqlType.ToCSTransform.Transform(values));
                    })
                });
            }
        }

        /// <summary>
        /// Creates instances of the map type given values from the DataTable.
        /// </summary>
        /// <param name="table">The DataTable.</param>
        /// <returns>An instance of the map type for each row in table.</returns>
        public object[] FromDataTable(DataTable table)
        {
            var instances = new object[table.Rows.Count];
            for(var i = 0; i < table.Rows.Count; i++)
            {
                instances[i] = Activator.CreateInstance(Type);
            }
            foreach(var map in maps)
            {
                map.Assign(instances, table);
            }
            return instances;
        }


        private class MemberMap
        {
            public string MemberName { get; set; }
            public PropertyInfo Property { get; set; }
            public FieldInfo Field { get; set; }
            public string[] ColumnNames { get; set; }
            public Action<object, object[]> Assigner { get; set; }

            public void Assign(object[] instances, DataTable table)
            {
                var columns = new DataColumn[ColumnNames.Length];
                for (var i = 0; i < ColumnNames.Length; i++)
                {
                    columns[i] = table.Columns[ColumnNames[i]];
                    if(columns[i] == null) { return; }
                }
                for (var i = 0; i < instances.Length; i++)
                {
                    var row = table.Rows[i];
                    var values = new object[ColumnNames.Length];
                    for (var j = 0; j < ColumnNames.Length; j++)
                    {
                        values[j] = row.ItemArray[columns[j].Ordinal];
                    }
                    Assigner(instances[i], values);
                }
            }
        }

    }
}

