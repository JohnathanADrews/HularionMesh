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
using System;
using System.Collections.Generic;
using System.Text;

namespace HularionMesh.Standard
{
    /// <summary>
    /// Standard implementations for domain representations.
    /// </summary>
    public class StandardDomainForm
    {

        ///// <summary>
        ///// A standard key creator for domain values.
        ///// </summary>
        //public static IParameterizedCreator<MeshDomain, IMeshKey> DomainValueKeyCreator =
        //    ParameterizedCreator.FromSingle<MeshDomain, IMeshKey>(domain => { return domain.Key.Clone().Append(MeshKey.CreateUniqueTagKey()); });

        /// <summary>
        /// A standard key creator for domain values.
        /// </summary>
        public static IParameterizedCreator<MeshDomain, IMeshKey> DomainValueKeyCreator =
            ParameterizedCreator.FromSingle<MeshDomain, IMeshKey>(domain => { return domain.Key.Clone().SetPart(MeshKeyPart.Unique,MeshKey.CreateUniqueTag()); });

        /// <summary>
        /// Creates a key creator for the specified domain.
        /// </summary>
        /// <param name="domain">The domain for which to create the key.</param>
        /// <returns>A key creator for the specified domain.</returns>
        public static ICreator<IMeshKey> CreateDomainValueKeyCreator(MeshDomain domain)
        {
            return new CreatorFunction<IMeshKey>(()=> DomainValueKeyCreator.Create(domain));
        }
    }
}
