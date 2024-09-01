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
using System.Linq;
using System.Reflection;
using System.Text;

namespace  HularionMesh.Translator.SqlBase.ORM
{
    /// <summary>
    /// Contains table mapping information for a member of a type.
    /// </summary>
    public class TypeMemberInfo
    {
        /// <summary>
        /// The name of the member.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The member that is being mapped.
        /// </summary>
        public PropertyInfo Property { get; private set; }

        /// <summary>
        /// The member that is being mapped.
        /// </summary>
        public FieldInfo Field { get; private set; }

        /// <summary>
        /// True iff this member represents a primary key column.
        /// </summary>
        public bool IsPrimaryKey { get; private set; }

        /// <summary>
        /// The kind of member being mapped.
        /// </summary>
        public MemberType MapType { get; private set; }

        /// <summary>
        /// The type of the member.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// The SQL type.
        /// </summary>
        public ISqlType SqlType { get; private set; }

        /// <summary>
        /// Contains the details for creating the column.
        /// </summary>
        public List<CreateColumnSpecification> CreateColumnSpecifications { get; private set; } = new List<CreateColumnSpecification>();

        public ColumnAttribute Column { get; private set; }

        private static Type columnAttribute = typeof(ColumnAttribute);
        private static Type primaryKeyAttribute = typeof(PrimaryKeyAttribute);

        /// <summary>
        /// The SqlMeshRepository.
        /// </summary>
        public SqlMeshRepository Repository { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="property">The property to map to a column.</param>
        public TypeMemberInfo(PropertyInfo property, SqlMeshRepository repository)
        {
            Repository = repository;
            Property = property;
            Name = property.Name;
            Type = property.PropertyType;
            MapType = MemberType.Property;
            Column = (ColumnAttribute)property.GetCustomAttributes().Where(x => x.GetType() == columnAttribute).FirstOrDefault();
            if (Column == null) { return; }
            //ColumnName = Repository.SqlRepository.ObjectNameCreator.Create(new SqlObject() { Name = column.Column, ObjectType = SqlObjectType.Column });
            IsPrimaryKey = (property.GetCustomAttributes().Where(x => x.GetType() == primaryKeyAttribute).Count() > 0);
            Initialize();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="field">The field to map to a column.</param>
        public TypeMemberInfo(FieldInfo field, SqlMeshRepository repository)
        {
            Repository = repository;
            Field = field;
            Name = field.Name;
            Type = field.FieldType;
            MapType = MemberType.Field;
            Column = (ColumnAttribute)field.GetCustomAttributes().Where(x => x.GetType() == columnAttribute).FirstOrDefault();
            if (Column == null) { return; }
            //ColumnName = Repository.SqlRepository.ObjectNameCreator.Create(new SqlObject() { Name = column.Column, ObjectType = SqlObjectType.Column });
            IsPrimaryKey = (field.GetCustomAttributes().Where(x => x.GetType() == primaryKeyAttribute).Count() > 0);
            Initialize();
        }

        /// <summary>
        /// Gets the value of this member within the container object.
        /// </summary>
        /// <param name="container">The object that has this property.</param>
        /// <returns>The value of this member within the container.</returns>
        public object[] GetValues(object container)
        {
            switch (MapType)
            {
                case MemberType.Field:
                    return SqlType.ToSqlTransform.Transform(Field.GetValue(container));
                case MemberType.Property:
                    return SqlType.ToSqlTransform.Transform(Property.GetValue(container));
            }
            return null;
        }

        private void Initialize()
        {
            SqlType = Repository.SqlRepository.SqlTypeProvider.Provide(DataType.FromCSharpType(this.Type));

            var sqlColumn = new SqlPropertyColumnTranslator(Repository, Type, Column.Column);

            for(var i = 0; i < SqlType.SqlTypeCount; i++)
            {
                var column = new CreateColumnSpecification();
                column.Name = Repository.SqlRepository.CreateColumnName(sqlColumn.ColumnNames[i]);
                column.IsPrimaryKey = IsPrimaryKey;
                if (IsPrimaryKey) { column.Type = SqlType.SqlPrimaryKeyTypeNames[i]; }
                else { column.Type = SqlType.SqlTypeNames[i]; }
                CreateColumnSpecifications.Add(column);
            }
        }

        /// <summary>
        /// Gets the SQL where clause for this member.
        /// </summary>
        /// <returns>The SQL where clause for this member.</returns>
        public string GetWhereString(string[] values)
        {
            var result = new StringBuilder();
            for(var i = 0; i < SqlType.SqlTypeCount; i++)
            {
                if (i > 0) { result.Append(" and "); }
                result.Append(CreateColumnSpecifications[i].Name);
                result.Append(" = ");
                result.Append(values[i]);
            }
            return result.ToString();
        }

        /// <summary>
        /// Determines whether the specified property is mapped to a column.
        /// </summary>
        /// <param name="property">The property to check.</param>
        /// <returns>true iff the specified property is mapped to a column.</returns>
        public static bool PropertyIsAColumn(PropertyInfo property)
        {
            return (property.GetCustomAttributes().Where(x => x.GetType() == columnAttribute).Count() > 0);
        }

        /// <summary>
        /// Determines whether the specified field is mapped to a column.
        /// </summary>
        /// <param name="field">The field to check.</param>
        /// <returns>true iff the specified field is mapped to a column.</returns>
        public static bool PropertyIsAColumn(FieldInfo field)
        {
            return (field.GetCustomAttributes().Where(x => x.GetType() == columnAttribute).Count() > 0);
        }

        /// <summary>
        /// The kind of member being mapped.
        /// </summary>
        public enum MemberType
        {
            Property,
            Field
        }

    }
}
