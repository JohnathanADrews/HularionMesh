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
using System.Text;

namespace HularionMesh.DomainLink
{
    /// <summary>
    /// Contains the formatting specification for a domain name and link key.
    /// </summary>
    public class DomainLinkForm
    {

        /// <summary>
        /// Creates a link domain name using the provided linked domain names.
        /// </summary>
        /// <param name="domainA">A linked domain.</param>
        /// <param name="domainB">A linked domain.</param>
        /// <returns>The linked domain name.</returns>
        public MeshDomainLink CreateLinkDomain(MeshDomain domainA, MeshDomain domainB)
        {
            var ordered = GetOrderedDomains(domainA, domainB);
            var link = new MeshDomainLink()
            {
                DomainA = ordered.FirstDomain,
                DomainB = ordered.SecondDomain
            };
            link.Key = CreateKey(domainA, domainB);
            return link;
        }

        /// <summary>
        /// Creates a link key using the keys of the the linked values.
        /// </summary>
        /// <param name="domainA">A linked domain.</param>
        /// <param name="domainB">A linked domain.</param>
        /// <returns>A link key using the keys of the the linked values.</returns>
        public MeshKey CreateKey(MeshDomain domainA, MeshDomain domainB, string sMember = null, string tMember = null, string uniqueId = null)
        {
            return CreateKey(domainA.Key, domainB.Key, sMember: sMember, tMember: tMember, uniqueId: uniqueId);
        }

        /// <summary>
        /// Creates a link key using the keys of the the linked values.
        /// </summary>
        /// <param name="aKey">The key of a linked domain or an object of that domain.</param>
        /// <param name="bKey">The key of a linked domain or an object of that domain.</param>
        /// <returns>A link key using the keys of the the linked values.</returns>
        public MeshKey CreateKey(IMeshKey aKey, IMeshKey bKey, string sMember = null, string tMember = null, string uniqueId = null)
        {
            var names = GetOrderedKeys(aKey, bKey);
            var sKey = aKey;
            var tKey = bKey;
            if (names.SecondKey == aKey)
            {
                sKey = bKey;
                tKey = aKey;
            }

            var key = new MeshKey();
            key.SetSKeyPart(sKey);
            key.SetTKeyPart(tKey);
            if (sMember != null || tMember != null || uniqueId != null)
            {
                var extra = new MeshKey();
                key.SetPart(MeshKeyPart.LinkExtra, extra);
                if (sMember != null) { extra.SetSMemberPart(sMember); }
                if (tMember != null) { extra.SetTMemberPart(tMember); }
                if (uniqueId != null) { extra.SetUniquePart(uniqueId); }
            }
            return key;
        }

        /// <summary>
        /// Gets the S-type domain key.
        /// </summary>
        /// <param name="keyA">The key of a linked domain.</param>
        /// <param name="keyB">The key of a linked domain.</param>
        /// <returns>The S-type domain key.</returns>
        public IMeshKey SelectSTypeKey(IMeshKey keyA, IMeshKey keyB)
        {
            var domainA = new MeshDomain() { Key = MeshKey.Parse(keyA) };
            var domainB = new MeshDomain() { Key = MeshKey.Parse(keyB) };
            return GetOrderedDomains(domainA, domainB).FirstDomain.Key;
        }

        /// <summary>
        /// Gets the S-type domain.
        /// </summary>
        /// <param name="domainA">A linked domain.</param>
        /// <param name="domainB">A linked domain.</param>
        /// <returns>The S-type domain.</returns>
        public MeshDomain SelectSTypeDomain(MeshDomain domainA, MeshDomain domainB)
        {
            return GetOrderedDomains(domainA, domainB).FirstDomain;
        }

        /// <summary>
        /// Gets the S-type domain.
        /// </summary>
        /// <param name="names">The names of the domains.</param>
        /// <returns>The S-type domain.</returns>
        public MeshDomain SelectSTypeDomain(LinkedDomains names)
        {
            return GetOrderedDomains(names.DomainA, names.DomainB).FirstDomain;
        }


        /// <summary>
        /// Gets the T-type domain key.
        /// </summary>
        /// <param name="keyA">The key of a linked domain.</param>
        /// <param name="keyB">The key of a linked domain.</param>
        /// <returns>The T-type domain key.</returns>
        public IMeshKey SelectTTypeKey(IMeshKey keyA, IMeshKey keyB)
        {
            var domainA = new MeshDomain() { Key = MeshKey.Parse(keyA) };
            var domainB = new MeshDomain() { Key = MeshKey.Parse(keyB) };
            return GetOrderedDomains(domainA, domainB).SecondDomain.Key;
        }

        /// <summary>
        /// Gets theT-type domain.
        /// </summary>
        /// <param name="domainA">A linked domain.</param>
        /// <param name="domainB">A linked domain.</param>
        /// <returns>The T-type domain.</returns>
        public MeshDomain SelectTTypeDomain(MeshDomain domainA, MeshDomain domainB)
        {
            return GetOrderedDomains(domainA, domainB).SecondDomain;
        }

