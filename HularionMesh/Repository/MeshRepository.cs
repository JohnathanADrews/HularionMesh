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
using HularionMesh.Domain;
using HularionMesh.DomainAggregate;
using HularionMesh.DomainLink;
using HularionMesh.DomainValue;
using HularionMesh.MeshType;
using HularionMesh.Standard;
using HularionMesh.Structure;
using HularionMesh.SystemDomain;
using HularionMesh;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HularionCore.Pattern.Functional;
using HularionCore.Pattern.Topology;
using HularionMesh.User;
using HularionMesh.Query;
using HularionCore.System;

namespace HularionMesh.Repository
{
    /// <summary>
    /// A repository for registering domains and interacting with a mesh service.
    /// </summary>
    public class MeshRepository : IMeshRepository
    {
        /// <summary>
        /// The unique key assigned to this repository which should be equivalent to a data source key used by a client.
        /// </summary>
        public string Key { get; private set; } = MeshKey.CreateUniqueTag();

        /// <summary>
        /// The mesh mechanism that will provide the data services whether file, database, cloud, etcetera.
        /// </summary>
        public IMeshServiceProvider MeshServicesProvider { get; private set; }

        /// <summary>
        /// Manages the domain types an primitive types.
        /// </summary>
        public TypedDomainRegistrar TypeRegistrar { get; private set; } = new TypedDomainRegistrar();

        /// <summary>
        /// Stores and manages the domain mechanics for the mesh domains. this repository sets up the system domains. Use this to add/replace mechanic providers.
        /// </summary>
        public DomainMechanicManager MechanicManager { get; private set; } = new DomainMechanicManager();

        /// <summary>
        /// Provides the domain mechanic give the mesh domain. Set this to overrie the DomainMechanicManager's DomainMechaincProvider provider.
        /// </summary>
        public IParameterizedProvider<MeshDomain, IDomainRepositoryMechanic> DomainMechanicProvider { get; private set; }

        /// <summary>
        /// Provides a domain key given a type.
        /// </summary>
        public IParameterizedProvider<Type, IMeshKey> TypeKeyProvider { get; private set; }

        /// <summary>
        /// Provides a DomainOperationLink given a Type.
        /// </summary>
        public IParameterizedProvider<Type, DomainOperationLink> TypeOperationLinkProvider { get; private set; }

        /// <summary>
        /// Provides a domain given a domain key.
        /// </summary>
        public IParameterizedProvider<IMeshKey, MeshDomain> DomainByKeyProvider { get; private set; }

        private Dictionary<Type, DomainOperationLink> typeOperationLinks { get; set; } = new Dictionary<Type, DomainOperationLink>();
        private Dictionary<string, MeshDomain> domainNameMap = new Dictionary<string, MeshDomain>();
        private Dictionary<string, MeshDomain> domainPartialKeyMap = new Dictionary<string, MeshDomain>();
        private Dictionary<string, MeshDomain> domainFullKeyMap = new Dictionary<string, MeshDomain>();
        private Dictionary<string, MeshDomain> domainAliasMap = new Dictionary<string, MeshDomain>();

        /// <summary>
        /// The domain for the system Set.
        /// </summary>
        public MeshDomain SetDomain { get; private set; }
        /// <summary>
        /// The domain for the system KeyedValue.
        /// </summary>
        public MeshDomain KeyedValueDomain { get; private set; }
        /// <summary>
        /// The domain for the system Map.
        /// </summary>
        public MeshDomain MapDomain { get; private set; }

        private Dictionary<IMeshKey, MeshDomain> systemDomains { get; set; } = new Dictionary<IMeshKey, MeshDomain>();


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="meshServicesProvider">The mesh services provider that this repository will use ot process requests.</param>
        public MeshRepository(IMeshServiceProvider meshServicesProvider)
        {
            MeshServicesProvider = meshServicesProvider;
            TypeKeyProvider = ParameterizedProvider.FromSingle<Type, IMeshKey>(type =>
            {
                if (DataType.TypeIsKnown(type)) { return DataType.FromCSharpType(type).Key; }
                var domain = TypeRegistrar.GetDomainFromType(type);
                if (domain != null) { return domain.Key; }
                return null;
            });
            DomainByKeyProvider = ParameterizedProvider.FromSingle<IMeshKey, MeshDomain>(domainKey =>
            {
                if(domainKey == null) { return null; }
                return TypeRegistrar.GetDomain(domainKey.GetDomainKeyPart());
            });

            var assembly = Assembly.GetExecutingAssembly();
            var assemblyDetail = new AssemblyRegistrationDetail() { InitializeDomainProperties = false };
            assemblyDetail.Assemblies.Add(assembly);
            assemblyDetail.SetRegistrationCheckerFromAttributes<MeshSystemDomainAttribute>();
            assemblyDetail.UniqueNameProvider = ParameterizedProvider.FromSingle<Type, string>(type =>
            {
                var attribute = type.GetTypeInfo().GetCustomAttributes().Where(x => x.GetType() == typeof(MeshSystemDomainAttribute)).FirstOrDefault();
                if (attribute == null) { return null; }
                return ((MeshSystemDomainAttribute)attribute).Key;
            });
            assemblyDetail.ValuesProvider = ParameterizedProvider.FromSingle<Type, IDictionary<string, object>>(type =>
            {
                return new Dictionary<string, object>();
            });

            RegisterAssemblies(assemblyDetail);


            SetDomain = GetDomainFromType(typeof(UniqueSet<>));
            KeyedValueDomain = GetDomainFromType(typeof(KeyedValue<,>));
            MapDomain = GetDomainFromType(typeof(Map<,>));

            systemDomains.Add(SetDomain.Key, SetDomain);
            systemDomains.Add(KeyedValueDomain.Key, KeyedValueDomain);
            systemDomains.Add(MapDomain.Key, MapDomain);

            DomainMechanicProvider = MechanicManager.DomainMechanicProvider;

            var setDomainMechanic = new UniqueSetDomainRepositoryMechanic();
            foreach (var type in setDomainMechanic.MappedTypes) { TypeRegistrar.RegisterDomainType(type, SetDomain); }
            MechanicManager.SetMechanic(SetDomain, setDomainMechanic);

            var mapDomainMechanic = new MapDomainRepositoryMechanic();
            foreach (var type in mapDomainMechanic.MappedTypes) { TypeRegistrar.RegisterDomainType(type, MapDomain); }
            MechanicManager.SetMechanic(MapDomain, mapDomainMechanic);

            TypeOperationLinkProvider = ParameterizedProvider.FromSingle<Type, DomainOperationLink>(type =>
            {
                if (type == null) { return null; }
                return typeOperationLinks.LockAddGet(type, () =>
                {
                    var domain = TypeRegistrar.GetDomainFromType(type);
                    var mechanic = DomainMechanicProvider.Provide(domain);
                    RealizedMeshDomain realizedDomain = null;
                    if (mechanic != null) { realizedDomain = mechanic.CreateRealizedDomainFromType(this, type); }
                    else { realizedDomain = CreateRealizedDomainFromType(type); }
                    return DomainOperationLink.CreateDomainOperationLink(this, realizedDomain, sourceType: type);
                });
            });

            //Initialize here so that the domain mechanics are setup.
            TypeRegistrar.InitializeProperties(DomainMechanicProvider);

        }

        /// <summary>
        /// Gets the domain service for the domain associated with the provided type.
        /// </summary>
        /// <typeparam name="DomainType">The type associated with the domain.</typeparam>
        /// <returns>The domain service for the domain associated with the provided type.</returns>
        public IDomainValueService GetDomainService<DomainType>()
        {
            return GetDomainService(typeof(DomainType));
        }

        /// <summary>
        /// Gets the domain service for the domain associated with the provided type.
        /// </summary>
        /// <param name="domainType">The type associated with the domain.</param>
        /// <returns>The domain service for the domain associated with the provided type.</returns>
        public IDomainValueService GetDomainService(Type domainType)
        {
            var domain = TypeRegistrar.GetDomain(domainType);
            return MeshServicesProvider.DomainServiceCommunicator.ValuesServiceByDomainProvider.Provide(domain).Response;
        }

        /// <summary>
        /// Returns true iff the domain has been registered and false otherwise.
        /// </summary>
        /// <param name="name">The name of the domain</param>
        /// <returns>true iff the domain has been registered and false otherwise.</returns>
        public bool HasDomainByName(string name)
        {
            name = name.Replace(".", string.Empty);
            return domainNameMap.ContainsKey(name);
        }

