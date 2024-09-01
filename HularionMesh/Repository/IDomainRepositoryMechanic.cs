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
using HularionMesh.DomainAggregate;
using HularionMesh.DomainValue;
using HularionMesh.MeshType;
using HularionMesh.Structure;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HularionMesh.Repository
{
    /// <summary>
    /// The mechanical operations for a domain within a repository that does not operate like a typical domain.
    /// </summary>
    public interface IDomainRepositoryMechanic
    {
        /// <summary>
        /// The types this domain maps to.
        /// </summary>
        IEnumerable<Type> MappedTypes { get; }

        /// <summary>
        /// Initializes the properties for the domain associated to this mechanic.
        /// </summary>
        void InitializeProperties();

        /// <summary>
        /// true iff the domain can create a type manifest for the provided type. 
        /// </summary>
        /// <param name="type">The type for which to create the manifest.</param>
        /// <returns>true iff the domain can create a type manifest for the provided type. </returns>
        bool CanCreateTypeManifest(Type type);

        /// <summary>
        /// Creates the type manifest for the object if possible. Returns null otherwise.
        /// </summary>
        /// <param name="type">The type for which to create the manifest.</param>
        /// <returns>The manifestor for the type and domain.</returns>
        TypeManifest CreateTypeManifest(Type type);

        /// <summary>
        /// Creates a RealizedMeshDomain, which includes generics, for the provided type.
        /// </summary>
        /// <param name="repository">The repository containing information for any related domains.</param>
        /// <param name="type">The type to use to create the RealizedMeshDomain.</param>
        /// <returns>The corresponding RealizedMeshDomain.</returns>
        RealizedMeshDomain CreateRealizedDomainFromType(IMeshRepository repository, Type type);

        /// <summary>
        /// Creates a RealizedMeshDomain, which includes generics, for the provided type.
        /// </summary>
        /// <typeparam name="T">The type to use to create the RealizedMeshDomain.</typeparam>
        /// <param name="repository">The repository containing information for any related domains.</param>
        /// <returns>The corresponding RealizedMeshDomain.</returns>
        RealizedMeshDomain CreateRealizedDomainFromType<T>(IMeshRepository repository);

        /// <summary>
        /// Sets up the DomainOperationLink for the system domain in the repository.
        /// </summary>
        /// <param name="repository">The repository in which to setup the link.</param>
        /// <param name="link">The link to setup (i.e. add sublinks, etcetera).</param>
        void SetupOperationLink(IMeshRepository repository, DomainOperationLink link, IDictionary<IMeshKey, DomainOperationLink> nodeLinks);

        /// <summary>
        /// Sets up the save link for the  system domain in the repository.
        /// </summary>
        /// <param name="repository">The repository in which to setup the link.</param>
        /// <param name="link">The link to setup (i.e. add sublinks, etcetera). Any added links must be added to the link.Added member. See SetDomain system domain for an example.</param>
        void SetupSaveLink(IMeshRepository repository, SaveLink link);

        /// <summary>
        /// Linkst the linked items to the provided item.
        /// </summary>
        /// <param name="queriedLink">The linking details.</param>
        /// <param name="operationLink">General mesh node informaiton.</param>
        /// <param name="item">The item on which to setup the links.</param>
        /// <param name="linkedItems">The items to link.</param>
        /// <returns>A follow-up action to call once all of the linking is complete.</returns>
        Action LinkResults(QueriedLink queriedLink, DomainOperationLink operationLink, object item, object[] linkedItems);

    }

}
