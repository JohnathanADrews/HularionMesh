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
using System;
using System.Collections.Generic;
using System.Text;

namespace HularionMesh.Structure
{
    /// <summary>
    /// A store of values that manages links among objects.
    /// </summary>
    public interface IDomainLinkStore
    {

        /// <summary>
        /// The S-type domain.
        /// </summary>
        MeshDomain STypeDomain { get; }
        /// <summary>
        /// The T-type domain.
        /// </summary>
        MeshDomain TTypeDomain { get; }
        /// <summary>
        /// The linking domain.
        /// </summary>
        MeshDomainLink LinkDomain { get; }
        /// <summary>
        /// The formatting specification for the link keys.
        /// </summary>
        DomainLinkForm LinkForm { get; }


        /// <summary>
        /// Links the objects with the provided keys if they are not already linked.
        /// </summary>
        /// <param name="aKeys">The keys of the S-type objects or T-type objects.</param>
        /// <param name="bKeys">The keys of the S-type objects or T-type objects.</param>
        /// <param name="sMemberName">The name of the member of the S-type objects that is ocupied by T-type objects.</param>
        /// <param name="tMemberName">The name of the member of the T-type objects that is ocupied by S-type objects.</param>
        /// <param name="userKey">The key of the user making the link request.</param>
        /// <remarks>The implementer should use MeshDomain.KeyIsObjectInThisDomain() to determine which of aKeys and bKeys are S-type keys and T-type keys.</remarks>
        IEnumerable<DomainLinker> Link(IEnumerable<IMeshKey> aKeys, IEnumerable<IMeshKey> bKeys, string sMemberName = null, string tMemberName = null, IMeshKey userKey = null);

        /// <summary>
        /// Unlinks the objects with the provided keys.
        /// </summary>
        /// <param name="aKeys">The keys of the S-type objects.</param>
        /// <param name="bKeys">The keys of the T-type objects.</param>
        /// <param name="userKey">The key of the user making the link request.</param>
        /// <remarks>The implementer should use MeshDomain.KeyIsObjectInThisDomain() to determine which of aKeys and bKeys are S-type keys and T-type keys.</remarks>
        void UnLink(IEnumerable<IMeshKey> aKeys, IEnumerable<IMeshKey> bKeys, string sMemberName = null, string tMemberName = null, IMeshKey userKey = null);

        /// <summary>
        /// Gets the link objects of the linked domain that are related to each of the provided keys.
        /// </summary>
        /// <param name="keys">The keys of the subject domain for which the related linked keys will be provided.</param>
        /// <returns>The keys of the linked domain that are related to each of the provided keys.</returns>
        /// <remarks>If A1 is linked to B1, B2, and C1, then given A1.Key, the result is (A1.Key,{ A1-B1, A1-B2, A1-C1 }).</remarks>
        IDictionary<IMeshKey, IEnumerable<DomainLinker>> GetLinkedLinkers(IEnumerable<IMeshKey> keys);

        /// <summary>
        /// Gets the keys of the linked domain that are related to each of the provided keys.
        /// </summary>
        /// <param name="keys">The keys of the domain for which the related linked keys will be provided. If "keys" are for "DomainA", the resutl will be keys for "DomainB".</param>
        /// <returns>The keys of the linked domain that are related to each of the provided keys.</returns>
        /// <remarks>If A1 is linked to B1, B2, and C1, then given A1.Key, the result is (A1.Key,{ B1.Key, B2.Key, C1.Key }).</remarks>
        IDictionary<IMeshKey, IEnumerable<IMeshKey>> GetLinkedKeys(IEnumerable<IMeshKey> keys);

        /// <summary>
        /// Gets the linkers for the given linked keys and member name.
        /// </summary>
        /// <param name="linkedKeys">The S-type keys or T-type keys.</param>
        /// <param name="memberName">The name of the member for the given key.</param>
        /// <returns>The linkers for the links with the matching keys and member names.</returns>
        IDictionary<IMeshKey, IEnumerable<DomainLinker>> GetLinks(IEnumerable<IMeshKey> linkedKeys, string memberName);

        /// <summary>
        /// Gets the linkers with the given keys.
        /// </summary>
        /// <param name="linkerKeys">The keys associated with the linker objects.</param>
        /// <returns>The linkers with the given keys.</returns>
        IEnumerable<DomainLinker> GetLinks(params IMeshKey[] linkerKeys);

        /// <summary>
        /// Gets all linkers using "where".
        /// </summary>
        /// <param name="where">The search criteria.</param>
        /// <returns>The matching linkers.</returns>
        IEnumerable<DomainLinker> GetLinkers(WhereExpressionNode where);

        /// <summary>
        /// Finds all linkers using "where" and then returns the domain keys for the given domain.
        /// </summary>
        /// <param name="domain">The domain of the keys to return.</param>
        /// <param name="where">The search criteria.</param>
        /// <returns>The "domain" object keys present in the found linkers.</returns>
        IEnumerable<IMeshKey> GetDomainObjectKeys(MeshDomain domain, WhereExpressionNode where);


    }
}
