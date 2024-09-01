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
    /// A plug in a QueriedDomainObject. This is a placeholder for a DomainObject that may be referenced multiple times to avoid redundant DomainObjects.
    /// </summary>
    public class QueriedLink
    {

        /// <summary>
        /// The alias assigned to the link in the query.  X in A.X -> B
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// The keys of the domain objects linking to other objects. A in A.X -> B
        /// </summary>
        public IList<IMeshKey> FromKeys { get; set; } = new List<IMeshKey>();

        /// <summary>
        /// The keys of the domain object linked from other domain objects.  B in A.X -> B
        /// </summary>
        public IList<IMeshKey> ToKeys { get; set; } = new List<IMeshKey>();

    }
}
