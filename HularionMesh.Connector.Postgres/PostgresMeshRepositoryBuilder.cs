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
using HularionMesh.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace HularionMesh.Connector.Postgres
{
    /// <summary>
    /// A repository builder for a Postgres connector.
    /// </summary>
    public class PostgresMeshRepositoryBuilder
    {
        /// <summary>
        /// The details for registering an assembly.
        /// </summary>
        public AssemblyRegistrationDetail RegistrationDetail { get; set; } = new AssemblyRegistrationDetail();

        /// <summary>
        /// The connection string to the database server/
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// The name of the database.
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// The profile of the user connecting to the database.
        /// </summary>
        public UserProfile UserProfile { get; set; }


        /// <summary>
        /// Creates a MeshRepository using a Postgres store with a standard setup.
        /// </summary>
        /// <typeparam name="IncludeAttributeType">The type of attribute that will cause a class to be added as a domain if that class has the attribute.</typeparam>
        /// <param name="userProfile">The profile of the user creating the repository.</param>
        /// <returns>An in-memory MeshRepository</returns>
        public static MeshRepository CreateRepository<IncludeAttributeType>(string connectionString, string databaseName = null, UserProfile userProfile = null)
            where IncludeAttributeType : Attribute
        {
            if (userProfile == null) { userProfile = UserProfile.DefaultUser; }
            var dbRegex = new Regex("database=.*(;|)");
            var createConnection = dbRegex.Replace(connectionString, string.Empty);
            var dbMatches = dbRegex.Matches(connectionString);
            if (databaseName == null)
            {
                if (dbMatches.Count > 0)
                {
                    databaseName = (string)dbMatches[0].Value;
                    databaseName = databaseName.Replace("database=", string.Empty).Trim(new char[] { ';' });
                }
                else { databaseName = String.Format("DB{0}", MeshKey.CreateUniqueTag()).ToLower(); }
            }
            databaseName = databaseName.ToLower();
            PostgresRepository.CreateDatabase(createConnection, databaseName);
            connectionString = String.Format("{0};database={1};", createConnection, databaseName);
            var provider = new PostgresMeshService(connectionString);
            var detail = AssemblyRegistrationDetail.CreateAssemblyRegistrationDetail(Assembly.GetCallingAssembly());
            detail.InitializeDomainProperties = true;
            detail.SetRegistrationCheckerFromAttributes<IncludeAttributeType>();
            detail.UniqueNameProvider = TypeToDomainName.DefaultProvider;
            var repository = new MeshRepository(provider);
            repository.RegisterAssemblies(detail);
            return repository;
        }

        /// <summary>
        /// Creates a MeshRepository using a Postgres store with a standard setup.
        /// </summary>
        /// <typeparam name="IncludeAttributeType">The type of attribute that will cause a class to be added as a domain if that class has the attribute.</typeparam>
        /// <param name="userProfile">The profile of the user creating the repository.</param>
        /// <returns>An in-memory MeshRepository</returns>
        public static MeshRepository CreateRepository(string connectionString, IEnumerable<Type> includeAttributes = null, IEnumerable<Assembly> includeAssemblies = null, string databaseName = null, UserProfile userProfile = null, bool includeAttributeAssemblies = true, bool includeCallingAssembly = true)
        {
            if (includeAttributes == null) { includeAttributes = new Type[] { }; }
            if (includeAssemblies == null) { includeAssemblies = new Assembly[] { }; }
            if (userProfile == null) { userProfile = UserProfile.DefaultUser; }
            var dbRegex = new Regex("database=.*(;|)");
            var createConnection = dbRegex.Replace(connectionString, string.Empty);
            var dbMatches = dbRegex.Matches(connectionString);
            if (databaseName == null)
            {
                if (dbMatches.Count > 0)
                {
                    databaseName = (string)dbMatches[0].Value;
                    databaseName = databaseName.Replace("database=", string.Empty).Trim(new char[] { ';' });
                }
                else { databaseName = String.Format("DB{0}", MeshKey.CreateUniqueTag()).ToLower(); }
            }
            databaseName = databaseName.ToLower();
            PostgresRepository.CreateDatabase(createConnection, databaseName);
            connectionString = String.Format("{0};database={1};", createConnection, databaseName);
            var provider = new PostgresMeshService(connectionString);
            var assemblies = new HashSet<Assembly>(includeAssemblies);
            if (includeCallingAssembly) { assemblies.Add(Assembly.GetCallingAssembly()); }
            if (includeAttributeAssemblies)
            {
                foreach(var attributeType in includeAttributes) { assemblies.Add(attributeType.Assembly); }
            }
            var detail = AssemblyRegistrationDetail.CreateAssemblyRegistrationDetail(assemblies.ToArray());
            detail.InitializeDomainProperties = true;
            detail.SetRegistrationCheckerFromAttributes(includeAttributes.ToArray());
            detail.UniqueNameProvider = TypeToDomainName.DefaultProvider;
            var repository = new MeshRepository(provider);
            repository.RegisterAssemblies(detail);
            return repository;
        }

        /// <summary>
        /// Creates the repository using the values set in this.
        /// </summary>
        /// <returns>A MeshRepository.</returns>
        public MeshRepository Create()
        {
            if (UserProfile == null) { UserProfile = UserProfile.DefaultUser; }
            var dbRegex = new Regex("database=.*(;|)");
            var createConnection = dbRegex.Replace(ConnectionString, string.Empty);
            var dbMatches = dbRegex.Matches(ConnectionString);
            var databaseName  = DatabaseName;
            if (databaseName == null)
            {
                if (dbMatches.Count > 0)
                {
                    databaseName = (string)dbMatches[0].Value;
                    databaseName = databaseName.Replace("database=", string.Empty).Trim(new char[] { ';' });
                }
                else { databaseName = String.Format("DB{0}", MeshKey.CreateUniqueTag()).ToLower(); }
            }
            databaseName = databaseName.ToLower();
            PostgresRepository.CreateDatabase(createConnection, databaseName);
            var connectionString = String.Format("{0};database={1};", createConnection, databaseName);
            RegistrationDetail.InitializeDomainProperties = true;
            var provider = new PostgresMeshService(connectionString);
            var repository = new MeshRepository(provider);
            repository.RegisterAssemblies(RegistrationDetail);
            repository.TypeRegistrar.InitializeProperties(repository.DomainMechanicProvider);
            return repository;
        }
    }
}
