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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HularionMesh.Repository
{
    /// <summary>
    /// The details used to register an assembly.
    /// </summary>
    public class AssemblyRegistrationDetail
    {
        /// <summary>
        /// The assemblies to register.
        /// </summary>
        public List<Assembly> Assemblies = new List<Assembly>();

        /// <summary>
        /// Required.  If true is returned for a given type, the type will be added to the repository.
        /// </summary>
        public IParameterizedProvider<Type, bool> RegistrationChecker { get; set; }

        /// <summary>
        /// Required.  Provides the domain key for the type domain.
        /// </summary>
        public IParameterizedProvider<Type, string> UniqueNameProvider { get; set; } = TypeToDomainName.DefaultProvider;

        /// <summary>
        /// Optional.  Provides name aliases for the domain with the given unique name.  This allows an operation to use an alias rather than a key.  All aliases in the repository must be unique.
        /// </summary>
        public IParameterizedProvider<Type, IEnumerable<string>> AliasesProvider { get; set; }

        /// <summary>
        /// Optional.  Provides custom values for type domain.  For example, "name" or "description".
        /// </summary>
        public IParameterizedProvider<Type, IDictionary<string, object>> ValuesProvider { get; set; }

        /// <summary>
        /// Defaults to true.  Iff true, the domain properties of the registered domains are initialized.  
        /// If there are property references in other assemblies, set this to false and then call myRepository.TypeRegistrar.InitializeProperties() after all assemblies are registered.
        /// </summary>
        public bool InitializeDomainProperties { get; set; } = true;

        /// <summary>
        /// Sets the registration checker to check for the provided domain registration attributes.
        /// </summary>
        /// <param name="attributes">The attributes to set.</param>
        public void SetRegistrationCheckerFromAttributes(params Type[] attributes)
        {
            this.RegistrationChecker = RegistrationCheckerFromAttributes(attributes);
        }

        /// <summary>
        /// Sets the registration checker to check for the provided domain registration attributes.
        /// </summary>
        /// <typeparam name="AttributeType">The type of the attribute to set.</typeparam>
        public void SetRegistrationCheckerFromAttributes<AttributeType>()
            where AttributeType : Attribute
        {
            this.RegistrationChecker = RegistrationCheckerFromAttribute<AttributeType>();
        }

        /// <summary>
        /// Sets the registration checker to check for the provided domain registration attributes.
        /// </summary>
        /// <typeparam name="AttributeType1">The type of an attribute to set.</typeparam>
        /// <typeparam name="AttributeType2">The type of an attribute to set.</typeparam>
        public void SetRegistrationCheckerFromAttributes<AttributeType1, AttributeType2>()
            where AttributeType1 : Attribute
            where AttributeType2 : Attribute
        {
            this.RegistrationChecker = RegistrationCheckerFromAttributes<AttributeType1, AttributeType2>();
        }

        /// <summary>
        /// Sets the registration checker to check for the provided domain registration attributes.
        /// </summary>
        /// <typeparam name="AttributeType1">The type of an attribute to set.</typeparam>
        /// <typeparam name="AttributeType2">The type of an attribute to set.</typeparam>
        /// <typeparam name="AttributeType3">The type of an attribute to set.</typeparam>
        public void SetRegistrationCheckerFromAttributes<AttributeType1, AttributeType2, AttributeType3>()
            where AttributeType1 : Attribute
            where AttributeType2 : Attribute
            where AttributeType3 : Attribute
        {
            this.RegistrationChecker = RegistrationCheckerFromAttributes<AttributeType1, AttributeType2, AttributeType3>();
        }

        /// <summary>
        /// Sets the registration checker to check for the provided domain registration attributes.
        /// </summary>
        /// <typeparam name="AttributeType1">The type of an attribute to set.</typeparam>
        /// <typeparam name="AttributeType2">The type of an attribute to set.</typeparam>
        /// <typeparam name="AttributeType3">The type of an attribute to set.</typeparam>
        /// <typeparam name="AttributeType4">The type of an attribute to set.</typeparam>
        public void SetRegistrationCheckerFromAttributes<AttributeType1, AttributeType2, AttributeType3, AttributeType4>()
            where AttributeType1 : Attribute
            where AttributeType2 : Attribute
            where AttributeType3 : Attribute
            where AttributeType4 : Attribute
        {
            this.RegistrationChecker = RegistrationCheckerFromAttributes<AttributeType1, AttributeType2, AttributeType3, AttributeType4>();
        }

        /// <summary>
        /// Sets the registration checker to check for the provided domain registration attributes.
        /// </summary>
        /// <typeparam name="AttributeType1">The type of an attribute to set.</typeparam>
        /// <typeparam name="AttributeType2">The type of an attribute to set.</typeparam>
        /// <typeparam name="AttributeType3">The type of an attribute to set.</typeparam>
        /// <typeparam name="AttributeType4">The type of an attribute to set.</typeparam>
        /// <typeparam name="AttributeType5">The type of an attribute to set.</typeparam>
        public void SetRegistrationCheckerFromAttributes<AttributeType1, AttributeType2, AttributeType3, AttributeType4, AttributeType5>()
            where AttributeType1 : Attribute
            where AttributeType2 : Attribute
            where AttributeType3 : Attribute
            where AttributeType4 : Attribute
            where AttributeType5 : Attribute
        {
            this.RegistrationChecker = RegistrationCheckerFromAttributes<AttributeType1, AttributeType2, AttributeType3, AttributeType4, AttributeType5>();
        }

        /// <summary>
        /// Returns a registration checker that returns true iff the type has one of the provided attributes.
        /// </summary>
        /// <param name="attributes">The attributes to check for.</param>
        /// <returns>A registration checker that returns true iff the type has one of the provided attributes.</returns>
        public static IParameterizedProvider<Type, bool> RegistrationCheckerFromAttributes(params Type[] attributes)            
        {
            return ParameterizedProvider.FromSingle<Type, bool>(type => 
            {
                foreach(var typeAttribute in type.GetCustomAttributes().Select(x=>x.GetType()))
                {
                    foreach (var attrtibute in attributes)
                    {
                        if (typeAttribute == attrtibute) 
                        { 
                            return true; 
                        }
                    }
                }
                return false;
            });
        }

        public static IParameterizedProvider<Type, bool> RegistrationCheckerFromAttribute<AttributeType>()
            where AttributeType : Attribute
        {
            return RegistrationCheckerFromAttributes(typeof(AttributeType));
        }

        public static IParameterizedProvider<Type, bool> RegistrationCheckerFromAttributes<AttributeType1, AttributeType2>()
            where AttributeType1 : Attribute
            where AttributeType2 : Attribute
        {
            return RegistrationCheckerFromAttributes(typeof(AttributeType1), typeof(AttributeType2));
        }

        public static IParameterizedProvider<Type, bool> RegistrationCheckerFromAttributes<AttributeType1, AttributeType2, AttributeType3>()
            where AttributeType1 : Attribute
            where AttributeType2 : Attribute
            where AttributeType3 : Attribute
        {
            return RegistrationCheckerFromAttributes(typeof(AttributeType1), typeof(AttributeType2), typeof(AttributeType3));
        }

        public static IParameterizedProvider<Type, bool> RegistrationCheckerFromAttributes<AttributeType1, AttributeType2, AttributeType3, AttributeType4>()
            where AttributeType1 : Attribute
            where AttributeType2 : Attribute
            where AttributeType3 : Attribute
            where AttributeType4 : Attribute
        {
            return RegistrationCheckerFromAttributes(typeof(AttributeType1), typeof(AttributeType2), typeof(AttributeType3), typeof(AttributeType4));
        }

        public static IParameterizedProvider<Type, bool> RegistrationCheckerFromAttributes<AttributeType1, AttributeType2, AttributeType3, AttributeType4, AttributeType5>()
            where AttributeType1 : Attribute
            where AttributeType2 : Attribute
            where AttributeType3 : Attribute
            where AttributeType4 : Attribute
            where AttributeType5 : Attribute
        {
            return RegistrationCheckerFromAttributes(typeof(AttributeType1), typeof(AttributeType2), typeof(AttributeType3), typeof(AttributeType4), typeof(AttributeType5));
        }

        /// <summary>
        /// Creates an AssemblyRegistrationDetail.
        /// </summary>
        /// <param name="assemblies">The assemblies to include.</param>
        /// <returns>The AssemblyRegistrationDetail.</returns>
        public static AssemblyRegistrationDetail CreateAssemblyRegistrationDetail(params Assembly[] assemblies)
        {
            var detail = new AssemblyRegistrationDetail() { };
            detail.Assemblies.AddRange(assemblies);
            detail.SetRegistrationCheckerFromAttributes<MeshRepositoryDomainAttribute>();
            var defaultNameProvider = detail.UniqueNameProvider;
            detail.UniqueNameProvider = ParameterizedProvider.FromSingle<Type, string>(type =>
            {
                var attribute = type.GetTypeInfo().GetCustomAttributes().Where(x => x.GetType() == typeof(MeshRepositoryDomainAttribute)).FirstOrDefault();
                if (attribute == null) { return defaultNameProvider.Provide(type); }
                return ((MeshRepositoryDomainAttribute)attribute).Key;
            });
            detail.ValuesProvider = ParameterizedProvider.FromSingle<Type, IDictionary<string, object>>(type =>
            {
                var attribute = type.GetTypeInfo().GetCustomAttributes().Where(x => x.GetType() == typeof(MeshRepositoryDomainAttribute)).FirstOrDefault();
                if (attribute == null) { return null; }
                return ((MeshRepositoryDomainAttribute)attribute).Values;
            });
            return detail;
        }

    }
}
