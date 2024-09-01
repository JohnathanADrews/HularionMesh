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
using HularionMesh.DomainLink;
using HularionMesh.DomainValue;
using HularionMesh.Standard;
using System;
using System.Collections.Generic;
using System.Text;

namespace HularionMesh.Structure
{
    /// <summary>
    /// Provides interaction for linked objects.
    /// </summary>
    public interface IDomainLinkService
    {

        /// <summary>
        /// The public name of the S-type.
        /// </summary>
        MeshDomain STypeDomain { get; }

        /// <summary>
        /// The public name of the T-type.
        /// </summary>
        MeshDomain TTypeDomain { get; }

        /// <summary>
        /// The formatting specification for the domain link.
        /// </summary>
        DomainLinkForm LinkForm { get; }

        /// <summary>
        /// The domain for the linking objects.
        /// </summary>
        MeshDomainLink LinkDomain { get; }

        /// <summary>
        /// Processes affect request.
        /// </summary>
        IParameterizedFacade<DomainLinkAffectRequest, DomainLinkAffectResponse> AffectProcessor { get; }

        /// <summary>
        /// Processes query request.
        /// </summary>
        IParameterizedFacade<DomainLinkQueryRequest, DomainLinkQueryResponse> QueryProcessor { get; }

        /// <summary>
        /// Provides all of the domain links.
        /// </summary>
        IProvider<IEnumerable<DomainLinker>> AllLinksProvider { get; }
        
    }
}
