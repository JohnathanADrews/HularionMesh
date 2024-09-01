#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionCore.Logic;
using HularionMesh.Domain;
using HularionMesh.DomainAggregate;
using HularionMesh.DomainLink;
using HularionMesh.DomainValue;
using HularionMesh.Standard;
using HularionMesh;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HularionMesh.Structure;
using HularionMesh.Response;
using HularionCore.Pattern.Functional;
using HularionCore.Pattern.Topology;

namespace HularionMesh.Standard
{
    /// <summary>
    /// A standard composite service.
    /// </summary>
    public class StandardAggregateService : IDomainAggregateService
    {

        /// <summary>
        /// Executes composite query requests and provides responses.
        /// </summary>
        public IParameterizedFacade<DomainAggregateQueryRequest, DomainAggregateQueryResponse> QueryProcessor { get; private set; }
        /// <summary>
        /// Processes affect requests.
        /// </summary>
        public IParameterizedFacade<DomainAggregateAffectorRequest, DomainAggregateAffectorResponse> AffectProcessor { get; private set; }


        private IParameterizedProvider<MeshDomain, ServiceResponse<IDomainValueService>> domainValueServiceProvider;
        private IParameterizedProvider<LinkedDomains, ServiceResponse<IDomainLinkService>> domainLinkServiceProvider;

        /// <summary>
        /// Constructor.
        /// </summary>
        public StandardAggregateService(IParameterizedProvider<MeshDomain, ServiceResponse<IDomainValueService>> domainValueServiceProvider,
            IParameterizedProvider<LinkedDomains, ServiceResponse<IDomainLinkService>> domainLinkServiceProvider
            )
        {
            this.domainValueServiceProvider = domainValueServiceProvider;
            this.domainLinkServiceProvider = domainLinkServiceProvider;
            //this.domainByKeyProvider = domainByKeyProvider;

            var queryTraverser = new TreeTraverser<AggregateQueryItem>();
            QueryProcessor = ParameterizedFacade.FromSingle<DomainAggregateQueryRequest, DomainAggregateQueryResponse>(
                request =>
                {
                    var response = ProcessQueryRequest(request);
                    return response;
                });

            AffectProcessor = ParameterizedFacade.FromSingle<DomainAggregateAffectorRequest, DomainAggregateAffectorResponse>(
                request =>
                {
                    //if (request.AnyOrder) { }
                    var response = new DomainAggregateAffectorResponse();
                    foreach (var item in request.Items)
                    {
                        if (item.Creator != null)
                        {
                            var service = domainValueServiceProvider.Provide(item.Domain).Response;
                            var aResponse = service.AffectProcessor.Process(new DomainValueAffectRequest() { Affected = new IDomainValueAffectItem[] { item.Creator }, UserKey = request.UserKey, RequestTime = request.RequestTime });
                            item.Creator.ResponseProcessor.Process(aResponse);
                        }                        
                        if(item.Inserter != null)
                        {
                            var service = domainValueServiceProvider.Provide(item.Domain).Response;
                            var insertRequest = new DomainValueAffectRequest() { Affected = new IDomainValueAffectItem[] { item.Inserter }, UserKey = request.UserKey, RequestTime = request.RequestTime };
                            var aResponse = service.AffectProcessor.Process(insertRequest);
                            item.Inserter.ResponseProcessor.Process(aResponse);
                        }
                        if (item.Updater != null)
                        {
                            var service = domainValueServiceProvider.Provide(item.Domain).Response;
                            var updateRequest = new DomainValueAffectRequest() { Affected = new IDomainValueAffectItem[] { item.Updater }, UserKey = request.UserKey, RequestTime = request.RequestTime };
                            var aResponse = service.AffectProcessor.Process(updateRequest);
                            item.Updater.ResponseProcessor.Process(aResponse);
                        }
                        if (item.Deleter != null)
                        {
                            var service = domainValueServiceProvider.Provide(item.Domain).Response;
                            var aResponse = service.AffectProcessor.Process(new DomainValueAffectRequest() { Affected = new IDomainValueAffectItem[] { item.Deleter }, UserKey = request.UserKey, RequestTime = request.RequestTime });
                            item.Deleter.ResponseProcessor.Process(aResponse);
                        }
                        if (item.Link != null)
                        {
                            if (item.Link.UserKey == null) { item.Link.UserKey = request.UserKey; }
                            var service = domainLinkServiceProvider.Provide(new LinkedDomains() { DomainA = item.Link.DomainA, DomainB = item.Link.DomainB }).Response;
                            var linkResponse = service.AffectProcessor.Process(new DomainLinkAffectRequest[] { item.Link });
                        }
                    }
                    return response;
                });

        }


