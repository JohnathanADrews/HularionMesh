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
using HularionMesh.Standard;
using HularionMesh;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HularionMesh.Structure;
using HularionCore.Pattern.Topology;

namespace HularionMesh.Memory
{
    /// <summary>
    /// Constains a store of values in memory.
    /// </summary>
    public class MemoryDomainValueStore : IDomainValueStore
    {

        /// <summary>
        /// The domain this store manages.
        /// </summary>
        public MeshDomain Domain { get; private set; }

        private Dictionary<long, DomainObject> store = new Dictionary<long, DomainObject>();
        private Dictionary<string, long> keyStore = new Dictionary<string, long>();
        private TreeTraverser<WhereExpressionNode> whereTravereser = new TreeTraverser<WhereExpressionNode>();
        private long maxKey = 0;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="domain">The domain this store manages.</param>
        public MemoryDomainValueStore(MeshDomain domain)
        {
            Domain = domain;
        }


        /// <summary>
        /// Returns the values indicated by the where clause.
        /// </summary>
        /// <param name="userKey">The key of the user making the request.</param>
        /// <param name="where">Indicates which values to retrieve from storage.</param>
        /// <param name="readRequest">The values to read from the matching objects.</param>
        /// <returns>The values indicated by the where clause.</returns>
        public DomainObject[] QueryValues(IMeshKey userKey, WhereExpressionNode where, DomainReadRequest readRequest)
        {
            lock (store)
            {
                return ExtractReads(InternalQuery(where).Values, readRequest).ToArray();
            }
        }

        /// <summary>
        /// Queries the number of records matching where.
        /// </summary>
        /// <param name="userKey">The key of the user making the request.</param>
        /// <param name="where">The root node of a where expression tree.</param>
        /// <param name="readRequest">The values to read in the query.</param>
        /// <returns>The number of records matching where.</returns>
        public long QueryCount(IMeshKey userKey, WhereExpressionNode where)
        {
            var queryResult = InternalQuery(where);
            return queryResult.Count;
        }

        /// <summary>
        /// Adds the provided values to storage.
        /// </summary>
        /// <param name="userKey">The key of the user making the request.</param>
        /// <param name="values">The values to add to storage.</param>
        public void InsertValues(IMeshKey userKey, params DomainObject[] values)
        {
            lock (store)
            {
                foreach(var value in values)
                {
                    var key = maxKey++;
                    store.Add(key, value);
                    keyStore.Add(value.Key.Serialized, key);
                }
            }
        }

