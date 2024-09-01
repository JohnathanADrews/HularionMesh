#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionCore.Injector;
using HularionCore.Pattern.Functional;
using HularionMesh.DomainAggregate;
using HularionMesh.Structure;
using System;
using System.Collections.Generic;
using System.Text;

namespace HularionMesh.Injector
{
    /// <summary>
    /// An injectable implementation for IDomainAggregateService. Call SetMembers to inject members from another instance.
    /// </summary>
    public class InjectorDomainAggregateService : Injectable<IDomainAggregateService>, IDomainAggregateService
    {
        public IParameterizedFacade<DomainAggregateQueryRequest, DomainAggregateQueryResponse> QueryProcessor {get; set;}
        public IParameterizedFacade<DomainAggregateAffectorRequest, DomainAggregateAffectorResponse> AffectProcessor {get; set;}
        public ITransform<AggregateQueryResponseNode, object> FullRenderer {get; set;}
        public ITransform<AggregateQueryResponseNode, object> MergeRenderer {get; set; }
        public ITransform<AggregateQueryResponseNode, object> LinkRenderer { get; set; }

        /// <summary>
        /// Constructor. Assign members directly or call SetMembers to assign all members from a source provider. 
        /// </summary>
        public InjectorDomainAggregateService()
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="values">The providers to use. They will overwrite only null values in order.</param>
        public InjectorDomainAggregateService(params IDomainAggregateService[] values)
        {
            SetMembers(InjectorOverwriteMode.ValueOverNull, values);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="mode">The overwrite mode.</param>
        /// <param name="values">The providers to use. They will overwrite each other in order according to "mode".</param>
        public InjectorDomainAggregateService(InjectorOverwriteMode mode, params IDomainAggregateService[] values)
        {
            SetMembers(mode, values);
        }

        //public InjectorDomainAggregateService(IDomainAggregateService service)
        //{
        //    QueryProcessor = service.QueryProcessor;
        //    AffectProcessor = service.AffectProcessor;
        //    FullRenderer = service.FullRenderer;
        //    MergeRenderer = service.MergeRenderer;
        //    LinkRenderer = service.LinkRenderer;
        //}
    }
}