        /// <summary>
        /// Returns true iff the domain has been registered and false otherwise.
        /// </summary>
        /// <param name="key">The key of the domain</param>
        /// <returns>true iff the domain has been registered and false otherwise.</returns>
        public bool HasDomainByKey(string key)
        {
            if(key == null) { return false; }
            return domainPartialKeyMap.ContainsKey(key);
        }

        /// <summary>
        /// Returns the domain with th eprovided name.
        /// </summary>
        /// <param name="name">The name of the domain.</param>
        /// <returns>The domain with th eprovided name.</returns>
        public MeshDomain GetDomainFromName(string name)
        {
            name = name.Replace(".", string.Empty);
            if (!domainNameMap.ContainsKey(name))
            {
                throw new ArgumentException(String.Format("The provided domain name, {0}, is not associated to any domain ANg6l3NNu0uBMx9W0j9Z8w.", name));
            }
            return domainNameMap[name];
        }

        /// <summary>
        /// Returns the domain with the provided key.
        /// </summary>
        /// <param name="key">The key of the domain.</param>
        /// <returns>The domain with the provided key.</returns>
        public MeshDomain GetDomainFromKey(string key)
        {
            if (!domainPartialKeyMap.ContainsKey(key))
            {
                throw new ArgumentException(String.Format("The provided domain key, {0}, is not associated to any domain b3IV4R0Bn0amXfSwi3hFdg.", key));
            }
            return domainPartialKeyMap[key];
        }

        /// <summary>
        /// Returns the domain with the provided key.
        /// </summary>
        /// <param name="key">The key of the domain.</param>
        /// <returns>The domain with the provided key.</returns>
        public MeshDomain GetDomainFromKey(IMeshKey key)
        {
            return GetDomainFromKey(key.Serialized);
        }

        /// <summary>
        /// Gets the domain with the given name or key.  Returns null if not found.
        /// </summary>
        /// <param name="identifier">The name or key of the domain.</param>
        /// <returns>The domain with the given name or null if not found.</returns>
        public MeshDomain GetDomainFromNameOrKey(string identifier)
        {
            if (HasDomainByName(identifier)) { return GetDomainFromName(identifier); }
            if (domainAliasMap.ContainsKey(identifier)) { return domainAliasMap[identifier]; }
            if (domainPartialKeyMap.ContainsKey(identifier)) { return GetDomainFromKey(identifier); }
            if (domainFullKeyMap.ContainsKey(identifier)) { return domainFullKeyMap[identifier]; }
            return null;
        }

        /// <summary>
        /// Gets the domain with the associated type.
        /// </summary>
        /// <typeparam name="DomainType">The type of the domain to get.</typeparam>
        /// <returns>The domain with the associated type.</returns>
        public MeshDomain GetDomainFromType<DomainType>()
        {
            return TypeRegistrar.GetDomainFromType<DomainType>();
        }

        /// <summary>
        /// Gets the domain with the associated type.
        /// </summary>
        /// <param name="domainType">The type of the domain to get.</param>
        /// <returns>The domain with the associated type.</returns>
        public MeshDomain GetDomainFromType(Type domainType)
        {
            return TypeRegistrar.GetDomainFromType(domainType);
        }

        /// <summary>
        /// Gets the realized domain given the domain type.
        /// </summary>
        /// <typeparam name="DomainType">The type of the domain.</typeparam>
        /// <returns>The realized domain given the domain type.</returns>
        public RealizedMeshDomain CreateRealizedDomainFromType<DomainType>()
        {
            return CreateRealizedDomainFromType(typeof(DomainType));
        }

        /// <summary>
        /// Gets the realized domain given the domain type.
        /// </summary>
        /// <param name="domainType">The type of the domain.</param>
        /// <returns>The realized domain given the domain type.</returns>
        public RealizedMeshDomain CreateRealizedDomainFromType(Type domainType)
        {
            var generics = MeshGeneric.FromType(domainType, TypeKeyProvider);
            var edomain = TypeRegistrar.GetDomainFromType(domainType).CreateRealizedMeshDomain(generics, DomainByKeyProvider);
            return edomain;
        }

        /// <summary>
        /// Returns all of the domains registered with this repository.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<MeshDomain> GetDomains()
        {
            return TypeRegistrar.Domains;
        }

        /// <summary>
        /// Get the link service linking domains associated to each of the types.
        /// </summary>
        /// <typeparam name="DomainAType">One of the types associated to a domain in the link.</typeparam>
        /// <typeparam name="DomainBType">One of the types associated to a domain in the link.</typeparam>
        /// <returns>The link service linking domains associated to each of the types.</returns>
        public IDomainLinkService GetDomainLinkService<DomainAType, DomainBType>()
        {
            return GetDomainLinkService(typeof(DomainAType), typeof(DomainBType));
        }

        /// <summary>
        /// Get the link service linking domains associated to each of the types.
        /// </summary>
        /// <param name="typeA">One of the types associated to a domain in the link.</param>
        /// <param name="typeB">One of the types associated to a domain in the link.</param>
        /// <returns>The link service linking domains associated to each of the types.</returns>
        public IDomainLinkService GetDomainLinkService(Type typeA, Type typeB)
        {
            var domainA = TypeRegistrar.GetDomain(typeA);
            var domainB = TypeRegistrar.GetDomain(typeB);
            if (domainA == null || domainB == null) { return null; }
            //return MeshServicesProvider.DynamicProvider.DomainLinkServiceProvider.Provide(new LinkedDomains() { DomainA = domainA, DomainB = domainB });
            return MeshServicesProvider.DomainServiceCommunicator.LinkServiceByLinkedDomainsProvider.Provide(new LinkedDomains() { DomainA = domainA, DomainB = domainB }).Response;
        }

        /// <summary>
        /// Get the link service linking the domains.
        /// </summary>
        /// <param name="domainA">One of the domains in the link.</param>
        /// <param name="domainB">One of the domains in the link.</param>
        /// <returns>The link service linking the domains.</returns>
        public IDomainLinkService GetDomainLinkService(MeshDomain domainA, MeshDomain domainB)
        {
            if (domainA == null || domainB == null) { return null; }
            //return MeshServicesProvider.DynamicProvider.DomainLinkServiceProvider.Provide(new LinkedDomains() { DomainA = domainA, DomainB = domainB });
            return MeshServicesProvider.DomainServiceCommunicator.LinkServiceByLinkedDomainsProvider.Provide(new LinkedDomains() { DomainA = domainA, DomainB = domainB }).Response;
        }

        /// <summary>
        /// Registers the type using the provided domain.
        /// </summary>
        /// <param name="domainType">The type to register.</param>
        /// <param name="domain">The domain corresponding to the type.</param>
        public void RegisterDomainType(Type domainType, MeshDomain domain)
        {
            if (domainType.IsGenericType)
            {
                //Need to use this circuitious path to get the generic parameters since it does nto work for typeof(Set<T>) types. The domainType works directly if taken from the assembly, however.
                foreach (var generic in domainType.GetGenericTypeDefinition().UnderlyingSystemType.GetTypeInfo().GenericTypeParameters)
                {
                    domain.GenericsParameters.Add(new MeshGeneric() { Name = generic.Name, Mode = TypeGenericMode.Parameter });
                }
            }

            TypeRegistrar.RegisterDomainType(domainType, domain);
            //MeshServicesProvider.DynamicProvider.DomainRegistrar.Process(domain);
            //MeshServicesProvider.DomainServiceCommunicator.DomainCreator.Process(domain);
            domainNameMap.Add(domainType.Name, domain);
            domainPartialKeyMap.Add(domain.Key.Serialized, domain);
            domainFullKeyMap.Add(domain.UniqueName, domain);
        }

