#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion


using HularionCore.Logic;
using HularionCore.Pattern.Functional;
using HularionCore.Pattern.Topology;
using HularionCore.TypeGraph;
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
    /// A system domain representing a set of items.
    /// </summary>
    /// <typeparam name="T">The generic type of the set.</typeparam>
    [MeshSystemDomainAttribute(UniqueSet.UniqueSet_KeyPartial, "UniqueSet")]
    public class UniqueSet<T>
    {
        /// <summary>
        /// The key of the obejct.
        /// </summary>
        [DomainPropertyAttribute(DomainObjectPropertySelector.Key)]
        public IMeshKey Key { get; set; }

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
        public string SerializedGenerics { get; set; }

        /// <summary>
        /// Gets a HashSet containing the items in the UniqueSet when accessed.
        /// </summary>
        public HashSet<T> HashSet { get { return new HashSet<T>(Items); } }

        internal HashSet<T> Items { get; set; } = new HashSet<T>();
        internal HashSet<IMeshKey> Keys { get; set; } = new HashSet<IMeshKey>();
        internal HashSet<T> NullKeys { get; set; } = new HashSet<T>();
        internal bool NullKeysUpdated { get; set; } = false;


        private DataType itemType;

        private PropertyInfo keyProperty = null;
        private FieldInfo keyField = null;

        private static Type stringType = typeof(string);
        private static Type keyType = typeof(IMeshKey);

        private Func<object, IMeshKey> itemKeyProvider = value => MeshKey.NullKey;
        private Action<T> itemAdder = null;


        public static PropertyInfo GetItemsProperty(Type type)
        {
            var property = type.GetProperty("Items");
            return property;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public UniqueSet()
        {
            Setup();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="items">The items to add.</param>
        public UniqueSet(IEnumerable<T> items)
        {
            Setup();
            if (items == null) { return; }
            AddRange(items);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="items">The items to add.</param>
        public UniqueSet(params T[] items)
        {
            Setup();
            if (items == null) { return; }
            AddRange(items);
        }

        /// <summary>
        /// Adds the given item to the set.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Add(T item)
        {
            if (item == null) { return; }
            itemAdder(item);
        }


        /// <summary>
        /// Adds the the given items.
        /// </summary>
        /// <param name="items">The items to add.</param>
        public void AddRange(IEnumerable<T> items)
        {
            if (items == null) { return; }
            foreach(var item in items)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Adds the items to the set.
        /// </summary>
        /// <param name="items">The items to add.</param>
        public void AddMany(params T[] items)
        {
            AddRange(items);
        }

        /// <summary>
        /// Adds the the given items.
        /// </summary>
        /// <param name="items">The items to add.</param>
        internal void AddObjectRange(IEnumerable<object> items)
        {
            if (items == null) { return; }
            foreach(var item in items)
            {
                if(item == null)
                {
                    Items.Add(default(T));
                }
                else if(itemType != null)
                {
                    Items.Add((T)itemType.Parse(item.ToString()));
                }
                else
                {
                    Items.Add((T)item);
                }
            }
        }

        /// <summary>
        /// Gets the number items in the set.
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            return Items.Count();
        }

        /// <summary>
        /// Determines whether the provided item is contained in the set.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>true iff the item is contained in the set.</returns>
        public bool Contains(T item)
        {
            return Items.Contains(item);
        }

        /// <summary>
        /// Removes the item if it exists.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        public void Remove(T item)
        {
            Items.Remove(item);
        }

        /// <summary>
        /// Clears all items from the set.
        /// </summary>
        public void Clear()
        {
            Items.Clear();
        }

        /// <summary>
        /// Gets an array of items in the set.
        /// </summary>
        /// <returns>The items in the set.</returns>
        public T[] ToArray()
        {
            return Items.ToArray();
        }

        /// <summary>
        /// Get a list of items in the set.
        /// </summary>
        /// <returns>The items in the set.</returns>
        public List<T> ToList()
        {
            return Items.ToList();
        }

        /// <summary>
        /// Gets a list of items in set as type object.
        /// </summary>
        /// <returns>A list of items in set as type object.</returns>
        public List<object> ToObjectList()
        {
            return Items.Select(x => (object)x).ToList();
        }


        private void Setup()
        {
            itemType = DataType.FromCSharpType<T>();
            var proxyType = typeof(DomainPropertyAttribute);
            var type = typeof(T);
            foreach (var property in type.GetNonStaticGetSetProperties())
            {
                var attribute = property.GetCustomAttributes()
                    .Where(x => x.GetType() == proxyType)
                    .Where(x => ((DomainPropertyAttribute)x).Selector == DomainObjectPropertySelector.Key)
                    .FirstOrDefault();
                if (attribute != null)
                {
                    keyProperty = property;
                    break;
                }
            }
            foreach (var field in type.GetNonStaticFields())
            {
                var attribute = field.GetCustomAttributes()
                    .Where(x => x.GetType() == proxyType)
                    .Where(x => ((DomainPropertyAttribute)x).Selector == DomainObjectPropertySelector.Key)
                    .FirstOrDefault();
                if (attribute != null)
                {
                    keyField = field;
                    break;
                }
            }



            if (keyProperty != null)
            {
                itemKeyProvider = value => MeshKey.Parse(keyProperty.GetValue(value));
            }
            else if (keyField != null)
            {
                itemKeyProvider = value => MeshKey.Parse(keyField.GetValue(value));
            }
            else
            {
                if (!DataType.TypeIsKnown(typeof(T)))
                {
                    throw new ArgumentException(String.Format("The type '{0}' does not have a property with the DomainPropertyAttribute for Key or is an unknown type. [iSfhU1OPaEazKu0ltH7DcQ]", typeof(T).Name));
                }
                itemAdder = item =>
                {
                    Items.Add(item);
                };
            }
            if(itemAdder == null)
            {
                itemAdder = item =>
                {
                    var key = itemKeyProvider(item);
                    if (MeshKey.KeyIsNull(key))
                    {
                        NullKeys.Add(item);
                        Items.Add(item);
                        NullKeysUpdated = true;
                        return;
                    }
                    else
                    {
                        if (NullKeysUpdated)
                        {
                            NullKeysUpdated = false;
                            foreach (var value in NullKeys)
                            {
                                var valueKey = itemKeyProvider(value);
                                if (!MeshKey.KeyIsNull(valueKey))
                                {
                                    Keys.Add(valueKey);
                                    NullKeys.Remove(value);
                                }
                            }
                        }
                        lock (Keys)
                        {
                            if (Keys.Contains(key)) { return; }
                            Items.Add(item);
                        }
                    }
                };
            }
        }

    }

    public static class UniqueSet
    {
        /// <summary>
        /// The unique name for a Set.
        /// </summary>
        public const string UniqueSet_KeyPartial = "System_UniqueSet";

        public const string UniqueSetItemsDomainValue = "Items";

        private const string methodToObjectList = "ToObjectList";

        public const string methodAddRange = "AddRange";
        public const string methodAddObjectRange = "AddObjectRange";

        public static Type UniqueSetDomainType = typeof(UniqueSet<>);

        public static Type[] DomainTypes = new Type[] { UniqueSetDomainType };


        public static bool MapsType(Type type)
        {
            if (type == null) { return false; }
            if (!type.IsGenericType) { return false; }
            type = type.GetGenericTypeDefinition();
            if (DomainTypes.Contains(type)) { return true; }
            return false;
        }

        /// <summary>
        /// Converts the object "set" into an IList of object.
        /// </summary>
        /// <param name="set">The object containing the values.</param>
        /// <returns>The values in 'set' as an IList of object.</returns>
        public static IList<object> GetUniqueSetValues(object set)
        {
            if (set == null) { return new List<object>(); }
            return (List<object>)set.GetType().GetMethod(methodToObjectList).Invoke(set, new object[] { });
        }

        public static IList<object> GetDomainObjectValues(DomainObject value)
        {
            if (!value.Values.ContainsKey(MeshKeyword.UniqueSetDomainItems.Alias)) { return new List<object>(); }
            if (value.Values[MeshKeyword.UniqueSetDomainItems.Alias] == null) { return new List<object>(); }
            var items = value.Values[MeshKeyword.UniqueSetDomainItems.Alias];
            if (items == null) { return new List<object>(); }
            var setType = UniqueSetDomainType.MakeGenericType(items.GetType().GetGenericArguments()[0]);
            var set = Activator.CreateInstance(setType, new object[] { items });
            return GetUniqueSetValues(set);
        }

        public static IList<object> GetUpdaterValues(DomainObjectUpdater updater)
        {
            var domainObject = new DomainObject();
            domainObject.Values[MeshKeyword.UniqueSetDomainItems.Alias] = updater.Values[MeshKeyword.UniqueSetDomainItems.Alias];
            return GetDomainObjectValues(domainObject);
        }

        public static object MakeGenericList(Type type, IEnumerable<object> objects)
        {
            var gType = typeof(List<>).MakeGenericType(type);
            var addMethod = gType.GetMethod("Add");
            var result = Activator.CreateInstance(gType);
            var parameter = new object[1];
            foreach(var item in objects)
            {
                parameter[0] = item;
                addMethod.Invoke(result, parameter);
            }
            return result;
        }

        public static IEnumerable<DomainLinkAffectResponse> Link(MeshRepository repository, IMeshKey uniqueSetKey, params IMeshKey[] itemKeys)
        {
            if(itemKeys.Length == 0)
            {
                return new List<DomainLinkAffectResponse>();
            }

            var uniqueSetDomain = repository.DomainByKeyProvider.Provide(uniqueSetKey);
            var itemDomain = repository.DomainByKeyProvider.Provide(itemKeys.First());
            var linkService = repository.GetDomainLinkService(uniqueSetDomain, itemDomain);

            var affects = new List<DomainLinkAffectRequest>();
            foreach (var itemKey in itemKeys)
            {
                affects.Add(new DomainLinkAffectRequest()
                {
                    Mode = LinkAffectMode.LinkWhere,
                    WhereA = WhereExpressionNode.CreateKeysIn(uniqueSetKey),
                    WhereB = WhereExpressionNode.CreateKeysIn(itemKey),
                    DomainA = uniqueSetDomain,
                    DomainB = itemDomain,
                    AMember = UniqueSet.UniqueSetItemsDomainValue
                });
            }
            var result = linkService.AffectProcessor.Process(affects.ToArray());
            return result;
        }

        public static IEnumerable<DomainLinkAffectResponse> Unlink(MeshRepository repository, IMeshKey uniqueSetKey, params IMeshKey[] itemKeys)
        {
            if(itemKeys.Length == 0)
            {
                return new List<DomainLinkAffectResponse>();
            }

            var uniqueSetDomain = repository.DomainByKeyProvider.Provide(uniqueSetKey);
            var itemDomain = repository.DomainByKeyProvider.Provide(itemKeys.First());
            var linkService = repository.GetDomainLinkService(uniqueSetDomain, itemDomain);

            var affects = new List<DomainLinkAffectRequest>();
            foreach (var itemKey in itemKeys)
            {
                affects.Add(new DomainLinkAffectRequest()
                {
                    Mode = LinkAffectMode.UnlinkWhere,
                    WhereA = WhereExpressionNode.CreateKeysIn(uniqueSetKey),
                    WhereB = WhereExpressionNode.CreateKeysIn(itemKey),
                    DomainA = uniqueSetDomain,
                    DomainB = itemDomain,
                    AMember = UniqueSet.UniqueSetItemsDomainValue
                });
            }
            var result = linkService.AffectProcessor.Process(affects.ToArray());
            return result;
        }

    }

    /// <summary>
    /// The repository mechanic for the Set sytem domain.
    /// </summary>
    public class UniqueSetDomainRepositoryMechanic : IDomainRepositoryMechanic
    {

        public IEnumerable<Type> MappedTypes { get { return UniqueSet.DomainTypes; } }

        public void InitializeProperties()
        {
            //Nothing to do here.
        }

        public bool CanCreateTypeManifest(Type type)
        {
            return UniqueSet.MapsType(type);
        }

        public TypeManifest CreateTypeManifest(Type type)
        {
            if (!type.IsGenericType || !CanCreateTypeManifest(type) || type.GetGenericTypeDefinition() != UniqueSet.UniqueSetDomainType) { return null; }

            var manifestor = new TypeManifest();
            var genericType = type.GetGenericArguments()[0];
            var newSetType = UniqueSet.UniqueSetDomainType.MakeGenericType(genericType);
            var propertyAssigners = TypeManager.GetMemberAssigners(type);
            var reversePropertyAssigners = TypeManager.GetReverseMemberAssigners(type);

            manifestor.ToTypeObject = new Func<DomainObject, object>(domainObject =>
            {
                object set = Activator.CreateInstance(newSetType, new object[] { });
                if (domainObject.Values.ContainsKey(UniqueSet.UniqueSetItemsDomainValue))
                {
                    set.GetType().GetMethod(UniqueSet.methodAddRange).Invoke(set, new object[] { domainObject.Values[UniqueSet.UniqueSetItemsDomainValue] });
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
                if (DataType.TypeIsKnown(genericType))
                {
                    result.Values.Add(UniqueSet.UniqueSetItemsDomainValue, newSetType.GetMethod("ToList").Invoke(value, new object[] { }));
                }
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
            if (type.IsArray) { type = UniqueSet.UniqueSetDomainType.MakeGenericType(type.GetElementType()); }
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

        /// <summary>
        /// Sets up the DomainOperationLink for the system domain in the repository.
        /// </summary>
        /// <param name="repository">The repository in which to setup the link.</param>
        /// <param name="link">The link to setup (i.e. add sublinks, etcetera).</param>
        public void SetupOperationLink(IMeshRepository repository, DomainOperationLink link, IDictionary<IMeshKey, DomainOperationLink> nodeLinks)
        {
            var setGeneric = link.RealizedDomain.GenericsArguments[link.RealizedDomain.Domain.GenericsParameters[0].Name].Clone();
            setGeneric.Name = link.RealizedDomain.Domain.GenericsParameters[0].Name;
            setGeneric.Mode = TypeGenericMode.Argument;
            if (!DataType.TypeIsKnown(setGeneric.Key))
            {
                var nextDomain = repository.GetDomainFromKey(setGeneric.Key);
                for (var i = 0; i < nextDomain.GenericsParameters.Count(); i++)
                {
                    setGeneric.Generics[i].Name = nextDomain.GenericsParameters[i].Name;
                }
                var realizedDomain = nextDomain.CreateRealizedMeshDomain(setGeneric.Generics.ToArray(), repository.DomainByKeyProvider);
                DomainOperationLink typeLink;
                if (nodeLinks.ContainsKey(realizedDomain.Key)) { typeLink = nodeLinks[realizedDomain.Key]; }
                else
                {
                    typeLink = new DomainOperationLink()
                    {
                        RealizedDomain = nextDomain.CreateRealizedMeshDomain(setGeneric.Generics.ToArray(), repository.DomainByKeyProvider),
                        Parent = link,
                        NodeType = DomainOperationMode.Domain
                    };
                }

                if (link.SourceType.IsGenericType) { typeLink.SourceType = link.SourceType.GetGenericArguments().First(); }
                if (link.SourceType.IsArray) { typeLink.SourceType = link.SourceType.GetElementType(); }

                var memberLink = new DomainOperationLink()
                {
                    MemberName = UniqueSet.UniqueSetItemsDomainValue,
                    Member = typeLink,
                    Parent = link,
                    NodeType = DomainOperationMode.Link,
                    SourceType = typeLink.SourceType
                };
                typeLink.Parent = memberLink;
                link.Members.Add(UniqueSet.UniqueSetItemsDomainValue, memberLink);
            }
            link.IsSystemDomain = true;
        }

        /// <summary>
        /// Sets up the save link for the  system domain in the repository.
        /// </summary>
        /// <param name="repository">The repository in which to setup the link.</param>
        /// <param name="link">The link to setup (i.e. add sublinks, etcetera).</param>
        public void SetupSaveLink(IMeshRepository repository, SaveLink link)
        {
            if (link.DomainObject == null || link.DomainObject.Key == null) { link.DomainObject = DomainObject.Derive(link.Value); }
            link.DomainObject.Meta[MeshKeyword.Generics.Alias] = link.OperationLink.RealizedDomain.SerializedGenerics;

            var sourceType = link.Value.GetType();
            var valueType = sourceType.GetGenericArguments().First();
            var values = UniqueSet.GetUniqueSetValues(link.Value);

            if (DataType.TypeIsKnown(valueType)) { return; }
            var valueDomain = repository.GetDomainFromType(valueType);

            //var subjectDomain = repository.GetDomainFromType(valueType);
            //var linkDomain = repository.GetDomainFromType(valueType);
            var currentKeys = new List<IMeshKey>();
            if (link.DomainObject != null && link.DomainObject.Key != null)
            {
                var domainRead = new AggregateQueryItem()
                {
                    Mode = AggregateDomainMode.Domain,
                    Domain = link.OperationLink.RealizedDomain.Domain,
                    DomainWhere = WhereExpressionNode.CreateKeysIn(link.DomainObject.Key)
                };
                var linkRead = new AggregateQueryItem()
                {
                    Mode = AggregateDomainMode.Link,
                    LinkWhere = WhereExpressionNode.CreateMemberIn(MeshKeyword.SMember.Alias, new string[] { UniqueSet.UniqueSetItemsDomainValue })
                        .CombineWithOperator(WhereExpressionNode.CreateMemberIn(MeshKeyword.TMember.Alias, new string[] { UniqueSet.UniqueSetItemsDomainValue }), BinaryOperator.OR)
                };
                var linkedRead = new AggregateQueryItem()
                {
                    Mode = AggregateDomainMode.Domain,
                    Domain = valueDomain
                };
                domainRead.Links.Add(linkRead);
                linkRead.Links.Add(linkedRead);
                var aggregateService = repository.MeshServicesProvider.AggregateServiceProvider.Provide();
                var linkresult = aggregateService.QueryProcessor.Process(new DomainAggregateQueryRequest() { Read = domainRead });

                if (linkresult.Links.Count > 0)
                {
                    currentKeys = linkresult.Links.First().ToKeys.ToList();
                }
            }

            var updateLinkKeys = new List<IMeshKey>();

            foreach (var value in values)
            {
                var next = link.LinkProvider.Provide(value);
                var domainObject = DomainObject.Derive(value);
                if (next == null)
                {
                    next = new SaveLink() 
                    { 
                        Parent = link, 
                        DomainObject = domainObject, 
                        OperationLink = link.OperationLink.Members[UniqueSet.UniqueSetItemsDomainValue].Member, 
                        Value = value,
                        LinkProvider = link.LinkProvider
                    };
                    link.Added.Add(next);
                }
                if (domainObject.Key != null)
                {
                    updateLinkKeys.Add(next.DomainObject.Key);
                }
                link.Links.Add(next);
            }

            var removeLinkKeys = currentKeys.Where(x => !updateLinkKeys.Contains(x)).ToList();


            link.LinkAffectorProvider = new ProviderFunction<AggregateAffectorItem[]>(() =>
            {
                var affectors = new List<AggregateAffectorItem>();
                foreach (var next in link.Links)
                {
                    if (next.DomainObject.Key != null && currentKeys.Contains(next.DomainObject.Key)) { continue; }
                    affectors.Add(new AggregateAffectorItem()
                    {
                        Link = new DomainLinkAffectRequest()
                        {
                            Mode = LinkAffectMode.LinkKeys,
                            DomainA = link.OperationLink.RealizedDomain.Domain,
                            DomainB = next.OperationLink.RealizedDomain.Domain,
                            AMember = UniqueSet.UniqueSetItemsDomainValue,
                            ObjectAKey = link.DomainObject.Key,
                            ObjectBKey = next.DomainObject.Key,
                            LinkIsExclusive = false
                        }
                    });
                }
                foreach(var key in removeLinkKeys)
                {
                    affectors.Add(new AggregateAffectorItem()
                    {
                        Link = new DomainLinkAffectRequest()
                        {
                            Mode = LinkAffectMode.UnlinkKeys,
                            DomainA = link.OperationLink.RealizedDomain.Domain,
                            DomainB = valueDomain,
                            AMember = UniqueSet.UniqueSetItemsDomainValue,
                            ObjectAKey = link.DomainObject.Key,
                            ObjectBKey = key
                        }
                    });
                }
                return affectors.ToArray();
            });
        }


        public Action LinkResults(QueriedLink queriedLink, DomainOperationLink operationLink, object item, object[] linkedItems)
        {
            var sourceType = item.GetType();
            var addMethod = sourceType.GetMethod("Add");
            foreach (var toItem in linkedItems)
            {
                addMethod.Invoke(item, new object[] { toItem });
            }
            return new Action(() => { });
        }

    }
}
