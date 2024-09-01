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
using HularionMesh.DomainLink;
using HularionMesh.DomainValue;
using HularionMesh.MeshType;
using HularionMesh.SystemDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HularionMesh.DomainAggregate
{
    /// <summary>
    /// Defines how an item in the query tree should be executed.
    /// </summary>
    public class AggregateQueryItem
    {

        /// <summary>
        /// The public name of the domain to use.
        /// </summary>
        public MeshDomain Domain { get; set; }

        /// <summary>
        /// The name of the properties representing the queried result.
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// The properties of the domain to get.
        /// </summary>
        public DomainReadRequest Reads { get; set; }

        /// <summary>
        /// The query definition for linked requested items of other domains.
        /// </summary>
        public IList<AggregateQueryItem> Links { get; set; }

        /// <summary>
        /// The type of domain that is being read.
        /// </summary>
        public AggregateDomainMode Mode { get; set; }

        /// <summary>
        /// (MAY be null.) Indicates how to filter keys and values using the value domain.  Implicitly restricted to results of the parent unless this node is root.
        /// </summary>
        public WhereExpressionNode DomainWhere { get; set; }

        /// <summary>
        /// (Leave null if root. MAY be null.) Indicates how to filter keys and values using the link domain.
        /// </summary>
        public WhereExpressionNode LinkWhere { get; set; }

        /// <summary>
        /// (MAY be null.) Indicates how to filter keys and values using the value domain when this query item is visited the first time. 
        /// </summary>
        public WhereExpressionNode RecurrenceRootWhere { get; set; }

        /// <summary>
        /// The items that this item is imposing on.
        /// </summary>
        public HashSet<AggregateQueryItem> Impositions { get; set; } = new HashSet<AggregateQueryItem>();

        /// <summary>
        /// Determines which key to match the Subject if this is a link node. The Subject is the domain in the Parent to this node.
        /// </summary>
        public LinkKeyMatchMode LinkKeyMatchMode { get; set; } = LinkKeyMatchMode.Both;

        /// <summary>
        /// Constructor.
        /// </summary>
        public AggregateQueryItem()
        {
            Links = new List<AggregateQueryItem>();
            DomainWhere = WhereExpressionNode.ReadAll;
        }


        /// <summary>
        /// Creates a copy of this item.
        /// </summary>
        /// <returns>The copy.</returns>
        public AggregateQueryItem Clone()
        {
            var clone = new AggregateQueryItem();
            clone.Domain = Domain;
            clone.Alias = Alias;
            clone.Reads = Reads;
            clone.Links = Links;
            clone.Mode = Mode;
            clone.DomainWhere = DomainWhere;
            clone.LinkWhere = LinkWhere;
            clone.Impositions = Impositions;
            return clone;
        }

        public override string ToString()
        {
            return String.Format("{0} '{1}' {2} {3}", GetHashCode(), Alias, Mode, Domain);
        }

    }
}
