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
using HularionMesh;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HularionMesh.Structure;
using HularionCore.Structure;
using HularionCore.Pattern.Functional;

namespace HularionMesh
{
    /// <summary>
    /// Manages value and link services.
    /// </summary>
    public class ServiceManager
    {

        /// <summary>
        /// Provides a domain value service given its domain name.
        /// </summary>
        public IParameterizedProvider<MeshDomain, IDomainValueService> DomainValueServiceProvider { get; private set; }
               
        /// <summary>
        /// Provides a domain link service given the linked domain names.
        /// </summary>
        public IParameterizedProvider<LinkedDomains, IDomainLinkService> DomainLinkServiceProvider { get; private set; }


        Dictionary<MeshDomain, IDomainValueService> domainServices = new Dictionary<MeshDomain, IDomainValueService>();
        Dictionary<IMeshKey, MeshDomain> keyedDomains = new Dictionary<IMeshKey, MeshDomain>();
        Dictionary<MeshDomain, IDomainLinkService> linkServices = new Dictionary<MeshDomain, IDomainLinkService>();
        Table<MeshDomain, MeshDomain> linkTable = new Table<MeshDomain, MeshDomain>();

        private object locker = new object();

        /// <summary>
        /// Constructor.
        /// </summary>
        public ServiceManager()
        {
            DomainValueServiceProvider = ParameterizedProvider.FromSingle<MeshDomain, IDomainValueService>(
                domain =>
                {
                    //var services = domainServices.Where(x => x.Key == domain).ToList();
                    //if(services.Count == 0) { return null; }
                    //return services.First().Value;
                    //Even thought GetHashCode and Equals are set in MeshKey, ContainsKey does not recognize it.

                    //
                    if (domainServices.ContainsKey(domain))
                    {
                        return domainServices[domain];
                    }
                    var ks = keyedDomains.ContainsKey(domain.Key);
                    return null;
                });
            DomainLinkServiceProvider = ParameterizedProvider.FromSingle<LinkedDomains, IDomainLinkService>(
                link =>
                {
                    return linkTable.GetValue<IDomainLinkService>(link.DomainA, link.DomainB);
                });
        }


        /// <summary>
        /// Adds domain value services.
        /// </summary>
        /// <param name="services">The domain value services.</param>
        public void AddDomainServices(params IDomainValueService[] services)
        {
            lock (domainServices)
            {
                foreach (var service in services)
                {
                    domainServices.Add(service.Domain, service);
                    keyedDomains.Add(service.Domain.Key, service.Domain);
                }
            }
        }

        /// <summary>
        /// Adds domain link services.
        /// </summary>
        /// <param name="services">The link services to add.</param>
        public void AddLinkServices(params IDomainLinkService[] services)
        {
            lock (linkServices)
            {
                foreach (var service in services)
                {
      //              linkServices.Add(service.DomainService.Domain, service);
                }
            }
            lock (linkTable)
            {
                foreach (var service in services)
                {
                    //Set at both combinations to make lookup easier.  Lookup is commutative.
                    linkTable.SetValue(service.STypeDomain, service.TTypeDomain, service);
                    linkTable.SetValue(service.TTypeDomain, service.STypeDomain, service);
                }
            }
        }

        /// <summary>
        /// Gets all of the registered MeshDomains
        /// </summary>
        /// <returns></returns>
        public IEnumerable<MeshDomain> GetMeshDomains()
        {
            lock (domainServices)
            {
                return domainServices.Keys;
            }
        }

        /// <summary>
        /// Gets all of the registered MeshDomains
        /// </summary>
        /// <returns></returns>
        public IEnumerable<MeshDomainLink> GetLinkedDomains()
        {
            lock (linkTable)
            {
                var links = linkTable.GetAllEntries<IDomainLinkService>().Select(x => new MeshDomainLink() { DomainA = x.Column, DomainB = x.Row, Key = x.Value.LinkDomain.Key }).ToList();
                return links.Distinct().ToList();
            }
        }

        /// <summary>
        /// Gets the domain given its key.
        /// </summary>
        /// <param name="key">The key of the domain.</param>
        /// <returns>The domain given its key.</returns>
        public MeshDomain GetDomain(IMeshKey key)
        {
            lock(locker)
            {
                var domainKey = key.GetKeyPart(MeshKeyPart.Domain);
                if (!keyedDomains.ContainsKey(domainKey)) { return null; }
                return keyedDomains[domainKey];
            }
        }

        public void UpdateDomain(MeshDomain domain)
        {
            lock (locker)
            {
                var domainKey = domain.Key.GetKeyPart(MeshKeyPart.Domain);
                if (!keyedDomains.ContainsKey(domainKey)) { return ; }
                keyedDomains[domainKey].Update(domain);
            }
        }

        public void DeleteDomain(IMeshKey domainKey)
        {
            lock (locker)
            {
                domainKey = domainKey.GetKeyPart(MeshKeyPart.Domain);
                if (!keyedDomains.ContainsKey(domainKey)) { return; }
                domainServices.Remove(keyedDomains[domainKey]);
                keyedDomains.Remove(domainKey);
            }
        }

    }
}
