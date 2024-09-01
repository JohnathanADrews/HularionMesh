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
using HularionCore.Pattern.Identifier;
using HularionMesh.DomainValue;
using System;
using System.Collections.Generic;
using System.Text;

namespace HularionMesh.DomainLink
{
    /// <summary>
    /// The response to a domain link query request.
    /// </summary>
    public class DomainLinkQueryResponse : IKeyedValue<object>
    {
        /// <summary>
        /// The request key.
        /// </summary>
        public object Key { get; set; }

        /// <summary>
        /// Maps the keys of the subject domain to the keys of the sub-domain.
        /// </summary>
        public IDictionary<IMeshKey, IList<IMeshKey>> LinkedKeys { get; set; } = new Dictionary<IMeshKey, IList<IMeshKey>>();

        /// <summary>
        /// The list of keys in the link domain.
        /// </summary>
        public IList<IMeshKey> LinkKeys { get; set; } = new List<IMeshKey>();

        /// <summary>
        /// Maps member names of the subject domain to the keys of the sub-domain.
        /// </summary>
        public IDictionary<string, IList<IMeshKey>> Members { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public DomainLinkQueryResponse()
        {
        }
    }
}
