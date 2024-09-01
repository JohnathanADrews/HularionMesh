#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionMesh.DomainLink;
using HularionMesh.DomainValue;
using System;
using System.Collections.Generic;
using System.Text;

namespace HularionMesh.DomainAggregate
{
    /// <summary>
    /// The response to a domain aggregate query request.
    /// </summary>
    public class DomainAggregateQueryResponse
    {
        /// <summary>
        /// The request key.
        /// </summary>
        public object Key { get; set; }

        /// <summary>
        /// The root query response node.
        /// </summary>
        public AggregateQueryResponseNode Result { get; set; }

        /// <summary>
        /// The keys to the root query result objects.
        /// </summary>
        public IList<IMeshKey> RootKeys { get; set; } = new List<IMeshKey>();

        /// <summary>
        /// The items found with the query.
        /// </summary>
        public IList<DomainObject> Objects { get; set; } = new List<DomainObject>();

        /// <summary>
        /// The links found with the query.
        /// </summary>
        public IList<QueriedLink> Links { get; set; } = new List<QueriedLink>();

    }
}
