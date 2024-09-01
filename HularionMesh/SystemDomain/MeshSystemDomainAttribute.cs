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
using System;
using System.Collections.Generic;
using System.Text;

namespace HularionMesh.SystemDomain
{
    /// <summary>
    /// An attribute indicating that the type represents a system domain.
    /// </summary>
    internal class MeshSystemDomainAttribute : MeshRepositoryDomainAttribute
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="key">The domain key.</param>
        public MeshSystemDomainAttribute(string key) : base(key)
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="key">The domain key.</param>
        /// <param name="name">The name of the domain.</param>
        public MeshSystemDomainAttribute(string key, string name) : base(key, name)
        {

        }
    }
}
