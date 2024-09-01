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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HularionMesh.SystemDomain
{
    /// <summary>
    /// Contains the details for internal domains used by the mesh system.
    /// </summary>
    public class MeshSystemDomain
    {
        /// <summary>
        /// The unique name for a KeyValuePair.
        /// </summary>
        public const string KeyedValue_KeyPartial = KeyedValue.KeyedValue_KeyPartial;
        /// <summary>
        /// The unique name for a Map.
        /// </summary>
        public const string Map_KeyPartial = Map.Map_KeyPartial;
        /// <summary>
        /// The unique name for a Set.
        /// </summary>
        public const string Set_KeyPartial = UniqueSet.UniqueSet_KeyPartial;

        /// <summary>
        /// The domain key for the system KeyedValue.
        /// </summary>
        public static MeshKey KeyedValueDomainKey { get; private set; }
        /// <summary>
        /// The domain key for the system Map.
        /// </summary>
        public static MeshKey MapDomainKey { get; private set; }
        /// <summary>
        /// The domain key for the system Set.
        /// </summary>
        public static MeshKey SetDomainKey { get; private set; }

        private static List<IMeshKey> domainKeys = new List<IMeshKey>();


        static MeshSystemDomain()
        {
            var systemTypeAttribute = typeof(MeshSystemDomainAttribute);
            var assembly = Assembly.GetExecutingAssembly();
            var systemDomainTypes = assembly.GetTypes().Where(x => x.GetCustomAttributes().Select(y => y.GetType()).Contains(systemTypeAttribute)).ToList();
            foreach(var systemType in systemDomainTypes)
            {
                var attribute = systemType.GetCustomAttribute<MeshSystemDomainAttribute>();
                var domain = new MeshDomain() { UniqueName = attribute.Key };

            }


            KeyedValueDomainKey = (new MeshDomain() { UniqueName = KeyedValue_KeyPartial }).Key;
            domainKeys.Add(KeyedValueDomainKey);

            MapDomainKey = (new MeshDomain() { UniqueName = Map_KeyPartial }).Key;
            domainKeys.Add(MapDomainKey);

            SetDomainKey = (new MeshDomain() { UniqueName = Set_KeyPartial }).Key;
            domainKeys.Add(SetDomainKey);

        }

        /// <summary>
        /// Returns true iff the domain is a mesh system domain.
        /// </summary>
        /// <param name="domain">The domain to check.</param>
        /// <returns>true iff the domain is a mesh system domain.</returns>
        public static bool DomainIsSystem(MeshDomain domain)
        {
            return domainKeys.Contains(domain.Key);
        }

        /// <summary>
        /// Returns true iff the domain is a mesh system domain.
        /// </summary>
        /// <param name="domainKey">The domain key to check.</param>
        /// <returns>true iff the domain is a mesh system domain.</returns>
        public static bool DomainIsSystem(IMeshKey domainKey)
        {
            return domainKeys.Contains(domainKey);
        }

        /// <summary>
        /// Returns true iff the domain is a mesh system domain.
        /// </summary>
        /// <param name="domainKey">The domain key to check.</param>
        /// <returns>true iff the domain is a mesh system domain.</returns>
        public static bool DomainIsSystem(string domainKey)
        {
            return domainKeys.Contains(MeshKey.Parse(domainKey));
        }

    }
}
