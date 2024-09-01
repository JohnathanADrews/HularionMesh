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
using HularionMesh.MeshType;
using HularionMesh;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HularionMesh.Structure;
using HularionCore.Logic;

namespace HularionMesh.Standard
{
    /// <summary>
    /// A standard implementation of IDomainLinkStore using an IDomainValueStore to store the link objects.
    /// </summary>
    public class StandardDomainLinkStore : IDomainLinkStoreWithValueService
    {

        /// <summary>
        /// The S-type domain.
        /// </summary>
        public MeshDomain STypeDomain { get; private set; }
        /// <summary>
        /// The T-type domain.
        /// </summary>
        public MeshDomain TTypeDomain { get; private set; }
        /// <summary>
        /// The linking domain.
        /// </summary>
        public MeshDomainLink LinkDomain { get; private set; }
        /// <summary>
        /// The formatting specification for the link keys.
        /// </summary>
        public DomainLinkForm LinkForm { get; private set; }

        /// <summary>
        /// The domain values service used to store the link objects.
        /// </summary>
        public IDomainValueService LinkDomainService { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="domains">The domains being linked.</param>
        /// <param name="linkForm">The formatting details for the links.</param>
        /// <param name="linkDomainService">The service that will manage the link objects.</param>
        public StandardDomainLinkStore(LinkedDomains domains, DomainLinkForm linkForm, IDomainValueService linkDomainService)
        {
            LinkForm = linkForm;
            STypeDomain = linkForm.SelectSTypeDomain(domains.DomainA, domains.DomainB);
            TTypeDomain = linkForm.SelectTTypeDomain(domains.DomainA, domains.DomainB);
            LinkDomain = linkForm.CreateLinkDomain(domains.DomainA, domains.DomainB);
            LinkDomainService = linkDomainService;
        }

        /// <summary>
        /// Links the objects with the provided keys if they are not already linked.
        /// </summary>
        /// <param name="aKeys">The keys of the S-type objects.</param>
        /// <param name="bKeys">The keys of the T-type objects.</param>
        /// <param name="sMemberName">The name of the member of the S-type objects that is ocupied by T-type objects.</param>
        /// <param name="tMemberName">The name of the member of the T-type objects that is ocupied by S-type objects.</param>
        /// <param name="userKey">The key of the user making the link request.</param>
        public IEnumerable<DomainLinker> Link(IEnumerable<IMeshKey> aKeys, IEnumerable<IMeshKey> bKeys, string sMemberName = null, string tMemberName = null, IMeshKey userKey = null)
        {
            var result = new List<DomainLinker>();
            IEnumerable<IMeshKey> sKeys;
            IEnumerable<IMeshKey> tKeys;
            if (STypeDomain == TTypeDomain)
            {
                sKeys = aKeys;
                tKeys = bKeys;
            }
            else
            {
                sKeys = GetKeySet(STypeDomain, aKeys, bKeys);
                tKeys = GetKeySet(TTypeDomain, aKeys, bKeys);
            }
            if (sKeys == null || tKeys == null) { return result; }
            var linkAdds = new Dictionary<IMeshKey, DomainLink.DomainLinker>();
            foreach (var sKey in sKeys)
            {
                foreach (var tKey in tKeys)
                {
                    var domainKey = LinkForm.CreateKey(sKey, tKey, sMember:sMemberName, tMember: tMemberName);
                    var linker = new DomainLinker() { DomainKey = domainKey, SKey = sKey, TKey = tKey, SMember = sMemberName, TMember = tMemberName, Creator = userKey, Creation = DateTime.UtcNow };
                    linkAdds.Add(domainKey, linker);
                }
            }
            var query = new DomainValueQueryRequest() { Where = WhereExpressionNode.CreateKeysIn(linkAdds.Select(x=>x.Key)), Reads = DomainReadRequest.ReadKeys };
            var queryResult = LinkDomainService.QueryProcessor.Process(query);
            var existing = queryResult.Values.Select(x => x.Key);
            var adds = linkAdds.Where(x => !existing.Contains(x.Key)).Select(x=>x.Value.ToDomainObject()).ToList();
            var addRequest = new DomainValueAffectRequest()
            {
                UserKey = userKey,
                Affected = adds.Select(x=> new DomainValueAffectInsert() { Value = x }).ToArray()
            };
            LinkDomainService.AffectProcessor.Process(addRequest);
            result = adds.Select(x => DomainLinker.FromDomainObject(x)).ToList();
            return result;
        }

        /// <summary>
        /// Unlinks the objects with the provided keys.
        /// </summary>
        /// <param name="aKeys">The keys of the S-type objects.</param>
        /// <param name="bKeys">The keys of the T-type objects.</param>
        public void UnLink(IEnumerable<IMeshKey> aKeys, IEnumerable<IMeshKey> bKeys, string sMemberName = null, string tMemberName = null, IMeshKey userKey = null)
        {
            IEnumerable<IMeshKey> sKeys;
            IEnumerable<IMeshKey> tKeys;
            if (STypeDomain == TTypeDomain)
            {
                sKeys = aKeys;
                tKeys = bKeys;
            }
            else
            {
                sKeys = GetKeySet(STypeDomain, aKeys, bKeys);
                tKeys = GetKeySet(TTypeDomain, aKeys, bKeys);
            }
            if (sKeys == null || tKeys == null) { return; }
            var deleteWhere = WhereExpressionNode.CreateBinaryOperatorNode(BinaryOperator.AND);
            deleteWhere.Nodes[0] = WhereExpressionNode.CreateMemberIn(MeshKeyword.SKey.Alias, sKeys.Select(x => (object)x).ToList());
            deleteWhere.Nodes[1] = WhereExpressionNode.CreateMemberIn(MeshKeyword.TKey.Alias, tKeys.Select(x => (object)x).ToList());
            if (sMemberName != null)
            {
                var memberWhere = WhereExpressionNode.CreateComparisonNode(DataTypeComparison.Equal);
                memberWhere.Property = MeshKeyword.SMember.Alias;
                memberWhere.Value = sMemberName;
                deleteWhere.Nodes[0] = deleteWhere.Nodes[0].CombineWithOperator(memberWhere, BinaryOperator.AND);
            }
            if (tMemberName != null)
            {
                var memberWhere = WhereExpressionNode.CreateComparisonNode(DataTypeComparison.Equal);
                memberWhere.Property = MeshKeyword.TMember.Alias;
                memberWhere.Value = tMemberName;
                deleteWhere.Nodes[1] = deleteWhere.Nodes[1].CombineWithOperator(memberWhere, BinaryOperator.AND);
            }

            var deleteRequest = new DomainValueAffectRequest()
            {
                UserKey = userKey,
                Affected = new IDomainValueAffectItem[] { new DomainValueAffectDelete() { Where = deleteWhere, Reads = DomainReadRequest.ReadNone } }
            };
            LinkDomainService.AffectProcessor.Process(deleteRequest);
        }

        /// <summary>
        /// Gets the link objects of the linked domain that are related to each of the provided keys.
        /// </summary>
        /// <param name="keys">The keys of the subject domain for which the related linked keys will be provided.</param>
        /// <returns>The keys of the linked domain that are related to each of the provided keys.</returns>
        /// <remarks>If A1 is linked to B1, B2, and C1, then given A1.Key, the result is (A1.Key,{ A1-B1, A1-B2, A1-C1 }).</remarks>
        public IDictionary<IMeshKey, IEnumerable<DomainLinker>> GetLinkedLinkers(IEnumerable<IMeshKey> keys)
        {
            var result = new Dictionary<IMeshKey, IEnumerable<DomainLinker>>();
            if (keys == null || keys.Count() == 0) { return result; }

            keys = keys.Distinct().ToList();

            if (STypeDomain == TTypeDomain)
            {
                var where = new WhereExpressionNode() {  Operator = BinaryOperator.OR };
                where.Nodes = new WhereExpressionNode[]
                {
                    new WhereExpressionNode() { Type = DataType.MeshKey, Comparison = DataTypeComparison.In, Property = MeshKeyword.TKey.Name, Values = keys.Select(x => x).ToArray() },
                    new WhereExpressionNode() { Type = DataType.MeshKey, Comparison = DataTypeComparison.In, Property = MeshKeyword.SKey.Name, Values = keys.Select(x => x).ToArray() }
                };
                var query = new DomainValueQueryRequest() { Where = where, Reads = DomainReadRequest.ReadAll };
                var queryResult = LinkDomainService.QueryProcessor.Process(query);
                var hValues = new HashSet<IMeshKey>(keys);
                foreach (var value in queryResult.Values)
                {
                    var skey = MeshKey.Parse(value.Values[MeshKeyword.SKey.Name]);
                    var tkey = MeshKey.Parse(value.Values[MeshKeyword.TKey.Name]);
                    if (hValues.Contains(skey))
                    {
                        if (!result.ContainsKey(skey)) { result.Add(skey, new List<DomainLinker>()); }
                        ((List<DomainLinker>)result[skey]).Add(DomainLinker.FromDomainObject(value));
                    }
                    else if (hValues.Contains(tkey))
                    {
                        if (!result.ContainsKey(tkey)) { result.Add(tkey, new List<DomainLinker>()); }
                        ((List<DomainLinker>)result[tkey]).Add(DomainLinker.FromDomainObject(value));
                    }
                }
            }
            else
            {
                var domain = STypeDomain;
                var where = WhereExpressionNode.CreateMemberIn(MeshKeyword.SKey.Alias, keys.Select(x => (object)x).ToList());
                if (TTypeDomain.KeyIsObjectInThisDomain(keys.First()))
                {
                    domain = TTypeDomain;
                    where.Property = MeshKeyword.TKey.Alias;
                }
                var query = new DomainValueQueryRequest() { Where = where, Reads = DomainReadRequest.ReadAll };
                var queryResult = LinkDomainService.QueryProcessor.Process(query);
                if (domain == STypeDomain)
                {
                    foreach (var value in queryResult.Values)
                    {
                        var key = MeshKey.Parse(value.Values[MeshKeyword.SKey.Name]);
                        if (!result.ContainsKey(key)) { result.Add(key, new List<DomainLinker>()); }
                        ((List<DomainLinker>)result[key]).Add(DomainLinker.FromDomainObject(value));

                    }
                }
                if (domain == TTypeDomain)
                {
                    foreach (var value in queryResult.Values)
                    {
                        var key = MeshKey.Parse(value.Values[MeshKeyword.TKey.Name]);
                        if (!result.ContainsKey(key)) { result.Add(key, new List<DomainLinker>()); }
                        ((List<DomainLinker>)result[key]).Add(DomainLinker.FromDomainObject(value));
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the keys of the linked domain that are related to each of the provided keys.
        /// </summary>
        /// <param name="keys">The keys of the subject domain for which the related linked keys will be provided.</param>
        /// <returns>The keys of the linked domain that are related to each of the provided keys.</returns>
        public IDictionary<IMeshKey, IEnumerable<IMeshKey>> GetLinkedKeys(IEnumerable<IMeshKey> keys)
        {
            if (keys == null || keys.Count() == 0) { return new Dictionary<IMeshKey, IEnumerable<IMeshKey>>(); }
            var linkers = GetLinkedLinkers(keys);
            if(STypeDomain == TTypeDomain)
            {
                var result = new Dictionary<IMeshKey, IEnumerable<IMeshKey>>();
                foreach (var linker in linkers)
                {
                    var linkKeys = new List<IMeshKey>();
                    linkKeys.AddRange(linker.Value.Where(x => x.SKey.EqualsKey(x.TKey)).Select(x=>x.SKey));
                    linkKeys.AddRange(linker.Value.Where(x => !x.SKey.EqualsKey(x.TKey) && linker.Key.EqualsKey(x.SKey)).Select(x => x.TKey));
                    linkKeys.AddRange(linker.Value.Where(x => !x.SKey.EqualsKey(x.TKey) && linker.Key.EqualsKey(x.TKey)).Select(x => x.SKey));
                    result.Add(linker.Key, linkKeys);
                }
                return result;
            }
            //If the keys are S-type, map them to the T-type keys.
            if (STypeDomain.KeyIsObjectInThisDomain(keys.First()))
            {
                return linkers.ToDictionary(x => x.Key, x => (IEnumerable<IMeshKey>)x.Value.Select(y => y.TKey).ToList());
            }
            //If the keys are not S-type (and therefor T-type), map them to the S-type keys.
            return linkers.ToDictionary(x => x.Key, x => (IEnumerable<IMeshKey>)x.Value.Select(y => y.SKey).ToList());
        }

        /// <summary>
        /// Gets the linkers for the given linked keys and member name.
        /// </summary>
        /// <param name="linkedKeys">The S-type keys or T-type keys.</param>
        /// <param name="memberName">The name of the member for the given key.</param>
        /// <returns>The linkers for the links with the matching keys and member names.</returns>
        public IDictionary<IMeshKey, IEnumerable<DomainLinker>> GetLinks(IEnumerable<IMeshKey> linkedKeys, string memberName)
        {
            var result = GetLinkedLinkers(linkedKeys);
            var keys = result.Keys.ToList();
            foreach(var key in keys)
            {
                var list = result[key];
                var matches = new List<DomainLinker>();
                foreach(var linker in list)
                {
                    if (key.EqualsKey(linker.SKey) && linker.SMember == memberName)
                    {
                        matches.Add(linker);
                    }
                    if (key.EqualsKey(linker.TKey) && linker.TMember == memberName)
                    {
                        matches.Add(linker);
                    }
                }
                result[key] = matches;
            }
            return result;
        }

        /// <summary>
        /// Gets the linkers with the given keys.
        /// </summary>
        /// <param name="linkerKeys">The keys associated with the linker objects.</param>
        /// <returns>The linkers with the given keys.</returns>
        public IEnumerable<DomainLinker> GetLinks(params IMeshKey[] linkerKeys)
        {
            var query = new DomainValueQueryRequest() { Where = WhereExpressionNode.CreateKeysIn(linkerKeys), Reads = DomainReadRequest.ReadAll };
            var queryResult = LinkDomainService.QueryProcessor.Process(query);
            return queryResult.Values.Select(x => DomainLinker.FromDomainObject(x)).ToList();
        }

        /// <summary>
        /// Gets all linkers using "where".
        /// </summary>
        /// <param name="where">The search criteria.</param>
        /// <returns>The matching linkers.</returns>
        public IEnumerable<DomainLinker> GetLinkers(WhereExpressionNode where)
        {
            var query = new DomainValueQueryRequest() { Where = where, Reads = DomainReadRequest.ReadAll };
            var queryResult = LinkDomainService.QueryProcessor.Process(query);
            return queryResult.Values.Select(x => new DomainLinker(x)).ToList();
        }

        /// <summary>
        /// Finds all linkers using "where" and then returns the domain keys for the given domain.
        /// </summary>
        /// <param name="domain">The domain of the keys to return.</param>
        /// <param name="where">The search criteria.</param>
        /// <returns>The "domain" object keys present in the found linkers.</returns>
        public IEnumerable<IMeshKey> GetDomainObjectKeys(MeshDomain domain, WhereExpressionNode where)
        {
            var linkers = GetLinkers(where);
            if(linkers == null || linkers.Count() == 0) { return new List<IMeshKey>(); }
            var sKeys = linkers.Select(x => x.SKey).ToList();
            var tKeys = linkers.Select(x => x.TKey).ToList();
            var result = GetKeySet(domain, sKeys, tKeys);
            if (result == null) { return new List<IMeshKey>(); }
            return result;
        }

        private IEnumerable<IMeshKey> GetKeySet(MeshDomain domain, params IEnumerable<IMeshKey>[] keySets)
        {
            foreach (var set in keySets)
            {
                if (set == null || set.Count() == 0) { continue; }
                if (domain.KeyIsObjectInThisDomain(set.First())) { return set; }
            }
            return null;
        }
    }
}
