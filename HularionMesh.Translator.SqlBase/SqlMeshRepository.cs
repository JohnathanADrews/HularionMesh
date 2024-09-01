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
using HularionMesh.SystemDomain;
using  HularionMesh.Translator.SqlBase.Model;
using  HularionMesh.Translator.SqlBase.ORM;
using  HularionMesh.Translator.SqlBase.SqlGenerator;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using HularionCore.Pattern.Functional;
using  HularionMesh.Translator.SqlBase.Mechanic;
using System.Threading;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using  HularionMesh.Translator.SqlBase.Result;
using HularionMesh.Repository;

namespace  HularionMesh.Translator.SqlBase
{
    /// <summary>
    /// A repository used to map objects in Mesh-to-SQL.
    /// </summary>
    public class SqlMeshRepository
    {
        /// <summary>
        /// The mesh connector.
        /// </summary>
        public ISqlRepository SqlRepository;

        /// <summary>
        /// Provides the mechanic for the given domain key or null if there is no mechanic.
        /// </summary>
        public IParameterizedProvider<IMeshKey, ISqlRepositoryMechanic> MechanicProvider { get; private set; }

        /// <summary>
        /// Provides the table name for the given domain.
        /// </summary>
        public IParameterizedProvider<MeshDomain, string> DomainTableNameProvider { get; private set; }

        /// <summary>
        /// Provides a SQL domain given a mesh domain.
        /// </summary>
        public IParameterizedProvider<MeshDomain, SqlDomainTranslator> SqlDomainProvider { get; private set; }

        private TypeMap typeMap;

        private Dictionary<Type, TypeTableInfo> typeInfos;

        private HashSet<MeshDomain> addDomains = new HashSet<MeshDomain>();

        private Dictionary<string, DomainGenericInfo> preparedGenerics = new Dictionary<string, DomainGenericInfo>();

        private Dictionary<IMeshKey, DomainGenericInfo> domainGenerics = new Dictionary<IMeshKey, DomainGenericInfo>();

        private Dictionary<MeshDomain, SqlDomainTranslator> sqlDomains = new Dictionary<MeshDomain, SqlDomainTranslator>();

        private const string domainValuedGeneric = "_domain_Valued_Generic_";

        private static Type domainLinkType = typeof(MeshDomainLink);

        private List<CreateColumnSpecification> keyColumnSpecifications = new List<CreateColumnSpecification>();
        private Dictionary<string, List<CreateColumnSpecification>> metaColumnSpecifications = new Dictionary<string, List<CreateColumnSpecification>>();
        private Dictionary<MeshDomain, Dictionary<string, List<CreateColumnSpecification>>> domainColumnSpecifications = new Dictionary<MeshDomain, Dictionary<string, List<CreateColumnSpecification>>>();

        private Dictionary<Type, TypeTableInfo> modelInfos = new Dictionary<Type, TypeTableInfo>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public SqlMeshRepository(ISqlRepository sqlRepository)
        {
            SqlRepository = sqlRepository;
            DomainTableNameProvider = ParameterizedProvider.FromSingle<MeshDomain, string>(domain =>
            {
                if(domain.GetType() != domainLinkType) { return SqlRepository.CreateTableName(domain.Key.Serialized); }
                var suffix = Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(domain.Key.Serialized)));
                var linkDomain = (MeshDomainLink)domain;
                var form = sqlRepository.LinkKeyFormProvider.Provide(linkDomain.GetLinkedDomains());
                var sDomain = linkDomain.GetSTypeDomain(form);
                var tDomain = linkDomain.GetTTypeDomain(form);
                var maxLength = 6;
                var tableName = String.Format("({0}.{1}){2}", sDomain.UniqueName.Length > maxLength ? sDomain.UniqueName.Substring(0, maxLength) : sDomain.UniqueName,
                    tDomain.UniqueName.Length > maxLength ? tDomain.UniqueName.Substring(0, maxLength) : tDomain.UniqueName, suffix);
                tableName = SqlRepository.CreateTableName(tableName);
                return tableName;
            });

