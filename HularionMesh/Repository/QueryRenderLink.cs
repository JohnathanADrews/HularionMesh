#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionMesh.DomainAggregate;
using System;
using System.Collections.Generic;
using System.Text;

namespace HularionMesh.Repository
{
    /// <summary>
    /// Specifies a link node in a multi-request domain query.
    /// </summary>
    public class QueryRenderLink
    {
        /// <summary>
        /// Display's the domain name for convenience.
        /// </summary>
        public string DomainName { get { return OperationLink.DomainName; } }

        /// <summary>
        /// The response node related to the link.
        /// </summary>
        public AggregateQueryResponseNode Response { get; set; }

        /// <summary>
        /// The parent of this node.
        /// </summary>
        public QueryRenderLink Parent { get; set; }

        /// <summary>
        /// The child nodes of this node.
        /// </summary>
        public List<QueryRenderLink> Links { get; set; } = new List<QueryRenderLink>();

        /// <summary>
        /// The members being linked in the query.
        /// </summary>
        public Dictionary<string, List<QueryRenderLink>> Members { get; set; } = new Dictionary<string, List<QueryRenderLink>>();

        /// <summary>
        /// The operation link related to this node.
        /// </summary>
        public DomainOperationLink OperationLink { get; set; }

        /// <summary>
        /// The domain object values.
        /// </summary>
        public Dictionary<IMeshKey, object> Values { get; set; } = new Dictionary<IMeshKey, object>();

        public override string ToString()
        {
            return String.Format("QueryRenderLink {0}: {1} - {2}", GetHashCode(), OperationLink.MemberName, DomainName);
        }

    }
}
