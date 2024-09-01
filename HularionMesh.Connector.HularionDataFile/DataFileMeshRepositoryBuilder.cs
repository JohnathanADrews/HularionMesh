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
using HularionMesh.Memory;
using HularionMesh.Repository;
using HularionMesh.Standard;
using HularionMesh.User;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace HularionMesh.Connector.HularionDataFile
{
    /// <summary>
    /// Creates a mesh repository using a file as a data souce.
    /// </summary>
    public class DataFileMeshRepositoryBuilder
    {
        /// <summary>
        /// Accumulates the details for registrations such as type include attributes and assemblies to search.
        /// </summary>
        public AssemblyRegistrationDetail RegistrationDetail { get; private set; } = new AssemblyRegistrationDetail();

        /// <summary>
        /// The directory in which to create the data file. Should not include the filename.
        /// </summary>
        public string Directory { get; set; }

        /// <summary>
        /// The name of the file to use or create.
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// The extension of the fielname.
        /// </summary>
        public string Extension { get; set; }

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
        /// Constructor.
        /// </summary>
        /// <param name="userProfile">The profile of the user creating the repository. Defaults to UserProfile.DefaultUser.</param>
        public DataFileMeshRepositoryBuilder(UserProfile userProfile = null)
        {
            UserProfile = userProfile;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="directory">The directory in which to create the data file. Should not include the filename.</param>
        /// <param name="filename">The name of the file to use or create.</param>
        /// <param name="userProfile">The profile of the user creating the repository. Defaults to UserProfile.DefaultUser.</param>
        public DataFileMeshRepositoryBuilder(string directory, string filename, UserProfile userProfile = null)
        {
            Directory = directory;
            Filename = filename;
            UserProfile = userProfile;
        }

        /// <summary>
        /// Sets the directory for the mesh file.
        /// </summary>
        /// <param name="directory">The directory for the mesh file.</param>
        /// <returns>this</returns>
        public DataFileMeshRepositoryBuilder SetDirectory(string directory)
        {
            Directory = directory;
            return this;
        }

        /// <summary>
        /// Sets the directory for the mesh file.
        /// </summary>
        /// <returns>this</returns>
        public DataFileMeshRepositoryBuilder SetDirectoryToCallerDirectory()
        {
            var directory = Assembly.GetCallingAssembly().Location;
            Directory = directory.Substring(0, directory.LastIndexOf(@"\"));
            return this;
        }

        /// <summary>
        /// Sets the directory for the mesh file.
        /// </summary>
        /// <returns>this</returns>
        public DataFileMeshRepositoryBuilder SetDirectoryToThisDirectory()
        {
            var directory = Assembly.GetExecutingAssembly().Location;
            Directory = directory.Substring(0, directory.LastIndexOf(@"\"));
            return this;
        }

        /// <summary>
        /// Sets the directory for the mesh file.
        /// </summary>
        /// <returns>this</returns>
        public DataFileMeshRepositoryBuilder SetDirectoryToEntryDirectory()
        {
            var directory = Assembly.GetEntryAssembly().Location;
            Directory = directory.Substring(0, directory.LastIndexOf(@"\"));
            return this;
        }

        /// <summary>
        /// Sets the fielname for the mesh file.
        /// </summary>
        /// <param name="filename">The fielname for the mesh file.</param>
        /// <param name="includesExtension">iff true, the file extension is included in the filename string.</param>
        /// <returns>this</returns>
        public DataFileMeshRepositoryBuilder SetFilename(string filename, bool includesExtension = false)
        {
            if (includesExtension)
            {
                var index = filename.LastIndexOf('.');
                Filename = filename.Substring(0, index);
                Extension = filename.Substring(index + 1, filename.Length - index - 1);
            }
            else
            {
                Filename = filename;
            }
            return this;
        }

        /// <summary>
        /// Sets the extension for the filename. e.g. "hdf" for "filename.hdf"
        /// </summary>
        /// <param name="extension">The extension for the filenam</param>
        /// <returns>this</returns>
        public DataFileMeshRepositoryBuilder SetExtension(string extension)
        {
            this.Extension = extension;
            if (!String.IsNullOrWhiteSpace(Extension))
            {
                this.Extension = this.Extension.Trim('.');
            }
            return this;
        }

        /// <summary>
        /// Sets the user profile used to build the repository.
        /// </summary>
        /// <param name="userProfile">The user profile used to build the repository.</param>
        /// <returns>this</returns>
        public DataFileMeshRepositoryBuilder SeUserProfile(UserProfile userProfile)
        {
            this.UserProfile = userProfile;
            return this;
        }

        /// <summary>
        /// Adds the provided assemblies to the included domain type search.
        /// </summary>
        /// <param name="assemblies">The assemblies to the included domain type search.</param>
        /// <returns>this</returns>
        public DataFileMeshRepositoryBuilder AddAssemblies(params Assembly[] assemblies)
        {
            this.Assemblies.AddRange(assemblies);
            return this;
        }

        /// <summary>
        /// Adds the provided attribute to the included domain attributes.
        /// </summary>
        /// <typeparam name="T">The type of the attribute to add.</typeparam>
        /// <returns>this</returns>
        public DataFileMeshRepositoryBuilder AddIncludeAttribute<T>()
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
        public DataFileMeshRepositoryBuilder AddIncludeAttributes(params Type[] includeTypes)
        {
            this.IncludeTypes.AddRange(includeTypes);
            return this;
        }

        /// <summary>
        /// Builds the repository.
        /// </summary>
        /// <returns>The respository</returns>
        public FileRepository Build()
        {
            var extension = "hdf";
            if (!String.IsNullOrWhiteSpace(Extension))
            {
                var ext = Extension.Trim('.');
                if (!String.IsNullOrWhiteSpace(Extension))
                {
                    extension = ext;
                }
            }
            if (String.IsNullOrWhiteSpace(Filename))
            {
                throw new MeshFileException("The filename was not set.");
            }
            if (String.IsNullOrWhiteSpace(Directory))
            {
                throw new MeshFileException("The filename was not set.");
            }

            var location = String.Format(@"{0}\{1}.{2}", Directory.Trim(new char[] { '\\' }), Filename, extension);

            var file = new FileAccessor(location);
            var provider = new HularionDataFileProvider(file);
            var repository = new MeshRepository(provider);
            var detail = new AssemblyRegistrationDetail();
            detail.Assemblies = Assemblies;
            detail.SetRegistrationCheckerFromAttributes(IncludeTypes.ToArray());
            detail.InitializeDomainProperties = true;
            repository.RegisterAssemblies(detail);
            provider.LoadFile();
            var result = new FileRepository(repository, provider);
            result.FilePath = location;
            result.Directory = Directory;
            return result;
        }


        /// <summary>
        /// Creates a MeshRepository using a Postgres store with a standard setup.
        /// </summary>
        /// <typeparam name="IncludeAttributeType">The type of attribute that will cause a class to be added as a domain if that class has the attribute.</typeparam>
        /// <param name="directory">The directory in which the data file will be placed. DOes not include the filename.</param>
        /// <param name="fileName">The name of the file to create.</param>
        /// <param name="userProfile">The profile of the user creating the repository.</param>
        /// <returns>An in-memory MeshRepository</returns>
        public static FileRepository CreateRepository<IncludeAttributeType>(string directory, string fileName, UserProfile userProfile = null, string extension = null)
            where IncludeAttributeType : Attribute
        {
            var builder = new DataFileMeshRepositoryBuilder();
            builder.SetDirectory(directory);
            builder.SetFilename(fileName);
            builder.SetExtension(extension);
            builder.AddIncludeAttribute<IncludeAttributeType>();
            var result = builder.Build();
            return result;
        }


        /// <summary>
        /// Creates a builder using the provided parameters.
        /// </summary>
        /// <param name="directory">The directory in which the data file will be placed. DOes not include the filename.</param>
        /// <param name="fileName">The name of the file to create.</param>
        /// <param name="extension">The extension of the file to create.</param>
        /// <param name="userProfile">The profile of the user creating the repository.</param>
        /// <param name="assemblies">The assemblies in which to search for domain types.</param>
        /// <param name="includeTypes">The attributes used to include domain types.</param>
        /// <returns>An in-memory MeshRepository</returns>
        public static DataFileMeshRepositoryBuilder CreateBuilder(string directory = null, string fileName = null, string extension = null, UserProfile userProfile = null, IEnumerable<Assembly> assemblies = null, IEnumerable<Type> includeTypes = null)
        {
            var builder = new DataFileMeshRepositoryBuilder();
            builder.SetDirectory(directory);
            builder.SetFilename(fileName);
            builder.SetExtension(extension);
            builder.SeUserProfile(userProfile);
            if(assemblies != null)
            {
                builder.AddAssemblies(assemblies.ToArray());
            }
            if(includeTypes != null)
            {
                builder.AddIncludeAttributes(includeTypes.ToArray());
            }
            return builder;
        }

        /// <summary>
        /// Creates a MeshRepository using a Postgres store with a standard setup.
        /// </summary>
        /// <param name="directory">The directory in which the data file will be placed. DOes not include the filename.</param>
        /// <param name="fileName">The name of the file to create.</param>
        /// <param name="extension">The extension of the file.</param>
        /// <param name="registrationDetail">The registration detail for the repository.</param>
        /// <param name="userProfile">The profile of the user creating the repository.</param>
        /// <returns>An in-memory MeshRepository</returns>
        //public static FileRepository CreateRepository(string directory, string fileName, AssemblyRegistrationDetail registrationDetail, UserProfile userProfile = null, string extension = null)
        //{
        //    if(extension == null) { extension = "hdf"; }
        //    var location = String.Format(@"{0}\{1}.{2}", directory.Trim(new char[] { '\\' }), fileName, extension);
        //    var file = new FileAccessor(location);
        //    var provider = new HularionDataFileProvider(file);
        //    var repository = new MeshRepository(provider);
        //    repository.RegisterAssemblies(registrationDetail);
        //    provider.LoadFile();
        //    var result = new FileRepository(repository, provider);
        //    return result;

        //}


    }
}
