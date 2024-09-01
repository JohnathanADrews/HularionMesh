#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionCore.Pattern;
using HularionCore.Pattern.Functional;
using HularionCore.Pattern.Topology;
using HularionMesh.DomainAggregate;
using HularionMesh.DomainValue;
using HularionMesh.MeshType;
using HularionMesh.SystemDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HularionMesh.Domain
{
    /// <summary>
    /// A domain in the mesh services.
    /// </summary>
    public class MeshDomain
    {
        /// <summary>
        /// The key of the domain.
        /// </summary>
        public MeshKey Key { get; set; }

        /// <summary>
        /// A user-friendly domain name.
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// The unique name of the domain.
        /// </summary>
        public string UniqueName 
        { 
            get { return uniqueName; } 
            set 
            { 
                uniqueName = value;
                //Key = new MeshKey() { Serialized = String.Format("{0}{1}{2}", MeshKeyPart.Domain, MeshKey.PartialAffixDelimiter, UniqueName) };
                Key = new MeshKey();
                Key.SetPart(MeshKeyPart.Domain, UniqueName);
            } 
        }

        private string uniqueName { get; set; }

        /// <summary>
        /// Static values related to the domain.
        /// </summary>
        public IDictionary<string, object> Values { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// The properies of this domain.
        /// </summary>
        public List<ValueProperty> Properties { get; set; } = new List<ValueProperty>();

        /// <summary>
        /// The meta proxy properties of this domain.
        /// </summary>
        public List<ValueProperty> Proxies { get; set; } = new List<ValueProperty>();

        /// <summary>
        /// The generic arguments for this domain.
        /// </summary>
        public List<MeshGeneric> GenericsParameters { get; set; } = new List<MeshGeneric>();

        /// <summary>
        /// True iff the domain contians generic parameters.
        /// </summary>
        public bool IsGeneric { get { return GenericsParameters.Count() > 0; } }

        /// <summary>
        /// The serialized generics.
        /// </summary>
        public string SerializedGenerics { get { return MeshGeneric.SerializeGenerics(GenericsParameters.ToArray()); } set { GenericsParameters = MeshGeneric.Deserialize(value).ToList(); } }

        private static Type thisType = typeof(MeshDomain);

        /// <summary>
        /// Constructor.
        /// </summary>
        public MeshDomain()
        {
            UniqueName = MeshKey.CreateUniqueTag();
        }

        public void Update(MeshDomain domain)
        {
            this.FriendlyName = domain.FriendlyName;
            this.Properties = domain.Properties;
        }

        /// <summary>
        /// Directly sets this domain's key.
        /// </summary>
        /// <param name="key"></param>
        public void SetKey(IMeshKey key)
        {
            this.Key = MeshKey.Parse(key.Serialized);
        }

        /// <summary>
        /// Removes null and proxy members from the domain object.
        /// </summary>
        /// <param name="domainObject">The domain object to prune.</param>
        /// <param name="generics">The generics used by the domain object.</param>
        public void Prune(DomainObject domainObject, IDictionary<string, MeshGeneric> generics = null)
        {
            var remove = new HashSet<string>();
            var properties = Properties.ToDictionary(x => x.Name, x => x);
            if (generics == null) { generics = new Dictionary<string, MeshGeneric>(); }
            foreach (var value in domainObject.Values)
            {
                if (!properties.ContainsKey(value.Key)) { remove.Add(value.Key); continue; }
                var property = properties[value.Key];
                if (property.HasGenerics) { }
                MeshGeneric generic = null;
                if (generics.ContainsKey(property.Type)) { generic = generics[property.Type]; }
                if (property.Proxy != ValuePropertyProxy.None) { remove.Add(value.Key); continue; }
                if (generic == null && !DataType.TypeIsKnown(property.Type)) { remove.Add(value.Key); }
                if (generic != null && !DataType.TypeIsKnown(generic.Key)) { remove.Add(value.Key); }
            }
            foreach (var value in remove)
            {
                domainObject.Values.Remove(value);
            }
        }

        /// <summary>
        /// Creates an extended mesh domain from this domain.
        /// </summary>
        /// <param name="generics">The generic arguments to match the generic parameters of the domain.</param>
        /// <returns>An extended mesh domain from this domain.</returns>
        public RealizedMeshDomain CreateRealizedMeshDomain(MeshGeneric[] generics, IParameterizedProvider<IMeshKey, MeshDomain> domainProvider)
        {
            var realizedDomain = new RealizedMeshDomain(this, generics, domainProvider);
            return realizedDomain;
        }
        /// <summary>
        /// Creates an extended mesh domain from this domain.
        /// </summary>
        /// <param name="generics">The generic arguments to match the generic parameters of the domain.</param>
        /// <returns>An extended mesh domain from this domain.</returns>
        public RealizedMeshDomain CreateRealizedMeshDomain(IDictionary<string, MeshGeneric> arguments, IParameterizedProvider<IMeshKey, MeshDomain> domainProvider)
        {
            foreach(var argument in arguments) { argument.Value.Name = argument.Key; }
            var realizedDomain = new RealizedMeshDomain(this, arguments.Values.ToArray(), domainProvider);
            return realizedDomain;
        }

        /// <summary>
        /// Creates a default domain object in this domain.
        /// </summary>
        /// <returns>The created default domain object.</returns>
        public DomainObject CreateDefault()
        {
            var result = new DomainObject();
            //result.Key = Key.Clone().Append(MeshKey.CreateUniqueTagKey());
            result.Key = Key.Clone().SetPart(MeshKeyPart.Unique, MeshKey.CreateUniqueTag());
            foreach (var property in Properties)
            {                
                result.Values.Add(property.Name, property.Default);
            }
            foreach(var meta in MeshDomain.MetaProperties)
            {
                result.Meta.Add(meta.Name, meta.Default);
            }
            return result;
        }

        /// <summary>
        /// The key property for a domain object.
        /// </summary>
        public static ValueProperty ObjectMeshKey = new ValueProperty() { Name = MeshKeyword.Key.Alias, Type = DataType.MeshKey.Key.Serialized };

        /// <summary>
        /// The datetime that the domain object was created.
        /// </summary>
        public static ValueProperty CreationTime = new ValueProperty() { Name = MeshKeyword.ValueCreationTime.Alias, Type = DataType.DateTime.Key.Serialized };
        /// <summary>
        /// The key of the user who created the domain object.
        /// </summary>
        public static ValueProperty CreationUser = new ValueProperty() { Name = MeshKeyword.ValueCreator.Alias, Type = DataType.MeshKey.Key.Serialized };
        /// <summary>
        /// The datetome the domain object was last updated.
        /// </summary>
        public static ValueProperty UpdateTime = new ValueProperty() { Name = MeshKeyword.ValueUpdateTime.Alias, Type = DataType.DateTime.Key.Serialized };
        /// <summary>
        /// The key of the user who last updated the domain object.
        /// </summary>
        public static ValueProperty UpdateUser = new ValueProperty() { Name = MeshKeyword.ValueUpdater.Alias, Type = DataType.MeshKey.Key.Serialized };
        /// <summary>
        /// The generic arguments specified for the domain object.
        /// </summary>
        public static ValueProperty Generics = new ValueProperty() { Name = MeshKeyword.Generics.Alias, Type = DataType.Text8.Key.Serialized };
        /// <summary>
        /// The meta-information properties such as creation time and creation user.
        /// </summary>
        public static ValueProperty[] MetaProperties { get; private set; } = new ValueProperty[] { CreationTime, CreationUser, UpdateTime, UpdateUser, Generics };

        /// <summary>
        /// Maps the MetaProperty enum values to the corresponding ValueProperty.
        /// </summary>
        public static Dictionary<MetaProperty, ValueProperty> MetaMap = new Dictionary<MetaProperty, ValueProperty>()
        {
            { MetaProperty.ValueCreator, CreationUser },
            { MetaProperty.CreationTime, CreationTime },
            { MetaProperty.UpdateTime, UpdateTime },
            { MetaProperty.ValueUpdater, UpdateUser },
            { MetaProperty.Generics, Generics }
        };

        /// <summary>
        /// Determines whether the key is for an object within this domain.
        /// </summary>
        /// <param name="objectKey">The key to check.</param>
        /// <returns>true iff the key is for an object within this domain.</returns>
        public bool KeyIsObjectInThisDomain(IMeshKey objectKey)
        {
            var meshKey = (MeshKey)MeshKey.Parse(objectKey.Serialized).GetKeyPart(MeshKeyPart.Domain);
            return (Key == meshKey);
        }

        public override bool Equals(object obj)
        {
            if(obj == null || !thisType.IsAssignableFrom(obj.GetType())) { return false; }
            return this.Key.EqualsKey(((MeshDomain)obj).Key);
        }

        public override int GetHashCode()
        {
            return this.Key.GetHashCode();
        }

        public static bool operator ==(MeshDomain domain1, MeshDomain domain2)
        {
            if (Object.ReferenceEquals(domain1, null))
            {
                if (Object.ReferenceEquals(domain2, null)) { return true; }
                return false;
            }
            if (Object.ReferenceEquals(domain2, null)) { return false; }
            return domain1.Equals(domain2);
        }

        public static bool operator !=(MeshDomain domain1, MeshDomain domain2)
        {
            return !(domain1 == domain2);
        }

        public override string ToString()
        {
            return Key.ToString();
        }


        public static MeshDomain MeshDomainDomain { get; private set; }

        static MeshDomain()
        {
            MeshDomainDomain = new MeshDomain() { FriendlyName = "System::MeshDomain" };
            var typeRegistrar = new TypedDomainRegistrar();
            MeshDomainDomain.Properties = typeRegistrar.GetTypeProperties(typeof(MeshDomain));
        }

    }


}