        /// <summary>
        /// Registers the assemblies specified in the registration detail object.
        /// </summary>
        /// <param name="detail">The details used for registering the assembly.</param>
        public void RegisterAssemblies(AssemblyRegistrationDetail detail)
        {
            foreach (var assembly in new HashSet<Assembly>(detail.Assemblies))
            {
                foreach (var type in assembly.ManifestModule.Assembly.DefinedTypes)
                {
                    if (!detail.RegistrationChecker.Provide(type)) { continue; }
                    var uniqueName = detail.UniqueNameProvider.Provide(type);
                    if (uniqueName == null) { throw new Exception(String.Format("The type {0} is declared as a domain type but is not providing a unique name. [T1nEstxrZ06gfiN82GpVtg]", type.Name)); }                    
                    var domain = new MeshDomain() { UniqueName = uniqueName };
                    if(detail.ValuesProvider != null)
                    {
                        var values = detail.ValuesProvider.Provide(type);
                        if (values != null) { domain.Values = values; }
                    }
                    RegisterDomainType(type, domain);
                    if(detail.AliasesProvider != null)
                    {
                        RegisterDomainAliases(domain, detail.AliasesProvider.Provide(type));
                    }
                }
            }

            if (detail.InitializeDomainProperties) { TypeRegistrar.InitializeProperties(DomainMechanicProvider); }
            foreach(var domain in TypeRegistrar.Domains)
            {
                MeshServicesProvider.DomainServiceCommunicator.DomainCreator.Process(domain);
            }
        }

        private void RegisterDomainAliases(MeshDomain domain, IEnumerable<string> aliases)
        {
            foreach(var alias in aliases)
            {
                if (domainAliasMap.ContainsKey(alias)) { throw new ArgumentException(String.Format("Domain alias {0} cannot be assigned to {1} because it is already assigned to {2}. [XDbpRb2BECOwtyRdDqOUw] ", alias, domain.Key, domainAliasMap[alias].Key)); }
                domainAliasMap.Add(alias, domain);
            }
        }

        /// <summary>
        /// Returns all the data types that could be members within a domain.
        /// </summary>
        /// <returns>All the data types that could be members within a domain.</returns>
        //public IEnumerable<HularionType> GetBaseDataTypes()
        //{
        //    var types = DataType.GetDataTypes().Select(x => x.GetHularionType()).ToList();
        //    return types;
        //}

        /// <summary>
        /// Returns all the values in the domain with the associated type.
        /// </summary>
        /// <typeparam name="T">The type associated to the domain.</typeparam>
        /// <returns>All the values in the domain with the associated type.</returns>
        public IEnumerable<T> GetValues<T>(WhereExpressionNode where = null , DomainReadRequest read = null)
            where T : class, new()
        {
            if (where == null) { where = WhereExpressionNode.ReadAll; }
            if (read == null) { read = DomainReadRequest.ReadAll; }
            var domain = GetDomainFromType<T>();
            var query = DomainValueQueryRequest.CreateQueryRequest(where, read);
            //var response = MeshServicesProvider.DynamicProvider.DomainValueServiceProvider.Provide(domain).QueryProcessor.Process(query);
            var response = MeshServicesProvider.DomainServiceCommunicator.ValuesServiceByDomainProvider.Provide(domain).Response.QueryProcessor.Process(query);
            return response.Values.Select(x => x.Manifest<T>()).ToList();
        }

        /// <summary>
        /// Creates the domain objects of type T.
        /// </summary>
        /// <typeparam name="T">The type associated with the domain in which the objects are created.</typeparam>
        /// <param name="userProfile">The key of the user creating the objects.</param>
        /// <param name="count">The number of objects to create.</param>
        /// <returns>The created domain objects.</returns>
        public IEnumerable<DomainObject> CreateDomainObjects<T>(UserProfile userProfile, int count)
            where T : class, new()
        {
            var domain = TypeRegistrar.GetDomain<T>();
            //var domain = ExtendedMeshDomain.FromType<T>(TypeKeyProvider);
            //var extendedDomain = domain.CreateExtendedMeshDomain(TypeGeneric.FromType<T>(TypeKeyProvider));
            //domain.Generics
            return CreateDomainObjects(userProfile, domain, MeshGeneric.FromType<T>(TypeKeyProvider), count);
        }

        /// <summary>
        /// Creates the domain objects of type T.
        /// </summary>
        /// <param name="userProfile">The key of the user creating the objects.</param>
        /// <param name="domain">The domain of the value to create.</param>
        /// <param name="count">The number of objects to create.</param>
        /// <returns>The created domain objects.</returns>
        public IEnumerable<DomainObject> CreateDomainObjects(UserProfile userProfile, MeshDomain domain, MeshGeneric[] generics, int count)
        {
            var creates = new List<IDomainValueAffectItem>();
            for (var i = 0; i < count; i++)
            {
                creates.Add(new DomainValueAffectCreate() { Value = new DomainObject(), Generics = generics });
            }
            var meshAffect = new DomainValueAffectRequest() { RequestTime = DateTime.UtcNow, Affected = creates.ToArray(), Reads = DomainReadRequest.ReadAll };
            meshAffect.UserKey = userProfile.UserKey;
            //var meshResponse = MeshServicesProvider.DynamicProvider.DomainValueServiceProvider.Provide(domain).AffectProcessor.Process(meshAffect);
            //var providerResponse = MeshServicesProvider.DomainServiceCommunicator.ValuesServiceByDomainProvider.Provide(domain);
            var meshResponse = MeshServicesProvider.DomainServiceCommunicator.ValuesServiceByDomainProvider.Provide(domain).Response.AffectProcessor.Process(meshAffect);
            return meshResponse.Reads;
        }

        /// <summary>
        /// Creates the domain objects of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="userProfile">The key of the user creating the objects.</param>
        /// <param name="count">The number of objects to create.</param>
        /// <returns>The created domain objects manifested as type T</returns>
        public IEnumerable<T> CreateValues<T>(UserProfile userProfile, int count)
            where T : class, new()
        {
            return CreateDomainObjects<T>(userProfile, count).Select(x => x.Manifest<T>()).ToList();
        }

        /// <summary>
        /// Creates objects of type T and links them to an object with key linkKey.
        /// </summary>
        /// <typeparam name="T">The type to create.</typeparam>
        /// <param name="userProfile">The key of the user creating the objects.</param>
        /// <param name="linkKey">The key of the object to link to the new objects.</param>
        /// <param name="count">The number of objects to create.</param>
        /// <param name="linkMember">The name of the member in LinkType that references the created T type objects.</param>
        /// <param name="linkBackMember">The name of the member in the created T type objects that reference back to the LinkType object.</param>
        /// <returns></returns>
        public IEnumerable<T> CreateAndLinkValues<T>(UserProfile userProfile, IMeshKey linkKey, int count, string linkMember = null, string linkBackMember = null)
            where T : class, new()
        {
            var objects = CreateDomainObjects<T>(userProfile, count);

            var tDomain = GetDomainFromType<T>();
            var tProperty = tDomain.Properties.Where(x => x.Name == linkMember).FirstOrDefault();
            var linkDomain = GetDomainFromKey(linkKey.GetKeyPart(MeshKeyPart.Domain));
            var linkProperty = linkDomain.Properties.Where(x => x.Name == linkMember).FirstOrDefault();

            var linkService = GetDomainLinkService(tDomain, linkDomain);

            var linkKeys = new IMeshKey[] { linkKey };

            if (linkProperty != null)
            {
                var key = MeshKey.Parse(linkProperty.Type);
                if(key == SetDomain.Key)
                {
                    var coll = GetCollection();
                    var linkQuery = new DomainLinkQueryRequest()
                    {
                        SubjectDomain = linkDomain,
                        SubjectWhere = WhereExpressionNode.CreateKeysIn(linkKey),
                        LinkWhere = WhereExpressionNode.ReadAll,
                        Mode = LinkQueryRequestMode.LinkedKeys
                    };
                    var linkSetService = GetDomainLinkService(SetDomain, linkDomain);
                    var linkedResult = linkSetService.QueryProcessor.Process(linkQuery);
                    IMeshKey setKey = null;
                    if (linkedResult.Members.ContainsKey(linkMember) && linkedResult.Members != null)
                    {
                        setKey = (IMeshKey)linkedResult.LinkedKeys[linkedResult.Members[linkMember].First()].First();
                        //setKey = (IMeshKey)linkedResult.Members[linkMember].First();
                    }
                    else
                    {
                        var set = CreateDomainObjects(userProfile, SetDomain, MeshGeneric.FromType<T>(TypeKeyProvider), 1);
                        setKey = set.First().Key;
                    }
                    //var items = CreateValues<T>(userKey, count).ToList();
                    LinkObjectsByKey(new IMeshKey[] { setKey }, objects.Select(x => x.Key).ToList());
                    if (linkBackMember != null) { LinkTypeObjects(objects.Select(x => x.Key).ToList(), linkKeys, memberOfContainingValue: linkBackMember); }
                    //return objects.Select(x => x.Manifest<T>()).ToList();
                }
                else
                {
                    //
                }
            }
            else
            {
                LinkObjectsByKey(linkKeys, objects.Select(x => x.Key).ToList(), memberOfContainingValue: linkMember, memberOfContainedValue: linkBackMember);
            }


            //var linkService = GetDomainLinkService(tDomain, linkDomain);
            //var linkRequest = new DomainLinkAffectRequest() { UserKey = userKey, DomainA = linkDomain, DomainB = GetDomainFromType<T>(), Mode = LinkAffectMode.Link };

            //linkRequest.WhereA = WhereExpressionNode.CreateKeysIn(DomainObject.Derive<LinkType>(link).Key);
            //linkRequest.WhereB = WhereExpressionNode.CreateKeysIn(objects.Select(x => x.Key).ToArray());
            //linkRequest.AMember = linkMember;
            //linkRequest.BMember = linkBackMember;
            //linkService.AffectProcessor.Process(linkRequest);

            return objects.Select(x => x.Manifest<T>()).ToList();
        }

