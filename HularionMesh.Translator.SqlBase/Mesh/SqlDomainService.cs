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

using HularionMesh;
using HularionMesh.Domain;
using HularionMesh.DomainLink;
using HularionMesh.MeshType;
using HularionMesh.Standard;
using HularionMesh.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace  HularionMesh.Translator.SqlBase.Mesh
{
    /// <summary>
    /// Implements IDomainService for SQL domains.
    /// </summary>
    public class SqlDomainService : IDomainService
    {

        /// <summary>
        /// The SqlMeshRepository.
        /// </summary>
        public SqlMeshRepository Repository { get; private set; }

        private Dictionary<MeshDomain, IDomainValueService> domainValueServices = new Dictionary<MeshDomain, IDomainValueService>();
        private IParameterizedProvider<MeshDomain, IDomainValueService> domainServiceProvider;
        private Dictionary<LinkedDomains, IDomainLinkService> domainLinkServices = new Dictionary<LinkedDomains, IDomainLinkService>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sqlRepository">Contains the SQL system-specific functionality.</param>
        public SqlDomainService(ISqlRepository sqlRepository)
        {
            Repository = new SqlMeshRepository(sqlRepository);
            domainServiceProvider = ParameterizedProvider.FromSingle<MeshDomain, IDomainValueService>(domain => GetDomainValueService(domain));
        }

        /// <summary>
        /// Creates the given domain.
        /// </summary>
        /// <param name="domain">The domain to create.</param>
        public void CreateDomain(MeshDomain domain)
        {
            Repository.CreateDomainOnce(Repository.SqlDomainProvider.Provide(domain));
        }

        /// <summary>
        /// Updates the given domain.
        /// </summary>
        /// <param name="domain">The domain to update.</param>
        public void UpdateDomain(MeshDomain domain)
        {
            Repository.CreateOrUpdateDomain(Repository.SqlDomainProvider.Provide(domain));
        }

        /// <summary>
        /// Deletes the domain with the given key.
        /// </summary>
        /// <param name="domainKey">THe key fo the domain to delete.</param>
        public void DeleteDomain(IMeshKey domainKey)
        {
            Repository.DeleteDomain(domainKey);
        }

        /// <summary>
        /// Provides the MeshDomain given the domain key.
        /// </summary>
        /// <param name="domainKey">The key of the domain to get.</param>
        public MeshDomain GetDomain(IMeshKey domainKey)
        {
            return Repository.GetDomain(domainKey);
        }

        /// <summary>
        /// Provides the MeshDomain given the domain's friendly name.
        /// </summary>
        /// <param name="friendlyName">The friendly name of the domain to get.</param>
        public MeshDomain GetDomain(string friendlyName)
        {
            var where = new WhereExpressionNode()
            {
                Comparison = DataTypeComparison.Equal,
                Property = MeshKeyword.DomainFriendlyName.Alias,
                Value = friendlyName
            };
            var domains = Repository.GetDomains(where);
            return domains.FirstOrDefault();
        }

        /// <summary>
        /// Provides all the MeshDomains.
        /// </summary>
        public IEnumerable<MeshDomain> GetAllValueDomains()
        {
            var domains = Repository.GetAllValueDomains();
            return domains;
        }

        /// <summary>
        /// Provides all the LinkedDomains.
        /// </summary>
        public IEnumerable<MeshDomainLink> GetAllLinkDomains()
        {
            var domains = Repository.GetAllLinkedDomains();
            return domains;
        }

        /// <summary>
        /// Provides the IDomainValueService given the MeshDomain.
        /// </summary>
        /// <param name="domain">The domain affected by the service to get.</param>
        public IDomainValueService GetDomainValueService(MeshDomain domain)
        {
            if (domainValueServices.ContainsKey(domain)) { return domainValueServices[domain]; }
            lock (domainValueServices)
            {
                if (!domainValueServices.ContainsKey(domain))
                {
                    var store = new SqlDomainValueStore(Repository, Repository.SqlDomainProvider.Provide(domain));
                    var service = new StandardDomainValueService(domain, StandardDomainForm.CreateDomainValueKeyCreator(domain), store);
                    domainValueServices.Add(domain, service);
                }
            }
            return domainValueServices[domain];
        }

        /// <summary>
        /// Provides the IDomainValueService given the MeshDomain's key.
        /// </summary>
        /// <param name="domainKey">The key of the domain affected by the service to get.</param>
        public IDomainValueService GetDomainValueService(IMeshKey domainKey)
        {
            var service = GetDomainValueService(GetDomain(domainKey));
            return service;
        }

        /// <summary>
        /// Provides the IDomainLinkService given the LinkedDomains.
        /// </summary>
        /// <param name="domains">The link domain between domains affected by the service to get.</param>
        public IDomainLinkService GetDomainLinkService(LinkedDomains domains)
        {
            var linkForm = Repository.SqlRepository.LinkKeyFormProvider.Provide(domains);
            if (domainLinkServices.ContainsKey(domains)) { return domainLinkServices[domains]; }
            lock (domainLinkServices)
            {
                if (!domainLinkServices.ContainsKey(domains))
                {
                    var domain = linkForm.CreateLinkDomain(domains.DomainA, domains.DomainB);
                    Repository.SaveDomains(domain);
                    var store = new StandardDomainLinkStore(domains, linkForm, GetDomainValueService(domain));
                    var linkService = new StandardDomainLinkService(store, domains, linkForm, domainServiceProvider);
                    domainLinkServices.Add(domains, linkService);
                }

            }
            return domainLinkServices[domains];
        }


    }
}
