#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionCore.Pattern.Topology;
using HularionMesh.Domain;
using HularionMesh.DomainAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HularionMesh.Repository
{
    /// <summary>
    /// Contains the immediate linking details for a mesh domain.
    /// </summary>
    public class DomainOperationLink
    {
        /// <summary>
        /// The key for this link.
        /// </summary>
        public IMeshKey Key { get { if (RealizedDomain == null) { return MeshKey.NullKey; } return RealizedDomain.Key; } }

        /// <summary>
        /// The name of the domain for convenience.
        /// </summary>
        public string DomainName { get { if (RealizedDomain == null) { return null; } return RealizedDomain.Domain.ToString(); } }

        /// <summary>
        /// The extended domain, which includes the domain and the selected generics.
        /// </summary>
        public RealizedMeshDomain RealizedDomain { get; set; }

        /// <summary>
        /// The immdiate anscestor of this node.
        /// </summary>
        public DomainOperationLink Parent { get; set; }

        /// <summary>
        /// The kind of link in the operation mesh.
        /// </summary>
        public DomainOperationMode NodeType { get; set; }

        /// <summary>
        /// The immediate descendents of this node.
        /// </summary>
        public List<DomainOperationLink> Links { get; set; } = new List<DomainOperationLink>();

        /// <summary>
        /// The member links specified by member name.
        /// </summary>
        public Dictionary<string, DomainOperationLink> Members { get; set; } = new Dictionary<string, DomainOperationLink>();

        /// <summary>
        /// The name of the member this node belongs to in the parent node.
        /// </summary>
        public string MemberName { get; set; }

        /// <summary>
        /// In a Member link, this is the node to which the member points.
        /// </summary>
        public DomainOperationLink Member { get; set; }

        /// <summary>
        /// The type of the member.
        /// </summary>
        public DomainOperationMemberType MemberType { get; set; } = DomainOperationMemberType.Dynamic;

        /// <summary>
        /// C# Type Only - The type from which this link was generated.
        /// </summary>
        public Type SourceType { get; set; }

        /// <summary>
        /// C# Type Only - The property of this member relative to the parent, if it is a property.
        /// </summary>
        public PropertyInfo MemberProperty { get; set; }

        /// <summary>
        /// C# Type Only - The field of this member relative to the parent, if it is a field.
        /// </summary>
        public FieldInfo MemberField { get; set; }

        /// <summary>
        /// true iff this node represents a system domain.
        /// </summary>
        public bool IsSystemDomain { get; set; } = false;

        /// <summary>
        /// True iff the RealizedDomain has gneeric arguments.
        /// </summary>
        public bool IsGeneric
        {
            get
            {
                if(RealizedDomain == null || RealizedDomain.GenericsArguments == null) { return false; }
                return RealizedDomain.GenericsArguments.Count() > 0;
            }
        }

        public IDictionary<string, MeshGeneric> GetGenericArguments()
        {
            var node = this;
            while(node.Parent != null) { node = node.Parent; }
            return node.RealizedDomain.GenericsArguments;
        }

        public override string ToString()
        {
            return String.Format("DomainOperationLink {0}: {1} {2}  {3}", GetHashCode(), NodeType, MemberName, DomainName);
        }


        public static DomainOperationLink CreateDomainOperationLink(MeshRepository repository, RealizedMeshDomain domain, Type sourceType = null)
        {
            if (MeshGeneric.ContainsParameter(domain.GenericsArguments.Select(x => x.Value).ToArray()))
            {
                throw new ArgumentException(String.Format("The generics specified for the domain contains a generic parameter. One or more specified generic types may be invalid. [efenYblTV0iv3xVrmdRyqQ]"));
            }
            var traverser = new TreeTraverser<DomainOperationLink>();
            var map = new Dictionary<IMeshKey, DomainOperationLink>();
            var rootLink = new DomainOperationLink() { RealizedDomain = domain, SourceType = sourceType, MemberName = "<root>", NodeType = DomainOperationMode.Domain };
            var nodeLinks = new Dictionary<IMeshKey, DomainOperationLink>();
            //nodeLinks.Add(rootLink.Key, rootLink);
            map.Add(rootLink.Key, rootLink);
            traverser.CreateEvaluationPlan(TreeTraversalOrder.ParentLeftRight, rootLink, node =>
            {
                if (node.NodeType == DomainOperationMode.Link)
                {
                    if (nodeLinks.ContainsKey(node.Member.Key)) 
                    { 
                        return new DomainOperationLink[] { }; 
                    }
                    return new DomainOperationLink[] { node.Member };
                }
                if(node.NodeType == DomainOperationMode.Domain && !nodeLinks.ContainsKey(node.RealizedDomain.Key))
                {
                    nodeLinks.Add(node.RealizedDomain.Key, node);
                    AssignNodeMembers(repository, node, nodeLinks);
                    return node.Members.Values.ToArray();
                }
                return new DomainOperationLink[] { };
            }, true);

            return rootLink;
        }


        private static void AssignNodeMembers(MeshRepository repository, DomainOperationLink link, IDictionary<IMeshKey, DomainOperationLink> nodeLinks)
        {
            var mechanic = repository.DomainMechanicProvider.Provide(link.RealizedDomain.Domain);
            if (mechanic != null)
            {
                mechanic.SetupOperationLink(repository, link, nodeLinks);
                return;
            }

            Dictionary<string, PropertyInfo> sourceProperties = new Dictionary<string, PropertyInfo>();
            Dictionary<string, FieldInfo> sourceFields = new Dictionary<string, FieldInfo>();
            var domainProperties = link.RealizedDomain.Properties.Where(x => repository.HasDomainByKey(x.Type)).ToDictionary(x => x, x => repository.GetDomainFromKey(x.Type));
            if (link.SourceType != null)
            {
                sourceProperties = link.SourceType.GetProperties().ToDictionary(x => x.Name, x => x);
                sourceFields = link.SourceType.GetFields().ToDictionary(x => x.Name, x => x);
            }
            var propertyLinks = new Dictionary<IMeshKey, DomainOperationLink>();
            foreach (var property in domainProperties)
            {
                var realizedDomain = property.Value.CreateRealizedMeshDomain(property.Key.Generics.ToDictionary(x=>x.Name, x=>x), repository.DomainByKeyProvider);
                DomainOperationLink propertyNode = null;
                if (nodeLinks.ContainsKey(realizedDomain.Key)) { propertyNode = nodeLinks[realizedDomain.Key]; }
                else if (propertyLinks.ContainsKey(realizedDomain.Key)) { propertyNode = propertyLinks[realizedDomain.Key]; }
                else
                {
                    propertyNode = new DomainOperationLink()
                    {
                        NodeType = DomainOperationMode.Domain,
                        RealizedDomain = realizedDomain
                    };
                    propertyLinks.Add(realizedDomain.Key, propertyNode);
                }
                var memberNode = new DomainOperationLink()
                {
                    NodeType = DomainOperationMode.Link,
                    MemberName = property.Key.Name, 
                    Parent = link, 
                    Member = propertyNode
                };
                //propertyNode.Parent = memberNode; //A property node may have multiple parents, so we cannot set the parent here.
                link.Members.Add(property.Key.Name, memberNode);
                if (sourceProperties.ContainsKey(property.Key.Name))
                {
                    memberNode.MemberType = DomainOperationMemberType.Property;
                    memberNode.MemberProperty = sourceProperties[property.Key.Name];
                    propertyNode.SourceType = sourceProperties[property.Key.Name].PropertyType;
                    memberNode.SourceType = sourceProperties[property.Key.Name].PropertyType;
                }
                if (sourceFields.ContainsKey(property.Key.Name))
                {
                    memberNode.MemberType = DomainOperationMemberType.Field;
                    memberNode.MemberField = sourceFields[property.Key.Name];
                    propertyNode.SourceType = sourceFields[property.Key.Name].FieldType;
                    memberNode.SourceType = sourceFields[property.Key.Name].FieldType;
                }
            }
        }


        /// <summary>
        /// Decribes whether an operation link member is dynamic or derived from a C# property or C# field.
        /// </summary>
        public enum DomainOperationMemberType
        {
            Dynamic,
            Property,
            Field
        }
    }

}