        private DomainAggregateQueryResponse ProcessQueryRequest(DomainAggregateQueryRequest request)
        {
            var response = new DomainAggregateQueryResponse() { Key = request.Key };

            //Get the impositions.
            var qiMap = CreateQueryImpositionMap(request.Read);
            //var qiMap = new Dictionary<AggregateQueryItem, QueryImposition>();

            //if(ShouldDoReverseChain(QueryDomainObject)){ DoReverseChain(QueryDomainObject); }
            //else { "continue the process." }

            var keyMap = new Dictionary<MeshDomain, HashSet<IMeshKey>>();
            var readerMap = new Dictionary<AggregateQueryItem, HashSet<IMeshKey>>();
            var qdoRoot = new QueryDomainObject() { Reader = request.Read, Imposition = qiMap[request.Read] };

            response.RootKeys = QueryKeyMap(qdoRoot, keyMap, readerMap, qiMap);

            var objectMap = new Dictionary<IMeshKey, DomainObject>();
            //var qObjectMap = new Dictionary<IMeshKey, QueriedDomainObject>();
            foreach (var domainKeys in keyMap)
            {
                var valueRequest = new DomainValueQueryRequest() { Reads = DomainReadRequest.ReadAll, Where = WhereExpressionNode.CreateKeysIn(domainKeys.Value.ToArray()) };
                var valueService = this.domainValueServiceProvider.Provide(domainKeys.Key).Response;
                var valueResponse = valueService.QueryProcessor.Process(valueRequest);
                foreach (var value in valueResponse.Values)
                {
                    objectMap.Add(value.Key, value);
                    //qObjectMap.Add(value.Key, new QueriedDomainObject() { DomainObject = value });
                }
            }
            var qdoTraverser = new TreeTraverser<QueryDomainObject>();
            qdoTraverser.CreateEvaluationPlan(TreeTraversalOrder.ParentLeftRight, qdoRoot, node =>
            {
                if (node.Reader.Mode == AggregateDomainMode.Domain)
                {
                    if (node.Parent != null)
                    {
                        node.ResponseNode.Alias = node.Parent.Reader.Alias;
                    }
                    var links = new HashSet<AggregateQueryResponseNode>();
                    node.ResponseNode.Domain = node.Reader.Domain;
                    foreach (var key in node.Keys) { node.ResponseNode.Items.Add(objectMap[key]); }
                    foreach (var link in node.Links)
                    {
                        var memberName = link.Key.Alias;
                        foreach (var keyset in link.Value.LinkQueryResponse.LinkedKeys)
                        {
                            if (keyset.Value.Count() > 0)
                            {
                                response.Links.Add(new QueriedLink() { Alias = link.Key.Alias, FromKeys = new List<IMeshKey>() { keyset.Key }, ToKeys = keyset.Value });
                            }
                            var linkNode = link.Value.Links.First().Value.ResponseNode;
                            links.Add(linkNode);
                        }
                    }
                    node.ResponseNode.Links = links.ToList();
                }
                return node.Links.Values.ToArray();
            }, true);

            response.Result = qdoRoot.ResponseNode;
            response.Objects = objectMap.Values.ToList();
            return response;
        }

