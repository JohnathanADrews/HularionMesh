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
using HularionMesh.Translator.SqlBase.ORM;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace  HularionMesh.Translator.SqlBase
{
    /// <summary>
    /// The property information for a domain property in Sql.
    /// </summary>
    public class SqlDomainPropertyTranslator
    {
        /// <summary>
        /// The type of the property.
        /// </summary>
        public SqlPropertyCategory Type { get; set; }

        /// <summary>
        /// The property of the domain.
        /// </summary>
        public ValueProperty MeshProperty { get; set; }

        /// <summary>
        /// The property information of the corresponding type.
        /// </summary>
        public PropertyInfo TypeProperty { get; set; }

        /// <summary>
        /// The name of the columns storing the property values.
        /// </summary>
        public string[] ColumnNames { get; set; }

        /// <summary>
        /// The name of the columns with the prefixes. Must match a DataTable column.
        /// </summary>
        public string[] PrefixedColumnNames { get; set; }

        /// <summary>
        /// The column translator for this property.
        /// </summary>
        public SqlPropertyColumnTranslator ColumnTranslator { get; private set; }

        /// <summary>
        /// The SQL type converter.
        /// </summary>
        public ISqlType SqlType { get; private set; }

        /// <summary>
        /// Provides the sql type converter given a Mesh data type.
        /// </summary>
        public IParameterizedProvider<DataType, ISqlType> PropertyTypeProvider { get; set; }

        /// <summary>
        /// The SqlMeshRepository.
        /// </summary>
        public SqlMeshRepository Repository { get; private set; }

        /// <summary>
        /// A key to uniquely identify the translator.
        /// </summary>
        public ObjectKey Key = new ObjectKey();

        private Action<DomainObject, object[]> assigner;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="repository">The SqlMeshRepository.</param>
        /// <param name="propertyType">The proeprty to translate.</param>
        /// <param name="meshProperty">The mesh domain property.</param>
        public SqlDomainPropertyTranslator(SqlMeshRepository repository, SqlPropertyCategory propertyType, ValueProperty meshProperty)
        {
            Repository = repository;
            Type = propertyType;
            SqlType = repository.SqlRepository.SqlTypeProvider.Provide(DataType.FromKey(meshProperty.Type));

            MeshProperty = meshProperty;
            var prefixedNames = new List<string>();
            var columnNames = new List<string>();
            if (propertyType == SqlPropertyCategory.Key)
            {
                ColumnTranslator = new SqlPropertyColumnTranslator(repository, SqlType, MeshKeyword.Key.Alias);
                for (var i = 0; i < SqlType.SqlTypeCount; i++)
                {
                    prefixedNames.Add(ColumnTranslator.ColumnNames[i]);
                    var columnName = Repository.SqlRepository.CreateColumnName(ColumnTranslator.ColumnNames[i]);
                    columnNames.Add(columnName);
                    Key.SetPart(i.ToString(), columnNames[i]);
                }
            }
            if (propertyType == SqlPropertyCategory.Meta)
            {
                ColumnTranslator = new SqlPropertyColumnTranslator(repository, SqlType, meshProperty.Name);
                for (var i = 0; i < SqlType.SqlTypeCount; i++)
                {
                    var prefixedName = String.Format("{0}{1}", SqlMeshKeyword.MetaPrefix.Alias, ColumnTranslator.ColumnNames[i]);
                    prefixedNames.Add(prefixedName);
                    var columnName = Repository.SqlRepository.CreateColumnName(prefixedName);
                    columnNames.Add(columnName);
                    Key.SetPart(i.ToString(), columnName);
                }
            }
            if (propertyType == SqlPropertyCategory.Value)
            {
                //non-generics only
                ColumnTranslator = new SqlPropertyColumnTranslator(repository, SqlType, meshProperty.Name);
                for (var i = 0; i < SqlType.SqlTypeCount; i++)
                {
                    var prefixedName = String.Format("{0}{1}", SqlMeshKeyword.ValuePrefix.Alias, ColumnTranslator.ColumnNames[i]);
                    prefixedNames.Add(prefixedName);
                    var columnName = Repository.SqlRepository.CreateColumnName(prefixedName);
                    columnNames.Add(columnName);
                    Key.SetPart(i.ToString(), columnName);
                }
            }
            PrefixedColumnNames = prefixedNames.ToArray();
            ColumnNames = columnNames.ToArray();


            switch (Type)
            {
                case SqlPropertyCategory.Key:
                    {
                        assigner = (domainObject, values) =>
                        {
                            domainObject.Key = MeshKey.Parse(SqlType.ToCSTransform.Transform(values));
                        };
                    }
                    break;
                case SqlPropertyCategory.Meta:
                    {
                        assigner = (domainObject, values) =>
                        {
                            if (domainObject.Meta.ContainsKey(MeshProperty.Name)) { domainObject.Meta.Remove(MeshProperty.Name); }
                            domainObject.Meta.Add(MeshProperty.Name, SqlType.ToCSTransform.Transform(values));
                        };
                        break;
                    }
                case SqlPropertyCategory.Value:
                    {
                        assigner = (domainObject, values) =>
                        {
                            if (domainObject.Values.ContainsKey(MeshProperty.Name)) { domainObject.Values.Remove(MeshProperty.Name); }
                            domainObject.Values.Add(MeshProperty.Name, SqlType.ToCSTransform.Transform(values));
                        };
                        break;
                    }
            }

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="repository">The SqlMeshRepository.</param>
        /// <param name="meshProperty">The mesh domain property.</param>
        /// <param name="generic">The geric for a generic-typed property.</param>
        public SqlDomainPropertyTranslator(SqlMeshRepository repository, ValueProperty meshProperty, MeshGeneric generic)
        {
            Repository = repository;
            Type =  SqlPropertyCategory.Value;
            SqlType = repository.SqlRepository.SqlTypeProvider.Provide(DataType.FromKey(generic.Key));

            MeshProperty = meshProperty;
            var prefixedNames = new List<string>();
            var columnNames = new List<string>();

            for (var i = 0; i < SqlType.SqlTypeCount; i++)
            {
                var prefixedName = String.Format("{0}{1}_{2}_{3}", SqlMeshKeyword.GenericPrefix.Alias, meshProperty.Name, SqlType.SqlTypeNames[i], i);
                prefixedNames.Add(prefixedName);
                var columnName = Repository.SqlRepository.CreateColumnName(prefixedName);
                columnNames.Add(columnName);
                Key.SetPart(i.ToString(), columnName);
            }

            PrefixedColumnNames = prefixedNames.ToArray();
            ColumnNames = columnNames.ToArray();

            assigner = (domainObject, values) =>
            {
                if (domainObject.Values.ContainsKey(MeshProperty.Name)) { domainObject.Values.Remove(MeshProperty.Name); }
                domainObject.Values.Add(MeshProperty.Name, SqlType.ToCSTransform.Transform(values));
            };

        }

        /// <summary>
        /// Assigns the value to the domain object.
        /// </summary>
        /// <param name="domainObject">The domain object to which the value will be assigned.</param>
        /// <param name="values">The values to assign.</param>
        public void AssignValue(DomainObject domainObject, object[] values)
        {
            if (values == null) { return; }
            foreach(var value in values)
            {
                if (value == null || value == DBNull.Value) { return; }
            }
            assigner(domainObject, values);
        }

        /// <summary>
        /// Gets a comma-delimited string containing the column names for a select statement.
        /// </summary>
        /// <returns>A comma-delimited string containing the column names for a select statement.</returns>
        public string GetSelectString()
        {
            var result = new StringBuilder();
            for (var i = 0; i < ColumnNames.Length; i++)
            {
                if (i > 0) { result.Append(", "); }
                result.Append(ColumnNames[i]);
            }
            return result.ToString();
        }

        /// <summary>
        /// Gets a comma-delimited string containing the column names for a select statement.
        /// </summary>
        /// <param name="aliases">The aliases to which the column values are assigned.</param>
        /// <returns>A comma-delimited string containing the column names for a select statement.</returns>
        public string GetSelectString(string[] aliases)
        {
            var result = new StringBuilder();
            for (var i = 0; i < ColumnNames.Length; i++)
            {
                if (i > 0) { result.Append(", "); }
                result.Append(ColumnNames[i]);
                result.Append(" as ");
                result.Append(Repository.SqlRepository.CreateColumnName(aliases[i]));
            }
            return result.ToString();
        }

        /// <summary>
        /// Gets the string for the start of an insert statement.
        /// </summary>
        /// <returns>The string for the start of an insert statement.</returns>
        public string GetStartInsertString()
        {
            var result = new StringBuilder();
            for (var i = 0; i < ColumnNames.Length; i++)
            {
                if (i > 0) { result.Append(", "); }
                result.Append(ColumnNames[i]);
            }
            return result.ToString();
        }

        /// <summary>
        /// Gets the string for the value part of an insert statement.
        /// </summary>
        /// <param name="parameterCreator">Creates and maintains the parameters.</param>
        /// <param name="value">The value it insert.</param>
        /// <returns>The string for the value part of an insert statement.</returns>
        public string GetValueInsertString(ParameterCreator parameterCreator, object value)
        {
            var result = new StringBuilder();
            var values = SqlType.ToSqlTransform.Transform(value);
            for (var i = 0; i < ColumnNames.Length; i++)
            {
                if (i > 0) { result.Append(", "); }
                var parameter = parameterCreator.Create(values[i]);
                result.Append(parameter.Name);
            }
            return result.ToString();
        }

        /// <summary>
        /// Creates the columns for creating the domain property.
        /// </summary>
        /// <returns>The columns for creating the domain property.</returns>
        public CreateColumnSpecification[] GetCreateColumnSpecifications()
        {
            var result = new List<CreateColumnSpecification>();
            for(var i = 0; i < SqlType.SqlTypeCount; i++)
            {
                result.Add(new CreateColumnSpecification() { IsPrimaryKey = (MeshProperty == MeshDomain.ObjectMeshKey), Name = ColumnNames[i], Type = SqlType.SqlTypeNames[i] });
            }
            return result.ToArray();
        }


    }
}
