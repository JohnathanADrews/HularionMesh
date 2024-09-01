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
    /// Contains the domains in a link.
    /// </summary>
    public class LinkedDomains
    {
        /// <summary>
        /// A domain in the link.  It could be either the S-type domain or the T-type domain.  It is left to the user of this class to determine which it is.
        /// </summary>
        public MeshDomain DomainA { get; set; }

        /// <summary>
        /// A domain in the link.  It could be either the S-type domain or the T-type domain.  It is left to the user of this class to determine which it is.
        /// </summary>
        public MeshDomain DomainB { get; set; }

        Type thistype = typeof(LinkedDomains);


        public override bool Equals(object obj)
        {
            if(obj == null || obj.GetType() != thistype) { return false; }
            var ld = (LinkedDomains)obj;
            return (DomainA == ld.DomainA && DomainB == ld.DomainB);
        }

        public static bool operator ==(LinkedDomains domainA, LinkedDomains domainB)
        {
            if (Object.ReferenceEquals(domainA, null))
            {
                if (Object.ReferenceEquals(domainB, null)) { return true; }
                return false;
            }
            if (Object.ReferenceEquals(domainB, null)) { return false; }
            return domainA.Equals(domainB);
        }

        public static bool operator !=(LinkedDomains domainA, LinkedDomains domainB)
        {
            return !(domainA == domainB);
        }
    }
}
