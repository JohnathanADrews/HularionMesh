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
using HularionMesh.Query;
using HularionMesh.Structure;
using System;
using System.Collections.Generic;
using System.Text;

namespace HularionMesh.Repository
{
    /// <summary>
    /// The functionality for a mesh repository.
    /// </summary>
    public interface IMeshRepository
    {

        /// <summary>
        /// Returns the domain with the provided key.
        /// </summary>
        /// <param name="key">The key of the domain.</param>
        /// <returns>The domain with the provided key.</returns>
        MeshDomain GetDomainFromKey(IMeshKey key);

        /// <summary>
        /// Returns the domain associated to the provided type.
        /// </summary>
        /// <param name="type">The type associated to the domain.</param>
        /// <returns>The domain associated to the provided type.</returns>
        MeshDomain GetDomainFromType(Type type);

        /// <summary>
        /// Provides the type key corresponding to a type.
        /// </summary>
        IParameterizedProvider<Type, IMeshKey> TypeKeyProvider { get; }

        /// <summary>
        /// Provides a mesh domain given a domain key.
        /// </summary>
        IParameterizedProvider<IMeshKey, MeshDomain> DomainByKeyProvider { get; }

        /// <summary>
        /// Provides the domain mechanic give the mesh domain. Set this to overrie the DomainMechanicManager's DomainMechaincProvider provider.
        /// </summary>
        IParameterizedProvider<MeshDomain, IDomainRepositoryMechanic> DomainMechanicProvider { get; }

        /// <summary>
        /// The mesh mechanism that will provide the data services whether file, database, cloud, etcetera.
        /// </summary>
        IMeshServiceProvider MeshServicesProvider { get; }

        /// <summary>
        /// Provides a DomainOperationLink given a Type.
        /// </summary>
        IParameterizedProvider<Type, DomainOperationLink> TypeOperationLinkProvider { get; }

        /// <summary>
        /// Creates a mesh query that can be used to query the mesh.
        /// </summary>
        /// <typeparam name="DomainType">The type of the domain to use as the root query.</typeparam>
        /// <returns>The mesh query for the domain associated with the given type.</returns>
        MeshRootQuery<DomainType> CreateQuery<DomainType>() where DomainType : class;

    }
}