        /// <summary>
        /// Inserts the existing values of type T.
        /// </summary>
        /// <typeparam name="T">The type of the values to insert.</typeparam>
        /// <param name="userKey">The key of the user creating the objects.</param>
        /// <param name="objects">The objects to insert.</param>
        public void InsertValues<T>(IMeshKey userKey, params T[] objects)
        {
            var domain = GetDomainFromType<T>();
            var inserts = new List<IDomainValueAffectItem>();
            for (var i = 0; i < objects.Length; i++)
            {
                inserts.Add(new DomainValueAffectInsert() { Value = DomainObject.Derive(objects[i]) });
            }
            var meshAffect = new DomainValueAffectRequest() { RequestTime = DateTime.UtcNow, Affected = inserts.ToArray(), Reads = DomainReadRequest.ReadNone };
            meshAffect.UserKey = userKey;
            //MeshServicesProvider.DynamicProvider.DomainValueServiceProvider.Provide(domain).AffectProcessor.Process(meshAffect);
            MeshServicesProvider.DomainServiceCommunicator.ValuesServiceByDomainProvider.Provide(domain).Response.AffectProcessor.Process(meshAffect);
        }

        /// <summary>
        /// Updates the values of type T, updating all members within the domain objects, with the assumption that they all changed.
        /// </summary>
        /// <typeparam name="T">The type of the values to update.</typeparam>
        /// <param name="userProfile">The key of the user updating the objects.</param>
        /// <param name="objects">The objects to update.</param>
        public void UpdateValues<T>(UserProfile userProfile, params T[] objects)
        {
            var domain = GetDomainFromType<T>();
            var updates = new List<IDomainValueAffectItem>();
            for (var i = 0; i < objects.Length; i++)
            {
                updates.Add(new DomainValueAffectUpdate() { Updater = DomainObjectUpdater.Derive(objects[i]) });
            }
            var meshAffect = new DomainValueAffectRequest() { RequestTime = DateTime.UtcNow, Affected = updates.ToArray(), Reads = DomainReadRequest.ReadNone };
            meshAffect.UserKey = userProfile.UserKey;
            //MeshServicesProvider.DynamicProvider.DomainValueServiceProvider.Provide(domain).AffectProcessor.Process(meshAffect);
            MeshServicesProvider.DomainServiceCommunicator.ValuesServiceByDomainProvider.Provide(domain).Response.AffectProcessor.Process(meshAffect);
        }

        /// <summary>
        /// Saves the provided objects.
        /// </summary>
        /// <typeparam name="T">The type of objects to save.</typeparam>
        /// <param name="objects">The objects to save.</param>
        public void Save<T>(UserProfile userProfile, params T[] objects)
        {
            Save(userProfile, objects.Select(x => (object)x).ToArray());
        }

        /// <summary>
        /// Saves the provided objects.
        /// </summary>
        /// <param name="userProfile">The user saving the objects.</param>
        /// <param name="objects">The objects to save.</param>
        public void Save(UserProfile userProfile, params object[] objects)
        {
            var savePlan = SaveLink.CreateSaveLinks(this, objects.Select(x => (object)x).ToArray());

            //Figure out which objects exist to determine whether to insert or update.
            var checkKeys = savePlan.Where(x => x.DomainObject.Key != null && x.DomainObject.Key != MeshKey.NullKey).Select(x => x.DomainObject.Key).ToList();
            var insertKeys = new HashSet<IMeshKey>();
            var domainKeys = new Dictionary<IMeshKey, List<IMeshKey>>();
            foreach(var saveLink in savePlan)
            {
                if (saveLink.DomainObject.Key == null || saveLink.DomainObject.Key == MeshKey.NullKey) { continue; }
                var domainKey = saveLink.DomainObject.Key.GetDomainKeyPart();
                if (!domainKeys.ContainsKey(domainKey)) { domainKeys[domainKey] = new List<IMeshKey>(); }
                domainKeys[domainKey].Add(saveLink.DomainObject.Key);
            }
            foreach(var domainSet in domainKeys)
            {
                var domainService = MeshServicesProvider.DomainServiceCommunicator.ValueServiceByDomainKeyProvider.Provide(domainSet.Key).Response;
                var domainResponse = domainService.QueryProcessor.Process(new DomainValueQueryRequest() { Reads = DomainReadRequest.ReadKeys, Where = WhereExpressionNode.CreateKeysIn(domainSet.Value) });
                var existing = new HashSet<IMeshKey>(domainResponse.Values.Select(x => x.Key));
                foreach(var key in domainSet.Value)
                {
                    if (!existing.Contains(key)) { insertKeys.Add(key); }
                }
            }

            var requests = new List<AggregateAffectorItem>();
            for (var i = 0; i < savePlan.Length; i++)
            {
                var saveLink = savePlan[i];
                if (saveLink.DomainObject.Key == null || saveLink.DomainObject.Key == MeshKey.NullKey)
                {
                    //Prune off the domain values corresponding to links, but only if there is no mechanic for the domain.
                    var mechanic = DomainMechanicProvider.Provide(saveLink.OperationLink.RealizedDomain.Domain);
                    if (mechanic == null) { saveLink.OperationLink.RealizedDomain.Prune(saveLink.DomainObject); }
                    var creator = new DomainValueAffectCreate()
                    {
                        Value = saveLink.DomainObject,
                        ResponseProcessor = ParameterizedVoidFacade.FromSingle<DomainValueAffectResponse>(value =>
                        {
                            saveLink.DomainObject.Update(saveLink.Value);
                        })
                    };
                    requests.Add(new AggregateAffectorItem() { Domain = saveLink.OperationLink.RealizedDomain.Domain, Creator = creator });
                }
                else if (insertKeys.Contains(saveLink.DomainObject.Key))
                {
                    requests.Add(new AggregateAffectorItem() { Domain = saveLink.OperationLink.RealizedDomain.Domain, Inserter = new DomainValueAffectInsert() { Value = saveLink.DomainObject } });
                }
                else
                {
                    requests.Add(new AggregateAffectorItem() { Domain = saveLink.OperationLink.RealizedDomain.Domain, Updater = new DomainValueAffectUpdate() { Updater = new DomainObjectUpdater(saveLink.DomainObject) } });
                }
            }
            var meshAffect = new DomainAggregateAffectorRequest() { UserKey = userProfile.UserKey, Items = requests.ToArray() };
            var service = this.MeshServicesProvider.AggregateServiceProvider.Provide();
            var meshResponse = service.AffectProcessor.Process(meshAffect);

            requests = new List<AggregateAffectorItem>();
            for (var i = 0; i < savePlan.Length; i++)
            {
                var item = savePlan[i];
                requests.AddRange(item.LinkAffectorProvider.Provide());
            }
            meshAffect = new DomainAggregateAffectorRequest() { Items = requests.ToArray() };
            service = this.MeshServicesProvider.AggregateServiceProvider.Provide();
            meshResponse = service.AffectProcessor.Process(meshAffect);
        }

