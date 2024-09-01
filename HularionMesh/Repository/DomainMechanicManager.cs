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
using System;
using System.Collections.Generic;
using System.Text;

namespace HularionMesh.Repository
{
    /// <summary>
    /// Stores the repository mechanics for a set of domains.
    /// </summary>
    public class DomainMechanicManager
    {

        private Dictionary<MeshDomain, IDomainRepositoryMechanic> domainMechanics = new Dictionary<MeshDomain, IDomainRepositoryMechanic>();
        private Dictionary<IMeshKey, MeshDomain> keyedDomains = new Dictionary<IMeshKey, MeshDomain>();

        /// <summary>
        /// Provides the domain mechanic give the mesh domain.
        /// </summary>
        public IParameterizedProvider<MeshDomain, IDomainRepositoryMechanic> DomainMechanicProvider { get; private set; }

        /// <summary>
        /// Provides the domain mechanic given the mesh domain.
        /// </summary>
        public IParameterizedProvider<IMeshKey, IDomainRepositoryMechanic> DomainKeyMechaincProvider { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public DomainMechanicManager()
        {
            DomainMechanicProvider = ParameterizedProvider.FromSingle<MeshDomain, IDomainRepositoryMechanic>(domain => GetMechanic(domain));
            DomainKeyMechaincProvider = ParameterizedProvider.FromSingle<IMeshKey, IDomainRepositoryMechanic>(domainKey => GetMechanic(domainKey));
        }

        /// <summary>
        /// Sets the mechanic for the given domain.
        /// </summary>
        /// <param name="domain">The domain for which to set the mechaic.</param>
        /// <param name="mechanic">The domain mechanic to set.</param>
        public void SetMechanic(MeshDomain domain, IDomainRepositoryMechanic mechanic)
        {
            lock (domainMechanics)
            {
                if (domainMechanics.ContainsKey(domain)) { domainMechanics.Remove(domain); }
                domainMechanics.Add(domain, mechanic);
                if (keyedDomains.ContainsKey(domain.Key)) { keyedDomains.Remove(domain.Key); }
                keyedDomains.Add(domain.Key, domain);
            }
        }

        /// <summary>
        /// Gets the domain mechanic for the provided domain or null if none were registered.
        /// </summary>
        /// <param name="domain">The domain's mechanic to get.</param>
        /// <returns>The domain mechanic for the provided domain or null if none were registered.</returns>
        public IDomainRepositoryMechanic GetMechanic(MeshDomain domain)
        {
            if (domainMechanics.ContainsKey(domain)) { return domainMechanics[domain]; }
            return null;
        }

        /// <summary>
        /// Gets the domain mechanic for the provided domain or null if none were registered.
        /// </summary>
        /// <param name="domainKey">The key of the domain's mechanic to get.</param>
        /// <returns>The domain mechanic for the provided domain or null if none were registered.</returns>
        public IDomainRepositoryMechanic GetMechanic(IMeshKey domainKey)
        {
            if (keyedDomains.ContainsKey(domainKey)) { return GetMechanic(keyedDomains[domainKey]); }
            return null;
        }



    }
}
