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
    /// Implements IDomainLinkService using an in-memory store.
    /// </summary>
    public class MemoryDomainLinkService : StandardDomainLinkService
    {

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="domains">The names of the linked domains.</param>
        /// <param name="linkForm">The formatting values for creating the link domain name and keys.</param>
        /// <param name="domainServiceProvider">Provides domain value services.</param>
        public MemoryDomainLinkService(LinkedDomains domains, DomainLinkForm linkForm,
            IParameterizedProvider<MeshDomain, IDomainValueService> domainServiceProvider)
        : base(new MemoryDomainLinkStore(domains, linkForm), domains, linkForm, domainServiceProvider)
        {
        }

    }
}