        public void InsertDomainObjects(UserProfile userprofile, params DomainObject[] domainObjects)
        {
            var domains = new Dictionary<MeshDomain, List<DomainObject>>();
            foreach (var domainObject in domainObjects)
            {
                if (MeshKey.KeyIsNull(domainObject.Key)) { continue; }
                var domain = DomainByKeyProvider.Provide(domainObject.Key);
                if (!domains.ContainsKey(domain)) { domains.Add(domain, new List<DomainObject>()); }
                domains[domain].Add(domainObject);
            }

            foreach (var domainPair in domains)
            {
                var service = MeshServicesProvider.AggregateServiceProvider.Provide();
                var affectors = domainPair.Value.Select(x => new AggregateAffectorItem() { Domain = domainPair.Key, Inserter = new DomainValueAffectInsert() { Value = x } }).ToArray();
                service.AffectProcessor.Process(new DomainAggregateAffectorRequest() { Items = affectors });
            }


            //var service = MeshServicesProvider.AggregateServiceProvider.Provide();
            //var affectors = domainObjects.Select(x => new AggregateAffectorItem() { Inserter = new DomainValueAffectInsert() { Value = x } }).ToArray();
            //service.AffectProcessor.Process(new DomainAggregateAffectorRequest() { Items = affectors });

        }

        /// <summary>
        /// Delets the objects with the specified keys.
        /// </summary>
        /// <param name="userProfile">The user deleting the objects.</param>
        /// <param name="objectKeys">The keys of the objects to delete.</param>
        public void Delete(UserProfile userprofile, params IMeshKey[] objectKeys)
        {
            var deleteKeys = objectKeys.Where(x => x != null && x!= MeshKey.NullKey).ToList();
            var domainKeys = new Dictionary<IMeshKey, List<IMeshKey>>();
            foreach (var key in deleteKeys)
            {
                var domainKey = key.GetDomainKeyPart();
                if (!domainKeys.ContainsKey(domainKey)) { domainKeys[domainKey] = new List<IMeshKey>(); }
                domainKeys[domainKey].Add(key);
            }

            var linkDomains = MeshServicesProvider.DomainServiceCommunicator.AllLinkDomainsProvider.Provide().Response;
            foreach (var linkDomain in linkDomains)
            {
                if (domainKeys.ContainsKey(linkDomain.DomainA.Key))
                {
                    var linkService = MeshServicesProvider.DomainServiceCommunicator.LinkServiceByLinkedDomainsProvider.Provide(linkDomain.GetLinkedDomains()).Response;
                    var linkQueryResponse = linkService.QueryProcessor.Process(new DomainLinkQueryRequest() 
                    { 
                        SubjectDomain = linkDomain.DomainA, 
                        SubjectWhere = WhereExpressionNode.CreateKeysIn(domainKeys[linkDomain.DomainA.Key]), 
                        Mode = LinkQueryRequestMode.LinkKeys, 
                        LinkedWhere = WhereExpressionNode.ReadAll 
                    });
                    var affects = new List<DomainLinkAffectRequest>();
                    foreach(var linkKey in linkQueryResponse.LinkedKeys)
                    {
                        if (linkKey.Value.Count() == 0) { continue; }
                        affects.Add(new DomainLinkAffectRequest()
                        {
                            Mode = LinkAffectMode.UnlinkWhere,
                            WhereA = WhereExpressionNode.CreateKeysIn(linkKey.Key),
                            WhereB = WhereExpressionNode.CreateKeysIn(linkKey.Value),
                            DomainA = linkDomain.DomainA,
                            DomainB = linkDomain.DomainB
                        });
                    }
                    linkService.AffectProcessor.Process(affects.ToArray());
                }
                if (domainKeys.ContainsKey(linkDomain.DomainB.Key))
                {
                    var linkService = MeshServicesProvider.DomainServiceCommunicator.LinkServiceByLinkedDomainsProvider.Provide(linkDomain.GetLinkedDomains()).Response;
                    var linkQueryResponse = linkService.QueryProcessor.Process(new DomainLinkQueryRequest()
                    {
                        SubjectDomain = linkDomain.DomainB,
                        SubjectWhere = WhereExpressionNode.CreateKeysIn(domainKeys[linkDomain.DomainB.Key]),
                        Mode = LinkQueryRequestMode.LinkKeys,
                        LinkedWhere = WhereExpressionNode.ReadAll
                    });
                    var affects = new List<DomainLinkAffectRequest>();
                    foreach (var linkKey in linkQueryResponse.LinkedKeys)
                    {
                        if(linkKey.Value.Count() == 0) { continue; }
                        affects.Add(new DomainLinkAffectRequest()
                        {
                            Mode = LinkAffectMode.UnlinkWhere,
                            WhereA = WhereExpressionNode.CreateKeysIn(linkKey.Key),
                            WhereB = WhereExpressionNode.CreateKeysIn(linkKey.Value),
                            DomainA = linkDomain.DomainB,
                            DomainB = linkDomain.DomainA
                        });
                    }
                    linkService.AffectProcessor.Process(affects.ToArray());
                }
            }

            foreach (var domainSet in domainKeys)
            {
                var domainService = MeshServicesProvider.DomainServiceCommunicator.ValueServiceByDomainKeyProvider.Provide(domainSet.Key).Response;
                domainService.AffectProcessor.Process(new DomainValueAffectRequest() { Affected = new IDomainValueAffectItem[] { new DomainValueAffectDelete() { Where = WhereExpressionNode.CreateKeysIn(domainSet.Value) } } });
            }

        }


        /// <summary>
        /// Creates a query item tree given the type.
        /// </summary>
        /// <param name="type">The type from which the query item tree will be created.</param>
        /// <returns>The root query item.</returns>
        public AggregateQueryItem CreateQueryItem(Type type)
        {
            //Create the operation link given the type.
            var opLink = TypeOperationLinkProvider.Provide(type);
            var traverser = new TreeTraverser<DomainOperationLink>();

            //Create the query.
            var queryPlan = new Dictionary<DomainOperationLink, AggregateQueryItem>();
            var opLinks = new HashSet<DomainOperationLink>();
            var plan = traverser.CreateUniqueNodeEvaluationPlan(TreeTraversalOrder.LeftRightParent, opLink,
                node =>
                {
                    if (node.NodeType == DomainOperationMode.Link) { return new DomainOperationLink[] { node.Member }; }
                    return node.Members.Values.ToArray();
                }, true);

            for (var i = 0; i < plan.Length; i++)
            {
                queryPlan.Add(plan[i], new AggregateQueryItem());
            }
            for (var i = 0; i < plan.Length; i++)
            {
                var link = plan[i];
                var query = queryPlan[link];

                if (link.NodeType == DomainOperationMode.Domain)
                {
                    query.Mode = AggregateDomainMode.Domain;
                    query.Domain = link.RealizedDomain.Domain;
                    query.Reads = DomainReadRequest.ReadAll;
                    query.Mode = AggregateDomainMode.Domain;
                    query.Alias = link.MemberName;
                }

                if (link.NodeType == DomainOperationMode.Link)
                {
                    query.Mode = AggregateDomainMode.Link;
                    query.Alias = link.MemberName;
                    query.Domain = link.Member.RealizedDomain.Domain;
                    var parent = queryPlan[link.Parent];
                    var member = queryPlan[link.Member];
                    parent.Links.Add(query);
                    query.Links.Add(member);

                    var linkService = GetDomainLinkService(link.Member.RealizedDomain.Domain, link.Parent.RealizedDomain.Domain);
                    if (!String.IsNullOrWhiteSpace(link.MemberName))
                    {
                        var linkWhere = new WhereExpressionNode();
                        if (linkService.STypeDomain == link.Parent.RealizedDomain.Domain) { linkWhere.Property = MeshKeyword.SMember.Name; }
                        else { linkWhere.Property = MeshKeyword.TMember.Name; }
                        linkWhere.Type = DataType.Text8;
                        linkWhere.Value = link.MemberName;
                        linkWhere.Comparison = DataTypeComparison.Equal;
                        if (linkService.STypeDomain == linkService.TTypeDomain)
                        {
                            query.LinkKeyMatchMode = LinkKeyMatchMode.SKey;
                        }
                        query.LinkWhere = linkWhere;
                    }
                }
            }
            var queryRoot = queryPlan[opLink];
            return queryRoot;
        }

        /// <summary>
        /// Queries this repository for values of type T and its members.
        /// </summary>
        /// <typeparam name="T">The type to query.</typeparam>
        /// <param name="keys">The keys for values of type T.</param>
        /// <returns>The query response with rendered values of type T, including their members.</returns>
        public RepositoryQueryResponse<T> QueryTree<T>(params IMeshKey[] keys)
            where T : class
        {
            var response = new RepositoryQueryResponse<T>();
            QueryTree(response, typeof(T), keys);
            return response;
        }

