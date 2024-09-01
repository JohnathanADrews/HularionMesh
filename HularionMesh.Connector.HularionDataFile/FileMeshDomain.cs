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
using System.Text;

namespace HularionMesh.Connector.HularionDataFile
{
    /// <summary>
    /// A representation of a MeshDomain for file storage.
    /// </summary>
    public class FileMeshDomain
    {
        /// <summary>
        /// The key of the domain.
        /// </summary>
        public MeshKey Key { get; set; }

        /// <summary>
        /// The generic arguments for this domain.
        /// </summary>
        public string Generics { get; set; }

        /// <summary>
        /// Static values related to the domain.
        /// </summary>
        public IDictionary<string, object> Values { get; set; }

        /// <summary>
        /// The property names of the values.
        /// </summary>
        public List<FileValueProperty> Properties { get; set; } = new List<FileValueProperty>();

        /// <summary>
        /// The property names of the values.
        /// </summary>
        public List<FileValueProperty> Proxies { get; set; } = new List<FileValueProperty>();


        /// <summary>
        /// Constructor
        /// </summary>
        public FileMeshDomain()
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="domain">The domain from which to create this file domain.</param>
        public FileMeshDomain(MeshDomain domain)
        {
            Key = domain.Key;
            Values = domain.Values;
            Generics = domain.SerializedGenerics;
            Properties = domain.Properties.Select(x => new FileValueProperty(x)).ToList();
            Proxies = domain.Proxies.Select(x => new FileValueProperty(x)).ToList();
        }

        public MeshDomain GetDomain()
        {
            var domain = new MeshDomain();
            domain.Key = Key;
            domain.Values = Values;
            domain.GenericsParameters = MeshGeneric.Deserialize(Generics).ToList();
            domain.Properties = Properties.Select(x => x.GetProperty()).ToList();
            return domain;
        }

    }
}
