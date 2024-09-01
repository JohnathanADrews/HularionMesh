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
using HularionMesh.DomainAggregate;
using System;
using System.Collections.Generic;
using System.Text;

namespace HularionMesh.DomainLink
{
    /// <summary>
    /// Represents a query request on a domain link.
    /// </summary>
    public class DomainLinkQueryRequest : IKeyedValue<object>
    {
        /// <summary>
        /// The request key.
        /// </summary>
        public object Key { get; set; }

        /// <summary>
        /// The key of the version.
        /// </summary>
        public object VersionKey { get; set; }

        /// <summary>
        /// The primary domain in the context of this query.
        /// </summary>
        public MeshDomain SubjectDomain { get; set; }

        /// <summary>
        /// The where clause of the subject domain.
        /// </summary>
        public WhereExpressionNode SubjectWhere { get; set; }

        /// <summary>
        /// The where clause of the domain linked to the subject.  MAY be null.
        /// </summary>
        public WhereExpressionNode LinkedWhere { get; set; }

        /// <summary>
        /// The where clause of the values linking the S and T types.  MAY be null.
        /// </summary>
        public WhereExpressionNode LinkWhere { get; set; }

        /// <summary>
        /// The mode in which to execute the query.
        /// </summary>
        public LinkQueryRequestMode Mode { get; set; }

        /// <summary>
        /// Determines which key to match to the Subject keys. 
        /// </summary>
        public LinkKeyMatchMode LinkKeyMatchMode { get; set; } = LinkKeyMatchMode.Both;

    }

}