        /// <summary>
        /// Queries this repository for values of type T and its members.
        /// </summary>
        /// <typeparam name="T">The type to query.</typeparam>
        /// <param name="where">Determines which domain objects to read.</param>
        /// <returns>The query response with rendered values of type T, including their members.</returns>
        public RepositoryQueryResponse<T> QueryTree<T>(WhereExpressionNode where)
            where T : class
        {
            var domain = TypeRegistrar.GetDomain<T>();
            //If generic, make sure the generic types are the same.
            if (domain.IsGeneric)
            {
                var generics = MeshGeneric.FromType<T>(TypeRegistrar.TypeKeyProvider);
                var serialized = MeshGeneric.SerializeGenerics(generics);
                var genericWhere = new WhereExpressionNode() { Mode = WhereExpressionNodeValueMode.Meta, Value = serialized, Comparison = DataTypeComparison.Equal, Property = MeshKeyword.Generics.Alias };
                where = where.CombineWithOperator(genericWhere, BinaryOperator.AND);
            }
            //Get the root keys.
            var request = new DomainAggregateQueryRequest() { Read = new AggregateQueryItem() { Domain = TypeRegistrar.GetDomain<T>(), RecurrenceRootWhere = where, Reads = DomainReadRequest.ReadKeys, Mode = AggregateDomainMode.Domain } };
            var service = MeshServicesProvider.AggregateServiceProvider.Provide();
            var queryResult = service.QueryProcessor.Process(request);

            var response = new RepositoryQueryResponse<T>();
            QueryTree(response, typeof(T), queryResult.Result.Items.Select(x=>x.Key).ToArray());
            return response;
        }

        /// <summary>
        /// Queries this repository for values of the given type and its members.
        /// </summary>
        /// <param name="type">The type to query.</param>
        /// <param name="keys">The keys for values of type T.</param>
        /// <returns>The query response with rendered values the given type, including their members.</returns>
        public RepositoryQueryResponse QueryTree(Type type, params IMeshKey[] keys)
        {
            var response = new RepositoryQueryResponse(type);
            QueryTree(response, type, keys);
            return response;
        }

        /// <summary>
        /// Queries this repository for values of type T and its members.
        /// </summary>
        /// <param name="response">The response in which to add the results.</param>
        /// <param name="type">The type to query.</param>
        /// <param name="keys">The keys for values of type T.</param>
        /// <returns>The rendered values of type T, including their members.</returns>
        private void QueryTree(RepositoryQueryResponse response, Type type, params IMeshKey[] keys)
        {

            var queryRoot = CreateQueryItem(type);

            //Create the operation link given the type.
            var opLink = TypeOperationLinkProvider.Provide(type);           
            queryRoot.RecurrenceRootWhere = WhereExpressionNode.CreateKeysIn(keys);

            //Execute the query.
            var request = new DomainAggregateQueryRequest() { Read = queryRoot };
            var service = MeshServicesProvider.AggregateServiceProvider.Provide();
            var queryResult = service.QueryProcessor.Process(request);



            var resultObjects = queryResult.Objects.ToDictionary(x => x.Key, x => x);
            var rootObjects = new HashSet<DomainObject>(queryResult.RootKeys.Select(x => resultObjects[x]).ToList());
            var objectLinks = new Dictionary<DomainObject, Dictionary<string, List<DomainObject>>>();
            var doTraverser = new TreeTraverser<DomainObject>();
            var resultOpLinks = new Dictionary<IMeshKey, DomainOperationLink>();
            var memberOpLinks = new Dictionary<IMeshKey, DomainOperationLink>();


            foreach (var item in queryResult.Objects) { objectLinks.Add(item, new Dictionary<string, List<DomainObject>>()); }
            foreach (var queryLink in queryResult.Links)
            {
                foreach (var subjectKey in queryLink.FromKeys)
                {
                    var item = resultObjects[subjectKey];
                    var ol = objectLinks[item];
                    if (!ol.ContainsKey(queryLink.Alias)) { ol.Add(queryLink.Alias, new List<DomainObject>()); }
                    ol[queryLink.Alias].AddRange(queryLink.ToKeys.Select(x => resultObjects[x]).ToList());
                }
            }

            //Match each item to its operation link.
            var processedLinks = new HashSet<DomainObject>();
            foreach (var root in rootObjects)
            {
                resultOpLinks[root.Key] = opLink;

                processedLinks.Add(root);
                doTraverser.CreateEvaluationPlan(TreeTraversalOrder.ParentLeftRight, root, node =>
                {
                    var nodeOpLink = resultOpLinks[node.Key];
                    var next = new List<DomainObject>();
                    foreach(var pair in objectLinks[node])
                    {
                        foreach(var item in pair.Value)
                        {
                            if (!processedLinks.Contains(item)) 
                            {
                                next.Add(item);
                                processedLinks.Add(item);
                            }
                        }
                        var memberOpLink = nodeOpLink.Members[pair.Key];
                        foreach (var nextItem in objectLinks[node][pair.Key]) 
                        { 
                            resultOpLinks[nextItem.Key] = memberOpLink.Member;
                            memberOpLinks[nextItem.Key] = memberOpLink;
                        }
                    }
                    return next.ToArray();
                }, true);
            }

            //Now that we have enough information to Manifest the values, manifest them.
            var resultItems = new Dictionary<IMeshKey, object>();
            var roots = new List<object>();
            foreach (var item in queryResult.Objects)
            {
                var domain = TypeRegistrar.GetDomain(item.Key.GetKeyPart(MeshKeyPart.Domain));
                var generics = new MeshGeneric[] { };
                if (domain.IsGeneric) { generics = MeshGeneric.Deserialize((string)item.Meta[MeshKeyword.Generics.Alias]); }
                var extendedDomain = domain.CreateRealizedMeshDomain(generics, DomainByKeyProvider);
                object itemValue;
                //if (memberOpLinks.ContainsKey(item.Key)) { itemValue = item.Manifest(memberOpLinks[item.Key].SourceType); }
                //else { itemValue = item.Manifest(resultOpLinks[item.Key].SourceType); }
                itemValue = item.Manifest(resultOpLinks[item.Key].SourceType);
                var st = resultOpLinks[item.Key].SourceType;
                item.Manifest(st);
                resultItems.Add(item.Key, itemValue);
            }

            var followUps = new List<Action>();
            foreach (var queryLink in queryResult.Links)
            {
                foreach (var subjectKey in queryLink.FromKeys)
                {
                    var itemOpLink = resultOpLinks[subjectKey];
                    var subjectItem = resultItems[subjectKey];
                    DomainOperationLink memberOpLink = null;
                    if (memberOpLinks.ContainsKey(subjectKey)) { memberOpLink = memberOpLinks[subjectKey]; }
                    var mechanic = DomainMechanicProvider.Provide(itemOpLink.RealizedDomain.Domain);

                    if (mechanic == null)
                    {
                        var toKey = queryLink.ToKeys.First();
                        var toItem = resultItems[toKey];
                        if (itemOpLink.Members[queryLink.Alias].MemberProperty != null)
                        {
                            itemOpLink.Members[queryLink.Alias].MemberProperty.SetValue(subjectItem, toItem);
                        }
                        else if (itemOpLink.Members[queryLink.Alias].MemberField != null)
                        {
                            itemOpLink.Members[queryLink.Alias].MemberField.SetValue(subjectItem, toItem);
                        }
                    }
                    else
                    {
                        var followUp = mechanic.LinkResults(queryLink, itemOpLink, subjectItem, queryLink.ToKeys.Select(x=>resultItems[x]).ToArray());
                        if(followUp != null) { followUps.Add(followUp); }
                    }
                }
            }
            foreach (var followUp in followUps) { followUp(); }

            foreach (var key in keys)
            {
                if (queryResult.RootKeys.Contains(key)) { response.AddResultItem(resultItems[key]); }
            }
        }

        /// <summary>
        /// Queries the repository using the provided AggregateQueryItem.
        /// </summary>
        /// <typeparam name="T">The type to which to cast the result.</typeparam>
        /// <param name="queryRoot">The root AggregateQueryItem of a query tree.</param>
        /// <returns>The requested objects.</returns>
        public RepositoryQueryResponse<T> Query<T>(AggregateQueryItem queryRoot)
            where T : class
        {
            var response = new RepositoryQueryResponse<T>();
            Query(response, typeof(T), queryRoot);
            return response;
        }

