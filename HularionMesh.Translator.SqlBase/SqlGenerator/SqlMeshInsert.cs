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
using HularionMesh.DomainValue;
using HularionMesh.MeshType;
using HularionMesh.SystemDomain;
using  HularionMesh.Translator.SqlBase.ORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HularionMesh.DomainLink;

namespace  HularionMesh.Translator.SqlBase.SqlGenerator
{
    /// <summary>
    /// A SQL insert statement derived from a mesh request.
    /// </summary>
    public class SqlMeshInsert
    {
        /// <summary>
        /// The key fo the user performing the insert.
        /// </summary>
        public IMeshKey UserKey { get; private set; }

        /// <summary>
        /// The domain in which the insert is happening.
        /// </summary>
        public SqlDomainTranslator SqlDomain { get; private set; }

        /// <summary>
        /// The SQL insert statement.
        /// </summary>
        public string Insert { get; set; }

        /// <summary>
        /// The parameters used in the insert statement.
        /// </summary>
        public ParameterCreator ParameterCreator { get; private set; } = new ParameterCreator();

        public SqlMeshRepository Repository { get; private set; }


        private bool isDomainLinkType;

        /// <summary>
        /// Constructo.
        /// </summary>
        /// <param name="userKey">The key fo the user performing the insert.</param>
        /// <param name="sqlDomain">The domain in which the insert is happening.</param>
        /// <param name="values">The object being inserted.</param>
        public SqlMeshInsert(IMeshKey userKey,
            SqlDomainTranslator sqlDomain,
            SqlMeshRepository repository,
            params DomainObject[] values)
        {
            this.UserKey = userKey;
            SqlDomain = sqlDomain;
            isDomainLinkType = (SqlDomain.GetType() == typeof(MeshDomainLink));
            Repository = repository;
            foreach (var value in values) { value.Key = repository.SqlRepository.DomainValueKeyCreator.Create(sqlDomain.Domain); }
            var mechanic = repository.MechanicProvider.Provide(sqlDomain.Domain.Key);
            if(mechanic == null)
            {
                SetupInsert(values);
            }
            else
            {
                mechanic.SetupInsert(this, values);
            }
        }

        /// <summary>
        /// Sets up the insert statements for each of the provided values.
        /// </summary>
        /// <param name="values">The values to setup.</param>
        public void SetupInsert(DomainObject[] values)
        {
            var command = new StringBuilder();
            var members = SqlDomain.Properties.Where(x => !x.MeshProperty.IsGenericParameter).Select(x => x.MeshProperty.Name).ToList();
            var genericMembers = SqlDomain.Properties.Where(x => x.MeshProperty.IsGenericParameter).ToList();
            var genericMap = new Dictionary<DomainObject, Dictionary<string, MeshGeneric>>();
            if (SqlDomain.Domain.IsGeneric)
            {
                var genericArguments = new Dictionary<string, Dictionary<string, MeshGeneric>>();
                foreach (var value in values)
                {
                    var serialized = (string)value.Meta[MeshKeyword.Generics.Alias];
                    if (!genericArguments.ContainsKey(serialized))
                    {
                        genericArguments.Add(serialized, MeshGeneric.Deserialize(serialized).ToDictionary(x => x.Name, x => x));
                    }
                    genericMap.Add(value, genericArguments[serialized]);
                }
            }

            var metasValues = new Dictionary<SqlDomainPropertyTranslator, object>();
            metasValues.Add(SqlDomain.GetMetaProperty(MetaProperty.CreationTime), DateTime.UtcNow);
            metasValues.Add(SqlDomain.GetMetaProperty(MetaProperty.ValueCreator), UserKey);
            metasValues.Add(SqlDomain.GetMetaProperty(MetaProperty.UpdateTime), DateTime.UtcNow);
            metasValues.Add(SqlDomain.GetMetaProperty(MetaProperty.ValueUpdater), UserKey);

            Action<DomainObject> addCreate = domainObject =>
            {

                var serializedGenerics = String.Format("{0}", domainObject.Meta[MeshKeyword.Generics.Alias]);
                var generics = MeshGeneric.Deserialize(serializedGenerics);
                var genericSet = SqlDomain.GetGenericSet(generics);

                command.Append(String.Format("insert into {0} (", SqlDomain.TableName));
                command.Append(SqlDomain.KeyProperty.GetStartInsertString());
                foreach (var meta in metasValues)
                {
                    command.Append(",");
                    command.Append(meta.Key.GetStartInsertString());
                }
                command.Append(",");
                command.Append(SqlDomain.GetMetaProperty(MetaProperty.Generics).GetStartInsertString());

                if (SqlDomain.Domain.IsGeneric)
                {
                    var arguments = genericMap[domainObject];
                    foreach (var property in genericSet)
                    {
                        var dataType = DataType.FromKey(arguments[property.Key.Type].Key);
                        var sqlType = Repository.SqlRepository.SqlTypeProvider.Provide(dataType);
                        if (!domainObject.Values.ContainsKey(property.Key.Name)) { continue; }
                        command.Append(",");
                        command.Append(property.Value.GetStartInsertString());
                    }
                }

                //if (!isDomainLinkType)
                //{
                //    command.Append(",");
                //    command.Append(Repository.SqlRepository.CreateColumnName(SqlMeshKeyword.GenericsColumnName.Alias));
                //}

                foreach (var property in SqlDomain.NonGenericValueProperties)
                {
                    command.Append(",");
                    command.Append(property.GetStartInsertString());
                }


                command.Append(") values (");


                command.Append(SqlDomain.KeyProperty.GetValueInsertString(ParameterCreator, domainObject.Key));

                foreach (var meta in metasValues)
                {
                    command.Append(",");
                    command.Append(meta.Key.GetValueInsertString(ParameterCreator, meta.Value));
                }
                command.Append(",");
                command.Append(SqlDomain.GetMetaProperty(MetaProperty.Generics).GetValueInsertString(ParameterCreator, domainObject.Meta[MeshKeyword.Generics.Alias]));

                if (SqlDomain.Domain.IsGeneric)
                {
                    var arguments = genericMap[domainObject];
                    foreach (var property in genericSet)
                    {
                        if (!domainObject.Values.ContainsKey(property.Key.Name)) { continue; }
                        command.Append(",");

                        command.Append(property.Value.GetValueInsertString(ParameterCreator, domainObject.Values[property.Key.Name]));
                    }
                }

                //Generics
                //if (!isDomainLinkType)
                //{
                //    object generics = string.Empty;
                //    if (domainObject.Meta.ContainsKey(MeshKeyword.Generics.Alias)) { generics = domainObject.Meta[MeshKeyword.Generics.Alias]; }
                //    parameter = GetParameter(generics, DataType.Text8);
                //    command.Append(",");
                //    command.Append(parameter.Name);
                //}

                foreach (var property in SqlDomain.NonGenericValueProperties)
                {
                    command.Append(",");
                    command.Append(property.GetValueInsertString(ParameterCreator, domainObject.Values[property.MeshProperty.Name]));
                }

                //foreach (var key in members)
                //{
                //    if (!domainObject.Values.ContainsKey(key)) { continue; }
                //    command.Append(",");
                //    parameter = GetParameter(domainObject.Values[key], SqlDomain.Properties.Where(x => x.MeshProperty.Name == key).First().MeshProperty.Type);
                //    command.Append(parameter.Name);
                //}


                command.Append(");\n");
            };

            foreach (var value in values)
            {
                var tableName = Repository.SqlRepository.CreateTableName(SqlDomain.Domain.Key);
                addCreate(value);
            }

            Insert = command.ToString();
        }

    }
}
