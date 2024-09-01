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
using HularionMesh.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace HularionMesh.Structure
{
    /// <summary>
    /// The communicators for a domain service. Implement this directly or implement IDomainService and use the StandardDomainServiceCommunicator.
    /// </summary>
    public interface IDomainServiceCommunicator
    {
        /// <summary>
        /// Creates the given domain.
        /// </summary>
        IParameterizedFacade<MeshDomain, ServiceResponse> DomainCreator { get; }
        /// <summary>
        /// Updates the given domain.
        /// </summary>
        IParameterizedFacade<MeshDomain, ServiceResponse> DomainUpdater { get; }
        /// <summary>
        /// Deletes the domain with the given key.
        /// </summary>
        IParameterizedFacade<IMeshKey, ServiceResponse> DomainDeleter { get; }
        /// <summary>
        /// Provides the MeshDomain given the domain key.
        /// </summary>
        IParameterizedProvider<IMeshKey, ServiceResponse<MeshDomain>> DomainByKeyProvider { get; }
        /// <summary>
        /// Provides the MeshDomain given the domain's friendly name.
        /// </summary>
        IParameterizedProvider<string, ServiceResponse<MeshDomain>> DomainByNameProvider { get; }
        /// <summary>
        /// Provides all the MeshDomains.
        /// </summary>
        IProvider<ServiceResponse<IEnumerable<MeshDomain>>> AllValueDomainsProvider { get; }
        /// <summary>
        /// Provides all the LinkedDomains.
        /// </summary>
        IProvider<ServiceResponse<IEnumerable<MeshDomainLink>>> AllLinkDomainsProvider { get; }
        /// <summary>
        /// Provides the IDomainValueService given the MeshDomain.
        /// </summary>
        IParameterizedProvider<MeshDomain, ServiceResponse<IDomainValueService>> ValuesServiceByDomainProvider { get; }
        /// <summary>
        /// Provides the IDomainValueService given the MeshDomain's key.
        /// </summary>
        IParameterizedProvider<IMeshKey, ServiceResponse<IDomainValueService>> ValueServiceByDomainKeyProvider { get; }
        /// <summary>
        /// Provides the IDomainLinkService given the LinkedDomains.
        /// </summary>
        IParameterizedProvider<LinkedDomains, ServiceResponse<IDomainLinkService>> LinkServiceByLinkedDomainsProvider { get; }

    }
}