        /// <summary>
        /// Queries the repository using the provided AggregateQueryItem.
        /// </summary>
        /// <typeparam name="T">The type to which to cast the result.</typeparam>
        /// <param name="queryRoot">The root AggregateQueryItem of a query tree.</param>
        /// <returns>The requested objects.</returns>
        public void Query(RepositoryQueryResponse response, Type type, AggregateQueryItem queryRoot)
        {
            var opLink = TypeOperationLinkProvider.Provide(type);

            //Execute the query.
            var request = new DomainAggregateQueryRequest() { Read = queryRoot };
            var service = MeshServicesProvider.AggregateServiceProvider.Provide();
            var queryResult = service.QueryProcessor.Process(request);

            var resultObjects = queryResult.Objects.ToDictionary(x => x.Key, x => x);
            var rootObjects = new HashSet<DomainObject>(queryResult.RootKeys.Select(x => resultObjects[x]).ToList());
            var objectLinks = new Dictionary<DomainObject, Dictionary<string, List<DomainObject>>>();
            var doTraverser = new TreeTraverser<DomainObject>();
            var resultOpLinks = new Dictionary<IMeshKey, DomainOperationLink>();
            var memberOpLinks = new Dictionary<IMeshKey, DomainOperationLink>();

            foreach (var item in queryResult.Objects) { objectLinks.Add(item, new Dictionary<string, List<DomainObject>>()); }
            foreach (var queryLink in queryResult.Links)
            {
                foreach (var subjectKey in queryLink.FromKeys)
                {
                    var item = resultObjects[subjectKey];
                    var ol = objectLinks[item];
                    if (!ol.ContainsKey(queryLink.Alias)) { ol.Add(queryLink.Alias, new List<DomainObject>()); }
                    ol[queryLink.Alias].AddRange(queryLink.ToKeys.Select(x => resultObjects[x]).ToList());
                }
            }

            //Match each item to its operation link.
            var processedLinks = new HashSet<DomainObject>();
            foreach (var root in rootObjects)
            {
                resultOpLinks[root.Key] = opLink;

                processedLinks.Add(root);
                doTraverser.CreateEvaluationPlan(TreeTraversalOrder.ParentLeftRight, root, node =>
                {
                    var nodeOpLink = resultOpLinks[node.Key];
                    var next = new List<DomainObject>();
                    foreach (var pair in objectLinks[node])
                    {
                        foreach (var item in pair.Value)
                        {
                            if (!processedLinks.Contains(item))
                            {
                                next.Add(item);
                                processedLinks.Add(item);
                            }
                        }
                        var memberOpLink = nodeOpLink.Members[pair.Key];
                        foreach (var nextItem in objectLinks[node][pair.Key])
                        {
                            resultOpLinks[nextItem.Key] = memberOpLink.Member;
                            memberOpLinks[nextItem.Key] = memberOpLink;
                        }
                    }
                    return next.ToArray();
                }, true);
            }

            //Now that we have enough information to Manifest the values, manifest them.
            var resultItems = new Dictionary<IMeshKey, object>();
            var roots = new List<object>();
            foreach (var item in queryResult.Objects)
            {
                var domain = TypeRegistrar.GetDomain(item.Key.GetKeyPart(MeshKeyPart.Domain));
                var generics = new MeshGeneric[] { };
                if (domain.IsGeneric) { generics = MeshGeneric.Deserialize((string)item.Meta[MeshKeyword.Generics.Alias]); }
                var extendedDomain = domain.CreateRealizedMeshDomain(generics, DomainByKeyProvider);
                object itemValue;
                itemValue = item.Manifest(resultOpLinks[item.Key].SourceType);
                var st = resultOpLinks[item.Key].SourceType;
                item.Manifest(st);
                resultItems.Add(item.Key, itemValue);
            }

            var followUps = new List<Action>();
            foreach (var queryLink in queryResult.Links)
            {
                foreach (var subjectKey in queryLink.FromKeys)
                {
                    var itemOpLink = resultOpLinks[subjectKey];
                    var subjectItem = resultItems[subjectKey];
                    DomainOperationLink memberOpLink = null;
                    if (memberOpLinks.ContainsKey(subjectKey)) { memberOpLink = memberOpLinks[subjectKey]; }
                    var mechanic = DomainMechanicProvider.Provide(itemOpLink.RealizedDomain.Domain);

                    if (mechanic == null)
                    {
                        var toKey = queryLink.ToKeys.First();
                        var toItem = resultItems[toKey];
                        if (itemOpLink.Members[queryLink.Alias].MemberProperty != null)
                        {
                            itemOpLink.Members[queryLink.Alias].MemberProperty.SetValue(subjectItem, toItem);
                        }
                        else if (itemOpLink.Members[queryLink.Alias].MemberField != null)
                        {
                            itemOpLink.Members[queryLink.Alias].MemberField.SetValue(subjectItem, toItem);
                        }
                    }
                    else
                    {
                        var followUp = mechanic.LinkResults(queryLink, itemOpLink, subjectItem, queryLink.ToKeys.Select(x => resultItems[x]).ToArray());
                        if (followUp != null) { followUps.Add(followUp); }
                    }
                }
            }
            foreach (var followUp in followUps) { followUp(); }


            foreach (var key in queryResult.RootKeys)
            {
                response.AddResultItem(resultItems[key]);
            }
        }

        /// <summary>
        /// Gets the number of items in a domain matching the provided where clause.
        /// </summary>
        /// <typeparam name="T">The type corresponding to the domain.</typeparam>
        /// <param name="user">The user making the request.</param>
        /// <param name="where">Determines which objects to count.</param>
        /// <returns>The number of matching objects.</returns>
        public long Count<T>(UserProfile user, WhereExpressionNode where = null)
        {
            if(where == null) { where = WhereExpressionNode.ReadAll; }
            var domain = TypeRegistrar.GetDomainFromType<T>();
            var domainService = MeshServicesProvider.DomainServiceCommunicator.ValueServiceByDomainKeyProvider.Provide(domain.Key).Response;
            var countResponse = domainService.QueryProcessor.Process(new DomainValueQueryRequest() { UserKey = user.UserKey, Where = where, Reads = new DomainReadRequest() { Mode = DomainReadRequestMode.Count } });
            return countResponse.Count;
        }

        /// <summary>
        /// Sets the values for an AggregateQueryItem linking two other AggregateQueryItem(s).
        /// </summary>
        /// <param name="parentDomain">The parent domain in the context of the query.</param>
        /// <param name="childDomain">The child domain in the context of the query.</param>
        /// <param name="memberName">The name of the member of parentDomain which refers to childDomain.</param>
        /// <param name="linkQueryItem"></param>
        public void SetLinkMember(MeshDomain parentDomain, MeshDomain childDomain, string memberName, AggregateQueryItem linkQueryItem)
        {
            var linkService = GetDomainLinkService(childDomain, parentDomain);
            if (!String.IsNullOrWhiteSpace(memberName))
            {
                var linkWhere = new WhereExpressionNode();
                if (linkService.STypeDomain == parentDomain) { linkWhere.Property = MeshKeyword.SMember.Name; }
                else { linkWhere.Property = MeshKeyword.TMember.Name; }
                linkWhere.Type = DataType.Text8;
                linkWhere.Value = memberName;
                linkWhere.Comparison = DataTypeComparison.Equal;
                if (linkService.STypeDomain == linkService.TTypeDomain)
                {
                    linkQueryItem.LinkKeyMatchMode = LinkKeyMatchMode.SKey;
                }
                linkQueryItem.LinkWhere = linkWhere;
            }
        }

        /// <summary>
        /// Creates a mesh query that can be used to query the mesh.
        /// </summary>
        /// <typeparam name="DomainType">The type of the domain to use as the root query.</typeparam>
        /// <returns>The mesh query for the domain associated with the given type.</returns>
        public MeshRootQuery<DomainType> CreateQuery<DomainType>()
            where DomainType : class
        {
            var result = new MeshRootQuery<DomainType>(this);
            return result;
        }

