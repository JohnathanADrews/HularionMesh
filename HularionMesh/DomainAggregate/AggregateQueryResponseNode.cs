#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionMesh.Domain;
using HularionMesh.DomainValue;
using System;
using System.Collections.Generic;
using System.Text;

namespace HularionMesh.DomainAggregate
{
    /// <summary>
    /// Contains the details for an aggregate response node.
    /// </summary>
    public class AggregateQueryResponseNode
    {
        /// <summary>
        /// A unique key to identify this node.
        /// </summary>
        public IMeshKey Key { get; private set; } = MeshKey.CreateUniqueTagKey();

        /// <summary>
        /// The name of the domain this node represents.
        /// </summary>
        public MeshDomain Domain { get; set; }

        /// <summary>
        /// The alias to represent the node in the result.
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// In a linked object, this is the name of the containing object's reference as specified in the DomainLinker (e.g. member/property name).
        /// </summary>
        public string Reference { get; set; }

        /// <summary>
        /// The nodes that are linked to this node.
        /// </summary>
        public IList<AggregateQueryResponseNode> Links { get; set; } = new List<AggregateQueryResponseNode>();

        /// <summary>
        /// The items found with the query at this node.
        /// </summary>
        public IList<DomainObject> Items { get; set; } = new List<DomainObject>();

        /// <summary>
        /// Maps a linked node to 
        /// </summary>
        public IDictionary<AggregateQueryResponseNode, IDictionary<DomainObject, IList<DomainObject>>> LinkMap { get; set; }

        /// <summary>
        /// The alias for the key property.
        /// </summary>
        public string KeyAlias { get; set; }

        /// <summary>
        /// The aliases for the value properties.
        /// </summary>
        public IDictionary<string, string> ValueAliases { get; set; }

        /// <summary>
        /// The aliases for the meta properties.
        /// </summary>
        public IDictionary<string, string> MetaAliases { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public AggregateQueryResponseNode()
        {
            Links = new List<AggregateQueryResponseNode>();
            Items = new List<DomainObject>();
            LinkMap = new Dictionary<AggregateQueryResponseNode, IDictionary<DomainObject, IList<DomainObject>>>();
            MetaAliases = new Dictionary<string, string>();
            ValueAliases = new Dictionary<string, string>();
        }

        public override string ToString()
        {
            return String.Format("{0} . {1} . {2}", Domain, Alias, Key);
        }
    }

}