        private List<IMeshKey> QueryKeyMap(QueryDomainObject qdoRoot, 
            Dictionary<MeshDomain, HashSet<IMeshKey>> keyMap, 
            Dictionary<AggregateQueryItem, HashSet<IMeshKey>> readerMap,
            Dictionary<AggregateQueryItem, QueryImposition> qiMap, 
            bool ignoreImpositions = false)
        {
            var qdoTraverser = new TreeTraverser<QueryDomainObject>();
            var recurrenceRoots = new HashSet<AggregateQueryItem>();
            var queriedKeys = new HashSet<IMeshKey>();
            List<IMeshKey> roots = null;
            var qdoPlan = qdoTraverser.CreateEvaluationPlan(TreeTraversalOrder.ParentLeftRight, qdoRoot, node =>
            {
                var next = new List<QueryDomainObject>();
                if (node.Reader.Mode == AggregateDomainMode.Domain)
                {
                    node.ValueService = this.domainValueServiceProvider.Provide(node.Reader.Domain).Response;
                    var keyRequest = new DomainValueQueryRequest() { Reads = DomainReadRequest.ReadKeys };
                    keyRequest.Where = node.Reader.DomainWhere.CombineWithOperator(node.LinkedWhere, BinaryOperator.AND);
                    if (!recurrenceRoots.Contains(node.Reader) && node.Reader.RecurrenceRootWhere != null)
                    {
                        keyRequest.Where = keyRequest.Where.CombineWithOperator(node.Reader.RecurrenceRootWhere, BinaryOperator.AND);
                        recurrenceRoots.Add(node.Reader);
                    }

                    if (!ignoreImpositions && node.Imposition.IsStrictImposedOn)
                    {
                        //SetImpositionCounts(node);
                        //if (node.ImpositionCounts.ShouldReverseChain)
                        {
                            var imposedWhere = ReverseChainQuery(node.Reader, qiMap, keyRequest.Where);
                            keyRequest.Where = keyRequest.Where.CombineWithOperator(imposedWhere, BinaryOperator.AND);
                        }
                    }

                    var keyResponse = node.ValueService.QueryProcessor.Process(keyRequest);
                    node.Keys = keyResponse.Values.Select(x => (IMeshKey)MeshKey.Parse(x.Key)).ToList();
                    if (!keyMap.ContainsKey(node.Reader.Domain)) { keyMap.Add(node.Reader.Domain, new HashSet<IMeshKey>()); }
                    if (!readerMap.ContainsKey(node.Reader)) { readerMap.Add(node.Reader, new HashSet<IMeshKey>()); }
                    var contained = new List<IMeshKey>();
                    foreach (var key in node.Keys)
                    {
                        keyMap[node.Reader.Domain].Add(key);
                        readerMap[node.Reader].Add(key);
                        queriedKeys.Add(key);
                    }
                    foreach (var link in node.Reader.Links)
                    {
                        var linkNode = new QueryDomainObject() { Reader = link, Parent = node, Imposition = ignoreImpositions ? null : qiMap[link] };
                        node.Links.Add(link, linkNode);
                    }
                    next.AddRange(node.Links.Values);
                    if (node == qdoRoot) { roots = node.Keys; }
                }

                if (node.Reader.Mode == AggregateDomainMode.Link)
                {
                    node.LinkService = this.domainLinkServiceProvider.Provide(new LinkedDomains() { DomainA = node.Parent.Reader.Domain, DomainB = node.Reader.Links[0].Domain }).Response;
                    var linkRequest = new DomainLinkQueryRequest() { Mode = LinkQueryRequestMode.LinkedKeys, LinkKeyMatchMode = node.Reader.LinkKeyMatchMode };
                    linkRequest.SubjectDomain = node.Parent.Reader.Domain;
                    linkRequest.SubjectWhere = WhereExpressionNode.CreateKeysIn(node.Parent.Keys);
                    linkRequest.LinkWhere = node.Reader.LinkWhere;
                    linkRequest.LinkedWhere = node.Reader.Links[0].DomainWhere;

                    if (!ignoreImpositions)
                    {
                        var linkedImposition = qiMap[node.Reader.Links[0]];
                        if (linkedImposition.IsStrictImposedOn)
                        {
                            var imposedWhere = ReverseChainQuery(node.Reader.Links[0], qiMap, linkRequest.LinkedWhere);
                            linkRequest.LinkedWhere = linkRequest.LinkedWhere.CombineWithOperator(imposedWhere, BinaryOperator.AND);
                        }
                    }
                    node.LinkQueryResponse = node.LinkService.QueryProcessor.Process(new DomainLinkQueryRequest[] { linkRequest }).First();
                    linkRequest.Mode = LinkQueryRequestMode.LinkKeys;
                    var linkNode = new QueryDomainObject() { Reader = node.Reader.Links[0], Parent = node, Imposition = ignoreImpositions ? null : qiMap[node.Reader.Links[0]] };
                    var keys = new List<IMeshKey>();
                    foreach (var keySet in node.LinkQueryResponse.LinkedKeys) { keys.AddRange(keySet.Value); }
                    var nextKeys = keys.Where(x => !queriedKeys.Contains(x)).ToList();
                    linkNode.LinkedWhere = WhereExpressionNode.CreateKeysIn(nextKeys);
                    node.Links.Add(linkNode.Reader, linkNode);
                    if (nextKeys.Count() > 0) { next.Add(linkNode); }
                }

                return next.ToArray();
            }, true);

            return roots;
        }


