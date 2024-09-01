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
using HularionMesh.Repository;
using HularionMesh.Standard;
using HularionMesh.User;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HularionMesh.Memory
{
    /// <summary>
    /// An in-memory repository builder.
    /// </summary>
    public class MemoryMeshRepositoryBuilder
    {

        /// <summary>
        /// Accumulates the details for registrations such as type include attributes and assemblies to search.
        /// </summary>
        public AssemblyRegistrationDetail RegistrationDetail { get; private set; } = new AssemblyRegistrationDetail();

        /// <summary>
        /// The profile of the user creating the repository.
        /// </summary>
        public UserProfile UserProfile { get; set; } = UserProfile.DefaultUser;

        /// <summary>
        /// The assemblies included in the mesh.
        /// </summary>
        public List<Assembly> Assemblies { get; set; } = new List<Assembly>();

        /// <summary>
        /// The types to include in the mesh.
        /// </summary>
        public List<Type> IncludeTypes { get; set; } = new List<Type>();



        /// <summary>
        /// Sets the user profile used to build the repository.
        /// </summary>
        /// <param name="userProfile">The user profile used to build the repository.</param>
        /// <returns>this</returns>
        public MemoryMeshRepositoryBuilder SeUserProfile(UserProfile userProfile)
        {
            this.UserProfile = userProfile;
            return this;
        }

        /// <summary>
        /// Adds the provided assemblies to the included domain type search.
        /// </summary>
        /// <param name="assemblies">The assemblies to the included domain type search.</param>
        /// <returns>this</returns>
        public MemoryMeshRepositoryBuilder AddAssemblies(params Assembly[] assemblies)
        {
            this.Assemblies.AddRange(assemblies);
            return this;
        }

        /// <summary>
        /// Adds the provided attribute to the included domain attributes.
        /// </summary>
        /// <typeparam name="T">The type of the attribute to add.</typeparam>
        /// <returns>this</returns>
        public MemoryMeshRepositoryBuilder AddIncludeAttribute<T>()
            where T : Attribute
        {
            this.IncludeTypes.Add(typeof(T));
            return this;
        }

        /// <summary>
        /// Adds the provided attributes to the included domain attributes.
        /// </summary>
        /// <param name="includeTypes">The attribute types to include.</param>
        /// <returns>this</returns>
        public MemoryMeshRepositoryBuilder AddIncludeAttributes(params Type[] includeTypes)
        {
            this.IncludeTypes.AddRange(includeTypes);
            return this;
        }

        /// <summary>
        /// Creates an in-memory MeshRepository using a standard setup.
        /// </summary>
        /// <typeparam name="IncludeAttributeType">The type of attribute that will cause a class to be added as a domain if that class has the attribute.</typeparam>
        /// <param name="userProfile">The profile of the user creating the repository.</param>
        /// <returns>An in-memory MeshRepository</returns>
        public static MeshRepository CreateRepository<IncludeAttributeType>(UserProfile userProfile = null)
            where IncludeAttributeType : Attribute
        {
            if (userProfile == null) { userProfile = UserProfile.DefaultUser; }
            var attributeType = typeof(IncludeAttributeType);
            var memoryProvider = new MemoryMeshServiceProvider(StandardLinkForm.LinkKeyFormProvider, StandardDomainForm.DomainValueKeyCreator);
            var detail = AssemblyRegistrationDetail.CreateAssemblyRegistrationDetail(attributeType.Assembly);
            detail.InitializeDomainProperties = true;
            detail.SetRegistrationCheckerFromAttributes<IncludeAttributeType>();
            detail.UniqueNameProvider = TypeToDomainName.DefaultProvider;
            var repository = new MeshRepository(memoryProvider);
            repository.RegisterAssemblies(detail);
            return repository;
        }

        /// <summary>
        /// Creates an in-memory MeshRepository using a standard setup.
        /// </summary>
        /// <typeparam name="IncludeAttributeType">The type of attribute that will cause a class to be added as a domain if that class has the attribute.</typeparam>
        /// <param name="userProfile">The profile of the user creating the repository.</param>
        /// <returns>An in-memory MeshRepository</returns>
        public static MeshRepository CreateRepository(UserProfile userProfile = null, IEnumerable<Type> domainIncludeAttributes = null, IEnumerable<Assembly> assemblies = null)
        {
            if (userProfile == null) { userProfile = UserProfile.DefaultUser; }
            if (domainIncludeAttributes == null) { domainIncludeAttributes = new Type[] { }; }
            var memoryProvider = new MemoryMeshServiceProvider(StandardLinkForm.LinkKeyFormProvider, StandardDomainForm.DomainValueKeyCreator);
            var includeAssemblies = new List<Assembly>();
            includeAssemblies.AddRange(domainIncludeAttributes.Select(x => x.Assembly));
            if (assemblies != null) { includeAssemblies.AddRange(assemblies); }
            var detail = AssemblyRegistrationDetail.CreateAssemblyRegistrationDetail(includeAssemblies.Distinct().ToArray());
            detail.InitializeDomainProperties = true;
            detail.SetRegistrationCheckerFromAttributes(domainIncludeAttributes.ToArray());
            detail.UniqueNameProvider = TypeToDomainName.DefaultProvider;
            var repository = new MeshRepository(memoryProvider);
            repository.RegisterAssemblies(detail);
            return repository;
        }


        /// <summary>
        /// Creates a builder using the provided parameters.
        /// </summary>
        /// <param name="userProfile">The profile of the user creating the repository.</param>
        /// <param name="assemblies">The assemblies in which to search for domain types.</param>
        /// <param name="includeTypes">The attributes used to include domain types.</param>
        /// <returns>An in-memory MeshRepository</returns>
        public static MemoryMeshRepositoryBuilder CreateBuilder(UserProfile userProfile = null, IEnumerable<Assembly> assemblies = null, IEnumerable<Type> includeTypes = null)
        {
            var builder = new MemoryMeshRepositoryBuilder();
            builder.SeUserProfile(userProfile);
            if (assemblies != null)
            {
                builder.AddAssemblies(assemblies.ToArray());
            }
            if (includeTypes != null)
            {
                builder.AddIncludeAttributes(includeTypes.ToArray());
            }
            return builder;
        }


        /// <summary>
        /// Builds the repository.
        /// </summary>
        /// <returns>The respository</returns>
        public MeshRepository Build()
        {
            var provider = new MemoryMeshServiceProvider(StandardLinkForm.LinkKeyFormProvider, StandardDomainForm.DomainValueKeyCreator);
            var repository = new MeshRepository(provider);
            var detail = new AssemblyRegistrationDetail();
            detail.Assemblies = Assemblies;
            detail.SetRegistrationCheckerFromAttributes(IncludeTypes.ToArray());
            detail.InitializeDomainProperties = true;
            repository.RegisterAssemblies(detail);
            return repository;
        }
    }
}
