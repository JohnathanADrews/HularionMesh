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
using HularionCore.Pattern.Functional;
using HularionMesh.Domain;
using HularionMesh.DomainAggregate;
using HularionMesh.DomainLink;
using HularionMesh.DomainValue;
using HularionMesh.Standard;
using HularionMesh.Structure;

namespace HularionMesh.Memory
{
    /// <summary>
    /// Provides mesh services using a memory value store.
    /// </summary>
    public class MemoryMeshServiceProvider : IMeshServiceProvider
    {
        /// <summary>
        /// The communicators for a domain service.
        /// </summary>
        public IDomainServiceCommunicator DomainServiceCommunicator { get; private set; }
        //public IDynamicMeshServiceProvider DynamicProvider { get; private set; }
        public IProvider<IDomainAggregateService> AggregateServiceProvider { get; private set; }


        /// <summary>
        /// Provides all of the domain objects in the service suite.
        /// </summary>
        public IProvider<IEnumerable<DomainObject>> AllObjectsProvider { get; private set; }

        /// <summary>
        /// Provides all of the domain link objects in the service suite.
        /// </summary>
        public IProvider<IEnumerable<DomainLinker>> AllLinksProvider { get; private set; }

        /// <summary>
        /// Provides mesh services using a memory store.
        /// </summary>
        /// <param name="linkKeyFormProvider">Provides the serialization form for a link key.</param>
        /// <param name="domainValueKeyCreator">Creates domain value keys given the domain.</param>
        public MemoryMeshServiceProvider(IParameterizedProvider<LinkedDomains, DomainLinkForm> linkKeyFormProvider,
            IParameterizedCreator<MeshDomain, IMeshKey> domainValueKeyCreator)
        {
            DomainServiceCommunicator = new StandardDomainServiceCommunicator(new MemoryDomainService(linkKeyFormProvider, domainValueKeyCreator));
            //var communicator = new MemoryDomainServiceCommunicator(new MemoryDomainService(linkKeyFormProvider, domainValueKeyCreator));
            //DomainServiceCommunicator = communicator;

            var aggregateService = new StandardAggregateService(this.DomainServiceCommunicator.ValuesServiceByDomainProvider, this.DomainServiceCommunicator.LinkServiceByLinkedDomainsProvider);
            //var aggregateService = new MemoryDomainAggregateService(communicator);
            AggregateServiceProvider = new ProviderFunction<IDomainAggregateService>(() => aggregateService);

        }

    }
}