        private QueryImpositionCounts GetImpositionCounts(QueryImposition imposition, WhereExpressionNode nodeWhere)
        {
            var counts = new QueryImpositionCounts(imposition);
            var valueService = this.domainValueServiceProvider.Provide(imposition.Reader.Domain).Response;
            counts.TargetCount = valueService.QueryProcessor.Process(new DomainValueQueryRequest[] { new DomainValueQueryRequest()
            {
                Reads = new DomainReadRequest(){ Mode = DomainReadRequestMode.Count },
                Where = nodeWhere
            }}).First().Count;

            foreach (var imposed in imposition.StrictImposedOn)
            {
                valueService = this.domainValueServiceProvider.Provide(imposed.Reader.Domain).Response;
                var imposedWhere = GetDomainWhere(imposed.Reader);
                var impositionCount = valueService.QueryProcessor.Process(new DomainValueQueryRequest[] { new DomainValueQueryRequest()
                {
                    Reads = new DomainReadRequest(){ Mode = DomainReadRequestMode.Count },
                    Where = imposedWhere
                }}).First().Count;
                counts.ImpositionCounts[imposed] = impositionCount;
            }
            return counts;
        }

        private WhereExpressionNode ReverseChainQuery(AggregateQueryItem reader,
            Dictionary<AggregateQueryItem, QueryImposition> qiMap, 
            WhereExpressionNode nodeWhere)
        {
            var queryImposition = qiMap[reader];
            var counts = GetImpositionCounts(queryImposition, nodeWhere);

            var impositions = counts.ImpositionCounts.OrderBy(x => x.Value).Select(x => x.Key).ToList();            
            var reverseRuns = new HashSet<QueryImposition>();
            var imposedKeys = new HashSet<IMeshKey>();

            var runBranchChain = new Func<QueryImposition, bool, WhereExpressionNode, HashSet<IMeshKey>>((imposition, doForwardChain, rootWhere) =>
            {
                var imposedRunKeys = new HashSet<IMeshKey>();
                var path = imposition.StrictImpositionPath;
                //var node = queryDomainObject;
                var newReaders = path.Select(x =>
                {
                    var clone = x.Clone();
                    clone.Links = new List<AggregateQueryItem>();
                    return clone;
                }).ToArray();

                if (doForwardChain) { newReaders = newReaders.Reverse().ToArray(); }

                for (var i = 0; i < newReaders.Length - 1; i++)
                {
                    newReaders[i].Links.Add(newReaders[i + 1]);
                }

                var keyMap = new Dictionary<MeshDomain, HashSet<IMeshKey>>();
                var qdoRoot = new QueryDomainObject() { Reader = newReaders.First() };
                var readerMap = new Dictionary<AggregateQueryItem, HashSet<IMeshKey>>();
                var rootKeys = QueryKeyMap(qdoRoot, keyMap, readerMap, qiMap, ignoreImpositions: true);
                var keys = new HashSet<IMeshKey>();
                if (readerMap.ContainsKey(newReaders.Last())) { keys = readerMap[newReaders.Last()]; }
                return keys;
            });

            //1. Run the reverse chain on those with a small key set to reduce the imposed on key set.
            foreach (var imposition in impositions)
            {
                if(imposition != impositions.First() && imposedKeys.Count < counts.ImpositionCounts[imposition])
                {
                    break;
                }
                reverseRuns.Add(imposition);

                var runKeys = runBranchChain(imposition, false, null);
                if (imposition == impositions.First())
                {
                    imposedKeys = new HashSet<IMeshKey>(runKeys);
                }
                else
                {
                    var remove = imposedKeys.Where(x => !runKeys.Contains(x)).ToList();
                    foreach (var key in remove)
                    {
                        imposedKeys.Remove(key);
                    }
                }
            }


            //2. Run the forward chain on the those that didn't run in (1.).
            //3. Run the reverse chain on those in (2.) to reduce the imposed on set to the minimum amount.
            var valueService = this.domainValueServiceProvider.Provide(reader.Domain).Response;
            var domainWhere = GetDomainWhere(reader).CombineWithOperator(WhereExpressionNode.CreateKeysIn(imposedKeys), BinaryOperator.AND);
            var response = valueService.QueryProcessor.Process(new DomainValueQueryRequest() { Where = domainWhere, Reads = DomainReadRequest.ReadKeys });
            imposedKeys = new HashSet<IMeshKey>(response.Values.Select(x => x.Key));

            var remaining = impositions.Where(x => !reverseRuns.Contains(x)).ToList();
            foreach(var imposition in remaining)
            {
                var imposerKeys = runBranchChain(imposition, true, null);
                var imposerWhere = GetDomainWhere(imposition.Reader).CombineWithOperator(WhereExpressionNode.CreateKeysIn(imposerKeys), BinaryOperator.AND);
                var keys = runBranchChain(imposition, false, imposerWhere);
                var newKeys = new HashSet<IMeshKey>();
                foreach(var key in keys) 
                {
                    if (imposedKeys.Contains(key)) { newKeys.Add(key); }
                }
                imposedKeys = newKeys;
            }

            return WhereExpressionNode.CreateKeysIn(imposedKeys);

        }


