#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionCore.Pattern.Functional;
using HularionMesh.Structure;
using System;
using System.Collections.Generic;
using System.Text;

namespace HularionMesh.Standard
{
    /// <summary>
    /// A standard implementation of IMeshServiceProvider as specified by the constructor(s).
    /// </summary>
    public class StandardMeshServiceProvider : IMeshServiceProvider
    {
        /// <summary>
        /// The communicators for a domain service.
        /// </summary>
        public IDomainServiceCommunicator DomainServiceCommunicator { get; private set; }
        /// <summary>
        /// Provides domainobject mesh services.
        /// </summary>
        //public IDynamicMeshServiceProvider DynamicProvider { get; private set; }
        /// <summary>
        /// Provides a domain aggregate service.
        /// </summary>
        public IProvider<IDomainAggregateService> AggregateServiceProvider { get; private set; }

        /// <summary>
        /// Constructs the IMeshServiceProvider using the given domain service.
        /// </summary>
        /// <param name="domainService">The IDomainService from which the mesh services will be provided.</param>
        public StandardMeshServiceProvider(IDomainService domainService)
        {
            DomainServiceCommunicator = new StandardDomainServiceCommunicator(domainService);

            var aggregateService = new StandardAggregateService(DomainServiceCommunicator.ValuesServiceByDomainProvider, DomainServiceCommunicator.LinkServiceByLinkedDomainsProvider);
            AggregateServiceProvider = new ProviderFunction<IDomainAggregateService>(() => aggregateService);
        }
    }
}
