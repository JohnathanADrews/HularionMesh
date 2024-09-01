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
using  HularionMesh.Translator.SqlBase.ORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HularionMesh.MeshType;

namespace  HularionMesh.Translator.SqlBase.Model
{
    /// <summary>
    /// Maps a mesh domain to a SQL table to get or set the domain.
    /// </summary>
    [Table("MeshDomain")]
    public class SqlMeshDomain
    {
        /// <summary>
        /// The domain key.
        /// </summary>
        [ColumnAttribute("Key")]
        [PrimaryKeyAttribute]
        public string Key { get; set; }

        /// <summary>
        /// The name of the table storing the domain values.
        /// </summary>
        [ColumnAttribute("TableName")]
        public string TableName { get; set; }

        /// <summary>
        /// The generic parameters of the domain.
        /// </summary>
        [ColumnAttribute("Generics")]
        public string Generics { get; set; }

        /// <summary>
        /// true iff the domain has generic parameters.
        /// </summary>
        [ColumnAttribute("IsGeneric")]
        public bool IsGeneric { get; set; }

        /// <summary>
        /// The unique portion of the domain key.
        /// </summary>
        [ColumnAttribute("UniqueName")]
        public string UniqueName { get; set; }

        /// <summary>
        /// true iff this domain links two other domains.
        /// </summary>
        [ColumnAttribute("IsLinkDomain")]
        public bool IsLinkDomain { get; set; }

        /// <summary>
        /// A domain key if this domain is a link domain.
        /// </summary>
        [ColumnAttribute("ADomainKey")]
        public string ADomainKey { get; set; }

        /// <summary>
        /// A domain key if this domain is a link domain.
        /// </summary>
        [ColumnAttribute("BDomainKey")]
        public string BDomainKey { get; set; }

        /// <summary>
        /// The properties associated with this domain.
        /// </summary>
        public List<SqlDomainProperty> Properties { get; set; } = new List<SqlDomainProperty>();

        /// <summary>
        /// The values associated with this domain.
        /// </summary>
        public List<SqlDomainValue> Values { get; set; } = new List<SqlDomainValue>();

        private static Type linkDomainType = typeof(MeshDomainLink);

        /// <summary>
        /// The SqlMeshRepository.
        /// </summary>
        public SqlMeshRepository Repository { get; private set; }

        /// <summary>
        /// The mesh domain that this model represents.
        /// </summary>
        public MeshDomain Domain { get; set; }

        public readonly int DefaultMultiOrder = 0;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SqlMeshDomain()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="domain">The mesh domain associated with this domain.</param>
        /// <param name="repository">The SQL repository.</param>
        public SqlMeshDomain(MeshDomain domain, SqlMeshRepository repository)
        {
            Domain = domain;
            Repository = repository;
            FromDomain();
            if (domain.GetType() == linkDomainType)
            {
                var link = (MeshDomainLink)domain;
                IsLinkDomain = true;
                ADomainKey = link.DomainA.Key.Serialized;
                BDomainKey = link.DomainB.Key.Serialized;
            }
            TableName = repository.DomainTableNameProvider.Provide(domain);
        }

        //public SqlMeshDomain(MeshDomainLink domain, SqlMeshRepository repository)
        //{
        //    Domain = domain;
        //    Repository = repository;
        //    FromDomain();
        //}

        /// <summary>
        /// Creates a MeshDomain from this.
        /// </summary>
        /// <returns>The cretaed domain.</returns>
        public MeshDomain GetDomain()
        {
            var domain = new MeshDomain();
            domain.SetKey(MeshKey.Parse(Key));
            domain.SerializedGenerics = Generics;
            domain.UniqueName = UniqueName;
            domain.Properties = Properties.Select(x => new ValueProperty()
            {
                //Key = MeshKey.Parse(x.Key),
                Name = x.Name,
                Type = x.Type,
                Generics = MeshGeneric.Deserialize(x.Generics).ToList(),
                IsGenericParameter = x.IsGenericParameter,
                Default = String.Format("{0}", x.Default), 
                HasGenerics = !String.IsNullOrWhiteSpace(x.Generics)
            }).ToList();
            domain.Values = Values.ToDictionary(x => x.Name, x => (object)x.Value);
            return domain;
        }

        private void FromDomain()
        {
            Key = Domain.Key.Serialized;
            Generics = Domain.SerializedGenerics;
            IsGeneric = Domain.IsGeneric;
            UniqueName = Domain.UniqueName;
            foreach(var property in Domain.Properties)
            {
                if (property.IsGenericParameter)
                {
                    AddSqlDomainProperty(property, null, DefaultMultiOrder);
                    continue;
                }
                if (!DataType.TypeIsKnown(property.Type)) { continue; }
                var dataType = DataType.FromKey(property.Type);
                var sqlType = Repository.SqlRepository.SqlTypeProvider.Provide(dataType);
                if (property.Proxy == ValuePropertyProxy.KeyProxy)
                {
                    for (var i = 0; i < sqlType.SqlTypeCount; i++)
                    {
                        AddSqlDomainProperty(property, sqlType.SqlPrimaryKeyTypeNames[i], i);
                    }
                }
                else
                {
                    for (var i = 0; i < sqlType.SqlTypeCount; i++)
                    {
                        AddSqlDomainProperty(property, sqlType.SqlTypeNames[i], i);
                    }
                }
            }
            Values = Domain.Values.Select(x => new SqlDomainValue() { Name = x.Key, Value = (x.Value == null ? null : x.Value.ToString()) }).ToList();
        }

        private void AddSqlDomainProperty(ValueProperty property, string sqlType, int multiOrder)
        {
            var sqlProperty = new SqlDomainProperty();
            sqlProperty.Name = property.Name;
            sqlProperty.DomainKey = Domain.Key.Serialized;
            sqlProperty.Type = property.Type;
            sqlProperty.Generics = MeshGeneric.SerializeGenerics(property.Generics.ToArray());
            sqlProperty.IsGenericParameter = property.IsGenericParameter;
            sqlProperty.ValuePropertyProxy = String.Format("{0}", property.Proxy);
            sqlProperty.Default = String.Format("{0}", property.Default);
            sqlProperty.SqlType = sqlType;
            sqlProperty.MultiTypeOrder = multiOrder;
            Properties.Add(sqlProperty);
        }

        public override string ToString()
        {
            return String.Format("{0} : {1}", IsLinkDomain ? "Link" : "Domain", Key);
        }


    }
}
