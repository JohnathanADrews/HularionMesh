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
using HularionMesh.DomainLink;
using HularionMesh.DomainValue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HularionMesh.Repository
{
    /// <summary>
    /// A link node for creating an aggregate command to save a nested object.
    /// </summary>
    public class SaveLink
    {
        /// <summary>
        /// The domain object of the node.
        /// </summary>
        public DomainObject DomainObject { get; set; }

        /// <summary>
        /// The name of this link's domain.
        /// </summary>
        public string DomainName { get { return OperationLink == null ? null : OperationLink.DomainName; } }

        /// <summary>
        /// The operation link object that corresponds to the object's position.
        /// </summary>
        public DomainOperationLink OperationLink { get; set; }

        /// <summary>
        /// The type of the value of this node
        /// </summary>
        public Type SourceType { get; set; }

        public DomainOperationLink MemberOperationLink { get; set; }

        public List<SaveLink> Links { get; set; } = new List<SaveLink>();

        public Dictionary<string, SaveLink> Members { get; set; } = new Dictionary<string, SaveLink>();

        public SaveLink Parent { get; set; }

        public object Value { get; set; }

        /// <summary>
        /// The links added by this link.
        /// </summary>
        public HashSet<SaveLink> Added { get; private set; } = new HashSet<SaveLink>();

        /// <summary>
        /// Set by the repository to be used by a system or other special domain. Domains should try to get a link and then only create one if ther result is null.
        /// </summary>
        public IParameterizedProvider<object, SaveLink> LinkProvider { get; set; }

        public IProvider<AggregateAffectorItem[]> LinkAffectorProvider { get; set; }

        public SaveLink()
        {
            LinkAffectorProvider = new ProviderFunction<AggregateAffectorItem[]>(() =>
            {
                var affectors = new List<AggregateAffectorItem>();
                foreach (var link in Members)
                {
                    affectors.Add(new AggregateAffectorItem()
                    {
                        Link = new DomainLinkAffectRequest()
                        {
                            Mode = LinkAffectMode.LinkKeys,
                            DomainA = OperationLink.RealizedDomain.Domain,
                            DomainB = link.Value.OperationLink.RealizedDomain.Domain,
                            AMember = link.Key,
                            ObjectAKey = DomainObject.Key,
                            ObjectBKey = link.Value.DomainObject.Key
                        }
                    });
                }
                return affectors.ToArray();
            });
        }


        public static SaveLink[] CreateSaveLinks(MeshRepository repository, object[] objects)
        {
            var result = new List<SaveLink>();
            var linkTraverser = new TreeTraverser<SaveLink>();
            var map = new Dictionary<object, SaveLink>();
            var linkProvider = ParameterizedProvider.FromSingle<object, SaveLink>(x => map.ContainsKey(x) ? map[x] : null);
            var roots = new HashSet<object>(objects).Where(x => x != null)
                .Select(x => new SaveLink() { Value = x, LinkProvider = linkProvider, OperationLink = repository.TypeOperationLinkProvider.Provide(x.GetType()) }).ToArray();
            foreach(var node in roots) { map.Add(node.Value, node); }
            //linkProvider is necessary for domains with a mechanic in order to provide any linked nodes.
            var plan = linkTraverser.CreateEvaluationPlan(TreeTraversalOrder.ParentLeftRight, roots, node =>
            {
                var nodeMechanic = repository.DomainMechanicProvider.Provide(node.OperationLink.RealizedDomain.Domain);
                if (nodeMechanic != null)
                {
                    nodeMechanic.SetupSaveLink(repository, node);
                    foreach (var add in node.Added)
                    {
                        if (!map.ContainsKey(add.Value)) { map.Add(add.Value, add); }
                    }
                }
                else
                {
                    node.DomainObject = DomainObject.Derive(node.Value);
                    node.DomainObject.Meta[MeshKeyword.Generics.Alias] = node.OperationLink.RealizedDomain.SerializedGenerics;
                    foreach (var value in node.DomainObject.Values)
                    {
                        if (value.Value == null) { continue; } //There is no value.
                        if (!node.OperationLink.Members.ContainsKey(value.Key)) { continue; } //It is not a member known by the domain.
                        var nextLink = linkProvider.Provide(value.Value);
                        if(nextLink != null) 
                        {
                            node.Members.Add(value.Key, nextLink);
                            node.Links.Add(nextLink);
                            continue; 
                        }
                        nextLink = new SaveLink() { Parent = node, Value = value.Value, LinkProvider = linkProvider };
                        nextLink.MemberOperationLink = node.OperationLink.Members[value.Key];
                        nextLink.OperationLink = nextLink.MemberOperationLink.Member;
                        node.Members.Add(value.Key, nextLink);
                        node.Links.Add(nextLink);
                        node.Added.Add(nextLink);
                        if (!map.ContainsKey(value.Value)) { map.Add(value.Value, nextLink); }
                    }
                    node.OperationLink.RealizedDomain.Prune(node.DomainObject);
                }
                return node.Added.ToArray();
            }, true);

            return plan;
        }

        public override string ToString()
        {
            return String.Format("SaveLink {0}: {1} - {2}", GetHashCode(), OperationLink.MemberName, DomainName);
        }

    }
}
