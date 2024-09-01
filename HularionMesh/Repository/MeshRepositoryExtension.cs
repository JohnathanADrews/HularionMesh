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

namespace HularionMesh.Repository
{
    /// <summary>
    /// Extensions for MeshRepository
    /// </summary>
    public static class MeshRepositoryExtension
    {

        /// <summary>
        /// Creates the domain operation link for the provided type.
        /// </summary>
        /// <param name="type">The type used create the operation link.</param>
        /// <returns>The domain operation link for the provided type.</returns>
        public static DomainOperationLink CreateOperationLink(this IMeshRepository repository, Type type)
        {
            return repository.TypeOperationLinkProvider.Provide(type);
        }

        /// <summary>
        /// Creates the operation link for the provided type.
        /// </summary>
        /// <typeparam name="T">The type used create the operation link.</typeparam>
        /// <returns>The domain operation link for the provided type.</returns>
        public static DomainOperationLink CreateOperationLink<T>(this IMeshRepository repository)
        {
            return repository.TypeOperationLinkProvider.Provide(typeof(T));
        }

    }
}
