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
using HularionMesh.Domain;
using HularionMesh.DomainLink;
using HularionMesh.DomainValue;
using HularionMesh.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HularionMesh.DomainAggregate
{
    /// <summary>
    /// Extends IDomainAggregateService for some common functions.
    /// </summary>
    public static class DomainAggregateServiceExtension
    {
        /// <summary>
        /// Imports the provided domain objects.
        /// </summary>
        /// <param name="service">The IDomainAggregateService.</param>
        /// <param name="domainByKeyProvider">Provides a domain given its key.</param>
        /// <param name="objects">The objects to import.</param>
        /// <returns>The affector response.</returns>
        public static DomainAggregateAffectorResponse ImportObjects(this IDomainAggregateService service, IParameterizedProvider<IMeshKey, MeshDomain> domainByKeyProvider, IEnumerable<DomainObject> objects)
        {
            var keyedObjects = objects.ToDictionary(x => x, x => x.Key.GetKeyPart(MeshKeyPart.Domain));
            var domains = keyedObjects.Values.Distinct().ToDictionary(x => x, x => domainByKeyProvider.Provide(x));
            var items = new List<AggregateAffectorItem>();
            foreach (var item in keyedObjects)
            {
                items.Add(new AggregateAffectorItem() { Domain = domains[item.Value], Inserter = new DomainValueAffectInsert() { Value = item.Key } });
            }
            var request = new DomainAggregateAffectorRequest() { Items = items.ToArray() };
            return service.AffectProcessor.Process(request);
        }

        /// <summary>
        /// Imports the provided links.
        /// </summary>
        /// <param name="service">The IDomainAggregateService.</param>
        /// <param name="domainByKeyProvider">Provides a domain given its key.</param>
        /// <param name="links">The links to import.</param>
        /// <returns>The affector response.</returns>
        public static DomainAggregateAffectorResponse ImportLinks(this IDomainAggregateService service, IParameterizedProvider<IMeshKey, MeshDomain> domainByKeyProvider, IEnumerable<DomainLinker> links)
        {
            var domainKeys = new List<IMeshKey>(links.Select(x => x.SKey).Distinct());
            domainKeys.AddRange(links.Select(x => x.TKey).Distinct());
            var domains = domainKeys.Distinct().ToDictionary(x => x, x => domainByKeyProvider.Provide(x));
            var linkRequests = links.Select(x => DomainLinkAffectRequest.FromDomainLinker(x)).ToList();
            foreach (var link in linkRequests)
            {
                link.DomainA = domains[link.ObjectAKey];
                link.DomainB = domains[link.ObjectBKey];
            }

            var request = new DomainAggregateAffectorRequest() { Items = linkRequests.Select(x => new AggregateAffectorItem() { Link = x }).ToArray() };
            return service.AffectProcessor.Process(request);
        }

        /// <summary>
        /// Imports the provided objects and links.
        /// </summary>
        /// <param name="service">The IDomainAggregateService.</param>
        /// <param name="domainByKeyProvider">Provides a domain given its key.</param>
        /// <param name="objects">The objects to import.</param>
        /// <param name="links">The links to import.</param>
        /// <returns>The affector response.</returns>
        public static DomainAggregateAffectorResponse ImportObjectsAndLinks(this IDomainAggregateService service, IParameterizedProvider<IMeshKey, MeshDomain> domainByKeyProvider, IEnumerable<DomainObject> objects, IEnumerable<DomainLinker> links)
        {
            var response = new DomainAggregateAffectorResponse();
            var objectResponse = service.ImportObjects(domainByKeyProvider, objects);
            var linkResponse = service.ImportLinks(domainByKeyProvider, links);
            response.SubResponses.Add(objectResponse);
            response.SubResponses.Add(linkResponse);
            return response;
        }

    }
}