        /// <summary>
        /// Updates the values indicated by the where to the properties provided.
        /// </summary>
        /// <param name="userKey">The key of the user making the request.</param>
        /// <param name="updaters">Provides the update details.</param>
        public void UpdateValues(IMeshKey userKey, params DomainObjectUpdater[] updaters)
        {
            lock (store)
            {
                foreach (var updater in updaters)
                {
                    var matches = InternalQuery(updater.Where);
                    foreach (var match in matches.Values)
                    {
                        foreach (var property in updater.Values)
                        {
                            if (match.Values.ContainsKey(property.Key))
                            {
                                match.Values.Remove(property.Key);
                            }
                            match.Values.Add(property.Key, property.Value);
                        }
                        foreach (var property in updater.Meta)
                        {
                            if (match.Meta.ContainsKey(property.Key))
                            {
                                match.Meta.Remove(property.Key);
                            }
                            match.Meta.Add(property.Key, property.Value);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Deletes the values indicated by the where clause.
        /// </summary>
        /// <param name="userKey">The key of the user making the request.</param>
        /// <param name="where">Indicates the values to delete.</param>
        public void DeleteValues(IMeshKey userKey, WhereExpressionNode where)
        {
            lock (store)
            {
                var matches = InternalQuery(where);
                foreach (var match in matches)
                {
                    store.Remove(match.Key);
                    keyStore.Remove(match.Value.Key.Serialized);
                }
            }
        }

        /// <summary>
        /// Provides values indicated by the where without locking the store.
        /// </summary>
        /// <param name="where">Indicates which values to get.</param>
        /// <returns>The indicated values.</returns>
        private Dictionary<long, DomainObject> InternalQuery(WhereExpressionNode where)
        {

            ConvertTreeToBinary(where);
            var plan = whereTravereser.CreateEvaluationPlan(TreeTraversalOrder.LeftRightParent, where, whereNode => whereNode.Nodes, true);
            //plan = whereTravereser.CreateEvaluationPlan(TreeTraversalOrder.LeftRightParent, request.Where, node => node.Nodes, false);
            //Add an exteded node to each where node to indicate the truth value of each item.
            var nodes = new Dictionary<WhereExpressionNode, QueryNode>();
            for (int i = 0; i < plan.Length; i++)
            {
                nodes.Add(plan[i], new QueryNode() { WhereNode = plan[i] });
            }
            //For now, just brute force the result.
            //Add optimized query analysis.
            //Add optional query storage and pre-processing.

            QueryNode node = null;
            //1. Compute the truth value at each leaf node for each value.
            for (var j = 0; j < plan.Length; j++)
            {
                node = nodes[plan[j]];
                if (node.WhereNode.Nodes.Length == 0) //leaf node
                {
                    switch (node.WhereNode.Mode)
                    {
                        case WhereExpressionNodeValueMode.Constant:
                            if ((bool)node.WhereNode.Value) { node.TrueKeys = new HashSet<long>(store.Keys); }
                            break;
                        case WhereExpressionNodeValueMode.Key:
                            foreach (var key in node.WhereNode.Values)
                            {
                                var meshKey = MeshKey.Parse(key);
                                if (keyStore.ContainsKey(meshKey.Serialized))
                                {
                                    node.TrueKeys.Add(keyStore[meshKey.Serialized]);
                                }
                            }
                            break;
                        case WhereExpressionNodeValueMode.Value:
                            foreach (var value in store)
                            {
                                if (!value.Value.Values.ContainsKey(node.WhereNode.Property)) { break; }
                                if (WhereComparisonEvaluator.EvaluateComparison(node.WhereNode, value.Value.Values[node.WhereNode.Property]))
                                {
                                    node.TrueKeys.Add(value.Key);
                                }
                            }
                            break;
                        case WhereExpressionNodeValueMode.Meta:
                            foreach (var value in store)
                            {
                                if (!value.Value.Meta.ContainsKey(node.WhereNode.Property)) { break; }
                                if (WhereComparisonEvaluator.EvaluateComparison(node.WhereNode, value.Value.Meta[node.WhereNode.Property]))
                                {
                                    node.TrueKeys.Add(value.Key);
                                }
                            }
                            break;

                    }
                }
                else
                {
                    var left = nodes[node.WhereNode.Nodes[0]];
                    var right = nodes[node.WhereNode.Nodes[1]];
                    foreach (var key in store.Keys)
                    {
                        if (node.WhereNode.Operator.Evaluate(left.TrueKeys.Contains(key), right.TrueKeys.Contains(key)))
                        {
                            node.TrueKeys.Add(key);
                        }
                    }
                }
            }

            //node is root at this point, so all of the true keys are the result keys.
            var result = new Dictionary<long, DomainObject>();
            foreach (var key in node.TrueKeys)
            {
                result.Add(key, store[key]);
            }
            return result;
        }

        /// <summary>
        /// Returns domain read response objects given the provided values an the read request.
        /// </summary>
        /// <param name="values">The objects to read from.</param>
        /// <param name="request">Indicates the properties to read.</param>
        /// <returns></returns>
        private IEnumerable<DomainObject> ExtractReads(IEnumerable<DomainObject> values, DomainReadRequest request)
        {
            if(request.Mode == DomainReadRequestMode.None) { return new List<DomainObject>(); }

            var result = new List<DomainObject>();
            DomainObject response;
            foreach (var value in values)
            {
                response = new DomainObject() { Key = value.Key };
                result.Add(response);

                switch (request.Mode)
                {
                    case DomainReadRequestMode.Include:
                        foreach (var property in request.Values)
                        {
                            if (value.Values.ContainsKey(property))
                            {
                                response.Values.Add(property, value.Values[property]);
                            }
                        }
                        foreach (var property in request.Meta)
                        {
                            if (value.Meta.ContainsKey(property))
                            {
                                response.Meta.Add(property, value.Meta[property]);
                            }
                        }
                        break;
                    case DomainReadRequestMode.Exclude:
                        foreach (var property in value.Values.Keys)
                        {
                            if (!request.Values.Contains(property))
                            {
                                response.Values.Add(property, value.Values[property]);
                            }
                        }
                        foreach (var property in value.Meta.Keys)
                        {
                            if (!request.Meta.Contains(property))
                            {
                                response.Meta.Add(property, value.Meta[property]);
                            }
                        }
                        break;
                    case DomainReadRequestMode.All:
                        response.Values = value.Values.ToDictionary(x => x.Key, x => x.Value);
                        response.Meta = value.Meta.ToDictionary(x => x.Key, x => x.Value);
                        break;
                    case DomainReadRequestMode.JustKeys:
                        break;
                    case DomainReadRequestMode.JustMeta:
                        response.Meta = value.Meta.ToDictionary(x => x.Key, x => x.Value);
                        break;
                    case DomainReadRequestMode.JustValues:
                        response.Values = value.Values.ToDictionary(x => x.Key, x => x.Value);
                        break;
                }
            }
            return result;

        }

        /// <summary>
        /// Converts the tree into a binary tree, which allows us to render operators to AND, OR, and NOT expressions.
        /// </summary>
        /// <param name="root">The root node of the tree.</param>
        private void ConvertTreeToBinary(WhereExpressionNode root)
        {
            var plan = whereTravereser.CreateEvaluationPlan(TreeTraversalOrder.ParentLeftRight, root, node => node.Nodes, false);
            WhereExpressionNode[] newNodes;
            foreach (var node in plan)
            {
                if (node.Nodes.Length > 2)
                {
                    newNodes = new WhereExpressionNode[node.Nodes.Length - 2];
                    for (int i = 0; i < newNodes.Length; i++)
                    {
                        newNodes[i] = node.Clone();
                    }
                    newNodes[0].Nodes = new WhereExpressionNode[] { node.Nodes[0], node.Nodes[1] };
                    for (int i = 1; i < newNodes.Length; i++)
                    {
                        newNodes[i].Nodes = new WhereExpressionNode[] { newNodes[i - 1], node.Nodes[i + 1] };
                    }
                    node.Nodes = new WhereExpressionNode[] { newNodes[newNodes.Length - 1], node.Nodes[node.Nodes.Length - 1] };
                }
            }
        }


        private class QueryNode
        {
            public WhereExpressionNode WhereNode { get; set; }

            public HashSet<long> TrueKeys { get; set; }

            public QueryNode()
            {
                TrueKeys = new HashSet<long>();
            }
        }
    }
}
