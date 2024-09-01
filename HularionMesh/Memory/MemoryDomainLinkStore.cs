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
using HularionMesh.Standard;
using HularionMesh;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HularionMesh.Structure;
using HularionCore.Pattern.Functional;

namespace HularionMesh.Memory
{
    /// <summary>
    /// Manages in-memory storage for domain links.
    /// </summary>
    public class MemoryDomainLinkStore : StandardDomainLinkStore
    {

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="domains">The domains being linked.</param>
        /// <param name="linkForm">Provides formatting for link related names.</param>
        public MemoryDomainLinkStore(LinkedDomains domains, DomainLinkForm linkForm)
            : base(domains, linkForm, 
                  new MemoryDomainValueService(linkForm.CreateLinkDomain(domains.DomainA, domains.DomainB),
                    new MemoryDomainValueStore(linkForm.CreateLinkDomain(domains.DomainA, domains.DomainB)), 
                        Creator.ForSingle<IMeshKey>(() => MeshKey.CreateUniqueTagKey())))
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="domainStore">A store for the domain values.</param>
        /// <param name="domains">The domains being linked.</param>
        /// <param name="linkForm">Provides formatting for link related names.</param>
        public MemoryDomainLinkStore(MemoryDomainValueStore domainStore, LinkedDomains domains, DomainLinkForm linkForm)
            : base(domains, linkForm,
                  new MemoryDomainValueService(linkForm.CreateLinkDomain(domains.DomainA, domains.DomainB), domainStore, Creator.ForSingle<IMeshKey>(() => MeshKey.CreateUniqueTagKey())))
        {
        }

    }
}