            var keyTranslator = new SqlPropertyColumnTranslator(this, sqlRepository.SqlTypeProvider.Provide(DataType.MeshKey), MeshKeyword.Key.Alias);
            for(var i=0;i< keyTranslator.SqlType.SqlTypeCount; i++)
            {
                keyColumnSpecifications.Add(new CreateColumnSpecification() { IsPrimaryKey = true, Name = sqlRepository.CreateColumnName(keyTranslator.ColumnNames[i]), Type = keyTranslator.SqlType.SqlPrimaryKeyTypeNames[i] });
            }
            foreach(var meta in MeshDomain.MetaProperties)
            {
                metaColumnSpecifications.Add(meta.Name, new List<CreateColumnSpecification>());
                var metaTranslator = new SqlPropertyColumnTranslator(this, sqlRepository.SqlTypeProvider.Provide(DataType.FromKey( meta.Type)), meta.Name);
                for (var i = 0; i < metaTranslator.SqlType.SqlTypeCount; i++)
                {
                    metaColumnSpecifications[meta.Name].Add(new CreateColumnSpecification() 
                    { 
                        IsPrimaryKey = false, 
                        Name = sqlRepository.CreateColumnName(String.Format("{0}{1}", SqlMeshKeyword.MetaPrefix.Alias, metaTranslator.ColumnNames[i])), 
                        Type = metaTranslator.SqlType.SqlTypeNames[i] 
                    });
                }
            }

            typeMap = new TypeMap(this);
            typeInfos = Assembly.GetExecutingAssembly().GetTypes().Where(x => TypeTableInfo.TypeIsATable(x)).ToDictionary(x => x, x => new TypeTableInfo(x, this));
            foreach (var info in typeInfos.Values)
            {
                sqlRepository.CreateTable(info.CreateTableSpecification);
            }
            var mechanics = new Dictionary<IMeshKey, ISqlRepositoryMechanic>();
            mechanics.Add(MeshSystemDomain.SetDomainKey, new SetSqlMechanic(this));
            //mechanics.Add(MeshSystemDomain.MapDomainKey, new MapSqlMechanic());
            MechanicProvider = mechanics.MakeParameterizedProvider();

