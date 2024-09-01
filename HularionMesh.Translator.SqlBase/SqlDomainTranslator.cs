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
using HularionMesh;
using HularionMesh.Domain;
using HularionMesh.DomainValue;
using HularionMesh.MeshType;
using HularionMesh.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace  HularionMesh.Translator.SqlBase
{
    /// <summary>
    /// Manages translators for properties within a mesh domain. 
    /// </summary>
    public class SqlDomainTranslator
    {
        /// <summary>
        /// The translated domain.
        /// </summary>
        public MeshDomain Domain { get; private set; }

        /// <summary>
        /// The name of the table assciated with the domain.
        /// </summary>
        public string TableName { get; private set; }

        /// <summary>
        /// The translated Mesh properties.
        /// </summary>
        public List<SqlDomainPropertyTranslator> Properties { get; private set; } = new List<SqlDomainPropertyTranslator>();

        /// <summary>
        /// The property information for the domain key.
        /// </summary>
        public SqlDomainPropertyTranslator KeyProperty { get; private set; }

        /// <summary>
        /// The domain meta properties.
        /// </summary>
        public Dictionary<MetaProperty, SqlDomainPropertyTranslator> MetaProperties { get; private set; } = new Dictionary<MetaProperty, SqlDomainPropertyTranslator>();

        /// <summary>
        /// The non-generic domain value properties.
        /// </summary>
        public List<SqlDomainPropertyTranslator> NonGenericValueProperties { get; private set; } = new List<SqlDomainPropertyTranslator>();

        /// <summary>
        /// The generic domain value properties.
        /// </summary>
        //public List<SqlDomainPropertyTranslator> GenericValueProperties { get; private set; } = new List<SqlDomainPropertyTranslator>();

        public Dictionary<string , Dictionary<ValueProperty, SqlDomainPropertyTranslator>> NewGenerics = new Dictionary<string, Dictionary<ValueProperty, SqlDomainPropertyTranslator>>();

        /// <summary>
        /// The SQL repository.
        /// </summary>
        public SqlMeshRepository Repository { get; private set; }


        /// <summary>
        /// Provides a Mesh data type given a name.
        /// </summary>
        private IParameterizedProvider<string, DataType> dataTypeProvider;

        private Dictionary<string, SqlDomainPropertyTranslator> metaProperties = new Dictionary<string, SqlDomainPropertyTranslator>();
        private Dictionary<string, SqlDomainPropertyTranslator> valueProperties = new Dictionary<string, SqlDomainPropertyTranslator>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="domain">The translated domain.</param>
        /// <param name="sqlRepository">The connector-implemented repository.</param>
        public SqlDomainTranslator(SqlMeshRepository sqlRepository, MeshDomain domain)
        {
            Repository = sqlRepository;
            Domain = domain;
            dataTypeProvider = ParameterizedProvider.FromSingle<string, DataType>(typeKey =>
            {
                var dataType = DataType.FromKey(MeshKey.Parse(typeKey));
                if (dataType == null && typeKey == MeshKey.TypeKey.Serialized) { dataType = DataType.Text8; }
                return dataType;
            });
            TableName = Repository.DomainTableNameProvider.Provide(domain);
            var properties = new List<SqlDomainPropertyTranslator>();

            KeyProperty = new SqlDomainPropertyTranslator(Repository, SqlPropertyCategory.Key, MeshDomain.ObjectMeshKey);
            //properties.Add(new SqlDomainPropertyTranslator(SqlPropertyCategory.Key, ));
            MetaProperties = MeshDomain.MetaMap.ToDictionary(x=>x.Key, x => new SqlDomainPropertyTranslator(Repository, SqlPropertyCategory.Meta, x.Value));
            //NonGenericValueProperties = domain.Properties.Where(x => !x.IsGenericParameter).Select(x => new SqlDomainPropertyTranslator(Repository, SqlPropertyCategory.Property, x)).ToList();
            NonGenericValueProperties = domain.Properties.Where(x => DataType.TypeIsKnown(x.Type))
                .Select(x => new SqlDomainPropertyTranslator(Repository, SqlPropertyCategory.Value, x)).ToList();
            //GenericValueProperties = domain.Properties.Where(x => x.IsGenericParameter).Select(x => new SqlDomainPropertyTranslator(Repository, SqlPropertyCategory.Value, x)).ToList();

            Properties.Add(KeyProperty);
            Properties.AddRange(MetaProperties.Values);
            Properties.AddRange(NonGenericValueProperties);
            //Properties.AddRange(GenericValueProperties);

            foreach (var property in MetaProperties)
            {
                metaProperties.Add(property.Value.MeshProperty.Name, property.Value);
            }
            foreach (var property in NonGenericValueProperties)
            {
                valueProperties.Add(property.MeshProperty.Name, property);
            }



        }

        /// <summary>
        /// Gets the translator property given the category and the name.
        /// </summary>
        /// <param name="category">The category of property.</param>
        /// <param name="name">The name of the property</param>
        /// <returns>The property with the givne category and name.</returns>
        public SqlDomainPropertyTranslator GetProperty(SqlPropertyCategory category, string name)
        {
            if (category == SqlPropertyCategory.Key) { return KeyProperty; }
            if (category == SqlPropertyCategory.Meta) { return metaProperties[name]; }
            return valueProperties[name];
        }

        /// <summary>
        /// Gets the property translator given its name.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <returns>The property translator</returns>
        public SqlDomainPropertyTranslator GetProperty(string name)
        {
            if(KeyProperty.MeshProperty.Name == name) { return KeyProperty; }
            if (metaProperties.ContainsKey(name)) { return metaProperties[name]; }
            if (valueProperties.ContainsKey(name)) { return valueProperties[name]; }
            return null;
        }

        /// <summary>
        /// Gets the meta property translator give the type of meta property that it is.
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        public SqlDomainPropertyTranslator GetMetaProperty(MetaProperty meta)
        {
            return MetaProperties[meta];
        }



        /// <summary>
        /// Adds the domain's generic properties.
        /// </summary>
        /// <param name="generics">The generics to add.</param>
        public IEnumerable<SqlDomainPropertyTranslator> AddGenericProperties(MeshGeneric[] generics)
        {
            var added = new List<SqlDomainPropertyTranslator>();
            var serializedGenerics = MeshGeneric.SerializeGenerics(generics);
            lock (NewGenerics)
            {
                if (NewGenerics.ContainsKey(serializedGenerics)) { return added; }
                var genericSet = new Dictionary<ValueProperty, SqlDomainPropertyTranslator>();
                NewGenerics.Add(serializedGenerics, genericSet);

                var namedGenerics = generics.ToDictionary(x => x.Name, x => x);
                foreach (var property in Domain.Properties)
                {
                    if (!property.IsGenericParameter) { continue; }
                    var generic = namedGenerics[property.Type];
                    if (!DataType.TypeIsKnown(generic.Key)) { continue; }

                    var translator = new SqlDomainPropertyTranslator(Repository, property, generic);
                    genericSet.Add(property, translator);
                    added.Add(translator);
                }
            }
            return added;
        }

        /// <summary>
        /// Gets a dictionary of Mesh properties to their translators.
        /// </summary>
        /// <param name="generics">The generic arguments corresponding to the domain generic parameters.</param>
        /// <returns>A dictionary of Mesh properties to their translators.</returns>
        public Dictionary<ValueProperty, SqlDomainPropertyTranslator> GetGenericSet(MeshGeneric[] generics)
        {
            var serialized = MeshGeneric.SerializeGenerics(generics);
            if (!NewGenerics.ContainsKey(serialized))
            {
                AddGenericProperties(generics);
                if (!NewGenerics.ContainsKey(serialized)) { return null; }
            }
            return NewGenerics[serialized];
        }


    }
}
