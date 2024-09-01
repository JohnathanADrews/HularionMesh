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
    /// Maps a C# type property to a domain object property
    /// </summary>
    public enum DomainObjectPropertySelector
    {
        /// <summary>
        /// The key of the domain object.  Must be a string, a MeshKey, or implement IMeshKey.
        /// </summary>
        Key,
        /// <summary>
        /// The generics tree with the specified generic types.  Must be a TypeGeneric[].
        /// </summary>
        Generic,
        /// <summary>
        /// The serialized generic type tree.  Must be a string.
        /// </summary>
        SerializedGeneric,
        /// <summary>
        /// The key of user to create the domain object. Must be a string, a MeshKey, or implement IMeshKey.
        /// </summary>
        Creator,
        /// <summary>
        /// The key of user to last update the domain object. Must be a string, a MeshKey, or implement IMeshKey.
        /// </summary>
        Updater,
        /// <summary>
        /// The date/time the domain object was created. Must be a string, an int, a long, or a DateTime.
        /// </summary>
        CreationTime,
        /// <summary>
        /// The date/time the domain object was last updated.  Must be a string, an int, a long, or a DateTime.
        /// </summary>
        UpdateTime
    }
}