            SqlDomainProvider = ParameterizedProvider.FromSingle<MeshDomain, SqlDomainTranslator>(domain =>
            {
                lock (sqlDomains)
                {
                    if (!sqlDomains.ContainsKey(domain)) { sqlDomains.Add(domain, new SqlDomainTranslator(this, domain)); }
                    return sqlDomains[domain];
                }
            });
        }


        #region SQL


        /// <summary>
        /// Executes the provided command.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="parameters">The parameters to use in the command.</param>
        public void ExecuteMeshCommand(string command, IEnumerable<SqlMeshParameter> parameters = null)
        {
            SqlRepository.ExecuteCommand(command, parameters);
        }

        /// <summary>
        /// Creates objects of the specified type and maps the query result to them.
        /// </summary>
        /// <typeparam name="T">The type of object to create.</typeparam>
        /// <param name="query">The query to retrieve the values.</param>
        /// <param name="parameters">The parameters to the query.</param>
        /// <returns>The objects created from the query result.</returns>
        public IEnumerable<T> ExecuteQuery<T>(string query, IEnumerable<SqlMeshParameter> parameters = null)
            where T : class
        {
            var table = SqlRepository.ExecuteQuery(query, parameters);
            return typeMap.GetValues<T>(table);
        }

        /// <summary>
        /// Creates objects of the specified type and maps the query result to them.
        /// </summary>
        /// <param name="type">The type of object to create.</param>
        /// <param name="query">The query to retrieve the values.</param>
        /// <param name="parameters">The parameters to the query.</param>
        /// <returns>The objects created from the query result.</returns>
        public IEnumerable<object> ExecuteQuery(Type type, string query, IEnumerable<SqlMeshParameter> parameters = null)
        {
            var table = SqlRepository.ExecuteQuery(query, parameters);
            return typeMap.GetValues(type, table);
        }

        /// <summary>
        /// Saves the provided values.
        /// </summary>
        /// <param name="values">The values to save.</param>
        /// <returns>The mapping of the value to the related save result.</returns>
        public Dictionary<object, RecordSaveResult> SaveValues(params object[] values)
        {
            values = values.Where(x => x != null).Distinct().ToArray();
            var result = new Dictionary<object, RecordSaveResult>();
            foreach (var value in values) { result[value] = new RecordSaveResult() { Value = value }; }
            values = values.Where(x => x != null).ToArray();

            var typeValues = RegisterTypes(values);

            var affectCommand = new StringBuilder();
            var affectParameterCreator = new ParameterCreator();

            //Iterate by object type.
            foreach (var typeSet in typeValues)
            {
                //foreach value of the type.
                foreach (var value in typeSet.Value)
                {
                    var table = QueryTable(typeSet.Key, value);

                    if (table.Rows.Count > 0)
                    {
                        result[value].Operation = RecordOperation.Update;
                        AddUpdateRecord(typeSet.Key, affectCommand, affectParameterCreator, value);
                    }
                    else
                    {
                        result[value].Operation = RecordOperation.Create;
                        AddInsertRecord(typeSet.Key, affectCommand, affectParameterCreator, value);
                    }
                }
            }

            var affectString = affectCommand.ToString();
            SqlRepository.ExecuteCommand(affectString, affectParameterCreator.Parameters);
            return result;
        }

        /// <summary>
        /// Registers the types of the values and returns objects having types with a corresponding table.
        /// </summary>
        /// <param name="values">The values with types to check.</param>
        /// <returns>Objects having types with a corresponding table.</returns>
        private Dictionary<TypeTableInfo, List<object>> RegisterTypes(IEnumerable<object> values)
        {
            var typeValues = new Dictionary<TypeTableInfo, List<object>>();
            foreach (var value in values)
            {
                var type = value.GetType();
                if (!typeInfos.ContainsKey(type))
                {
                    if (TypeTableInfo.TypeIsATable(type))
                    {
                        var typeInfo = new TypeTableInfo(type, this);
                        lock (typeInfos) { typeInfos.Add(type, typeInfo); }
                        SqlRepository.CreateTable(typeInfo.CreateTableSpecification);
                    }
                    else { continue; }
                }
                var info = typeInfos[type];
                if (!typeValues.ContainsKey(info)) { typeValues.Add(info, new List<object>()); }
                typeValues[info].Add(value);
            }
            return typeValues;
        }

        /// <summary>
        /// Queries for the table matching the provided value.
        /// </summary>
        /// <param name="tableTypeInfo">Information related to the type and tbale.</param>
        /// <param name="value">The value for which to query.</param>
        /// <returns>The data table with the matching record.</returns>
        private DataTable QueryTable(TypeTableInfo tableTypeInfo, object value)
        {
            var parameterCreator = new ParameterCreator();
            var keys = tableTypeInfo.KeyMembers;
            var query = new StringBuilder();
            query.Append("select ");
            foreach (var key in keys)
            {
                if (key != keys.First()) { query.Append(", "); }
                for (var i = 0; i < key.SqlType.SqlTypeCount; i++)
                {
                    if (i > 0) { query.Append(", "); }
                    query.Append(String.Format("{0}", key.CreateColumnSpecifications[i].Name));
                }
            }
            query.Append(String.Format(" from {0} where ", tableTypeInfo.TableName));
            foreach (var key in keys)
            {
                if (key != keys.First()) { query.Append(", "); }
                var keyValues = key.GetValues(value);
                for (var i = 0; i < key.SqlType.SqlTypeCount; i++)
                {
                    if (i > 0) { query.Append(", "); }
                    var parameter = parameterCreator.Create(keyValues[i]);
                    query.Append(String.Format("{0} = {1}", key.CreateColumnSpecifications[i].Name, parameter.Name));
                }
            }
            query.Append(";\n");
            var queryString = query.ToString();
            var table = SqlRepository.ExecuteQuery(query.ToString(), parameterCreator.Parameters);
            return table;
        }

        private void AddUpdateRecord(TypeTableInfo tableTypeInfo, StringBuilder command, ParameterCreator parameterCreator, object value)
        {
            var members = tableTypeInfo.NonKeyMembers.Where(x => x.CreateColumnSpecifications.Count() > 0).ToList();
            var keys = tableTypeInfo.KeyMembers.Where(x => x.CreateColumnSpecifications.Count() > 0).ToList();
            command.Append(String.Format(" update {0} set ", tableTypeInfo.TableName));
            foreach (var column in members)
            {
                if (column != members.First()) { command.Append(","); }
                var updateValues = column.GetValues(value);
                for (var i = 0; i < column.SqlType.SqlTypeCount; i++)
                {
                    if (i > 0) { command.Append(","); }
                    var parameter = parameterCreator.Create(updateValues[i]);
                    command.Append(String.Format("{0} = {1}", column.CreateColumnSpecifications[i].Name, parameter.Name));
                }
            }
            command.Append(" where ");
            foreach (var key in keys)
            {
                if (key != keys.First()) { command.Append(" and "); }
                var keyValues = key.GetValues(value);
                for (var i = 0; i < key.SqlType.SqlTypeCount; i++)
                {
                    if(i> 0) { command.Append(" and "); }
                    var parameter = parameterCreator.Create(keyValues[i]);
                    command.Append(String.Format("{0} = {1}", key.CreateColumnSpecifications[i].Name, parameter.Name));
                }
            }
            command.Append(";\n");
        }

        private void AddInsertRecord(TypeTableInfo tableTypeInfo, StringBuilder command, ParameterCreator parameterCreator, object value)
        {
            var members = tableTypeInfo.Members.Values.Where(x => x.CreateColumnSpecifications.Count() > 0).ToList();

            command.Append(String.Format("insert into {0} (", tableTypeInfo.TableName));

            foreach (var column in members)
            {
                if (column != members.First()) { command.Append(","); }
                for (var i = 0; i < column.SqlType.SqlTypeCount; i++)
                {
                    if(i > 0) { command.Append(","); }
                    command.Append(column.CreateColumnSpecifications[i].Name);
                }
            }
            command.Append(") values (");
            foreach (var column in members)
            {
                if (column != members.First()) { command.Append(","); }
                var insertValues = column.GetValues(value);
                for(var i = 0; i < column.SqlType.SqlTypeCount; i++)
                {
                    if (i > 0) { command.Append(","); }
                    var parameter = parameterCreator.Create(insertValues[i]);
                    command.Append(parameter.Name);
                    command.Append(" ");
                }
            }
            command.Append(");\n");
        }

        private TypeTableInfo GetModelInfo<ModelType>()
        {
            return GetModelInfo(typeof(ModelType));
        }

        /// <summary>
        /// Gets the model table info given its type.
        /// </summary>
        /// <param name="modelType">The type of the model.</param>
        /// <returns>The model table.</returns>
        public TypeTableInfo GetModelInfo(Type modelType)
        {
            lock (modelInfos)
            {
                if (!modelInfos.ContainsKey(modelType)) { modelInfos.Add(modelType, new TypeTableInfo(modelType, this)); }
            }
            return modelInfos[modelType];
        }

        #endregion


        #region Mesh

        /// <summary>
        /// Creates the domain. Use this for domains that are non-dynamic (e.g. non-C# type associated).
        /// </summary>
        /// <param name="domain"></param>
        public void CreateDomainOnce(SqlDomainTranslator sqlDomain)
        {
            lock (addDomains)
            {
                if (addDomains.Contains(sqlDomain.Domain)) { return; }
                CreateOrUpdateDomain(sqlDomain);
                addDomains.Add(sqlDomain.Domain);
            }
        }

        /// <summary>
        /// Creates a table for the specified domain or updates the table by adding columns.
        /// </summary>
        /// <remarks>
        /// 1. Deprecated columns are not handled.
        /// 2. Columns setting a different type are not changed.
        /// </remarks>
        /// <param name="domain">The mesh domain to create or update.</param>
        public void CreateOrUpdateDomain(SqlDomainTranslator sqlDomain)
        {
            var domain = sqlDomain.Domain;

            //Add the domain records.
            SaveDomains(domain);

            CreateTableSpecification table = new CreateTableSpecification() { Name = sqlDomain.TableName };
            foreach (var property in MeshDomain.MetaProperties)
            {
                table.Columns.AddRange(metaColumnSpecifications[property.Name]);
            }

            var mechanic = MechanicProvider.Provide(domain.Key);
            if (mechanic != null)
            {
                mechanic.SqlDomainTranslator = sqlDomain;
                mechanic.CreateDomain(table);
                return;
            }

            table.Columns.AddRange(keyColumnSpecifications);
            if (!domainColumnSpecifications.ContainsKey(domain))
            {
                var domainColumns = new Dictionary<string, List<CreateColumnSpecification>>();
                domainColumnSpecifications.Add(domain, domainColumns);
                foreach (var property in domain.Properties)
                {
                    if (!DataType.TypeIsKnown(property.Type)) { continue; }
                    domainColumns.Add(property.Name, new List<CreateColumnSpecification>());
                    var propertyTranslator = new SqlPropertyColumnTranslator(this, SqlRepository.SqlTypeProvider.Provide(DataType.FromKey(property.Type)), property.Name);
                    for (var i = 0; i < propertyTranslator.SqlType.SqlTypeCount; i++)
                    {
                        domainColumns[property.Name].Add(new CreateColumnSpecification()
                        {
                            IsPrimaryKey = false,
                            Name = SqlRepository.CreateColumnName(String.Format("{0}{1}", SqlMeshKeyword.ValuePrefix.Alias, propertyTranslator.ColumnNames[i])),
                            Type = propertyTranslator.SqlType.SqlTypeNames[i]
                        });
                    }
                }
            }
            foreach (var specList in domainColumnSpecifications[domain])
            {
                table.Columns.AddRange(specList.Value);
            }

            SqlRepository.CreateTable(table);

        }

        /// <summary>
        /// Deletes the domain with the provided key.
        /// </summary>
        /// <param name="domainKey">The key of the domain to delete.</param>
        public void DeleteDomain(IMeshKey domainKey)
        {
            var sqlCommand = String.Format("drop table {0};", domainKey);
            ExecuteMeshCommand(sqlCommand);
        }

        /// <summary>
        /// Gets the domain with the provided key.
        /// </summary>
        /// <param name="domainKey"></param>
        /// <returns></returns>
        public MeshDomain GetDomain(IMeshKey domainKey)
        {
            var sqlDomain = GetTableValues<SqlMeshDomain>(SqlMeshKeyword.MeshDomainTable.Alias, MeshKeyword.Key.Alias, domainKey).FirstOrDefault();
            if (sqlDomain == null) { return null; }
            sqlDomain.Properties = GetTableValues<SqlDomainProperty>(SqlMeshKeyword.DomainPropertyTable.Alias, SqlMeshKeyword.DomainKey.Alias, domainKey).ToList();
            return sqlDomain.GetDomain();
        }

        private IEnumerable<T> GetTableValues<T>(string tableName, string columnName, object keyValue)
            where T:class
        {
            var parameterCreator = new ParameterCreator();
            var keyType = SqlRepository.SqlTypeProvider.Provide(DataType.MeshKey);
            var command = new StringBuilder();
            command.Append(String.Format("select * from {0} where ", SqlRepository.CreateTableName(tableName)));
            var values = keyType.ToSqlTransform.Transform(keyValue);
            for(var i=0;i< keyType.SqlTypeCount; i++)
            {
                if (i > 0) { command.Append(" and "); }
                command.Append(SqlRepository.CreateColumnName(String.Format("{0}_{1}", columnName, i)));
                command.Append(" = ");
                command.Append(parameterCreator.Create(values[i]).Name);
            }
            var result = ExecuteQuery<T>(command.ToString(), parameterCreator.Parameters);
            return result;
        }

        /// <summary>
        /// Gets the domain matching the where clause.
        /// </summary>
        /// <param name="where">The where clause used to find the domain(s).</param>
        /// <returns>The matching domains.</returns>
        public IEnumerable<MeshDomain> GetDomains(WhereExpressionNode where)
        {
            var sqlWhere = new SqlMeshWhere(typeof(MeshDomain), where, this);
            var sqlCommand = String.Format("select {0} from {1} where {2};", SqlRepository.CreateColumnName(MeshKeyword.Key.Alias), SqlRepository.CreateTableName(SqlMeshKeyword.MeshDomainTable.Alias), sqlWhere.Where);
            var sqlDomainKeys = ExecuteQuery<SqlMeshDomain>(sqlCommand).ToList();
            var domains = sqlDomainKeys.Select(x => GetDomain(MeshKey.Parse(x.Key))).ToList();
            return domains;
        }

        /// <summary>
        /// Saves the provided domains.
        /// </summary>
        /// <param name="domains">The domains to save.</param>
        public void SaveDomains(params MeshDomain[] domains)
        {
            foreach (var domain in domains)
            {
                var sqlDomain = new SqlMeshDomain(domain, this);
                SaveValues(sqlDomain);
                //var select = new SqlMeshSelect(DomainReadRequest.ReadAll, SqlRepository);
                //var where = new SqlMeshWhere(WhereExpressionNode.CreateMemberIn(SqlMeshKeyword.DomainKey.Alias, new object[] { sqlDomain.Key }), this);
                //var query = new SqlMeshQuery(domain, select, where, this);
                //var sqlQuery = String.Format("select {0}, {1} from {2} where {0} = '{3}'"
                //    , SqlRepository.CreateColumnName(SqlMeshKeyword.DomainKey.Alias)
                //    , SqlRepository.CreateColumnName(SqlMeshKeyword.DomainPropertyTableName.Alias)
                //    , SqlRepository.CreateTableName(SqlMeshKeyword.DomainPropertyTable.Alias)
                //    , domain.Key);
                //var properties = ExecuteQuery<SqlDomainProperty>(sqlQuery);
                SaveValues(sqlDomain.Properties.ToArray());
                SaveValues(sqlDomain.Values.ToArray());
            }
        }

        /// <summary>
        /// Provides all the value domains.
        /// </summary>
        /// <returns>All the value domains.</returns>
        public IEnumerable<MeshDomain> GetAllValueDomains()
        {
            var sqlCommand = String.Format("select * from {0} where {1};", SqlRepository.CreateTableName(SqlMeshKeyword.MeshDomainTable.Alias), GetModelInfo<SqlMeshDomain>().GetMember(SqlMeshKeyword.IsLinkDomain.Alias).GetWhereString(new string[] { "0" }));
            var sqlDomains = ExecuteQuery<SqlMeshDomain>(sqlCommand);
            sqlCommand = String.Format("select * from {0};", GetModelInfo<SqlDomainProperty>().TableName);
            var sqlProperties = ExecuteQuery<SqlDomainProperty>(sqlCommand);
            sqlCommand = String.Format("select * from {0};", GetModelInfo<SqlDomainValue>().TableName);
            var sqlValues = ExecuteQuery<SqlDomainValue>(sqlCommand);
            foreach (var domain in sqlDomains)
            {
                domain.Properties = sqlProperties.Where(x => x.DomainKey == domain.Key).ToList();
                domain.Values = sqlValues.Where(x => x.DomainKey == domain.Key).ToList();
            }
            var result = sqlDomains.Select(x => x.GetDomain()).ToList();
            return result;
        }

        /// <summary>
        /// Provides all the link domains.
        /// </summary>
        /// <returns>The link domains.</returns>
        public IEnumerable<MeshDomainLink> GetAllLinkedDomains()
        {
            var sqlCommand = String.Format("select * from  {0};", SqlRepository.CreateTableName(SqlMeshKeyword.MeshDomainTable.Alias));
            var sqlDomains = ExecuteQuery<SqlMeshDomain>(sqlCommand).ToDictionary(x => x.Key, x => x);
            var result = new List<MeshDomainLink>();
            foreach (var domain in sqlDomains)
            {
                if (domain.Value.IsLinkDomain && sqlDomains.ContainsKey(domain.Value.ADomainKey) && sqlDomains.ContainsKey(domain.Value.BDomainKey))
                {
                    var linkDomain = new MeshDomainLink()
                    {
                        DomainA = sqlDomains[domain.Value.ADomainKey].GetDomain(),
                        DomainB = sqlDomains[domain.Value.BDomainKey].GetDomain()
                    };
                    linkDomain.SetKey(MeshKey.Parse(domain.Value.Key));
                    result.Add(linkDomain);
                }
            }
            return result;
        }

        /// <summary>
        /// Executes the query and maps the result to domain objects with the specified domain.
        /// </summary>
        /// <param name="domain">The domain of the objects to map.</param>
        /// <param name="query">The query to retrieve the object information.</param>
        /// <param name="parameters">The query parameters.</param>
        /// <returns>The query result mapped to domain objects with the specified domain.</returns>
        public DomainObject[] QueryDomainObject(SqlDomainTranslator domain, string query, IEnumerable<SqlMeshParameter> parameters = null)
        {
            var result = new List<DomainObject>();
            var table = SqlRepository.ExecuteQuery(query, parameters);

            if(table.Rows.Count == 0) { return result.ToArray(); }

            //Find the properties for which there is a valid result.
            var properties = new Dictionary<SqlDomainPropertyTranslator, int[]>();
            foreach(var property in domain.Properties)
            {
                var valid = true;
                var indices = new int[property.SqlType.SqlTypeCount];
                for(var i=0;i< property.SqlType.SqlTypeCount; i++)
                {
                    var columnName = property.PrefixedColumnNames[i];
                    if (!table.Columns.Contains(columnName))
                    {
                        valid = false;
                        break;
                    }
                    indices[i] = table.Columns[columnName].Ordinal;
                }
                if (valid) { properties.Add(property, indices); }
            }

            var objectMap = new Dictionary<DomainObject, DataRow>();
            foreach (DataRow row in table.Rows)
            {
                var domainObject = new DomainObject();
                objectMap.Add(domainObject, row);

                foreach(var property in properties)
                {
                    var values = new object[property.Key.ColumnNames.Length];
                    for(var i = 0; i < property.Key.SqlType.SqlTypeCount; i++)
                    {
                        values[i] = row.ItemArray[property.Value[i]];
                    }
                    property.Key.AssignValue(domainObject, values);
                }
            }

            var mechanic = MechanicProvider.Provide(domain.Domain.Key);
            if (mechanic != null)
            {
                mechanic.ManifestQueryResult(objectMap.Keys);
            }



            if (domain.Domain.IsGeneric && mechanic == null)
            {
                var genericItems = new Dictionary<DomainObject, Dictionary<SqlDomainPropertyTranslator, int[]>>();
                var genericMap = new Dictionary<string, Dictionary<SqlDomainPropertyTranslator, int[]>>();
                foreach(var pair in objectMap)
                {
                    if (!pair.Key.Meta.ContainsKey(MeshKeyword.Generics.Alias)) { continue; }
                    var serialized = String.Format("{0}", pair.Key.Meta[MeshKeyword.Generics.Alias]);
                    if (String.IsNullOrWhiteSpace(serialized)) { continue; }
                    if (genericMap.ContainsKey(serialized))
                    {
                        genericItems.Add(pair.Key, genericMap[serialized]);
                        continue; 
                    }
                    var generics = MeshGeneric.Deserialize(serialized);
                    serialized = MeshGeneric.SerializeGenerics(generics);
                    if (genericMap.ContainsKey(serialized)) { continue; }
                    var set = domain.GetGenericSet(generics);

                    var genericProperties = new Dictionary<SqlDomainPropertyTranslator, int[]>();
                    foreach (var property in set)
                    {
                        var valid = true;
                        var indices = new int[property.Value.SqlType.SqlTypeCount];
                        for (var i = 0; i < property.Value.SqlType.SqlTypeCount; i++)
                        {
                            var columnName = property.Value.PrefixedColumnNames[i];
                            if (!table.Columns.Contains(columnName))
                            {
                                valid = false;
                                break;
                            }
                            indices[i] = table.Columns[columnName].Ordinal;
                        }
                        if (valid) { genericProperties.Add(property.Value, indices); }
                    }

                    genericMap.Add(serialized, genericProperties);
                    genericItems.Add(pair.Key, genericMap[serialized]);
                }

                foreach(var item in genericItems)
                {
                    var row = objectMap[item.Key];
                    foreach (var property in item.Value)
                    {
                        var values = new object[property.Key.SqlType.SqlTypeCount];
                        for(var i = 0; i < property.Value.Length; i++)
                        {
                            values[i] = row[property.Value[i]];
                        }
                        property.Key.AssignValue(item.Key, values);
                    }
                }
            }

            return objectMap.Keys.ToArray();
        }
        
        /// <summary>
        /// Returns the number of records in the provided domain.
        /// </summary>
        /// <param name="domain">The domain for which the records will be counted.</param>
        /// <param name="where">Determines which records will be counted.</param>
        /// <returns>The number of matching records in the domain.</returns>
        public CountResult GetDomainWhereCount(SqlDomainTranslator domain, WhereExpressionNode where)
        {            
            var sqlWhere = new SqlMeshWhere(domain, where, this);
            var sqlCommand = String.Format("select count(*) as Result from {0} where {1};", domain.TableName, sqlWhere.Where);
            var result = ExecuteQuery<CountResult>(sqlCommand);
            return result.First();
        }

        /// <summary>
        /// Makes a domain key for the specified generics.
        /// </summary>
        /// <param name="domainKey">The base domain key.</param>
        /// <param name="serializedGenerics">The generics.</param>
        /// <returns>The domain key with the generics.</returns>
        private string MakeDomainGenericKey(IMeshKey domainKey, string serializedGenerics)
        {
            return String.Format("{0}-{1}", domainKey, serializedGenerics);
        }

        /// <summary>
        /// Prepares the repository for the domain generics.
        /// </summary>
        /// <param name="domain">The generic domain.</param>
        /// <param name="values">The values for which the repository will be prepared.</param>
        public void PrepareDomainGenerics(MeshDomain domain, DomainObject[] values)
        {
            var mechanic = MechanicProvider.Provide(domain.Key);
            var serialializedGenerics = values.Where(x => x != null)
                .Select(x => (string)x.Meta[MeshKeyword.Generics.Alias])
                .Where(x => !String.IsNullOrWhiteSpace(x))
                .Distinct();


            foreach (var serialialized in serialializedGenerics)
            {
                PrepareDomainGenerics(domain, serialialized, mechanic);
            }
        }

        private DomainGenericInfo PrepareDomainGenerics(MeshDomain domain, string serializedGenerics, ISqlRepositoryMechanic mechanic)
        {
            var generics = MeshGeneric.Deserialize(serializedGenerics);
            var domainGenericKey = MakeDomainGenericKey(domain.Key, serializedGenerics);
            if (preparedGenerics.ContainsKey(domainGenericKey)) { return preparedGenerics[domainGenericKey]; }
            lock (preparedGenerics)
            {
                if (preparedGenerics.ContainsKey(domainGenericKey)) { return preparedGenerics[domainGenericKey]; }
                if (MeshGeneric.ContainsParameter(generics))
                {
                    throw new ArgumentException("A generic parameter must be resolved prior to using in a repository. [7W6vFEBWkkSBem03RDIfVw]");
                }
                if (!domainGenerics.ContainsKey(domain.Key)) { domainGenerics.Add(domain.Key, new DomainGenericInfo() { Domain = domain }); ; }

                if (mechanic != null)
                {
                    mechanic.PrepareGenerics(this, SqlRepository, domain, generics);
                }
                else
                {
                    var stored = domainGenerics[domain.Key].PathNode;
                    var discovered = true;
                    for (var i = 0; i < generics.Length; i++)
                    {
                        var value = generics[i];
                        var typeKey = value.Key.Serialized;
                        if (!DataType.TypeIsKnown(value.Key)) { typeKey = domainValuedGeneric; }
                        if (stored.Next.ContainsKey(typeKey))
                        {
                            stored = stored.Next[typeKey];
                            continue;
                        }
                        else
                        {
                            discovered = false;
                            stored.Next.Add(typeKey, new GenericPathNode() { GenericKey = typeKey });
                            continue;
                        }
                    }
                    if (!discovered)
                    {
                        AddGenericColumns(domain, generics, domainGenerics[domain.Key]);
                    }
                }
                preparedGenerics.Add(domainGenericKey, domainGenerics[domain.Key]);
            }
            return preparedGenerics[domainGenericKey];
        }

        private void AddGenericColumns(MeshDomain domain, MeshGeneric[] generics, DomainGenericInfo genericInfo)
        {
            var serializedGenerics = MeshGeneric.SerializeGenerics(generics);
            var namedGenerics = generics.ToDictionary(x => x.Name, x => x);
            var columns = new List<CreateColumnSpecification>();
            var sqlDomain = SqlDomainProvider.Provide(domain);
            var addedProperties = sqlDomain.AddGenericProperties(generics);
            foreach(var property in addedProperties)
            {
                SqlRepository.AddTableColumns(sqlDomain.TableName, property.GetCreateColumnSpecifications());
            }
        }

        #endregion



        private class DomainGenericInfo
        {
            public MeshDomain Domain { get; set; }

            public GenericPathNode PathNode { get; set; } = new GenericPathNode();

            public Dictionary<string, string> TableNames { get; set; } = new Dictionary<string, string>();
        }

        private class GenericPathNode
        {

            public string GenericKey { get; set; }

            public Dictionary<string, GenericPathNode> Next { get; set; } = new Dictionary<string, GenericPathNode>();

        }

    }
}
