#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionMesh.Translator.SqlBase.ORM;
using  HularionMesh.Translator.SqlBase.SqlGenerator;
using HularionMesh.Domain;
using HularionMesh.DomainValue;
using HularionMesh.MeshType;
using HularionMesh.SystemDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace  HularionMesh.Translator.SqlBase.Mechanic
{
    /// <summary>
    /// The details for implementing Set in a SQL repository.
    /// </summary>
    public class SetSqlMechanic : ISqlRepositoryMechanic
    {
        /// <summary>
        /// The SqlMeshRepository.
        /// </summary>
        public SqlMeshRepository Repository;

        /// <summary>
        /// Sets the SqlDomainTranslator when the domain is being created.
        /// </summary>
        public SqlDomainTranslator SqlDomainTranslator { get; set; }

        private ISqlType keyType;
        private ISqlType containerType;

        private string[] keyColumns;
        private string[] containerColumns;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="repository">The SqlMeshRepository.</param>
        public SetSqlMechanic(SqlMeshRepository repository)
        {
            Repository = repository;
            keyType = Repository.SqlRepository.SqlTypeProvider.Provide(DataType.MeshKey);
            containerType = Repository.SqlRepository.SqlTypeProvider.Provide(DataType.MeshKey);
            keyColumns = GetColumns(Repository.SqlRepository.SqlTypeProvider.Provide(DataType.MeshKey), MeshKeyword.Key.Alias);
            containerColumns = GetColumns(containerType, SqlMeshKeyword.SetContainerKey.Alias);
        }

        private string CreateSetDomainTableName(string suffix)
        {
            return Repository.SqlRepository.CreateTableName(String.Format("{0}&{1}", MeshSystemDomain.SetDomainKey, suffix));
        }

        /// <summary>
        /// Creates the domain using the given table specification. 
        /// </summary>
        public void CreateDomain(CreateTableSpecification table)
        {
            //var intType = Repository.SqlRepository.SqlTypeProvider.Provide(DataType.SignedInteger32);
            var stringType = Repository.SqlRepository.SqlTypeProvider.Provide(DataType.Text8);
            AddColumns(table, stringType, MeshKeyword.Key.Alias, isPrimaryKey: true);
            AddColumns(table, stringType, SqlMeshKeyword.SetContainerKey.Alias);
            AddColumns(table, stringType, SqlMeshKeyword.SetValueColumn.Alias);
            Repository.SqlRepository.CreateTable(table);

            var sqlTypes = Repository.SqlRepository.SqlTypesProvider.Provide();
            foreach (var type in sqlTypes)
            {
                table = new CreateTableSpecification() { Name = CreateSetDomainTableName(type.MeshDataType.Name) };
                AddColumns(table, stringType, SqlMeshKeyword.SetContainerKey.Alias);
                AddColumns(table, type, SqlMeshKeyword.SetValueColumn.Alias);
                Repository.SqlRepository.CreateTable(table);
            }
        }

        private void AddColumns(CreateTableSpecification table, ISqlType type, string baseName, bool isPrimaryKey = false)
        {
            var columns = GetColumns(type, baseName);
            for (var i = 0; i < type.SqlTypeCount; i++)
            {
                table.Columns.Add(new CreateColumnSpecification()
                {
                    Name = columns[i],
                    Type = type.SqlTypeNames[i],
                    IsPrimaryKey = isPrimaryKey
                });
            }
        }

        private string[] GetColumns(ISqlType sqlType, string baseName)
        {
            var columns = new string[sqlType.SqlTypeCount];
            for (var i = 0; i < sqlType.SqlTypeCount; i++)
            {
                columns[i] = Repository.SqlRepository.CreateColumnName(String.Format("{0}_{1}", baseName, i));
            }
            return columns;
        }

        private string[] GetReadColumns(ISqlType sqlType, string baseName)
        {
            var columns = new string[sqlType.SqlTypeCount];
            for (var i = 0; i < sqlType.SqlTypeCount; i++)
            {
                columns[i] = String.Format("{0}_{1}", baseName, i);
            }
            return columns;
        }


        /// <summary>
        /// Prepares the domain to handle the provided generics.
        /// </summary>
        /// <param name="meshRepository">The calling repository.</param>
        /// <param name="sqlRepository">The SQL implementation connector.</param>
        /// <param name="domain">The domain to prepare.</param>
        /// <param name="generics">The generics to prepare.</param>
        public void PrepareGenerics(SqlMeshRepository meshRepository, ISqlRepository sqlRepository, MeshDomain domain, MeshGeneric[] generics)
        {
        }

        /// <summary>
        /// Sets up the insert statement.
        /// </summary>
        /// <param name="insert">The insert statement to setup.</param>
        /// <param name="values">The values to insert.S</param>
        public void SetupInsert(SqlMeshInsert insert, DomainObject[] values)
        {
            var command = new StringBuilder();
            //Create a new domain object without the values to insert.
            var newValues = values.Select(x => x.Clone(includeValues: false)).ToArray();
            //Add the Set domain records. X (table1) -> Set (table2) -> Items (table3)
            insert.SetupInsert(newValues);
            //Add the Set item records.
            DataType dataType = null;
            ISqlType sqlType = null;
            string[] columns = null;
            foreach (var value in values)
            {
                if (dataType == null) 
                {
                    if (!value.Meta.ContainsKey(MeshKeyword.Generics.Alias)) { return; }
                    var generics = MeshGeneric.Deserialize((string)value.Meta[MeshKeyword.Generics.Alias]);
                    if (generics.Length != 1 || generics[0].Generics.Count > 0) { return; }
                    if (!DataType.TypeIsKnown(generics[0].Key)) { return; }
                    dataType = DataType.FromKey(generics[0].Key);
                    sqlType = Repository.SqlRepository.SqlTypeProvider.Provide(dataType);
                    columns = GetColumns(sqlType, SqlMeshKeyword.SetValueColumn.Alias);
                }
                var items = UniqueSet.GetDomainObjectValues(value);
                if (items.Count() == 0) { continue; }
                foreach (var item in items)
                {
                    var objectValues = sqlType.ToSqlTransform.Transform(item);
                    command.Append("insert into ");
                    command.Append(CreateSetDomainTableName(dataType.Name));
                    command.Append(" (");
                    for(var i = 0;i< containerColumns.Length; i++)
                    {
                        if (i > 0) { containerColumns.Append(", "); }
                        command.Append(containerColumns[i]);
                    }
                    for (var i = 0; i < sqlType.SqlTypeCount; i++)
                    {
                        command.Append(", ");
                        command.Append(columns[i]);
                    }
                    command.Append(") values (");
                    var containerValues = containerType.ToSqlTransform.Transform(value.Key);
                    for(var i=0;i< containerType.SqlTypeCount; i++)
                    {
                        if (i > 0) { command.Append(", "); }
                        command.Append(insert.ParameterCreator.Create(containerValues[i]).Name);
                    }
                    for (var i = 0; i < sqlType.SqlTypeCount; i++)
                    {
                        command.Append(", ");
                        command.Append(insert.ParameterCreator.Create(objectValues[i]).Name);
                    }
                    command.Append(");\n");
                }
            }
            insert.Insert = String.Format("{0}\n{1}", insert.Insert, command.ToString());        
        }

        private void AddItemInserts(StringBuilder command, ParameterCreator parameterCreator, ISqlType sqlType, string[] columns, SqlMeshParameter[] keyParameters, IList<object> items)
        {
            if (items.Count() == 0) { return; }
            foreach (var item in items)
            {
                var objectValues = sqlType.ToSqlTransform.Transform(item);
                command.Append("insert into ");
                command.Append(CreateSetDomainTableName(sqlType.MeshDataType.Name));
                command.Append(" (");
                for (var i = 0; i < containerColumns.Length; i++)
                {
                    if (i > 0) { containerColumns.Append(", "); }
                    command.Append(containerColumns[i]);
                }
                for (var i = 0; i < sqlType.SqlTypeCount; i++)
                {
                    command.Append(", ");
                    command.Append(columns[i]);
                }
                command.Append(") values (");
                //var containerValues = containerType.ToSqlTransform.Transform(containerKey);
                for (var i = 0; i < containerType.SqlTypeCount; i++)
                {
                    if (i > 0) { command.Append(", "); }
                    command.Append(keyParameters[i].Name);
                }
                for (var i = 0; i < sqlType.SqlTypeCount; i++)
                {
                    command.Append(", ");
                    command.Append(parameterCreator.Create(objectValues[i]).Name);
                }
                command.Append(");\n");
            }
        }

        /// <summary>
        /// Sets up the update statement.
        /// </summary>
        /// <param name="update">The update statement.</param>
        /// <param name="updaters">The update values.</param>
        public MechanicUpdateOption SetupUpdate(SqlMeshUpdate update, DomainObjectUpdater[] updaters)
        {
            var command = new StringBuilder();

            foreach (var updater in updaters)
            {
                if (!DataType.TypeIsKnown(updater.Generics[0].Key)) { continue; }
                var dataType = DataType.FromKey(updater.Generics[0].Key);
                var sqlType = Repository.SqlRepository.SqlTypeProvider.Provide(dataType);
                var typeColumns = GetColumns(sqlType, SqlMeshKeyword.SetValueColumn.Alias);

                var where = new SqlMeshWhere(SqlDomainTranslator, updater.Where, Repository);
                var select = new SqlMeshSelect(DomainReadRequest.ReadKeys, Repository, SqlDomainTranslator);
                var query = new SqlMeshQuery(SqlDomainTranslator, select , where, Repository);
                var keys = Repository.QueryDomainObject(SqlDomainTranslator, query.Query, query.ParameterCreator.Parameters).Select(x=>x.Key).ToList();


                var items = UniqueSet.GetUpdaterValues(updater);

                command.Append("\n");
                foreach (var key in keys)
                {
                    command.Append(String.Format("delete from {0} where ", CreateSetDomainTableName(dataType.Name)));
                    var keyValues = keyType.ToSqlTransform.Transform(key);
                    var keyParameters = new SqlMeshParameter[keyType.SqlTypeCount];
                    for (var i = 0; i < containerType.SqlTypeCount; i++)
                    {
                        if(i > 0) { command.Append(" and "); }
                        command.Append(containerColumns[i]);
                        command.Append(" = ");
                        keyParameters[i] = update.ParameterCreator.Create(keyValues[i]);
                        command.Append(keyParameters[i].Name);
                    }
                    command.Append(";\n");
                    AddItemInserts(command, update.ParameterCreator, sqlType, typeColumns, keyParameters, items);
                }
                command.Append("\n");

            }
            update.Update = command.ToString();
            return MechanicUpdateOption.UpdateMeta;
        }

        /// <summary>
        /// Further manifests the result of a query.
        /// </summary>
        /// <param name="objects">The objects to further manifest.</param>
        public void ManifestQueryResult(IEnumerable<DomainObject> objects)
        {
            objects = objects.Where(x => x.Meta.ContainsKey(MeshKeyword.Generics.Alias) && x.Meta[MeshKeyword.Generics.Alias] != null).ToList();

            foreach (var value in objects)
            {
                var generic = value.GetGenerics().FirstOrDefault();
                if (!DataType.TypeIsKnown(generic.Key)) { continue; }
                var dataType = DataType.FromKey(generic.Key);

                var sqlType = Repository.SqlRepository.SqlTypeProvider.Provide(dataType);
                var columns = GetColumns(sqlType, SqlMeshKeyword.SetValueColumn.Alias);
                var parameterCreator = new ParameterCreator();
                var query = new StringBuilder();
                query.Append(" select ");
                for(var i = 0; i < sqlType.SqlTypeCount; i++)
                {
                    if (i > 0) { query.Append(", "); }
                    query.Append(columns[i]);
                }
                query.Append(" from ");
                query.Append(CreateSetDomainTableName(dataType.Name));
                query.Append(" where ");
                var containerValues = containerType.ToSqlTransform.Transform(value.Key);
                for(var i = 0; i < containerType.SqlTypeCount; i++)
                {
                    if (i > 0) { query.Append(" and "); }
                    query.Append(containerColumns[i]);
                    query.Append(" = ");
                    query.Append(parameterCreator.Create(containerValues[i]).Name);
                }

                var itemsQuery = query.ToString();
                var table = Repository.SqlRepository.ExecuteQuery(itemsQuery, parameterCreator.Parameters);
                var tableColumns = GetReadColumns(sqlType, SqlMeshKeyword.SetValueColumn.Alias);
                var values = new object[sqlType.SqlTypeCount];
                var indices = new int[sqlType.SqlTypeCount];
                for(var i=0;i< sqlType.SqlTypeCount;i++)
                {
                    indices[i] = table.Columns[tableColumns[i]].Ordinal;
                }
                var items = new List<object>();
                foreach(DataRow dataRow in table.Rows)
                {
                    for(var i = 0; i < tableColumns.Length; i++)
                    {
                        values[i] = dataRow.ItemArray[indices[i]];
                    }
                    var itemValue = sqlType.ToCSTransform.Transform(values);
                    items.Add(itemValue);
                }

                value.Values[UniqueSet.UniqueSetItemsDomainValue] = UniqueSet.MakeGenericList(dataType.CSharpType, items);
            }

        }


    }
}
