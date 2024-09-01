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
using HularionMesh;
using System;
using System.Collections.Generic;
using System.Text;
using HularionMesh.Structure;
using System.Linq;
using HularionCore.Pattern.Functional;

namespace HularionMesh.Memory
{

    /// <summary>
    /// An in-memory domain service.
    /// </summary>
    public class MemoryDomainService : IDomainService
    {

        private ServiceManager serviceManager = new ServiceManager();
        private IParameterizedProvider<LinkedDomains, DomainLinkForm> linkKeyFormProvider;
        private IParameterizedCreator<MeshDomain, IMeshKey> domainValueKeyCreator;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="linkKeyFormProvider">Provides the form for the link keys.</param>
        /// <param name="domainValueKeyCreator">Creates keys for domain values.</param>
        public MemoryDomainService(IParameterizedProvider<LinkedDomains, DomainLinkForm> linkKeyFormProvider,
            IParameterizedCreator<MeshDomain, IMeshKey> domainValueKeyCreator)
        {
            this.linkKeyFormProvider = linkKeyFormProvider;
            this.domainValueKeyCreator = domainValueKeyCreator;
        }

        /// <summary>
        /// Creates the specified domain.
        /// </summary>
        /// <param name="domain">The domain to create.</param>
        public void CreateDomain(MeshDomain domain)
        {
            GetDomainValueService(domain);
        }

        /// <summary>
        /// Deletes the domain with the given key.
        /// </summary>
        /// <param name="domainKey"></param>
        public void DeleteDomain(IMeshKey domainKey)
        {
            serviceManager.DeleteDomain(domainKey);
        }

        /// <summary>
        /// Gets all the linked domains.
        /// </summary>
        /// <returns>The linked domains.</returns>
        public IEnumerable<MeshDomainLink> GetAllLinkDomains()
        {
            return serviceManager.GetLinkedDomains();
        }

        /// <summary>
        /// Gets all the value domains.
        /// </summary>
        /// <returns>The alue domains.</returns>
        public IEnumerable<MeshDomain> GetAllValueDomains()
        {
            return serviceManager.GetMeshDomains();
        }

        /// <summary>
        /// Gets the domain with the provided key.
        /// </summary>
        /// <param name="domainKey">The key of the domain.</param>
        /// <returns>The domain with the provided key.</returns>
        public MeshDomain GetDomain(IMeshKey domainKey)
        {
            var domain = serviceManager.GetDomain(domainKey);
            return domain;
        }

        /// <summary>
        /// Gets the domain value service given the domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <returns>The domain value service.</returns>
        public IDomainValueService GetDomainValueService(MeshDomain domain)
        {
            var service = serviceManager.DomainValueServiceProvider.Provide(domain);
            if (service == null)
            {
                service = new MemoryDomainValueService(domain, new CreatorFunction<IMeshKey>(() => domainValueKeyCreator.Create(domain)));
                serviceManager.AddDomainServices(service);
            }
            return (MemoryDomainValueService)service;
        }

        /// <summary>
        /// Gets the domain value serice given the domain key.
        /// </summary>
        /// <param name="domainKey">The domain key.</param>
        /// <returns>The domain value service.</returns>
        public IDomainValueService GetDomainValueService(IMeshKey domainKey)
        {
            var domain = serviceManager.GetDomain(domainKey);
            return GetDomainValueService(domain);
        }

        /// <summary>
        /// Updates the provided domain.
        /// </summary>
        /// <param name="domain">The domain to update.</param>
        public void UpdateDomain(MeshDomain domain)
        {
            serviceManager.UpdateDomain(domain);
        }

        /// <summary>
        /// Gets the link service given the linked domains.
        /// </summary>
        /// <param name="domains">The links domains.</param>
        /// <returns>The link service.</returns>
        public IDomainLinkService GetDomainLinkService(LinkedDomains domains)
        {
            var service = serviceManager.DomainLinkServiceProvider.Provide(domains);
            if (service == null)
            {
                var form = linkKeyFormProvider.Provide(domains);
                var domainValueProvider = ParameterizedProvider.FromSingle<MeshDomain, IDomainValueService>(domain =>
                {
                    return (IDomainValueService)GetDomainValueService(domain);
                });
                service = new MemoryDomainLinkService(domains, linkKeyFormProvider.Provide(domains), domainValueProvider);
                serviceManager.AddLinkServices(service);
            }
            return service;
        }
    }
}
