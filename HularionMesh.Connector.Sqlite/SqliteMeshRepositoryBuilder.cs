#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionMesh.Repository;
using HularionMesh.User;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace HularionMesh.Connector.Sqlite
{
    /// <summary>
    /// A repository builder for a Sqlite connector.
    /// </summary>
    public class SqliteMeshRepositoryBuilder
    {
        /// <summary>
        /// The directory for the database.
        /// </summary>
        public string Directory { get; set; }

        /// <summary>
        /// The name (filename) of the database.
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// The suffix of the database. Defaults to ".db"
        /// </summary>
        public string DatabaseSuffix { get; set; } = ".db";

        /// <summary>
        /// The profile of the user creating the repository. Defaults to UserProfile.DefaultUser.
        /// </summary>
        public UserProfile UserProfile { get; set; } = UserProfile.DefaultUser;

        /// <summary>
        /// Iff true, an existing database file will be used. Otherwise, a new file is created. Defaults to true.
        /// </summary>
        public bool UseExisting { get; set; } = true;

        /// <summary>
        /// The assemblies in which to search for types containing an include attribute.
        /// </summary>
        public HashSet<Assembly> AttributeSearchAssemblies { get; private set; } = new HashSet<Assembly>();

        /// <summary>
        /// The mesh include attributes.
        /// </summary>
        public List<Type> Attributes { get; private set; } = new List<Type>();

        /// <summary>
        /// Iff true, the assembly calling Build is used in the build. Default to true.
        /// </summary>
        public bool UseCallingAssembly { get; set; } = true;

        /// <summary>
        /// Sets the directory.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <returns>This builder.</returns>
        public SqliteMeshRepositoryBuilder SetDirectory(string directory)
        {
            Directory = directory;
            return this;
        }

        /// <summary>
        /// Sets the database name.
        /// </summary>
        /// <param name="databaseName">The database name.</param>
        /// <returns>This builder.</returns>
        public SqliteMeshRepositoryBuilder SetDatabaseName(string databaseName)
        {
            DatabaseName = databaseName;
            return this;
        }

        /// <summary>
        /// Sets the database file suffix.
        /// </summary>
        /// <param name="databaseSuffix">The database file suffix.</param>
        /// <returns>This builder.</returns>
        public SqliteMeshRepositoryBuilder SetDatabaseSuffix(string databaseSuffix)
        {
            DatabaseSuffix = databaseSuffix;
            return this;
        }

        /// <summary>
        /// Sets the user profile.
        /// </summary>
        /// <param name="userProfile">The user profile.</param>
        /// <returns>This builder.</returns>
        public SqliteMeshRepositoryBuilder SetUserProfile(UserProfile userProfile)
        {
            UserProfile = userProfile;
            return this;
        }

        /// <summary>
        /// Sets UseExisting.
        /// </summary>
        /// <param name="useExisting">UseExisting</param>
        /// <returns>This builder.</returns>
        public SqliteMeshRepositoryBuilder SetUseExisting(bool useExisting)
        {
            UseExisting = useExisting;
            return this;
        }

        /// <summary>
        /// Sets UseCallingAssembly.
        /// </summary>
        /// <param name="useCaller">UseCallingAssembly</param>
        /// <returns>This builder.</returns>
        public SqliteMeshRepositoryBuilder AddCalligAssembly(bool useCaller)
        {
            UseCallingAssembly = useCaller;
            return this;
        }

        /// <summary>
        /// Adds an assembly to search for types having a mesh include attribute.
        /// </summary>
        /// <param name="assembly">The assembly to add.</param>
        /// <returns>This builder.</returns>
        public SqliteMeshRepositoryBuilder AddAttributeAssembly(Assembly assembly)
        {
            AttributeSearchAssemblies.Add(assembly);
            return this;
        }

        /// <summary>
        /// Adds a mesh include attribute type.
        /// </summary>
        /// <typeparam name="AttributeType">The mesh include attribute type.</typeparam>
        /// <param name="includeAssembly">Iff true, the assembly containing the attribute is included.</param>
        /// <returns>This builder.</returns>
        public SqliteMeshRepositoryBuilder AddAttribute<AttributeType>(bool includeAssembly = false)
        {
            return AddAttribute(typeof(AttributeType), includeAssembly);
        }

        /// <summary>
        /// Adds a mesh include attribute type.
        /// </summary>
        /// <param name="attribute">The mesh include attribute type.</param>
        /// <param name="includeAssembly">Iff true, the assembly containing the attribute is included.</param>
        /// <returns>This builder.</returns>
        public SqliteMeshRepositoryBuilder AddAttribute(Type attribute, bool includeAssembly = false)
        {
            Attributes.Add(attribute);
            if (includeAssembly)
            {
                AttributeSearchAssemblies.Add(attribute.Assembly);
            }
            return this;
        }

        public MeshRepository Build()
        {
            var databaseName = DatabaseName;
            var databaseSuffix = DatabaseSuffix;

            if (String.IsNullOrWhiteSpace(databaseName)) { databaseName = MeshKey.CreateUniqueTag(); }
            if (String.IsNullOrWhiteSpace(databaseSuffix)) { databaseSuffix = ".db"; }
            
            var location = String.Format(@"{0}.{1}", databaseName, databaseSuffix);
            if (!String.IsNullOrWhiteSpace(Directory))
            {
                location = String.Format(@"{0}\{1}.{2}", Directory.Trim(new char[] { '\\' }), databaseName, databaseSuffix);
                if (!System.IO.Directory.Exists(Directory)) { System.IO.Directory.CreateDirectory(Directory); }
            }

            if (!UseExisting && File.Exists(location)) { File.Delete(location); }
            var provider = new SqliteMeshService(String.Format("DataSource={0}", location));

            var assemblies = AttributeSearchAssemblies.ToList();
            if (UseCallingAssembly)
            {
                assemblies.Add(Assembly.GetCallingAssembly());
            }

            var detail = AssemblyRegistrationDetail.CreateAssemblyRegistrationDetail(assemblies.ToArray());
            detail.InitializeDomainProperties = true;
            detail.SetRegistrationCheckerFromAttributes(Attributes.ToArray());
            detail.UniqueNameProvider = TypeToDomainName.DefaultProvider;
            var repository = new MeshRepository(provider);
            repository.RegisterAssemblies(detail);
            return repository;
        }

        /// <summary>
        /// Creates a MeshRepository using a Sqlite store with a standard setup.
        /// </summary>
        /// <typeparam name="IncludeAttributeType">The type of attribute that will cause a class to be added as a domain if that class has the attribute.</typeparam>
        /// <param name="directory">The directory in which to create the SQLite store.</param>
        /// <param name="databaseName">The name of the database file.</param>
        /// <param name="userProfile">The profile of the user creating the repository.</param>
        /// <param name="useExisting">If false, any existing database with that name will be overwritten. If true (default), any current database with the matching name will be used.</param>
        /// <returns>A mesh repository with a SQLite store.</returns>
        public static SqliteMeshRepositoryBuilder CreateBuilder<IncludeAttributeType>(string directory, string databaseName = null, string databaseSuffix = null, UserProfile userProfile = null, bool useExisting = true)
            where IncludeAttributeType : Attribute
        {
            var builder = new SqliteMeshRepositoryBuilder();
            builder.SetDirectory(directory)
                .SetDatabaseName(databaseName)
                .SetDatabaseSuffix(databaseSuffix)
                .SetUserProfile(userProfile)
                .SetUseExisting(useExisting)
                .AddAttribute<IncludeAttributeType>(includeAssembly: true);
            return builder;
        }

        /// <summary>
        /// Creates a MeshRepository using a Sqlite store with a standard setup.
        /// </summary>
        /// <typeparam name="IncludeAttributeType">The type of attribute that will cause a class to be added as a domain if that class has the attribute.</typeparam>
        /// <param name="directory">The directory in which to create the SQLite store.</param>
        /// <param name="databaseName">The name of the database file.</param>
        /// <param name="userProfile">The profile of the user creating the repository.</param>
        /// <param name="useExisting">If false, any existing database with that name will be overwritten. If true (default), any current database with the matching name will be used.</param>
        /// <returns>A mesh repository with a SQLite store.</returns>
        public static MeshRepository CreateRepository<IncludeAttributeType>(string directory, string databaseName = null, string databaseSuffix = null, UserProfile userProfile = null, bool useExisting = true)
            where IncludeAttributeType : Attribute
        {
            var builder = CreateBuilder< IncludeAttributeType>(directory, databaseName: databaseName, databaseSuffix: databaseSuffix, userProfile: userProfile, useExisting: useExisting);
            var repository = builder.Build();
            return repository;
        }



    }
}
