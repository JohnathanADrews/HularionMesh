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
using HularionMesh.MeshType;
using System;
using System.Collections.Generic;
using System.Text;

namespace HularionMesh.DomainLink
{
    /// <summary>
    /// Links two domains.
    /// </summary>
    public class MeshDomainLink : MeshDomain
    {
        /// <summary>
        /// A linked domain.
        /// </summary>
        public MeshDomain DomainA { get; set; }

        /// <summary>
        /// A linked domain.
        /// </summary>
        public MeshDomain DomainB { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public MeshDomainLink()
        {
            Properties.Add(new ValueProperty() { Name = MeshKeyword.SKey.Alias, Type = DataType.MeshKey.Key.Serialized });
            Properties.Add(new ValueProperty() { Name = MeshKeyword.TKey.Alias, Type = DataType.MeshKey.Key.Serialized });
            Properties.Add(new ValueProperty() { Name = MeshKeyword.SMember.Alias, Type = DataType.Text8.Key.Serialized });
            Properties.Add(new ValueProperty() { Name = MeshKeyword.TMember.Alias, Type = DataType.Text8.Key.Serialized });
        }

        /// <summary>
        /// Gets the linked domains.
        /// </summary>
        /// <returns></returns>
        public LinkedDomains GetLinkedDomains()
        {
            return new LinkedDomains() { DomainA = DomainA, DomainB = DomainB };
        }

        /// <summary>
        /// Gets the S-type domain.
        /// </summary>
        /// <param name="form">The formatting to use.</param>
        /// <returns>The S-type domain.</returns>
        public MeshDomain GetSTypeDomain(DomainLinkForm form)
        {
            return form.SelectSTypeDomain(GetLinkedDomains());
        }

        /// <summary>
        /// Gets the T-type domain.
        /// </summary>
        /// <param name="form">The formatting to use.</param>
        /// <returns>The T-type domain.</returns>
        public MeshDomain GetTTypeDomain(DomainLinkForm form)
        {
            return form.SelectTTypeDomain(GetLinkedDomains());
        }

    }
}
