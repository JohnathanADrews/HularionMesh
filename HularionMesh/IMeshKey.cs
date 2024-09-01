#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionCore.Pattern.Set;
using System;
using System.Collections.Generic;
using System.Text;

namespace HularionMesh
{
    /// <summary>
    /// Represents a key that can contain many component parts.
    /// </summary>
    public interface IMeshKey
    {
        /// <summary>
        /// The serialized version of the key.
        /// </summary>
        string Serialized { get; set; }

        /// <summary>
        /// Returns true iff this key's Serialized value equals the provided keys Serialized value.
        /// </summary>
        /// <param name="key">The key to compare to.</param>
        /// <returns>true iff this key's Serialized value equals the provided keys Serialized value.</returns>
        bool EqualsKey(IMeshKey key);

        /// <summary>
        /// True if the key has no value.
        /// </summary>
        bool IsNull { get; }

        /// <summary>
        /// Sets the part of the key indicated by partKey.
        /// </summary>
        /// <param name="partKey">The part of the key to set.</param>
        /// <param name="partValue">The value to associate with the part.</param>
        /// <returns>this IMeshKey</returns>
        IMeshKey SetPart(MeshKeyPart partKey, string partValue);

        /// <summary>
        /// Sets the part of the key indicated by partKey.
        /// </summary>
        /// <param name="partKey">The part of the key to set.</param>
        /// <param name="partValue">The value to associate with the part.</param>
        /// <returns>this IMeshKey</returns>
        IMeshKey SetPart(string partKey, string partValue);

        /// <summary>
        /// Sets the s-type key part for this key.
        /// </summary>
        /// <param name="partValue">The value of the s-type key.</param>
        /// <returns>this IMeshKey</returns>
        IMeshKey SetSKeyPart(IMeshKey partValue);

        /// <summary>
        /// Sets the s-type member part for this key.
        /// </summary>
        /// <param name="partValue">The name of the s-type member.</param>
        /// <returns>this IMeshKey</returns>
        IMeshKey SetSMemberPart(string partValue);

        /// <summary>
        /// Sets the t-type key part for this key.
        /// </summary>
        /// <param name="partValue">The value of the t-type key.</param>
        /// <returns>this IMeshKey</returns>
        IMeshKey SetTKeyPart(IMeshKey partValue);

        /// <summary>
        /// Sets the t-type member part for this key.
        /// </summary>
        /// <param name="partValue">The name of the t-type member.</param>
        /// <returns>this IMeshKey</returns>
        IMeshKey SetTMemberPart(string partValue);

        /// <summary>
        /// Sets the unique part of the key to a newly generated unique string value.
        /// </summary>
        /// <returns>this IMeshKey</returns>
        IMeshKey SetUniquePart();

        /// <summary>
        /// Sets the unique part of the key to the given unique string value.
        /// </summary>
        /// <param name="partValue">The unique string value.</param>
        /// <returns>this IMeshKey</returns>
        IMeshKey SetUniquePart(string partValue);

        /// <summary>
        /// Clones this, returning the copy.
        /// </summary>
        /// <returns>The cloned key.</returns>
        IMeshKey Clone();

        /// <summary>
        /// Gets the partial corresponding to the provided part.
        /// </summary>
        /// <param name="part">The part of the key to get.</param>
        /// <returns>The partial of the key.</returns>
        IMeshKey GetKeyPart(MeshKeyPart part);

        /// <summary>
        /// Gets the part of the key that indicates the domain and creates a new key from it.
        /// </summary>
        /// <returns>The domain key part of this key.</returns>
        IMeshKey GetDomainKeyPart();
    }
}
