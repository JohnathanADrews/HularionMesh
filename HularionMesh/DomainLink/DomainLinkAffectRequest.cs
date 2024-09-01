#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionCore.Pattern;
using HularionCore.Pattern.Identifier;
using HularionMesh.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace HularionMesh.DomainLink
{
    /// <summary>
    /// A request for affecting a domain link.
    /// </summary>
    public class DomainLinkAffectRequest : IKeyedValue<object>
    {
        /// <summary>
        /// The request key.
        /// </summary>
        public object Key { get; set; }

        /// <summary>
        /// The user's key.
        /// </summary>
        public IMeshKey UserKey { get; set; }


        /// <summary>
        /// The mode in which to handle the request.
        /// </summary>
        public LinkAffectMode Mode { get; set; }

        /// <summary>
        /// true iff the linked objects can have only one link related to the given member.
        /// </summary>
        public bool LinkIsExclusive { get; set; } = true;

        /// <summary>
        /// The root node in the where expression for the A-type values. Used in LinkWhere and UnlinkWhere modes.
        /// </summary>
        public WhereExpressionNode WhereA { get; set; }

        /// <summary>
        /// The domain of WhereExpressionNode A.
        /// </summary>
        public MeshDomain DomainA { get; set; }

        /// <summary>
        /// The root node in the where expression for the B-type values. Used in LinkWhere and UnlinkWhere modes.
        /// </summary>
        public WhereExpressionNode WhereB { get; set; }

        /// <summary>
        /// The domain of WhereExpressionNode B.
        /// </summary>
        public MeshDomain DomainB { get; set; }

        /// <summary>
        /// The name of the member of value A that points to value B.
        /// </summary>
        public string AMember { get; set; }

        /// <summary>
        /// The name of the member of value B that points to value A.
        /// </summary>
        public string BMember { get; set; }

        /// <summary>
        /// The object of the A key in LinkKeys and UnlinkKeys modes.
        /// </summary>
        public IMeshKey ObjectAKey { get; set; }

        /// <summary>
        /// The object of the B key in LinkKeys and UnlinkKeys modes.
        /// </summary>
        public IMeshKey ObjectBKey { get; set; }


        public static DomainLinkAffectRequest FromDomainLinker(DomainLinker link)
        {
            var result = new DomainLinkAffectRequest()
            {
                Mode = LinkAffectMode.LinkKeys,
                AMember = link.SMember,
                BMember = link.TMember,
                ObjectAKey = link.SKey,
                ObjectBKey = link.TKey
            };
            return result;
        }
    }


}
