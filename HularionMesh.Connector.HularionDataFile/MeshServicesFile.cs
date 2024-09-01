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
using HularionMesh.DomainLink;
using HularionMesh.DomainValue;
using System;
using System.Collections.Generic;
using System.Text;

namespace HularionMesh.Connector.HularionDataFile
{
    /// <summary>
    /// Contains mesh data used to read to or write from a file.
    /// </summary>
    public class MeshServicesFile
    {
        /// <summary>
        /// The serializable mesh domains.
        /// </summary>
        public IList<FileMeshDomain> Domains { get; set; }

        /// <summary>
        /// The serializable domain objects.
        /// </summary>
        public IList<DomainObject> Objects { get; set; }

        /// <summary>
        /// The serializable object links.
        /// </summary>
        public IList<DomainLinker> Links { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public MeshServicesFile()
        {
            Domains = new List<FileMeshDomain>();
            Objects = new List<DomainObject>();
            Links = new List<DomainLinker>();
        }
    }
}
