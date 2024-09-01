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
using HularionMesh.DomainValue;
using HularionMesh;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HularionMesh.Structure;
using HularionCore.Pattern.Functional;
using HularionCore.Logic;

namespace HularionMesh.Standard
{
    /// <summary>
    /// A standard implementation of IDomainLinkService.
    /// </summary>
    public class StandardDomainLinkService : IDomainLinkService
    {
        public MeshDomain STypeDomain  { get; private set; }
        public MeshDomain TTypeDomain  { get; private set; }
        public DomainLinkForm LinkForm  { get; private set; }
        public MeshDomainLink LinkDomain  { get; private set; }
        public IDomainValueService LinkDomainService  { get; private set; }
        public IParameterizedFacade<DomainLinkAffectRequest, DomainLinkAffectResponse> AffectProcessor  { get; private set; }
        public IParameterizedFacade<DomainLinkQueryRequest, DomainLinkQueryResponse> QueryProcessor  { get; private set; }
        public IProvider<IEnumerable<DomainLinker>> AllLinksProvider  { get; private set; }

        internal IDomainLinkStore Store { get; private set; }
        private IDomainValueService sTypeService;
        private IDomainValueService tTypeService;

        private bool IsSelfReferencing { get { return STypeDomain.Key.EqualsKey(TTypeDomain.Key); } }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="linkStore">The store containing the links.</param>
        /// <param name="linkedDomains">The linked domains.</param>
        /// <param name="linkForm">The formatting for creating the link keys.</param>
        /// <param name="domainServiceProvider">Provides a domain value service givne a domain.</param>
        public StandardDomainLinkService(IDomainLinkStoreWithValueService linkStore, LinkedDomains linkedDomains, DomainLinkForm linkForm, IParameterizedProvider<MeshDomain, IDomainValueService> domainServiceProvider)
        {
            AssignDomains(linkedDomains, linkForm, domainServiceProvider);
            LinkDomainService = linkStore.LinkDomainService;
            Store = linkStore;
            SetupProcessors();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="domainStore"></param>
        /// <param name="linkStore">The store containing the links.</param>
        /// <param name="linkedDomains">The linked domains.</param>
        /// <param name="linkForm">The formatting for creating the link keys.</param>
        /// <param name="domainServiceProvider">Provides a domain value service givne a domain.</param>
        public StandardDomainLinkService(IDomainValueStore domainStore, IDomainLinkStore linkStore, LinkedDomains linkedDomains, DomainLinkForm linkForm, IParameterizedProvider<MeshDomain, IDomainValueService> domainServiceProvider)
        {
            AssignDomains(linkedDomains, linkForm, domainServiceProvider);
            LinkDomainService = new StandardDomainValueService(LinkDomain, Creator.ForSingle<IMeshKey>(() => MeshKey.CreateUniqueTagKey()), domainStore);
            Store = linkStore;
            SetupProcessors();
        }

        private void AssignDomains(LinkedDomains domains, DomainLinkForm linkForm, IParameterizedProvider<MeshDomain, IDomainValueService> domainServiceProvider)
        {
            STypeDomain = linkForm.SelectSTypeDomain(domains.DomainA, domains.DomainB);
            TTypeDomain = linkForm.SelectTTypeDomain(domains.DomainA, domains.DomainB);
            LinkDomain = linkForm.CreateLinkDomain(domains.DomainA, domains.DomainB);
            LinkForm = linkForm;
            sTypeService = domainServiceProvider.Provide(STypeDomain);
            tTypeService = domainServiceProvider.Provide(TTypeDomain);
        }

        private void SetupProcessors()
        {
            QueryProcessor = ParameterizedFacade.FromSingle<DomainLinkQueryRequest, DomainLinkQueryResponse>(
                request =>
                {
                    var response = new DomainLinkQueryResponse() { Key = request.Key };

                    //Figure out which service is which.
                    var subjectSevice = sTypeService;
                    var linkedSevice = tTypeService;
                    var subjectKeyMember = MeshKeyword.SKey.Name;
                    var linkedKeyMember = MeshKeyword.TKey.Name;
                    if (request.SubjectDomain == TTypeDomain)
                    {
                        subjectSevice = tTypeService;
                        linkedSevice = sTypeService;
                        subjectKeyMember = MeshKeyword.TKey.Name;
                        linkedKeyMember = MeshKeyword.SKey.Name;
                    }

                    //Get the Subject keys from the subject service.
                    var subjectKeys = subjectSevice.QueryProcessor.Process(new DomainValueQueryRequest()
                    { Key = request.Key, Where = request.SubjectWhere, Reads = DomainReadRequest.ReadKeys })
                    .Values.Select(x => x.Key).ToList();

                    var linkedKeys = subjectKeys.ToDictionary(x => x, x => new List<IMeshKey>());

                    //Get the Linked keys, specifying the subject keys and Link (link object) where. We do not know the Linked keys yet.
                    if (IsSelfReferencing)
                    {
                        var queryRequest = new DomainValueQueryRequest() { Reads = DomainReadRequest.ReadAll };
                        queryRequest.Where = WhereExpressionNode.CreateBinaryOperatorNode(BinaryOperator.AND);
                        queryRequest.Where.Nodes[0] = WhereExpressionNode.CreateBinaryOperatorNode(BinaryOperator.OR);
                        queryRequest.Where.Nodes[0].Nodes[0] = WhereExpressionNode.CreateMemberIn(subjectKeyMember, subjectKeys);
                        queryRequest.Where.Nodes[0].Nodes[1] = WhereExpressionNode.CreateMemberIn(linkedKeyMember, subjectKeys);
                        queryRequest.Where.Nodes[1] = request.LinkWhere == null ? WhereExpressionNode.WhereTrue : request.LinkWhere;
                        if (request.LinkKeyMatchMode == LinkKeyMatchMode.SKey)
                        {
                            queryRequest.Where.Nodes[0] = WhereExpressionNode.CreateMemberIn(MeshKeyword.SKey.Alias, subjectKeys);
                        }
                        if (request.LinkKeyMatchMode == LinkKeyMatchMode.TKey)
                        {
                            queryRequest.Where.Nodes[0] = WhereExpressionNode.CreateMemberIn(MeshKeyword.TKey.Alias, subjectKeys);
                        }

                        var linkResponseKeys = LinkDomainService.QueryProcessor.Process(queryRequest).Values;
                        var subjectHash = new HashSet<IMeshKey>(subjectKeys);
                        foreach (var link in linkResponseKeys)
                        {
                            var sKey = MeshKey.Parse(link.Values[subjectKeyMember]);
                            var tKey = MeshKey.Parse(link.Values[linkedKeyMember]);
                            if (subjectHash.Contains(sKey)) { linkedKeys[sKey].Add(tKey); }
                            else { linkedKeys[tKey].Add(sKey); }
                        }
                    }
                    else
                    {
                        var queryRequest = new DomainValueQueryRequest() { Reads = DomainReadRequest.ReadAll };
                        queryRequest.Where = WhereExpressionNode.CreateBinaryOperatorNode(BinaryOperator.AND);
                        queryRequest.Where.Nodes[0] = WhereExpressionNode.CreateMemberIn(subjectKeyMember, subjectKeys);
                        queryRequest.Where.Nodes[1] = request.LinkWhere == null ? WhereExpressionNode.WhereTrue : request.LinkWhere;
                        var linkResponseKeys = LinkDomainService.QueryProcessor.Process(queryRequest).Values;
                        foreach (var link in linkResponseKeys)
                        {
                            linkedKeys[MeshKey.Parse(link.Values[subjectKeyMember])].Add(MeshKey.Parse(link.Values[linkedKeyMember]));
                        }
                    }

                    //Now that we have the links, filter out any Linked keys using request.LinkedWhere. 
                    if (request.LinkedWhere != null)
                    {
                        var uniqueLinkedKeys = new HashSet<IMeshKey>();
                        foreach (var link in linkedKeys)
                        {
                            foreach (var linkedKey in link.Value) { uniqueLinkedKeys.Add(linkedKey); }
                        }
                        var linkedRequest = new DomainValueQueryRequest()
                        {
                            Key = request.Key,
                            Where = request.LinkedWhere,
                            Reads = DomainReadRequest.ReadKeys
                        };
                        //Use the AND operator and any Linked keys we already figured out to prevent returning all keys if that is what request.LinkedWhere is getting.
                        linkedRequest.Where = WhereExpressionNode.CreateBinaryOperatorNode(BinaryOperator.AND);
                        linkedRequest.Where.Nodes[0] = request.LinkedWhere;
                        linkedRequest.Where.Nodes[1] = WhereExpressionNode.CreateKeysIn(uniqueLinkedKeys);
                        var queriedKeys = linkedSevice.QueryProcessor.Process(linkedRequest).Values.Select(x => x.Key).ToList();
                        RemoveNonIncludedKeys(linkedKeys, queriedKeys);
                    }
                    //if(request.Mode == LinkQueryRequestMode.LinkedKeys)
                    response.LinkedKeys = linkedKeys.ToDictionary(x => x.Key, x => (IList<IMeshKey>)x.Value.ToList());

                    return response;
                });

            AffectProcessor = ParameterizedFacade.FromSingle<DomainLinkAffectRequest, DomainLinkAffectResponse>(
                request =>
                {


                    string sMember = null;
                    string tMember = null;

                    var sTypeDomain = LinkForm.SelectSTypeDomain(request.DomainA, request.DomainB);
                    if (request.DomainA == sTypeDomain)
                    {
                        sMember = request.AMember;
                        tMember = request.BMember;
                    }
                    else
                    {
                        sMember = request.BMember;
                        tMember = request.AMember;
                    }


                    if (request.Mode == LinkAffectMode.LinkKeys || request.Mode == LinkAffectMode.UnlinkKeys)
                    {
                        IMeshKey sKey = null;
                        IMeshKey tKey = null;
                        if (request.DomainA == sTypeDomain)
                        {
                            sKey = request.ObjectAKey;
                            tKey = request.ObjectBKey;
                        }
                        else
                        {
                            sKey = request.ObjectBKey;
                            tKey = request.ObjectAKey;
                        }
                        switch (request.Mode)
                        {
                            case LinkAffectMode.LinkKeys:
                                if (request.LinkIsExclusive)
                                {
                                    RemoveCurrentLinks(sKey, tKey, sMember, tMember);
                                }
                                Store.Link(new IMeshKey[] { sKey }, new IMeshKey[] { tKey }, sMemberName: sMember, tMemberName: tMember, MeshKey.Parse(request.UserKey));
                                break;
                            case LinkAffectMode.UnlinkKeys:
                                Store.UnLink(new IMeshKey[] { sKey }, new IMeshKey[] { tKey }, sMemberName: sMember, tMemberName: tMember);
                                break;
                        }
                    }

                    
                    if (request.Mode == LinkAffectMode.LinkWhere || request.Mode == LinkAffectMode.UnlinkWhere)
                    {
                        WhereExpressionNode whereS;
                        WhereExpressionNode whereT;
                        if (request.DomainA == sTypeDomain)
                        {
                            whereS = request.WhereA;
                            whereT = request.WhereB;
                        }
                        else
                        {
                            whereS = request.WhereB;
                            whereT = request.WhereA;
                        }
                        var sKeys = sTypeService.QueryProcessor.Process(new DomainValueQueryRequest()
                        {
                            Key = request.Key,
                            Where = whereS,
                            Reads = DomainReadRequest.ReadKeys
                        }).Values.Select(x => x.Key).ToList();
                        var tKeys = tTypeService.QueryProcessor.Process(new DomainValueQueryRequest()
                        {
                            Key = request.Key,
                            Where = whereT,
                            Reads = DomainReadRequest.ReadKeys
                        }).Values.Select(x => x.Key).ToList();

                        switch (request.Mode)
                        {
                            case LinkAffectMode.LinkWhere:
                                Store.Link(sKeys, tKeys, sMember, tMember, MeshKey.Parse(request.UserKey));
                                break;
                            case LinkAffectMode.UnlinkWhere:
                                Store.UnLink(sKeys, tKeys);
                                break;
                        }
                    }


                    var response = new DomainLinkAffectResponse() { Key = request.Key };
                    return response;
                });

            AllLinksProvider = new ProviderFunction<IEnumerable<DomainLinker>>(() => Store.GetLinkers(WhereExpressionNode.ReadAll));
        }

        private void RemoveNonIncludedKeys(IDictionary<IMeshKey, List<IMeshKey>> linked, IEnumerable<IMeshKey> include)
        {
            foreach (var link in linked)
            {
                var links = link.Value.ToList();
                link.Value.Clear();
                link.Value.AddRange(links.Where(x => include.Contains(x)).ToList());
            }
        }


        private void RemoveCurrentLinks(IMeshKey sKey, IMeshKey tKey, string sMemberName = null, string tMemberName = null)
        {
            IDictionary<IMeshKey, IEnumerable<DomainLinker>> linkers = null;
            if (sMemberName != null)
            {
                linkers = Store.GetLinks(new IMeshKey[] { sKey }, sMemberName);
                foreach(var linker in linkers)
                {
                    Store.UnLink(new IMeshKey[] { linker.Key }, linker.Value.Select(x=>x.TKey).ToArray(), sMemberName:sMemberName, tMemberName: tMemberName);
                }
            }
            if (tMemberName != null)
            {
                linkers = Store.GetLinks(new IMeshKey[] { tKey }, tMemberName);
                foreach (var linker in linkers)
                {
                    Store.UnLink(new IMeshKey[] { linker.Key }, linker.Value.Select(x => x.TKey).ToArray(), sMemberName: sMemberName, tMemberName: tMemberName);
                }
            }
            
        }

    }
}