        private Dictionary<AggregateQueryItem, QueryImposition> CreateQueryImpositionMap(AggregateQueryItem reader)
        {
            var qiTraverser = new TreeTraverser<QueryImposition>();
            var qiRoot = new QueryImposition() { Reader = reader };
            var qiMap = new Dictionary<AggregateQueryItem, QueryImposition>();
            var visited = new HashSet<AggregateQueryItem>() { reader };
            var qiPlan = qiTraverser.CreateEvaluationPlan(TreeTraversalOrder.LeftRightParent, qiRoot, node =>
            {
                node.Next = node.Reader.Links.Where(x=>!visited.Contains(x)).Select(x => 
                {
                    visited.Add(x); 
                    return new QueryImposition() { Parent = node, Reader = x };
                }).ToList();
                return node.Next.ToArray();
            }, true);
            foreach (var qi in qiPlan) { qiMap[qi.Reader] = qi; }
            var impositionHandled = new HashSet<AggregateQueryItem>();
            //qiPlan = qiTraverser.CreateEvaluationPlan(TreeTraversalOrder.LeftRightParent, qiRoot, node =>
            //{
                
            //    return node.Next.ToArray();
            //}, true);

            for(var i = 0; i < qiPlan.Length; i++)
            {
                var node = qiPlan[i];
                if (impositionHandled.Contains(node.Reader)) { continue; }
                var impositions = new HashSet<AggregateQueryItem>(node.Reader.Impositions);
                while (impositions.Count > 0)
                {
                    var target = node.Parent;
                    var path = new List<AggregateQueryItem>() { node.Reader };
                    while (target != null)
                    {
                        path.Add(target.Reader);
                        if (impositions.Contains(target.Reader)) { impositions.Remove(target.Reader); }
                        foreach (var targetImposition in target.Reader.Impositions) { impositions.Add(targetImposition); }
                        if (impositions.Count() == 0)
                        {
                            break;
                        }
                        else
                        {
                            //target.PathImposedOn.Add(node);
                            impositionHandled.Add(target.Reader);
                        }
                        target = target.Parent;
                    }
                    node.StrictImpositionPath = path.ToArray();
                    node.StrictImposition = target.Reader;
                    target.StrictImposedOn.Add(node);
                }
            }

             return qiMap;
        }


