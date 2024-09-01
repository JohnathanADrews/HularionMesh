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
using HularionMesh.Domain;
using HularionMesh.DomainAggregate;
using HularionMesh.DomainLink;
using HularionMesh.DomainValue;
using HularionMesh.MeshType;
using HularionMesh.Repository;
using HularionMesh.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HularionMesh.SystemDomain
{
    /// <summary>
    /// A system domain representing a set of key/value pairs, each having a unique key.
    /// </summary>
    /// <typeparam name="KeyType">The type of the key.</typeparam>
    /// <typeparam name="ValueType"></typeparam>
    [MeshSystemDomainAttribute(Map.Map_KeyPartial, Map.Map_Name)]
    public class Map<KeyType, ValueType>
    {
        /// <summary>
        /// The key of the obejct.
        /// </summary>
        [DomainPropertyAttribute(DomainObjectPropertySelector.Key)]
        public IMeshKey MeshKey { get; set; }

        /// <summary>
        /// The creator of the object.
        /// </summary>
        [DomainPropertyAttribute(DomainObjectPropertySelector.Creator)]
        public string Creator { get; set; }

        /// <summary>
        /// The creation time of the object.
        /// </summary>
        [DomainPropertyAttribute(DomainObjectPropertySelector.CreationTime)]
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// The last updater of the object.
        /// </summary>
        [DomainPropertyAttribute(DomainObjectPropertySelector.Updater)]
        public string Updater { get; set; }

        /// <summary>
        /// The time the object was last updated.
        /// </summary>
        [DomainPropertyAttribute(DomainObjectPropertySelector.UpdateTime)]
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// The serialized generics of the object.
        /// </summary>
        [DomainPropertyAttribute(DomainObjectPropertySelector.SerializedGeneric)]
        public string SerializedGeneric { get; set; }

        private Dictionary<KeyType, ValueType> Mapping { get; set; } = new Dictionary<KeyType, ValueType>();

        internal UniqueSet<KeyedValue<KeyType, ValueType>> MapSet { get; set; } = new UniqueSet<KeyedValue<KeyType, ValueType>>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public Map()
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="keyedValues">The values to add.</param>
        public Map(IEnumerable<KeyedValue<KeyType, ValueType>> keyedValues)
        {
            if(keyedValues == null) { return; }
            foreach(var keyedValue in keyedValues)
            {
                Mapping.Add(keyedValue.Key, keyedValue.Value);
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dictionary">The dictionary containing the values to add.</param>
        public Map(Dictionary<KeyType, ValueType> dictionary)
        {
            if(dictionary == null) { return; }
            Mapping = dictionary.ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// Sets the given value to the given key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Set(KeyType key, ValueType value)
        {
            Mapping[key] = value;
        }

        /// <summary>
        /// Sets the given pairs in the Map.
        /// </summary>
        /// <param name="pairs">The key/value pairs to add.</param>
        public void Set(params KeyedValue<KeyType, ValueType>[] pairs)
        {
            foreach(var pair in pairs)
            {
                Mapping[pair.Key] = pair.Value;
            }
        }

        /// <summary>
        /// Determines whether the key is present in the Map.
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>true iff the key is present in the Map.</returns>
        public bool ContainsKey(KeyType key)
        {
            return Mapping.ContainsKey(key);
        }

        /// <summary>
        /// Gets the value associated to the given key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value associated to the given key.</returns>
        public ValueType Get(KeyType key)
        {
            return Mapping[key];
        }

        /// <summary>
        /// Removes the specified item from the map.
        /// </summary>
        /// <param name="key">The key of the key/item pair to remove.</param>
        public void Remove(KeyType key)
        {
            Mapping.Remove(key);
        }

        /// <summary>
        /// Removes all items from the map.
        /// </summary>
        public void Clear()
        {
            Mapping.Clear();
        }

        /// <summary>
        /// Gets all the key/value pairs in the map.
        /// </summary>
        /// <returns>All the key/value pairs in the map.</returns>
        public IEnumerable<KeyedValue<KeyType, ValueType>> GetKeyedValues()
        {
            return Mapping.Select(x => new KeyedValue<KeyType, ValueType>(x.Key, x.Value)).ToList();
        }

        /// <summary>
        /// Gets all the key/value pairs as a set.
        /// </summary>
        /// <returns>All the key/value pairs as a set.</returns>
        public UniqueSet<KeyedValue<KeyType, ValueType>> GetMapSet()
        {
            MapSet.AddRange(GetKeyedValues());
            return MapSet;
        }

        /// <summary>
        /// Sets the key/value pairs using the provided set.
        /// </summary>
        /// <param name="mapSet">Contains the key/value pairs to set.</param>
        public void SetMapSet(UniqueSet<KeyedValue<KeyType, ValueType>> mapSet)
        {
            MapSet = mapSet;
            Set(mapSet.Items.ToArray());
        }

        /// <summary>
        /// Creates a dictionary from this map.
        /// </summary>
        /// <returns>A dictionary containing the map values.</returns>
        public IDictionary<KeyType, ValueType> ToDictionary()
        {
            return Mapping.ToDictionary(x => x.Key, x => x.Value);
        }

    }

    public static class Map
    {
        /// <summary>
        /// The unique name for a Map.
        /// </summary>
        public const string Map_KeyPartial = "System_Map";
        public const string Map_Name = "Map";

        public const string MappingDomainValue = "Mapping";


        internal const string methodGetKeyedValues = "GetKeyedValues";
        internal const string methodGetMapSet = "GetMapSet";
        internal const string methodSetMapSet = "SetMapSet";
        internal const string methodToDictionary = "ToDictionary";

        public static Type MapDomainType = typeof(Map<,>);

        public static Type[] DomainTypes = new Type[] { MapDomainType };

        public static bool MapsType(Type type)
        {
            if (type == null) { return false; }
            if (!type.IsGenericType) { return false; }
            type = type.GetGenericTypeDefinition();
            if (DomainTypes.Contains(type)) { return true; }
            return false;
        }

        /// <summary>
        /// Creates a dictionary from the provided map.
        /// </summary>
        /// <param name="map">The map that will generate the dictionary.</param>
        /// <returns>The dictionary.</returns>
        /// <exception cref="ArgumentException">map must be of type Map.</exception>
        public static object ToDictionary(object map)
        {
            if(map == null)
            {
                return null;
            }
            var type = map.GetType();
            if(type.GetGenericTypeDefinition() != typeof(Map<,>))
            {
                throw new ArgumentException("The provided value is not of type Map. it is type '{0}'. []", type.Name);
            }
            var method = type.GetMethods().Where(x => x.Name == methodToDictionary).First();
            return method.Invoke(map, new object[] { });
        }

    }

    /// <summary>
    /// The repository mechanic for the Set sytem domain.
    /// </summary>
    public class MapDomainRepositoryMechanic : IDomainRepositoryMechanic
    {
        /// <summary>
        /// The mapped types.
        /// </summary>
        public IEnumerable<Type> MappedTypes { get { return Map.DomainTypes; } }

        public void InitializeProperties()
        {
            //Nothing to do here.
        }

        public bool CanCreateTypeManifest(Type type)
        {
            return Map.MapsType(type);
        }

        public TypeManifest CreateTypeManifest(Type type)
        {
            if (!type.IsGenericType || !CanCreateTypeManifest(type) || type.GetGenericTypeDefinition() != Map.MapDomainType) { return null; }

            var manifestor = new TypeManifest();
            var genericKeyType = type.GetGenericArguments()[0];
            var genericValueType = type.GetGenericArguments()[1];
            var newMapType = Map.MapDomainType.MakeGenericType(type.GetGenericArguments());
            //var keyedValueType = KeyedValue.KeyValuePairType.MakeGenericType(genericKeyType, genericValueType);
            var propertyAssigners = TypeManager.GetMemberAssigners(type);
            var reversePropertyAssigners = TypeManager.GetReverseMemberAssigners(type);
            var methodGetKeyedValues = type.GetMethod(Map.methodGetKeyedValues);

            manifestor.ToTypeObject = new Func<DomainObject, object>(domainObject =>
            {
                object set = null;
                if (domainObject.Values.ContainsKey(Map.MappingDomainValue))
                {
                    set = Activator.CreateInstance(newMapType, new object[] { domainObject.Values[Map.MappingDomainValue] });
                }
                else
                {
                    set = Activator.CreateInstance(newMapType, new object[] { });
                }
                foreach (var selector in propertyAssigners)
                {
                    selector(set, domainObject);
                }
                return set;
            });


            manifestor.UpdateTypeObject = new Action<DomainObject, object>((domainObject, typeObject) =>
            {
                foreach (var selector in propertyAssigners) { selector(typeObject, domainObject); }
            });

            manifestor.ToDomainObject = new Func<object, DomainObject>(value =>
            {
                if (value == null)
                {
                    return null;
                }
                var result = new DomainObject();
                foreach (var assigner in reversePropertyAssigners)
                {
                    assigner(value, result);
                }
                return result;
            });

            return manifestor;
        }

        public RealizedMeshDomain CreateRealizedDomainFromType(IMeshRepository repository, Type type)
        {
            var domain = new RealizedMeshDomain(repository.GetDomainFromType(type), MeshGeneric.FromType(type, repository.TypeKeyProvider), repository.DomainByKeyProvider)
            {
                Domain = repository.GetDomainFromType(type),
                GenericsArguments = MeshGeneric.FromType(type, repository.TypeKeyProvider).ToDictionary(x => x.Name, x => x)
            };
            return domain;
        }

        public RealizedMeshDomain CreateRealizedDomainFromType<T>(IMeshRepository repository)
        {
            return CreateRealizedDomainFromType(repository, typeof(T));
        }

        public void SetupOperationLink(IMeshRepository repository, DomainOperationLink link, IDictionary<IMeshKey, DomainOperationLink> nodeLinks)
        {
            var keyedValueType = KeyedValue.KeyedValueType.MakeGenericType(link.SourceType.GetGenericArguments());
            var setType = UniqueSet.UniqueSetDomainType.MakeGenericType(keyedValueType);
            var setDomain = repository.GetDomainFromType(setType);
            var setMechanic = repository.DomainMechanicProvider.Provide(setDomain);

            var typeLink = new DomainOperationLink()
            {
                RealizedDomain = setMechanic.CreateRealizedDomainFromType(repository, setType),
                Parent = link,
                NodeType = DomainOperationMode.Domain,
                SourceType = setType
            };
            var memberLink = new DomainOperationLink()
            {
                MemberName = Map.MappingDomainValue,
                Member = typeLink,
                Parent = link,
                NodeType = DomainOperationMode.Link,
                SourceType = typeLink.SourceType
            };
            typeLink.Parent = memberLink;
            link.Members.Add(Map.MappingDomainValue, memberLink);
            link.IsSystemDomain = true;
        }

        public void SetupSaveLink(IMeshRepository repository, SaveLink link)
        {
            if (link.DomainObject == null || link.DomainObject.Key == null) { link.DomainObject = DomainObject.Derive(link.Value); }
            link.DomainObject.Meta[MeshKeyword.Generics.Alias] = link.OperationLink.RealizedDomain.SerializedGenerics;

            var sourceType = link.Value.GetType();
            var keyedValueSet = sourceType.GetMethod(Map.methodGetMapSet).Invoke(link.Value, new object[] { });

            var setSave = new SaveLink()
            {
                Parent = link,
                DomainObject = DomainObject.Derive(keyedValueSet),
                OperationLink = link.OperationLink.Members[Map.MappingDomainValue].Member,
                Value = keyedValueSet,
                LinkProvider = link.LinkProvider
            };
            setSave.DomainObject.Values.Clear();
            link.Links.Add(setSave);
            link.Added.Add(setSave);
            //Add in Members to use the standard LinkAffectorProvider.
            link.Members.Add(Map.MappingDomainValue, setSave);

        }

        public Action LinkResults(QueriedLink queriedLink, DomainOperationLink operationLink, object item, object[] linkedItems)
        {
            return new Action(() =>
            {
                item.GetType().GetMethod(Map.methodSetMapSet).Invoke(item, new object[] { linkedItems[0] });
            });
        }

    }
}
