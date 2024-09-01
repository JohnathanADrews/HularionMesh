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
using HularionCore.Pattern.Topology;
using HularionCore.Structure;
using HularionMesh.Domain;
using HularionMesh.DomainLink;
using HularionMesh.DomainValue;
using HularionMesh.Exceptions;
using HularionMesh.Memory;
using HularionMesh.MeshType;
using HularionMesh.Repository;
using HularionMesh.SystemDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HularionMesh.Domain
{
    /// <summary>
    /// Provides domain value properties for stringly-typed objects.
    /// </summary>
    public class TypedDomainRegistrar
    {        
        private Dictionary<Type, MeshDomain> domains = new Dictionary<Type, MeshDomain>();

        private Dictionary<string, MeshDomain> domainsByKey = new Dictionary<string, MeshDomain>();

        private Type domainPropertyAttributeType = typeof(DomainPropertyAttribute);

        /// <summary>
        /// Provides a domain key given a type.
        /// </summary>
        public IParameterizedProvider<Type, IMeshKey> TypeKeyProvider { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public TypedDomainRegistrar()
        {

            TypeKeyProvider = ParameterizedProvider.FromSingle<Type, IMeshKey>(type =>
            {
                if (DataType.TypeIsKnown(type)) { return DataType.FromCSharpType(type).Key; }
                var domain = GetDomainFromType(type);
                if (domain != null) { return domain.Key; }
                return null;
            });
        }

        /// <summary>
        /// A collection of all of the registered domains.
        /// </summary>
        public IEnumerable<MeshDomain> Domains
        {
            get
            {
                lock (domains)
                {
                    return domainsByKey.Values;
                }
            }
        }

        /// <summary>
        /// Registers the given type using the provided name instead of the type's name.
        /// </summary>
        /// <typeparam name="DomainType">The type to register.</typeparam>
        /// <param name="domain">The domain name of the type.</param>
        public void RegisterDomainType<DomainType>(MeshDomain domain)
        {
            RegisterDomainType(typeof(DomainType), domain);
        }

        /// <summary>
        /// Registers the given type using the provided name instead of the type's name.
        /// </summary>
        /// <param name="domainType">THe type to register.</param>
        /// <param name="domain">The domain name of the type.</param>
        public void RegisterDomainType(Type domainType, MeshDomain domain)
        {
            if (domainType.IsGenericType) { domainType = domainType.GetGenericTypeDefinition(); }
            lock (domains)
            {
                if (domains.ContainsKey(domainType))
                {
                    if (domain != domains[domainType])
                    {
                        throw new ArgumentException(String.Format("The provided domain type, {0}, could not be registered as {1}: it is already registered as {2} [BAD9DC01-8B0E-443B-8472-7DA2806881FD].", domainType.Name, domain, domains[domainType]));
                    }
                }
                //domain.IsInternal = true;
                //domain.Properties = GetValueProperties(domainType);
                if (!domains.ContainsKey(domainType)) { domains.Add(domainType, domain); }
                if (!domainsByKey.ContainsKey(domain.Key.Serialized)) { domainsByKey.Add(domain.Key.Serialized, domain); }
                //domainsByTypeFullName.Add(domainType.FullName, domain);
            }
        }

        /// <summary>
        /// Gets the registered type name or the default type name if not registered.
        /// </summary>
        /// <param name="domainType">The type whose name will be returned.</param>
        /// <returns>The name of the provided type.</returns>
        public MeshDomain GetDomain(Type domainType)
        {
            if (domainType.IsGenericType)
            {
                domainType = domainType.GetGenericTypeDefinition();
            }
            if (domains.ContainsKey(domainType))
            {
                return domains[domainType];
            }
            return null;
        }

        /// <summary>
        /// Gets the domain given the domain key
        /// </summary>
        /// <param name="domainKey"></param>
        /// <returns></returns>
        public MeshDomain GetDomain(IMeshKey domainKey)
        {
            if (domainKey == null) { return null; }
            return GetDomain(domainKey.Serialized);
        }

        /// <summary>
        /// Gets the domain given the domain key
        /// </summary>
        /// <param name="domainKey"></param>
        /// <returns></returns>
        public MeshDomain GetDomain(string domainKey)
        {
            if (string.IsNullOrWhiteSpace(domainKey)) { return null; }
            if (domainsByKey.ContainsKey(domainKey))
            {
                return domainsByKey[domainKey];
            }
            return null;
        }

        /// <summary>
        /// Gets the registered type name or the default type name if not registered.
        /// </summary>
        /// <typeparam name="DomainType">The type whose name will be returned.</typeparam>
        /// <returns>The name of the provided type.</returns>
        public MeshDomain GetDomain<DomainType>()
        {
            return GetDomain(typeof(DomainType));
        }

        /// <summary>
        /// Gets the domain properties given the provided type.
        /// </summary>
        /// <param name="type">The type for which to provide the properties.</param>
        /// <returns>The domain properties given the provided type.</returns>
        public List<ValueProperty> GetTypeProperties(Type type)
        {
            var typeDomain = GetDomainFromType(type);
            var properties = type.GetProperties();

            var result = new List<ValueProperty>();

            //using Tuple<Type, int> because the traverser is complaining about having the same node being set.
            var traverser = new TreeTraverser<Tuple<Type, int>>();
            var parentMap = new Dictionary<Tuple<Type, int>, Tuple<Type, int>>();
            var genericMap = new Dictionary<Tuple<Type, int>, MeshGeneric>();
            var count = 0;

            var arrayProperties = new List<ValueProperty>();

            foreach (var property in type.GetProperties())
            {
                if (property.CustomAttributes.Select(x=>x.AttributeType).Contains(domainPropertyAttributeType))
                {
                    var proxyValue = (DomainPropertyAttribute)property.GetCustomAttributes().Where(x => x.GetType() == domainPropertyAttributeType).First();                    
                    var proxy = new ValueProperty() { Name = property.Name };
                    if (proxyValue.Selector == DomainObjectPropertySelector.Key) { proxy.Proxy = ValuePropertyProxy.KeyProxy; }
                    if (proxyValue.Selector == DomainObjectPropertySelector.CreationTime) { proxy.Proxy = ValuePropertyProxy.CreationTimeProxy; }
                    if (proxyValue.Selector == DomainObjectPropertySelector.Creator) { proxy.Proxy = ValuePropertyProxy.CreatorProxy; }
                    if (proxyValue.Selector == DomainObjectPropertySelector.UpdateTime) { proxy.Proxy = ValuePropertyProxy.UpdateTimeProxy; }
                    if (proxyValue.Selector == DomainObjectPropertySelector.Updater) { proxy.Proxy = ValuePropertyProxy.UpdaterProxy; }
                    if (proxyValue.Selector == DomainObjectPropertySelector.Generic) { proxy.Proxy = ValuePropertyProxy.Generics; }
                    if (proxyValue.Selector == DomainObjectPropertySelector.SerializedGeneric) { proxy.Proxy = ValuePropertyProxy.Generics; }
                    result.Add(proxy);
                    continue; 
                }
                genericMap.Clear();
                parentMap.Clear();
                var plan = traverser.CreateEvaluationPlan(TreeTraversalOrder.ParentLeftRight, new Tuple<Type, int>(property.PropertyType, count++), pType =>
                {
                    //if (!pType.Item1.IsGenericType) { return new Tuple<Type, int>[]{ }; }
                    var generics = pType.Item1.GenericTypeArguments.Select(x => new Tuple<Type, int>(x, count++)).ToList();
                    if (generics == null || generics.Count() == 0)
                    {
                        generics = pType.Item1.GetTypeInfo().GenericTypeParameters.Select(x => new Tuple<Type, int>(x, count++)).ToList();
                    }
                    if (pType.Item1.IsArray)
                    {
                        generics.Add(new Tuple<Type, int>(pType.Item1.GetElementType(), count++));
                    }
                    foreach (var generic in generics)
                    {
                        parentMap.Add(generic, pType);
                    }
                    return generics.ToArray();
                }, true);

                //the first item in plan is the root property.  All others branch off from it.
                var item = plan[0];
                var dataType = DataType.FromCSharpType(item.Item1);

                var valueProperty = new ValueProperty() { Name = property.Name, HasGenerics = property.PropertyType.ContainsGenericParameters };
                if (property.PropertyType.IsArray)
                {
                    valueProperty.HasGenerics = true;
                }

                if (dataType == DataType.UnknownCSType)
                {
                    var domain = GetDomain(item.Item1);
                    if (domain != null) 
                    { 
                        valueProperty.Type = domain.Key.Serialized;
                        valueProperty.Domain = domain;
                    }
                    else if (item.Item1.IsGenericParameter)
                    {
                        valueProperty.IsGenericParameter = true;
                        valueProperty.Type = item.Item1.Name;
                        valueProperty.Generics = MeshGeneric.FromType(item.Item1, TypeKeyProvider).ToList();
                        result.Add(valueProperty);
                        continue;
                    }
                    else { continue; } //The property type is not handled.
                }
                else { valueProperty.Type = dataType.Key.Serialized; }

                result.Add(valueProperty);

                for (var i = 0; i < plan.Length; i++) { genericMap.Add(plan[i], new MeshGeneric()); }
                valueProperty.Generics = genericMap[item].Generics;
                if (property.PropertyType.IsGenericType)
                {
                    valueProperty.HasGenerics = true;
                    var genericParameters = property.PropertyType.GetGenericTypeDefinition().GetTypeInfo().GenericTypeParameters;
                    for (var i = 0; i < genericParameters.Length && i < genericMap[plan[0]].Generics.Count(); i++) { genericMap[plan[0]].Generics[i].Name = genericParameters[i].Name; }
                }

                if (property.PropertyType.IsGenericParameter)
                {
                    valueProperty.Type = property.PropertyType.Name;
                    valueProperty.IsGenericParameter = true;
                    continue;
                }

                var genericsType = MeshGeneric.FromType(property.PropertyType, TypeKeyProvider);


                for (var i = 1; i < plan.Length; i++)
                {
                    item = plan[i];
                    var parent = parentMap[item];
                    var parentGeneric = genericMap[parent];
                    dataType = DataType.FromCSharpType(item.Item1);
                    IMeshKey meshKey = MeshKey.NullKey;
                    if (dataType == DataType.UnknownCSType)
                    {

                        var domain = GetDomain(item.Item1);
                        if (domain != null) { meshKey = domain.Key; }
                    }
                    else { meshKey = dataType.Key; }

                    var generic = genericMap[item];
                    generic.Key = meshKey;
                    if(item.Item1.IsGenericParameter)
                    {
                        generic.Name = item.Item1.Name;
                        generic.Mode = TypeGenericMode.Parameter;
                    }
                    else
                    {
                        generic.Mode = TypeGenericMode.Argument;
                    }
                    parentGeneric.Generics.Add(generic);
                }
            }
            return result;
        }

        /// <summary>
        /// Creates a DomainValueAffectCreate affect for each provided value.
        /// </summary>
        /// <param name="values">The values used to create the affect.</param>
        /// <returns>A domain value create affector for each value.</returns>
        public DomainValueAffectCreate[] CreateDomainValueAffectCreate(params object[] values)
        {
            var creators = values.Select(value => new DomainValueAffectCreate() { Source = value, Value = DomainObject.Derive(value) }).ToArray();
            return creators;
        }

        /// <summary>
        /// Creates a DomainValueAffectCreate affect for each provided value.
        /// </summary>
        /// <param name="values">The values used to create the affect.</param>
        /// <returns>A domain value create affector for each value.</returns>
        public DomainValueAffectInsert[] CreateDomainValueAffectInsert(params object[] values)
        {
            var inserts = values.Select(value => new DomainValueAffectInsert() { Value = DomainObject.Derive(value) }).ToArray();
            return inserts;
        }

        /// <summary>
        /// Gets the type of a domain from the domain.
        /// </summary>
        /// <param name="domain">The domain associated with the type.</param>
        /// <returns>The type of a domain from the domain.</returns>
        public Type GetDomainType(MeshDomain domain)
        {
            return domains.Where(x => x.Value.Key == domain.Key).Select(x => x.Key).FirstOrDefault();
        }

        /// <summary>
        /// Gets the type of a domain from the domain key.
        /// </summary>
        /// <param name="key">The key of the domain.</param>
        /// <returns>The type of a domain from the domain key.</returns>
        public Type GetDomainTypeFromDomainKey(IMeshKey key)
        {
            var domain = domainsByKey[key.Serialized];
            return GetDomainType(domain);
        }

        /// <summary>
        /// Gets the domain with the associated type.
        /// </summary>
        /// <typeparam name="DomainType">The type of the domain to get.</typeparam>
        /// <returns>The domain with the associated type.</returns>
        public MeshDomain GetDomainFromType<DomainType>()
        {
            return GetDomainFromType(typeof(DomainType));
        }

        /// <summary>
        /// Gets the domain with the associated type.
        /// </summary>
        /// <param name="domainType">The type of the domain to get.</param>
        /// <returns>The domain with the associated type.</returns>
        public MeshDomain GetDomainFromType(Type domainType)
        {
            if (domainType.IsGenericType) { domainType = domainType.GetGenericTypeDefinition(); }
            if (domains.ContainsKey(domainType)) { return domains[domainType]; }
            return null;
            //throw new MeshDomainNotFoundException(String.Format("The domain for type '{0}' was not registered. Make sure the assembly for the domain type is included as well as the domain include attribute. [wfgIq6XSzkGNGvkMGvhv5A]", domainType.Name));
        }

        /// <summary>
        /// This will initialize the properties for each domain.  This must be called exactly after all of the domain types have been registered so that references can be correctly resolved.
        /// </summary>
        public void InitializeProperties(IParameterizedProvider<MeshDomain, IDomainRepositoryMechanic> domainMechanicProvider)
        {
            var processed = new HashSet<MeshDomain>();
            foreach(var domain in domains)
            {
                if (processed.Contains(domain.Value)) { continue; }
                if (!processed.Contains(domain.Value)) { processed.Add(domain.Value); }
                var mechanic = domainMechanicProvider.Provide(domain.Value);
                if(mechanic != null)
                {
                    mechanic.InitializeProperties();
                    continue;
                }
                var properties = GetTypeProperties(domain.Key);
                domain.Value.Properties = properties.Where(x => x.Proxy == ValuePropertyProxy.None).ToList();
                domain.Value.Proxies = properties.Where(x => x.Proxy != ValuePropertyProxy.None).ToList();
            }
        }

    }



}
