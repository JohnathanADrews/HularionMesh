#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace HularionMesh.DomainAggregate
{
    /// <summary>
    /// A request to affect multiple domain objects or links.
    /// </summary>
    public class DomainAggregateAffectorRequest
    {
        /// <summary>
        /// The request key.
        /// </summary>
        public object Key { get; set; }

        /// <summary>
        /// The key of the user performing the operation.
        /// </summary>
        public IMeshKey UserKey { get; set; }

        /// <summary>
        /// The time of the request.
        /// </summary>
        public DateTime RequestTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The items that will be affected.
        /// </summary>
        public AggregateAffectorItem[] Items { get; set; }

        /// <summary>
        /// If true, Items can be processed in any order, which allows them to be batched, for example, by domain.
        /// </summary>
        public bool AnyOrder { get; set; } = false;


    }
}
