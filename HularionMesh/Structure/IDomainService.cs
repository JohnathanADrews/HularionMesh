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
using System;
using System.Collections.Generic;
using System.Text;

namespace HularionMesh.Structure
{
    /// <summary>
    /// Modifies and provides domain information.
    /// </summary>
    public interface IDomainService
    {
        /// <summary>
        /// Creates the given domain.
        /// </summary>
        /// <param name="domain">The domain to create.</param>
        void CreateDomain(MeshDomain domain);

        /// <summary>
        /// Updates the given domain.
        /// </summary>
        /// <param name="domain">The domain to update.</param>
        void UpdateDomain(MeshDomain domain);

        /// <summary>
        /// Deletes the domain with the given key.
        /// </summary>
        /// <param name="domainKey">THe key fo the domain to delete.</param>
        void DeleteDomain(IMeshKey domainKey);

        /// <summary>
        /// Provides the MeshDomain given the domain key.
        /// </summary>
        /// <param name="domainKey">The key of the domain to get.</param>
        MeshDomain GetDomain(IMeshKey domainKey);

        /// <summary>
        /// Provides all the MeshDomains.
        /// </summary>
        IEnumerable<MeshDomain> GetAllValueDomains();

        /// <summary>
        /// Provides all the LinkedDomains.
        /// </summary>
        IEnumerable<MeshDomainLink> GetAllLinkDomains();

        /// <summary>
        /// Provides the IDomainValueService given the MeshDomain.
        /// </summary>
        /// <param name="domain">The domain affected by the service to get.</param>
        IDomainValueService GetDomainValueService(MeshDomain domain);

        /// <summary>
        /// Provides the IDomainValueService given the MeshDomain's key.
        /// </summary>
        /// <param name="domainKey">The key of the domain affected by the service to get.</param>
        IDomainValueService GetDomainValueService(IMeshKey domainKey);

        /// <summary>
        /// Provides the IDomainLinkService given the LinkedDomains.
        /// </summary>
        /// <param name="domains">The link domain between domains affected by the service to get.</param>
        IDomainLinkService GetDomainLinkService(LinkedDomains domains);

    }
}
