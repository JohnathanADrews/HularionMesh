#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace HularionMesh
{
    /// <summary>
    /// Defines the key parts in a mesh.
    /// </summary>
    public class MeshKeyPart
    {
        /// <summary>
        /// The name of the key part.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The key part for a domain object.
        /// </summary>
        public static MeshKeyPart Object = new MeshKeyPart() { Name = "Object" };

        /// <summary>
        /// The key part for a domain.
        /// </summary>
        public static MeshKeyPart Domain = new MeshKeyPart() { Name = "Domain" };

        /// <summary>
        /// The key part for a domain.
        /// </summary>
        public static MeshKeyPart Unique = new MeshKeyPart() { Name = "U" };

        /// <summary>
        /// The s-key key part for a link domain key.
        /// </summary>
        public static MeshKeyPart SKey = new MeshKeyPart() { Name = "SKey" };

        /// <summary>
        /// The t-key key part for a link domain key.
        /// </summary>
        public static MeshKeyPart TKey = new MeshKeyPart() { Name = "TKey" };

        /// <summary>
        /// The s-member part for a link domain key.
        /// </summary>
        public static MeshKeyPart SMember = new MeshKeyPart() { Name = "SMember" };

        /// <summary>
        /// The t-member part for a link domain key.
        /// </summary>
        public static MeshKeyPart TMember = new MeshKeyPart() { Name = "TMember" };

        /// <summary>
        /// The extra parts of a link key, following the SKey and TKey.
        /// </summary>
        public static MeshKeyPart LinkExtra = new MeshKeyPart() { Name = "X" };


        public override string ToString()
        {
            return Name;
        }
    }
}