        private WhereExpressionNode GetDomainWhere(AggregateQueryItem reader)
        {
            var where = reader.DomainWhere;
            if (reader.RecurrenceRootWhere != null)
            {
                where.CombineWithOperator(reader.RecurrenceRootWhere, BinaryOperator.AND);
            }
            return where;
        }

        /// <summary>
        /// Manages information through the query process.
        /// </summary>
        private class QueryDomainObject
        {

            /// <summary>
            /// The reader provided in the request.
            /// </summary>
            public AggregateQueryItem Reader { get; set; }

            /// <summary>
            /// The parent of this.
            /// </summary>
            public QueryDomainObject Parent { get; set; }

            /// <summary>
            /// The keys of the objects of this domain.
            /// </summary>
            public List<IMeshKey> Keys { get; set; } = new List<IMeshKey>();

            /// <summary>
            /// The links related to this node.
            /// </summary>
            public Dictionary<AggregateQueryItem, QueryDomainObject> Links { get; set; } = new Dictionary<AggregateQueryItem, QueryDomainObject>();

            /// <summary>
            /// A node in the response tree.
            /// </summary>
            public AggregateQueryResponseNode ResponseNode { get; set; } = new AggregateQueryResponseNode();

            /// <summary>
            /// The service that links this domain object node to its parent.
            /// </summary>
            public IDomainLinkService LinkService { get; set; }

            /// <summary>
            /// The service used to get the values of this domain.
            /// </summary>
            public IDomainValueService ValueService { get; set; }

            /// <summary>
            /// If this node is for a domain, LinkedWhere filters for the keys that were selected in the link.
            /// </summary>
            public WhereExpressionNode LinkedWhere { get; set; } = WhereExpressionNode.ReadAll;

            public DomainLinkQueryResponse LinkQueryResponse { get; set; }

            public QueryImposition Imposition { get; set; }


            /// <summary>
            /// Constructor.
            /// </summary>
            public QueryDomainObject()
            {
            }

            public override string ToString()
            {
                return String.Format("QueryDomainObject: {0}", GetHashCode());
            }

        }

        private class QueryImposition
        {

            public QueryImposition Parent { get; set; }

            public AggregateQueryItem Reader { get; set; }

            public List<QueryImposition> Next { get; set; } = new List<QueryImposition>();


            /// <summary>
            /// The imposition nearest to root for a strict imposition. In Strict, overlapping impositions are redundant in the overlap.
            /// </summary>
            public AggregateQueryItem StrictImposition { get; set; }

            /// <summary>
            /// The imposition path for a strict imposition.
            /// </summary>
            public AggregateQueryItem[] StrictImpositionPath { get; set; }

            public List<QueryImposition> StrictImposedOn { get; set; } = new List<QueryImposition>();

            public bool IsStrictImposedOn { get { return StrictImposedOn.Count > 0; } }

            public bool IsStrictImposing { get { return StrictImposition != null; } }

        }

        private class QueryImpositionCounts
        {
            public QueryImposition Imposition { get; set; }

            public long TargetCount { get; set; }

            public Dictionary<QueryImposition, long> ImpositionCounts { get; set; } = new Dictionary<QueryImposition, long>();

            public bool ShouldReverseChain { get { return (ImpositionCounts.Values.Where(x=>x < TargetCount).Count() > 0); } }


            public QueryImpositionCounts(QueryImposition imposition)
            {
                Imposition = imposition;
            }

        }
    }
}
