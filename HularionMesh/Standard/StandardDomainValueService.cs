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
using HularionMesh.DomainValue;
using HularionMesh;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HularionMesh.Structure;
using HularionCore.Pattern.Functional;

namespace HularionMesh.Standard
{
    /// <summary>
    /// A standard implementation for IDomainValueService.
    /// </summary>
    public class StandardDomainValueService : IDomainValueService
    {
        public MeshDomain Domain { get; private set; }
        public IParameterizedFacade<DomainValueQueryRequest, DomainValueQueryResponse> QueryProcessor { get; private set; }
        public IParameterizedFacade<DomainValueAffectRequest, DomainValueAffectResponse> AffectProcessor { get; private set; }

        internal IDomainValueStore Store;

        private readonly Type deleteType = typeof(DomainValueAffectDelete);
        private readonly Type updateType = typeof(DomainValueAffectUpdate);
        private readonly Type createType = typeof(DomainValueAffectCreate);
        private readonly Type insertType = typeof(DomainValueAffectInsert);

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="domain">The mesh domain.</param>
        /// <param name="domainValueKeyCreator">Creates domain value keys.</param>
        /// <param name="store">The domain value sotre.</param>
        public StandardDomainValueService(MeshDomain domain, ICreator<IMeshKey> domainValueKeyCreator, IDomainValueStore store)
        {
            this.Domain = domain;
            Store = store;
            SetupProcessors(domainValueKeyCreator);
        }

        private void SetupProcessors(ICreator<IMeshKey> domainValueKeyCreator)
        {

            QueryProcessor = ParameterizedFacade.FromSingle<DomainValueQueryRequest, DomainValueQueryResponse>(
                request =>
                {
                    if(request.Reads.Mode == DomainReadRequestMode.Count)
                    {
                        return new DomainValueQueryResponse() { Key = request.Key, Count = Store.QueryCount(request.UserKey, request.Where) };
                    }
                    var response = new DomainValueQueryResponse() { Key = request.Key, Values = Store.QueryValues(request.UserKey, request.Where, request.Reads) };
                    response.Count = response.Values.Count();
                    return response;
                });
            AffectProcessor = ParameterizedFacade.FromSingle<DomainValueAffectRequest, DomainValueAffectResponse>(
                request =>
                {
                    var response = new DomainValueAffectResponse() { Key = request.Key };
                    for (int i = 0; i < request.Affected.Length; i++)
                    {
                        var affector = request.Affected[i];
                        var affectorType = affector.GetType();
                        if (affectorType == createType)
                        {
                            var creator = (DomainValueAffectCreate)affector;
                            if (creator.Value.Key == null || creator.Value.Key == MeshKey.NullKey)
                            {
                                creator.Value.Key = domainValueKeyCreator.Create();
                            }
                            if (creator.Value.Meta.ContainsKey(MeshKeyword.ValueCreationTime.Name)) { creator.Value.Meta.Remove(MeshKeyword.ValueCreationTime.Name); }
                            creator.Value.Meta.Add(MeshKeyword.ValueCreationTime.Name, request.RequestTime);
                            if (creator.Value.Meta.ContainsKey(MeshKeyword.ValueCreator.Name)) { creator.Value.Meta.Remove(MeshKeyword.ValueCreator.Name); }
                            creator.Value.Meta.Add(MeshKeyword.ValueCreator.Name, request.UserKey);
                            if (creator.Value.Meta.ContainsKey(MeshKeyword.ValueUpdateTime.Name)) { creator.Value.Meta.Remove(MeshKeyword.ValueUpdateTime.Name); }
                            creator.Value.Meta.Add(MeshKeyword.ValueUpdateTime.Name, request.RequestTime);
                            if (creator.Value.Meta.ContainsKey(MeshKeyword.ValueUpdater.Name)) { creator.Value.Meta.Remove(MeshKeyword.ValueUpdater.Name); }
                            creator.Value.Meta.Add(MeshKeyword.ValueUpdater.Name, request.UserKey);

                            Store.InsertValues(request.UserKey, creator.Value);
                            response.CreateReads.Add(creator.Value);
                        }
                        else if (affectorType == insertType)
                        {
                            var inserter = (DomainValueAffectInsert)affector;
                            if (Store.QueryValues(request.UserKey, WhereExpressionNode.CreateKeysIn(inserter.Value.Key), DomainReadRequest.ReadKeys).Count() == 0)
                            {
                                Store.InsertValues(request.UserKey, inserter.Value);
                            }
                        }
                        else if (affectorType == updateType)
                        {
                            var updater = (DomainValueAffectUpdate)affector;
                            if (updater.Updater.Meta.ContainsKey(MeshKeyword.ValueUpdater.Alias) && updater.Updater.Meta[MeshKeyword.ValueUpdater.Alias] == null) { updater.Updater.Meta.Remove(MeshKeyword.ValueUpdater.Alias); }
                            if (!updater.Updater.Meta.ContainsKey(MeshKeyword.ValueUpdater.Alias)) { updater.Updater.Meta.Add(MeshKeyword.ValueUpdater.Alias, request.UserKey); }
                            if (updater.Updater.Meta.ContainsKey(MeshKeyword.ValueUpdateTime.Alias) && updater.Updater.Meta[MeshKeyword.ValueUpdateTime.Alias] == null) { updater.Updater.Meta.Remove(MeshKeyword.ValueUpdateTime.Alias); }
                            if (!updater.Updater.Meta.ContainsKey(MeshKeyword.ValueUpdateTime.Alias)) { updater.Updater.Meta.Add(MeshKeyword.ValueUpdateTime.Alias, request.RequestTime); }
                            if (updater.Updater.Meta.ContainsKey(MeshKeyword.ValueUpdateTime.Name))
                            {
                                updater.Updater.Meta.Remove(MeshKeyword.ValueUpdateTime.Name);
                            }
                            updater.Updater.Meta.Add(MeshKeyword.ValueUpdateTime.Name, request.RequestTime);
                            if (updater.Updater.Meta.ContainsKey(MeshKeyword.ValueUpdater.Name))
                            {
                                updater.Updater.Meta.Remove(MeshKeyword.ValueUpdater.Name);
                            }
                            updater.Updater.Meta.Add(MeshKeyword.ValueUpdater.Name, request.UserKey);
                            Store.UpdateValues(request.UserKey, updater.Updater);
                        }
                        else if (affectorType == deleteType)
                        {
                            var deleter = (DomainValueAffectDelete)affector;
                            Store.DeleteValues(request.UserKey, deleter.Where);
                        }
                    }
                    return response;
                });
        }
    }
}