        /// <summary>
        /// Links valueA to valueB on the memberOfA member of A
        /// </summary>
        /// <param name="containingValue">The containing member to link.</param>
        /// <param name="containedValue">The contained member to link.</param>
        /// <param name="memberOfContainingValue">The name of the member to link to.</param>
        public void LinkTypeObjects<Type1, Type2>(Type1 containingValue, Type2 containedValue, string memberOfContainingValue)
        {
            if (containingValue == null || containedValue == null) { throw new Exception("The provided values cannot be null. [lzrw4lNPlE6SgoP7rw8Oig]"); }
            //if (String.IsNullOrWhiteSpace(memberOfContainingValue)) { throw new Exception("The indicated member name cannot be null. [DjId2pkPFEuEWZXfGBXFiA]"); }
            var containingType = containingValue.GetType();
            var containedType = containedValue.GetType();

            var linkService = GetDomainLinkService(containingType, containedType);
            if (linkService == null) { throw new Exception(String.Format("The link service for types {0} and {1} could not be found. [7ajpK6ge20e8gMCCpxSX7Q]", containingType.Name, containedType.Name)); }

            var linkRequest = new DomainLinkAffectRequest()
            {
                DomainA = TypeRegistrar.GetDomain(containingType),
                DomainB = TypeRegistrar.GetDomain(containedType),
                AMember = memberOfContainingValue,
                WhereA = WhereExpressionNode.CreateKeysIn(DomainObject.Derive(containingValue).Key),
                WhereB = WhereExpressionNode.CreateKeysIn(DomainObject.Derive(containedValue).Key),
                Mode = LinkAffectMode.LinkWhere
            };
            var linkResponse = linkService.AffectProcessor.Process(linkRequest);

        }

        /// <summary>
        /// Links the two domain objects on the member of the contained object.
        /// </summary>
        /// <param name="containingValue">The containing member to link.</param>
        /// <param name="containedValues">The contained member to link. They must all have the same domain.</param>
        /// <param name="memberOfContainingValue">The name of the member to link to.</param>
        public DomainLinkAffectResponse Link(DomainObject containingValue, IEnumerable<DomainObject> containedValues, string memberOfContainingValue)
        {
            if (containingValue == null || containedValues == null || containedValues.Count() == 0) { throw new Exception("The provided values cannot be null. [PG419SU2tkKkhV3rjpdTFg]"); }

            var domainA = GetDomainFromKey(containingValue.Key.GetKeyPart(MeshKeyPart.Domain));
            var domainB = GetDomainFromKey(containedValues.First().Key.GetKeyPart(MeshKeyPart.Domain));

            var linkService = GetDomainLinkService(domainA, domainB);
            if (linkService == null) { throw new Exception(String.Format("The link service for domains {0} and {1} could not be found. [osB0aM3LRkGoytfI8P54fg]", domainA.Key, domainB.Key)); }

            var linkRequest = new DomainLinkAffectRequest()
            {
                DomainA = domainA,
                DomainB = domainB,
                AMember = memberOfContainingValue,
                WhereA = WhereExpressionNode.CreateKeysIn(containingValue.Key),
                WhereB = WhereExpressionNode.CreateKeysIn(containedValues.Select(x => x.Key).ToList()),
                Mode = LinkAffectMode.LinkWhere
            };
            var linkResponse = linkService.AffectProcessor.Process(linkRequest);
            return linkResponse;
        }

        /// <summary>
        /// Links the two domain objects on the member of the contained object.
        /// </summary>
        /// <param name="containingValueKeys">The key of containing member to link. They must all have the same domain.</param>
        /// <param name="containedValueKeys">The keys of the contained to link. They must all have the same domain.</param>
        /// <param name="memberOfContainingValue">The name of the member to link to.</param>
        /// <param name="memberOfContainedValue">>The name of the member to link back to.</param>
        public DomainLinkAffectResponse LinkObjectsByKey(IEnumerable<IMeshKey> containingValueKeys, IEnumerable<IMeshKey> containedValueKeys, string memberOfContainingValue = null, string memberOfContainedValue = null)
        {
            if (containingValueKeys == null || containingValueKeys.Count() == 0 || containedValueKeys == null || containedValueKeys.Count() == 0) { throw new Exception("The provided values cannot be null. [8r1DgM4QXESJqxB8kqJvfg]"); }

            var domainA = GetDomainFromKey(containingValueKeys.First().GetKeyPart(MeshKeyPart.Domain));
            var domainB = GetDomainFromKey(containedValueKeys.First().GetKeyPart(MeshKeyPart.Domain));

            var linkService = GetDomainLinkService(domainA, domainB);
            if (linkService == null) { throw new Exception(String.Format("The link service for domains {0} and {1} could not be found. [66h4bdD5BkOQblBntJCrqQ]", domainA.Key, domainB.Key)); }

            var linkRequest = new DomainLinkAffectRequest()
            {
                DomainA = domainA,
                DomainB = domainB,
                AMember = memberOfContainingValue,
                WhereA = WhereExpressionNode.CreateKeysIn(containingValueKeys),
                WhereB = WhereExpressionNode.CreateKeysIn(containedValueKeys),
                Mode = LinkAffectMode.LinkWhere
            };
            var linkResponse = linkService.AffectProcessor.Process(linkRequest);
            return linkResponse;
        }


        /// <summary>
        /// Gets the collection of domain information from this repository.
        /// </summary>
        /// <returns>The collection of domain information from this repository.</returns>
        public RepositoryCollection GetCollection()
        {
            var collection = new RepositoryCollection();
            collection.Domains = MeshServicesProvider.DomainServiceCommunicator.AllValueDomainsProvider.Provide().Response.ToList();
            var objects = new List<DomainObject>();
            foreach (var domain in collection.Domains)
            {
                var service = MeshServicesProvider.DomainServiceCommunicator.ValuesServiceByDomainProvider.Provide(domain).Response;
                var reads = service.QueryProcessor.Process(new DomainValueQueryRequest() { Reads = DomainReadRequest.ReadAll, Where = WhereExpressionNode.ReadAll });
                objects.AddRange(reads.Values);
            }
            collection.Objects = objects;

            collection.LinkDomains = MeshServicesProvider.DomainServiceCommunicator.AllLinkDomainsProvider.Provide().Response.ToList();

            var links = new List<DomainLinker>();
            var linkServices = new HashSet<IDomainLinkService>(collection.LinkDomains.Select(x => MeshServicesProvider.DomainServiceCommunicator.LinkServiceByLinkedDomainsProvider.Provide(x.GetLinkedDomains()).Response));
            foreach (var service in linkServices)
            {
                links.AddRange(service.AllLinksProvider.Provide());
            }
            collection.Links = links.Distinct().ToList();
            return collection;
        }

        /// <summary>
        /// Queries the repository for domain objects in the provided domain and adds them to the collection.
        /// </summary>
        /// <param name="domain">The domain to which the where expression applies.</param>
        /// <param name="where">Determines which objects to retrieve.</param>
        /// <param name="current"></param>
        /// <returns></returns>
        public void QueryToExport(MeshDomain domain, WhereExpressionNode where, bool queryTree = true, RepositoryCollection current = null)
        {

        }

        public ImportCollection ExportQuery<T>(WhereExpressionNode where, ImportCollection collection = null)
        {
            if(collection == null) { collection = new ImportCollection(); }

            var queryRoot = CreateQueryItem(typeof(T));
            queryRoot.RecurrenceRootWhere = where;

            //Execute the query.
            var request = new DomainAggregateQueryRequest() { Read = queryRoot };
            var service = MeshServicesProvider.AggregateServiceProvider.Provide();
            var queryResult = service.QueryProcessor.Process(request);

            collection.ImportQueryResponse(queryResult);

            return collection;
        }

        public void ImportCollection(UserProfile userProfile, ImportCollection collection)
        {
            InsertDomainObjects(userProfile, collection.Objects.Values.ToArray());
            foreach (var link in collection.Links)
            {
                LinkObjectsByKey(link.FromKeys, link.ToKeys, link.Alias);
            }
        }

        public void ImportCollection(UserProfile userProfile, RepositoryCollection collection)
        {
            InsertDomainObjects(userProfile, collection.Objects.ToArray());
            foreach (var link in collection.Links)
            {
                if (!String.IsNullOrWhiteSpace(link.SMember))
                {
                    LinkObjectsByKey(new IMeshKey[] { link.SKey }, new IMeshKey[] { link.TKey }, link.SMember);
                }
                if (!String.IsNullOrWhiteSpace(link.TMember))
                {
                    LinkObjectsByKey(new IMeshKey[] { link.TKey }, new IMeshKey[] { link.SKey }, link.TMember);
                }
            }
        }


    }
}
