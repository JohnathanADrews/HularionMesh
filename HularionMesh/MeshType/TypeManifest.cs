#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionMesh.DomainValue;
using System;
using System.Collections.Generic;
using System.Text;

namespace HularionMesh.MeshType
{
    /// <summary>
    /// Contains the functions used to convert typed objects to and from domain objects.
    /// </summary>
    public class TypeManifest
    {
        /// <summary>
        /// Converts a domain obejct to a typed object.
        /// </summary>
        public Func<DomainObject, object> ToTypeObject { get; set; }

        /// <summary>
        /// Converts a typed object to a domain object.
        /// </summary>
        public Func<object, DomainObject> ToDomainObject { get; set; }

        /// <summary>
        /// Updates a domain object given a typed object.
        /// </summary>
        public Action<DomainObject, object> UpdateTypeObject { get; set; }

    }
}
