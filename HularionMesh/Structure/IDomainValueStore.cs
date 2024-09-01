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
using System;
using System.Collections.Generic;
using System.Text;

namespace HularionMesh.Structure
{
    /// <summary>
    /// Represents a store of domain objects specific to the specified domain.
    /// </summary>
    public interface IDomainValueStore
    {
        /// <summary>
        /// The domain of the objects managed by this store.
        /// </summary>
        MeshDomain Domain { get; }

        /// <summary>
        /// Queries the values of this store.
        /// </summary>
        /// <param name="userKey">The key of the user making the request.</param>
        /// <param name="where">The root node of a where expression tree.</param>
        /// <param name="readRequest">The values to read in the query.</param>
        /// <returns>The objects matching the provided query parameters.</returns>
        DomainObject[] QueryValues(IMeshKey userKey, WhereExpressionNode where, DomainReadRequest readRequest);

        /// <summary>
        /// Queries the number of records matching where.
        /// </summary>
        /// <param name="userKey">The key of the user making the request.</param>
        /// <param name="where">The root node of a where expression tree.</param>
        /// <returns>The number of records matching where.</returns>
        long QueryCount(IMeshKey userKey, WhereExpressionNode where);

        /// <summary>
        /// Inserts the provided values to the store and sets the key on each object.
        /// </summary>
        /// <param name="userKey">The key of the user making the request.</param>
        /// <param name="values">The values to add.</param>
        void InsertValues(IMeshKey userKey, params DomainObject[] values);

        /// <summary>
        /// Updates the values specified by the updater and then reads the requested values back.
        /// </summary>
        /// <param name="userKey">The key of the user making the request.</param>
        /// <param name="updaters">Specifies which objects and which object members to update.</param>
        void UpdateValues(IMeshKey userKey, params DomainObjectUpdater[] updaters);

        /// <summary>
        /// Deletes the specified objects.
        /// </summary>
        /// <param name="userKey">The key of the user making the request.</param>
        /// <param name="where">The root where node which details how to query the objects to delete.</param>
        void DeleteValues(IMeshKey userKey, WhereExpressionNode where);



    }
}