        /// <summary>
        /// Get T-type domain.
        /// </summary>
        /// <param name="names">The domains.</param>
        /// <returns>The T-type domain.</returns>
        public MeshDomain SelectTTypeDomain(LinkedDomains names)
        {
            return GetOrderedDomains(names.DomainA, names.DomainB).SecondDomain;
        }

        /// <summary>
        /// Chooses and returns the S-Type value.
        /// </summary>
        /// <typeparam name="ValueType">The type of value to return.</typeparam>
        /// <param name="domainA">The name of a linked domain.</param>
        /// <param name="domainB">The name of a linked domain.</param>
        /// <param name="aValue">One of the values to choose from.</param>
        /// <param name="bValue">One of the values to choose from.</param>
        /// <returns>The S-Type value.</returns>
        public ValueType SelectSTypeValue<ValueType>(MeshDomain domainA, MeshDomain domainB, ValueType aValue, ValueType bValue)
        {
            var name = SelectSTypeDomain(domainA, domainB);
            if (name == domainA) { return aValue; }
            return bValue;
        }

        /// <summary>
        /// Chooses and returns the S-Type value.
        /// </summary>
        /// <typeparam name="ValueType">The type of value to return.</typeparam>
        /// <param name="names">The names of the domains.</param>
        /// <param name="aValue">One of the values to choose from.</param>
        /// <param name="bValue">One of the values to choose from.</param>
        /// <returns>The S-Type value.</returns>
        public ValueType SelectSTypeValue<ValueType>(LinkedDomains names, ValueType aValue, ValueType bValue)
        {
            var name = SelectSTypeDomain(names);
            if (name == names.DomainA) { return aValue; }
            return bValue;
        }

        /// <summary>
        /// Chooses and returns the T-Type value.
        /// </summary>
        /// <typeparam name="ValueType">The type of value to return.</typeparam>
        /// <param name="domainA">The name of a linked domain.</param>
        /// <param name="domainB">The name of a linked domain.</param>
        /// <param name="aValue">One of the values to choose from.</param>
        /// <param name="bValue">One of the values to choose from.</param>
        /// <returns>The T-Type value.</returns>
        public ValueType SelectTTypeValue<ValueType>(MeshDomain domainA, MeshDomain domainB, ValueType aValue, ValueType bValue)
        {
            var name = SelectTTypeDomain(domainA, domainB);
            if (name == domainA) { return aValue; }
            return bValue;
        }

        /// <summary>
        /// Chooses and returns the T-Type value.
        /// </summary>
        /// <typeparam name="ValueType">The type of value to return.</typeparam>
        /// <param name="domains">The names of the domains.</param>
        /// <param name="aValue">One of the values to choose from.</param>
        /// <param name="bValue">One of the values to choose from.</param>
        /// <returns>The T-Type value.</returns>
        public ValueType SelectTTypeValue<ValueType>(LinkedDomains domains, ValueType aValue, ValueType bValue)
        {
            var name = SelectTTypeDomain(domains);
            if (name == domains.DomainA) { return aValue; }
            return bValue;
        }

        /// <summary>
        /// Gets the ordered domains given the two provided domains.
        /// </summary>
        /// <param name="domainA">One of the domains.</param>
        /// <param name="domainB">One of the domains.</param>
        /// <returns>The ordered domains given the two provided domains.</returns>
        public OrderedLinkedDomains GetOrderedDomains(MeshDomain domainA, MeshDomain domainB)
        {
            var ordered = new OrderedLinkedDomains() { FirstDomain = domainA, SecondDomain = domainB };
            if (string.CompareOrdinal(domainA.Key.Serialized, domainB.Key.Serialized) > 0)
            {
                ordered.FirstDomain = domainB;
                ordered.SecondDomain = domainA;
            }
            return ordered;
        }

        /// <summary>
        /// Gets the ordered domains given the two provided domains.
        /// </summary>
        /// <param name="aKey">A key that is a domain key or contains a domain key.</param>
        /// <param name="bKey">A key that is a domain key or contains a domain key.</param>
        /// <returns>The ordered domains given the two provided domains.</returns>
        public OrderedLinkedKeys GetOrderedKeys(IMeshKey aKey, IMeshKey bKey)
        {
            var ordered = new OrderedLinkedKeys() { FirstKey = aKey, SecondKey = bKey };
            var aDomainKey = aKey.GetDomainKeyPart();
            var bDomainKey = bKey.GetDomainKeyPart();
            if (string.CompareOrdinal(aDomainKey.Serialized, bDomainKey.Serialized) > 0)
            {
                ordered.FirstKey = bKey;
                ordered.SecondKey = aKey;
            }
            return ordered;
        }

    }
}
